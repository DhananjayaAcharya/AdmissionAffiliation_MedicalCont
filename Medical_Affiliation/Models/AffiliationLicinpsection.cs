using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationLicinpsection
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public DateOnly? PreviousInspectionDate { get; set; }

    public string? ActionTaken { get; set; }

    public DateTime CreatedAt { get; set; }
}
