using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaFinancialDetail
{
    public int Id { get; set; }

    public string? RegistrationNo { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public decimal? AnnualBudget { get; set; }

    public byte[]? AuditedExpenditureFile { get; set; }

    public string? AuditedExpenditureFileName { get; set; }

    public decimal? DepositsHeld { get; set; }

    public decimal? TuitionFee { get; set; }

    public decimal? UnionFee { get; set; }

    public decimal? SportsFee { get; set; }

    public decimal? LibraryFee { get; set; }

    public decimal? OthersFee { get; set; }

    public string? AccountBooksMaintained { get; set; }

    public string? AccountsDulyAudited { get; set; }

    public DateTime? CreatedAt { get; set; }
}
