using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;

namespace Medical_Affiliation.Services.Faculty
{
    public class InstitutionPreviewService : IInstitutionPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public InstitutionPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<InstitutionPreviewViewModel> GetInstitutionPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            return await _context.AffInstitutionsDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode)
                .Select(x => new InstitutionPreviewViewModel
                {
                    CollegeCode = x.CollegeCode,

                    GeneralDetails = new InstitutionGeneralDisplayVM
                    {
                        InstitutionId = x.InstitutionId,
                        CollegeCode = x.CollegeCode,
                        FacultyCode = x.FacultyCode,

                        TypeOfInstitution = x.TypeOfInstitution,
                        NameOfInstitution = x.NameOfInstitution,
                        Address = x.Address,
                        VillageTownCity = x.VillageTownCity,
                        Taluk = x.Taluk,
                        District = x.District,
                        PinCode = x.PinCode,

                        SurveyNoPidNo = x.SurveyNoPidNo,

                        YearOfEstablishment = x.YearOfEstablishment,

                        FinancingAuthority = x.FinancingAuthority,
                        StatusOfCollege = x.StatusOfCollege,

                        CourseApplied = x.CourseApplied,

                        MinorityCategory = x.MinorityCategory,
                        RunningCourse = x.RunningCourse,

                        MinorityInstitute = x.MinorityInstitute,
                        AttachedToMedicalClg = x.AttachedToMedicalClg,
                        RuralInstitute = x.RuralInstitute,

                        GovAutonomousCertNumber = x.GovAutonomousCertNumber,
                        HasGovAutoCertFile = !string.IsNullOrEmpty(x.DocumentName)
                    },

                    ContactDetails = new InstitutionContactDisplayVM
                    {
                        MobileNumber = x.MobileNumber,
                        StdCode = x.StdCode,
                        Fax = x.Fax,
                        Website = x.Website,
                        College_URL = x.CollegeUrl,
                        EmailId = x.EmailId,
                        AltLandlineMobile = x.AltLandlineMobile,
                        AltEmailId = x.AltEmailId
                    },

                    AuthorityDetails = new InstitutionAuthorityDisplayVM
                    {
                        HeadOfInstitution = x.HeadOfInstitution,
                        HeadAddress = x.HeadAddress,
                        HeadOfInstitution_Mob_NO = x.HeadOfInstitutionMobNo,
                        HeadOfInstitution_Email = x.HeadOfInstitutionEmail,

                        NodalOfficer_Name = x.NodalOfficerName,
                        NodalOfficer_Mob_Number = x.NodalOfficerMobNumber,
                        NodalOfficer_Email = x.NodalOfficerEmail,

                        Principal_Name = x.PrincipalName,
                        Principal_Mob_No = x.PrincipalMobNo,
                        Principal_Email = x.PrincipalEmail,

                        PrincipalMobileNumber = x.PrincipalMobileNumber,
                        PrincipalEmailId = x.PrincipalEmailId,

                        DeanName = x.DeanName,
                        DeanMobileNumber = x.DeanMobileNumber,
                        DeanEmailId = x.DeanEmailId
                    },

                    TrustDetails = new InstitutionTrustDisplayVM
                    {
                        TrustName = x.TrustName,
                        TrustAddress = x.TrustAddress,
                        TrustEstablishmentDate = x.TrustEstablishmentDate,
                        TrustPresidentName = x.TrustPresidentName,
                        TrustPresidentContactNo = x.TrustPresidentContactNo
                    },

                    OtherDetails = new InstitutionOtherDisplayVM
                    {
                        DocumentName = x.DocumentName,
                        DocumentContentType = x.DocumentContentType
                    }
                })
                .FirstOrDefaultAsync();
        }
    }
}