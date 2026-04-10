using System;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.ViewModels
{
    public class IntakeDetailsLatestViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Faculty Code")]
        public string FacultyCode { get; set; } = null!;

        [Required]
        [Display(Name = "College Code")]
        public string CollegeCode { get; set; } = null!;

        [Required]
        [Display(Name = "Course Level")]
        public string CourseLevel { get; set; } = null!;

        [Required]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = null!;

        [Display(Name = "Existing Intake (CA)")]
        public int ExistingIntakeCa { get; set; }

        [Display(Name = "Additional Seats Requested")]
        public int AdditionalSeatRequested { get; set; }

        [Display(Name = "New Course Seats Requested")]
        public int NewCourseSeatRequested { get; set; }

        [Display(Name = "Total Intake")]
        public int TotalIntake { get; set; }

        [Display(Name = "Is Declared")]
        public int IsDeclared { get; set; }

        [Required]
        [Display(Name = "Principal Name")]
        public string PrincipalName { get; set; } = null!;

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
    }

}
