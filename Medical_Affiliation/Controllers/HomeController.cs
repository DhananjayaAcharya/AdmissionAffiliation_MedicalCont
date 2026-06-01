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
        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
                                    on upload.CollegeCode equals college.CollegeCode

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



        public async Task<IActionResult> AllFacultyList()
        {

            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (!string.IsNullOrEmpty(collegeCode))
            {
                // 1. Find college details by code
                var college = await _context.AffiliationCollegeMasters
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

                // 2. Set session so the JS LoadFaculty() knows which college to fetch
                //HttpContext.Session.SetString("CollegeCode", collegeCode);
                //AHttpContext.Session.SetString("CollegeName", college?.CollegeName ?? "Unknown");
            }

            // 3. Pass the full list of colleges to the ViewBag for the dropdown
            ViewBag.Colleges = await _context.AffiliationCollegeMasters.ToListAsync();

            return View();
        }

        
        [HttpGet]
        [Route("GetAllFacultyReport")]
        public async Task<IActionResult> GetAllFacultyReport()
        {
            try
            {
                var departments = await _context.DepartmentMastersForUgs
                                .AsNoTracking()
                                .ToListAsync();

                var designations = await _context.UgdesignationMasters
                    .AsNoTracking()
                    .ToListAsync();

                var colleges = await _context.AffiliationCollegeMasters
                    .AsNoTracking()
                    .ToListAsync();

                var faculties = await _context.UgFacultyDetails
                    .Where(f => f.IsDeclared==true )
                    .AsNoTracking()
                    .ToListAsync();

                var rawData = (from faculty in faculties
                               join col in colleges
                                   on faculty.CollegeCode equals col.CollegeCode into colJoin
                               from col in colJoin.DefaultIfEmpty()

                               join dept in departments
                                   on faculty.DepartmentCode equals dept.DepartmentCode into deptJoin
                               from dept in deptJoin.DefaultIfEmpty()

                               join desig in designations
                                   on faculty.DesignationCode equals desig.DesignationId into desigJoin
                               from desig in desigJoin.DefaultIfEmpty()

                               select new
                               {
                                   Id = faculty.Id,
                                   CollegeName = col?.CollegeName,
                                   DepartmentName = dept?.DepartmentName,
                                   DesignationName = desig?.DesignationName,
                                   NameOftheFaculty = faculty.NameOftheFaculty,
                                   MobileNo = faculty.MobileNo,
                                   PanNo = faculty.Panno,

                                   DOB = faculty.Dob,
                                   DateOfAppointment = faculty.DateOfAppointment,
                                   StateCouncilRegNo = faculty.StateCouncilRegNo,
                                   AEBASAttendId = faculty.AebasattendId,
                                   ProfessionalQualification = faculty.ProfessionalQualification,

                                   NatureOfEmployment = faculty.NatureOfEmployment,
                                   TeachingExpInYrs = faculty.TeachingExpInYrs,
                                   PhotoFilePath = faculty.PhotoFilePath
                               })
               .GroupBy(x => x.Id)
               .Select(g => g.First())
               .ToList();

                return Json(new { success = true, data = rawData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> DesignationWiseFacultyCount()
        {
            ViewBag.Colleges = await (
                        from f in _context.UgFacultyDetails
                        join c in _context.AffiliationCollegeMasters
                            on f.CollegeCode equals c.CollegeCode
                        where f.IsDeclared == true
                        group c by new
                        {
                            c.CollegeCode,
                            c.CollegeName
                        }
                        into g
                        orderby g.Key.CollegeName
                        select new
                        {
                            CollegeCode = g.Key.CollegeCode,
                            CollegeName = g.Key.CollegeName
                        }
                    ).ToListAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDesignationWiseCount(string collegeCode)
        {
            try
            {
                var result =
                    from d in _context.UgdesignationMasters

                    join f in _context.UgFacultyDetails
                            .Where(x =>
                                x.IsDeclared == true &&
                                x.CollegeCode == collegeCode)
                    on d.DesignationId equals f.DesignationCode
                    into facultyGroup

                    orderby d.Order

                    select new
                    {
                        d.DesignationId,
                        d.DesignationName,
                        Count = facultyGroup.Count()
                    };

                return Json(new
                {
                    success = true,
                    data = await result.ToListAsync()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
