using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class IntakeDetailsLatest
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int? ExistingIntakeCa { get; set; }

    public int? AdditionalSeatRequested { get; set; }

    public int? NewCourseSeatRequested { get; set; }

    public int? TotalIntake { get; set; }

    public int? IsDeclared { get; set; }

    public string? PrincipalName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public byte[]? BankStatement { get; set; }

    public string? Nmcdata { get; set; }

    public byte[]? Nmcdoc { get; set; }

    public string? ExsistingIntake2425 { get; set; }

    public string? ExsistingIntake2526 { get; set; }

    public byte[]? Nmcfor202526 { get; set; }

    public string? RequestingIntake26 { get; set; }

    public DateOnly? CourseRequestingYear { get; set; }
}
