using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedCollegeProfile
{
    public int CgpId { get; set; }

    public string CgpCollegeCode { get; set; } = null!;

    public string? CgpCollegeName { get; set; }

    public string? CgpLogoPath { get; set; }

    public string? CgpAddress { get; set; }

    public string? CgpEmail { get; set; }

    public string? CgpPhoneNumber { get; set; }

    public string? CgpWebsite { get; set; }

    public int? CgpEstablishedYear { get; set; }

    public string? CgpPrincipalName { get; set; }

    public DateTime CgpCreatedDate { get; set; }

    public DateTime? CgpModifiedDate { get; set; }

    public string? CgpModifiedBy { get; set; }
}
