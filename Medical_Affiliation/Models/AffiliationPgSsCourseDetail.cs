using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationPgSsCourseDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string CoursePrefix { get; set; } = null!;

    public int? PresentIntake { get; set; }

    public int? RguhsIntake { get; set; }

    public DateOnly? Lopdate { get; set; }

    public DateOnly? DateofRecognitionByNmc { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CourseCode { get; set; } = null!;
}
