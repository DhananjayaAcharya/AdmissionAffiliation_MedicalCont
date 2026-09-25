using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CollegeEmailUpdateViewModel
    {
        [Display(Name = "Faculty")]
        [Required(ErrorMessage = "Please select a faculty.")]
        public string? FacultyCode { get; set; }

        public List<SelectListItem> Faculties { get; set; } = new();

    }

    public class CollegeMasterImportViewModel
    {
        public string? CollegeCode { get; set; }

        public string? CollegeName { get; set; }

        public string? CollegeTown { get; set; }

        public string? FacultyCode { get; set; }

        public string? Password { get; set; }

        public string? HashedPassword { get; set; }

        public string? IsDeclared { get; set; }

        public string? ChangedPassword { get; set; }

        public string? PrincipalNameDeclared { get; set; }

        public string? PrincipalMobileNumber { get; set; }

        public string? DistrictId { get; set; }

        public string? TalukId { get; set; }

        public bool? Status { get; set; }

        public string? CollegeEmail { get; set; }

        public bool ShowNodalOfficerDetails { get; set; }

        public bool ShowIntakeDetails { get; set; }

        public bool ShowRepositoryDetails { get; set; }
    }
}