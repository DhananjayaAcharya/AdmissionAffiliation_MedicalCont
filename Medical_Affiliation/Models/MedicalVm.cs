using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class MedicalVm
    {
            public int InstitutionId { get; set; }

            // Codes
            public string FacultyCode { get; set; }
            public string CollegeCode { get; set; }

            // Institution basic details
            //public string TypeOfOrganization { get; set; }
            [Required(ErrorMessage = "Please select institution type")]  // Only on SELECTED value
            public string? TypeOfInstitution { get; set; }

            public IEnumerable<SelectListItem>? TypeOfInstitutionList { get; set; }  // NO [Required]

            public string NameOfInstitution { get; set; }
            public string AddressOfInstitution { get; set; }
            public string VillageTownCity { get; set; }
            public string Taluk { get; set; }
            public string District { get; set; }
            public string PinCode { get; set; }
            public string MobileNumber { get; set; }
            public string StdCode { get; set; }
            public string Fax { get; set; }
            public string Website { get; set; }
            public string EmailId { get; set; }
            public string AltLandlineOrMobile { get; set; }
            public string AltEmailId { get; set; }
            public string AcademicYearStarted { get; set; }

            // MUST be non-nullable for checkboxes
            public bool IsRuralInstitution { get; set; }
            public bool IsMinorityInstitution { get; set; }

            // Trust / society
            public string TrustName { get; set; }
            public string PresidentName { get; set; }
            public string AadhaarNumber { get; set; }
            public string PANNumber { get; set; }
            public string RegistrationNumber { get; set; }

            //[DataType(DataType.Date)]
            public DateOnly? RegistrationDate { get; set; }

            // MUST be non-nullable for checkboxes
            public bool Amendments { get; set; }

            public string ExistingTrustName { get; set; }
            public string GOKObtainedTrustName { get; set; }

            // MUST be non-nullable for checkboxes
            public bool ChangesInTrustName { get; set; }
            public bool OtherNursingCollegeInCity { get; set; }

            public string CategoryOfOrganisation { get; set; }
            public string ContactPersonName { get; set; }
            public string ContactPersonRelation { get; set; }
            public string ContactPersonMobile { get; set; }

            // This was also bool? but is not rendered as checkbox in your view; keep as bool if you plan to use checkbox
            public bool OtherPhysiotherapyCollegeInCity { get; set; }

            // Courses and headings
            public string CoursesAppliedText { get; set; }
            public string HeadOfInstitutionName { get; set; }
            public string HeadOfInstitutionAddress { get; set; }
            public string FinancingAuthorityName { get; set; }
            public string CollegeStatus { get; set; }

            // Document numbers
            public string GovAutonomousCertNumber { get; set; }
            public string KncCertificateNumber { get; set; }

            // Files (IFormFile)
            public IFormFile GovAutonomousCertFile { get; set; }
            public IFormFile GovCouncilMembershipFile { get; set; }
            public IFormFile GokOrderExistingCoursesFile { get; set; }
            public IFormFile FirstAffiliationNotifFile { get; set; }
            public IFormFile ContinuationAffiliationFile { get; set; }
            public IFormFile KncCertificateFile { get; set; }
            public IFormFile AmendedDoc { get; set; }
            public IFormFile AadhaarFile { get; set; }
            public IFormFile PANFile { get; set; }
            public IFormFile BankStatementFile { get; set; }
            public IFormFile RegistrationCertificateFile { get; set; }
            public IFormFile RegisteredTrustMemberDetails { get; set; }
            public IFormFile AuditStatementFile { get; set; }
    }
    public class Medical_TrustMemberDetailsRowVM
    {
        public int SlNo { get; set; }          // optional, DB identity
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public string TrustMemberName { get; set; }
        //public string? Designation { get; set; }      // optional display field
        public string Qualification { get; set; }
        public string MobileNumber { get; set; }
        public int? Age { get; set; }

        // HTML date input binding (yyyy-MM-dd)
        public string JoiningDateString { get; set; }

        // Bind dropdown value (store designation identifier/code as string)
        public string? DesignationCode { get; set; }
    }

    public class Medical_TrustMemberDetailsListVM
    {
        public string FacultyCode { get; set; }   // if same for all rows, keep here
        public string CollegeCode { get; set; }   // if same for all rows, keep here
        public List<TrustMemberDetailsRowVM> Rows { get; set; } = new();
        public IEnumerable<SelectListItem> DesignationList { get; set; } = new List<SelectListItem>();
    }
}

