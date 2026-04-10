using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class HospitalFacility
{
    public int HospitalDetailsId { get; set; }

    public int FacilityId { get; set; }

    public int FacultyCode { get; set; }

    public string? CourseLevel { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;
}
