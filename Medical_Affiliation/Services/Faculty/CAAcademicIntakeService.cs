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
        private readonly ICAPgCourseService _pgCourseService;

        public CAAcademicIntakeService(
            ApplicationDbContext context,
            ICAPgCourseService pgCourseService,
            IUserContext userContext)
        {
            _context = context;
            _pgCourseService = pgCourseService;
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
            model.PgCourseDetails = await _pgCourseService.GetPgCourseDetailsAsync();
            model.UgCourseDetails = await GetAffiliationCourseDetails();
            model.TeachingFacultyDetails = await GetTeachingFacultyDetails();

            return model;
        }

        public async Task<AffiliationCourseDetailsDisplayVM> GetAffiliationCourseDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.AffiliationCourseDetails
                .Where(x => x.Collegecode == collegeCode && x.Facultycode == facultyCode.ToString())
                .OrderBy(x => x.CourseName)
                .Select(x => new AffiliationCourseDetailsDisplayVM
                {
                    CourseId = x.CourseId,
                    CourseName = x.CourseName,
                    IntakeDuring202526 = x.IntakeDuring202526,
                    IntakeSlab = x.IntakeSlab,
                    TypeOfPermission = x.Typeofpermission,

                    YearOfLOP = x.YearofLop,
                    DateOfRecognition = x.Dateofrecognition,
                    YearOfObtainingECAndFC = x.YearofObtainingEcandFc,
                    SanctionedIntakeECFC = x.SannctionedIntakeEcFc,

                    SanctionedIntakePermission = x.SanctionedIntakePermission,
                    DateOfLOPRenewalGOIMCI = x.DateOfLoprenewalGoimci,
                    DateOfLOPRenewalDCIKSDC = x.DateOfLoprenewalDciksdc,

                    YearOfLastAffiliationRGUHS = x.YearOfLastAffiliationRguhs,
                    SanctionedIntakeLastAffiliation = x.SanctionedIntakeLastAffiliation,

                    DateOfPreviousLICInspection = x.DateOfPreviousLicinspection,
                    ActionTakenOnDeficiencies = x.ActionTakenOnDeficiencies,

                    HasGOKOrder = x.GokorderPath != null,
                    HasLastAffiliationFile = x.LastAffiliationRguhsfilePath != null,
                    HasPreviousNotificationFile = x.PreviousNotificationFilesPath != null
                })
                .FirstOrDefaultAsync();

            return data;
        }


        public async Task<List<DentalTeachingFacultyDisplayVM>> GetTeachingFacultyDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();
            var facultyIntCode = _userContext.FacultyId;

            // =========================
            // Get Academic Intake
            // =========================
            var academicIntake = await _context.AcademicIntakes
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (academicIntake == null)
                return new List<DentalTeachingFacultyDisplayVM>();

            int totalIntake = academicIntake.Ay2026TotalIntake;

            // =========================
            // Calculate Seat Slab
            // =========================
            int slabValue = ((totalIntake + 49) / 50) * 50;

            // =========================
            // Get Seat Slab Id
            // =========================
            var seatSlabId = await _context.SeatSlabMasters
                .Where(x => x.SeatSlab == slabValue)
                .Select(x => x.SeatSlabId)
                .FirstOrDefaultAsync();

            if (seatSlabId == null)
                return new List<DentalTeachingFacultyDisplayVM>();

            // =========================
            // Departments
            // =========================
            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == _userContext.FacultyId)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            // =========================
            // Top 3 Designations
            // =========================
            var designations = await _context.DesignationMasters
                .Where(x => x.FacultyCode == _userContext.FacultyId)
                .OrderBy(x => x.DesignationOrder)
                .Take(3)
                .ToListAsync();

            // =========================
            // Faculty Requirement Master
            // =========================
            var facultyRequirements = await _context.DepartmentWiseFacultyMasters
                .Where(x =>
                    x.FacultyCode == facultyIntCode &&
                    x.SeatSlabId == seatSlabId.ToString())
                .ToListAsync();

            // =========================
            // Existing Saved Records
            // =========================
            var existingRecords = await _context.CollegeDesignationDetails
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .ToListAsync();

            var result = new List<DentalTeachingFacultyDisplayVM>();

            foreach (var dept in departments)
            {
                foreach (var desig in designations)
                {
                    var requirement = facultyRequirements.FirstOrDefault(x =>
                        x.DepartmentCode == dept.DepartmentCode &&
                        x.DesignationCode == desig.DesignationCode);

                    int requiredSeats = requirement?.Seats ?? 0;

                    var existing = existingRecords.FirstOrDefault(x =>
                        x.DepartmentCode == dept.DepartmentCode &&
                        x.DesignationCode == desig.DesignationCode &&
                        x.SeatSlabId == seatSlabId.ToString());

                    result.Add(new DentalTeachingFacultyDisplayVM
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        Faculty = facultyCode,

                        DepartmentCode = dept.DepartmentCode,
                        DepartmentName = dept.DepartmentName,

                        DesignationCode = desig.DesignationCode,
                        DesignationName = desig.DesignationName,

                        SeatSlabId = seatSlabId.ToString(),

                        ExistingSeatIntake = existing?.RequiredIntake ?? requiredSeats.ToString(),
                        PresentSeatIntake = existing?.AvailableIntake ?? "0"
                    });
                }
            }

            return result;
        }
    }
}
