using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    [Authorize(Policy = "CollegeOnly")]
    public class BaseController : Controller
    {
        protected string? FacultyCode => User.FindFirst("FacultyCode")?.Value;
        protected string? CollegeCode => User.FindFirst("CollegeCode")?.Value;

        protected string BasePath { get; } = @"E:\Affiliation_Medical";

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

                return level ?? "UG";
            }
        }

        protected List<string> GetSortedCourseLevels(string raw)
        {
            var order = new List<string> { "UG", "PG", "SS" };

            var levels = string.IsNullOrEmpty(raw)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(raw)?
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim().ToUpper())
                    .Distinct()
                    .ToList() ?? new List<string>();

            return levels
                .OrderBy(l => order.Contains(l) ? order.IndexOf(l) : int.MaxValue)
                .ThenBy(l => l)
                .ToList();
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