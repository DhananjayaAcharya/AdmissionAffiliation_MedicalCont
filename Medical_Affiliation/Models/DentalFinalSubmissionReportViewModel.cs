using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class DentalFinalSubmissionReportViewModel
    {
        // Dashboard summary
        public int TotalColleges { get; set; }
        public int SubmittedColleges { get; set; }
        public int TotalSubmissions { get; set; }

        // Selected filters
        public string? AcademicYear { get; set; }
        public string? CourseLevel { get; set; }
        public int? AffiliationTypeId { get; set; }

        // Filter dropdown options
        public List<SelectListItem> AcademicYears { get; set; } = new();
        public List<SelectListItem> CourseLevels { get; set; } = new();
        public List<SelectListItem> AffiliationTypes { get; set; } = new();
        public List<DentalSubmissionReportRowViewModel> Submissions { get; set; } = new();

    }

    public class DentalSubmissionReportRowViewModel
    {
        public string CollegeCode { get; set; } = "";
        public string CollegeName { get; set; } = "";
        public string? ApplicationNumber { get; set; }
        public string? AcademicYear { get; set; }
        public string? CourseLevel { get; set; }
        public string? AffiliationType { get; set; }
        public bool? PrincipalConsent { get; set; }
        public DateTime? SubmittedDate { get; set; }

        public bool IsSubmitted => SubmittedDate.HasValue;
    }
}