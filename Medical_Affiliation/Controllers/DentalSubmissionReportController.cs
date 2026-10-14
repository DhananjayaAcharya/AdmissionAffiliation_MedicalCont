using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class DentalSubmissionReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public DentalSubmissionReportController( ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(  string? academicYear,  string? courseLevel, int? affiliationTypeId)
    {

        var excludedCollegeCodes = new List<string>
        {
            //"D038",
            "D039",
            "D040"
        };

        // Base query for Dental submissions
        var collegesQuery = _context.AffiliationCollegeMasters
            .AsNoTracking()
            .Where(c => c.FacultyCode == "2" && !excludedCollegeCodes.Contains(c.CollegeCode));

        var submissionQuery = _context.FinalDentalSubmissions
            .AsNoTracking()
            .Where(c => c.FacultyCode == 2);



        if (!string.IsNullOrWhiteSpace(academicYear))
        {
            submissionQuery = submissionQuery.Where(
                x => x.AcademicYear == academicYear);
        }

        if (!string.IsNullOrWhiteSpace(courseLevel))
        {
            submissionQuery = submissionQuery.Where(
                x => x.CourseLevel == courseLevel);
        }

        if (affiliationTypeId.HasValue)
        {
            submissionQuery = submissionQuery.Where(
                x => x.AffiliationTypeId == affiliationTypeId.Value);
        }

        // Count submitted colleges matching the filters
        var submissions = await collegesQuery
            .Select(c => new DentalSubmissionReportRowViewModel
            {
                CollegeCode = c.CollegeCode,
                CollegeName = c.CollegeName,
                ApplicationNumber = submissionQuery
                    .Where(s => s.CollegeCode == c.CollegeCode)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .Select(s => s.ApplicationNumber)
                    .FirstOrDefault(),

                AcademicYear = submissionQuery
                    .Where(s => s.CollegeCode == c.CollegeCode)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .Select(s => s.AcademicYear)
                    .FirstOrDefault(),

                CourseLevel = submissionQuery
                    .Where(s => s.CollegeCode == c.CollegeCode)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .Select(s => s.CourseLevel)
                    .FirstOrDefault(),

                AffiliationType = submissionQuery
                    .Where(s => s.CollegeCode == c.CollegeCode)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .Select(s => s.AffiliationType.TypeDescription)
                    .FirstOrDefault(),

                PrincipalConsent = submissionQuery
                    .Where(s => s.CollegeCode == c.CollegeCode)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .Select(s => (bool?)s.PrincipalConsent)
                    .FirstOrDefault(),

                SubmittedDate = submissionQuery
                    .Where(s => s.CollegeCode == c.CollegeCode)
                    .OrderByDescending(s => s.SubmittedDate)
                    .ThenByDescending(s => s.CreatedDate)
                    .Select(s => s.SubmittedDate)
                    .FirstOrDefault(),
            })
            .OrderBy(c => c.CollegeName)
            .ToListAsync();

        

        // Academic Year dropdown
        var academicYears = await _context.FinalDentalSubmissions
            .AsNoTracking()
            .Where(x => x.FacultyCode == 2)
            .Select(x => x.AcademicYear)
            .Distinct()
            .OrderByDescending(x => x)
            .ToListAsync();

        // Course Level dropdown
        var courseLevels = await _context.FinalDentalSubmissions
            .AsNoTracking()
            .Where(x => x.FacultyCode == 2)
            .Select(x => x.CourseLevel)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        // Affiliation Type dropdown
        var affiliationTypes = await _context.TypeOfAffiliations
            .AsNoTracking()
            .ToListAsync();

        var model = new DentalFinalSubmissionReportViewModel
        {
            AcademicYear = academicYear,
            CourseLevel = courseLevel,
            AffiliationTypeId = affiliationTypeId,

            TotalColleges = submissions.Count,
            TotalSubmissions = submissions.Count(s => s.IsSubmitted),

            Submissions = submissions,

            AcademicYears = academicYears
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x,
                    Selected = x == academicYear
                }).ToList(),

            CourseLevels = courseLevels
                .Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x,
                    Selected = x == courseLevel
                }).ToList(),

            AffiliationTypes = affiliationTypes
                .Select(x => new SelectListItem
                {
                    Value = x.TypeId.ToString(),
                    Text = x.TypeDescription,
                    Selected = x.TypeId.ToString() == affiliationTypeId.ToString()
                }).ToList(),
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var submission = await _context.FinalDentalSubmissions
            .AsNoTracking()
            .Include(x => x.CollegeCodeNavigation)
            .Include(x => x.AffiliationType)
            .Include(x => x.FacultyCodeNavigation)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.FacultyCode == 2);

        if (submission == null)
            return NotFound();

        return View(submission);
    }
}