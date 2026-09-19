using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstFeeHead
{
    public int FeeHeadId { get; set; }

    public string FeeHeadName { get; set; } = null!;

    public string FeeHeadCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<MstAffiliationFeeStructure> MstAffiliationFeeStructures { get; set; } = new List<MstAffiliationFeeStructure>();

    public virtual ICollection<TxnCollegeFeePaymentDetail> TxnCollegeFeePaymentDetails { get; set; } = new List<TxnCollegeFeePaymentDetail>();
}
