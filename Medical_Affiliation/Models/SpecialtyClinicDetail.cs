using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class SpecialtyClinicDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string ClinicName { get; set; } = null!;

    public int ClinicSequence { get; set; }

    public string? Weekdays { get; set; }

    public string? Timings { get; set; }

    public int? NumberOfCasesAvg { get; set; }

    public string? ClinicInchargeName { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
