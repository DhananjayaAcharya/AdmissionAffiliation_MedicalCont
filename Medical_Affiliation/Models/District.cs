using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class District
{
    public string DistrictId { get; set; } = null!;

    public string DistrictNameEng { get; set; } = null!;

    public string DistrictNameKan { get; set; } = null!;

    public int? FromPostalCode { get; set; }

    public int? ToPostalCode { get; set; }

    public string? StateId { get; set; }
}
