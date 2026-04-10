using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class AHSInstituteDetailViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "College Code is required.")]
        [StringLength(20, ErrorMessage = "College Code cannot exceed 20 characters.")]
        public string CollegeCode { get; set; } = null!;

        [Required(ErrorMessage = "Faculty Code is required.")]
        [StringLength(20, ErrorMessage = "Faculty Code cannot exceed 20 characters.")]
        public string FacultyCode { get; set; } = null!;

        [Required(ErrorMessage = "Institute Name is required.")]
        [StringLength(100, ErrorMessage = "Institute Name cannot exceed 100 characters.")]
        public string InstituteName { get; set; } = null!;

        [Required(ErrorMessage = "Institute Address is required.")]
        [StringLength(200, ErrorMessage = "Institute Address cannot exceed 200 characters.")]
        public string InstituteAddress { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Trust/Society Name cannot exceed 100 characters.")]
        public string? TrustSocietyName { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? YearOfEstablishmentOfTrust { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? YearOfEstablishmentOfCollege { get; set; }

        [StringLength(50, ErrorMessage = "Institution Type cannot exceed 50 characters.")]
        public string? InstitutionType { get; set; }

        [StringLength(100, ErrorMessage = "HOD of Institution name cannot exceed 100 characters.")]
        public string? HodofInstitution { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? Dob { get; set; }

        [RegularExpression(@"^\d{1,3}$", ErrorMessage = "Age must be a valid number up to 3 digits.")]
        public string? Age { get; set; }

        [StringLength(50, ErrorMessage = "Teaching Experience cannot exceed 50 characters.")]
        public string? TeachingExperience { get; set; }

        [MinLength(1, ErrorMessage = "At least one Degree is required.")]
        public List<string> Degree { get; set; } = new List<string>();

        [StringLength(200, ErrorMessage = "Course Selected Specialities cannot exceed 200 characters.")]
        public string? CourseSelectedSpecialities { get; set; }

        [Required(ErrorMessage = "Qualifications are required.")]
        [StringLength(200, ErrorMessage = "Qualifications cannot exceed 200 characters.")]
        public string Qualifications { get; set; } = null!;

        [Required(ErrorMessage = "Highest Qualification is required.")]
        [StringLength(100, ErrorMessage = "Highest Qualification cannot exceed 100 characters.")]
        public string HighestQualification { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Course Code cannot exceed 50 characters.")]
        public string? CourseCode { get; set; }

        public bool IsReadOnly { get; set; }

        //chandan
        public string? OtherDegreeText { get; set; }

        //chandan

        public IFormFile? TrustEstablishmentDocument { get; set; }
        public IFormFile? CollegeEstablishmentDocument { get; set; }

        // ⭐ View fields (byte[])
        public byte[]? TrustDocData { get; set; }
        public byte[]? EstablishmentDocData { get; set; }
    }

}
