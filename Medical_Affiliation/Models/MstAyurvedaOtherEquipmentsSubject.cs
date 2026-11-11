using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;


public partial class MstAyurvedaOtherEquipmentsSubject
{
    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; } = null!;

    public int FacultyId { get; set; }
}
