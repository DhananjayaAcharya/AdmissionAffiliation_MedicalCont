using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TeachingStaffDepartmentWiseDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? CourseLevel { get; set; }

    public string? DepartmentCode { get; set; }

    public string? DesignationCode { get; set; }

    public string? DesignationName { get; set; }

    public DateOnly? Ugfrom { get; set; }

    public DateOnly? Ugto { get; set; }

    public DateOnly? Pgfrom { get; set; }

    public DateOnly? Pgto { get; set; }

    public decimal? TotalExperience { get; set; }

    public string? UgcollegeCode { get; set; }

    public string? PgcollegeCode { get; set; }

    public string? NameOfFaculty { get; set; }
}
