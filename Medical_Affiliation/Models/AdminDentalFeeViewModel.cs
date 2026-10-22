using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class AdminDentalFeeViewModel
    {
        public string? CollegeCode { get; set; }

        public string? CollegeName { get; set; }

        public string? TypeOfInstitution { get; set; }

        public bool IsGovernment { get; set; }

        public int FacultyCode { get; set; } = 2;

        public int? AffiliationTypeId { get; set; }

        public string? CourseLevel { get; set; }

        public List<SelectListItem> Colleges { get; set; } = new();

        public List<SelectListItem> AffiliationTypes { get; set; } = new();

        public List<SelectListItem> CourseLevels { get; set; } = new();

        public List<AdminDentalFeeTypeViewModel> DentalFeeTypes { get; set; } = new();

        public List<AdminDentalFeeItemViewModel> FeeItems { get; set; }
            = new();

        public List<AdminDentalPaymentViewModel> Payments { get; set; }
            = new();

        public decimal TotalApplicableFee { get; set; }

        public decimal TotalAmountPaid { get; set; }

        public decimal BalanceAmount =>
            TotalApplicableFee - TotalAmountPaid;

        public DateTime CurrentUtc { get; set; }
    }

    public class AdminDentalFeeItemViewModel
    {
        // Existing TxnDentalFeeStructure record
        public int? Id { get; set; }

        // MstDentalFeeTypes details
        public int FeeTypeId { get; set; }

        public string? FeeTypeName { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime? ActivationDate { get; set; }

        // Determined by server-side eligibility rules
        public bool IsApplicable { get; set; }

        public string? ApplicabilityReason { get; set; }

        // Fee structure details
        public int? DentalFeeStructureId { get; set; }

        public string? CourseName { get; set; }

        public int? CourseCode { get; set; }

        public string? CourseLevel { get; set; }

        public string? CalculationType { get; set; }

        public int? AcademicIntake2026 { get; set; }

        // Amounts
        public decimal AmountToBePaid { get; set; }

        public decimal CalculatedAmount { get; set; }

        public bool IsActive { get; set; }

        public bool HasExistingRecord => Id.HasValue;
    }

    public class AdminDentalPaymentViewModel
    {
        public int Id { get; set; }

        public string? CollegeCode { get; set; }

        public int FacultyCode { get; set; }

        public int AffiliationTypeId { get; set; }

        public string? CourseLevel { get; set; }

        public string? TransactionId { get; set; }

        public string? TransactionReceiptPath { get; set; }

        public decimal AmountPaid { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? CreatedBy { get; set; }

        public string? ModifiedBy { get; set; }
    }

    public class AdminDentalFeeAuditViewModel
    {
        public int Id { get; set; }

        public string? CollegeCode { get; set; }

        public int? FeeStructureTransactionId { get; set; }

        public int? PaymentId { get; set; }

        public string? FieldName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? Reason { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
    }

    public class AdminDentalFeeTypeViewModel
    {
        public int Id { get; set; }

        public int FacultyCode { get; set; }

        public string FeeType { get; set; } = null!;

        public string? CourseLevel { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public int AffiliationTypeId { get; set; }

        public DateTime? ActivationDate { get; set; }

        public bool IsCurrentlyApplicable { get; set; }

        public string? ApplicabilityReason { get; set; }
    }


    public class UpdateDentalLateSubmissionFeeViewModel
    {
        public int Id { get; set; }

        public int FacultyCode { get; set; }

        public int AffiliationTypeId { get; set; }

        public string? CourseLevel { get; set; }
        public string? CollegeCode { get; set; }

        public DateTime? ActivationDate { get; set; }

        public bool IsActive { get; set; }
    }
}
