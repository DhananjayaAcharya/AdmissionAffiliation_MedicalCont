using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AssociatedInstitution
{
    public int Id { get; set; }

    public int TypeOfAffiliation { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string AssociatedCollegeCode { get; set; } = null!;

    public string AssociatedFacultyCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? CourseCode { get; set; }

    public string? CourseName { get; set; }

    public string? CourseLevel { get; set; }

    public int AnnualIntake { get; set; }

    public DateTime CreatedOn { get; set; }
}
