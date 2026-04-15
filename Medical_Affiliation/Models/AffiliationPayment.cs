using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationPayment
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string TransactionReferenceNo { get; set; } = null!;

    public string? SupportingDocument { get; set; }

    public string? RegistrationNumber { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
