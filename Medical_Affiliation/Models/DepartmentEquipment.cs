using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentEquipment
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string? NameOfEquipment { get; set; }

    public int? NumbersAvailable { get; set; }

    public string? FunctionalStatus { get; set; }

    public string? IsAdequate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
