using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstLibraryEquipmentsType
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string TypeOfEquipment { get; set; } = null!;
}
