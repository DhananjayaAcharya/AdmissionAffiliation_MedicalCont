using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffFacultyCourseMappingWithDatum
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? CourseCode { get; set; }

    public string? RguhsnotificationNo { get; set; }

    public string? Document { get; set; }

    public DateTime? CreatedAt { get; set; }
}
