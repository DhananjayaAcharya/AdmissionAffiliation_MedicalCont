using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffNonTeachingFaculty
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? FacultyName { get; set; }

    public string? Designation { get; set; }

    public string? Qualification { get; set; }

    public string? TotalExperience { get; set; }

    public string? Remarks { get; set; }
}
