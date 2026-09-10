using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CollegeEmailUpdateViewModel
    {
        [Display(Name = "Faculty")]
        [Required(ErrorMessage = "Please select a faculty.")]
        public string? FacultyCode { get; set; }

        public List<SelectListItem> Faculties { get; set; } = new();

    }
}
