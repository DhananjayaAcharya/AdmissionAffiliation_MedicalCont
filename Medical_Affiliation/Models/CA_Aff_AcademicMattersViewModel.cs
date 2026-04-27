using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Aff_AcademicMattersViewModel
    {
        public string? CourseLevel { get; set; }
        public string? CollegeCode { get; set; }
        public int? FacultyId { get; set; }
        public int? AffiliationType { get; set; }

        public List<AcademicPerformanceViewModel> AcademicRows { get; set; } = new();
        public List<CourseCurriculumDisplayViewModel>? CourseCurriculumdvm { get; set; }


        //public CourseCurriculumViewModel CourseCurriculum { get; set; }
        //public ExaminationSchemeViewModel ExaminationScheme { get; set; }
        //public StudentRegisterRecordViewModel StudentRegisterRecord { get; set; }

        public List<CourseCurriculumViewModel>? CourseCurriculums { get; set; }


        public List<ExaminationSchemeViewModel> ExaminationSchemes { get; set; } = new List<ExaminationSchemeViewModel>();

        public List<ExaminationSchemeRowViewModel> ExaminationSchemess { get; set; }
    = new();
        public List<StudentRegisterRecordViewModel> StudentRegisterRecords { get; set; } = new List<StudentRegisterRecordViewModel>();
    }

    public class CourseCurriculumDisplayViewModel
    {
        public int CourseCurriculumId { get; set; }
        public int CurriculumId { get; set; }
        public string CurriculumName { get; set; } = string.Empty;
        public bool HasPdf { get; set; }
        public string? CurriculumDetails { get; set; }

        // Display
        public string? FileName { get; set; }

        // Identifier used to fetch the file
        public int? FileId { get; set; }
    }


    // ✅ UPDATED: Added Scheme properties for table display
    public class ExaminationSchemeViewModel
    {
        public int? ExaminationSchemeId { get; set; }
        public int? SchemeId { get; set; }
        public string? SchemeCode { get; set; } = "";
        public string? SchemeName { get; set; } = "";
        public int? NumberOfStudents { get; set; }
        public string? Remarks { get; set; } = "";
    }

    public class AcademicPerformanceViewModel
    {
        public int AcademicPerformanceId { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int? YearOfStudyId { get; set; }

        public string? YearName { get; set; }

        [Required(ErrorMessage = "Enter Regular Students")]
        [Range(0, int.MaxValue, ErrorMessage = "Enter non-negative integer")]
        public int? RegularStudents { get; set; }

        [Required(ErrorMessage = "Enter Repeater Students")]
        [Range(0, int.MaxValue, ErrorMessage = "Enter non-negative integer")]
        public int? RepeaterStudents { get; set; }

        [Required(ErrorMessage = "Enter Passed Students")]
        [Range(0, int.MaxValue, ErrorMessage = "Enter non-negative integer")]
        public int? NumberOfStudentsPassed { get; set; }

        public decimal? PassPercentage { get; set; }

        [Range(0, int.MaxValue)]
        [Required(ErrorMessage = "Enter No of FirstClass Students")]
        public int? FirstClassCount { get; set; }

        [Range(0, int.MaxValue)]
        [Required(ErrorMessage = "Enter No of Distinct Students")]
        public int? DistinctionCount { get; set; }

        public string? Remarks { get; set; }
    }

    public class AcademicPerformancePgSsViewModel : AcademicPerformanceViewModel
    {
        public string? Subject { get; set; }
    }


    public class CourseCurriculumViewModel
    {
        public int? CourseCurriculumId { get; set; }
        public int? CurriculumId { get; set; }

        public string CurriculumName { get; set; }

        public bool? HasPdf { get; set; }

        
        public string? CurriculumDetails { get; set; }

        
        public List<IFormFile>? CurriculumPdfFiles { get; set; }


        //public string? CurriculumPdfPath { get; set; }



        public List<CourseCurriculumFileInfo>? UploadedFiles { get; set; } = new List<CourseCurriculumFileInfo>();
    }
    //public class CourseCurriculumFileViewModel
    //{
    //    public int? Id { get; set; }
    //    public string? FileName { get; set; } = "";
    //    public byte[]? FileContent { get; set; } = Array.Empty<byte>();
    //}

    public class CourseCurriculumFileInfo
    {
        public int? Id { get; set; }
        public string? FileName { get; set; } = "";
    }



    public class StudentRegisterRecordViewModel
    {
        public int? StudentRegisterRecordId { get; set; }
        public int? RegisterRecordId { get; set; }

        public bool IsExists { get; set; }
        public string? RegisterName { get; set; }

        [Required(ErrorMessage = "Please select Yes or No")]

        public bool? IsMaintained { get; set; }

    }

    public class PgSubjectSectionVM
    {
        public string Subject { get; set; }

        public List<YearDataVM> YearData { get; set; } = new();
    }

    public class YearDataVM
    {
        public int YearOfStudyId { get; set; }
        public string YearName { get; set; }

        public int? RegularStudents { get; set; }
        public int? RepeaterStudents { get; set; }
        public int? NumberOfStudentsPassed { get; set; }
        public decimal? PassPercentage { get; set; }
        public int? FirstClassCount { get; set; }
        public int? DistinctionCount { get; set; }
        public string Remarks { get; set; }
    }


    public class CA_Aff_BaseAcademicMattersViewModel
    {
        public string CollegeCode { get; set; }
        public int FacultyId { get; set; }
        public int AffiliationType { get; set; }

        public List<SelectListItem> Subjects { get; set; } = new();

        public List<PgSubjectSectionVM> Sections { get; set; } = new();
    }

    public class CA_Aff_PgAcademicMattersViewModel
    : CA_Aff_BaseAcademicMattersViewModel
    {
        // future PG-specific properties here
    }

    public class CA_Aff_SsAcademicMattersViewModel
    : CA_Aff_BaseAcademicMattersViewModel
    {
        // future SS-specific properties here
    }
}
public class ExaminationSchemeRowViewModel
{
    public int? SchemeId { get; set; }           // RS3–RS7
    public string? SchemeCode { get; set; }
    // Display only
    [Required(ErrorMessage = "Required")]
    [Range(0, 9999, ErrorMessage = "Invalid number")]
    public int? NumberOfStudents { get; set; }  // User input
}
