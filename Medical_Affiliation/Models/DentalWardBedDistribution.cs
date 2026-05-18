using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalWardBedDistribution
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int HospitalDetailsId { get; set; }

    public int AffiliationTypeId { get; set; }

    public int SeatSlab { get; set; }

    public int WardId { get; set; }

    public string? WardName { get; set; }

    public int BedsRequired { get; set; }

    public int BedsPresent { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;

    public virtual MstDentalBedDistribution Ward { get; set; } = null!;
}
