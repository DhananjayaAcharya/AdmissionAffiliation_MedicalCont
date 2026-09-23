using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models;

public class DentalLibraryPageViewModel
{
    public string CollegeCode { get; set; } = string.Empty;
    public string CourseLevel { get; set; }
    public int FacultyId { get; set; }
    public int AffiliationTypeId { get; set; }
    public List<DentalLibraryExpenditureRowViewModel> Expenditures { get; set; } = new();
    public List<DentalLibraryServiceRowViewModel> Services { get; set; } = new();
}

public class DentalLibraryExpenditureRowViewModel
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal ExpenditureProposed { get; set; }
}

public class DentalLibraryServiceRowViewModel
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}
