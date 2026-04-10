using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaVehicleDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string VehicleRegNo { get; set; } = null!;

    public string VehicleForCode { get; set; } = null!;

    public int? SeatingCapacity { get; set; }

    public DateOnly? ValidityDate { get; set; }

    public string? RcBookStatus { get; set; }

    public string? InsuranceStatus { get; set; }

    public string? DrivingLicenseStatus { get; set; }

    public string? CourseLevel { get; set; }
}
