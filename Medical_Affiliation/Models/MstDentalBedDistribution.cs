using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalBedDistribution
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int SeatSlab { get; set; }

    public string WardName { get; set; } = null!;

    public int BedRequirement { get; set; }

    public virtual ICollection<DentalWardBedDistribution> DentalWardBedDistributions { get; set; } = new List<DentalWardBedDistribution>();

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
