using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingFacultyDetail
{
    public int Id { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string NameOfFaculty { get; set; } = null!;

    public string? Subject { get; set; }

    public string Designation { get; set; } = null!;

    public string? RecognizedPgTeacher { get; set; }

    public string Mobile { get; set; } = null!;

    public string? Email { get; set; }

    public string Pan { get; set; } = null!;

    public string Aadhaar { get; set; } = null!;

    public string? DepartmentDetails { get; set; }
}
