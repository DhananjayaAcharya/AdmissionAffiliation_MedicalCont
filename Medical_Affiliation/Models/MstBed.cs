using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstBed
{
    public int FacultyId { get; set; }

    public int BedId { get; set; }

    public int? BedCount { get; set; }
}
