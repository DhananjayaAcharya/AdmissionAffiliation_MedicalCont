using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedLibTechnicalProcess
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string ProcessName { get; set; } = null!;

    public string? Value { get; set; }

    public string CourseLevel { get; set; } = null!;
}
