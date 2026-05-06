using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class MainDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;


        public MainDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class RguhsDashboardViewModel
        {
            public List<FacultyItem> Faculties { get; set; } = new();
            public List<CollegeItem> Colleges { get; set; } = new();
            public string ActiveFaculty { get; set; } = "ALL";
            public string SearchTerm { get; set; } = "";
            public int TotalColleges { get; set; }
            public int TotalFaculties { get; set; }
            public int TotalTowns { get; set; }
        }

        public class FacultyItem
        {
            public string FacultyId { get; set; }
            public string FacultyName { get; set; }
            public string Icon { get; set; }
            public int CollegeCount { get; set; }
        }

        public class CollegeItem
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string CollegeTown { get; set; }
            public string FacultyCode { get; set; }
            public string FacultyName { get; set; }
            public string FacultyIcon { get; set; }
        }


        // GET: College/GetData
        [HttpGet]
        public async Task<IActionResult> Rguhsdashboard(string faculty = "ALL", string search = "")
        {
            var vm = new RguhsDashboardViewModel
            {
                ActiveFaculty = faculty ?? "ALL",
                SearchTerm = search ?? ""
            };

            // ── 1. Load all faculties ─────────────────────────────────────
            var facultyEntities = await _context.Faculties
                .Where(f => f.FacultyId == 1)
                .OrderBy(f => f.FacultyId)
                .ToListAsync();

            var allColleges = await _context.AffiliationCollegeMasters
                .Where(e => e.FacultyCode == "1")
                .OrderBy(c => c.CollegeName)
                .ToListAsync();

            // ── 3. Build faculty list with per-faculty college counts ─────
            vm.Faculties = facultyEntities.Select(f =>
            {
                string id = f.FacultyId.ToString();
                return new FacultyItem
                {
                    FacultyId = id,
                    FacultyName = f.FacultyName ?? "Unknown",
                    Icon = GetFacultyIcon(id),
                    CollegeCount = allColleges.Count(c => c.FacultyCode.ToString() == id)
                };
            }).ToList();

            // ── 4. Apply faculty filter ───────────────────────────────────
            var filtered = allColleges.AsEnumerable();

            if (vm.ActiveFaculty != "ALL")
                filtered = filtered.Where(c => c.FacultyCode.ToString() == vm.ActiveFaculty);

            // ── 5. Apply search filter (case-insensitive) ─────────────────
            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
            {
                string q = vm.SearchTerm.Trim().ToLower();
                filtered = filtered.Where(c =>
                    (c.CollegeName ?? "").ToLower().Contains(q) ||
                    (c.CollegeTown ?? "").ToLower().Contains(q));
            }

            // ── 6. Build college list with faculty info attached ──────────
            var facultyLookup = vm.Faculties.ToDictionary(f => f.FacultyId);

            vm.Colleges = filtered.Select(c =>
            {
                string code = c.FacultyCode.ToString();
                facultyLookup.TryGetValue(code, out var fac);
                return new CollegeItem
                {
                    CollegeCode = c.CollegeCode ?? "",
                    CollegeName = c.CollegeName ?? "",
                    CollegeTown = c.CollegeTown ?? "",
                    FacultyCode = code,
                    FacultyName = fac?.FacultyName ?? "Unknown",
                    FacultyIcon = fac?.Icon ?? "🎓"
                };
            }).ToList();

            // ── 7. Stats ──────────────────────────────────────────────────
            vm.TotalColleges = allColleges.Count;
            vm.TotalFaculties = vm.Faculties.Count;
            vm.TotalTowns = allColleges
                                    .Select(c => c.CollegeTown)
                                    .Distinct()
                                    .Count();

            return View(vm);
        }

        private static string GetFacultyIcon(string id) => id switch
        {
            "1" => "🏥",
            "2" => "🦷",
            "3" => "💉",
            "4" => "⚗️",
            "5" => "🦾",
            "6" => "🔬",
            "7" => "🌿",
            "8" => "🧪",
            "9" => "🧘",
            _ => "🎓"
        };
        [HttpGet]
        public async Task<IActionResult> CollegeCourses(string collegeCode)
        {
            var courses = await _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode)
                .OrderBy(x => x.CourseName)
                .Select(x => new
                {
                    x.CourseName,
                    x.CourseCode,
                    x.ExistingIntake,
                    x.CollegeName,
                    x.FacultyCode
                })
                .ToListAsync();

            return Json(courses);
        }

        // Controllers/LoginController.cs
        public IActionResult MultiLogin()
        {
            // ✅ Clear all existing session data
            HttpContext.Session.Clear();

            // ✅ Generate new captcha after clearing session
            var captcha = new Random().Next(10000, 99999).ToString();
            HttpContext.Session.SetString("CaptchaCode", captcha);

            //var colleges = new[] { "M016", "M011", "M019", "Test1" };

            var model = new AdmissionLoginViewModel
            {
                Faculties = _context.Faculties
                .Where(e=>e.Status.ToLower()=="active")
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName,
                }).ToList(),

                Colleges = new List<SelectListItem>(),

                CaptchaCode = captcha
            };

            return View(model);
        }
    }
}
