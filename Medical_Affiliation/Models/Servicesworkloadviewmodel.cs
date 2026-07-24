using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class ServicesWorkloadViewModel
    {
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string TypeOfAffiliation { get; set; } = string.Empty;

        // C.i. Specialty clinics run by the Department of Surgery (9 fixed rows)
        public List<SpecialtyClinicRowVM> SpecialtyClinics { get; set; } = new();

        // C.ii. Services provided by the Department of Surgery (3 fixed rows)
        public List<DepartmentServiceRowVM> DepartmentServices { get; set; } = new();

        // D. Clinical Material and Investigative Workload (21 fixed rows)
        public List<ClinicalWorkloadRowVM> ClinicalWorkload { get; set; } = new();
    }

    public class SpecialtyClinicRowVM
    {
        public string ClinicName { get; set; } = string.Empty;   // fixed label
        public int ClinicSequence { get; set; }

        public string? Weekdays { get; set; }
        public string? Timings { get; set; }
        public int? NumberOfCasesAvg { get; set; }
        public string? ClinicInchargeName { get; set; }
    }

    public class DepartmentServiceRowVM
    {
        public string ServiceName { get; set; } = string.Empty;  // fixed label
        public int ServiceSequence { get; set; }

        public string? IsAvailable { get; set; }   // Yes / No
        public string? Remarks { get; set; }
    }

    public class ClinicalWorkloadRowVM
    {
        public string ParticularName { get; set; } = string.Empty; // fixed label
        public int ParticularSequence { get; set; }

        public decimal? EntireHospital { get; set; }
        public decimal? OnDayOfAssessment { get; set; }
        public decimal? Random3Days { get; set; }
        public decimal? PreviousYear { get; set; }
    }
}