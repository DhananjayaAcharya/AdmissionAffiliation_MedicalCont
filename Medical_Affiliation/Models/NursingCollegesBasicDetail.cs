using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingCollegesBasicDetail
{
    public int Id { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? InstituteName { get; set; }

    public string? InstituteAddress { get; set; }

    public string? YearofEstablishmentOfTrust { get; set; }

    public string? YearofEstablishmentOfCollege { get; set; }

    public string? InstitutionType { get; set; }

    public string? HodofInstitution { get; set; }

    public string? TrustSocietyName { get; set; }

    public string? CourseCode { get; set; }

    public string? CourseName { get; set; }
}
