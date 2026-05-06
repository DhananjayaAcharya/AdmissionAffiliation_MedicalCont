using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationPgSsCourseDetailsForGok
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string CoursePrefix { get; set; } = null!;

    public int? PresentIntake { get; set; }

    public int? SanctionedIntake { get; set; }

    public DateOnly? Gokdate { get; set; }

    public string AcademicYear { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CourseCode { get; set; } = null!;

    public string? DocumentofGokpath { get; set; }
}
