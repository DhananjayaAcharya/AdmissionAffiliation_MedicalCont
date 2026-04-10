using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstHostelFacility
{
    public int HostelFacilityId { get; set; }

    public string? HostelFacilityName { get; set; }

    public int FacultyId { get; set; }
}
