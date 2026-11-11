using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblAyurvedaStaffAvailability
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int StaffDetailId { get; set; }

    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public int? IntakeCapacity60 { get; set; }

    public int? IntakeCapacity100 { get; set; }

    public int? IntakeCapacity150 { get; set; }

    public int? IntakeCapacity200 { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? StaffDepartments { get; set; }

    public string? StaffSubDepartments { get; set; }

    public string? StaffCadre { get; set; }
}
