using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NaturoPathyCollegeMaster
{
    public double Slno { get; set; }

    public string? CollegeName { get; set; }

    public string? CollegeCode { get; set; }

    public string? Address { get; set; }

    public string? Password { get; set; }

    public string? PrincipalName { get; set; }

    public string? LandLineNo { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? SecondaryMobileNo { get; set; }

    public string? SecondaryEmail { get; set; }

    public double? FacultyCode { get; set; }
}
