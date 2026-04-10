using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Ugdetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? Ugintake { get; set; }

    public string Course { get; set; } = null!;

    public string? FreshOrIncrease { get; set; }

    public DateOnly? FirstLopdate { get; set; }

    public string? NumberOfSeats { get; set; }

    public string? PermittedYear { get; set; }

    public string? RecognizedYear { get; set; }

    public byte[]? Rguhsnotification { get; set; }

    public byte[]? Gmc { get; set; }

    public byte[]? Nmc { get; set; }

    public string? SeatSlab { get; set; }

    public byte[]? Ksnc { get; set; }

    public int? Total { get; set; }

    public string? CourseLevel { get; set; }

    public string? CourseCode { get; set; }
}
