using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class CollegeDetailsUpdateViewModel
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string CollegeTown { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int? SelectedFacultyId { get; set; }
        public string SelectedCollegeCode { get; set; }
        public List<SelectListItem> FacultyList { get; set; } = new();
        public List<CollegeUpdateItem> CollegeList { get; set; } = new();
    }
    public class CollegeUpdateItem
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
    }
}
