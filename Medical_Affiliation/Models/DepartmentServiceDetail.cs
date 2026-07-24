using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentServiceDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string ServiceName { get; set; } = null!;

    public int ServiceSequence { get; set; }

    public string? IsAvailable { get; set; }

    public string? Remarks { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
