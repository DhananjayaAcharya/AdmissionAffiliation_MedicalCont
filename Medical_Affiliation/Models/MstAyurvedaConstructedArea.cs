using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAyurvedaConstructedArea
{
    public int ConstructedAreaId { get; set; }

    public string ConstructedAreaName { get; set; } = null!;

    public int FacultyId { get; set; }
}
