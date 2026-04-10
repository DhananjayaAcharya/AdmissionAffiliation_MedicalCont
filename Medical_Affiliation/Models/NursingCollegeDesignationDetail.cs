using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingCollegeDesignationDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string? DesignationCode { get; set; }

    public string? Department { get; set; }

    public string? DepartmentCode { get; set; }

    public string? SeatSlabId { get; set; }

    public string RequiredIntake { get; set; } = null!;

    public string AvailableIntake { get; set; } = null!;
}
