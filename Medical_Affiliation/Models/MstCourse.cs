using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstCourse
{
    public int Id { get; set; }

    public int CourseCode { get; set; }

    public string CourseName { get; set; } = null!;

    public int? FacultyCode { get; set; }

    public string CourseLevel { get; set; } = null!;

    public string CoursePrefix { get; set; } = null!;

    public string SubjectName { get; set; } = null!;
}
