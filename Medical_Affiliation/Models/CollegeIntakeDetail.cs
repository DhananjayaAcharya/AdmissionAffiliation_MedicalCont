using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegeIntakeDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? CourseLevel { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string? NoOfSeatsIntake { get; set; }

    public byte[]? Document { get; set; }

    public bool? IsRemarksExist { get; set; }

    public string? Remarks { get; set; }
}
