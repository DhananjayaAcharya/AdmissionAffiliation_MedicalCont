using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstMedLibraryEquipment
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string EquipmentName { get; set; } = null!;
}
