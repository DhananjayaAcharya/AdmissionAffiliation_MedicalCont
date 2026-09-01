using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnCollegeFeePayment
{
    public int PaymentId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int AffiliationTypeId { get; set; }

    public string? ApplicationNo { get; set; }

    public string AcademicYear { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? PaymentMode { get; set; }

    public string? TransactionRefNo { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MstAffiliationType AffiliationType { get; set; } = null!;

    public virtual ICollection<TxnCollegeFeePaymentDetail> TxnCollegeFeePaymentDetails { get; set; } = new List<TxnCollegeFeePaymentDetail>();
}
