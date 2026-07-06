using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class CourseAffiliationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseAffiliationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var typeOfAffiliation = HttpContext.Session.GetString("TypeOfAffiliation");
            // Allow ?courseLevel=UG (or PG/SS) or ?level=UG to override/set session value
            // and also accept a previously stored "SelectedLevel" session key.
            var queryCourseLevel = Request.Query["courseLevel"].ToString();
            if (string.IsNullOrWhiteSpace(queryCourseLevel))
            {
                // some redirects use 'level' as the query param
                queryCourseLevel = Request.Query["level"].ToString();
            }

            if (!string.IsNullOrWhiteSpace(queryCourseLevel))
            {
                HttpContext.Session.SetString("CourseLevel", queryCourseLevel.Trim());
            }

            // Primary session key is CourseLevel; fall back to SelectedLevel for compatibility
            var courseLevel = HttpContext.Session.GetString("CourseLevel")?.Trim();
            if (string.IsNullOrWhiteSpace(courseLevel))
            {
                var selected = HttpContext.Session.GetString("SelectedLevel");
                if (!string.IsNullOrWhiteSpace(selected))
                {
                    courseLevel = selected.Trim();
                    HttpContext.Session.SetString("CourseLevel", courseLevel);
                }
            }

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // Master course list for this faculty (we'll normalize course level in-memory to avoid EF null/casing issues)
            var masterCourses = await _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                var requestedLevel = courseLevel.Trim().ToUpperInvariant();
                masterCourses = masterCourses
                    .Where(c => !string.IsNullOrWhiteSpace(c.CourseLevel) && c.CourseLevel.Trim().ToUpperInvariant() == requestedLevel)
                    .ToList();
            }

            // Courses this college already has
            var offeredCourseCodes = await _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode.ToString() == facultyCode)
                .Select(x => x.CourseCode)
                .ToListAsync();

            // Courses the college has already requested (saved in AddCoursedetails)
            var requestedEntities = await _context.AddCoursedetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();
            var requestedCourseCodes = requestedEntities.Select(x => x.CourseCode).ToList();

            // Resolve college name — prefer intake table but fall back to college master if absent
            var collegeInfo = await _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode.ToString() == facultyCode)
                .Select(x => x.CollegeName)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(collegeInfo))
            {
                collegeInfo = await _context.AffiliationCollegeMasters
                    .Where(a => a.CollegeCode == collegeCode)
                    .Select(a => a.CollegeName)
                    .FirstOrDefaultAsync();
            }

            if (!string.IsNullOrWhiteSpace(collegeInfo))
            {
                HttpContext.Session.SetString("CollegeName", collegeInfo);
            }

            // Build the VM so that any previously requested courses show up in the main list
            // pre-checked and with their requested intake editable.
            var allCourses = masterCourses
                .OrderBy(c => c.CourseLevel)
                .ThenBy(c => c.CourseName)
                .Select(c =>
                {
                    var code = c.CourseCode.ToString();
                    var req = requestedEntities.FirstOrDefault(r => r.CourseCode == code);
                    return new CourseIntakeViewModel1
                    {
                        CourseCode = code,
                        CourseName = c.CourseName,
                        CourseLevel = c.CourseLevel,
                        IsAlreadyOffered = offeredCourseCodes.Contains(code),
                        AddCourseRequested = req != null && req.AddCourseRequested,
                        RequestedCourseIntake = req?.RequestedCourseIntake
                    };
                })
                .ToList();

            var vm = new CollegeCourseListViewModel1
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                TypeOfAffiliation = typeOfAffiliation,
                CollegeName = collegeInfo,
                AllCourses = allCourses,
                RequestedCourses = allCourses.Where(a => a.AddCourseRequested).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRequestedCourses(CollegeCourseListViewModel1 model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var typeOfAffiliation = HttpContext.Session.GetString("TypeOfAffiliation");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var requested = model.AllCourses
                .Where(c => !c.IsAlreadyOffered && c.AddCourseRequested)
                .ToList();

            foreach (var course in requested)
            {
                var existing = await _context.AddCoursedetails.FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseCode == course.CourseCode);

                if (existing == null)
                {
                    _context.AddCoursedetails.Add(new AddCoursedetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        TypeOfAffiliation = typeOfAffiliation,
                        CourseLevel = course.CourseLevel,
                        CourseCode = course.CourseCode,
                        AddCourseRequested = true,
                        RequestedCourseIntake = course.RequestedCourseIntake,
                        CreatedOn = DateTime.Now
                    });
                }
                else
                {
                    existing.RequestedCourseIntake = course.RequestedCourseIntake;
                    existing.AddCourseRequested = true;
                    existing.UpdatedOn = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Requested courses saved successfully.";
            return RedirectToAction("Institution_Details", "ContinuesAffiliation_Facultybased");
        }
    }
}