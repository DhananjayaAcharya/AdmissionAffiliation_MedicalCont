using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NonTeachingStaffDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? CourseLevel { get; set; }

    public string? StaffName { get; set; }

    public string? Designation { get; set; }

    public string? MobileNumber { get; set; }

    public string? SalaryPaid { get; set; }
}
