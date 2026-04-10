using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaSsLopsavedDate
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int CourseCode { get; set; }

    public string CourseName { get; set; } = null!;

    public int? SanctionedIntake { get; set; }

    public DateOnly? LopDate { get; set; }

    public DateOnly? RecognitionDate { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CoursesApplied { get; set; }
}
