using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AyurvedicCollegeMaster
{
    public int Id { get; set; }

    public double? SlNo { get; set; }

    public string? CollegeName { get; set; }

    public string? Address { get; set; }

    public string? PrincipalName { get; set; }

    public string? OfficelandlineNumber { get; set; }

    public double? PrincipalMobileNo { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public double? SecondaryMobileNo { get; set; }

    public double? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? SecondaryEmail { get; set; }
}
