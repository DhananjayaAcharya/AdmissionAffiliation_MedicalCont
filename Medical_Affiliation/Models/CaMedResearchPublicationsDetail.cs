using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedResearchPublicationsDetail
{
    public int SlNo { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public int? PublicationsNo { get; set; }

    public byte[]? PublicationsPdf { get; set; }

    public string? PublicationsPdfName { get; set; }

    public string? Pi { get; set; }

    public int? Rguhsfunded { get; set; }

    public int? ExternalBodyFunding { get; set; }

    public byte[]? ProjectsPdf { get; set; }

    public string? ProjectsPdfName { get; set; }

    public byte[]? ClinicalTrialsPdf { get; set; }

    public string? ClinicalTrialsPdfName { get; set; }

    public int? StudentsRguhsfunded { get; set; }

    public int? StudentsExternalBodyFunding { get; set; }

    public byte[]? StudentsProjectsPdf { get; set; }

    public string? StudentsProjectsPdfName { get; set; }

    public int? FacultyRguhsfunded { get; set; }

    public int? FacultyExternalBodyFunding { get; set; }

    public byte[]? FacultyProjectsPdf { get; set; }

    public string? FacultyProjectsPdfName { get; set; }

    public string? CourseLevel { get; set; }
}
