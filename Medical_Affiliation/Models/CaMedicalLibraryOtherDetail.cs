using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedicalLibraryOtherDetail
{
    public int DigitalValuationId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationType { get; set; }

    public string? RegistrationNo { get; set; }

    public string HasDigitalValuationCentre { get; set; } = null!;

    public int? NoOfSystems { get; set; }

    public string? HasStableInternet { get; set; }

    public string? HasCccameraSystem { get; set; }

    public string? UploadedFileName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? SpecialFeaturesQuestion { get; set; }

    public string? CourseLevel { get; set; }

    public string? SpecialFeaturesAchievementsPdfPath { get; set; }
}
