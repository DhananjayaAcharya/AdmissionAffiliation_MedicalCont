using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAyurvedaConstructedAreaDetail
{
    public int ConstructedAreaDetailId { get; set; }

    public int ConstructedAreaId { get; set; }

    public string MegaSectionName { get; set; } = null!;

    public string SectionName { get; set; } = null!;

    public string? IntakeCapacity60 { get; set; }

    public string? IntakeCapacity100 { get; set; }

    public string? IntakeCapacity150 { get; set; }

    public string? IntakeCapacity200 { get; set; }

    public int FacultyId { get; set; }
}
