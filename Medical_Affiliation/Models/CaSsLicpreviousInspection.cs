using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaSsLicpreviousInspection
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public DateOnly? InspectionDate { get; set; }

    public string? ActionTaken { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CoursesApplied { get; set; }
}
