namespace Medical_Affiliation.Models
{
    public class CA_SS_CoursesAppliedVM
    {
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }

        // Warning Message
        public string CautionMessage { get; set; } = string.Empty;

        // DM Courses
        public List<SSCourseRow> DMCourses { get; set; } = new();

        // M.Ch Courses
        public List<SSCourseRow> MChCourses { get; set; } = new();
    }

    public class SSCourseRow
    {
        public string CourseName { get; set; } = string.Empty;

        // From CollegeCourseIntakeDetails
        public int? PresentIntake { get; set; }

        public int? CourseCode { get; set; } 
    }
}

