using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class BuildingTypeMaster
{
    public int? BuildingType { get; set; }

    public string? BuildingName { get; set; }

    public int? FacultyId { get; set; }
}
