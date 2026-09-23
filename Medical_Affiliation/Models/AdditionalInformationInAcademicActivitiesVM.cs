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

        public bool? HasCMEProgrammeConducted { get; set; }

        public int? NoOfCMEProgrammesConducted { get; set; }

        public bool? HasCMEProgrammeAttended { get; set; }

        public int? NoOfCMEProgrammesAttended { get; set; }

        // New PDF upload
        public IFormFile? CMEProgrammePdf { get; set; }

        // Existing PDF path
        public string? CMEProgrammePdfPath { get; set; }

        public bool IsActive { get; set; } = true;

        public List<DentalConferencesConductedVM> ConferencesConducted { get; set; } = new List<DentalConferencesConductedVM>();
        public List<DentalConferencesAttendedVM> ConferencesAttended { get; set; } = new List<DentalConferencesAttendedVM>();
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


    public class DentalConferencesAttendedVM
    {
        public int Id { get; set; }

        // ============================
        // College
        // ============================

        public string? CollegeCode { get; set; }


        // ============================
        // Faculty
        // ============================

        public int FacultyCode { get; set; }


        // ============================
        // Course Level
        // UG / PG / SS
        // ============================

        public string? CourseLevel { get; set; }


        // ============================
        // Affiliation Type
        // ============================

        public int? TypeId { get; set; }


        // ============================
        // Conference Details
        // ============================

        [Required(ErrorMessage = "Conference name is required")]
        [StringLength(
            500,
            ErrorMessage = "Conference name cannot exceed 500 characters"
        )]
        public string ConferenceName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Conference place is required")]
        [StringLength(
            300,
            ErrorMessage = "Conference place cannot exceed 300 characters"
        )]
        public string ConferencePlace { get; set; } = string.Empty;


        [Required(ErrorMessage = "Conference date is required")]
        public DateOnly? ConferenceDate { get; set; }


        // ============================
        // Student Participants
        // ============================

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Enter a valid number of student participants"
        )]
        public int StudentParticipants { get; set; }


        // ============================
        // Teacher Participants
        // ============================

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Enter a valid number of teacher participants"
        )]
        public int TeacherParticipants { get; set; }


        // ============================
        // Total Participants
        // ============================

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Enter a valid total number of participants"
        )]
        public int TotalParticipants { get; set; }


        // ============================
        // Status
        // ============================

        public bool IsActive { get; set; } = true;
    }
}
