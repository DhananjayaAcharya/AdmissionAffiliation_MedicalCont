using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentalResearchLab
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string? Space { get; set; }

    public string? Equipment { get; set; }

    public string? ResearchProjectsCompletedPast3Yrs { get; set; }

    public string? ResearchProjectsInProgress { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
