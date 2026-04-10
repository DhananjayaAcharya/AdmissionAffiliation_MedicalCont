using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_LibraryStaffDetailsViewModel
    {
        public string? RegistrationNo { get; set; }
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public int? Id { get; set; }

        [Required(ErrorMessage = "Staff Name is required.")]
        [StringLength(100, ErrorMessage = "Staff Name cannot exceed 100 characters.")]
        public string StaffName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Qualification is required.")]
        [StringLength(100, ErrorMessage = "Qualification cannot exceed 100 characters.")]
        public string? Qualification { get; set; }

        [Required(ErrorMessage = "Experience (Years) is required.")]
        [Range(0, 60, ErrorMessage = "Experience must be between 0 and 60 years.")]
        public int? ExperienceYears { get; set; }

        [Required(ErrorMessage = "Remarks are required.")]
        [StringLength(200, ErrorMessage = "Remarks cannot exceed 200 characters.")]
        public string? Remarks { get; set; }

        public List<CA_LibraryStaffDetailsViewModel> ExistingStaff { get; set; }
            = new List<CA_LibraryStaffDetailsViewModel>();
    }
}