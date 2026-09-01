namespace Medical_Affiliation.Models
{
    public class MedicalCollegeCourseIntakeViewModel
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;
        public string CourseLevel { get; set; } = string.Empty;
        public List<MedicalCollegeCourseIntakeRowViewModel> Courses { get; set; } = new();
    }

    public class MedicalCollegeCourseIntakeRowViewModel
    {
        public int Slno { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Intake2627 { get; set; }
    }
}
