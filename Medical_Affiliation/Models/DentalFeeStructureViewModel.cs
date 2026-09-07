using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public class DentalFeeStructureViewModel
{
    public string CollegeCode { get; set; } = null!;
    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string? AffiliationCategory { get; set; }

    public List<DentalFeeTypeRowViewModel> FeeTypes { get; set; }
        = new();

    public List<DentalOtherFeeViewModel> OtherFees { get; set; }
        = new();

    public List<DentalCollegeCourseViewModel> ApplicableCourses { get; set; }
    = new List<DentalCollegeCourseViewModel>();

    // =====================================================
    // PAYMENT / TRANSACTION DETAILS
    // =====================================================

    public int? PaymentId { get; set; }
    public string? TransactionId { get; set; }
    public string? TransactionReceiptPath { get; set; }
    public decimal AmountPaid { get; set; }
    public IFormFile? TransactionReceipt {  get; set; }
}

public class DentalFeeTypeRowViewModel
{
    public int FeeTypeId { get; set; }

    public string FeeType { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public List<DentalCourseFeeViewModel> CourseFees { get; set; }
        = new();
}

public class DentalCourseFeeViewModel
{
    public int? Id { get; set; }

    public int? DentalFeeStructureId { get; set; }

    public string CourseName { get; set; } = null!;

    public int? CourseCode { get; set; }

    public string? CourseLevel { get; set; }

    public decimal? AmountToBePaid { get; set; }

    public int AcademicIntake2026 { get; set; }

    public string? CalculationType { get; set; }
    public decimal CalculatedAmount { get; set; }
    public bool IsApplicable { get; set; } = true;

}

public class DentalOtherFeeViewModel
{
    public int? Id { get; set; }
    public int? DentalOtherFeeStructureId { get; set; }

    public string FeeName { get; set; } = null!;

    public decimal AmountToBePaid { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsApplicable { get; set; } = true;

}

/* ============================================================
   PURPOSE:
   Temporary ViewModel used in the GET method to map the
   college's Dental courses with their AY 2026 academic intake.

   This is used internally while building the fee structure.
   ============================================================ */

public class DentalCollegeCourseViewModel
{
    public int CourseCode { get; set; }

    public string CourseName { get; set; } = null!;

    public string? CourseLevel { get; set; }

    // AY 2026 Existing Intake
    public int Ay2026ExistingIntake { get; set; }

    // AY 2026 Additional Requested Intake
    public int Ay2026AddRequestedIntake { get; set; }

    // AY 2026 Final / Total Intake
    public int Ay2026TotalIntake { get; set; }
}