using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class PgAcademicIntakeViewModel
    {
        public int? SelectedCourseCode { get; set; }

        public List<SelectListItem> PgCourses { get; set; }
            = new();

        public CourseIntakeViewModel? Course { get; set; }
    }

   
}
