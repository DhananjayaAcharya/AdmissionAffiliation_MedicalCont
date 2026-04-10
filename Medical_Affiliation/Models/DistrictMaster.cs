using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DistrictMaster
{
    public string DistrictId { get; set; } = null!;

    public string DistrictName { get; set; } = null!;

    public string FromPostalCode { get; set; } = null!;

    public string ToPostalCode { get; set; } = null!;

    public string StateId { get; set; } = null!;

    public virtual ICollection<HospitalDetailsForAffiliation> HospitalDetailsForAffiliations { get; set; } = new List<HospitalDetailsForAffiliation>();
}
