using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAyurvedaStaffDetail
{
    public int StaffDetailId { get; set; }

    public int StaffId { get; set; }

    public string StaffDepartments { get; set; } = null!;

    public string? StaffSubDepartments { get; set; } = null!;

    public string? IntakeCapacity60 { get; set; }

    public string? IntakeCapacity100 { get; set; }

    public string? IntakeCapacity150 { get; set; }

    public string? IntakeCapacity200 { get; set; }

    public int FacultyId { get; set; }

    public string? StaffCadre { get; set; }
}
