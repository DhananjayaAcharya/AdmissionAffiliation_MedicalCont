using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LatestExcelAff
{
    public int Id { get; set; }

    public string CollegeName { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int CourseCode { get; set; }
}
