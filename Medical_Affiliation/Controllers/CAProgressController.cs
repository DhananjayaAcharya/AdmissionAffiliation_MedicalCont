using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    [Authorize]
    public class CAProgressController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CAProgressController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? facultyCode,
            string? collegeCode,
            string? courseLevel = "UG",
            int typeId = 2)
        {
            // --------------------------------------------------------
            // Default values
            // --------------------------------------------------------

            courseLevel = string.IsNullOrWhiteSpace(courseLevel)
                ? "UG"
                : courseLevel.Trim().ToUpper();

            // --------------------------------------------------------
            // Faculty Dropdown
            // --------------------------------------------------------

            var faculties = await _context.Faculties
                .Where(x => x.Status == "Active")
                .OrderBy(x => x.FacultyName)
                .Select(x => new
                {
                    x.FacultyId,
                    x.FacultyName
                })
                .ToListAsync();

            ViewBag.Faculties = faculties
                .Select(x => new SelectListItem
                {
                    Value = x.FacultyId.ToString(),
                    Text = x.FacultyName,
                    Selected = x.FacultyId.ToString() == facultyCode
                })
                .ToList();

            // --------------------------------------------------------
            // College Dropdown
            // --------------------------------------------------------

            var collegesQuery = _context.AffiliationCollegeMasters
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(facultyCode))
            {
                collegesQuery = collegesQuery
                    .Where(x => x.FacultyCode == facultyCode);
            }

            var colleges = await collegesQuery
                .OrderBy(x => x.CollegeName)
                .Select(x => new
                {
                    x.CollegeCode,
                    x.CollegeName
                })
                .Distinct()
                .ToListAsync();

            ViewBag.Colleges = colleges
                .Select(x => new SelectListItem
                {
                    Value = x.CollegeCode,
                    Text = x.CollegeName,
                    Selected = x.CollegeCode == collegeCode
                })
                .ToList();

            // --------------------------------------------------------
            // Course Level Dropdown
            // --------------------------------------------------------

            var courseLevels = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "UG",
                    Value = "UG",
                    Selected = courseLevel == "UG"
                },
                new SelectListItem
                {
                    Text = "PG",
                    Value = "PG",
                    Selected = courseLevel == "PG"
                },
                new SelectListItem
                {
                    Text = "SS",
                    Value = "SS",
                    Selected = courseLevel == "SS"
                }
            };

            ViewBag.CourseLevels = courseLevels;

            // --------------------------------------------------------
            // Type of Affiliation Dropdown
            // --------------------------------------------------------

            var affiliationTypes = await _context.TypeOfAffiliations
                .OrderBy(x => x.TypeDescription)
                .Select(x => new
                {
                    x.TypeId,
                    x.TypeDescription
                })
                .ToListAsync();

            ViewBag.AffiliationTypes = affiliationTypes
                .Select(x => new SelectListItem
                {
                    Value = x.TypeId.ToString(),
                    Text = x.TypeDescription,
                    Selected = x.TypeId == typeId
                })
                .ToList();

            // --------------------------------------------------------
            // CA Progress
            // --------------------------------------------------------

            var progressQuery = _context.CaProgresses
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(collegeCode))
            {
                progressQuery = progressQuery
                    .Where(x => x.CollegeCode == collegeCode);
            }

            progressQuery = progressQuery
                .Where(x => x.CourseLevel == courseLevel);

            var progress = await progressQuery
                .OrderBy(x => x.Id)
                .Select(x => new CAProgressViewModel
                {
                    Id = x.Id,
                    CollegeCode = x.CollegeCode,
                    CourseLevel = x.CourseLevel,
                    StepKey = x.StepKey,
                    IsCompleted = x.IsCompleted ?? false,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return View(progress);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkToggleStatus( int FacultyId, string CollegeCode, string CourseLevel,  string StepKey, bool IsCompleted)
        {
            string facultyCode = FacultyId.ToString();

            var query =
                from progress in _context.CaProgresses

                join college in _context.AffiliationCollegeMasters
                    on progress.CollegeCode equals college.CollegeCode

                where college.FacultyCode == facultyCode
                      && progress.CourseLevel == CourseLevel
                      && progress.StepKey == StepKey

                select progress;

            // Particular college
            if (CollegeCode != "ALL")
            {
                query = query.Where(x =>
                    x.CollegeCode == CollegeCode);
            }

            var records = query.ToList();

            if (!records.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "No matching CA progress records found."
                });
            }

            foreach (var record in records)
            {
                record.IsCompleted = IsCompleted;
                record.UpdatedAt = DateTime.Now;
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isCompleted = IsCompleted,
                updatedCount = records.Count,

                totalColleges = records
                    .Select(x => x.CollegeCode)
                    .Distinct()
                    .Count()
            });
        }

        [HttpGet]
        public IActionResult GetBulkTabStatus(int facultyId, string collegeCode, string courseLevel, string stepKey)
        {
            string facultyCode = facultyId.ToString();

            var query =
                from progress in _context.CaProgresses

                join college in _context.AffiliationCollegeMasters
                    on progress.CollegeCode equals college.CollegeCode

                where college.FacultyCode == facultyCode
                      && progress.CourseLevel == courseLevel
                      && progress.StepKey == stepKey

                select progress;

            // Particular college
            if (collegeCode != "ALL")
            {
                query = query.Where(x =>
                    x.CollegeCode == collegeCode);
            }

            var records = query.ToList();

            if (!records.Any())
            {
                return Json(new
                {
                    isCompleted = false,
                    totalColleges = 0,
                    completedColleges = 0
                });
            }

            var totalColleges = records
                .Select(x => x.CollegeCode)
                .Distinct()
                .Count();

            var completedColleges = records
                .Where(x => x.IsCompleted == true)
                .Select(x => x.CollegeCode)
                .Distinct()
                .Count();

            var isCompleted =
                completedColleges == totalColleges;

            return Json(new
            {
                isCompleted,
                totalColleges,
                completedColleges
            });
        }

        [HttpGet]
        public IActionResult GetCATabs( int facultyId,  string courseLevel)
        {
            string facultyCode = facultyId.ToString();

            var tabs =
                (
                    from progress in _context.CaProgresses

                    join college in _context.AffiliationCollegeMasters
                        on progress.CollegeCode equals college.CollegeCode

                    where college.FacultyCode == facultyCode
                          && progress.CourseLevel == courseLevel

                    select new
                    {
                        stepKey = progress.StepKey,
                        tabName = progress.StepKey
                    }
                )
                .Distinct()
                .OrderBy(x => x.tabName)
                .ToList();

            return Json(tabs);
        }

        // ============================================================
        // TOGGLE STATUS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(ToggleCAProgressViewModel model)
        {
            if (model.Id <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid progress record."
                });
            }

            var progress = await _context.CaProgresses
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (progress == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Progress record not found."
                });
            }

            // Set exactly what the checkbox is sending
            progress.IsCompleted = model.IsCompleted;
            progress.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isCompleted = progress.IsCompleted,
                updatedAt = progress.UpdatedAt?.ToString("dd-MM-yyyy HH:mm"),
                message = progress.IsCompleted == true
                    ? "Page marked as completed."
                    : "Page marked as not completed."
            });
        }


        // ============================================================
        // GET COLLEGES BY FACULTY
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetCollegesByFaculty(
            string facultyCode)
        {
            if (string.IsNullOrWhiteSpace(facultyCode))
            {
                return Json(new List<object>());
            }

            var colleges = await _context.AffiliationCollegeMasters
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.CollegeName)
                .Select(x => new
                {
                    collegeCode = x.CollegeCode,
                    collegeName = x.CollegeName
                })
                .Distinct()
                .ToListAsync();

            return Json(colleges);
        }
    }
}