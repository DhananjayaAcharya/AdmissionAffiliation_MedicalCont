using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffAllFacultyDetailswithdatum
{
    public int FacultyId { get; set; }

    public string RegistrationNumber { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string TeachingFacultyName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string AdharNumber { get; set; } = null!;

    public string? PanNumber { get; set; }

    public string? AdharFile { get; set; }

    public string? PanFile { get; set; }

    public string FacultyDepartment { get; set; } = null!;

    public string FacultyDesignation { get; set; } = null!;

    public string? Qualification { get; set; }

    public string? UgInstituteName { get; set; }

    public string? PgInstituteName { get; set; }

    public int? UgYearOfPassing { get; set; }

    public int? PgYearOfPassing { get; set; }

    public decimal? UgExperience { get; set; }

    public decimal? PgExperience { get; set; }

    public DateOnly DateOfJoining { get; set; }

    public string? Form16BankStatementFile { get; set; }

    public bool? RecognizedPgGuide { get; set; }
}
