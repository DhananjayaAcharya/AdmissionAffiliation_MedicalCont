using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ApplicationProgress
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int TypeOfAffiliation { get; set; }

    public string CourseLevel { get; set; } = null!;

    public string StepKey { get; set; } = null!;

    public bool IsCompleted { get; set; }

    public DateTime UpdatedOn { get; set; }
}
