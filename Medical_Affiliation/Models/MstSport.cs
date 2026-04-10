using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstSport
{
    public int SportsCode { get; set; }

    public string? SportsName { get; set; }

    public string? SportsType { get; set; }

    public int FacultyId { get; set; }
}
