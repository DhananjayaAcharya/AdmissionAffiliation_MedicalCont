using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliatedHospitalDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? CollegeName { get; set; }

    public string? AffiliationHospitalName { get; set; }

    public int? TotalNoOfBeds { get; set; }

    public byte[]? ClinicalLetter { get; set; }
}
