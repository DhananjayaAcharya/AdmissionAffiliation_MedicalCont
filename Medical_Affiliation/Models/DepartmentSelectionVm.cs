using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class DepartmentSelectionVm
    {
        [Required(ErrorMessage = "Please select a department.")]
        public string DepartmentCode { get; set; } = string.Empty;
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
    }
}
