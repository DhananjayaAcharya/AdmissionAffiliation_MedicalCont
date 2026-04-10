using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingCollegeRegistration
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string InstituteName { get; set; } = null!;

    public string? InstituteAddress { get; set; }

    public DateOnly? YearOfEstablishmentOfTrust { get; set; }

    public DateOnly? YearOfEstablishmentOfCollege { get; set; }

    public string? InstitutionType { get; set; }

    public string? HodOfInstitution { get; set; }

    public string? TrustSocietyName { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;
}
