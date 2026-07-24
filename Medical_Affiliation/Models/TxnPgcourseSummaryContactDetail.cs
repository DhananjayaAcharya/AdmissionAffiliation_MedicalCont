using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnPgcourseSummaryContactDetail
{
    public int PgcourseSummaryContactDetailId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int PgcourseSummaryDetailId { get; set; }

    public string EntityType { get; set; } = null!;

    public int EntitySequence { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? State { get; set; }

    public string? PinCode { get; set; }

    public string? PhoneOffice { get; set; }

    public string? PhoneResidence { get; set; }

    public string? Fax { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual TxnPgcourseSummaryDetail PgcourseSummaryDetail { get; set; } = null!;
}
