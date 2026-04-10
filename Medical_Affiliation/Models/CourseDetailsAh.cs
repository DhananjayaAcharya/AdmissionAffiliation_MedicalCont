using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CourseDetailsAh
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string? RguhsnotificationNo { get; set; }

    public byte[]? Document { get; set; }

    public string IsRecognized { get; set; } = null!;
}
