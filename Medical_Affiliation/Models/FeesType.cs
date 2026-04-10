using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FeesType
{
    public int FeesCode { get; set; }

    public string? FeesType1 { get; set; }

    public decimal? Fees { get; set; }

    public int? FacultyId { get; set; }
}
