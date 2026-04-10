using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class SeatSlabMasterRguh
{
    public int SeatSlabId { get; set; }

    public string? SeatSlabDescription { get; set; }

    public int? SeatSlabFrom { get; set; }

    public int? SeatSlabTo { get; set; }

    public int FacultyId { get; set; }

    public string CourseLevel { get; set; } = null!;

    public int Typeofdboiliation { get; set; }
}
