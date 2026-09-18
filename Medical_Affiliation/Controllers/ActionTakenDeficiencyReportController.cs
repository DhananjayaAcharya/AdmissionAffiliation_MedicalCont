using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class ActionTakenDeficiencyReportController : BaseController
    {
        public ActionTakenDeficiencyReportController(
            ApplicationDbContext context)
            : base(context)
        {
        }


        // ============================================================
        // INDEX
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ============================================================
            // GET SESSION VALUES
            // ============================================================

            var collegeCode = CollegeCode;

            // FacultyCode itself represents FacultyId
            var facultyCode = FacultyCode;

            var typeId = HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel = HttpContext.Session.GetString("CourseLevel");


            // ============================================================
            // VALIDATE SESSION
            // ============================================================

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["Error"] =
                    "Faculty, College, Affiliation Type or Course Level session details are missing.";

                return RedirectToAction("Index", "MainDashboard");
            }


            // ============================================================
            // CONVERT FACULTY CODE TO FACULTY ID
            // ============================================================

            if (!int.TryParse(facultyCode, out int facultyId))
            {
                TempData["Error"] = "Invalid faculty details.";

                return RedirectToAction("Index", "MainDashboard");
            }


            // ============================================================
            // GET EXISTING RECORDS
            // ============================================================

            var records = await _context.ActionTakenDeficiencyReports
                .Include(x => x.Faculty)
                .Include(x => x.Type)
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId &&
                    x.TypeId == typeId.Value &&
                    x.IsActive &&
                    x.CourseLevel == courseLevel)
                .OrderByDescending(x => x.ActionTakenDeficiencyReportId)
                .ToListAsync();


            // ============================================================
            // CREATE VIEW MODEL
            // ============================================================

            var model = new ActionTakenDeficiencyReportViewModel
            {
                ActionTakenDeficiencyReportId = 0,

                FacultyCode = facultyCode,

                CollegeCode = collegeCode,

                TypeId = typeId.Value,

                CourseLevel = courseLevel,

                ActionTakenDeficiencyRecords = records
            };


            return View(model);
        }


        // ============================================================
        // CREATE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string? deficiencyPointedOut,
            string? extentRemedied,
            IFormFile? relevantReportFile)
        {
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;

            var typeId = HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["Error"] =
                    "Session expired. Please login again.";

                return RedirectToAction(
                    "MultiLogin",
                    "MainDashboard");
            }

            if (!int.TryParse(facultyCode, out int facultyId))
            {
                TempData["Error"] =
                    "Invalid Faculty information.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validation
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(deficiencyPointedOut))
            {
                TempData["Error"] =
                    "Please enter the deficiency pointed out.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(extentRemedied))
            {
                TempData["Error"] =
                    "Please enter the extent to which the deficiency was remedied.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate affiliation type
            // --------------------------------------------------------

            var typeExists =
                await _context.TypeOfAffiliations
                    .AnyAsync(x => x.TypeId == typeId.Value);

            if (!typeExists)
            {
                TempData["Error"] =
                    "Invalid affiliation type.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Upload relevant report
            // --------------------------------------------------------

            string? reportPath = null;

            if (relevantReportFile != null &&
                relevantReportFile.Length > 0)
            {
                var extension =
                    Path.GetExtension(
                        relevantReportFile.FileName)
                        .ToLowerInvariant();

                if (extension != ".pdf")
                {
                    TempData["Error"] =
                        "Only PDF files are allowed.";

                    return RedirectToAction(nameof(Index));
                }

                if (relevantReportFile.Length >
                    1 * 1024 * 1024)
                {
                    TempData["Error"] =
                        "Report file size must not exceed 1 MB.";

                    return RedirectToAction(nameof(Index));
                }

                reportPath =
                    await SaveFileAndReturnPath(
                        relevantReportFile,
                        "ActionTakenDeficiencyReport");
            }


            // --------------------------------------------------------
            // Create entity
            // --------------------------------------------------------

            var entity =
                new ActionTakenDeficiencyReport
                {
                    FacultyId = facultyId,
                    CollegeCode = collegeCode,
                    TypeId = typeId.Value,
                    CourseLevel = courseLevel,

                    DeficiencyPointedOut =
                        deficiencyPointedOut.Trim(),

                    ExtentRemedied =
                        extentRemedied.Trim(),

                    RelevantReportPath =
                        reportPath,

                    IsActive = true,

                    CreatedBy =
                        User.Identity?.Name,

                    CreatedDate =
                        DateTime.Now
                };

            _context.ActionTakenDeficiencyReports
                .Add(entity);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Action taken deficiency report added successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;
            var typeId = AffTypeId;
            var courseLevel = CourseLevel;

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["Error"] =
                    "Session expired. Please login again.";

                return RedirectToAction(
                    "MultiLogin",
                    "MainDashboard");
            }

            if (!int.TryParse(facultyCode, out int facultyId))
            {
                TempData["Error"] =
                    "Invalid Faculty information.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Find record
            // --------------------------------------------------------

            var entity =
                await _context.ActionTakenDeficiencyReports
                    .FirstOrDefaultAsync(x =>
                        x.ActionTakenDeficiencyReportId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);

            if (entity == null)
            {
                TempData["Error"] =
                    "Record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Load all records
            // --------------------------------------------------------

            var records =
                await _context.ActionTakenDeficiencyReports
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.ActionTakenDeficiencyReportId)
                    .ToListAsync();


            // --------------------------------------------------------
            // Build ViewModel
            // --------------------------------------------------------

            var model =
                new ActionTakenDeficiencyReportViewModel
                {
                    ActionTakenDeficiencyReportId =
                        entity.ActionTakenDeficiencyReportId,

                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    TypeId = typeId.Value,
                    CourseLevel = courseLevel,

                    DeficiencyPointedOut =
                        entity.DeficiencyPointedOut,

                    ExtentRemedied =
                        entity.ExtentRemedied,

                    RelevantReportPath =
                        entity.RelevantReportPath,

                    ActionTakenDeficiencyRecords =
                        records
                };

            return View("Index", model);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string? deficiencyPointedOut,
            string? extentRemedied,
            IFormFile? relevantReportFile)
        {
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;
            var typeId = AffTypeId;
            var courseLevel = CourseLevel;

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["Error"] =
                    "Session expired. Please login again.";

                return RedirectToAction(
                    "MultiLogin",
                    "MainDashboard");
            }

            if (!int.TryParse(facultyCode, out int facultyId))
            {
                TempData["Error"] =
                    "Invalid Faculty information.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validation
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(deficiencyPointedOut))
            {
                TempData["Error"] =
                    "Please enter the deficiency pointed out.";

                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(extentRemedied))
            {
                TempData["Error"] =
                    "Please enter the extent to which the deficiency was remedied.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Find record
            // --------------------------------------------------------

            var entity =
                await _context.ActionTakenDeficiencyReports
                    .FirstOrDefaultAsync(x =>
                        x.ActionTakenDeficiencyReportId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);

            if (entity == null)
            {
                TempData["Error"] =
                    "Record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Update details
            // --------------------------------------------------------

            entity.DeficiencyPointedOut =
                deficiencyPointedOut.Trim();

            entity.ExtentRemedied =
                extentRemedied.Trim();


            // --------------------------------------------------------
            // Replace report if new file selected
            // --------------------------------------------------------

            if (relevantReportFile != null &&
                relevantReportFile.Length > 0)
            {
                var extension =
                    Path.GetExtension(
                        relevantReportFile.FileName)
                        .ToLowerInvariant();

                if (extension != ".pdf")
                {
                    TempData["Error"] =
                        "Only PDF files are allowed.";

                    return RedirectToAction(nameof(Index));
                }

                if (relevantReportFile.Length >
                    1 * 1024 * 1024)
                {
                    TempData["Error"] =
                        "Report file size must not exceed 1 MB.";

                    return RedirectToAction(nameof(Index));
                }


                // ----------------------------------------------------
                // Delete old report
                // ----------------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    entity.RelevantReportPath))
                {
                    DeleteOldFile(
                        entity.RelevantReportPath,
                        facultyCode);
                }


                // ----------------------------------------------------
                // Save new report using BaseController
                // ----------------------------------------------------

                entity.RelevantReportPath =
                    await SaveFileAndReturnPath(
                        relevantReportFile,
                        "ActionTakenDeficiencyReport");
            }


            // --------------------------------------------------------
            // Audit
            // --------------------------------------------------------

            entity.ModifiedBy =
                User.Identity?.Name;

            entity.ModifiedDate =
                DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Action taken deficiency report updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // ============================================================
            // GET SESSION VALUES
            // ============================================================

            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;

            // Affiliation Type ID is stored using "AffiliationType"
            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            // Course Level is stored using "CourseLevel"
            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");


            // ============================================================
            // VALIDATE SESSION
            // ============================================================

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["Error"] =
                    "Session expired. Please login again.";

                return RedirectToAction(
                    "MultiLogin",
                    "MainDashboard");
            }


            // ============================================================
            // CONVERT FACULTY CODE TO FACULTY ID
            // ============================================================

            if (!int.TryParse(facultyCode, out int facultyId))
            {
                TempData["Error"] =
                    "Invalid Faculty information.";

                return RedirectToAction(nameof(Index));
            }


            // ============================================================
            // FIND RECORD
            // ============================================================

            var entity =
                await _context.ActionTakenDeficiencyReports
                    .FirstOrDefaultAsync(x =>
                        x.ActionTakenDeficiencyReportId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            // ============================================================
            // RECORD NOT FOUND
            // ============================================================

            if (entity == null)
            {
                TempData["Error"] =
                    "Record not found.";

                return RedirectToAction(nameof(Index));
            }


            // ============================================================
            // SOFT DELETE
            // ============================================================

            entity.IsActive = false;

            entity.ModifiedBy =
                User.Identity?.Name;

            entity.ModifiedDate =
                DateTime.Now;


            // ============================================================
            // SAVE
            // ============================================================

            await _context.SaveChangesAsync();


            // ============================================================
            // SUCCESS MESSAGE
            // ============================================================

            TempData["Success"] =
                "Action taken deficiency report removed successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETE OLD FILE
        // ============================================================
        private void DeleteOldFile(
            string? storedPath,
            string facultyCode)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return;

            try
            {
                string absolutePath;

                // Existing path is already physical path
                if (Path.IsPathRooted(storedPath.Trim()))
                {
                    absolutePath = storedPath.Trim();
                }
                else
                {
                    string rootPath =
                        facultyCode == "2"
                            ? BaseDentalPath
                            : BaseMedicalPath;

                    var normalized =
                        storedPath
                            .Trim()
                            .Replace(
                                "/",
                                Path.DirectorySeparatorChar.ToString());

                    absolutePath =
                        Path.Combine(
                            rootPath,
                            normalized);
                }

                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error deleting old report: {ex.Message}");
            }
        }
    }
}