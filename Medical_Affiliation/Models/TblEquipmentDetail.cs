using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblEquipmentDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string EquipmentName { get; set; } = null!;

    public string? Make { get; set; }

    public string? Model { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
