using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CATrustDetailsService : ICATrustDetailsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CATrustDetailsService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<CATrustDetailsDisplayVM?> GetTrustDetailsAsync()
        {
            var collegeCode = _userContext.CollegeCode;

            var trust = await _context.InstitutionBasicDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

            if (trust == null)
                return null;

            return new CATrustDetailsDisplayVM
            {

                InstitutionId = trust.InstitutionId,
                // =========================
                // Trust / Society Details
                // =========================

                TrustName = trust.TrustName,
                PANNumber = trust.Pannumber,
                RegistrationNumber = trust.RegistrationNumber,
                RegistrationDate = trust.RegistrationDate,
                PresidentName = trust.PresidentName,
                CategoryOfOrganisation = trust.CategoryOfOrganisation,
                GOKObtainedTrustName = trust.GokobtainedTrustName,
                Amendments = trust.Amendments,

                // =========================
                // Trust Contact Details
                // =========================

                Address = trust.AddressOfInstitution,
                PinCode = trust.PinCode,
                MobileNumber = trust.MobileNumber,
                StdCode = trust.StdCode,
                Fax = trust.Fax,
                AltLandlineOrMobile = trust.AltLandlineOrMobile,
                EmailId = trust.EmailId,
                AltEmailId = trust.AltEmailId,

                // =========================
                // Trust Contact Person
                // =========================

                ContactPersonName = trust.ContactPersonName,
                ContactPersonRelation = trust.ContactPersonRelation,
                ContactPersonMobile = trust.ContactPersonMobile,

                // =========================
                // Other Trust Information
                // =========================

                ExistingTrustName = trust.ExistingTrustName,
                ChangesInTrustName = trust.ChangesInTrustName,

                // =========================
                // Document Availability
                // =========================

                HasPANFile = !string.IsNullOrWhiteSpace(trust.PanfilePath),

                HasBankStatementFile =
                    !string.IsNullOrWhiteSpace(trust.BankStatementFilePath),

                HasRegistrationCertificateFile =
                    !string.IsNullOrWhiteSpace(trust.RegistrationCertificateFilePath),

                HasAuditStatementFile =
                    !string.IsNullOrWhiteSpace(trust.AuditStatementFilePath),

                HasAmendedDoc =
                    !string.IsNullOrWhiteSpace(trust.AmendedDocPath),

                HasGokOrderExistingCoursesFile =
                    !string.IsNullOrWhiteSpace(trust.GokOrderExistingCoursesFilePath),

                HasRegisteredTrustMemberDetails =
                    !string.IsNullOrWhiteSpace(trust.RegisteredTrustMemberDetailsPath),

                HasAadhaarFile =
                    !string.IsNullOrWhiteSpace(trust.AadhaarFilePath),

                HasGovAutonomousCertFile =
                    !string.IsNullOrWhiteSpace(trust.GovAutonomousCertFilePath),

                HasGovCouncilMembershipFile =
                    !string.IsNullOrWhiteSpace(trust.GovCouncilMembershipFilePath),

                HasFirstAffiliationNotifFile =
                    !string.IsNullOrWhiteSpace(trust.FirstAffiliationNotifFilePath),

                HasContinuationAffiliationFile =
                    !string.IsNullOrWhiteSpace(trust.ContinuationAffiliationFilePath),

                HasKncCertificateFile =
                    !string.IsNullOrWhiteSpace(trust.KncCertificateFilePath),

                HasDCIFile =
                    !string.IsNullOrWhiteSpace(trust.DcicertificateFilePath),

                HasKSDCFile =
                    !string.IsNullOrWhiteSpace(trust.KsdccertificateFilePath)
            };
        }
    }
}
