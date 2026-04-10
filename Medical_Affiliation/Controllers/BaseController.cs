using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Medical_Affiliation.Controllers
{
    [Authorize(Policy = "CollegeOnly")]
    public class BaseController : Controller
    {
        protected string? FacultyCode => User.FindFirst("FacultyCode")?.Value;
        protected string? CollegeCode => User.FindFirst("CollegeCode")?.Value;
        protected string? CourseLevel => User.FindFirst("CourseLevel")?.Value;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine("=== BaseController.OnActionExecuting HIT ===");   // ← Add this line

            if (!(User.Identity?.IsAuthenticated ?? false) ||
                string.IsNullOrWhiteSpace(FacultyCode) ||
                string.IsNullOrWhiteSpace(CollegeCode))
            {
                Console.WriteLine("BaseController: Auth failed - Redirecting to Login");
                context.HttpContext.SignOutAsync("CollegeAuth").Wait();
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Login", null);
                return;
            }

            Console.WriteLine($"BaseController: Auth successful - CollegeCode: {CollegeCode}");
            base.OnActionExecuting(context);
        }
    }
}