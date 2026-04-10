using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_DepartmentLibraryRowVM  // Lightweight for display only
    {
        public int Id { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int? TotalBooks { get; set; }
        public int? BooksAdded { get; set; }
        public int? CurrentJournals { get; set; }
    }

    public class CA_DepartmentLibraryDetailsViewModel  // Full VM for input + list
    {
        public int? Id { get; set; }
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? RegistrationNo { get; set; }

        // === Input fields - validated ===
        [Required(ErrorMessage = "Please select a department")]
        public string? DepartmentCode { get; set; }

        [Required(ErrorMessage = "Total books is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Total books cannot be negative")]
        public int? TotalBooks { get; set; }

        [Required(ErrorMessage = "Books added is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Books added cannot be negative")]
        public int? BooksAdded { get; set; }

        [Required(ErrorMessage = "Current journals is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Current journals cannot be negative")]
        public int? CurrentJournals { get; set; }

        // Dropdown
        public List<SelectListItem> DepartmentList { get; set; } = new();

        // === Display list - NO validation ===
        public List<CA_DepartmentLibraryRowVM> ExistingList { get; set; } = new();
    }
}
