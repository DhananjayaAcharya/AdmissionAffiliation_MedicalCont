namespace Medical_Affiliation.Models
{
    public class CollegeDocumentViewModel
    {
        public string CollegeCode { get; set; } = string.Empty;

        public string? CollegeName { get; set; }

        public List<CollegeCourseDocumentViewModel> Courses { get; set; } = new();
    }

    public class CollegeCourseDocumentViewModel
    {
        public string? CourseName { get; set; }

        public bool HasAffiliationDocument { get; set; }

        public bool HasLopDocument { get; set; }
    }
}
