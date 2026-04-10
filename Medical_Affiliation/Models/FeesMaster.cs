using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FeesMaster
{
    public int? FacultyId { get; set; }

    public string? CourseCode { get; set; }

    public int? FeesCode { get; set; }

    public int? Typeofdboiliation { get; set; }

    public int? SeatSlabId { get; set; }

    public decimal? Fees { get; set; }

    public decimal? Scstfees { get; set; }

    public int AdmissionSeatSlab { get; set; }
}
