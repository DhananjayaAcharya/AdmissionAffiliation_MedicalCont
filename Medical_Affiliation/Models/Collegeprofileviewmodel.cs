using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CollegeProfileViewModel
    {
        public string CollegeCode { get; set; }

        [StringLength(250)]
        public string CollegeName { get; set; }

        // Existing logo path (for display) — not posted back as a file
        public string ExistingLogoPath { get; set; }

        // New logo file, only set when the user picks a new image
        public IFormFile Logo { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Url]
        [StringLength(200)]
        public string Website { get; set; }

        [Range(1800, 2100)]
        public int? EstablishedYear { get; set; }

        [StringLength(150)]
        public string PrincipalName { get; set; }
    }
}