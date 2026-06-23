using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AcademicIntakeYearWise
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public int? ExistingIntake { get; set; }

    public int? AdditionalIntake { get; set; }

    public int? TotalIntake { get; set; }

    public string? ApprovalType { get; set; }

    public DateOnly? LopDate { get; set; }

    public string? DocumentPath { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
