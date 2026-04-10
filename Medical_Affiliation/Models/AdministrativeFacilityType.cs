using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AdministrativeFacilityType
{
    public int FacilityId { get; set; }

    public string FacilityName { get; set; } = null!;

    public string Category { get; set; } = null!;

    public decimal? MinAreaSqM { get; set; }

    public bool IsMandatory { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
