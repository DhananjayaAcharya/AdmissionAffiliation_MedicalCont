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
                    int t2024 = inc.Ay2024ExistingIntake + inc.Ay2024IncreaseIntake;
                    int t2025 = inc.Ay2025ExistingIntake + inc.Ay2025LopNmcIntake;
                    int e2026 = t2025;                                         // carry-forward
                    int t2026 = e2026 + inc.Ay2026AddRequestedIntake;

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
                        if (inc.Ay2025NmcDocument is { Length: > 0 })
                            db.Ay2025NmcDocument = inc.Ay2025NmcDocument;

                        db.Ay2026ExistingIntake = e2026;
                        db.Ay2026AddRequestedIntake = inc.Ay2026AddRequestedIntake;
                        db.Ay2026TotalIntake = t2026;

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
        private static AcademicIntake MapToEntity(
            string facultyCode,
            string collegeCode,
            IntakeByLevelViewModel1 vm)
        {
            byte[]? docBytes = null;
            if (vm.AY2025_NmcDocument is { Length: > 0 })
            {
                try
                {
                    using var ms = new MemoryStream((int)vm.AY2025_NmcDocument.Length);
                    vm.AY2025_NmcDocument.CopyTo(ms);
                    docBytes = ms.ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] NMC document read failed for {vm.CourseCode}: {ex.Message}");
                }
            }

            return new AcademicIntake
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                Courses = vm.CourseCode!,

                Ay2024ExistingIntake = (int)vm.AY2024_ExistingIntake,
                Ay2024IncreaseIntake = (int)vm.AY2024_IncreaseIntake,
                Ay2024TotalIntake = (int)vm.AY2024_TotalIntake,   // recalculated server-side after this

                Ay2025ExistingIntake = (int)vm.AY2025_ExistingIntake,
                Ay2025LopNmcIntake = (int)vm.AY2025_LopNmcIntake,
                Ay2025TotalIntake = (int)vm.AY2025_TotalIntake,   // recalculated server-side after this
                Ay2025LopDate = vm.AY2025_LopDate,
                Ay2025NmcDocument = docBytes,

                Ay2026ExistingIntake = (int)vm.AY2026_ExistingIntake,    // recalculated server-side after this
                Ay2026AddRequestedIntake = (int)vm.AY2026_AddRequestedIntake,
                Ay2026TotalIntake = (int)vm.AY2026_TotalIntake         // recalculated server-side after this
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
        private void BuildModelData(
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
            IEnumerable<IntakeByLevelViewModel1> Project(string level) =>
                from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                let codeInt = int.Parse(d.CourseCode!)
                join c in allCourses on codeInt equals c.CourseCode
                where c.CourseLevel == level
                join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                from existing in ej.DefaultIfEmpty()
                select new IntakeByLevelViewModel1
                {
                    CourseCode = c.CourseCode.ToString(),
                    CourseName = c.CourseName,

                    // 2024-25
                    AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                    AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                    AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),

                    // 2025-26  (use DB value for existing, master-table value for new)
                    AY2025_ExistingIntake = existing?.Ay2025ExistingIntake ?? d.ExistingIntake ?? 0,
                    AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                    AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                    AY2025_LopDate = existing?.Ay2025LopDate,

                    // 2026-27
                    AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                    AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                    AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                };

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
                    HasNmcDocument = e.Ay2025NmcDocument != null && e.Ay2025NmcDocument.Length > 0,
                    CreatedOn = DateTime.Now
                }
            )
            .OrderBy(x => x.CourseLevel == "UG" ? 1 : x.CourseLevel == "PG" ? 2 : 3)
            .ToList();
        }
    }
}