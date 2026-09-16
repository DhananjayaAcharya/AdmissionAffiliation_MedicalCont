using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class WorkShopDetailsViewModel
    {
        public int WorkShopDetailsId { get; set; }

        // ==============================
        // References
        // ==============================

        [Required(ErrorMessage = "Please select Faculty.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Faculty.")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Please select College.")]
        public string CollegeCode { get; set; } = null!;

        [Required(ErrorMessage = "Please select Affiliation Type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Affiliation Type.")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Please select Course Level.")]
        public string CourseLevel { get; set; } = null!;


        // ==============================
        // Workshop Details
        // ==============================

        [Required(ErrorMessage = "Please enter Staff details.")]
        public string? Staff { get; set; }

        [Required(ErrorMessage = "Please enter Equipment details.")]
        public string? Equipment { get; set; }

        [Required(ErrorMessage = "Please enter Scope of Work.")]
        public string? ScopeOfWork { get; set; }


        // ==============================
        // Dropdown Lists
        // ==============================

        public List<SelectListItem> Faculties { get; set; }
            = new List<SelectListItem>();

        public List<SelectListItem> Colleges { get; set; }
            = new List<SelectListItem>();

        public List<SelectListItem> AffiliationTypes { get; set; }
            = new List<SelectListItem>();

        public List<SelectListItem> CourseLevels { get; set; }
            = new List<SelectListItem>();

        public List<WorkShopDetail> WorkshopRecords { get; set; }
            = new();
    }
}