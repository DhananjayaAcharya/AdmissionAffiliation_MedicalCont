using Medical_Affiliation.DATA;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    [Authorize(Policy = "CollegeOnly")]
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }
        protected string? FacultyCode => User.FindFirst("FacultyCode")?.Value;
        protected string? CollegeCode => User.FindFirst("CollegeCode")?.Value;

        protected string BaseMedicalPath { get; } = @"E:\Affiliation_Medical";
        protected string BaseDentalPath { get; } = @"E:\Affiliation_Dental";

        protected string CourseLevel
        {
            get
            {
                var level = HttpContext.Request.Query["level"].ToString();

                if (string.IsNullOrEmpty(level))
                {
                    level = HttpContext.Session.GetString("CourseLevel");
                }

                if (string.IsNullOrEmpty(level))
                {
                    level = User.FindFirst("CourseLevel")?.Value;
                }

                return string.IsNullOrWhiteSpace(level) ? "UG" : level;
            }
        }

        protected async Task<List<string>> GetSortedCourseLevels()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            var order = new List<string> { "UG", "PG", "SS" };

            var levels = await (
                from ai in _context.AcademicIntakes
                join mc in _context.MstCourses
                    on ai.Courses equals mc.CourseCode.ToString()
                where ai.CollegeCode == collegeCode && ai.Ay2026TotalIntake > 0 
                      && !string.IsNullOrEmpty(ai.Courses)
                select mc.CourseLevel
            )
            .Distinct()
            .ToListAsync();

            levels = levels
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim().ToUpper())
                .Distinct()
                .OrderBy(l => order.Contains(l)
                    ? order.IndexOf(l)
                    : int.MaxValue)
                .ThenBy(l => l)
                .ToList();

            // fallback
            if (!levels.Any())
            {
                levels.Add("UG");
            }

            return levels;
        }

        protected async Task<int?> GetAnnualIntakeAsync()
        {
            var intake = await _context.AcademicIntakes
                .Where(x => x.CollegeCode == CollegeCode &&
                            x.FacultyCode == FacultyCode)
                .Select(x => (int?)x.Ay2026TotalIntake)
                .FirstOrDefaultAsync();

            return intake;
        }

        protected async Task<string?> SaveFileAndReturnPath( IFormFile? file, string subFolder, string? filePrefix = null)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = FacultyCode == "2"
                ? BaseDentalPath
                : BaseMedicalPath;

            // Creates:
            // E:\Affiliation_Medical\DepartmentWisePublications\
            string uploadFolder = Path.Combine(basePath, subFolder);

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            string extension = Path.GetExtension(file.FileName);

            string fileName = string.IsNullOrWhiteSpace(filePrefix)
                ? $"{Guid.NewGuid()}{extension}"
                : $"{filePrefix}_{Guid.NewGuid()}{extension}";

            string fullPath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine("=== BaseController.OnActionExecuting HIT ===");

            // ✅ SAFE AUTH CHECK (NO FORCE LOGOUT)
            if (!(User.Identity?.IsAuthenticated ?? false) ||
                string.IsNullOrWhiteSpace(FacultyCode) ||
                string.IsNullOrWhiteSpace(CollegeCode))
            {
                Console.WriteLine("BaseController: Auth failed");

                context.Result = new RedirectToActionResult("MultiLogin", "MainDashboard", null);
                return;
            }

            // ✅ Preserve CourseLevel logic
            var levelFromUrl = context.HttpContext.Request.Query["level"].ToString();

            if (!string.IsNullOrEmpty(levelFromUrl))
            {
                context.HttpContext.Session.SetString("CourseLevel", levelFromUrl);
            }

            if (FacultyCode == "2")
            {
                var controller = context.RouteData.Values["controller"]?.ToString();
                var action = context.RouteData.Values["action"]?.ToString();

                var excludedRoutes = new List<(string Controller, string Action)>
                {
                    ("ContinuousAffiliationIncreaseintake", "IncreaseIntake"),
                    ("CollegeLogin", "Dashboard"),
                    ("Common", "IntakePrerequisite"),
                    ("CollegeLogin", "GetTaluks")
                };

                bool isExcluded = excludedRoutes.Any(x =>
                    x.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                    x.Action.Equals(action, StringComparison.OrdinalIgnoreCase));

                bool hasIntakeData = _context.AcademicIntakes
                    .Any(x => x.CollegeCode == CollegeCode);

                if (!hasIntakeData && !isExcluded)
                {
                    context.Result = new ViewResult
                    {
                        ViewName = "~/Views/Shared/IntakePrerequisite.cshtml"
                    };

                    return;
                }
            }

            Console.WriteLine($"BaseController: Auth successful - CollegeCode: {CollegeCode}");

            base.OnActionExecuting(context);
        }
    }
}