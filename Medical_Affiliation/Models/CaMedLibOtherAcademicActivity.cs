using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedLibOtherAcademicActivity
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public int ActivityId { get; set; }

    public string DepartmentWise { get; set; } = null!;

    public byte[]? ActivityPdf { get; set; }

    public string? ActivityPdfName { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string? CourseLevel { get; set; }
}
