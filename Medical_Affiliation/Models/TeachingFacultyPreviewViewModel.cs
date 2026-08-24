namespace Medical_Affiliation.Models
{
    public class TeachingFacultyPreviewViewModel
    {
        public string FacultyCode { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string DesignationCode { get; set; } = string.Empty;

        public string DesignationName { get; set; } = string.Empty;

        public string SeatSlabId { get; set; } = string.Empty;

        // Required faculty as per master
        public string RequiredFaculty { get; set; } = "0";

        // Faculty available as entered by college
        public string AvailableFaculty { get; set; } = "0";
    }

    public class TeachingFacultyDetailsPreviewViewModel
    {
        public List<TeachingFacultyPreviewViewModel> FacultyDetails { get; set; }
            = new();
    }
}
