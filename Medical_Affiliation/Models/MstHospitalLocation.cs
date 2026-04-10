using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstHospitalLocation
{
    public int FacultyId { get; set; }

    public int LocationId { get; set; }

    public string? LocationDescription { get; set; }
}
