using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Medical_Affiliation.Controllers
{
    public class AcademicIntakeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademicIntakeController(ApplicationDbContext context)
        {
            _context = context;

        }


        // Models/AcademicIntakePageViewModel.cs



        [HttpGet]
        public IActionResult AcademicIntakeData()
        {
            // Session pattern from your reference
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = HttpContext.Session.GetString("CollegeName");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Index", "Home");
            }

            // Safe parse for facultyId (exactly like your reference)
            if (!int.TryParse(facultyCode, out int facultyId))
            {
                return BadRequest("Invalid faculty code");
            }

            var model = new AcademicIntakePageViewModel1
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CollegeName = collegeName,
                FacultyId = facultyId
            };

            // Base data (EXACTLY like your reference)
            var intakeDetails = _context.CollegeCourseIntakeDetails
                .Where(d => d.FacultyCode == facultyId && d.CollegeCode == collegeCode)
                .ToList();

            var allCourses = _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)
                .ToList();

            // Load existing AcademicIntake data
            var existingIntakes = _context.AcademicIntakes
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .ToList();

            // ---------- UG SECTION (Exact pattern from your reference) ----------
            var ugintakelist = from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "UG"
                               join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                               from existing in ej.DefaultIfEmpty()
                               select new IntakeByLevelViewModel1
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                                   AY2024_TotalIntake = existing?.Ay2024ExistingIntake ?? d.ExistingIntake.GetValueOrDefault(),
                                   AY2025_ExistingIntake = d.ExistingIntake ?? 0,
                                   AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                                   AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                                   AY2025_LopDate = (existing?.Ay2025LopDate),
                                   AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                                   AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                                   AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                               };

            model.UgCourses = ugintakelist.ToList().DistinctBy(x => x.CourseCode).ToList();

            // ---------- PG SECTION ----------
            var pgintakelist = from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "PG"
                               join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                               from existing in ej.DefaultIfEmpty()
                               select new IntakeByLevelViewModel1
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                                   AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),
                                   AY2025_ExistingIntake = d.ExistingIntake ?? 0,
                                   AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                                   AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                                   AY2025_LopDate = (existing?.Ay2025LopDate),
                                   AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                                   AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                                   AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                               };

            model.PgCourses = pgintakelist.ToList().DistinctBy(x => x.CourseCode).ToList();

            // ---------- SS SECTION ----------
            var ssintakelist = from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "SS"
                               join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                               from existing in ej.DefaultIfEmpty()
                               select new IntakeByLevelViewModel1
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                                   AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),
                                   AY2025_ExistingIntake = d.ExistingIntake ?? 0,
                                   AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                                   AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                                   AY2025_LopDate = (existing?.Ay2025LopDate),
                                   AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                                   AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                                   AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                               };

            model.SsCourses = ssintakelist.ToList().DistinctBy(x => x.CourseCode).ToList();

            // Populate saved data table
            var savedIntakes = from e in existingIntakes
                               join c in allCourses on int.Parse(e.Courses) equals c.CourseCode
                               select new SavedAcademicIntakeRowViewModel1
                               {
                                   Id = e.Id,
                                   CourseLevel = c.CourseLevel,
                                   CourseCode = e.Courses,
                                   CourseName = c.CourseName,
                                   AY2024_TotalIntake = e.Ay2024ExistingIntake,
                                   AY2025_TotalIntake = e.Ay2025TotalIntake,
                                   AY2026_TotalIntake = e.Ay2026TotalIntake,
                                   HasNmcDocument = e.Ay2025NmcDocument != null,
                                   CreatedOn = DateTime.Now
                               };

            model.SavedIntakes = savedIntakes.OrderBy(x => x.CourseLevel == "UG" ? 1 : x.CourseLevel == "PG" ? 2 : 3).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> AcademicIntakeData(AcademicIntakePageViewModel1 model)
        {
            try
            {
                var facultyCode = HttpContext.Session.GetString("FacultyCode");
                var collegeCode = HttpContext.Session.GetString("CollegeCode");

                if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                {
                    return RedirectToAction("Collegelogin", "Login");
                }

                var incoming = new List<AcademicIntake>();

                void AddCourses(IEnumerable<IntakeByLevelViewModel1>? courses)
                {
                    if (courses == null) return;
                    foreach (var c in courses.Where(c => !string.IsNullOrEmpty(c.CourseCode)))
                    {
                        incoming.Add(CreateIntakeRecord(facultyCode, collegeCode, c.CourseCode, c));
                    }
                }

                AddCourses(model.UgCourses);
                AddCourses(model.PgCourses);
                AddCourses(model.SsCourses);

                if (incoming.Count == 0)
                {
                    TempData["SuccessMessage"] = "No courses/data to save.";
                    await LoadAcademicIntakeData(model);
                    return View(model);
                }

                // ────────────────────────────────────────────────
                // DEBUG: Log incoming values BEFORE any processing
                // ────────────────────────────────────────────────
                foreach (var inc in incoming)
                {
                    Console.WriteLine(
                        $"[INCOMING] Course: {inc.Courses,-30} | " +
                        $"2025 Exist: {inc.Ay2025ExistingIntake,5} | " +
                        $"2025 LOP: {inc.Ay2025LopNmcIntake,5} | " +
                        $"2026 Exist (from form): {inc.Ay2026ExistingIntake,5} | " +
                        $"2026 Requested: {inc.Ay2026AddRequestedIntake,5}"
                    );
                }

                var existing = await _context.AcademicIntakes
                    .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                    .ToDictionaryAsync(
                        x => x.Courses,
                        x => x,
                        StringComparer.OrdinalIgnoreCase);

                int added = 0;
                int updated = 0;

                foreach (var inc in incoming)
                {
                    if (existing.TryGetValue(inc.Courses, out var dbRecord))
                    {
                        // ────────────────────────────────────────────────
                        // UPDATE EXISTING RECORD
                        // ────────────────────────────────────────────────
                        dbRecord.FacultyCode = inc.FacultyCode;
                        dbRecord.CollegeCode = inc.CollegeCode;
                        dbRecord.Courses = inc.Courses;

                        // 2024–25
                        dbRecord.Ay2024ExistingIntake = inc.Ay2024ExistingIntake;
                        dbRecord.Ay2024IncreaseIntake = inc.Ay2024IncreaseIntake;
                        dbRecord.Ay2024TotalIntake = inc.Ay2024TotalIntake;

                        // 2025–26
                        dbRecord.Ay2025ExistingIntake = inc.Ay2025ExistingIntake;
                        dbRecord.Ay2025LopNmcIntake = inc.Ay2025LopNmcIntake;
                        dbRecord.Ay2025TotalIntake = inc.Ay2025TotalIntake;
                        dbRecord.Ay2025LopDate = inc.Ay2025LopDate;

                        if (inc.Ay2025NmcDocument != null && inc.Ay2025NmcDocument.Length > 0)
                        {
                            dbRecord.Ay2025NmcDocument = inc.Ay2025NmcDocument;
                        }

                        // 2026–27 - ENFORCE CARRY-FORWARD LOGIC SERVER-SIDE
                        // This is the most reliable way when client-side calculation is involved
                        int calculated2026Existing = (inc.Ay2025ExistingIntake) + (inc.Ay2025LopNmcIntake);

                        dbRecord.Ay2026ExistingIntake = calculated2026Existing;           // ← enforced
                        dbRecord.Ay2026AddRequestedIntake = inc.Ay2026AddRequestedIntake;
                        dbRecord.Ay2026TotalIntake = calculated2026Existing + (inc.Ay2026AddRequestedIntake);

                        // Optional: also log what we're actually saving
                        Console.WriteLine(
                            $"[SAVING UPDATE] {inc.Courses,-30} → 2026 Existing saved as: {dbRecord.Ay2026ExistingIntake}"
                        );

                        updated++;
                    }
                    else
                    {
                        // For new records - also enforce carry-forward
                        inc.Ay2026ExistingIntake = (inc.Ay2025ExistingIntake) + (inc.Ay2025LopNmcIntake);
                        inc.Ay2026TotalIntake = inc.Ay2026ExistingIntake + (inc.Ay2026AddRequestedIntake);

                        _context.AcademicIntakes.Add(inc);
                        added++;
                    }
                }

                int totalChanges = await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = totalChanges > 0
                    ? $"Saved successfully → {added} new, {updated} updated (DB changes: {totalChanges})"
                    : "No changes detected.";

                await LoadAcademicIntakeData(model);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Save error: {ex.Message}");
                if (ex.InnerException != null)
                    ModelState.AddModelError("", $"Inner: {ex.InnerException.Message}");

                await LoadAcademicIntakeData(model);
                return View(model);
            }
        }


        //// **Helper method to create AcademicIntake record**
        //private AcademicIntake CreateIntakeRecord(string facultyCode, string collegeCode, string courseCode, dynamic course)
        //{
        //    return new AcademicIntake
        //    {
        //        FacultyCode = facultyCode,
        //        CollegeCode = collegeCode,
        //        Courses = courseCode,
        //        Ay2024IncreaseIntake = course.AY2024_IncreaseIntake ?? 0,
        //        Ay2024TotalIntake = course.AY2024_TotalIntake ?? 0,
        //        Ay2025ExistingIntake = course.AY2025_ExistingIntake ?? 0,
        //        Ay2025LopNmcIntake = course.AY2025_LopNmcIntake ?? 0,
        //        Ay2025TotalIntake = course.AY2025_TotalIntake ?? 0,
        //        Ay2025LopDate = course.AY2025_LopDate,
        //        Ay2025NmcDocument = course.AY2025_NmcDocument != null
        //            ? Encoding.UTF8.GetBytes(course.AY2025_NmcDocument.ToString())
        //            : null,
        //        Ay2026ExistingIntake = course.AY2026_ExistingIntake ?? 0,
        //        Ay2026AddRequestedIntake = course.AY2026_AddRequestedIntake ?? 0,
        //        Ay2026TotalIntake = course.AY2026_TotalIntake ?? 0
        //    };
        //}


        // Helper method to reload data (extracted from GET for reuse)
        private async Task LoadAcademicIntakeData(AcademicIntakePageViewModel1 model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = HttpContext.Session.GetString("CollegeName");

            model.FacultyCode = facultyCode;
            model.CollegeCode = collegeCode;
            model.CollegeName = collegeName;

            if (!int.TryParse(facultyCode, out int facultyId)) return;

            model.FacultyId = facultyId;

            // Load base data (same as GET method)
            var intakeDetails = await _context.CollegeCourseIntakeDetails
                .Where(d => d.FacultyCode == facultyId && d.CollegeCode == collegeCode)
                .ToListAsync();

            var allCourses = await _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)
                .ToListAsync();

            var existingIntakes = await _context.AcademicIntakes
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .ToListAsync();

            // Populate UG, PG, SS courses (same LINQ as GET)
            model.UgCourses = (from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "UG"
                               join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                               from existing in ej.DefaultIfEmpty()
                               select new IntakeByLevelViewModel1
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                                   AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),
                                   AY2025_ExistingIntake = existing?.Ay2025ExistingIntake ?? 0,
                                   AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                                   AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                                   AY2025_LopDate = (existing?.Ay2025LopDate),
                                   AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                                   AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                                   AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                               }).DistinctBy(x => x.CourseCode).ToList();

            model.PgCourses = (from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "PG"
                               join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                               from existing in ej.DefaultIfEmpty()
                               select new IntakeByLevelViewModel1
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                                   AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),
                                   AY2025_ExistingIntake = existing?.Ay2025ExistingIntake ?? 0,
                                   AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                                   AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                                   AY2025_LopDate = (existing?.Ay2025LopDate),
                                   AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                                   AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                                   AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                               }).DistinctBy(x => x.CourseCode).ToList();

            model.SsCourses = (from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "SS"
                               join e in existingIntakes on c.CourseCode.ToString() equals e.Courses into ej
                               from existing in ej.DefaultIfEmpty()
                               select new IntakeByLevelViewModel1
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   AY2024_ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AY2024_IncreaseIntake = existing?.Ay2024IncreaseIntake ?? 0,
                                   AY2024_TotalIntake = existing?.Ay2024TotalIntake ?? d.ExistingIntake.GetValueOrDefault(),
                                   AY2025_ExistingIntake = existing?.Ay2025ExistingIntake ?? 0,
                                   AY2025_LopNmcIntake = existing?.Ay2025LopNmcIntake ?? 0,
                                   AY2025_TotalIntake = existing?.Ay2025TotalIntake ?? 0,
                                   AY2025_LopDate = (existing?.Ay2025LopDate),
                                   AY2026_ExistingIntake = existing?.Ay2026ExistingIntake ?? 0,
                                   AY2026_AddRequestedIntake = existing?.Ay2026AddRequestedIntake ?? 0,
                                   AY2026_TotalIntake = existing?.Ay2026TotalIntake ?? 0
                               }).DistinctBy(x => x.CourseCode).ToList();

            // Populate saved intakes table
            var savedIntakes = from e in existingIntakes
                               join c in allCourses on int.Parse(e.Courses) equals c.CourseCode
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
                               };

            model.SavedIntakes = savedIntakes.OrderBy(x => x.CourseLevel == "UG" ? 1 : x.CourseLevel == "PG" ? 2 : 3).ToList();
        }
        private AcademicIntake CreateIntakeRecord(
            string facultyCode,
            string collegeCode,
            string courseCode,
            IntakeByLevelViewModel1 course)
        {
            byte[]? nmcDocumentBytes = null;

            if (course.AY2025_NmcDocument is { Length: > 0 })
            {
                try
                {
                    using var ms = new MemoryStream((int)course.AY2025_NmcDocument.Length);
                    course.AY2025_NmcDocument.CopyTo(ms);
                    nmcDocumentBytes = ms.ToArray();
                }
                catch (Exception ex)
                {
                    // In production → use proper logger
                    Console.WriteLine($"Failed to read NMC document for {courseCode}: {ex.Message}");
                    // logger?.LogWarning(ex, "NMC document read failed for course {CourseCode}", courseCode);
                }
            }

            return new AcademicIntake
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                Courses = courseCode,

                // ──────────────────────────────
                //       2024–25 (all 3 fields)
                // ──────────────────────────────
                Ay2024ExistingIntake = course.AY2024_ExistingIntake ?? 0,
                Ay2024IncreaseIntake = course.AY2024_IncreaseIntake ?? 0,
                Ay2024TotalIntake = course.AY2024_TotalIntake
                                       ?? (course.AY2024_ExistingIntake ?? 0)
                                        + (course.AY2024_IncreaseIntake ?? 0),

                // ──────────────────────────────
                //       2025–26
                // ──────────────────────────────
                Ay2025ExistingIntake = course.AY2025_ExistingIntake ?? 0,
                Ay2025LopNmcIntake = course.AY2025_LopNmcIntake ?? 0,
                Ay2025TotalIntake = course.AY2025_TotalIntake
                                       ?? (course.AY2025_ExistingIntake ?? 0)
                                        + (course.AY2025_LopNmcIntake ?? 0),

                Ay2025LopDate = course.AY2025_LopDate,
                Ay2025NmcDocument = nmcDocumentBytes,

                // ──────────────────────────────
                //       2026–27
                // ──────────────────────────────
                Ay2026ExistingIntake = course.AY2026_ExistingIntake ?? 0,
                Ay2026AddRequestedIntake = course.AY2026_AddRequestedIntake ?? 0,
                Ay2026TotalIntake = course.AY2026_TotalIntake
                                           ?? (course.AY2026_ExistingIntake ?? 0)
                                            + (course.AY2026_AddRequestedIntake ?? 0),

                // Optional – good for auditing
                // CreatedAt = DateTime.UtcNow,
                // CreatedBy = HttpContext?.User?.Identity?.Name ?? "System",
                // IsActive  = true,
            };
        }

        // ✅ PERFECT File to Byte conversion
        private byte[] ConvertFileToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Array.Empty<byte>();

            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }


    }

}