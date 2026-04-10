using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffTeachingFacultyAllDetail
{
    public int TeachingFacultyId { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string TeachingFacultyName { get; set; } = null!;

    public string? Subject { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? Mobile { get; set; }

    public string? Email { get; set; }

    public string AadhaarNumber { get; set; } = null!;

    public byte[]? AadhaarDocument { get; set; }

    public string Pannumber { get; set; } = null!;

    public byte[]? Pandocument { get; set; }

    public string? RnRmnumber { get; set; }

    public byte[]? RnRmdocument { get; set; }

    public byte[]? Form16OrLast6MonthsStatement { get; set; }

    public byte[]? AppointmentOrderDocument { get; set; }

    public byte[]? GuideRecognitionDoc { get; set; }

    public byte[]? PhDrecognitionDoc { get; set; }

    public byte[]? LitigationDoc { get; set; }

    public string Department { get; set; } = null!;

    public string? DepartmentDetails { get; set; }

    public string Designation { get; set; } = null!;

    public string Qualification { get; set; } = null!;

    public string UginstituteName { get; set; } = null!;

    public int UgyearOfPassing { get; set; }

    public string PginstituteName { get; set; } = null!;

    public int PgyearOfPassing { get; set; }

    public string? PgpassingSpecialization { get; set; }

    public decimal? TeachingExperienceAfterUgyears { get; set; }

    public decimal? TeachingExperienceAfterPgyears { get; set; }

    public DateOnly DateOfJoining { get; set; }

    public bool? RecognizedPgTeacher { get; set; }

    public bool? RecognizedPhDteacher { get; set; }

    public bool? IsRecognizedPgguide { get; set; }

    public bool? IsExaminer { get; set; }

    public string? ExaminerFor { get; set; }

    public bool? LitigationPending { get; set; }

    public string? Nrtsnumber { get; set; }

    public string? Rguhstin { get; set; }

    public string? RemoveRemarks { get; set; }

    public bool IsRemoved { get; set; }

    public byte[]? OnlineTeachersDatabase { get; set; }

    public byte[]? Madetorecruit { get; set; }
}
