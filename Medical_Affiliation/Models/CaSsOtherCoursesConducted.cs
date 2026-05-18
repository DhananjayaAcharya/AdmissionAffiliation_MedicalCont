using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaSsOtherCoursesConducted
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string? SanctionedIntake { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? FileName { get; set; }

    public string? CourseLevel { get; set; }

    public string? CoursesApplied { get; set; }

    public string? DocumentPath { get; set; }
}
