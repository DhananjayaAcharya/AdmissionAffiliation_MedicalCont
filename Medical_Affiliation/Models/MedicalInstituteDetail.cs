using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalInstituteDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string InstituteName { get; set; } = null!;

    public string? TrustSocietyName { get; set; }

    public string? YearOfEstablishmentOfTrust { get; set; }

    public string? YearOfEstablishmentOfCollege { get; set; }

    public string? InstitutionType { get; set; }

    public string? HodofInstitution { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Age { get; set; }

    public string? TeachingExperience { get; set; }

    public string? PgDegree { get; set; }

    public string? SelectedSpecialities { get; set; }

    public string? InstituteAddress { get; set; }

    public string? Course { get; set; }

    public string? Specialisation { get; set; }

    public string? OtherDegree { get; set; }

    public byte[]? EshtablishmentDoc { get; set; }

    public byte[]? TrustDoc { get; set; }

    public string? Taluk { get; set; }

    public string? District { get; set; }
}
