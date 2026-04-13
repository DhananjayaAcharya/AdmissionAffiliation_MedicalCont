using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AddCourseToMstCourseViewModel
{
    public int? SelectedFacultyCode { get; set; } 
    public List<SelectListItem> Faculties { get; set; } = new();
    public List<SelectListItem> CourseLevels { get; set; } = new();
    public List<SelectListItem> CoursePrefixList { get; set; } = new();
    public List<MstCourse> Courses { get; set; } = new();
    public MstCourse NewCourse { get; set; } = new();  // For the form

}

public class CollegeDesignationDetailsViewModel
{
    public string DesignationCode { get; set; }
    public string Designation { get; set; }
    public int PresentIntake { get; set; }
    public int UGUnderRGUHSIntake { get; set; }
    public int UGPresentIntake { get; set; }
    public int PGUnderRGUHSIntake { get; set; }
    public int PGPresentIntake { get; set; }
    public string? GokSanctioned { get; set; }
    public string? PGGokSanctioned { get; set; }

}

//code added by ram on 07-11-2025

public class CourseDetailsAhsViewModel
{
    public int SlNo { get; set; }          // Serial number
    public string? CourseId { get; set; }      // Course code (from MstCourses)
    public string CourseName { get; set; } // Course name (from MstCourses)

    public bool IsRecognized { get; set; } // YES/NO → true/false
    public string RguhsNotificationNo { get; set; } // RGUHS Notification Number

    public IFormFile SupportingFile { get; set; }   // File upload (PDF, etc.)
    public string UploadedFilePath { get; set; }    // To show "View" link
}

public class CourseFacultyListViewModel
{
    public List<CourseDetailsAhsViewModel> Courses { get; set; }
}

public class NursingExamResultViewModel
{
    public string Course { get; set; } // BSc, PBSC, MSC
    public string Year { get; set; } // 1st Year, 2nd Year, etc.
    public int ExamAppearedCount { get; set; }
    public int PassedOutCount { get; set; }
    //public decimal YearOfPercentage => ExamAppearedCount > 0
    //    ? Math.Round((decimal)PassedOutCount / ExamAppearedCount * 100, 2)
    //    : 0;
    public string? YearOfPercentage { get; set; } // ✅ Add 'set;' here
}

//code updated by ram on 11-11-2025

public class AHSExamResultViewModel
{
    public string Course { get; set; } // BSc, PBSC, MSC
    public string Year { get; set; } // 1st Year, 2nd Year, etc.
    public int ExamAppearedCount { get; set; }
    public int PassedOutCount { get; set; }
    //public decimal YearOfPercentage => ExamAppearedCount > 0
    //    ? Math.Round((decimal)PassedOutCount / ExamAppearedCount * 100, 2)
    //    : 0;
    public string? YearOfPercentage { get; set; } // ✅ Add 'set;' here
}


//code added by ram on 13-11-2025

public class DentalExamResultViewModel
{
    public string Course { get; set; } // BSc, PBSC, MSC
    public string Year { get; set; } // 1st Year, 2nd Year, etc.
    public int ExamAppearedCount { get; set; }
    public int PassedOutCount { get; set; }
    //public decimal YearOfPercentage => ExamAppearedCount > 0
    //    ? Math.Round((decimal)PassedOutCount / ExamAppearedCount * 100, 2)
    //    : 0;
    public string? YearOfPercentage { get; set; } // ✅ Add 'set;' here
}

//code added by ram on 14-11-2025

public class UnaniExamResultViewModel
{
    public string Course { get; set; } // BSc, PBSC, MSC
    public string Year { get; set; } // 1st Year, 2nd Year, etc.
    public int ExamAppearedCount { get; set; }
    public int PassedOutCount { get; set; }
    //public decimal YearOfPercentage => ExamAppearedCount > 0
    //    ? Math.Round((decimal)PassedOutCount / ExamAppearedCount * 100, 2)
    //    : 0;
    public string? YearOfPercentage { get; set; } // ✅ Add 'set;' here
}


// Copy your AffHostelDetails structure
public class AffHospitalDetails
{
    public int HospitalDetailsId { get; set; }
    public string FacultyCode { get; set; } = string.Empty;
    public string CollegeCode { get; set; } = string.Empty;

    public string HospitalType { get; set; } = string.Empty;
    public string BuiltUpAreaSqFt { get; set; } = string.Empty;
    public bool HasSeparateHospital { get; set; }  // Renamed
    public bool SeparateProvisionMaleFemale { get; set; }
    public string TotalBeds { get; set; } = string.Empty;        // Beds instead of students
    public string TotalICUBeds { get; set; } = string.Empty;     // ICU beds
    public string TotalMaleBeds { get; set; } = string.Empty;    // Optional: keep male structure
    public string TotalMaleRooms { get; set; } = string.Empty;
    public byte[]? PossessionProofPath { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}

// Copy HostelFacility structure
public class HospitalFacilityRowVm
{
    public int HospitalFacilityId { get; set; }
    public string HospitalFacilityName { get; set; }
    public int FacultyId { get; set; }
    public int? AffId { get; set; }
    public string FacultyCode { get; set; }
    public string CollegeCode { get; set; }
    public bool IsAvailable { get; set; }
}

public class HospitalFacilityListVm
{
    public AffHospitalDetails Hospital { get; set; } = new();
    public string FacultyCode { get; set; }
    public string CollegeCode { get; set; }
    public List<HospitalFacilityRowVm> Rows { get; set; } = new();
    public IEnumerable<SelectListItem> HospitalTypes { get; set; } = new List<SelectListItem>();
}
public class FellowshipMedicalVm
{
    [Required]
    public string FacultyCode { get; set; } = "";

    [Required]
    public string CollegeCode { get; set; } = "";

    [Required, StringLength(150)]
    public string StudentName { get; set; } = "";

    [Required]
    public DateOnly DOB { get; set; }

    [Required]
    public DateOnly Dateofjoining { get; set; }

    [Required]
    public DateOnly Admission_openingDate { get; set; }

    [Required]
    public DateOnly EndingDate { get; set; }

    [Required, StringLength(150)]
    public string Course { get; set; } = "";

    [StringLength(150)]
    public string? KMC_CertificateNumber { get; set; }

    [StringLength(200)]
    public string? principal_name { get; set; }

    public bool principal_Declaration { get; set; }

    [StringLength(150)]
    public string? FellowshipCode { get; set; }
}

// Entity (add to your DbContext)
[Table("FellowShip_Medical")]
public class FellowShip_Medical
{
    public int id { get; set; }

    public string FacultyCode { get; set; } = "";
    public string Collegecode { get; set; } = "";
    public string StudentName { get; set; } = "";
    public DateOnly DOB { get; set; }
    public DateOnly Dateofjoining { get; set; }
    public DateOnly Admission_openingDate { get; set; }
    public DateOnly EndingDate { get; set; }
    public string Course { get; set; } = "";
    public string? KMC_CertificateNumber { get; set; }
    public string? principal_name { get; set; }
    public string principal_Declaration { get; set; } = "";
    public string? FellowshipCode { get; set; }
    public byte[]? SSLC_Doc { get; set; }
    public byte[]? KMC_Doc { get; set; }
    public byte[]? Experience_Letter_Doc { get; set; }
    public byte[]? AppointmentLetter_Doc { get; set; }
}
public class FellowshipMedicalPageVm
{
    public FellowshipMedicalVm Form { get; set; } = new();
    public List<FellowShipMedical> ExistingRecords { get; set; } = new();
}
public class FellowshipDashboardRowVm
{
    public int Id { get; set; }   // table has id as PK
                                  // New: college display
    public string CollegeName { get; set; } = string.Empty;
    public string? CollegeCode { get; set; }
    public string StudentName { get; set; } = "";
    public DateOnly DOB { get; set; }
    public DateOnly Dateofjoining { get; set; }
    public DateOnly Admission_openingDate { get; set; }
    public DateOnly EndingDate { get; set; }
    public string Course { get; set; } = "";
    public string? KMC_CertificateNumber { get; set; }
    public string? principal_name { get; set; }
    public string? principal_Declaration { get; set; }
    public string? FellowshipCode { get; set; }

    public string? ApprovalStatus { get; set; }
    public string? ApprovalRemark { get; set; }

    // document presence flags
    public bool HasSSLC { get; set; }
    public bool HasKMC { get; set; }
    public bool HasExperience { get; set; }
    public bool HasAppointment { get; set; }
}

public class FellowshipDashboardListVm
{
    public List<FellowshipDashboardRowVm> Items { get; set; } = new();
}
public class FellowshipDirectorDashboardRowVm
{
    public int Id { get; set; }
    public string? CollegeCode { get; set; }
    public string CollegeName { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;
    public DateOnly DOB { get; set; }
    public DateOnly Dateofjoining { get; set; }
    public DateOnly Admission_openingDate { get; set; }
    public DateOnly EndingDate { get; set; }

    public string Course { get; set; } = string.Empty;
    public string? KMC_CertificateNumber { get; set; }
    public string? principal_name { get; set; }
    public string? principal_Declaration { get; set; }
    public string? FellowshipCode { get; set; }

    // Section officer values (already stored on FellowShipMedical)
    public string? SectionOfficerApproval { get; set; }
    public string? SectionOfficerRemark { get; set; }

    // Director values (new columns expected on DB / model extension)
    public string? DirectorApprovalStatus { get; set; }
    public string? DirectorApprovalRemark { get; set; }

    // document presence flags
    public bool HasSSLC { get; set; }
    public bool HasKMC { get; set; }
    public bool HasExperience { get; set; }
    public bool HasAppointment { get; set; }
}

public class FellowshipDirectorDashboardListVm
{
    public List<FellowshipDirectorDashboardRowVm> Items { get; set; } = new();
}
public class FellowshipAcceptedRejectedRowVm
{
    public int Id { get; set; }
    public string? StudentName { get; set; }
    public DateOnly DOB { get; set; }
    public DateOnly Dateofjoining { get; set; }
    public DateOnly Admission_openingDate { get; set; }
    public DateOnly EndingDate { get; set; }
    public string? Course { get; set; }
    public string? KMC_CertificateNumber { get; set; }
    public string? principal_name { get; set; }
    public string? principal_Declaration { get; set; }
    public string? FellowshipCode { get; set; }

    // Document presence flag only (we won't load bytes to view)
    public bool HasSSLC { get; set; }
    public bool HasKMC { get; set; }
    public bool HasExperience { get; set; }
    public bool HasAppointment { get; set; }

    // Director decision columns
    public string? DR_ApprovalStatus { get; set; }
    public string? DR_ApprovalRemark { get; set; }
}

public class FellowshipAcceptedRejectedListVm
{
    public List<FellowshipAcceptedRejectedRowVm> Items { get; set; } = new();
}


public class InstitutionViewModel
{
    // Keys (from Session)
    public string CollegeCode { get; set; }
    public string FacultyCode { get; set; }

    public string? CourseLevel { get; set; }
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

    public string? DocumentName { get; set; }
    public string? DocumentContentType { get; set; }
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

public class AcademicReportViewModel
{
    public string SelectedCollege { get; set; } = "All";
    public string? CourseName { get; set; } 
    public string? CollegeName { get; set; }
    public List<string> Colleges { get; set; } = new List<string>();

    public List<AcademicIntake> IntakeData { get; set; } = new List<AcademicIntake>();

    // Optional: for better display / grouping in view
    public Dictionary<string, List<AcademicIntake>> GroupedByCollege { get; set; }
        = new Dictionary<string, List<AcademicIntake>>();

    // Helper property to check if we are showing all colleges
    public bool IsAllColleges => SelectedCollege == "All";
}


public class AcademicIntakeReportRow
{
    public string CollegeCode { get; set; }
    public string CollegeName { get; set; }

    public string CourseCode { get; set; }
    public string CourseName { get; set; }   // ✅ REQUIRED

    public int Ay2024ExistingIntake { get; set; }
    public int Ay2024IncreaseIntake { get; set; }
    public int Ay2024TotalIntake { get; set; }

    public int Ay2025ExistingIntake { get; set; }
    public int Ay2025LopNmcIntake { get; set; }
    public int Ay2025TotalIntake { get; set; }

    public byte[] Ay2025NmcDocument { get; set; }
    public DateOnly? Ay2025LopDate { get; set; }

    public int Ay2026ExistingIntake { get; set; }
    public int Ay2026AddRequestedIntake { get; set; }
    public int Ay2026TotalIntake { get; set; }
    public List<AcademicIntake> IntakeData { get; set; }

    public Dictionary<string, List<AcademicIntake>> GroupedByCollege { get; set; }

}


public class AffiliationLopViewModel
{
    public int Id { get; set; }

    public string CollegeName { get; set; }
    public string CourseName { get; set; }

    public string Ay2025LopDocument { get; set; }
}
// SignupViewModel.cs
public class SignupViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; }

    [Required]
    [Phone]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Enter Password")]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Re-enter Password")]
    public string ConfirmPassword { get; set; }

    [Required]
    [Display(Name = "Designation / Role")]
    public string TypeofMember { get; set; }   // Dropdown value
}

// LoginViewModel.cs
public class LoginViewModel
{
    [Required]
    [Phone]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

// Views/Account/Auth.cshtml.cs or in Models folder
public class AuthViewModel
{
    // Signup fields
    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Please select your role")]
    public string TypeofMember { get; set; }  // Student / Company / Admin

    // Login fields
    [Required]
    [Display(Name = "Email / Phone Number")]
    public string LoginIdentifier { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string LoginPassword { get; set; }

    public bool RememberMe { get; set; }
}
public class AssignedCollegeDto
{
    public string CollegeName { get; set; }
    public string CollegeCode { get; set; }
    public string ACMember { get; set; }
    public string SenetMember { get; set; }
    public string SubjectExpert { get; set; }
}
public class InspectionCollegeItem
{
    public int? InspectionId { get; set; }          // if already saved
    public string CollegeName { get; set; }
    public string CollegeCode { get; set; }
    public DateTime? DateOfInspection { get; set; }
    public bool IsCompleted { get; set; }
    public IFormFile AttendenceDoc { get; set; }     // only for upload
    public string ExistingAttendanceDocPath { get; set; }  // if already uploaded
    // Optional: Kilometers, TotalCost, ModeOfTravel, etc. — if per-college
}
public class LICInspectionDetailsViewModel
{
    // Basic Details
    public int Id { get; set; }
    public string TypeofMember { get; set; }
    public string role { get; set; }
    public string Name { get; set; }
    public DateOnly? DOB { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string PANNumber { get; set; }
    public string AadhaarNumber { get; set; }

    // Bank Details
    public string AccountHolderName { get; set; }   // ✅ KEEP THIS
    public string AccountNumber { get; set; }
    public string IFSCCode { get; set; }
    public string BankName { get; set; }
    public string BranchName { get; set; }

    // Inspection Details
    public DateOnly? DateOfInspection { get; set; }
    public double? Kilometers { get; set; }
    public decimal? TotalCost { get; set; }
    public bool IsCompleted { get; set; }
    // In LICInspectionDetailsViewModel
    public IFormFile? AttendenceDoc { get; set; }

    public string? ModeOfTravel { get; set; }

    public string? Collegename { get; set; }

    public string FromPlace { get; set; }

    public string ToPlace { get; set; }
    public List<AssignedCollegeDto> AssignedColleges { get; set; } = new();
    public List<InspectionCollegeItem> Colleges { get; set; } = new();
    public List<SelectListItem> ModeOfTravelOptions { get; set; } = new();   // ← add this
    public string SelectedCollegeCode { get;  set; }
    public string? SelectedCollegeName { get;  set; }
    public List<SelectListItem> CollegeOptions { get; set; }
    public List<LicinspectionDetail> SavedInspections { get; set; } = new();


    public List<SelectListItem> FacultyOptions { get; set; } = new();
    public int? SelectedFacultyId { get; set; }
    public string SelectedFacultyName { get; set; }
}

public class LICClaimDetailsViewModel
{
    [Required(ErrorMessage = "Please select a mode of travel.")]
    public string? ModeOfTravel { get; set; }

    [Required(ErrorMessage = "From place is required.")]
    public string? FromPlace { get; set; }

    [Required(ErrorMessage = "To place is required.")]
    public string? ToPlace { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Kilometers must be a positive value.")]
    public double? Kilometers { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a positive value.")]
    public decimal? TotalCost { get; set; }

    // Dropdown options — not bound from POST
    public List<SelectListItem> ModeOfTravelOptions { get; set; } = new();
}
public class PendingInspectionVM
{
    public int Id { get; set; }

    public string CollegeName { get; set; }

    public DateTime DateOfInspection { get; set; }

    public bool IsCompleted { get; set; }

    public string? AttendanceFilePath { get; set; }
}
public class CompletedInspectionVM
{
    public int Id { get; set; }

    public string CollegeName { get; set; }

    public DateTime DateOfInspection { get; set; }

    public bool IsCompleted { get; set; }
}
public class ClaimVM
{
    public int Id { get; set; }

    public string CollegeName { get; set; }

    public DateTime DateOfInspection { get; set; }

    public string ModeOfTravel { get; set; }

    public string FromPlace { get; set; }

    public string ToPlace { get; set; }

    public double Kilometers { get; set; }

    public decimal TotalCost { get; set; }
}


public class LICDashboardViewModel
{
    public LICInspectionDetailsViewModel Profile { get; set; }

    public List<PendingInspectionVM> PendingInspections { get; set; }

    public List<CompletedInspectionVM> CompletedInspections { get; set; }

    public List<ClaimVM> Claims { get; set; }
}

