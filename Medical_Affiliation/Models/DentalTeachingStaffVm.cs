using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class DentalTeachingStaffVm
    {
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? CourseLevel { get; set; }

        public List<SelectListItem> Colleges { get; set; } = new();
        //public List<DentalDesignationTeachingVm> DesignationHeaders { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> FacultyList { get; set; } = new();

        public List<SelectListItem> Designations { get; set; } = new();
        public List<FacultyExperienceVm> FacultyRows { get; set; } = new();

        //public List<DentalFacultyTeachingVm> FacultyRows { get; set; } = new();
    }

    //public class DentalFacultyTeachingVm
    //{
    //    public string? NameOfFaculty { get; set; }

    //    public List<DentalDesignationTeachingVm> Designations { get; set; } = new();
    //}

    public class SaveOtherCollegeVm
    {
        public string CollegeName { get; set; }

        public string CollegeTown { get; set; }

        public string? State { get; set; }

        public string? District { get; set; }

        public string? Taluk { get; set; }
    }


    //public class DentalDesignationTeachingVm
    //{
    //    public int Id { get; set; }

    //    public string? DesignationCode { get; set; }

    //    public string? DesignationName { get; set; }

    //    // Selected Course Level
    //    // UG or PG
    //    public string? CourseLevel { get; set; }

    //    // College based on selected course level
    //    public string? CollegeCode { get; set; }

    //    // Department / Subject
    //    public string? DepartmentCode { get; set; }

    //    public string? DepartmentName { get; set; }

    //    // Experience dates
    //    public DateTime? FromDate { get; set; }

    //    public DateTime? ToDate { get; set; }

    //    public decimal? TotalExperience { get; set; }

    //}

    //public class DentalTeachingStaffVm
    //{
    //    public string? CollegeCode { get; set; }
    //    public string? FacultyCode { get; set; }

    //    public List<SelectListItem> Colleges { get; set; } = new();

    //    public List<FacultyExperienceVm> FacultyRows { get; set; } = new();
    //}

    public class FacultyExperienceVm
    {
        public string? NameOfFaculty { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public decimal TotalExperience { get; set; }

        public List<FacultyExperienceDetailVm> Experiences { get; set; } = new();
    }

    public class FacultyExperienceModalVm
    {
        public string? NameOfFaculty { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public decimal TotalExperience { get; set; }

        public List<FacultyExperienceDetailVm> Experiences { get; set; }
            = new();
    }

    public class FacultyExperienceDetailVm
    {
        public int Id { get; set; }
        public string? DesignationCode { get; set; }

        public string? DesignationName { get; set; }

        public string? CourseLevel { get; set; }

        public string? CollegeCode { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal Experience { get; set; }
    }
}