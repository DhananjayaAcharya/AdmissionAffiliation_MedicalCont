using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class ContinuousAffiliationIncreaseintakeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ContinuousAffiliationIncreaseintakeController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // ════════════════════════════════════════════════════════════
        //  GET
        // ════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult IncreaseIntake()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = HttpContext.Session.GetString("CollegeName");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Index", "Home");

            if (!int.TryParse(facultyCode, out int facultyId))
                return BadRequest("Invalid faculty code");

            var model = new AcademicIntakePageViewModel1
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CollegeName = collegeName,
                FacultyId = facultyId
            };

            BuildModelData(model, facultyCode, collegeCode, facultyId);
            return View(model);
        }

        // ════════════════════════════════════════════════════════════
        //  POST
        // ════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> IncreaseIntake(AcademicIntakePageViewModel1 model)
        {
            // Always re-read from session; never trust hidden form fields as authority
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Collegelogin", "Login");

            if (!int.TryParse(facultyCode, out int facultyId))
                return BadRequest("Invalid faculty code");

            try
            {
                // ── 1. Collect all submitted rows ──────────────────────────────
                var incoming = new List<AcademicIntake>();
                CollectCourses(model.UgCourses, facultyCode, collegeCode, incoming);
                CollectCourses(model.PgCourses, facultyCode, collegeCode, incoming);
                CollectCourses(model.SsCourses, facultyCode, collegeCode, incoming);

                Console.WriteLine($"[POST] Collected {incoming.Count} course rows from form.");

                if (incoming.Count == 0)
                {
                    TempData["SuccessMessage"] = "No courses/data to save.";
                    BuildModelData(model, facultyCode, collegeCode, facultyId);
                    return View(model);
                }

                // ── 2. Debug: log what arrived from the browser ────────────────
                foreach (var inc in incoming)
                {
                    Console.WriteLine(
                        $"[INCOMING] {inc.Courses,-30} | " +
                        $"2024Exist={inc.Ay2024ExistingIntake} Inc={inc.Ay2024IncreaseIntake} | " +
                        $"2025Exist={inc.Ay2025ExistingIntake} LOP={inc.Ay2025LopNmcIntake} | " +
                        $"2026Req={inc.Ay2026AddRequestedIntake}");
                }

                // ── 3. Load current DB records into a dictionary ───────────────
                var existingDict = await _context.AcademicIntakes
                    .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                    .ToDictionaryAsync(x => x.Courses, StringComparer.OrdinalIgnoreCase);

                Console.WriteLine($"[POST] Found {existingDict.Count} existing DB records.");

                int added = 0, updated = 0;

                foreach (var inc in incoming)
                {
                    // Server-side recalculation — never trust client-computed values
                    //int t2024 = inc.Ay2024ExistingIntake + inc.Ay2024IncreaseIntake;
                    //int t2025 = inc.Ay2025ExistingIntake + inc.Ay2025LopNmcIntake;
                    //int e2026 = t2025;                                        
                    //int t2026 = e2026 + inc.Ay2026AddRequestedIntake;

                    int t2024 = facultyId == 2
                                                ? 0
                                                : inc.Ay2024ExistingIntake + inc.Ay2024IncreaseIntake;

                    int t2025 =
                        inc.Ay2025ExistingIntake +
                        inc.Ay2025LopNmcIntake;

                    int e2026 = t2025;

                    int t2026 =
                        e2026 +
                        inc.Ay2026AddRequestedIntake;

                    int e2027 = t2026;

                    int t2027 =
                        e2027 +
                        inc.Ay2027AddRequestedIntake;

                    if (existingDict.TryGetValue(inc.Courses, out var db))
                    {
                        // ── UPDATE ─────────────────────────────────────────────
                        db.FacultyCode = facultyCode;
                        db.CollegeCode = collegeCode;

                        db.Ay2024ExistingIntake = inc.Ay2024ExistingIntake;
                        db.Ay2024IncreaseIntake = inc.Ay2024IncreaseIntake;
                        db.Ay2024TotalIntake = t2024;

                        db.Ay2025ExistingIntake = inc.Ay2025ExistingIntake;
                        db.Ay2025LopNmcIntake = inc.Ay2025LopNmcIntake;
                        db.Ay2025TotalIntake = t2025;
                        db.Ay2025LopDate = inc.Ay2025LopDate;

                        // Only replace document bytes when a new file was uploaded
                        if (facultyId == 2)
                        {
                            var ugVm = model.UgCourses?
                                .FirstOrDefault(x => x.CourseCode == inc.Courses);

                            var pgVm = model.PgCourses?
                                .FirstOrDefault(x => x.CourseCode == inc.Courses);

                            var vm = ugVm ?? pgVm;

                            if (vm != null)
                            {

                                if (vm.AY2025_LopDocument != null)
                                
                                    db.Ay2025LopDocument =
                                        ToBytes(vm.AY2025_LopDocument);
                                

                                if (vm.AY2025_DCIDocument != null)
                                    db.Ay2025Dcidocument =
                                        ToBytes(vm.AY2025_DCIDocument);

                                if (vm.AY2025_KSDCDocument != null)
                                    db.Ay2025Ksdcdocument =
                                        ToBytes(vm.AY2025_KSDCDocument);

                                if (vm.AY2026_DCIDocument != null)
                                    db.Ay2026Dcidocument =
                                        ToBytes(vm.AY2026_DCIDocument);

                                if (vm.AY2026_KSDCDocument != null)
                                    db.Ay2026Ksdcdocument =
                                        ToBytes(vm.AY2026_KSDCDocument);

                                if (vm.AY2027_DCIDocument != null)
                                    db.Ay2027Dcidocument =
                                        ToBytes(vm.AY2027_DCIDocument);

                                if (vm.AY2027_KSDCDocument != null)
                                    db.Ay2027Ksdcdocument =
                                        ToBytes(vm.AY2027_KSDCDocument);
                            }
                        }
                        else
                        {
                            var ugVm = model.UgCourses?
                                .FirstOrDefault(x => x.CourseCode == inc.Courses);

                            var pgVm = model.PgCourses?
                                .FirstOrDefault(x => x.CourseCode == inc.Courses);

                            var ssVm = model.SsCourses?
                                .FirstOrDefault(x => x.CourseCode == inc.Courses);

                            var vm = ugVm ?? pgVm ?? ssVm;

                            if (vm?.AY2025_NmcDocument != null)
                            {
                                db.Ay2025NmcDocument =
                                    ToBytes(vm.AY2025_NmcDocument);
                            }
                        }


                        db.Ay2026ExistingIntake = e2026;
                        db.Ay2026AddRequestedIntake = inc.Ay2026AddRequestedIntake;
                        db.Ay2026TotalIntake = t2026;
                        db.Ay2027ExistingIntake = e2027;
                        db.Ay2027AddRequestedIntake = inc.Ay2027AddRequestedIntake;
                        db.Ay2027TotalIntake = t2027;


                        Console.WriteLine($"[UPDATE] {inc.Courses} → 2026Exist={e2026}, 2026Total={t2026}");
                        updated++;
                    }
                    else
                    {
                        // ── INSERT ─────────────────────────────────────────────
                        inc.Ay2024TotalIntake = t2024;
                        inc.Ay2025TotalIntake = t2025;
                        inc.Ay2026ExistingIntake = e2026;
                        inc.Ay2026TotalIntake = t2026;
                        inc.Ay2027ExistingIntake = e2027;

                        inc.Ay2027AddRequestedIntake =
                            inc.Ay2027AddRequestedIntake;

                        inc.Ay2027TotalIntake = t2027;

                        _context.AcademicIntakes.Add(inc);
                        Console.WriteLine($"[INSERT] {inc.Courses}");
                        added++;
                    }
                }

                // ── 4. Persist ─────────────────────────────────────────────────
                int changes = await _context.SaveChangesAsync();
                Console.WriteLine($"[POST] SaveChangesAsync returned {changes} row(s) affected.");

                TempData["SuccessMessage"] = changes > 0
                    ? $"Saved successfully — {added} new, {updated} updated. (DB rows affected: {changes})"
                    : "No changes detected — the submitted values may already match the database.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POST ERROR] {ex}");
                ModelState.AddModelError("", $"Save error: {ex.Message}");
                if (ex.InnerException != null)
                    ModelState.AddModelError("", $"Detail: {ex.InnerException.Message}");
            }

            // Always reload fresh data from DB before re-rendering the view
            BuildModelData(model, facultyCode, collegeCode, facultyId);
            return View(model);
        }

        // ════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Converts one IntakeByLevelViewModel1 row into an AcademicIntake entity.
        /// Handles IFormFile → byte[] conversion for the NMC document.
        /// </summary>
        /// 
        private static byte[]? ToBytes(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var ms = new MemoryStream();

            file.CopyTo(ms);

            return ms.ToArray();
        }
        private static AcademicIntake MapToEntity(
                          string facultyCode,
                          string collegeCode,
                          IntakeByLevelViewModel1 vm)
        {
            return new AcademicIntake
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                Courses = vm.CourseCode!,

                // ── 2024-25 ─────────────────────────────
                Ay2024ExistingIntake =
                    vm.AY2024_ExistingIntake ?? 0,

                Ay2024IncreaseIntake =
                    vm.AY2024_IncreaseIntake ?? 0,

                Ay2024TotalIntake =
                    vm.AY2024_TotalIntake ?? 0,

                // ── 2025-26 ─────────────────────────────
                Ay2025ExistingIntake =
                    vm.AY2025_ExistingIntake ?? 0,

                Ay2025LopNmcIntake =
                    vm.AY2025_LopNmcIntake ?? 0,

                Ay2025TotalIntake =
                    vm.AY2025_TotalIntake ?? 0,

                Ay2025LopDate =
                    vm.AY2025_LopDate,

                Ay2025LopDocument =
                         ToBytes(vm.AY2025_LopDocument),

                Ay2025NmcDocument =
                    ToBytes(vm.AY2025_NmcDocument),

                Ay2025Dcidocument =
                    ToBytes(vm.AY2025_DCIDocument),

                Ay2025Ksdcdocument =
                    ToBytes(vm.AY2025_KSDCDocument),

                // ── 2026-27 ─────────────────────────────
                Ay2026ExistingIntake =
                    vm.AY2026_ExistingIntake ?? 0,

                Ay2026AddRequestedIntake =
                    vm.AY2026_AddRequestedIntake ?? 0,

                Ay2026TotalIntake =
                    vm.AY2026_TotalIntake ?? 0,

                Ay2026Dcidocument =
                    ToBytes(vm.AY2026_DCIDocument),

                Ay2026Ksdcdocument =
                    ToBytes(vm.AY2026_KSDCDocument),

                // ── 2027-28 ─────────────────────────────
                Ay2027ExistingIntake =
                    vm.AY2027_ExistingIntake ?? 0,

                Ay2027AddRequestedIntake =
                    vm.AY2027_AddRequestedIntake ?? 0,

                Ay2027TotalIntake =
                    vm.AY2027_TotalIntake ?? 0,

                Ay2027Dcidocument =
                    ToBytes(vm.AY2027_DCIDocument),

                Ay2027Ksdcdocument =
                    ToBytes(vm.AY2027_KSDCDocument)
            };
        }
        /// <summary>
        /// Iterates a list of view-model rows, maps each to an entity, and appends to target.
        /// Skips rows with no CourseCode.
        /// </summary>
        private static void CollectCourses(
            IEnumerable<IntakeByLevelViewModel1>? rows,
            string facultyCode,
            string collegeCode,
            List<AcademicIntake> target)
        {
            if (rows == null) return;
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.CourseCode)) continue;
                target.Add(MapToEntity(facultyCode, collegeCode, r));
            }
        }

        /// <summary>
        /// Populates all ViewModel collections from the database.
        /// Shared between GET and POST (post-save redisplay) so data is always fresh.
        /// </summary>
        private async Task BuildModelData(
            AcademicIntakePageViewModel1 model,
            string facultyCode,
            string collegeCode,
            int facultyId)
        {
            // Sync model meta so view always shows correct header values
            model.FacultyCode = facultyCode;
            model.CollegeCode = collegeCode;
            model.FacultyId = facultyId;
            model.CollegeName ??= HttpContext.Session.GetString("CollegeName");
            var seatSlab = await _context.AcademicIntakes.Where(e => e.CollegeCode == collegeCode).Select(e => e.Ay2025TotalIntake).FirstOrDefaultAsync();
            
            if(seatSlab.ToString() != null) HttpContext.Session.SetString("SeatSlab", seatSlab.ToString());

            var hospitalDetailsId = await _context.HospitalDetailsForAffiliations.Where(e => e.CollegeCode == collegeCode).Select(e => e.HospitalDetailsId).FirstOrDefaultAsync();

            if (hospitalDetailsId.ToString() != null) HttpContext.Session.SetString("HospitalDetailsId", hospitalDetailsId.ToString());

            var intakeDetails = _context.CollegeCourseIntakeDetails
                .Where(d => d.FacultyCode == facultyId && d.CollegeCode == collegeCode)
                .ToList();

            var allCourses = _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)
                .ToList();

            var existingIntakes = _context.AcademicIntakes
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .ToList();

            // ── Reusable LINQ projection ───────────────────────────────────
            //IEnumerable<IntakeByLevelViewModel1> Project(string level) =>
            //    from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
            //    let codeInt = int.Parse(d.CourseCode!)
            //    join c in allCourses on codeInt equals c.CourseCode
            //    where c.CourseLevel == level
            //    join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
            //    from existing in ej.DefaultIfEmpty()
            //    select new IntakeByLevelViewModel1
            //    {
            //        CourseCode = c.CourseCode.ToString(),
            //        CourseName = c.CourseName,

            //        // 2024-25
            //        AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
            //        AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
            //        AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),

            //        // 2025-26  (use DB value for existing, master-table value for new)
            //        AY2025_ExistingIntake = existing?.Ay2025ExistingIntake ?? d.ExistingIntake ?? 0,
            //        AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
            //        AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
            //        AY2025_LopDate = existing?.Ay2025LopDate,

            //        // 2026-27
            //        AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
            //        AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
            //        AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
            //    };

            //code added by DP on 07052026

            IEnumerable<IntakeByLevelViewModel1> Project(string level)
            {
                // Courses for this level
                var levelCourses = allCourses
                    .Where(c => c.CourseLevel == level)
                    .ToList();

                // If intake details exist → use them
                if (intakeDetails.Any())
                {
                    return
                        from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                        let codeInt = int.Parse(d.CourseCode!)
                        join c in levelCourses on codeInt equals c.CourseCode
                        join e in existingIntakes
                            on c.CourseCode.ToString() equals e.Courses into ej
                        from existing in ej.DefaultIfEmpty()

                        select new IntakeByLevelViewModel1
                        {
                            CourseCode = c.CourseCode.ToString(),
                            CourseName = c.CourseName,

                            // 2024-25
                            AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                            AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                            AY2024_TotalIntake =
                                existing?.Ay2024TotalIntake
                                ?? d.ExistingIntake.GetValueOrDefault(),

                            // 2025-26
                            AY2025_ExistingIntake =
                                existing?.Ay2025ExistingIntake
                                ?? d.ExistingIntake
                                ?? 0,

                            AY2025_LopNmcIntake =
                                existing?.Ay2025LopNmcIntake ?? 0,

                            AY2025_TotalIntake =
                                existing?.Ay2025TotalIntake ?? 0,

                            AY2025_LopDate = existing?.Ay2025LopDate,

                            // 2026-27
                            AY2026_ExistingIntake =
                                existing?.Ay2026ExistingIntake ?? 0,

                            AY2026_AddRequestedIntake =
                                existing?.Ay2026AddRequestedIntake ?? 0,

                            AY2026_TotalIntake =
                             existing?.Ay2026TotalIntake ?? 0,

                            AY2027_ExistingIntake =
                                existing?.Ay2027ExistingIntake ?? 0,

                            AY2027_AddRequestedIntake =
                                  existing?.Ay2027AddRequestedIntake ?? 0,

                            AY2027_TotalIntake =
                                 existing?.Ay2027TotalIntake ?? 0
                        };
                }

                // Fallback → only MstCourses (for Dental etc.)
                return
                    from c in levelCourses
                    join e in existingIntakes
                        on c.CourseCode.ToString() equals e.Courses into ej
                    from existing in ej.DefaultIfEmpty()

                    select new IntakeByLevelViewModel1
                    {
                        CourseCode = c.CourseCode.ToString(),
                        CourseName = c.CourseName,

                        AY2024_ExistingIntake =
                            existing?.Ay2024ExistingIntake ?? 0,

                        AY2024_IncreaseIntake =
                            existing?.Ay2024IncreaseIntake ?? 0,

                        AY2024_TotalIntake =
                            existing?.Ay2024TotalIntake ?? 0,

                        AY2025_ExistingIntake =
                            existing?.Ay2025ExistingIntake ?? 0,

                        AY2025_LopNmcIntake =
                            existing?.Ay2025LopNmcIntake ?? 0,

                        AY2025_TotalIntake =
                            existing?.Ay2025TotalIntake ?? 0,

                        AY2025_LopDate =
                            existing?.Ay2025LopDate,

                        AY2026_ExistingIntake =
                            existing?.Ay2026ExistingIntake ?? 0,

                        AY2026_AddRequestedIntake =
                            existing?.Ay2026AddRequestedIntake ?? 0,

                        AY2026_TotalIntake =
                            existing?.Ay2026TotalIntake ?? 0,
                        AY2027_ExistingIntake =
                         existing?.Ay2027ExistingIntake ?? 0,

                        AY2027_AddRequestedIntake =
                            existing?.Ay2027AddRequestedIntake ?? 0,

                        AY2027_TotalIntake =
                            existing?.Ay2027TotalIntake ?? 0
                    };
            }

            model.UgCourses = Project("UG").DistinctBy(x => x.CourseCode).ToList();
            model.PgCourses = Project("PG").DistinctBy(x => x.CourseCode).ToList();
            model.SsCourses = Project("SS").DistinctBy(x => x.CourseCode).ToList();

            // ── Saved records summary table ────────────────────────────────
            model.SavedIntakes = (
                from e in existingIntakes
                where int.TryParse(e.Courses, out _)
                let codeInt = int.Parse(e.Courses)
                join c in allCourses on codeInt equals c.CourseCode
                select new SavedAcademicIntakeRowViewModel1
                {
                    Id = e.Id,
                    CourseLevel = c.CourseLevel,
                    CourseCode = e.Courses,
                    CourseName = c.CourseName,
                    AY2024_TotalIntake = e.Ay2024TotalIntake,
                    AY2025_TotalIntake = e.Ay2025TotalIntake,
                    AY2026_TotalIntake = e.Ay2026TotalIntake,
                    AY2027_TotalIntake = e.Ay2027TotalIntake,
                    HasNmcDocument =
                            e.Ay2025NmcDocument != null &&
                            e.Ay2025NmcDocument.Length > 0,

                    HasLopDocument =
                        e.Ay2025LopDocument != null &&
                        e.Ay2025LopDocument.Length > 0,

                    HasDciDocument =
                                (e.Ay2025Dcidocument != null &&
                                 e.Ay2025Dcidocument.Length > 0)

                                ||

                                (e.Ay2026Dcidocument != null &&
                                 e.Ay2026Dcidocument.Length > 0)

                                ||

                                (e.Ay2027Dcidocument != null &&
                                 e.Ay2027Dcidocument.Length > 0),

                  HasKsdcDocument =
                                (e.Ay2025Ksdcdocument != null &&
                                 e.Ay2025Ksdcdocument.Length > 0)

                                ||

                                (e.Ay2026Ksdcdocument != null &&
                                 e.Ay2026Ksdcdocument.Length > 0)

                                ||

                                (e.Ay2027Ksdcdocument != null &&
                                 e.Ay2027Ksdcdocument.Length > 0),

                    CreatedOn = DateTime.Now
                }
            )
            .OrderBy(x => x.CourseLevel == "UG" ? 1 : x.CourseLevel == "PG" ? 2 : 3)
            .ToList();
        }


        [HttpGet]
        public async Task<IActionResult> ViewNmcDocument(int id)
        {
            var doc = await _context.AcademicIntakes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doc == null || doc.Ay2025NmcDocument == null)
                return NotFound();

            Response.Headers.Add("Content-Disposition", "inline");

            return File(
                doc.Ay2025NmcDocument,
                "application/pdf");
        }
    }
}