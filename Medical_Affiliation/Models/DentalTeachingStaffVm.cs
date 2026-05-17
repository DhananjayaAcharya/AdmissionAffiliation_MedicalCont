using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class DentalTeachingStaffVm
    {
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? CourseLevel { get; set; }

        public List<SelectListItem> Colleges { get; set; } = new();
        public List<DentalDesignationTeachingVm> DesignationHeaders { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();

        public List<DentalFacultyTeachingVm> FacultyRows { get; set; } = new();
    }

    public class DentalFacultyTeachingVm
    {
        public string? NameOfFaculty { get; set; }

        public List<DentalDesignationTeachingVm> Designations { get; set; } = new();
    }


    public class DentalDesignationTeachingVm
    {
        public int Id { get; set; }

        public string? DesignationCode { get; set; }

        public string? DesignationName { get; set; }

        // Selected Course Level
        // UG or PG
        public string? CourseLevel { get; set; }

        // College based on selected course level
        public string? CollegeCode { get; set; }

        // Department / Subject
        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        // Experience dates
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal? TotalExperience { get; set; }
    }
}