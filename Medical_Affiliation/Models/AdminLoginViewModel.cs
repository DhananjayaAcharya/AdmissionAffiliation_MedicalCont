using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Admission_Affiliation.Models
{
    public class AdminLoginViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public IEnumerable<SelectListItem>? Faculties { get; set; }
        public IEnumerable<SelectListItem>? Colleges { get; set; }
        //[Required]
        //public string FacultyId { get; set; }

        //[Required]
        //public string CollegeId { get; set; }
    }

    public class BulkCollegeStatusUpdateModel
    {
        public List<string> CollegeCodes { get; set; }

        public bool Status { get; set; }
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

    public class DocumentUploadViewModelForAdmin
    {
        public bool? IsDocsALlUploaded { get; set; }
        public string? CollegeCode { get; set; }

    }

    public class AddCollegeViewModel
    {
        public string? CollegeCode { get; set; }
        public string? CollegeName { get; set; }
        public string Place { get; set; }
        public int? RGUHSintake { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }

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
    }

    public class AddCoursePageViewModel
    {
        public string username { get; set; }
        public AddCourseViewModel CourseModel { get; set; }
        public List<SelectListItem> CollegeList { get; set; }
        public List<AddCourseViewModel> ExistingCourses { get; set; }
        public DeleteCourseViewModel DeleteCourseModel { get; set; }
    }

    public class AddCourseViewModel
    {
        public int id { get; set; }
        public int? RGUHSintake { get; set; }
        public string CollegeCode { get; set; }
        public string? CollegeName { get; set; }
        public string? CourseName { get; set; }
        public int? ExistingIntake { get; set; }
    }

    public class FacultyCollegeViewModel
    {
        public string? SelectedFacultyCode { get; set; }
        public string? SelectedCollegeCode { get; set; }
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

    public class FacultyAndCollege
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string? FacultyId { get; set; }
        public string? FacultyName { get; set; }
                         
        public string collegeName { get; set; }
        public string PrincipalName { get; set; }
        public List<SelectListItem>? FacultyList { get; set; }
        public List<SelectListItem>? CollegeList { get; set; }
        public List<collegeWiseReportViewModel> IntakeList { get; set; } = new List<collegeWiseReportViewModel>();
    }

    public class collegeWiseReportViewModel
    {
        public string CollegeName { get; set; }
        public string courseName { get; set; }
        public int? RGUSHSintake { get; set; }
        public int? Presentintake { get; set; }
        public string PrincipalName { get; set; }
        public string CollegeCode { get; set; }

    }
    public class CollegeVerificationViewModel
    {
        public string CollegeCode { get; set; }
        public byte[] AllDocsForCourse { get; set; }
        public string CollegeName { get; set; }
        public int FacultyCode { get; set; }
        public string FacultyName { get; set; }
        public string CourseCode { get; set; }

        public List<CollegeVerificationCourseViewModel> Courses { get; set; }
    }

    public class CollegeVerificationCourseViewModel
    {
        public int SlNo { get; set; }
        public int? CourseCode { get; set; }
        public string CourseName { get; set; }
        public int? SanctionedIntakeFirstYear { get; set; }
        //public int TotalAdmissionMade { get; set; }
        public int? RguhsIntake { get; set; }
        public int? PresentIntake { get; set; } = null;

        public string Remarks { get; set; } // extra column
        public object ViewDocument { get; set; }
    }
    public class CollegeFlagViewModel
    {
        public string FacultyId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public bool ShowNodalOfficerDetails { get; set; }
        public bool ShowIntakeDetails { get; set; }
        public bool ShowRepositoryDetails { get; set; }
    }
    public class FacultyBulkFlagUpdateModel
    {
        public string FacultyId { get; set; }
        public string FlagType { get; set; }
        public bool IsChecked { get; set; }
    }

    public class TabsToggleViewModel
    {
        public List<CollegeFlagViewModel> CollegeFlags { get; set; }
        public List<Faculty> FacultiesViewModel {  get; set; }
        public string? SelectedFacultyId { get; set; }
    }

    public class BulkFlagUpdateModel
    {
        public string FacultyId { get; set; }
        public string FlagType { get; set; } = null!;
        public bool IsChecked { get; set; }
    }
    public class AdminSectionOfficerCredentialsViewModel
    {
        public List<Faculty> Faculties { get; set; }
        public string FacultyName { get; set; }
        public string FacultyCode { get; set; }
        public string SelectedFacultyId { get; set; }
        public string SOusername { get; set; }
        public string SOpassword { get; set; }
        public bool IsActive { get; set; }
        public int UserId { get; set; }
        public List<SelectListItem> SectionOfficersList { get; set; }
    }


}
