using Microsoft.AspNetCore.Http;

namespace Medical_Affiliation.Models
{
    public class MedicalCollegeCourseIntakeViewModel
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string FacultyCode { get; set; } = string.Empty;
        public string? CourseLevel { get; set; }
        public List<MedicalCollegeCourseIntakeRowViewModel> Courses { get; set; } = new();

        // Drives the conditional columns/inputs in the view
        public bool ShowIncreasedIntakeFields { get; set; }

        // ?? Previous details section ??????????????????????????
        public int? AffiliationTypeId { get; set; }
        public List<PreviousIntakeCourseVm> PreviousDetails { get; set; } = new();
    }

    public class MedicalCollegeCourseIntakeRowViewModel
    {
        public int Slno { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Intake2627 { get; set; }

        public string? IncreasedIntake { get; set; }
        public string? AcademicYear { get; set; }
    }

    // One course (e.g. MBBS) with its five fixed intake slabs
    public class PreviousIntakeCourseVm
    {
        public int CourseCode { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public List<PreviousIntakeSlabVm> Slabs { get; set; } = new();
    }

    // One slab row: 50 | 51-100 | 101-150 | 151-200 | 201-250
    public class PreviousIntakeSlabVm
    {
        public int Id { get; set; }                   // 0 = not saved yet
        public string IntakeSlab { get; set; } = string.Empty;
        public short? LopYear { get; set; }           // Year of obtaining LOP
        public IFormFile? NmcDocument { get; set; }   // populated on POST only
        public bool HasDocument { get; set; }         // populated on GET only
        public string? DocumentName { get; set; }     // populated on GET only
    }
}