using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FellowshipCourseIntake
{
    public int Slno { get; set; }

    public string? CollegeCode { get; set; }

    public int? FacultyCode { get; set; }

    public string? Collegename { get; set; }

    public string? Course { get; set; }

    public string? CourseCode { get; set; }

    public int? Intake2627 { get; set; }

    public string? UgPg { get; set; }
}
