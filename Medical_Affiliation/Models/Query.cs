using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Query
{
    public string? Id { get; set; }

    public string? CollegeName { get; set; }

    public string? CollegeAddress { get; set; }

    public string? CourseName { get; set; }

    public double? ExistingIntake { get; set; }

    public string? CollegeCode { get; set; }

    public double? PresentIntake { get; set; }

    public double? FacultyCode { get; set; }

    public string? CourseCode { get; set; }
}
