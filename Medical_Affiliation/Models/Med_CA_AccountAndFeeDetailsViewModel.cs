using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medical_Affiliation.Models
{
    public class Med_CA_AccountAndFeeDetailsViewModel
    {
        public string CollegeCode { get; set; } = null!;

        public string CourseLevel { get; set; } = null!;
        public string FacultyCode { get; set; } = null!;
        public string? SubFacultyCode { get; set; }
        public string? RegistrationNo { get; set; }

        [Required(ErrorMessage = "Authority Name & Address is required")]
        public string AuthorityNameAddress { get; set; } = "";

        [Required(ErrorMessage = "Authority Contact is required")]
        public string AuthorityContact { get; set; } = "";

        //[Required(ErrorMessage = "Recurrent Annual is required")]
        //public decimal RecurrentAnnual { get; set; }

        //[Required(ErrorMessage = "Non-Recurrent Annual is required")]
        //public decimal NonRecurrentAnnual { get; set; }

        //[Required(ErrorMessage = "Deposits is required")]
        //public decimal Deposits { get; set; }

        //[Required(ErrorMessage = "Tuition Fee is required")]
        //public decimal TuitionFee { get; set; }

        //[Required(ErrorMessage = "Sports Fee is required")]
        //public decimal SportsFee { get; set; }

        //[Required(ErrorMessage = "Union Fee is required")]
        //public decimal UnionFee { get; set; }

        //[Required(ErrorMessage = "Library Fee is required")]
        //public decimal LibraryFee { get; set; }

        //[Required(ErrorMessage = "Other Fee is required")]
        //public decimal OtherFee { get; set; }

        [Required(ErrorMessage = "Recurrent Annual is required")]
        public decimal? RecurrentAnnual { get; set; }

        [Required(ErrorMessage = "Non-Recurrent Annual is required")]
        public decimal? NonRecurrentAnnual { get; set; }

        [Required(ErrorMessage = "Deposits is required")]
        public decimal? Deposits { get; set; }

        [Required(ErrorMessage = "Tuition Fee is required")]
        public decimal? TuitionFee { get; set; }

        [Required(ErrorMessage = "Sports Fee is required")]
        public decimal? SportsFee { get; set; }

        [Required(ErrorMessage = "Union Fee is required")]
        public decimal? UnionFee { get; set; }

        [Required(ErrorMessage = "Library Fee is required")]
        public decimal? LibraryFee { get; set; }

        [Required(ErrorMessage = "Other Fee is required")]
        public decimal? OtherFee { get; set; }


        public decimal TotalFee { get; set; } // Can be calculated if needed

        //[Required(ErrorMessage = "Account Books Maintained is required")]
        //public string AccountBooksMaintained { get; set; } = "N"; // Y/N

        //[Required(ErrorMessage = "Accounts Audited is required")]
        //public string AccountsAudited { get; set; } = "N"; // Y/N


        [Required(ErrorMessage = "Account Books Maintained is required")]
        public string AccountBooksMaintained { get; set; } = string.Empty; // ← FIXED: no default "N"

        [Required(ErrorMessage = "Accounts Audited is required")]
        public string AccountsAudited { get; set; } = string.Empty; // ← FIXED: no default "N"

        public IFormFile? GoverningCouncilPdf { get; set; }
        public string? GoverningCouncilPdfName { get; set; }

        public IFormFile? AccountSummaryPdf { get; set; }
        public string? AccountSummaryPdfName { get; set; }

        public IFormFile? AuditedStatementPdf { get; set; }
        public string? AuditedStatementPdfName { get; set; }

        // ==================== DONATION FIELDS (Only for PG) ====================
        [RequiredIfPG(ErrorMessage = "Please specify whether donation is levied")]
        public string? DonationLevied { get; set; } = string.Empty;          // Y / N   (Only required for PG)

        public IFormFile? DonationPdf { get; set; }
        public string? DonationPdfName { get; set; }

        // Helper property to easily check in View
        public bool IsPG => CourseLevel?.Trim().ToUpper() == "PG";
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfPGAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (Med_CA_AccountAndFeeDetailsViewModel)validationContext.ObjectInstance;

            if (model.IsPG && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? "Donation Levied is required for Postgraduate courses.");
            }

            return ValidationResult.Success;
        }
    }
}