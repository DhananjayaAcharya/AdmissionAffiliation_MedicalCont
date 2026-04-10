using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class CA_VehicleDetailsViewModel
    {
        public int? Id { get; set; }
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? RegistrationNo { get; set; }

        // === INPUT FIELDS WITH VALIDATION ===
        [Required(ErrorMessage = "Vehicle Registration Number is required.")]
        [StringLength(20, ErrorMessage = "Registration number cannot exceed 20 characters.")]
        public string? VehicleRegNo { get; set; }

        [Required(ErrorMessage = "Please select Vehicle For.")]
        public string? VehicleForCode { get; set; }

        [Required(ErrorMessage = "Seating Capacity is required.")]
        [Range(1, 100, ErrorMessage = "Seating capacity must be between 1 and 100.")]
        public int? SeatingCapacity { get; set; }

        [Required(ErrorMessage = "FC Validity Date is required.")]
        public DateTime? ValidityDate { get; set; }

        //[Required(ErrorMessage = "Please select RC Book status.")]
        //public string? RcBookStatus { get; set; } = "N";

        //[Required(ErrorMessage = "Please select Insurance status.")]
        //public string? InsuranceStatus { get; set; } = "N";

        //[Required(ErrorMessage = "Please select Driving Licence status.")]
        //public string? DrivingLicenseStatus { get; set; } = "N";

        [Required(ErrorMessage = "Please select RC Book status.")]
        public string? RcBookStatus { get; set; }

        [Required(ErrorMessage = "Please select Insurance status.")]
        public string? InsuranceStatus { get; set; }

        [Required(ErrorMessage = "Please select Driving Licence status.")]
        public string? DrivingLicenseStatus { get; set; }


        // Dropdown
        public List<SelectListItem> VehicleForList { get; set; } = new();

        // Existing rows (no validation needed here)
        public List<CA_VehicleDetailsViewModel> ExistingList { get; set; } = new();
    }
}