using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationPgSsCourseDetailsRguh
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int? RguhsIntake { get; set; }

    public byte[]? RguhssupportingDocument { get; set; }

    public DateTime CreatedAt { get; set; }
}
