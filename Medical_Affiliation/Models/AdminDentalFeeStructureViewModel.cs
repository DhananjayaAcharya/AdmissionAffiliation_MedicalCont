using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models;

public class AdminDentalFeeStructurePageViewModel
{
    public AdminDentalFeeStructureForm Form { get; set; } = new();
    public List<AdminDentalFeeStructureRowViewModel> FeeStructures { get; set; } = new();
    public List<SelectListItem> Faculties { get; set; } = new();
    public List<SelectListItem> FeeTypes { get; set; } = new();
    public List<SelectListItem> AffiliationTypes { get; set; } = new();
    public bool IncludeInactive { get; set; }
}

public class AdminDentalFeeStructureForm
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Faculty")]
    public int FacultyCode { get; set; }

    [Required]
    [Display(Name = "Fee type")]
    public int FeeTypeId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Course level")]
    public string CourseLevel { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    [Display(Name = "Amount")]
    public decimal AmountToBePaid { get; set; }

    [StringLength(50)]
    [Display(Name = "Calculation type")]
    public string? CalculationType { get; set; }

    [Display(Name = "Affiliation type")]
    public int? AffiliationTypeId { get; set; }
}

public class AdminDentalFeeStructureRowViewModel
{
    public int Id { get; set; }
    public int FacultyCode { get; set; }
    public int FeeTypeId { get; set; }
    public int? AffiliationTypeId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public string FeeType { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int? CourseCode { get; set; }
    public string? CourseLevel { get; set; }
    public decimal AmountToBePaid { get; set; }
    public string? CalculationType { get; set; }
    public string AffiliationType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
