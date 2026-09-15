using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class PaymentReceipt
{
    public int ReceiptId { get; set; }

    public int PaymentDocumentId { get; set; }

    public string FilePath { get; set; } = null!;

    public Guid PublicAccessToken { get; set; }

    public DateTime? WhatsAppSentOn { get; set; }

    public string? WhatsAppStatus { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual PaymentAffiliationDocument PaymentDocument { get; set; } = null!;
}
