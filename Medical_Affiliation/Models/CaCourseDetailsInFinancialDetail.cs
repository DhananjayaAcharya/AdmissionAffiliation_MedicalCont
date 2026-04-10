using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaCourseDetailsInFinancialDetail
{
    public int Id { get; set; }

    public string? RegistrationNo { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string CourseCode { get; set; } = null!;

    public string? YearOfStarting { get; set; }

    public int? AdmissionsSanctioned { get; set; }

    public int? AdmissionsAdmitted { get; set; }

    public byte[]? GoksanctionIntakeFile { get; set; }

    public string? GoksanctionIntakeFileName { get; set; }

    public byte[]? ApexBodyPermissionAndIntakeFile { get; set; }

    public string? ApexBodyPermissionAndIntakeFileName { get; set; }

    public byte[]? RguhssanctionIntakeFile { get; set; }

    public string? RguhssanctionIntakeFileName { get; set; }

    public byte[]? GoipermissionFile { get; set; }

    public string? GoipermissionFileName { get; set; }

    public DateTime? CreatedAt { get; set; }
}
