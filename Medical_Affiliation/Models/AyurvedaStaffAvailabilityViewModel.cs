
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models;

public class AyurvedaStaffAvailabilityViewModel
{
    public int SelectedStaffId { get; set; } = 3;
    public string? SelectedStaffName { get; set; }
    public string? SelectedDepartment { get; set; }
    public int FacultyId { get; set; } = 4;
    public string? CollegeCode { get; set; }
    public string CourseLevel { get; set; } = "PG";
    public string? AcademicYear { get; set; }

    public List<SelectListItem> StaffCategories { get; set; } = new();
    public List<SelectListItem> Departments { get; set; } = new();
    public List<AyurvedaStaffAvailabilityItemViewModel> Items { get; set; } = new();
}

//public class AyurvedaStaffAvailabilityItemViewModel
//{
//    public int StaffDetailID { get; set; }

//    public string StaffDepartments { get; set; } = string.Empty;
//    public string StaffSubDepartments { get; set; } = string.Empty;

//    // New property for Teaching Staff cadre
//    public string? StaffCadre { get; set; }

//    public string? IntakeCapacity60Requirement { get; set; }
//    public string? IntakeCapacity100Requirement { get; set; }
//    public string? IntakeCapacity150Requirement { get; set; }
//    public string? IntakeCapacity200Requirement { get; set; }

//    public int? IntakeCapacity60 { get; set; }
//    public int? IntakeCapacity100 { get; set; }
//    public int? IntakeCapacity150 { get; set; }
//    public int? IntakeCapacity200 { get; set; }
//}

public class AyurvedaStaffAvailabilityItemViewModel
{
    public int StaffDetailID { get; set; }

    public string StaffDepartments { get; set; } = string.Empty;

    public string? StaffSubDepartments { get; set; }

    public string? StaffCadre { get; set; }

    public string? IntakeCapacity60Requirement { get; set; }
    public string? IntakeCapacity100Requirement { get; set; }
    public string? IntakeCapacity150Requirement { get; set; }
    public string? IntakeCapacity200Requirement { get; set; }

    public int? IntakeCapacity60 { get; set; }
    public int? IntakeCapacity100 { get; set; }
    public int? IntakeCapacity150 { get; set; }
    public int? IntakeCapacity200 { get; set; }
}
