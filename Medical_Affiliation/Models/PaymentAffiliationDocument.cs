using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class PaymentAffiliationDocument
{
    public int PaymentDocumentId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string CourseLevel { get; set; } = null!;

    public int AffiliationTypeId { get; set; }

    public string TransactionId { get; set; } = null!;

    public decimal? PaymentAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? ScreenshotFileName { get; set; }

    public string? ScreenshotFilePath { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public Guid PublicAccessToken { get; set; }

    public virtual ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
}
