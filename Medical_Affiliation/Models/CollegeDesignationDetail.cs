using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegeDesignationDetail
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

    public string? UgRguhsintake { get; set; }

    public string? UgPresentintake { get; set; }

    public string? PgRguhsintake { get; set; }

    public string? Goksanctioned { get; set; }

    public string? Pggoksanctioned { get; set; }

    public string? PgPresentintake { get; set; }
}
