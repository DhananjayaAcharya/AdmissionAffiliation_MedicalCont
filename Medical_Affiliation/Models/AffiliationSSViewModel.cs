using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class AffiliationSSViewModel
    {
        public string CollegeCode { get; set; }
        public int TypeOfAffiliation { get; set; }
        public List<SScourseVM> AllCourses { get; set; }
        public List<SSAssociatedInstitutions> AssociatedInstitutions { get; set; }
        public List<FacultyOptionVM> Faculties { get; set; } = new();
        public List<CollegeOptionVM> AssociatedColleges { get; set; } = new();
        public List<CourseOptionVM> Courses { get; set; } = new();
        public List<SelectListItem> CourseLevelList {  get; set; } = new();

    }
    public class SScourseVM
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseLevel { get; set; }
        public string YearOfStarting { get; set; }
        public int? Sanctioned { get; set; }
        public int? Admitted { get; set; }
        public string Remarks { get; set; }
    }

    public class SSAssociatedInstitutions
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseLevel { get; set; }
        public string FacultyCode { get; set; }
        public string AssociatedCollegeCode { get; set; }
        public string AssociatedCollegeName { get; set; }
        public string AssociatedFacultyCode { get; set; }
        public string AssociatedFacultyName { get; set; }

        public string SelectedFacultyCode { get; set; }
        public string SelectedAssociatedCollegeCode { get; set; }
        public string SelectedCourseCode { get; set; }

        public int? AnnualIntake { get; set; }


    }

    public class SSAssociatedInstitutionPostVM
    {
        public string CourseLevel { get; set; }
        //public string FacultyCode { get; set; }
        public string AssociatedCollegeCode { get; set; }
        public string AssociatedFacultyCode { get; set; }
        public string CourseCode { get; set; }
        public int AnnualIntake { get; set; }
    }


    public class FacultyOptionVM
    {
        public string FacultyCode { get; set; }
        public string FacultyName { get; set; }
    }


    public class CollegeOptionVM
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
    }

    public class CourseOptionVM
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseLevel { get; set; }
    }

}
