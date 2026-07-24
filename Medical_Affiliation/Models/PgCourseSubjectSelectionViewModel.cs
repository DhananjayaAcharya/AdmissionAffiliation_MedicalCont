using System.Collections.Generic;

namespace Medical_Affiliation.Models
{
    public class PgCourseSubjectSelectionViewModel
    {
        public string? FacultyCode { get; set; }
        public string? CollegeCode { get; set; }
        public string? CourseLevel { get; set; }
        public string? TypeOfAffiliation { get; set; }
        public List<PgCourseSubjectItem> Subjects { get; set; } = new List<PgCourseSubjectItem>();

        public class PgCourseSubjectItem
        {
            public string CourseCode { get; set; } = string.Empty;
            public string CourseName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
        }
    }
}
