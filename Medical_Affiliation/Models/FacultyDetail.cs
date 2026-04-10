using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FacultyDetail
{
    public int Id { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string NameOfFaculty { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string? RecognizedPgTeacher { get; set; }

    public string Mobile { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Pan { get; set; } = null!;

    public string Aadhaar { get; set; } = null!;

    public string? DepartmentDetails { get; set; }

    public byte[]? GuideRecognitionDoc { get; set; }

    public byte[]? PhDrecognitionDoc { get; set; }

    public byte[]? LitigationDoc { get; set; }

    public string? RemoveRemarks { get; set; }

    public bool? IsRemoved { get; set; }

    public string? RecognizedPhDteacher { get; set; }

    public string? LitigationPending { get; set; }

    public string? IsExaminer { get; set; }

    public string? ExaminerFor { get; set; }
}
