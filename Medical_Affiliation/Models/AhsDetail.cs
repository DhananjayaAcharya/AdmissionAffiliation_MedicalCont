using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsDetail
{
    public int RegistrationNumber { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string InstituteName { get; set; } = null!;

    public string? InstituteAddress { get; set; }

    public DateTime? YearOfEstablishmentOfTrust { get; set; }

    public DateTime? YearOfEstablishmentOfCollege { get; set; }

    public string? InstitutionType { get; set; }

    public string? TrustSocietyName { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;
}
