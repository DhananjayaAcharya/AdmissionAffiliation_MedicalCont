using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsteachingFacultyDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? CollegeName { get; set; }

    public string? TeachingFacultyName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? AdharNumber { get; set; }

    public byte[]? AdharFile { get; set; }

    public string? PanNumber { get; set; }

    public byte[]? PanFile { get; set; }

    public string? FacultyDepartment { get; set; }

    public string? FacultyDesignation { get; set; }

    public string? Qualification { get; set; }

    public string? UgInstituteName { get; set; }

    public string? PgInstituteName { get; set; }

    public string? UgYearOfPassing { get; set; }

    public string? PgYearOfPassing { get; set; }

    public string? UgExperience { get; set; }

    public string? PgExperience { get; set; }

    public DateOnly? DateOfJoining { get; set; }

    public byte[]? Form16BankStatementFile { get; set; }

    public bool? RecognizedPgGuide { get; set; }
}
