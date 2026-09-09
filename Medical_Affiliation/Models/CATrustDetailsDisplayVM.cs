namespace Medical_Affiliation.Models
{
    public class CATrustDetailsDisplayVM
    {

        public int InstitutionId { get; set; }
        // =========================
        // Trust / Society Details
        // =========================

        public string? TrustName { get; set; }
        public string? PANNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public DateOnly? RegistrationDate { get; set; }
        public string? PresidentName { get; set; }
        public string? CategoryOfOrganisation { get; set; }
        public string? GOKObtainedTrustName { get; set; }
        public bool? Amendments { get; set; }

        // =========================
        // Trust Contact Details
        // =========================

        public string? Address { get; set; }
        public string? PinCode { get; set; }
        public string? MobileNumber { get; set; }
        public string? StdCode { get; set; }
        public string? Fax { get; set; }
        public string? AltLandlineOrMobile { get; set; }
        public string? EmailId { get; set; }
        public string? AltEmailId { get; set; }

        // =========================
        // Trust Contact Person
        // =========================

        public string? ContactPersonName { get; set; }
        public string? ContactPersonRelation { get; set; }
        public string? ContactPersonMobile { get; set; }

        // =========================
        // Other Trust Information
        // =========================

        public string? ExistingTrustName { get; set; }
        public bool? ChangesInTrustName { get; set; }

        // =========================
        // Uploaded Documents
        // =========================

        public bool HasPANFile { get; set; }
        public bool HasBankStatementFile { get; set; }
        public bool HasRegistrationCertificateFile { get; set; }
        public bool HasAuditStatementFile { get; set; }
        public bool HasAmendedDoc { get; set; }
        public bool HasGokOrderExistingCoursesFile { get; set; }
        public bool HasRegisteredTrustMemberDetails { get; set; }
        public bool HasAadhaarFile { get; set; }
        public bool HasGovAutonomousCertFile { get; set; }
        public bool HasGovCouncilMembershipFile { get; set; }
        public bool HasFirstAffiliationNotifFile { get; set; }
        public bool HasContinuationAffiliationFile { get; set; }
        public bool HasKncCertificateFile { get; set; }
        public bool HasDCIFile { get; set; }
        public bool HasKSDCFile { get; set; }

        public List<DocumentViewerViewModel> Documents { get; set; } = new();
    }
}