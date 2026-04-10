//page created by ram on 06-11-2025

namespace Medical_Affiliation.Models
{
    public class AHS_UGPGSSDetailsFormViewModel
    {
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        public List<CourseDropdownVM> UGCourses { get; set; } = new();
        public List<CourseDropdownVM> PGCourses { get; set; } = new();
        public List<CourseDropdownVM> SSCourses { get; set; } = new();

        public List<AHSUGDetailsViewModel> UGDetailsList { get; set; } = new();
        public List<AHSPGDetailsViewModel> PGDetailsList { get; set; } = new();
        public List<SSDetailsViewModel> SSDetailsList { get; set; } = new();
    }
}
