using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedCaAccountAndFeeDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public byte[]? GoverningCouncilPdf { get; set; }

    public string? GoverningCouncilPdfName { get; set; }

    public string AuthorityNameAddress { get; set; } = null!;

    public string AuthorityContact { get; set; } = null!;

    public decimal RecurrentAnnual { get; set; }

    public decimal NonRecurrentAnnual { get; set; }

    public decimal Deposits { get; set; }

    public decimal TuitionFee { get; set; }

    public decimal SportsFee { get; set; }

    public decimal UnionFee { get; set; }

    public decimal LibraryFee { get; set; }

    public decimal OtherFee { get; set; }

    public decimal TotalFee { get; set; }

    public string AccountBooksMaintained { get; set; } = null!;

    public byte[]? AccountSummaryPdf { get; set; }

    public string? AccountSummaryPdfName { get; set; }

    public string AccountsAudited { get; set; } = null!;

    public byte[]? AuditedStatementPdf { get; set; }

    public string? AuditedStatementPdfName { get; set; }

    public string CourseLevel { get; set; } = null!;

    public string? DonationLevied { get; set; }

    public byte[]? DonationPdf { get; set; }

    public string? DonationPdfName { get; set; }
}
