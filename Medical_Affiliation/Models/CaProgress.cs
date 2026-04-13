using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaProgress
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? CourseLevel { get; set; }

    public string? StepKey { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
