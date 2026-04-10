using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationCollegeMasterFaculty4
{
    public string CollegeCode { get; set; } = null!;

    public string? CollegeName { get; set; }

    public string? CollegeTown { get; set; }

    public string? FacultyCode { get; set; }

    public string? Password { get; set; }

    public string? HashedPassword { get; set; }

    public byte[]? AllDocsForCourse { get; set; }

    public string? IsDeclared { get; set; }

    public string? ChangedPassword { get; set; }

    public string? PrincipalNameDeclared { get; set; }
}
