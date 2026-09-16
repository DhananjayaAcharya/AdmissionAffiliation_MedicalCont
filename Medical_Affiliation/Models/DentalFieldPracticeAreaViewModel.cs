using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medical_Affiliation.Models
{
    public class DentalFieldPracticeAreaViewModel
    {
        // ============================================================
        // Primary Key
        // ============================================================

        public int DentalFieldPracticeAreaId { get; set; }


        // ============================================================
        // Session Controlled Fields
        // ============================================================

        public int FacultyId { get; set; }

        public string? CollegeCode { get; set; }

        public int TypeId { get; set; }

        public string? CourseLevel { get; set; }


        // ============================================================
        // Dental Field Practice Area Details
        // ============================================================

        [Required(ErrorMessage = "Please enter Location.")]
        [StringLength(250, ErrorMessage = "Location cannot exceed 250 characters.")]
        public string? Location { get; set; }


        [Required(ErrorMessage = "Please enter Address.")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }


        [Required(ErrorMessage = "Please enter Managed By.")]
        [StringLength(250, ErrorMessage = "Managed By cannot exceed 250 characters.")]
        public string? ManagedBy { get; set; }


        // ============================================================
        // Staff List
        // ============================================================

        // New uploaded file
        public IFormFile? StaffListFile { get; set; }

        // Existing file path
        public string? StaffList { get; set; }


        // ============================================================
        // Population Served
        // ============================================================

        [Required(ErrorMessage = "Please enter Population Served.")]
        [Range(1, int.MaxValue, ErrorMessage = "Population Served must be greater than 0.")]
        public int? PopulationServed { get; set; }


        // ============================================================
        // Existing Records
        // ============================================================

        public List<DentalFieldPracticeArea> DentalFieldPracticeRecords { get; set; } = new();
    }
}