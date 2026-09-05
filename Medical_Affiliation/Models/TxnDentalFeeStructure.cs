using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnDentalFeeStructure
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public int FeeTypeId { get; set; }

    public int? DentalFeeStructureId { get; set; }

    public string? CourseName { get; set; }

    public int? CourseCode { get; set; }

    public string? CourseLevel { get; set; }

    public decimal AmountToBePaid { get; set; }

    public string? CalculationType { get; set; }

    public int? AcademicIntake2026 { get; set; }

    public decimal CalculatedAmount { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? DentalPaymentId { get; set; }

    public virtual MstAffiliationType AffiliationType { get; set; } = null!;

    public virtual MstDentalFeeStructure? DentalFeeStructure { get; set; }

    public virtual TxnDentalPayment? DentalPayment { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual MstDentalFeeType FeeType { get; set; } = null!;
}
