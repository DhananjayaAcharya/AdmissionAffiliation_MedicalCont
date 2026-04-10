using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalCourseDetail
{
    public int Id { get; set; }

    public string? UgIntake { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string FreshOrIncrease { get; set; } = null!;

    public DateOnly? FirstLopdate { get; set; }

    public int? NoOfSeats { get; set; }

    public int? PermittedYear { get; set; }

    public int? RecognizedYear { get; set; }

    public byte[]? Rguhsnotification { get; set; }

    public byte[]? Gmc { get; set; }

    public byte[]? Nmc { get; set; }
}
