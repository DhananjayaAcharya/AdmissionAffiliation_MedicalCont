using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class StaffUnitWiseDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string? UnitNo { get; set; }

    public int SrNo { get; set; }

    public string? Designation { get; set; }

    public string? Name { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public string? RelievedRetiredWorking { get; set; }

    public DateOnly? RelievingRetirementDate { get; set; }

    public int? AttendanceDaysForYear { get; set; }

    public decimal? AttendancePercentage { get; set; }

    public string? PhoneNo { get; set; }

    public string? Email { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
