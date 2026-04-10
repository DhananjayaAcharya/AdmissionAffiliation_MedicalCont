using Medical_Affiliation.DATA;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace Medical_Affiliation.Controllers
{
    public class AffiliationDirController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AffiliationDirController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult AffiliationDirLogin()
        {
            //HttpContext.SignOutAsync("AdminAuth");
            //HttpContext.Session.Clear();
            //var viewModel = new AdminLoginViewModel();
            //viewModel.CaptchaString = GenerateCaptchaCode();
            //HttpContext.Session.SetString("CaptchaCode", viewModel.CaptchaString);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AffiliationDirLogin(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //model.CaptchaString = GenerateCaptchaCode();
                return View(model);
            }

            //var storedCaptcha = HttpContext.Session.GetString("CaptchaCode");
            //if (model.CaptchaInput != storedCaptcha)
            //{
            //    TempData["CaptchaError"] = "Invalid Captcha Entered.";
            //    return View(model);
            //}
            var user = await _context.TblRguhsFacultyUsers.FirstOrDefaultAsync(u => u.UserName == model.UserName);

            HttpContext.Session.SetString("logoutController", "AffiliationDir");
            HttpContext.Session.SetString("FacultyCode", user.Faculty.ToString());


            if (user == null)
            {
                ModelState.AddModelError("UserName", "The username you entered is incorrect.");
                //HttpContext.Session.Remove("CaptchaCode");
                return View(model);
            }

            var userIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            

            HttpContext.Session.SetString("FacultyId", "200");

            // Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("Password", "The password you entered is incorrect.");
                return View(model);
            }


            //var claims = new List<Claim>
            //    {
            //        new Claim(ClaimTypes.Name, user.UserName),
            //        new Claim(ClaimTypes.Role, "Admin"),
            //        new Claim("FacultyId", user.Faculty.ToString()),
            //        new Claim("UserIP", userIP ?? string.Empty),
            //        new Claim("UserAgent", userAgent ?? string.Empty)
            //    };

            //var identity = new ClaimsIdentity(claims, "AdminAuth");
            //var principal = new ClaimsPrincipal(identity);

            //await HttpContext.SignInAsync("AdminAuth", principal, new AuthenticationProperties
            //{
            //    IsPersistent = true,
            //    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            //});

            HttpContext.Session.SetString("FacultyId", user.Faculty.ToString());


            return RedirectToAction("AffiliationDashboard");
        }

        //[Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> AffiliationDashboard()
        {
            var facultyIdStr = HttpContext.Session.GetString("FacultyId");
            ViewBag.facultyId = facultyIdStr;

            int.TryParse(facultyIdStr, out int facultyId);

            // Controller & Dashboard redirection
            ViewBag.Controller = facultyId > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = facultyId > 98 ? "AdminDashboard" : "SODashboard";

            // ---------- FACULTIES ----------
            var facultiesQuery = _context.Faculties
                .Where(f => f.Status == "Active");


            var courseList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Select Course Level --", Disabled = true, Selected = true },
                    new SelectListItem { Value = "All", Text = "All Course Levels" }
                };
            if (facultyId <= 98)
            {
                facultiesQuery = facultiesQuery.Where(f => f.FacultyId == facultyId);
                var facultyCourses = await _context.MstCourses
                    .Where(c => c.FacultyCode == facultyId)
                    .Select(c => c.CourseLevel)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                foreach (var course in facultyCourses)
                {
                    courseList.Add(new SelectListItem
                    {
                        Value = course,
                        Text = course
                    });
                }

            }

            var activeFaculties = await facultiesQuery
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName,
                    Selected = false   // preselect current faculty
                })
                .ToListAsync();

            activeFaculties.Insert(0, new SelectListItem
            {
                Value = "All",
                Text = "All Faculties",
                Selected = false
            });

            // Add default option at top
            activeFaculties.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select Faculty --",
                Selected = true
            });

            var subjectList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Select Subject --", Disabled = true, Selected = true },
                    //new SelectListItem { Value = "All", Text = "All Subjects" }
                };
            

            // ---------- VIEWMODEL ----------
            var viewModel = new FacultyAndCollege
            {
                FacultyList = activeFaculties,
                CourseList = courseList,
                CollegeList = new List<SelectListItem>(),
                CoursesData = subjectList,
                IntakeList = new List<collegeWiseReportViewModel>()
            };

            return View(viewModel);

        }


        [HttpGet]
        public async Task<JsonResult> GetCollegesForFaculty(string facultyId)
        {
            // If "All", skip faculty filtering
            List<SelectListItem> colleges;
            if (facultyId == "All")
            {
                colleges = await _context.AffiliationCollegeMasters
                    .OrderBy(c => c.CollegeName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = $"{c.CollegeName} , {c.CollegeTown}"
                    })
                    .ToListAsync();
            }
            else
            {
                // Find the FacultyCode from the FacultyId (make sure types match)
                var facultyCode = await _context.Faculties
                    .Where(f => f.FacultyId.ToString() == facultyId && f.Status == "Active")
                    .Select(f => f.FacultyId.ToString())
                    .FirstOrDefaultAsync();

                colleges = await _context.AffiliationCollegeMasters
                    .Where(c => c.FacultyCode == facultyCode)
                    .OrderBy(c => c.CollegeName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = $"{c.CollegeName} , {c.CollegeTown}"
                    })
                    .ToListAsync();
            }

            // Insert "All" + default at top
            //colleges.Insert(0, new SelectListItem
            //{
            //    Value = "All",
            //    Text = "All Colleges"
            //});
            //colleges.Insert(0, new SelectListItem
            //{
            //    Value = "",
            //    Text = "-- Select College --",
            //    Selected = true
            //});

            return Json(colleges);
        }


        [HttpGet]
        public async Task<JsonResult> GetCoursesForCollege(string collegeId)
        {
            List<SelectListItem> courseLevels;

            if (collegeId == "All")
            {
                // If "All Colleges" selected, include ALL course levels
                courseLevels = await _context.MstCourses
                    .Select(c => c.CourseLevel)
                    .Distinct()
                    .OrderBy(level => level)
                    .Select(level => new SelectListItem
                    {
                        Value = level,
                        Text = level
                    })
                    .ToListAsync();
            }
            else
            {
                var courseCodes = await _context.CollegeCourseIntakeDetails
                    .Where(x => x.CollegeCode == collegeId)
                    .Select(x => x.CourseCode)
                    .Distinct()
                    .ToListAsync();

                courseLevels = await _context.MstCourses
                    .Where(c => courseCodes.Contains(c.CourseCode.ToString()))
                    .Select(c => c.CourseLevel)
                    .Distinct()
                    .OrderBy(level => level)
                    .Select(level => new SelectListItem
                    {
                        Value = level,
                        Text = level
                    })
                    .ToListAsync();
            }

            return Json(courseLevels);
        }

        public async Task<JsonResult> GetSubjectsForCourses(string courseId, string facultyId, string CollegeCode)
        {
            var subjectList = new List<SelectListItem>();

            // Base query
            var query =
                from c in _context.MstCourses
                join icd in _context.CollegeCourseIntakeDetails
                    on c.CourseCode.ToString() equals icd.CourseCode
                select new { c, icd };

            // Filter by Faculty
            if (!string.IsNullOrEmpty(facultyId) && facultyId != "All")
            {
                query = query.Where(x => x.c.FacultyCode.ToString() == facultyId);
            }

            // Filter by College
            if (!string.IsNullOrEmpty(CollegeCode) && CollegeCode != "All")
            {
                query = query.Where(x => x.icd.CollegeCode == CollegeCode);
            }

            // Filter by Course — BUT ONLY IF courseId is not "All"
            if (!string.IsNullOrEmpty(courseId) && courseId != "All")
            {
                query = query.Where(x => x.c.CourseLevel == courseId);
            }

            // Get distinct subjects
            var subjects = await query
                .Where(x => !string.IsNullOrEmpty(x.c.SubjectName))
                .Select(x => new { x.c.CourseCode, x.c.SubjectName })
                .Distinct()
                .OrderBy(x => x.SubjectName)
                .ToListAsync();

            // OPTIONAL: Add “All Subjects” if more than one exists
            //if (subjects.Count > 1)
            //{
            //    subjectList.Add(new SelectListItem { Value = "All", Text = "All Subjects" });
            //}

            foreach (var s in subjects)
            {
                subjectList.Add(new SelectListItem
                {
                    Value = s.CourseCode.ToString(),   // course code as value
                    Text = s.SubjectName               // subject text for display
                });
            }

            return Json(subjectList);
        }


        [HttpGet]
        public async Task<JsonResult> GetIntakeDetails(string facultyId, string collegeId, string courseLevel, string subject)
        {
            var intakeDetails = await (
                from intake in _context.CollegeCourseIntakeDetails
                join course in _context.MstCourses
                    on intake.CourseCode equals course.CourseCode.ToString()
                where
                    (facultyId == "All" || string.IsNullOrEmpty(facultyId) || intake.FacultyCode.ToString() == facultyId)
                    && (collegeId == "All" || string.IsNullOrEmpty(collegeId) || intake.CollegeCode == collegeId)
                    && (
                         (string.IsNullOrEmpty(courseLevel) || courseLevel == "All")
                            || course.CourseLevel == courseLevel
                       )
                    && (
                         (string.IsNullOrEmpty(subject) || subject == "All")
                            || course.CourseCode.ToString() == subject
                       )
                select new collegeWiseReportViewModel
                {
                    CollegeName = intake.CollegeName,
                    courseName = course.CourseName,
                    RGUSHSintake = intake.ExistingIntake,
                    Presentintake = intake.PresentIntake,
                    PrincipalName = intake.CollegeAddress,
                    subjectName = course.SubjectName,
                    CollegeCode = intake.CollegeCode,
                    SubjectCode = course.CourseCode.ToString()

                }
            )
            .OrderBy(c => c.CollegeName)
            .ThenBy(c => c.courseName)
            .ToListAsync();

            var totalColleges = intakeDetails
            .Select(x => x.CollegeCode)
            .Distinct()
            .Count();

            var result = new { totalColleges, intakeDetails };
            return Json(result);
        }

        public async Task<JsonResult> GetStats(int facultyId, string collegeId, string courseLevel)
        {
            var stats = new
            {
                Faculties = facultyId == 0 || facultyId.ToString() == "All"
                    ? await _context.Faculties.CountAsync()
                    : 1,

                Colleges = string.IsNullOrEmpty(collegeId) || collegeId == "All"
                    ? await _context.AffiliationCollegeMasters
                        .Where(c => facultyId == 0 || facultyId == 98 || c.FacultyCode == facultyId.ToString())
                        .CountAsync()
                    : 1,

                Courses = string.IsNullOrEmpty(courseLevel) || courseLevel == "All"
                    ? await _context.MstCourses
                        .Where(c => facultyId == 0 || c.FacultyCode == facultyId)
                        .CountAsync()
                    : await _context.MstCourses
                        .Where(c => c.CourseLevel == courseLevel)
                        .CountAsync(),

                Subjects = await _context.MstCourses
                    .Where(c =>
                        (facultyId == 0 || facultyId == 98 || c.FacultyCode == facultyId) &&
                        (string.IsNullOrEmpty(collegeId) || collegeId == "All" ||
                            _context.CollegeCourseIntakeDetails.Any(icd =>
                                icd.CollegeCode == collegeId && icd.CourseCode == c.CourseCode.ToString())) &&
                        (string.IsNullOrEmpty(courseLevel) || courseLevel == "All" || c.CourseLevel == courseLevel)
                    )
                    .Select(c => c.SubjectName)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .CountAsync()
            };

            return Json(stats);
        }


        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("AffiliationDirLogin", "AffiliationDir");
        }
    }
}
