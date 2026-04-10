using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblMedicalSkillsLabEquipment
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsRequired { get; set; }

    public bool IsAvailable { get; set; }

    public int? Quantity { get; set; }

    public int DisplayOrder { get; set; }
}
