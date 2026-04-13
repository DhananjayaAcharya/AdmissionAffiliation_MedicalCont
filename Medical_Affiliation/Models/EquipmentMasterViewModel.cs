using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class EquipmentMasterViewModel
{
    [Required(ErrorMessage = "Equipment Name is required")]
    public string EquipmentName { get; set; }

    [Required(ErrorMessage = "Please select a department")]
    public string DepartmentCode { get; set; }

    // UI-only → should not be validated
    [ValidateNever]
    public List<SelectListItem> Departments { get; set; }
}