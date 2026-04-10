using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLicSenateMember
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string SenateMemberName { get; set; } = null!;

    public string SeCode { get; set; } = null!;

    public string? EmailId { get; set; }

    public string? MobileNumber { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }
}
