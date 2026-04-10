using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FacultyExamResult
{
    public int Id { get; set; }

    public string? Facultycode { get; set; }

    public string? Collegecode { get; set; }

    public string? Course { get; set; }

    public int? Year { get; set; }

    public int? ExamappearedCount { get; set; }

    public int? Passedoutcount { get; set; }

    public decimal? Yearofpercentage { get; set; }
}
