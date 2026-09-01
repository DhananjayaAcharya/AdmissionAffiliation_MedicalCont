using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnCollegeFeePaymentDetail
{
    public int PaymentDetailId { get; set; }

    public int PaymentId { get; set; }

    public int FeeHeadId { get; set; }

    public int? SeatCount { get; set; }

    public decimal Amount { get; set; }

    public virtual MstFeeHead FeeHead { get; set; } = null!;

    public virtual TxnCollegeFeePayment Payment { get; set; } = null!;
}
