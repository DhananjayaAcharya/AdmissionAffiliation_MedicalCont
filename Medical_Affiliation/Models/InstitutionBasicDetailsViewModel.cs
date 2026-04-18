using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class InstitutionBasicDetailsViewModel
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

        [DataType(DataType.Date)]
        public DateTime? RegistrationDate { get; set; }

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

    public class TrustMemberDetailsRowVM
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

    public class TrustMemberDetailsListVM
    {
        public string FacultyCode { get; set; }   // if same for all rows, keep here
        public string CollegeCode { get; set; }   // if same for all rows, keep here
        public List<TrustMemberDetailsRowVM> Rows { get; set; } = new();
        public IEnumerable<SelectListItem> DesignationList { get; set; } = new List<SelectListItem>();
    }


    public class InstitutionDetailsViewModel
    {
        public int InstitutionId { get; set; }
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }
        public string TypeOfInstitution { get; set; }
        public string NameOfInstitution { get; set; }
        public string Address { get; set; }
        public string VillageTownCity { get; set; }
        public string Taluk { get; set; }
        public string District { get; set; }
        public string PinCode { get; set; }
        public string MobileNumber { get; set; }
        public string StdCode { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string SurveyNoPidNo { get; set; }
        public bool MinorityInstitute { get; set; }
        public bool AttachedToMedicalClg { get; set; }
        public bool RuralInstitute { get; set; }
        public string? YearOfEstablishment { get; set; }
        public string EmailId { get; set; }
        public string AltLandlineMobile { get; set; }
        public string AltEmailId { get; set; }
        public string HeadOfInstitution { get; set; }
        public string HeadAddress { get; set; }
        public string FinancingAuthority { get; set; }
        public string StatusOfCollege { get; set; }
        public string CourseApplied { get; set; }

        // file upload
        public IFormFile DocumentFile { get; set; }
    }

    public class AffCourseDetailRowVM
    {
        public int Id { get; set; }              // AFF_CourseDetails.Id (0 for new)
        public string CollegeCode { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;

        public string CourseId { get; set; } = string.Empty;     // AFF_CourseDetails.CourseId NVARCHAR(50)
        public string CourseName { get; set; } = string.Empty;   // AFF_CourseDetails.CourseName NVARCHAR(200)

        public bool IsRecognized { get; set; }

        [Required(ErrorMessage = "RGUHS Notification No is required when course is recognized.")]
        [StringLength(100)]
        public string RguhsNotificationNo { get; set; } = string.Empty;  // AFF_CourseDetails.RguhsNotificationNo NVARCHAR(100)

        // File upload support
        public IFormFile? SupportingFile { get; set; }
        public string? ExistingFileUrl { get; set; }     // For displaying existing DocumentData links

        // Audit fields
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
    }

    public class AffCourseDetailsListVM
    {
        public List<AffCourseDetailRowVM> Courses { get; set; } = new();
    }
    public class SanctionedIntakeRowVm
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        public string? SanctionedIntake { get; set; }
        public string? EligibleSeatSlab { get; set; }
        // NEW: existing DB row id (for download)
        public int? ExistingIntakeId { get; set; }
        public IFormFile? DocumentFile { get; set; }
    }

    public class SanctionedIntakeCreateVm
    {
        public List<SanctionedIntakeRowVm> Courses { get; set; } = new();
    }
    public class AdminTeachingBlockRowVM
    {
        public int Id { get; set; }                    // Aff_AdminTeachingBlock.Id (0 for new)

        public string CollegeCode { get; set; } = "";
        public string FacultyCode { get; set; } = "";

        public int FacilityId { get; set; }   // from Mst_AdministrativeFacilities
        public string Facilities { get; set; } = "";   // name/description
        public string SizeSqFtAsPerNorms { get; set; } = "";

        public bool IsAvailable { get; set; }          // Yes / No
        public string NoOfRooms { get; set; } = "";    // user input
        public string SizeSqFtAvailablePerRoom { get; set; } = ""; // user input
                                                                   // read-only, calculated in UI; not persisted
        public string Deficiency { get; set; } = "";
    }

    public class AdminTeachingBlockListVM
    {
        public List<AdminTeachingBlockRowVM> Rows { get; set; } = new();
    }
    public class AffHostelDetails  // Singular name to match DbSet
    {
        public int HostelDetailsId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        public string HostelType { get; set; } = string.Empty;  // Changed: string, no FK

        public string BuiltUpAreaSqFt { get; set; } = string.Empty;      // Keep string
        public bool HasSeparateHostel { get; set; }
        public bool SeparateProvisionMaleFemale { get; set; }
        public string TotalFemaleStudents { get; set; } = string.Empty;  // Keep string
        public string TotalFemaleRooms { get; set; } = string.Empty;     // Keep string
        public string TotalMaleStudents { get; set; } = string.Empty;    // Keep string
        public string TotalMaleRooms { get; set; } = string.Empty;       // Keep string
        public byte[]? PossessionProofPath { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class MstHostelType
    {
        public int Id { get; set; }
        public string Hospital_Type { get; set; } = string.Empty;
    }

    public class AffHostelDetailsCreateVm
    {
        public AffHostelDetail Hostel { get; set; } = new();
        public IEnumerable<SelectListItem> HostelTypes { get; set; } = new List<SelectListItem>();

        public string? CourseLevel {  get; set; }

        //public string? OwnOrRented { get; set; }
    }

    public class HostelFacilityRowVm
    {
        // From Mst_HostelFacilities
        public int HostelFacilityId { get; set; }
        public string HostelFacilityName { get; set; }
        public int FacultyId { get; set; }

        // From Aff_HostelFacilityDetails
        public int? AffId { get; set; }          // null if not yet saved
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class HostelFacilityListVm
    {
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public List<HostelFacilityRowVm> Rows { get; set; } = new();
    }

    public class Aff_FacultyDetailsViewModel 
    {
        // Existing properties...
        public int FacultyDetailId { get; set; }
        public string NameOfFaculty { get; set; }
        public string Subject { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Aadhaar { get; set; }
        public string PAN { get; set; }
        public string RN_RMNumber { get; set; }
        public string Department { get; set; }
        public string DepartmentDetails { get; set; }
        public string SelectedDepartment { get; set; }
        public string Designation { get; set; }
        public string Qualification { get; set; }
        public string UGInstituteName { get; set; }
        public int? UGYearOfPassing { get; set; }
        public string PGInstituteName { get; set; }
        public int? PGYearOfPassing { get; set; }
        public string PGPassingSpecialization { get; set; }
        public decimal? TeachingExperienceAfterUGYears { get; set; }
        public decimal? TeachingExperienceAfterPGYears { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public bool RecognizedPgTeacher { get; set; }
        public bool RecognizedPhDTeacher { get; set; }
        public bool IsRecognizedPGGuide { get; set; }
        public bool IsExaminer { get; set; }
        public string ExaminerFor { get; set; }
        public List<string> ExaminerForList { get; set; }
        public bool LitigationPending { get; set; }
        public string NRTSNumber { get; set; }
        public string RGUHSTIN { get; set; }
        public string RemoveRemarks { get; set; }
        public bool IsRemoved { get; set; }

        // Document uploads
        public IFormFile AadhaarDocument { get; set; }
        public IFormFile PANDocument { get; set; }
        public IFormFile RN_RMDocument { get; set; }
        public IFormFile Form16OrLast6MonthsStatement { get; set; }
        public IFormFile AppointmentOrderDocument { get; set; }
        public IFormFile GuideRecognitionDoc { get; set; }
        public IFormFile PhDRecognitionDoc { get; set; }
        public IFormFile LitigationDoc { get; set; }

        // Existing dropdowns
        public List<SelectListItem> Subjects { get; set; }
        public List<SelectListItem> Designations { get; set; }
        public List<SelectListItem> DepartmentDetailsList { get; set; }
        public IFormFile OnlineTeachersDatabase { get; set; }
        public IFormFile madetorecruit { get; set; }
    }

    //public class teachingSTaffdocsVM
    //{
    //    public IFormFile OnlineTeachersDatabase { get; set; }
    //    public IFormFile madetorecruit { get; set; }
    //}

    public class NursingAffiliationViewModel
    {
        public SanctionedIntakeCreateVm AffSanIntake { get; set; }
        public AffCourseDetailsListVM AffCourseDetails { get; set; }
        public InstitutionDetailsViewModel InstituteDetails { get; set; }
        public InstitutionBasicDetailsViewModel NursingInstituteDetails { get; set; }
        public TrustMemberDetailsListVM TrustMemberDetails { get; set; }
    }
    public class NonTeachingFacultyViewModel
    {
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }

        public List<NonTeachingFacultyRow> Rows { get; set; } = new();
    }

    public class NonTeachingFacultyRow
    {
        public int Id { get; set; }
        public string FacultyName { get; set; }
        public string Designation { get; set; }
        public string Qualification { get; set; }
        public string? TotalExperience { get; set; }
        public string Remarks { get; set; }
    }



    public class InstitutionViewModel1
    {
        // Keys (from Session)
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }

        public string TypeOfInstitution { get; set; }
        public string NameOfInstitution { get; set; }
        public string Address { get; set; }
        public string VillageTownCity { get; set; }
        public string Taluk { get; set; }
        public string District { get; set; }
        public string PinCode { get; set; }
        public string MobileNumber { get; set; }
        public string StdCode { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string SurveyNoPidNo { get; set; }
        public bool MinorityInstitute { get; set; }
        public bool AttachedToMedicalClg { get; set; }
        public bool RuralInstitute { get; set; }
        public string YearOfEstablishment { get; set; }
        public string EmailId { get; set; }
        public string AltLandlineMobile { get; set; }
        public string AltEmailId { get; set; }
        public string HeadOfInstitution { get; set; }
        public string HeadAddress { get; set; }
        public string FinancingAuthority { get; set; }
        public string StatusOfCollege { get; set; }
        public string CourseApplied { get; set; }

        public string DocumentName { get; set; }
        public string DocumentContentType { get; set; }
        // DocumentData will be handled via IFormFile in controller, not in ViewModel

        public string NodalOfficer_Name { get; set; }
        public string NodalOfficer_Mob_Number { get; set; }
        public string NodalOfficer_Email { get; set; }

        public string Principal_Name { get; set; }
        public string Principal_Mob_No { get; set; }
        public string Principal_Email { get; set; }

        public string HeadOfInstitution_Mob_NO { get; set; }
        public string HeadOfInstitution_Email { get; set; }

        public string College_URL { get; set; }

        public string TrustName { get; set; }
        public string TrustAddress { get; set; }
        public DateOnly? TrustEstablishmentDate { get; set; }
        public string TrustPresidentName { get; set; }
        public string TrustPresidentContactNo { get; set; }

        public string DeanName { get; set; }
        public string DeanMobileNumber { get; set; }
        public string DeanEmailId { get; set; }

        public string PrincipalMobileNumber { get; set; }
        public string PrincipalEmailId { get; set; }

        public string MinorityCategory { get; set; }
        public string RunningCourse { get; set; }

        // Dropdown data
        public List<SelectListItem> TalukList { get; set; }
        public List<SelectListItem> DistrictList { get; set; }
        public List<SelectListItem> CourseList { get; set; }
        public List<SelectListItem> institutetypelist { get; set; }
        public List<SelectListItem> Institutestatuslist { get; set; }

        }



    public class AffiliationCourseDetailsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Faculty code is required")]
        public string Facultycode { get; set; } = string.Empty;

        [Required(ErrorMessage = "College code is required")]
        public string Collegecode { get; set; } = string.Empty;

        [Required]
        public string CourseId { get; set; } = "MBBS";

        [Required]
        public string courseName { get; set; } = "MBBS";

        // a. Present Intake
        [Required(ErrorMessage = "Intake during 2025-26 is required")]
        public string IntakeDuring_2025_26 { get; set; } = string.Empty;

        // b. Previous details
        public string Intake_slab { get; set; } = string.Empty;           // e.g., 50 / 51-100 etc.
        public string Typeofpermission { get; set; } = string.Empty;      // Fresh / Renewal / Increase

        public DateOnly? yearofLOP { get; set; }
        public string dateofrecognition { get; set; } = string.Empty;     // Date or Order details from apex bodies

        // c. EC & FC from Government of Karnataka
        public DateOnly? yearofObtainingECandFC { get; set; }
        public string sannctionedIntake_EC_FC { get; set; } = string.Empty;

        // New fields as per the image (Section c, d, e, f)
        public string SanctionedIntake_Permission { get; set; } = string.Empty;   // Column in section c
        public string DateOfLOP_Renewal_GOIMCI { get; set; } = string.Empty;      // "Date of LOP & renewal from GOI/MCI/NMC for UG"

        // d. EC & FC (already have year and sanctioned, kept for consistency)
        // e. Last affiliation by RGUHS
        public string YearOfLastAffiliation_RGUHS { get; set; } = string.Empty;
        public string SanctionedIntake_LastAffiliation { get; set; } = string.Empty;

        // f. Previous LIC Inspection
        public DateOnly? DateOfPreviousLICInspection { get; set; }
        public string ActionTakenOnDeficiencies { get; set; } = string.Empty;

        // File uploads
        public IFormFile? GOKorder { get; set; }                    // For EC & FC
        public IFormFile? LastAffiliationRGUHSFile { get; set; }    // For section e
        public IFormFile? PreviousNotificationFiles { get; set; }   // For section b (previous years)

        // For viewing already uploaded files (byte arrays from DB)
        public bool HasGOKOrder { get; set; }
        public bool HasLastAffiliationFile { get; set; }

        public bool HasPreviousNotificationFile { get; set; }
    }
    public class DeanDetailsViewModel
    {
        // Parent table: Aff_DeanOrDirectorDetails
        public int? Id { get; set; }

        public string? CourseLevel { get; set; }
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public string DeanOrDirectorName { get; set; }
        public string DeanQualification { get; set; }
        public DateOnly? DeanQualificationDate { get; set; }
        public string DeanUniversity { get; set; }
        public string DeanStateCouncilNumber { get; set; }
        public bool RecognizedByMCI { get; set; }
        public string UGYears { get; set; }

        // Child: Teaching
        public List<TeachingExperienceRow> TeachingExperiences { get; set; }

        // Child: Administrative
        public List<AdministrativeExperienceRow> AdministrativeExperiences { get; set; }

        public DeanDetailsViewModel()
        {
            TeachingExperiences = new List<TeachingExperienceRow>();
            AdministrativeExperiences = new List<AdministrativeExperienceRow>();
        }
    }

    public class TeachingExperienceRow
    {
        public int? Id { get; set; }          // PK of Aff_DeanTeachingExperience (for edit)
        public string? FacultyCode { get; set; }
        public string? CollegeCode { get; set; }
        public string? Designation { get; set; }   // JR, SR, etc.
        public DateOnly? UGFrom { get; set; }
        public DateOnly? UGTo { get; set; }
        public DateOnly? PGFrom { get; set; }
        public DateOnly? PGTo { get; set; }
        public decimal? TotalExperienceYears { get; set; }
    }

    public class AdministrativeExperienceRow
    {
        public int? Id { get; set; }          // PK of Aff_DeanAdministrativeExperience (for edit)
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public string PostHeld { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public decimal? TotalExperienceYears { get; set; }
    }

}
