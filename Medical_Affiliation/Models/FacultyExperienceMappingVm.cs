using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class FacultyExperienceAdminVm
    {
        public int? SelectedFacultyId { get; set; }

        public string? SelectedCollegeCode { get; set; }

        public string? SelectedFacultyCode { get; set; }

        public string? SelectedFacultyName { get; set; }


        // Faculty master
        public List<SelectListItem> FacultyMasters { get; set; } = new();

        // Colleges based on selected Faculty
        public List<SelectListItem> Colleges { get; set; } = new();

        // Faculty names based on selected College
        public List<SelectListItem> FacultyNames { get; set; } = new();
        public List<FacultyAdminRowVm> FacultyDetails { get; set; } = new();
    }

    public class FacultyAdminRowVm
    {
        public int FacultyDetailId { get; set; }

        public string? CollegeCode { get; set; }

        public string? FacultyCode { get; set; }

        public string? NameOfFaculty { get; set; }

        public string? DepartmentDetails { get; set; }

        public string? Designation { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public bool IsRemoved { get; set; }

        public List<TeachingStaffAdminRowVm> ExperienceDetails { get; set; } = new();
    }

    public class TeachingStaffAdminRowVm
    {
        public int Id { get; set; }

        public string? CourseLevel { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DesignationCode { get; set; }
        public string? DesignationName { get; set; }

        public DateOnly? UGFrom { get; set; }
        public DateOnly? UGTo { get; set; }

        public DateOnly? PGFrom { get; set; }
        public DateOnly? PGTo { get; set; }

        public string? UGCollegeCode { get; set; }
        public string? PGCollegeCode { get; set; }

        public decimal? TotalExperience { get; set; }
    }
}
