using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffDeanAdministrativeExperience
{
    public int Id { get; set; }

    public string? Facultycode { get; set; }

    public string? Collegecode { get; set; }

    public int? DeanId { get; set; }

    public string? PostHeld { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public decimal? TotalExperienceYears { get; set; }

    public string? CourseLevel { get; set; }

    public virtual AffDeanOrDirectorDetail? Dean { get; set; }
}
