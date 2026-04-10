using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingInstituteDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string InstituteName { get; set; } = null!;

    public string InstituteAddress { get; set; } = null!;

    public string? TrustSocietyName { get; set; }

    public string? YearOfEstablishmentOfTrust { get; set; }

    public string? YearOfEstablishmentOfCollege { get; set; }

    public string? InstitutionType { get; set; }

    public string? HodofInstitution { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Age { get; set; }

    public string? TeachingExperience { get; set; }

    public string? Degree { get; set; }

    public string? CourseSelectedSpecialities { get; set; }

    public string Qualifications { get; set; } = null!;

    public string HighestQualification { get; set; } = null!;
}
