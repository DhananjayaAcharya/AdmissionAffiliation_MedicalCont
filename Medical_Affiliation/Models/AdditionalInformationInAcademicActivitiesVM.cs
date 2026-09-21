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

        public List<DentalConferencesConductedVM> Conferences { get; set; } = new List<DentalConferencesConductedVM>();
    }


    public class DentalConferencesConductedVM
    {
        public int Id { get; set; }
        public string? CollegeCode { get; set; }

        public int FacultyCode { get; set; }

        public string? CourseLevel { get; set; }

        public int? TypeId { get; set; }


        // ============================
        // Conference Details
        // ============================

        [Required(ErrorMessage = "Conference name is required")]
        [StringLength(500, ErrorMessage = "Conference name cannot exceed 500 characters")]
        public string ConferenceName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Conference place is required")]
        [StringLength(300, ErrorMessage = "Conference place cannot exceed 300 characters")]
        public string ConferencePlace { get; set; } = string.Empty;


        [Required(ErrorMessage = "Conference date is required")]
        public DateOnly? ConferenceDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
