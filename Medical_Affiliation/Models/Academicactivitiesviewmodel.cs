using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class AcademicActivitiesViewModel
    {
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string TypeOfAffiliation { get; set; } = string.Empty;

        // G. Academic Activities (7 fixed rows)
        public List<AcademicActivityRowVM> AcademicActivities { get; set; } = new();

        // Publications from the department during the past 3 years
        [Display(Name = "Publications from the department during the past 3 years")]
        public string? PublicationsPast3Years { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? AssessmentDate { get; set; }

        // K. LIC Committee Observations
        [Display(Name = "LIC Committee Observations")]
        public string? LiccommitteeObservations { get; set; }
    }

    public class AcademicActivityRowVM
    {
        public string ActivityDetails { get; set; } = string.Empty;   // fixed label
        public int SlNo { get; set; }

        public int? NumberInLastYear { get; set; }
        public string? Remarks { get; set; }
    }
}