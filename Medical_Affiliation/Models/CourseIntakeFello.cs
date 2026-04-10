using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CourseIntakeFello
{
    public int Id { get; set; }

    public string? CollegeName { get; set; }

    public string? CollegeAddress { get; set; }

    public string? CourseName { get; set; }

    public int? ExistingIntake { get; set; }

    public string? CollegeCode { get; set; }

    public byte[]? DocumentAffiliation { get; set; }

    public int? PresentIntake { get; set; }

    public byte[]? DocumentLop { get; set; }

    public int? FacultyCode { get; set; }

    public byte[]? CourseCode { get; set; }
}
