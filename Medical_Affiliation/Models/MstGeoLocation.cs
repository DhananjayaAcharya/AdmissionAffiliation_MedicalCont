using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstGeoLocation
{
    public int? BuildingId { get; set; }

    public string? Type { get; set; }

    public string? BuildingName { get; set; }
}
