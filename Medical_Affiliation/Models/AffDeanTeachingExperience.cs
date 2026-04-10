using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffDeanTeachingExperience
{
    public int Id { get; set; }

    public string? Facultycode { get; set; }

    public string? Collegecode { get; set; }

    public int? DeanId { get; set; }

    public string? Designation { get; set; }

    public DateOnly? Ugfrom { get; set; }

    public DateOnly? Ugto { get; set; }

    public DateOnly? Pgfrom { get; set; }

    public DateOnly? Pgto { get; set; }

    public decimal? TotalExperienceYears { get; set; }

    public string? CourseLevel { get; set; }

    public virtual AffDeanOrDirectorDetail? Dean { get; set; }
}
