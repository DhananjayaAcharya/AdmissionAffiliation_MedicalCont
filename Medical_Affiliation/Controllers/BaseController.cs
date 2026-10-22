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
        protected string? FacultyCode => HttpContext.Session.GetString("FacultyCode") ?? User.FindFirst("FacultyCode")?.Value;
        protected string? CollegeCode => HttpContext.Session.GetString("CollegeCode") ?? User.FindFirst("CollegeCode")?.Value;
        protected int? AffTypeId => ResolveAffiliationTypeId();
        protected int? NameOfAffiliationType => Convert.ToInt32(HttpContext.Session.GetString("TypeOfAffiliation"));

        protected string? SelectedCourseLevel => HttpContext.Session.GetString("SelectedCourseLevel");

        protected int? ResolveAffiliationTypeId(int? requestValue = null)
        {
            if (requestValue.HasValue && requestValue.Value > 0)
            {
                return requestValue.Value;
            }

            var formAffiliationTypeId = HttpContext.Request.HasFormContentType
                ? HttpContext.Request.Form["AffiliationTypeId"].FirstOrDefault()
                : null;
            var formAffiliationType = HttpContext.Request.HasFormContentType
                ? HttpContext.Request.Form["AffiliationType"].FirstOrDefault()
                : null;

            var sessionValues = new[]
            {
                HttpContext.Session.GetString("AffiliationTypeId"),
                HttpContext.Session.GetString("AffiliationType"),
                HttpContext.Request.Query["affiliationTypeId"].FirstOrDefault(),
                formAffiliationTypeId,
                HttpContext.Request.Query["AffiliationType"].FirstOrDefault(),
                formAffiliationType
            };

            foreach (var value in sessionValues)
            {
                if (int.TryParse(value, out var parsed) && parsed > 0)
                {
                    HttpContext.Session.SetString("AffiliationTypeId", parsed.ToString());
                    return parsed;
                }
            }

            return null;
        }

        protected string BaseMedicalPath
        {
            get
            {
                return Directory.Exists(@"E:\")
                    ? @"E:\Affiliation_Medical"
                    : @"D:\Affiliation_Medical";
            }
        }

        protected string BaseDentalPath
        {
            get
            {
                return Directory.Exists(@"E:\")
                    ? @"E:\Affiliation_Dental"
                    : @"D:\Affiliation_Dental";
            }
        }

        //below 1 code added by ram for ayurveda faculty
        protected string BaseAyurvedaPath
        {
            get
            {
                return Directory.Exists(@"E:\")
                    ? @"E:\Affiliation_Ayurveda"
                    : @"D:\Affiliation_Ayurveda";
            }
        }


        protected string CourseLevel
        {
            get
            {
                var level = HttpContext.Session.GetString("CourseLevel");
                if (string.IsNullOrWhiteSpace(level))
                {
                    level = User.FindFirst("CourseLevel")?.Value;
                }

                return string.IsNullOrWhiteSpace(level) ? "UG" : level.Trim();
            }
        }

        protected void SetCourseLevelFromRequest()
        {
            var requestCourseLevel = HttpContext.Request.Query["courseLevel"].ToString();
            if (string.IsNullOrEmpty(requestCourseLevel))
            {
                requestCourseLevel = HttpContext.Request.Query["level"].ToString();
            }

            if (string.IsNullOrWhiteSpace(requestCourseLevel))
            {
                return;
            }

            requestCourseLevel = requestCourseLevel.Trim().ToUpper();
            var validLevels = new[] { "UG", "PG", "SS" };
            if (!validLevels.Contains(requestCourseLevel))
            {
                return;
            }

            HttpContext.Session.SetString("CourseLevel", requestCourseLevel);
            // Keep legacy/session keys in sync
            HttpContext.Session.SetString("SelectedCourseLevel", requestCourseLevel);
            HttpContext.Session.SetString("SelectedLevel", requestCourseLevel);
        }

        protected async Task<List<string>> GetSortedCourseLevels()
        {
            if (FacultyCode == "4")
            {
                var ayurLevels = await CourseLevelHelper.GetMedicalIntakeLevelsAsync(_context, CollegeCode);
                return ayurLevels.Any() ? ayurLevels : new List<string> { "UG" };
            }

            var collegeCode = CollegeCode;

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

        protected async Task<string?> SaveFileAndReturnPath(IFormFile? file, string subFolder, string? filePrefix = null)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = FacultyCode switch
            {
                "2" => BaseDentalPath,
                "4" => BaseAyurvedaPath,
                _ => BaseMedicalPath
            };

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

        public static class CourseLevelHelper
        {
            public static string Normalize(string? level)
            {
                var l = level?.Trim().ToUpperInvariant() ?? string.Empty;
                if (l.Contains("UG") || l.Contains("UNDERGRADUATE")) return "UG";
                if (l.Contains("SS") || l.Contains("SUPERSPECIAL")) return "SS";
                if (l.Contains("PG") || l.Contains("POSTGRADUATE")) return "PG";
                return l;
            }

            public static readonly string[] Order = { "UG", "PG", "SS" };

            public static async Task<List<string>> GetMedicalIntakeLevelsAsync(ApplicationDbContext ctx, string? collegeCode)
            {
                var raw = await ctx.MstMedicalCollegeCourseIntakes
                    .AsNoTracking()
                    .Where(i => i.CollCode == collegeCode && i.UgPg != null)
                    .Select(i => i.UgPg!)
                    .Distinct()
                    .ToListAsync();

                return raw.Select(Normalize)
                          .Where(l => !string.IsNullOrWhiteSpace(l))
                          .Distinct()
                          .OrderBy(l => Array.IndexOf(Order, l) is var i && i >= 0 ? i : int.MaxValue)
                          .ToList();
            }
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
            if (string.IsNullOrEmpty(levelFromUrl))
            {
                levelFromUrl = context.HttpContext.Request.Query["courseLevel"].ToString();
            }

            if (!string.IsNullOrEmpty(levelFromUrl))
            {
                // normalize
                var normalized = levelFromUrl.Trim().ToUpper();
                context.HttpContext.Session.SetString("CourseLevel", normalized);
                //temp lines 

                Console.WriteLine("===== COURSE LEVEL DEBUG =====");
                Console.WriteLine($"Request URL: {context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}");
                Console.WriteLine($"Query courseLevel: {context.HttpContext.Request.Query["courseLevel"]}");
                Console.WriteLine($"Query level: {context.HttpContext.Request.Query["level"]}");
                Console.WriteLine($"Session before update: {context.HttpContext.Session.GetString("CourseLevel")}");
                Console.WriteLine($"Claim CourseLevel: {User.FindFirst("CourseLevel")?.Value}");
                Console.WriteLine("==============================");
                context.HttpContext.Session.SetString("SelectedCourseLevel", normalized);
                context.HttpContext.Session.SetString("SelectedLevel", normalized);
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