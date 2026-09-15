using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class VwApplicationDate
{
    public int EventId { get; set; }

    public string Title { get; set; } = null!;

    public int? FacultyId { get; set; }

    public DateTime? ApplicationStartDate { get; set; }

    public DateTime? ApplicationEndDate { get; set; }
}
