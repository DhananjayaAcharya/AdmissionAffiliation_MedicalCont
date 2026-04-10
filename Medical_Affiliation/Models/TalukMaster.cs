using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TalukMaster
{
    public int Id { get; set; }

    public string TalukId { get; set; } = null!;

    public string DistrictId { get; set; } = null!;

    public string TalukName { get; set; } = null!;
}
