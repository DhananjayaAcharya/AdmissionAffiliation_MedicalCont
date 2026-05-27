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
                where ai.CollegeCode == collegeCode
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

            Console.WriteLine($"BaseController: Auth successful - CollegeCode: {CollegeCode}");

            base.OnActionExecuting(context);
        }
    }
}