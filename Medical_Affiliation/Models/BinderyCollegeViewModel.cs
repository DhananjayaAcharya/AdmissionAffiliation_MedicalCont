namespace Medical_Affiliation.ViewModels
{
    public class BinderyCollegeViewModel
    {
        public string CollegeCode { get; set; } = null!;

        public string CollegeName { get; set; } = null!;

        public string FacultyCode { get; set; } = null!;

        public string? HasEquipment { get; set; }

        public string CourseLevel { get; set; } = null!;
    }
}