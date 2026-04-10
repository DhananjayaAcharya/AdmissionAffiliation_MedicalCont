using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_FinancialCourseRowViewModel
    {
        public int? Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year of Starting is required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must be a 4-digit number.")]
        public string? YearOfStarting { get; set; }

        [Required(ErrorMessage = "Admissions Sanctioned is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater.")]
        public int? AdmissionsSanctioned { get; set; }

        [Required(ErrorMessage = "Admissions Admitted is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Must be 0 or greater.")]
        public int? AdmissionsAdmitted { get; set; }

        public string? GOKSanctionIntakeFileName { get; set; }
        public string? ApexBodyPermissionAndIntakeFileName { get; set; }
        public string? RGUHSSanctionIntakeFileName { get; set; }
        public string? GOIPermissionFileName { get; set; }
    }
}
