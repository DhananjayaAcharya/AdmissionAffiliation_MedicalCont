using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLicAcademicCouncilMember
{
    public int Id { get; set; }

    public string AcmemberName { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string? EmailId { get; set; }

    public string? MobileNumber { get; set; }

    public string? Address { get; set; }

    public string AcCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
