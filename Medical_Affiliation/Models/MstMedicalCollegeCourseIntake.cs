using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstMedicalCollegeCourseIntake
{
    public int Slno { get; set; }

    public string? CollCode { get; set; }

    public string? PvtGovt { get; set; }

    public int? Facultycode { get; set; }

    public string? Collegename { get; set; }

    public string? Address { get; set; }

    public string? District { get; set; }

    public string? Course { get; set; }

    public int? CourseCode { get; set; }

    public int? Intake2025 { get; set; }

    public string? UgPg { get; set; }

    public int? Intake2627 { get; set; }

    public string? AcademicYear { get; set; }

    public string? IncreasedIntake { get; set; }
}
