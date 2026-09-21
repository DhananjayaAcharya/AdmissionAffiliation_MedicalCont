using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class AdditionalInformationInAcademicActivitiesVM
    {
        public int Id { get; set; }

        public string? CollegeCode { get; set; }

        public int FacultyCode { get; set; }

        // No ModelState validation required
        public string? CourseLevel { get; set; }

        public bool HasMedicalEducationUnit { get; set; }

        public bool HasTOTProgrammesConducted { get; set; }

        public bool HasTOTProgrammesAttended { get; set; }

        // Number of TOT programmes conducted
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Enter a valid number of TOT programmes conducted"
        )]
        public int TOTProgrammesConducted { get; set; }

        // Number of TOT programmes attended
        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Enter a valid number of TOT programmes attended"
        )]
        public int TOTProgrammesAttended { get; set; }

        // New PDF upload
        public IFormFile? CMEProgrammePdf { get; set; }

        // Existing PDF path
        public string? CMEProgrammePdfPath { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
