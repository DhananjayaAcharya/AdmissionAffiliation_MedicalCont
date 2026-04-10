using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLicSubjectExpertiseMember
{
    public int Id { get; set; }

    public string ExpMemberName { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string? EmailId { get; set; }

    public string? MobileNumber { get; set; }

    public string? Address { get; set; }

    public string ExpCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
