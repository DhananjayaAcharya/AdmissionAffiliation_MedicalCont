using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class OtherCourseObservership
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string? NameOfQualificationCourse { get; set; }

    public string? PermittedByMciNmc { get; set; }

    public int? NumberOfAdmissionsPerYear { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
