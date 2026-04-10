using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CoursesOffered
{
    public int Id { get; set; }

    public int TypeOfAffiliation { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? CourseCode { get; set; }

    public string? CourseName { get; set; }

    public string? CourseLevel { get; set; }

    public string? YearOfStarting { get; set; }

    public int? SanctionedAdmissions { get; set; }

    public int? AdmittedAdmissions { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedOn { get; set; }
}
