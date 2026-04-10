using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationOtherCoursesPermittedByNmc
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int? PermissionByNmc { get; set; }

    public int? NumberOfAdmissionsPerYear { get; set; }

    public byte[]? NmcsupportingDocument { get; set; }

    public DateTime CreatedAt { get; set; }
}
