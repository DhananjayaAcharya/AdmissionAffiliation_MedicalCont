using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaExaminationScheme
{
    public int ExaminationSchemeId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyId { get; set; }

    public int AffiliationType { get; set; }

    public int SchemeId { get; set; }

    public int NumberOfStudents { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CourseLevel { get; set; }
}
