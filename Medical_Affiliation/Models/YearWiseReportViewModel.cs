using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

public class YearWiseReportViewModel
{
    // Filters
    public string SelectedYear { get; set; }
    public int? SelectedFacultyId { get; set; }
    public int? SelectedCollegeId { get; set; }

    // Dropdown data
    public List<string> YearList { get; set; }
    public List<SelectListItem> FacultyList { get; set; }
    public List<SelectListItem> CollegeList { get; set; }

    // Report rows
    public List<YearWiseRowDto> Records { get; set; }
}

public class YearWiseRowDto
{
    public string CollegeName { get; set; }
    public string CollegeAddress { get; set; }
    public string CourseName { get; set; }

    // 2025-26 columns
    public string? ExistingIntake_2025_26 { get; set; }
    public string? PresentIntake_2025_26 { get; set; }
    public byte[]? DocumentAffiliation_2025_26 { get; set; }
    public byte[]? DocumentLop_2025_26 { get; set; }

    // 2026-27 columns
    public string? RguhsIntake_2026_27 { get; set; }
    public string? CollegeIntake_2026_27 { get; set; }
    public byte[]? DocumentLop_2026_27 { get; set; }
    public byte[]? DocumentNmc_2026_27 { get; set; }
}
