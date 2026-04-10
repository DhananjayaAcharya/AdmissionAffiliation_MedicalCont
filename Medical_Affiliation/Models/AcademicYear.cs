using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AcademicYear
{
    public Guid AcademicYearId { get; set; }

    public string YearLabel { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
