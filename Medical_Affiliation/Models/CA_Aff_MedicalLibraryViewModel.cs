using System.ComponentModel.DataAnnotations;
using static Medical_Affiliation.Models.LibraryServiceRowViewModel;

namespace Medical_Affiliation.Models
{
    public class CA_Aff_MedicalLibraryViewModel : IValidatableObject
    {
        
        public string? CollegeCode { get; set; }

        public string? CourseLevel { get; set; }
        public int FacultyCode { get; set; }

        
        public int AffiliationType { get; set; }

        public string? RegistrationNo { get; set; }

        public bool? IsFirstLogin { get; set; }

        // ===============================
        // SECTION 1 – LIBRARY SERVICES
        // ===============================

        public List<LibraryServiceRowViewModel> LibraryServices { get; set; }
            = new List<LibraryServiceRowViewModel>();

        // ===============================
        // SECTION 2 – USAGE REPORT
        // ===============================
        
        public IFormFile? UsageReportPdf { get; set; }
        public string? ExistingUsageReportFileName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Usage Report PDF – required only if no existing file
            if (IsFirstLogin == true &&
     UsageReportPdf == null &&
     string.IsNullOrEmpty(ExistingUsageReportFileName))
            {
                yield return new ValidationResult(
                    "Please upload the User Details / Usage Report PDF",
                    new[] { nameof(UsageReportPdf) }
                );
            }

        }

        // ===============================
        // SECTION 3 – LIBRARY STAFF
        // ===============================

        public List<LibraryStaffViewModel> LibraryStaff { get; set; }
            = new List<LibraryStaffViewModel>();

        // ===============================
        // SECTION 4 – DEPARTMENTAL LIBRARY
        // ===============================

        public List<DepartmentLibraryViewModel> DepartmentLibraries { get; set; }
            = new List<DepartmentLibraryViewModel>();

        // ===============================
        // SECTION 5 – OTHER DETAILS
        // ===============================
        public MedicalLibraryOtherDetailsViewModel OtherDetails { get; set; }
            = new MedicalLibraryOtherDetailsViewModel();

        

    }

    public class LibraryServiceRowViewModel : IValidatableObject
    {
        public int?  ServiceId{ get; set; }

        [Required(ErrorMessage = "Please select Yes or No")]
        public string? IsAvailable { get; set; }   // Yes / No

        // Mandatory ONLY for User Education Programme (ServiceId = 6)
        public IFormFile? UploadedPdf { get; set; }
        public string? ExistingFileName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ServiceId == 6 &&
                IsAvailable == "Yes" &&
                UploadedPdf == null &&
                string.IsNullOrEmpty(ExistingFileName))
            {
                yield return new ValidationResult(
                    "PDF is mandatory for this service.",
                    new[] { nameof(UploadedPdf) }
                );
            }
        }
    }
    public class LibraryStaffViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string? StaffName { get; set; }

        [Required]
        public string? Designation { get; set; }

        [Required]
        public string? Qualification { get; set; }

        [Required]
        public int? Experience { get; set; }

        [Required]
        public string? Category { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
    public class DepartmentLibraryViewModel
    {
        [Required]
        public string? DepartmentCode { get; set; }

        [Required]
        public int? TotalBooks { get; set; }

        [Required]
        public int? BooksAddedInYear { get; set; }

        [Required]
        public int? CurrentJournals { get; set; }


        public string? LibraryStaff1 { get; set; }
        public string? LibraryStaff2 { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class MedicalLibraryOtherDetailsViewModel : IValidatableObject
    {
        public int? DigitalValuationId { get; set; }

        // Section 8
        [Required(ErrorMessage = "Please select Yes or No")]
        [RegularExpression("Yes|No")]
        public string? HasDigitalValuationCentre { get; set; }

        
        public int? NoOfSystems { get; set; }

        
        public string? HasStableInternet { get; set; }

       
        public string? HasCccameraSystem { get; set; }

        // Section 9
        public IFormFile? SpecialFeaturesPdf { get; set; }

        public string? UploadedFileName { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? HasSpecialFeaturesPdf { get; set; }

        [Required(ErrorMessage = "Please select Yes or No")]
        public string? SpecialFeaturesQuestion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Digital Valuation – required only if YES
            if (HasDigitalValuationCentre == "Yes")
            {
                if (NoOfSystems == null)
                    yield return new ValidationResult(
                        "Enter number of systems",
                        new[] { nameof(NoOfSystems) });

                if (string.IsNullOrEmpty(HasStableInternet))
                    yield return new ValidationResult(
                        "Select Internet availability",
                        new[] { nameof(HasStableInternet) });

                if (string.IsNullOrEmpty(HasCccameraSystem))
                    yield return new ValidationResult(
                        "Select CCTV availability",
                        new[] { nameof(HasCccameraSystem) });
            }

            // Special Features PDF – already correct
            // Special Features PDF – required ONLY if user selected YES
            if (SpecialFeaturesQuestion == "Yes")
            {
                if (SpecialFeaturesPdf == null &&
                    string.IsNullOrEmpty(UploadedFileName))
                {
                    yield return new ValidationResult(
                        "Please upload Special Features PDF",
                        new[] { nameof(SpecialFeaturesPdf) }
                    );
                }
            }

        }


    }
}




