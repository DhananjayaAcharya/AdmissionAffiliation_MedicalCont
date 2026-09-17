namespace Medical_Affiliation.Models
{
    public class MedicalCollegeCourseIntakeViewModel
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;
        public string? CourseLevel { get; set; }
        public List<MedicalCollegeCourseIntakeRowViewModel> Courses { get; set; } = new();

        // Drives the conditional columns/inputs in the view
        public bool ShowIncreasedIntakeFields { get; set; }
    }

    public class MedicalCollegeCourseIntakeRowViewModel
    {
        public int Slno { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Intake2627 { get; set; }

        public int? IncreasedIntake { get; set; }
        public string? AcademicYear { get; set; }
    }
}
