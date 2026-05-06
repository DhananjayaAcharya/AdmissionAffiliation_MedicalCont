using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UgandPgrepository
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? IntakeDetails { get; set; }

    public string Course { get; set; } = null!;

    public string? FreshOrIncrease { get; set; }

    public DateOnly? FirstLopdate { get; set; }

    public string? NumberOfSeats { get; set; }

    public string? PermittedYear { get; set; }

    public string? RecognizedYear { get; set; }

    public byte[]? Rguhsnotification { get; set; }

    public byte[]? Inc { get; set; }

    public byte[]? Knmc { get; set; }

    public byte[]? Gok { get; set; }

    public string? CourseName { get; set; }

    public byte[]? Ncisc { get; set; }

    public string? CourseLevel { get; set; }
}
