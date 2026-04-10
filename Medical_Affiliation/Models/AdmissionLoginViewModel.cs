using Admission_Affiliation.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class AdmissionLoginViewModel
{
    [Required]
    public string FacultyId { get; set; }

    [Required]
    public string CollegeId { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    public string Captcha { get; set; }

    public string CaptchaCode { get; set; }

    public IEnumerable<SelectListItem>? Faculties { get; set; }
    public IEnumerable<SelectListItem>? Colleges { get; set; }
}
public class FacultyViewModel
{
    public string FacultyId { get; set; }

    public string FacultyName { get; set; } = null!;

    public string? EmsFacultyId { get; set; }

    public string? FacultyAbbre { get; set; }

    public string? Status { get; set; }
}

public class AddCollegeViewModel
{
    public string? CollegeCode { get; set; }
    public string? CollegeName { get; set; }
    public string? editCollegeName { get; set; }
    public string Place { get; set; }
    public int? RGUHSintake { get; set; }
    public string Password { get; set; }
    public string NewPassword { get; set; }
    public string FacultyCode { get; set; }
    public List<SelectListItem> FacultyList { get; set; } = new();

}

public class DeleteCollegeViewModel
{
    public string? CollegeCode { get; set; }
    public string? CollegeName { get; set; }
}

public class DeleteCourseViewModel
{
    public string CollegeCode { get; set; }
    public string CourseName { get; set; }
    public string CourseCode { get; set; }
}

public class AddCoursePageViewModel
{
    public string username { get; set; }
    public int? SelectedFacultyId { get; set; }   // selected faculty
    public List<SelectListItem> FacultyList { get; set; }
    public AddCourseViewModel CourseModel { get; set; }
    public List<SelectListItem> CollegeList { get; set; }
    public List<AddCourseViewModel> ExistingCourses { get; set; }
    public DeleteCourseViewModel DeleteCourseModel { get; set; }
    public List<SelectListItem> CourseList { get; set; }
    public List<SelectListItem> CoursePrefixList { get; set; }
    public List<SelectListItem> CourseLevelList { get; set; }
}

public class AddCourseViewModel
{
    public int Id { get; set; }
    //public int? RGUHSintake { get; set; }
    public string CollegeCode { get; set; }
    public string? CollegeName { get; set; }
    public string? CourseName { get; set; }
    public int? ExistingIntake { get; set; }
    public int CourseCode { get; set; }
    public int FacultyId { get; set; }
    public int? CollegeFacultyId { get; set; }
    public string CourseLevel { get; set; }
    public string CoursePrefix { get; set; }
    public string SubjectName { get; set; }
}

public class FacultyCollegeViewModel
{
    public string? SelectedFacultyCode { get; set; }


    public string? SelectedCollegeCode { get; set; }

    public int CollegesCount { get; set; }

    public List<SelectListItem>? FacultyList { get; set; }
    public List<SelectListItem>? CollegeList { get; set; }

    public List<CollegeCourseReport>? ReportData { get; set; }
}

public class CollegeCourseReport
{
    public string CourseName { get; set; }
    public int ExistingIntake { get; set; }
    public int PresentIntake { get; set; }
    public string? CollegeName { get; set; }
    public string CollegeCode { get; set; }
}

public class AdminDocumentReport
{
    public string FacultyId { get; set; }
    public string FacultyName { get; set; }
    public string CollegeName { get; set; }

    public string selectedCollegeName { get; set; }
    public List<SelectListItem>? FacultyList { get; set; }
    public List<SelectListItem>? CollegeList { get; set; }

    public List<CourseDetailForAdmin>? Courses { get; set; } = new List<CourseDetailForAdmin>();
}

public class AdminDocumentIncompleteReport
{
    public string FacultyId { get; set; }
    public string FacultyName { get; set; }
    public string CollegeName { get; set; }

    public string selectedCollegeName { get; set; }
    public List<SelectListItem>? FacultyList { get; set; }
    public List<SelectListItem>? CollegeList { get; set; }

    public byte[]? DocumentByte3 { get; set; }
    public IFormFile? AllDocs { get; set; }

    public List<CourseDetailForAdminIncomplete>? Courses { get; set; } = new List<CourseDetailForAdminIncomplete>();
}
public class CollegeWithFlagsDto
{
    public string Value { get; set; }    // CollegeCode
    public string Text { get; set; }     // CollegeName
    public bool IsPresentIntakeMissing { get; set; }
    public bool IsDocumentLopMissing { get; set; }
}


public class CourseDetailForAdminIncomplete
{
    public string? CourseName { get; set; }
    public string? FacultyId { get; set; }
    public string? FacultyName { get; set; }

    public string? CollegeName { get; set; }
    public int? ExistingIntake { get; set; }
    public int? PresentIntake { get; set; }
    public IFormFile? Document1 { get; set; }
    public IFormFile? Document2 { get; set; }
    public bool? IsDocument1Available { get; set; }

    public IFormFile? DocumentFile1 { get; set; }
    public IFormFile? DocumentFile2 { get; set; }

    public byte[]? DocumentByte1 { get; set; }
    public byte[]? DocumentByte2 { get; set; }



}
public class CollegeDocsReportViewModel
{
    public string CollegeCode { get; set; }
    public string CollegeName { get; set; }
    public string CollegeTown { get; set; }
    public byte[] AllDocsForCourse { get; set; }
    public string PrincipalNameDeclared { get; set; }
    public string selectedCollegeName { get; set; }
    public List<SelectListItem>? FacultyList { get; set; }
    public List<SelectListItem>? CollegeList { get; set; }
}
public class PrintReportViewModel
{
    public string FacultyCode { get; set; }
    public string CollegeCode { get; set; }
    public string CourseLevel { get; set; }

    public List<SelectListItem> FacultyList { get; set; } = new();
    public List<SelectListItem> CollegeList { get; set; } = new();
    public List<SelectListItem> CourselevelList { get; set; } = new();

    public List<CollegeDocsReportViewModel> ReportData { get; set; } = new();
    public string FacultyAndCollege { get; set; }
}
public class FacultyAndCollege
{
    public string CollegeCode { get; set; }
    public string CollegeName { get; set; }
    public int FacultyId { get; set; }
    public string? FacultyName { get; set; }

    public string CollegeTown { get; set; }
    public string PrincipalName { get; set; }
    public string FacultyCode { get; set; }   // ✅ for binding in form
    public string CourseLevel { get; set; }   // ✅ for binding in form
    public string CourseName { get; set; }
    public List<SelectListItem>? FacultyList { get; set; }
    public List<SelectListItem>? CollegeList { get; set; }
    public List<SelectListItem>? CourseList { get; set; }
    public List<SelectListItem>? CoursesData { get; set; }
    public List<collegeWiseReportViewModel> IntakeList { get; set; } = new List<collegeWiseReportViewModel>();
}

public class collegeWiseReportViewModel
{
    public string CollegeName { get; set; }
    public string courseName { get; set; }
    public string subjectName { get; set; }
    public int? RGUSHSintake { get; set; }
    public int? Presentintake { get; set; }
    public string PrincipalName { get; set; }
    public string CollegeCode { get; set; }
    public string SubjectCode { get; set; }

}
public class CollegeVerificationViewModel
{
    public string CollegeCode { get; set; }
    public byte[] AllDocsForCourse { get; set; }
    public IFormFile AllDocsForCourse1 { get; set; }

    public string CollegeName { get; set; }
    public int FacultyCode { get; set; }
    public string FacultyName { get; set; }
    public string CollegeTown { get; set; }
    public List<CollegeVerificationCourseViewModel> Courses { get; set; }
}

public class CollegeVerificationCourseViewModel
{
    public int SlNo { get; set; }
    public string? CourseCode { get; set; }
    public string CourseName { get; set; }
    public int? SanctionedIntakeFirstYear { get; set; }
    //public int TotalAdmissionMade { get; set; }
    public int? RguhsIntake { get; set; }
    public int? PresentIntake { get; set; } = null;

    public string Remarks { get; set; } // extra column
    public object ViewDocument { get; set; }
}


public class CourseDetailForAdmin
{
    public string? CourseName { get; set; }
    public string CollegeName { get; set; }
    public string FacultyName { get; set; }
    public int? ExistingIntake { get; set; }
    public int? PresentIntake { get; set; }
    public IFormFile? Document1 { get; set; }
    public IFormFile? Document2 { get; set; }
    public bool? IsDocument1Available { get; set; }

    public IFormFile? DocumentFile1 { get; set; }
    public IFormFile? DocumentFile2 { get; set; }

    public byte[]? DocumentByte1 { get; set; }
    public byte[]? DocumentByte2 { get; set; }

}

public class CollegeCourseForAdminViewModel
{
    public string? CollegeCode { get; set; }
    public string? CollegeName { get; set; }
    public List<CourseDetailForAdmin>? Courses { get; set; } = new List<CourseDetailForAdmin>();
    public IFormFile? DocumentFile { get; set; } // for uploading
    public byte[]? DocumentFileContent { get; set; } // for serving the file

    public bool? DeclarationAccepted { get; set; }
    public string? PrincipalName { get; set; }
}


public class ChangePasswordViewModel
{
    public string? CollegeCode { get; set; }         // Username
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmNewPassword { get; set; }
}
public class InstituteUpdateViewModel
{
    [Required(ErrorMessage = "Principal Name is required.")]
    [StringLength(100)]
    public string PrincipalName { get; set; }

    [Required(ErrorMessage = "Principal Mobile Number is required.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number.")]
    public string PrincipalMobileNo { get; set; }

    [Required(ErrorMessage = "College Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string CollegeEmail { get; set; }

    [Required(ErrorMessage = "College Mobile Number is required.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number.")]
    public string CollegeMobileNo { get; set; }
}
public class BuildingDetailsViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Land in Acres")]
    public decimal LandInAcres { get; set; }

    [Required]
    [Display(Name = "Total Area of Building (Sq.ft)")]
    public decimal TotalAreaBuildingSqft { get; set; }

    [Required]
    [Display(Name = "Building Type")]
    public string BuildingType { get; set; }

    [Required]
    [Display(Name = "Building Owner Name")]
    public string BuildingOwnerName { get; set; }

    [Required]
    [Display(Name = "Any Court Case Pending")]
    public bool IsCourtCasePending { get; set; }

    [Required]
    [Display(Name = "Courses Imparted Here")]
    public bool AreCoursesImpartedHere { get; set; }
    public string FacultyCode { get; set; }

    [Required]
    public string CollegeCode { get; set; }

    [Required]
    [Display(Name = "Number of Classrooms")]
    public int NumberOfClassrooms { get; set; }

    [Required]
    [Display(Name = "Number of Labs")]
    public int NumberOfLabs { get; set; }

    [Required]
    [Display(Name = "Safe Drinking Water Available")]
    public bool IsSafeDrinkingWaterAvailable { get; set; }

    [Required]
    [Display(Name = "Office Facility Available")]
    public bool IsOfficeFacilityAvailable { get; set; }

    [Required]
    [Display(Name = "Electricity Available")]
    public bool IsElectricityAvailable { get; set; }

    [Required]
    [Display(Name = "Auditorium Available")]
    public bool IsAuditoriumAvailable { get; set; }

    [Required]
    [Display(Name = "Seating Capacity Available")]
    public bool IsSeatingCapacityAvailable { get; set; }

    [Required]
    [Display(Name = "Survey Number / PID")]
    public string SurveyNumberOrPid { get; set; }

    [Required]
    [Display(Name = "RR Number")]
    public string RrNumber { get; set; }

    // Uploaded Documents (PDFs) as byte arrays via IFormFile
    [Display(Name = "Court Case Document")]
    public IFormFile CourtCaseDocument { get; set; }

    [Display(Name = "Blueprint Completion Certificate")]
    public IFormFile BluePrintCompletionCertificate { get; set; }

    [Display(Name = "Blueprint Certificate Number")]
    public string BluePrintCertNo { get; set; }

    [Display(Name = "EC Encumbrance Certificate")]
    public IFormFile ECEncumbranceCertificate { get; set; }

    [Display(Name = "EC Certificate Number")]
    public string ECCertNo { get; set; }

    [Display(Name = "Tax Paid Certificate")]
    public IFormFile TaxPaidCertificate { get; set; }

    [Display(Name = "Tax Certificate Number")]
    public string TaxCertNo { get; set; }

    public IFormFile RTCOfLand { get; set; }

    public string RTCCertNo { get; set; }

    public IFormFile OccupancyCertificate { get; set; }

    public string OccupancyCertNo { get; set; }


    public IFormFile SaleDeedCertificate { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public string? ExistingCourtCaseDocument { get; set; }
    public string? ExistingBluePrintCompletionCertificate { get; set; }
    public string? ExistingECEncumbranceCertificate { get; set; }
    public string? ExistingTaxPaidCertificate { get; set; }
    public string? ExistingRTCOfLand { get; set; }
    public string? ExistingOccupancyCertificate { get; set; }
    public string? ExistingSaleDeedCertificate { get; set; }

}
public class TeachingFacultyViewModel
{
    public string FacultyCode { get; set; } = null!;
    public string Faculty { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string DesignationCode { get; set; } = null!;
    public string DesignationName { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;
    public string DepartmentCode { get; set; } = null!;


    public string SeatSlabId { get; set; } = null!;

    public string ExistingSeatIntake { get; set; } = null!;
    public string PresentSeatIntake { get; set; } = null!;

}
public class YearwiseMaterialViewModel
{
    public int Id { get; set; }

    public string CollegeCode { get; set; }
    public string FacultyCode { get; set; }

    public int ParametersId { get; set; }
    public string ParametersName { get; set; }

    public string Year1 { get; set; }
    public string Year2 { get; set; }
    public string Year3 { get; set; }

    //code added by ram on 06-11-2025

    public int? AffiliatedHospitalId { get; set; }

    public int? ParentHospitalId { get; set; }
}

//code added by ram on 07-11-2025

//public class FacultyIntakeViewModel
//{
//    //[Required]
//    //public string AffiliationType { get; set; } // Fresh/Additional/Increase

//    [Required]
//    public string CourseLevel { get; set; } // from Mst_Course.CourseLevel

//    [Required]
//    public string CourseCode { get; set; } // saved to DB
//    public string IntakeDetails { get; set; }
//    [Required]
//    public string FreshOrIncrease { get; set; }    // If required


//    // For display only - optional
//    //public string CourseName { get; set; }

//    // Uploads as IFormFile in view model; will convert to byte[] in controller
//    public IFormFile RGUHSNotificationFile { get; set; }
//    public IFormFile INCUploadFile { get; set; }
//    public IFormFile KNMCUploadFile { get; set; }
//    public IFormFile GOKUploadFile { get; set; }

//}
public class Ayurveda_FacultyIntakeViewModel
{
    //[Required]
    //public string AffiliationType { get; set; } // Fresh/Additional/Increase

    [Required]
    public string CourseLevel { get; set; } // from Mst_Course.CourseLevel

    [Required]
    public string CourseCode { get; set; } // saved to DB
    public string IntakeDetails { get; set; }
    [Required]
    public string FreshOrIncrease { get; set; }    // If required


    // For display only - optional
    //public string CourseName { get; set; }

    // Uploads as IFormFile in view model; will convert to byte[] in controller
    public IFormFile RGUHSNotificationFile { get; set; }
    public IFormFile GOKUploadFile { get; set; }
    public IFormFile NciscUploadFile { get; set; }
}

public class FacultyIntakeViewModel
{
    //[Required]
    //public string AffiliationType { get; set; } // Fresh/Additional/Increase

    [Required]
    public string CourseLevel { get; set; } // from Mst_Course.CourseLevel

    [Required]
    public string CourseCode { get; set; } // saved to DB
    public string IntakeDetails { get; set; }
    [Required]
    public string FreshOrIncrease { get; set; }    // If required


    // For display only - optional
    //public string CourseName { get; set; }

    // Uploads as IFormFile in view model; will convert to byte[] in controller
    public IFormFile RGUHSNotificationFile { get; set; }
    public IFormFile INCUploadFile { get; set; }
    public IFormFile KNMCUploadFile { get; set; }
    public IFormFile GOKUploadFile { get; set; }
}
    public class CollegeReportDto
    {
    public int Id { get; set; }
    public string Designation { get; set; }
    public string Department { get; set; }
    public string RequiredIntake { get; set; }
    public string AvailableIntake { get; set; }
}



public class AcademicIntakePageViewModel1
{
    public string CollegeCode { get; set; }
    public string FacultyCode { get; set; }
    public string CollegeName { get; set; }
    public int FacultyId { get; set; }

    // UG, PG, SS Courses - populated from CollegeCourseIntakeDetails + MstCourses
    public List<IntakeByLevelViewModel1> UgCourses { get; set; } = new List<IntakeByLevelViewModel1>();
    public List<IntakeByLevelViewModel1> PgCourses { get; set; } = new List<IntakeByLevelViewModel1>();
    public List<IntakeByLevelViewModel1> SsCourses { get; set; } = new List<IntakeByLevelViewModel1>();

    // Existing saved data from AcademicIntake table
    public List<SavedAcademicIntakeRowViewModel1> SavedIntakes { get; set; } = new List<SavedAcademicIntakeRowViewModel1>();

}

// ✅ FULLY CORRECTED - Individual course row for input forms
public class IntakeByLevelViewModel1
{
    public string CourseCode { get; set; }
    public string CourseName { get; set; }

    // 2024-25 Data - NULLABLE INTS ✅
    [Display(Name = "AY2024 Existing Intake")]
    public int? AY2024_ExistingIntake { get; set; }

    [Display(Name = "AY2024 Increase Intake")]
    [Range(0, int.MaxValue, ErrorMessage = "Increase intake must be 0 or positive")]
    public int? AY2024_IncreaseIntake { get; set; }

    [Display(Name = "AY2024 Total Intake")]
    [Range(0, int.MaxValue, ErrorMessage = "Total intake must be 0 or positive")]
    public int? AY2024_TotalIntake { get; set; }

    // 2025-26 Data - NULLABLE INTS ✅
    [Display(Name = "AY2025 Existing Intake")]
    public int? AY2025_ExistingIntake { get; set; }

    [Display(Name = "AY2025 LoP/NMC Intake")]
    [Range(0, int.MaxValue, ErrorMessage = "LoP/NMC intake must be 0 or positive")]
    public int? AY2025_LopNmcIntake { get; set; }

    [Display(Name = "AY2025 Total Intake")]
    [Range(0, int.MaxValue, ErrorMessage = "Total intake must be 0 or positive")]
    public int? AY2025_TotalIntake { get; set; }

    // 2025-26 Documents ✅
    [Display(Name = "AY2025 NMC Document")]
    public IFormFile? AY2025_NmcDocument { get; set; }  // File upload

    [Display(Name = "Current NMC Document")]
    public string? AY2025_NmcDocumentUrl { get; set; }  // Existing file URL

    [Display(Name = "AY2025 LoP Date")]
    public DateOnly? AY2025_LopDate { get; set; }  // Matches controller

    [Display(Name = "Current LoP Document")]
    public string? AY2025_LopDocumentUrl { get; set; }

    // 2026-27 Data - NULLABLE INTS ✅
    [Display(Name = "AY2026 Existing Intake")]
    public int? AY2026_ExistingIntake { get; set; }

    [Display(Name = "AY2026 Add Requested Intake")]
    [Range(0, int.MaxValue, ErrorMessage = "Requested intake must be 0 or positive")]
    public int? AY2026_AddRequestedIntake { get; set; }

    [Display(Name = "AY2026 Total Intake")]
    [Range(0, int.MaxValue, ErrorMessage = "Total intake must be 0 or positive")]
    public int? AY2026_TotalIntake { get; set; }
}

// Saved data display row (ENHANCED)
public class SavedAcademicIntakeRowViewModel1
{
    public int Id { get; set; }
    public string CourseLevel { get; set; }
    public string CourseCode { get; set; }
    public string CourseName { get; set; }

    [Display(Name = "AY2024 Total")]
    public int AY2024_TotalIntake { get; set; }

    [Display(Name = "AY2025 Total")]
    public int AY2025_TotalIntake { get; set; }

    [Display(Name = "AY2026 Total")]
    public int AY2026_TotalIntake { get; set; }

    public string PrincipalName { get; set; }
    public bool HasNmcDocument { get; set; }
    public bool HasLopDocument { get; set; }
    public DateTime CreatedOn { get; set; }

    // Helper properties for display
    public string NmcDocumentDisplay => HasNmcDocument ? "✅ Available" : "❌ None";
    public string LopDocumentDisplay => HasLopDocument ? "✅ Available" : "❌ None";
}


public class NonTeachingStaffItemVm
{
    public string Name { get; set; }

    public string Designation { get; set; }

    public string MobileNumber { get; set; }

    // BIT fields → bool?
    public decimal? SalaryPaid { get; set; }
    public bool? PfProvided { get; set; }
    public bool? EsiProvided { get; set; }
    public bool? ServiceRegisterMaintained { get; set; }
    public bool? SalaryAcquaintanceRegister { get; set; }
}

public class NonTeachingStaffVm
{
    public decimal? StaffId { get; set; }

    public string CollegeCode { get; set; }
    public string FacultyCode { get; set; }

    // Dropdown source
    public List<SelectListItem> DesignationList { get; set; } = new();

    // Multiple staff entries
    public List<NonTeachingStaffItemVm> StaffList { get; set; } = new();
}