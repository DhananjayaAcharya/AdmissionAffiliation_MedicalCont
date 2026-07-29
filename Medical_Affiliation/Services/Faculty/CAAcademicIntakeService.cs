using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAAcademicIntakeService : ICAAcademicIntakeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAAcademicIntakeService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<AcademicIntakePreviewViewModel> GetAcademicIntakePreviewAsync()
        {
            var facultyId = _userContext.FacultyId;
            var facultyCode = facultyId.ToString();
            var collegeCode = _userContext.CollegeCode;

            var collegeName = await _context.AffiliationCollegeMasters
                .Where(x => x.CollegeCode == collegeCode)
                .Select(x => x.CollegeName)
                .FirstOrDefaultAsync();

            var model = new AcademicIntakePreviewViewModel
            {
                FacultyId = facultyId,
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CollegeName = collegeName
            };

            var allCourses = await _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)
                .ToListAsync();

            var academicIntakes = await _context.AcademicIntakes
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode &&
                            x.CollegeCode == collegeCode)
                .ToListAsync();

            List<IntakeByLevelViewModel1> GetCoursesByLevel(string level)
            {
                return (from intake in academicIntakes
                        where int.TryParse(intake.Courses, out _)
                        let courseCode = int.Parse(intake.Courses!)
                        join course in allCourses
                            on courseCode equals course.CourseCode
                        where course.CourseLevel == level
                        select new IntakeByLevelViewModel1
                        {
                            CourseCode = course.CourseCode.ToString(),
                            CourseName = course.CourseName,

                            AY2024_ExistingIntake = intake.Ay2024ExistingIntake,
                            AY2024_IncreaseIntake = intake.Ay2024IncreaseIntake,
                            AY2024_TotalIntake = intake.Ay2024TotalIntake,

                            AY2025_ExistingIntake = intake.Ay2025ExistingIntake,
                            AY2025_LopNmcIntake = intake.Ay2025LopNmcIntake,
                            AY2025_TotalIntake = intake.Ay2025TotalIntake,
                            AY2025_LopDate = intake.Ay2025LopDate,

                            AY2026_ExistingIntake = intake.Ay2026ExistingIntake,
                            AY2026_AddRequestedIntake = intake.Ay2026AddRequestedIntake,
                            AY2026_TotalIntake = intake.Ay2026TotalIntake,

                            AY2027_ExistingIntake = intake.Ay2027ExistingIntake,
                            AY2027_AddRequestedIntake = intake.Ay2027AddRequestedIntake,
                            AY2027_TotalIntake = intake.Ay2027TotalIntake,

                            HasNmcDocument = intake.Ay2025NmcDocument != null,
                            HasLopDocument = intake.Ay2025LopDocument != null,

                            HasAY2025DciDocument = !string.IsNullOrEmpty(intake.Ay2025Dcidocument),
                            HasAY2025KsdcDocument = !string.IsNullOrEmpty(intake.Ay2025Ksdcdocument),

                            HasAY2026DciDocument = !string.IsNullOrEmpty(intake.Ay2026Dcidocument),
                            HasAY2026KsdcDocument = !string.IsNullOrEmpty(intake.Ay2026Ksdcdocument),

                            HasAY2027DciDocument = !string.IsNullOrEmpty(intake.Ay2027Dcidocument),
                            HasAY2027KsdcDocument = !string.IsNullOrEmpty(intake.Ay2027Ksdcdocument)
                        })
                        .OrderBy(x => x.CourseName)
                        .ToList();
            }

            model.UgCourses = GetCoursesByLevel("UG");
            model.PgCourses = GetCoursesByLevel("PG");
            model.SsCourses = GetCoursesByLevel("SS");

            return model;
        }
    }
}
