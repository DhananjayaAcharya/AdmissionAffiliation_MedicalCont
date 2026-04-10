using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffPrincipalDetail
{
    public int Id { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? DeanOrDirectorName { get; set; }

    public string? DeanQualification { get; set; }

    public DateOnly? DeanQualificationDate { get; set; }

    public string? DeanUniversity { get; set; }

    public string? DeanStateCouncilNumber { get; set; }

    public bool? RecognizedByMci { get; set; }

    public string? CourseLevel { get; set; }
}
