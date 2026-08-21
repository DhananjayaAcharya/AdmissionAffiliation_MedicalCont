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
            //var ugCourseDetails = await GetAffiliationCourseDetails();
            //var deanOrDeanDetails = await GetDeanOrDirectorDetails();
            var deanOrDeanDetails = await GetInstitutionDeanInfo();
            //var principalDetails = await GetPrincipalDetails();
            var principalDetails = await GetInstitutionPrincipalDetails();
            var institutionDetails = await GetInstitutionDetails();



            return new InstituionBasicDetailsDisplayVM
            {
                InstitutionDetails = institutionDetails,
                TrustMemberVM = trustDetails,
                IntakeForCourseVM = courseIntake,
                AffCoursesVM = CourseDetails,
                //AffiliationCourseDetailVM = ugCourseDetails,
                DeanOrDirectorDetailDisplayVM = deanOrDeanDetails,
                PrincipalDetailDisplayVM = principalDetails
            };
        }

        private async Task<InstitutionViewModel?> GetInstitutionDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var institution = await _context.AffInstitutionsDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString());

            if (institution == null)
                return null;

            var instType = await _context.MstInstitutionTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.InstitutionTypeId.ToString() == institution.TypeOfInstitution);

            if (instType == null) return null;

            var talukData = await _context.TalukMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TalukId == institution.Taluk);

            var districtData = await _context.DistrictMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.DistrictId == institution.District);

            var clgStatus = await _context.AffInstitutionStatusMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.StatusCode == institution.StatusOfCollege);

            return new InstitutionViewModel
            {
                InstitutionId = institution.InstitutionId,
                CollegeCode = institution.CollegeCode,
                FacultyCode = institution.FacultyCode,

                TypeOfInstitution = instType.InstitutionType,
                NameOfInstitution = institution.NameOfInstitution,

                Address = institution.Address,
                VillageTownCity = institution.VillageTownCity,
                Taluk = talukData.TalukName,
                District = districtData.DistrictName,
                PinCode = institution.PinCode,

                MobileNumber = institution.MobileNumber,
                StdCode = institution.StdCode,
                Fax = institution.Fax,

                Website = institution.Website,
                SurveyNoPidNo = institution.SurveyNoPidNo,

                MinorityInstitute = institution.MinorityInstitute,
                AttachedToMedicalClg = institution.AttachedToMedicalClg,
                RuralInstitute = institution.RuralInstitute,

                YearOfEstablishment = institution.YearOfEstablishment,

                EmailId = institution.EmailId,
                AltLandlineMobile = institution.AltLandlineMobile,
                AltEmailId = institution.AltEmailId,

                HeadOfInstitution = institution.HeadOfInstitution,
                HeadAddress = institution.HeadAddress,

                FinancingAuthority = institution.FinancingAuthority,
                StatusOfCollege = institution.StatusOfCollege,
                CourseApplied = institution.CourseApplied,

                DocumentName = institution.DocumentName,
                DocumentContentType = institution.DocumentContentType,

                NodalOfficer_Name = institution.NodalOfficerName,
                NodalOfficer_Mob_Number = institution.NodalOfficerMobNumber,
                NodalOfficer_Email = institution.NodalOfficerEmail,

                Principal_Name = institution.PrincipalName,
                Principal_Mob_No = institution.PrincipalMobNo,
                Principal_Email = institution.PrincipalEmail,

                HeadOfInstitution_Mob_NO =
                    institution.HeadOfInstitutionMobNo,

                HeadOfInstitution_Email =
                    institution.HeadOfInstitutionEmail,

                College_URL = institution.CollegeUrl,

                TrustName = institution.TrustName,
                TrustAddress = institution.TrustAddress,
                TrustEstablishmentDate =
                    institution.TrustEstablishmentDate,

                TrustPresidentName =
                    institution.TrustPresidentName,

                TrustPresidentContactNo =
                    institution.TrustPresidentContactNo,

                DeanName = institution.DeanName,
                DeanMobileNumber = institution.DeanMobileNumber,
                DeanEmailId = institution.DeanEmailId,

                PrincipalMobileNumber = institution.PrincipalMobileNumber,

                PrincipalEmailId = institution.PrincipalEmailId,

                MinorityCategory = institution.MinorityCategory,
                RunningCourse = institution.RunningCourse,
                CourseLevel = institution.CourseLevel,

                GovAutonomousCertNumber = institution.GovAutonomousCertNumber,

                hasGovAutoCertFile = !string.IsNullOrEmpty(institution.GovAutonomousCertPath)
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

        //public async Task<AffiliationCourseDetailDisplayVM> GetAffiliationCourseDetails()
        //{
        //    var collegeCode = _userContext.CollegeCode;
        //    var facultyCode = _userContext.FacultyId;

        //    var data = await _context.AffiliationCourseDetails
        //        .Where(x => x.Collegecode == collegeCode && x.Facultycode == facultyCode.ToString())
        //        .OrderBy(x => x.CourseName)
        //        .Select(x => new AffiliationCourseDetailDisplayVM
        //        {
        //            CourseName = x.CourseName,
        //            IntakeDuring202526 = x.IntakeDuring202526,
        //            IntakeSlab = x.IntakeSlab,
        //            TypeofPermission = x.Typeofpermission,
        //            YearOfLop = x.YearofLop.HasValue ? x.YearofLop.Value.Year.ToString() : null,
        //            DateOfRecognition = x.Dateofrecognition,
        //            YearOfObtainingEcAndFc = x.YearofObtainingEcandFc.HasValue ? x.YearofObtainingEcandFc.Value.Year.ToString() : null,
        //            SanctionedIntakeEcFc = x.SannctionedIntakeEcFc,
        //            HasGokOrder = x.GokorderPath != null
        //        })
        //        .FirstOrDefaultAsync();

        //    return data;
        //}

        public async Task<InstitutionViewModel?> GetInstitutionDeanInfo()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.AffInstitutionsDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .Select(x => new InstitutionViewModel
                {
                    DeanName = x.DeanName,
                    DeanEmailId = x.DeanEmailId,
                    DeanMobileNumber = x.DeanMobileNumber,
                    DocumentName = x.DocumentName
                })
                .FirstOrDefaultAsync();

            return data;
        }


        public async Task<AffDeanOrDirectorDetailDisplayVM?> GetDeanOrDirectorDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.AffDeanOrDirectorDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .Select(x => new AffDeanOrDirectorDetailDisplayVM
                {
                    DeanOrDirectorName = x.DeanOrDirectorName,
                    DeanQualification = x.DeanQualification,
                    DeanQualificationDate = x.DeanQualificationDate.HasValue
                        ? x.DeanQualificationDate.Value.ToString("dd-MM-yyyy")
                        : "—",
                    DeanUniversity = x.DeanUniversity,
                    DeanStateCouncilNumber = x.DeanStateCouncilNumber,
                    RecognizedByMci = x.RecognizedByMci == true ? "Yes" : "No"
                })
                .FirstOrDefaultAsync();

            return data;
        }

        public async Task<InstitutionViewModel?> GetInstitutionPrincipalDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.AffInstitutionsDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .Select(x => new InstitutionViewModel
                {
                    Principal_Name = x.PrincipalName,
                    PrincipalEmailId = x.PrincipalEmail,
                    PrincipalMobileNumber = x.PrincipalMobileNumber
                })
                .FirstOrDefaultAsync();

            return data;
        }

        public async Task<AffPrincipalDetailDisplayVM?> GetPrincipalDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.AffPrincipalDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .Select(x => new AffPrincipalDetailDisplayVM
                {
                    PrincipalName = x.DeanOrDirectorName,
                    PrincipalQualification = x.DeanQualification,
                    PrincipalQualificationDate = x.DeanQualificationDate.HasValue
                        ? x.DeanQualificationDate.Value.ToString("dd-MM-yyyy")
                        : "—",
                    PrincipalUniversity = x.DeanUniversity,
                    PrincipalStateCouncilNumber = x.DeanStateCouncilNumber,
                    RecognizedByMci = x.RecognizedByMci == true ? "Yes" : "No"
                })
                .FirstOrDefaultAsync();

            return data;
        }

    }
}
