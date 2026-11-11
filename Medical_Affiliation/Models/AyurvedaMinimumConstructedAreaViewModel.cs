using Microsoft.AspNetCore.Mvc.Rendering;
namespace Medical_Affiliation.Models;

public class AyurvedaMinimumConstructedAreaViewModel
{
    public int SelectedConstructedAreaID { get; set; }
    public string? SelectedConstructedAreaName { get; set; }
    public int FacultyId { get; set; } = 4;
    public string? CollegeCode { get; set; }
    public string? CourseLevel { get; set; } = "PG";
    public string? AcademicYear { get; set; }
    public List<SelectListItem> ConstructedAreas { get; set; } = new();
    public List<AyurvedaMinimumConstructedAreaItemViewModel> Items { get; set; } = new();
}

public class AyurvedaMinimumConstructedAreaItemViewModel
{
    public int ConstructedAreaDetailID { get; set; }
    public string MegaSectionName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string? IntakeCapacity60Requirement { get; set; }
    public string? IntakeCapacity100Requirement { get; set; }
    public string? IntakeCapacity150Requirement { get; set; }
    public string? IntakeCapacity200Requirement { get; set; }
    public decimal? IntakeCapacity60 { get; set; }
    public decimal? IntakeCapacity100 { get; set; }
    public decimal? IntakeCapacity150 { get; set; }
    public decimal? IntakeCapacity200 { get; set; }
}
