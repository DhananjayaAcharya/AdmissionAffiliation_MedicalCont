using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_FinancialDetailsViewModel
    {
        public string? RegistrationNo { get; set; }
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }

        // === MAIN FORM FIELDS ===
        [Required(ErrorMessage = "Annual Budget is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Annual Budget must be greater than zero.")]
        public decimal? AnnualBudget { get; set; }

        // File upload can't be validated with [Required] directly, we'll do it in POST
        public IFormFile? AuditedExpenditureUpload { get; set; }

        public string? AuditedExpenditureFileName { get; set; }

        [Required(ErrorMessage = "Deposits Held is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Deposits must be zero or greater.")]
        public decimal? DepositsHeld { get; set; }

        [Required(ErrorMessage = "Tuition Fee is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Tuition Fee must be zero or greater.")]
        public decimal? TuitionFee { get; set; }

        [Required(ErrorMessage = "Union Fee is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Union Fee must be zero or greater.")]
        public decimal? UnionFee { get; set; }

        [Required(ErrorMessage = "Sports Fee is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Sports Fee must be zero or greater.")]
        public decimal? SportsFee { get; set; }

        [Required(ErrorMessage = "Library Fee is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Library Fee must be zero or greater.")]
        public decimal? LibraryFee { get; set; }

        [Required(ErrorMessage = "Others Fee is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Others Fee must be zero or greater.")]
        public decimal? OthersFee { get; set; }

        [Required(ErrorMessage = "Please select whether account books are maintained.")]
        public string? AccountBooksMaintained { get; set; }

        [Required(ErrorMessage = "Please select whether accounts are duly audited.")]
        public string? AccountsDulyAudited { get; set; }

        public List<CA_FinancialCourseRowViewModel> Rows { get; set; } = new();
        public List<MstCourse> CourseMasterList { get; set; } = new();
        
        public List<CaCourseDetailsInFinancialDetail> CourseRows { get; set; } = new();
        public CaFinancialDetail? MainForm { get; set; }
    }
}
