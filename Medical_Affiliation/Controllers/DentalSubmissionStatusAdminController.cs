
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class DentalSubmissionStatusAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DentalSubmissionStatusAdminController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        private static bool GetEffectiveLockStatus( FinalDentalSubmission submission)
        {
            // Apply locking only to active submissions.
            if (!submission.IsActive)
                return false;

            // Active submissions are locked unless explicitly unlocked.
            return submission.IsLocked;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? courseLevel,
            string? collegeCode)
        {
            var model = new DentalSubmissionStatusAdminViewModel
            {
                SelectedCourseLevel = courseLevel,
                SelectedCollegeCode = collegeCode
            };

            // 1. Course levels available in Dental submissions.
            model.CourseLevels = await _context.FinalDentalSubmissions
                .AsNoTracking()
                .Where(x => x.FacultyCode == 2)
                .Select(x => x.CourseLevel)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x
                })
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(courseLevel) &&
                !model.CourseLevels.Any(x => x.Value == courseLevel))
            {
                return BadRequest("Invalid course level.");
            }

            // 2. Load Dental colleges only.
            var collegesQuery = _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(x => x.FacultyCode == "2");

            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                collegesQuery = collegesQuery.Where(c =>
                    _context.FinalDentalSubmissions.Any(s =>
                        s.CollegeCode == c.CollegeCode &&
                        s.FacultyCode == 2 &&
                        s.CourseLevel == courseLevel));
            }

            model.Colleges = await collegesQuery
                .OrderBy(x => x.CollegeName)
                .Select(x => new SelectListItem
                {
                    Value = x.CollegeCode,
                    Text = x.CollegeName ?? x.CollegeCode
                })
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(collegeCode) &&
                !model.Colleges.Any(x => x.Value == collegeCode))
            {
                return BadRequest("Invalid Dental college.");
            }

            // 3. Load matching Dental submissions.
            var submissionsQuery = _context.FinalDentalSubmissions
                .AsNoTracking()
                .Include(x => x.CollegeCodeNavigation)
                .Include(x => x.AffiliationType)
                .Where(x => x.FacultyCode == 2);

            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                submissionsQuery = submissionsQuery.Where(
                    x => x.CourseLevel == courseLevel);
            }

            if (!string.IsNullOrWhiteSpace(collegeCode))
            {
                submissionsQuery = submissionsQuery.Where(
                    x => x.CollegeCode == collegeCode);
            }

            var records = await submissionsQuery
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            model.Submissions = records
                .Select(x => new DentalSubmissionStatusAdminRowViewModel
                {
                    Id = x.Id,
                    CollegeCode = x.CollegeCode,
                    CollegeName = x.CollegeCodeNavigation.CollegeName,
                    CourseLevel = x.CourseLevel,
                    AcademicYear = x.AcademicYear,
                    AffiliationTypeId = x.AffiliationTypeId,
                    ApplicationNumber = x.ApplicationNumber,
                    SubmittedDate = x.SubmittedDate,
                    IsActive = x.IsActive,
                    IsLocked = GetEffectiveLockStatus(x)
                })
                .ToList();

            return View(
                "~/Views/DentalSubmissionStatusAdmin/Index.cshtml",
                model);
        }

        // Toggle between locked and unlocked.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(
            int id,
            string? courseLevel,
            string? collegeCode)
        {
            var submission = await _context.FinalDentalSubmissions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.FacultyCode == 2);

            if (submission == null)
                return NotFound();

            if (!submission.IsActive)
            {
                TempData["ErrorMessage"] =
                    "Lock management is available only for active submissions.";

                return RedirectToAction(nameof(Index), new
                {
                    courseLevel,
                    collegeCode
                });
            }

            // Toggle the lock state.
            submission.IsLocked = !submission.IsLocked;
            submission.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = submission.IsLocked
                ? "Dental submission locked successfully."
                : "Dental submission unlocked successfully.";

            return RedirectToAction(nameof(Index), new
            {
                courseLevel,
                collegeCode
            });
        }
    }
}
