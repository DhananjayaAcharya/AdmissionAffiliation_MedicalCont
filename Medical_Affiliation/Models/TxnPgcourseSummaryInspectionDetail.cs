using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnPgcourseSummaryInspectionDetail
{
    public int PgcourseSummaryInspectionDetailId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int PgcourseSummaryDetailId { get; set; }

    public string CourseLevel { get; set; } = null!;

    public int CourseLevelSequence { get; set; }

    public DateOnly? DateOfLastInspection { get; set; }

    public string? Purpose { get; set; }

    public string? Result { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual TxnPgcourseSummaryDetail PgcourseSummaryDetail { get; set; } = null!;
}
