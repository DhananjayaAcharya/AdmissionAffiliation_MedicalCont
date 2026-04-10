using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class CollegeCourseViewModel
    {
        public string? CollegeCode { get; set; }
        public string? CollegeName { get; set; }

        public string SelectedCourseLevel { get; set; }
        public IEnumerable<SelectListItem> CourseLevelList { get; set; }
        public List<CourseDetail>? Courses { get; set; } = new List<CourseDetail>();
        public DocumentUploadViewModel? DocumentUpload { get; set; }

        public bool DeclarationAcceptedUG { get; set; }
        public bool DeclarationAcceptedPG { get; set; }
        public bool DeclarationAcceptedSS { get; set; }
        public string? PrincipalNameUG { get; set; }
        public string? PrincipalNamePG { get; set; }
        public string? PrincipalNameSS { get; set; }
        //public bool? IsDocsALlUploaded { get; set; }
        public string? CollegeAddress { get; set; }
        public string? RguhsIntake202526 { get; set; }
        public string? CollegeIntake202526 { get; set; }
        public IFormFile? DocumentLop202627 { get; set; }
        public IFormFile? DocumentNmc202627 { get; set; }
        public bool IsDocumentLop202627Available { get; set; }
        public bool IsDocumentNmc202627Available { get; set; }
    }

    public class CollegeCourseFreezeViewModel
    {
        public int Id { get; set; }
        public bool freezeFlag { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string facultyCode { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }

        public List<FacultyForFreezeViewModel> FacultyForFreeze { get; set; }
    }

    public class FacultyForFreezeViewModel
    {
        public string FacultyCode { get; set; }
        public string FacultyName { get; set; }
    }

    public class CourseDetail
    {
        public string? CourseName { get; set; }
        public string CourseCode { get; set; }
        public int? ExistingIntake { get; set; }
        public int? PresentIntake { get; set; }
        public IFormFile? Document1 { get; set; }
        public IFormFile? Document2 { get; set; }
        public bool? IsDocument1Available { get; set; }
        public bool? IsDocument2Available { get; set; }
        public string? RguhsIntake202526 { get; set; }
        public string? CollegeIntake202526 { get; set; }
        public IFormFile? DocumentLop202627 { get; set; }
        public IFormFile? DocumentNmc202627 { get; set; }
        public bool freezeStatus { get; set; }
        public bool IsDocumentLop202627Available { get; set; }
        public bool IsDocumentNmc202627Available { get; set; }
        public string CourseLevel { get; set; }

    }

    public class DocumentUploadViewModel
    {
        public bool? IsDocsALlUploaded { get; set; }
        public string? CollegeCode { get; set; }

    }

}