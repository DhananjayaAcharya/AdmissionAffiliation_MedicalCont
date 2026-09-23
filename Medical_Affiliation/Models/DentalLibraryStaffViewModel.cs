using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models;

public class DentalLibraryStaffPageViewModel
{
    public string CollegeCode { get; set; } = string.Empty;
    public int FacultyId { get; set; }
    public int AffiliationTypeId { get; set; }
    public DentalLibraryStaffForm Form { get; set; } = new();
    public List<DentalLibraryStaffRowViewModel> Staff { get; set; } = new();
}

public class DentalLibraryStaffForm
{
    public int LibraryStaffId { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Designation")]
    public string Designation { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Qualification { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Experience from")]
    public DateOnly? ExperienceFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Experience to")]
    public DateOnly? ExperienceTo { get; set; }

    [Display(Name = "Currently working")]
    public bool CurrentlyWorking { get; set; }

    [StringLength(150)]
    [Display(Name = "Pay scale")]
    public string? PayScale { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

}

public class DentalLibraryStaffRowViewModel
{
    public int LibraryStaffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public DateOnly? ExperienceFrom { get; set; }
    public DateOnly? ExperienceTo { get; set; }
    public bool CurrentlyWorking { get; set; }
    public string? PayScale { get; set; }
    public string? Category { get; set; }
}
