using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class ApplicationStatusReportVM
    {
        public List<CollegeStatusRowVM> Colleges { get; set; } = new();

        public int TotalColleges { get; set; }
        public int FullyCompleted { get; set; }
        public int InProgress { get; set; }
        public int NotStarted { get; set; }

        public string? Search { get; set; }
        public string? FilterStatus { get; set; }
        public string? SortBy { get; set; }

        // NEW
        public string? SelectedCollegeCode { get; set; }

        //NEW - changes by ram on 21/05/2026

        public string? SelectedFaculty { get; set; }
        public string? SelectedCollege { get; set; }

        public List<SelectListItem> Faculties { get; set; } = new();
        public List<SelectListItem> CollegesDropdown { get; set; } = new();
    }

    public class CollegeStatusRowVM
    {
        public string CollegeCode { get; set; } = "";
        public string CollegeName { get; set; } = "";
        public List<string?> Levels { get; set; } = new();
        public int TotalSteps { get; set; }
        public int DoneSteps { get; set; }
        public int Percentage { get; set; }
        public List<StepGroupVM> StepGroups { get; set; } = new();

        public string PhoneNumber { get; set; } = "";
    }

    public class StepGroupVM
    {
        public string GroupName { get; set; } = "";
        public List<StepDetailVM> Steps { get; set; } = new();
    }

    public class StepDetailVM
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public bool Completed { get; set; }
    }

    public class MyApplicationStatusVM
    {
        public string CollegeCode { get; set; } = "";
        public int TotalSteps { get; set; }
        public int DoneSteps { get; set; }
        public int Percentage { get; set; }
        public List<string> Levels { get; set; } = new();
        public List<StepGroupVM> StepGroups { get; set; } = new();
    }
}
