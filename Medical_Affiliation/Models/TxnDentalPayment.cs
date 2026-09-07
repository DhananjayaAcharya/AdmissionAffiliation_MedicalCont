using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnDentalPayment
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string TransactionId { get; set; } = null!;

    public string TransactionReceiptPath { get; set; } = null!;

    public decimal AmountPaid { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? CourseLevel { get; set; }

    public virtual ICollection<TxnDentalFeeStructure> TxnDentalFeeStructures { get; set; } = new List<TxnDentalFeeStructure>();
}
