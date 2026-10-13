using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class DentalSubmissionStatusAdminViewModel
    {
        public string? SelectedCourseLevel { get; set; }
        public string? SelectedCollegeCode { get; set; }

        public List<SelectListItem> CourseLevels { get; set; } = new();
        public List<SelectListItem> Colleges { get; set; } = new();

        public List<DentalSubmissionStatusAdminRowViewModel> Submissions
        {
            get;
            set;
        } = new();
    }

    public class DentalSubmissionStatusAdminRowViewModel
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; } = string.Empty;
        public string? CollegeName { get; set; }

        public string CourseLevel { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;

        public int AffiliationTypeId { get; set; }
        public string? AffiliationTypeName { get; set; }

        public string ApplicationNumber { get; set; } = string.Empty;

        public DateTime? SubmittedDate { get; set; }

        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
    }
}