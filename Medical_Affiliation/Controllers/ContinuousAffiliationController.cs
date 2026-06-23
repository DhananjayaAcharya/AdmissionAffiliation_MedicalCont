using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Services.UserContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class ContinuousAffiliationController : BaseController
    {

        private readonly ApplicationDbContext _context;
        private readonly SessionUserContext _userContext;
        private readonly IUserContext __userContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ContinuousAffiliationController( ApplicationDbContext context): base(context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        //[HttpGet]
        //public IActionResult SetLevel(string level)
        //{
        //    if (level is "UG" or "PG" or "SS")
        //    {
        //        // If level changed, reset completed steps
        //        var existing = HttpContext.Session.GetString("CourseLevel");
        //        if (existing != level)
        //            HttpContext.Session.Remove("CA_Done");

        //        HttpContext.Session.SetString("CourseLevel", level);
        //    }

        //    // Always redirect to Institution Details (first step) for UG
        //    // Extend this switch when PG/SS steps are ready
        //    return level switch
        //    {
        //        "UG" => RedirectToAction("Institution_Details", "ContinuesAffiliation_Facultybased"),
        //        "PG" => RedirectToAction("Institution_Details", "ContinuesAffiliation_Facultybased"),
        //        "SS" => RedirectToAction("Institution_Details", "ContinuesAffiliation_Facultybased"),
        //        _ => RedirectToAction("ChooseLevel")
        //    };
        //}


        public IActionResult SetLevel(
                                        string level,
                                        string redirectController,
                                        string redirectAction)
        {
            if (level is "UG" or "PG" or "SS")
            {
                HttpContext.Session.SetString("CourseLevel", level);
            }

            // ✅ USE dynamic redirect
            return RedirectToAction(
                redirectAction,
                redirectController,
                new { level = level }
            );
        }


        // Utility: call this from any CA POST controller after SaveChangesAsync()
        // to mark that step green in the sidebar
        //
        // Usage in your CA controller:
        //   ContinuousAffiliationController.MarkDone(HttpContext, "BasicDetails");
        //
        public static void MarkDone(HttpContext ctx, string stepKey)
        {
            var raw = ctx.Session.GetString("CA_Done") ?? "";
            var steps = raw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!steps.Contains(stepKey))
            {
                steps.Add(stepKey);
                ctx.Session.SetString("CA_Done", string.Join(',', steps));
            }
        }

        protected async Task MarkStepCompleted(string collegeCode, string courseLevel, string stepKey)
        {
            var existing = await _context.CaProgresses
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.CourseLevel == courseLevel &&
                    x.StepKey == stepKey);

            if (existing == null)
            {
                _context.CaProgresses.Add(new CaProgress
                {
                    CollegeCode = collegeCode,
                    CourseLevel = courseLevel,
                    StepKey = stepKey,
                    IsCompleted = true,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                existing.IsCompleted = true;
                existing.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public IActionResult AcademicIntake()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(facultyCode) ||
                string.IsNullOrWhiteSpace(collegeCode))
            {
                return RedirectToAction("CollegeLogin");
            }

            var model = new AcademicIntakeViewModel();

            // Active Academic Years
            var activeYears = _context.AcademicYearMasters
                .Where(x => x.IsActive==true)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.AcademicYear)
                .ToList();

            // UG Courses
            var courses = _context.MstCourses
                .Where(x => x.FacultyCode.ToString() == facultyCode
                         && x.CourseLevel == "UG")
                .ToList();

            // Current Intake Details
            var intakeDetails = _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode
                         && x.FacultyCode.ToString() == facultyCode)
                .ToList();

            // Saved Year Wise Intake Records
            var savedYearWiseIntakes = _context.AcademicIntakeYearWises
                .Where(x => x.CollegeCode == collegeCode
                         && x.FacultyCode == facultyCode)
                .ToList();

            foreach (var course in courses)
            {
                var intake = intakeDetails
                    .FirstOrDefault(x =>
                        x.CourseCode == course.CourseCode.ToString());

                // Current Approved Intake for Card Header
                var currentApprovedIntake =
                    intake?.PresentIntake ?? 0;

                var yearWiseData =
                    new List<AcademicYearIntakeVm>();

                foreach (var year in activeYears)
                {
                    var saved = savedYearWiseIntakes
                        .FirstOrDefault(x =>
                            x.CourseCode == course.CourseCode.ToString()
                            && x.AcademicYear == year);

                    // First row -> user editable and saved value should be shown
                    int existingIntake;

                    if (!yearWiseData.Any())
                    {
                        existingIntake =
                            saved?.ExistingIntake
                            ?? currentApprovedIntake;
                    }
                    else
                    {
                        // Remaining rows -> show saved value if available,
                        // otherwise take previous row total
                        existingIntake =
                            saved?.ExistingIntake
                            ?? yearWiseData.Last().TotalIntake;
                    }

                    // Additional Intake
                    int additionalIntake =
                        saved?.AdditionalIntake ?? 0;

                    // Total Intake
                    int totalIntake =
                        saved?.TotalIntake
                        ?? (existingIntake + additionalIntake);

                    yearWiseData.Add(
                        new AcademicYearIntakeVm
                        {
                            AcademicYear = year,

                            ExistingIntake = existingIntake,

                            AdditionalIntake = additionalIntake,

                            TotalIntake = totalIntake,
                            ApprovalAuthority =saved?.ApprovalType?.Trim().ToUpper(),

                            LopDate =
                                saved?.LopDate != null
                                    ? saved.LopDate.Value
                                        .ToDateTime(TimeOnly.MinValue)
                                    : null,

                            DocumentPath =
                                saved?.DocumentPath
                        });
                }

                model.Courses.Add(
                    new CourseIntakeViewModel
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,

                        // Header
                        CurrentApprovedIntake =
                            currentApprovedIntake,

                        YearWiseIntakes = yearWiseData
                    });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicIntake(AcademicIntakeViewModel model)
        {
            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(facultyCode) ||
                string.IsNullOrWhiteSpace(collegeCode))
            {
                return RedirectToAction("CollegeLogin");
            }

            foreach (var course in model.Courses)
            {
                foreach (var year in course.YearWiseIntakes)
                {
                    var entity =
                        _context.AcademicIntakeYearWises
                        .FirstOrDefault(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseCode == course.CourseCode.ToString() &&
                            x.AcademicYear == year.AcademicYear);

                    if (entity == null)
                    {
                        entity = new AcademicIntakeYearWise
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseCode = course.CourseCode.ToString(),
                            AcademicYear = year.AcademicYear
                        };

                        _context.AcademicIntakeYearWises.Add(entity);
                    }

                    entity.ExistingIntake =
                        year.ExistingIntake;

                    entity.AdditionalIntake =
                        year.AdditionalIntake;

                    entity.TotalIntake =
                        year.ExistingIntake +
                        year.AdditionalIntake;

                    entity.ApprovalType =
                        year.ApprovalAuthority;

                    entity.LopDate =
                        year.LopDate.HasValue
                            ? DateOnly.FromDateTime(
                                year.LopDate.Value)
                            : null;

                    // Save document
                    if (year.Document != null)
                    {
                        // Delete old document
                        if (!string.IsNullOrWhiteSpace(
                                entity.DocumentPath) &&
                            System.IO.File.Exists(
                                entity.DocumentPath))
                        {
                            System.IO.File.Delete(
                                entity.DocumentPath);
                        }

                        var filePath = await SaveCourseFileAsync(
                                     year.Document,
                                     Path.Combine(
                                         "AcademicIntake",
                                         year.AcademicYear.Replace("/", "-")),
                                     facultyCode);

                        if (!string.IsNullOrWhiteSpace(filePath))
                        {
                            entity.DocumentPath = filePath;
                        }
                    }
                }


                // Update master intake table
                var latestYear = course.YearWiseIntakes.LastOrDefault();

                if (latestYear != null)
                {
                    var courseIntake = _context.CollegeCourseIntakeDetails
                        .FirstOrDefault(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode.ToString() == facultyCode &&
                            x.CourseCode == course.CourseCode.ToString());

                    if (courseIntake != null)
                    {
                        courseIntake.PresentIntake = latestYear.TotalIntake;
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Academic Intake Details Saved Successfully.";

            return RedirectToAction(nameof(AcademicIntake));
        }

        private async Task<string?> SaveCourseFileAsync(
     IFormFile? file,
     string folder,
     string facultyCode)
        {
            if (file == null || file.Length == 0)
                return null;

            string rootPath = facultyCode == "2"
                ? BaseDentalPath
                : BaseMedicalPath;

            string basePath =
                Path.Combine(rootPath, "CourseDetails");

            string fullFolder =
                Path.Combine(basePath, folder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string fileName =
                Guid.NewGuid() +
                Path.GetExtension(file.FileName);

            string fullPath =
                Path.Combine(fullFolder, fileName);

            using (var stream =
                   new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return FULL PHYSICAL PATH
            return fullPath;
        }

        [HttpGet]
        public IActionResult ViewAcademicIntakeDocument(int courseCode,string academicYear)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(facultyCode) ||
                string.IsNullOrWhiteSpace(collegeCode))
            {
                return RedirectToAction("CollegeLogin");
            }

            var entity = _context.AcademicIntakeYearWises
                .FirstOrDefault(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseCode == courseCode.ToString() &&
                    x.AcademicYear == academicYear);

            if (entity == null ||
                string.IsNullOrWhiteSpace(entity.DocumentPath))
            {
                return NotFound("Document not found.");
            }

            // If relative path is stored, build absolute path
            string filePath = entity.DocumentPath;

            if (!Path.IsPathRooted(filePath))
            {
                string rootPath = facultyCode == "2"
                    ? BaseDentalPath
                    : BaseMedicalPath;

                filePath = Path.Combine(rootPath, filePath);
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Detect content type
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            // Open in browser instead of download
            Response.Headers["Content-Disposition"] = "inline";

            return PhysicalFile(filePath, contentType);
        }



        public IActionResult AcademicIntakePG(int? SelectedCourseCode)
        {
            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(facultyCode) ||
                string.IsNullOrWhiteSpace(collegeCode))
            {
                return RedirectToAction("CollegeLogin");
            }

            var model = new PgAcademicIntakeViewModel();

            // PG Courses Dropdown
            model.PgCourses = _context.MstCourses
                .Where(x =>
                    x.FacultyCode.ToString() == facultyCode &&
                    x.CourseLevel == "PG")
                .OrderBy(x => x.CourseName)
                .Select(x => new SelectListItem
                {
                    Value = x.CourseCode.ToString(),
                    Text = x.CourseName
                })
                .ToList();

            model.SelectedCourseCode = SelectedCourseCode;

            // First page load
            if (!SelectedCourseCode.HasValue)
                return View(model);

            // Selected Course
            var course = _context.MstCourses
                .FirstOrDefault(x =>
                    x.CourseCode == SelectedCourseCode.Value);

            if (course == null)
                return View(model);

            // Current Intake
            var currentIntake =
                _context.CollegeCourseIntakeDetails
                .FirstOrDefault(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode.ToString() == facultyCode &&
                    x.CourseCode == SelectedCourseCode.ToString());

            var currentApprovedIntake =
                currentIntake?.PresentIntake ?? 0;

            // Academic Years
            var activeYears =
                _context.AcademicYearMasters
                .Where(x => x.IsActive == true)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.AcademicYear)
                .ToList();

            // Saved Data
            var savedData =
                _context.AcademicIntakeYearWises
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseCode == SelectedCourseCode.ToString())
                .ToList();

            var yearWise =
                new List<AcademicYearIntakeVm>();

            foreach (var year in activeYears)
            {
                var saved = savedData
                    .FirstOrDefault(x =>
                        x.AcademicYear == year);

                int existing;

                if (!yearWise.Any())
                {
                    existing =
                        saved?.ExistingIntake
                        ?? currentApprovedIntake;
                }
                else
                {
                    existing =
                        saved?.ExistingIntake
                        ?? yearWise.Last().TotalIntake;
                }

                int additional =
                    saved?.AdditionalIntake ?? 0;

                int total =
                    saved?.TotalIntake
                    ?? (existing + additional);

                yearWise.Add(
                    new AcademicYearIntakeVm
                    {
                        AcademicYear = year,

                        ExistingIntake = existing,

                        AdditionalIntake = additional,

                        TotalIntake = total,

                        ApprovalAuthority =
                            saved?.ApprovalType?.Trim().ToUpper(),

                        LopDate =
                            saved?.LopDate != null
                                ? saved.LopDate.Value
                                    .ToDateTime(TimeOnly.MinValue)
                                : null,

                        DocumentPath =
                            saved?.DocumentPath
                    });
            }

            model.Course = new CourseIntakeViewModel
            {
                CourseCode = course.CourseCode,

                CourseName = course.CourseName,

                CurrentApprovedIntake =
                    currentApprovedIntake,

                YearWiseIntakes = yearWise
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>AcademicIntakePG(PgAcademicIntakeViewModel model)
        {
            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            if (model.Course == null)
                return RedirectToAction(
                    nameof(AcademicIntakePG));

            foreach (var year in
                     model.Course.YearWiseIntakes)
            {
                var entity =
                    _context.AcademicIntakeYearWises
                    .FirstOrDefault(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseCode ==
                            model.Course.CourseCode.ToString()
                        &&
                        x.AcademicYear ==
                            year.AcademicYear);

                if (entity == null)
                {
                    entity =
                        new AcademicIntakeYearWise
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseCode =
                                model.Course.CourseCode
                                    .ToString(),
                            AcademicYear =
                                year.AcademicYear
                        };

                    _context.AcademicIntakeYearWises
                        .Add(entity);
                }

                entity.ExistingIntake =
                    year.ExistingIntake;

                entity.AdditionalIntake =
                    year.AdditionalIntake;

                entity.TotalIntake =
                    year.ExistingIntake +
                    year.AdditionalIntake;

                entity.ApprovalType =
                    year.ApprovalAuthority;

                entity.LopDate =
                    year.LopDate.HasValue
                    ? DateOnly.FromDateTime(
                        year.LopDate.Value)
                    : null;

                if (year.Document != null)
                {
                    var path =
                        await SaveCourseFileAsync(
                            year.Document,
                            Path.Combine(
                                "AcademicIntake",
                                year.AcademicYear),
                            facultyCode);

                    entity.DocumentPath = path;
                }
            }

            // Update CollegeCourseIntakeDetails
            var latestYear =
                model.Course.YearWiseIntakes.LastOrDefault();

            if (latestYear != null)
            {
                var courseIntake =
                    _context.CollegeCourseIntakeDetails
                    .FirstOrDefault(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode.ToString() == facultyCode &&
                        x.CourseCode ==
                            model.Course.CourseCode.ToString());

                if (courseIntake != null)
                {
                    courseIntake.PresentIntake =
                        latestYear.TotalIntake;

                    // Optional
                    // courseIntake.ModifiedDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Saved Successfully.";

            return RedirectToAction(
                nameof(AcademicIntakePG),
                new
                {
                    courseCode =
                        model.Course.CourseCode
                });
        }
    }
}


