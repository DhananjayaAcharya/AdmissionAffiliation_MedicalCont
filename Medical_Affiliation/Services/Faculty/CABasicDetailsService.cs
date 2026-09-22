using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Medical_Affiliation.Services.Faculty
{
    public class CABasicDetailsService : ICAInstitutionBasicDetails
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CABasicDetailsService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<InstituionBasicDetailsDisplayVM> GetAllDetails()
        {
            var facultyId = _userContext.FacultyId;
            var collegeCode = _userContext.CollegeCode;
            var trustDetails = await GetTrustMembers();
            var courseIntake = await GetSanctionedIntakeDetails();
            var CourseDetails = await GetAffCourseDetails();
            var ugCourseDetails = await GetAffiliationCourseDetails();
            var deanOrDeanDetails = await GetDeanOrDirectorDetails();
            var principalDetails = await GetPrincipalDetails();


            return new InstituionBasicDetailsDisplayVM
            {
                TrustMemberVM = trustDetails,
                IntakeForCourseVM = courseIntake,
                AffCoursesVM = CourseDetails,
                AffiliationCourseDetailVM = ugCourseDetails,
                DeanOrDirectorDetailDisplayVM = deanOrDeanDetails,
                PrincipalDetailDisplayVM = principalDetails
            };
        }

        public async Task<ContinuationTrustMemberListDisplayViewModel> GetTrustMembers()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var members = await _context.ContinuationTrustMemberDetails
                .Where(m => m.CollegeCode == collegeCode && m.FacultyCode == facultyCode.ToString())
                .Select(m => new ContinuationTrustMemberDisplayViewModel
                {
                    SlNo = m.SlNo,
                    TrustMemberName = m.TrustMemberName,
                    Faculty = m.FacultyCode,
                    CollegeCode = collegeCode,
                    Designation = m.Designation,
                    Qualification = m.Qualification,
                    MobileNumber = m.MobileNumber,
                    Age = m.Age,
                    JoiningDate = m.JoiningDate,
                    DesignationId = m.DesignationId
                })
                .ToListAsync();


            return new ContinuationTrustMemberListDisplayViewModel
            {
                Items = members,
                
            };
        }

        public async Task<AffSanctionedIntakeForCourseListDisplayViewModel> GetSanctionedIntakeDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var rawData = await _context.AffSanctionedIntakeForCourses
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
                .Select(e => new
                {
                    e.CourseName,
                    e.SanctionedIntake,
                    e.EligibleSeatSlab,
                    HasDocument = e.DocumentData != null
                })
                .ToListAsync(); // 👈 SQL stops here

            var data = rawData
                .GroupBy(e => e.CourseName)
                .Select(g => g.First())
                .Select(e => new AffSanctionedIntakeForCourseDisplayViewModel
                {
                    CourseName = e.CourseName,
                    SanctionedIntake = e.SanctionedIntake,
                    EligibleSeatSlab = e.EligibleSeatSlab,
                    HasDocument = e.HasDocument
                })
                .ToList();

            return new AffSanctionedIntakeForCourseListDisplayViewModel
            {
                Items = data
            };
        }

        public async Task<AffCourseDisplayVM> GetAffCourseDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var items = await _context.AffCourseDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString() && x.IsRecognized)
                .OrderBy(x => x.CourseName)
                .Select(x => new AffCourseDisplayItemVM
                {
                    CourseName = x.CourseName,
                    IsRecognized = x.IsRecognized,
                    RguhsNotificationNo = x.RguhsNotificationNo,
                    HasDocument = x.DocumentData != null
                })
                .ToListAsync();

            return new AffCourseDisplayVM
            {
                Items = items
            };
        }

        public async Task<AffiliationCourseDetailDisplayVM?> GetAffiliationCourseDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.AffiliationCourseDetails
                .Where(x => x.Collegecode == collegeCode && x.Facultycode == facultyCode.ToString())
                .OrderBy(x => x.CourseName)
                .Select(x => new AffiliationCourseDetailDisplayVM
                {
                    CourseName = x.CourseName,
                    IntakeDuring202526 = x.IntakeDuring202526,
                    IntakeSlab = x.IntakeSlab,
                    TypeofPermission = x.Typeofpermission,
                    YearOfLop = x.YearofLop.HasValue ? x.YearofLop.Value.Year.ToString() : null,
                    DateOfRecognition = x.Dateofrecognition,
                    YearOfObtainingEcAndFc = x.YearofObtainingEcandFc.HasValue ? x.YearofObtainingEcandFc.Value.Year.ToString() : null,
                    SanctionedIntakeEcFc = x.SannctionedIntakeEcFc,
                    HasGokOrder = x.GokorderPath != null
                })
                .FirstOrDefaultAsync();

            return data;
        }

        public async Task<AffDeanOrDirectorDetailDisplayVM?> GetDeanOrDirectorDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var entity = await _context.AffDeanOrDirectorDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .FirstOrDefaultAsync();

            if (entity == null)
                return null;

            return new AffDeanOrDirectorDetailDisplayVM
            {
                DeanOrDirectorName = entity.DeanOrDirectorName,
                DeanQualification = await ResolveQualificationNameAsync(entity.DeanQualification),
                DeanQualificationDate = entity.DeanQualificationDate.HasValue
                    ? entity.DeanQualificationDate.Value.ToString("dd-MM-yyyy")
                    : "—",
                DeanUniversity = entity.DeanUniversity,
                DeanStateCouncilNumber = entity.DeanStateCouncilNumber,
                RecognizedByMci = entity.RecognizedByMci == true ? "Yes" : "No"
            };
        }

        public async Task<AffPrincipalDetailDisplayVM?> GetPrincipalDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var entity = await _context.AffPrincipalDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .FirstOrDefaultAsync();

            if (entity == null)
                return null;

            return new AffPrincipalDetailDisplayVM
            {
                PrincipalName = entity.DeanOrDirectorName,
                PrincipalQualification = await ResolveQualificationNameAsync(entity.DeanQualification),
                PrincipalQualificationDate = entity.DeanQualificationDate.HasValue
                    ? entity.DeanQualificationDate.Value.ToString("dd-MM-yyyy")
                    : "—",
                PrincipalUniversity = entity.DeanUniversity,
                PrincipalStateCouncilNumber = entity.DeanStateCouncilNumber,
                RecognizedByMci = entity.RecognizedByMci == true ? "Yes" : "No"
            };
        }

        private async Task<string?> ResolveQualificationNameAsync(string? qualificationCode)
        {
            if (string.IsNullOrWhiteSpace(qualificationCode))
                return qualificationCode;

            if (!int.TryParse(qualificationCode, out var courseId))
                return qualificationCode;

            return await _context.MstCourses
                .AsNoTracking()
                .Where(course => course.Id == courseId)
                .Select(course => course.CourseName)
                .FirstOrDefaultAsync() ?? qualificationCode;
        }

    }
}
