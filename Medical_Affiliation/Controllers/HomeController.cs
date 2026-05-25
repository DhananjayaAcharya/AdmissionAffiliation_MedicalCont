using System.Diagnostics;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Medical_Affiliation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IStringLocalizer<HomeController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _context.UniversityImages.Select(e=> new UniversityImagesViewModel
            {
                ImgId = e.Id,
                ImgName = e.FileName,
                ImgByteArr = e.Photo
            }).ToListAsync();
            return View(images);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture) || !new[] { "en", "kn" }.Contains(culture))
            {
                return BadRequest(new { error = _localizer["InvalidCulture"].Value });
            }



            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                }
            );
            //return Ok();
            return LocalRedirect(returnUrl);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult SetCourseLevel(string courseLevel, string redirectController, string redirectAction)
        {
            courseLevel = courseLevel?.ToUpper();

            if (string.IsNullOrEmpty(courseLevel) ||
                !new[] { "UG", "PG", "SS" }.Contains(courseLevel))
            {
                HttpContext.Session.Clear();
                return Redirect("/Login/Login");
            }

            HttpContext.Session.SetString("CourseLevel", courseLevel);

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CourseLevel")))
            {
                HttpContext.Session.Clear();
                return Redirect("/Login/Login");
            }

            return RedirectToAction(redirectAction, redirectController);
        }
        public IActionResult Dashboard()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = User.Identity?.Name;
            ViewBag.CollegeName = collegeName; ViewBag.CollegeName = collegeName;

            
            ViewBag.CollegeName = collegeName ?? "Unknown College";
            ViewBag.CollegeCode = collegeCode;

            var hasPendingUpload = false;
            if (!string.IsNullOrEmpty(collegeCode))
            {
                hasPendingUpload = _context.UgFacultyDetails
                    .Any(f => f.CollegeCode == collegeCode && f.IsDeclared == true && f.PrintedCopyUploaded == false);
            }
            ViewBag.HasPendingUpload = hasPendingUpload;

            return View();
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult UniversityDashboard()
        {
            

            var uploadedColleges = (from upload in _context.UgPrintedUploads
                                    join college in _context.AffiliationCollegeMasters
                                    on EF.Functions.Collate(
                                           upload.CollegeCode,
                                           "SQL_Latin1_General_CP1_CI_AS")
                                    equals EF.Functions.Collate(
                                           college.CollegeCode,
                                           "SQL_Latin1_General_CP1_CI_AS")
                                    select new UploadedCollegeViewModel
                                    {
                                        CollegeCode = upload.CollegeCode,
                                        CollegeName = college.CollegeName
                                    })
                         .Distinct()
                         .OrderBy(c => c.CollegeName)
                         .ToList();

            return View(uploadedColleges);
        }
    }
}
