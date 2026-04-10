using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaLibraryStaffDetail
{
    public int Id { get; set; }

    public string? RegistrationNo { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string StaffName { get; set; } = null!;

    public string? Qualification { get; set; }

    public int? ExperienceYears { get; set; }

    public string? Remarks { get; set; }
}
