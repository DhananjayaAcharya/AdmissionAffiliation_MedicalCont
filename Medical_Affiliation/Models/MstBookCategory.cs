using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstBookCategory
{
    public long? Spid { get; set; }

    public string? Category { get; set; }

    public int? FacultyId { get; set; }
}
