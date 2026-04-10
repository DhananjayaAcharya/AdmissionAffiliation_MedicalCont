using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class CollegeDocsReportFilterViewModel
    {
        public string FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public string CourseLevel { get; set; }

        public List<SelectListItem> FacultyList { get; set; } = new();
        public List<SelectListItem> CollegeList { get; set; } = new();
        public List<CollegeDocsReportViewModel> ReportData { get; set; } = new();
    }
   
}
