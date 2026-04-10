using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class MedicalInstituteDetailViewModel
    {
        public int Id { get; set; }

        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }

        [Required(ErrorMessage = "Institute Name is required")]
        [StringLength(200)]
        public string InstituteName { get; set; }

        public string CollegeAddress { get; set; }

        public string SelectedDegree { get; set; }
        public string SelectedSpecialization { get; set; }


        [Required(ErrorMessage = "Trust/Society Name is required")]
        [StringLength(200)]
        public string TrustSocietyName { get; set; }

        [Required(ErrorMessage = "Year of Establishment (Trust) is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter a valid year (e.g., 2001)")]
        public string YearOfEstablishmentOfTrust { get; set; }

        [Required(ErrorMessage = "Year of Establishment (College) is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter a valid year (e.g., 2005)")]
        public string YearOfEstablishmentOfCollege { get; set; }

        [Required(ErrorMessage = "Institution Type is required")]
        public string InstitutionType { get; set; }

        [Required(ErrorMessage = "HOD Name is required")]
        [StringLength(150)]
        public string HODOfInstitution { get; set; }

        //[Required(ErrorMessage = "Faculty Name is required")]
        //[StringLength(150)]
        //public string FacultyName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateOnly DOB { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(21, 100, ErrorMessage = "Age must be between 21 and 100")]
        public string Age { get; set; }

        [Required(ErrorMessage = "Teaching Experience is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Enter valid years of experience")]
        public string TeachingExperience { get; set; }

        [Required(ErrorMessage = "PG Degree is required")]
        public int SelectedPGCourseId { get; set; }
        public IEnumerable<SelectListItem>? PGCourses { get; set; }

        [Required(ErrorMessage = "Please select at least one speciality.")]
        public string? SelectedQualification { get; set; }


        // For displaying checkbox options
        public List<SelectListItem>? SpecialityOptions { get; set; }

        public List<SelectListItem> DegreeList {  get; set; }
        public List<SelectListItem> SpecializationList { get; set; }

        //code added by ram on 06-11-2025
        public string? SelectedDistrictId { get; set; }
        public string? SelectedTalukId { get; set; }
        public List<SelectListItem> DistrictDropdownList { get; set; } = new();
        public List<SelectListItem> TalukDropdownList { get; set; } = new();
        public string? InstituteAddress { get; set; } 
    }
    public class Medical_CourseViewModel
    {
        // UG_CourseDetails fields
        public int ID { get; set; }
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }
        public string CourseCode { get; set; }
        public string FreshOrIncrease { get; set; }
        public DateTime? FirstLOPDate { get; set; }
        public int? NoOfSeats { get; set; }
        public int? PermittedYear { get; set; }
        public int? RecognizedYear { get; set; }

        // File uploads
        public IFormFile RGUHSNotificationFile { get; set; }
        public IFormFile GMCFile { get; set; }
        public IFormFile NMCFile { get; set; }

        // Dropdown for courses
        public List<SelectListItem> CoursesDropdown { get; set; } = new List<SelectListItem>();


        // List of existing courses for display
        //public List<MstCourse> ExistingCourses { get; set; } = new List<MstCourse>();

        public List<MedicalCourseDetail> ExistingCourses { get; set; } = new();
    }
    public class ClinicalData
    {
        public int ID { get; set; }
        public string CollegeCode { get; set; } = string.Empty;
        public int FacultyCode { get; set; } 
        public int ParametersId { get; set; }
        public string Parameters_Name { get; set; } = string.Empty;
        public DateTime Year1 { get; set; }
        public DateTime Year2 { get; set; }
        public DateTime Year3 { get; set; }
    }
    public class FacultyDetailsViewModel
    {

        public int FacultyDetailId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string NameOfFaculty { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Select Yes or No")]
        public string RecognizedPGTeacher { get; set; }

        [Required(ErrorMessage = "Select Yes or No")]
        public string RecognizedPhDTeacher { get; set; }

        [Required(ErrorMessage = "Select Yes or No")]
        public string LitigationPending { get; set; }

        [Required(ErrorMessage = "Mobile is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be 10 digits")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "PAN is required")]
        //[RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format")]
        public string PAN { get; set; }

        [Required(ErrorMessage = "Aadhaar is required")]
        //[RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar must be 12 digits")]
        public string Aadhaar { get; set; }
        public string? DepartmentDetail { get; set; }
        public List<SelectListItem>? Subjects { get; set; }
        public List<SelectListItem>? Designations { get; set; }

        public string SelectedDepartment { get; set; }  // <-- This stores the selected department
        public List<SelectListItem>? DepartmentDetails { get; set; }
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? InstituteName { get; set; }

        public IFormFile? GuideRecognitionDoc { get; set; }

        public IFormFile? PhDRecognitionDoc { get; set; }
        public IFormFile? LitigationDoc { get; set; }

        public byte[]? PGRecognitionDocData { get; set; }
        public byte[]? PhDRecognitionDocData { get; set; }
        public byte[]? LitigationDocData { get; set; }

        public string? IsExaminer { get; set; }   // Yes / No

        // Stores selected options like "UG,PG" in DB
        public string? ExaminerFor { get; set; }

        // Used for binding checkbox list in GET/POST
        public List<string>? ExaminerForList { get; set; }

        public string? RemoveRemarks { get; set; }

        public bool IsRemoved { get; set; }





    }
    public class CourseDropdownVM
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
    }

    public class UGDetailsViewModel
    {
        public int Id { get; set; }
        public string UGIntake { get; set; }
        public string Course { get; set; }
        public string FreshOrIncrease { get; set; }
        public DateOnly? FirstLOPDate { get; set; }
        public string NumberOfSeats { get; set; }
        public string PermittedYear { get; set; }
        public string RecognizedYear { get; set; }

        public IFormFile RGUHSNotificationFile { get; set; }
        public IFormFile GMCFile { get; set; }
        public IFormFile NMCFile { get; set; }

        // Existing file paths
        public string Rguhsnotification { get; set; }
        public string Gmc { get; set; }
        public string Nmc { get; set; }
        public List<SelectListItem> UGIntakeOptions { get; set; } = new();

        //code added bby ram on 06-11-2025

        public IFormFile KSNCFile { get; set; }

        //code added by ram on 07-11-2025

        public IFormFile Gok { get; set; }

        public byte[]? GokData { get; set; }

        public byte[]? GMCData { get; set; }
        public byte[]? NMCData { get; set; }

        public byte[]? RGUHSNotificationData { get; set; }

    }

    public class PGDetailsViewModel
    {
        public int Id { get; set; }
        public string PGIntake { get; set; }
        public string Course { get; set; }
        public string FreshOrIncrease { get; set; }
        public DateOnly? FirstLOPDate { get; set; }
        public string NumberOfSeats { get; set; }
        public string PermittedYear { get; set; }
        public string RecognizedYear { get; set; }

        public IFormFile RGUHSNotificationFile { get; set; }
        public IFormFile GMCFile { get; set; }
        public IFormFile NMCFile { get; set; }

        // Existing file paths
        public string Rguhsnotification { get; set; }
        public string Gmc { get; set; }
        public string Nmc { get; set; }

        //code added by ram on 06-11-2025

        public IFormFile KSNCFile { get; set; }

        //code added by ram on 07-11-2025

        public IFormFile Gok { get; set; }

        public byte[]? GokData { get; set; }

        public byte[]? GMCData { get; set; }
        public byte[]? NMCData { get; set; }

        public byte[]? RGUHSNotificationData { get; set; }


    }

    public class SSDetailsViewModel
    {
        public int Id { get; set; }
        public string SSIntake { get; set; }
        public string Course { get; set; }
        public string FreshOrIncrease { get; set; }
        public DateOnly? FirstLOPDate { get; set; }
        public string NumberOfSeats { get; set; }
        public string PermittedYear { get; set; }
        public string RecognizedYear { get; set; }

        public IFormFile RGUHSNotificationFile { get; set; }
        public IFormFile GMCFile { get; set; }
        public IFormFile NMCFile { get; set; }

        // Existing file paths
        public string Rguhsnotification { get; set; }
        public string Gmc { get; set; }
        public string Nmc { get; set; }
    }
    public class UGPGSSDetailsFormViewModel
    {
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }

        
        public List<SSDetailsViewModel> SSDetailsList { get; set; } = new();

        public List<CourseDropdownVM> UGCourses { get; set; } = new();
        public List<CourseDropdownVM> PGCourses { get; set; } = new();
        public List<CourseDropdownVM> SSCourses { get; set; } = new();

        //code added by ram on 06-11-2025

        public List<UGDetailsViewModel> UGDetailsList { get; set; } = new();
        public List<PGDetailsViewModel> PGDetailsList { get; set; } = new();

        // CHANGE THESE:
        //public List<AHSUGDetailsViewModel> UGDetailsList { get; set; } = new();
        //public List<AHSPGDetailsViewModel> PGDetailsList { get; set; } = new();
    }

}
    public class CollegeDesignationDetailViewModel
    {
        public int Id { get; set; }
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public string Designation { get; set; }
        public int RequiredIntake { get; set; }
        public int AvailableIntake { get; set; }

        // Dropdown
        public IEnumerable<SelectListItem> Designations { get; set; }
    }

    //code added by ram on 06-11-2025

    public class AHSUGDetailsViewModel
    {
        public int Id { get; set; }
        public string UGIntake { get; set; }
        public string Course { get; set; }
        public string FreshOrIncrease { get; set; }
        public DateOnly? FirstLOPDate { get; set; }
        public string NumberOfSeats { get; set; }
        public string PermittedYear { get; set; }
        public string RecognizedYear { get; set; }

        public IFormFile RGUHSNotificationFile { get; set; }
        public IFormFile GMCFile { get; set; }
        public IFormFile NMCFile { get; set; }

        public IFormFile KSNCFile { get; set; }

        public byte[] Rguhsnotification { get; set; }        // For showing existing file

        public byte[] Gmc { get; set; }

        public byte[] Nmc { get; set; }

        public byte[] ksnc { get; set; }

        public List<SelectListItem> UGIntakeOptions { get; set; } = new();

        




    }

    public class AHSPGDetailsViewModel
    {
        public int Id { get; set; }
        public string PGIntake { get; set; }
        public string Course { get; set; }
        public string FreshOrIncrease { get; set; }
        public DateOnly? FirstLOPDate { get; set; }
        public string NumberOfSeats { get; set; }
        public string PermittedYear { get; set; }
        public string RecognizedYear { get; set; }

        public IFormFile RGUHSNotificationFile { get; set; }
        public IFormFile GMCFile { get; set; }
        public IFormFile NMCFile { get; set; }


        public byte[] Rguhsnotification { get; set; }        // For showing existing file

        public byte[] Gmc { get; set; }

        public byte[] Nmc { get; set; }

        public IFormFile KSNCFile { get; set; } // newly added

        public byte[] ksnc { get; set; }

    }

public class CourseExamStatsViewModel
{
    public string CourseName { get; set; }
    public int Year { get; set; }
    public int ExamAppearedOut { get; set; }
    public int PassedOutCount { get; set; }
    public string? Percentage { get; set; }
    public string? ExamResults { get; set; }
}


