using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DummyList
{
    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string NameOfInstitution { get; set; } = null!;

    public string? PrincipalName { get; set; }

    public string? PrincipalMobNo { get; set; }

    public string? HeadOfInstitutionMobNo { get; set; }

    public string? DeanName { get; set; }

    public string? DeanMobileNumber { get; set; }
}
