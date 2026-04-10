using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffSanctionedIntakeForCourse
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string SanctionedIntake { get; set; } = null!;

    public string? EligibleSeatSlab { get; set; }

    public byte[]? DocumentData { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedBy { get; set; }
}
