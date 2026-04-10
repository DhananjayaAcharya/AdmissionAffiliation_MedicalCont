using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class AHSSOloginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string username {  get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }
    }

    public class AHSSOupdateIntakeDetails
    {
        public IEnumerable<SelectListItem>? Faculties { get; set; }
        public IEnumerable<SelectListItem>? Colleges { get; set; }

        public List<CollegeCourseIntakeDetail>? UploadedColleges { get; set; }
        public string? CollegeCode { get; set; }
        public string? CollegeName { get; set; }
        public string CollegeTown { get; set; }

        public string? FacultyCode { get; set; }

        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        
        public byte[] allCoursesBytes { get; set; }
        public IFormFile? allCoursesFile { get; set; }
        public string principalName { get; set; }
        public bool isRguhsNotification {  get; set; }
        public bool? DeclarationAccepted { get; set; }
        public string? PrincipalName { get; set; }
        public string? CollegeAddress { get; set; }
        public List<AHScourseIntakeViewModel> AHScourseIntakeViewModel { get; set; } = new List<AHScourseIntakeViewModel>();
    }
    public class AHScourseIntakeViewModel()
    {
        public string? CollegeCode { get; set; }
        //public int? ExistingIntake { get; set; }
        public string? CollegeName { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? FacultyName { get; set; }
        public string? FacultyCode { get; set; }

        public int? SanctionedIntake { get; set; }
        public byte[] notificationBytes { get; set; }
        public IFormFile? notificationFile { get; set; }
        public bool isNotificationDoc {  get; set; }
        public string? CourseLevel { get; set; }
        public string? CoursePrefix { get; set; }
        
    }

    //code added by ram on 07-11-2025
    public class NursingInstituteDetailViewModel
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; } = null!;

        public string FacultyCode { get; set; } = null!;

        public string InstituteName { get; set; } = null!;

        public string InstituteAddress { get; set; } = null!;

        public string? TrustSocietyName { get; set; }

        public DateOnly? YearOfEstablishmentOfTrust { get; set; }

        public DateOnly? YearOfEstablishmentOfCollege { get; set; }

        public string? InstitutionType { get; set; }

        public string? HodofInstitution { get; set; }

        public DateOnly? Dob { get; set; }

        public string? Age { get; set; }

        public string? TeachingExperience { get; set; }

        public List<string> Degree { get; set; } = new List<string>();
        //public string Degree { get; set; }

        public string? CourseSelectedSpecialities { get; set; }

        public string Qualifications { get; set; } = null!;

        public string HighestQualification { get; set; } = null!;
        public string? CourseCode { get; set; }
        public bool IsReadOnly { get; set; }

        public string? OtherDegreeText { get; set; }

        public IFormFile? TrustEstablishmentDocument { get; set; }
        public IFormFile? CollegeEstablishmentDocument { get; set; }

        // ⭐ View fields (byte[])
        public byte[]? TrustDocData { get; set; }
        public byte[]? EstablishmentDocData { get; set; }

        public string SelectedDistrictId { get; set; }
        public string SelectedTalukId { get; set; }
        public List<SelectListItem> DistrictDropdownList { get; set; } = new();
        public List<SelectListItem> TalukDropdownList { get; set; } = new();
    }
}
