using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class SeatSlabMaster
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string SeatSlabId { get; set; } = null!;

    public int SeatSlab { get; set; }
}
