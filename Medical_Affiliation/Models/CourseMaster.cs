using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CourseMaster
{
    public string CourseCode { get; set; } = null!;

    public string? CourseName { get; set; }

    public string CourseLevel { get; set; } = null!;

    public int? FacultyId { get; set; }

    public int? DisplayOrder { get; set; }

    public int? GroupCode { get; set; }
}
