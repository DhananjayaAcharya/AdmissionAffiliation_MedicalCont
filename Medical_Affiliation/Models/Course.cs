using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Course
{
    public int Id { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public int? FacultyId { get; set; }

    public int? Displayorder { get; set; }

    public int? GroupCode { get; set; }
}
