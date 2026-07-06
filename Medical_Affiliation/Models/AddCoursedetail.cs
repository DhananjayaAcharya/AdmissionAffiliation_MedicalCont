using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AddCoursedetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? TypeOfAffiliation { get; set; }

    public string? CourseLevel { get; set; }

    public string CourseCode { get; set; } = null!;

    public bool AddCourseRequested { get; set; }

    public int? RequestedCourseIntake { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
