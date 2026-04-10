using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingFacultyWithCollege
{
    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string? TeachingFacultyName { get; set; }

    public string? Designation { get; set; }

    public string? AadhaarNumber { get; set; }

    public string? Pannumber { get; set; }

    public int? FacultyCode { get; set; }
}
