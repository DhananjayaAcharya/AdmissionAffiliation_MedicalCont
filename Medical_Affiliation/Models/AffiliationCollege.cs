using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationCollege
{
    public int SlNo { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string? CollegeTown { get; set; }
}
