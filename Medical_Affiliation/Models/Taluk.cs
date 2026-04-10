using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Taluk
{
    public string TalukId { get; set; } = null!;

    public string DistrictId { get; set; } = null!;

    public string TalukNameEng { get; set; } = null!;

    public string TalukNameKan { get; set; } = null!;
}
