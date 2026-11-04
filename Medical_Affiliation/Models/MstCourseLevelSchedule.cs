using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstCourseLevelSchedule
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CourseLevel { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? LastDate { get; set; }

    public DateOnly? FineLastDate { get; set; }

    public decimal? FineAmount { get; set; }

    public DateTime UpdatedOn { get; set; }
}
