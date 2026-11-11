using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAyurvedaOtherEquipmentsDetail
{
    public int EquipmentDetailId { get; set; }

    public int EquipmentId { get; set; }

    public string SubSectionName { get; set; } = null!;

    public string EquipmentName { get; set; } = null!;

    public string MinimumNumberRequired { get; set; } = null!;

    public int FacultyId { get; set; }

    public string? MinimumNum100 { get; set; }

    public string? MinimumNum150 { get; set; }

    public string? MinimumNum200 { get; set; }

    public string? MinimumNum60 { get; set; }
}
