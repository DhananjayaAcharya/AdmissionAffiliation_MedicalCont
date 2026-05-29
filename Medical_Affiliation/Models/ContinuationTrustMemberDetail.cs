using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ContinuationTrustMemberDetail
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string TrustMemberName { get; set; } = null!;

    public string? Designation { get; set; }

    public string? Qualification { get; set; }

    public string? MobileNumber { get; set; }

    public int? Age { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public string? DesignationId { get; set; }

    public string? Ipaddress { get; set; }

    public string? Browser { get; set; }

    public string? DeviceType { get; set; }

    public byte[] RowTimestamp { get; set; } = null!;
}
