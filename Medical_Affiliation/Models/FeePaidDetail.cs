using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FeePaidDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int SlNo { get; set; }

    public string? Particulars { get; set; }

    public decimal? Amount { get; set; }

    public string? TransactionId { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public string? BankName { get; set; }

    public string? BankBranch { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
