using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstSportsItem
{
    public int SportsCode { get; set; }

    public int SportsItemId { get; set; }

    public string? SportsItemName { get; set; }

    public int FacultyId { get; set; }
}
