using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedicalLibraryService
{
    public int LibraryServiceId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationType { get; set; }

    public int ServiceId { get; set; }

    public string IsAvailable { get; set; } = null!;

    public string? UploadedFileName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? RegistrationNo { get; set; }

    public string? CourseLevel { get; set; }

    public string? UploadedPdfPath { get; set; }

    public string? UploadedFileDataPath { get; set; }
}
