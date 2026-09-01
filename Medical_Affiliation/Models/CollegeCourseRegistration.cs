using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegeCourseRegistration
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public int? FacultyCode { get; set; }
}
