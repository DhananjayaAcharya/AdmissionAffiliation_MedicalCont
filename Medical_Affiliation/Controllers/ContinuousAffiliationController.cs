using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.UserContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class ContinuousAffiliationController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly SessionUserContext _userContext;

        public ContinuousAffiliationController(ApplicationDbContext context)
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
    }
}