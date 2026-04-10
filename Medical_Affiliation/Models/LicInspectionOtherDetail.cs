using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicInspectionOtherDetail
{
    public int Id { get; set; }

    public string? SenateCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? CollegeName { get; set; }

    public string? MemberName { get; set; }

    public string? MemberCode { get; set; }

    public DateOnly? InspectionDate { get; set; }

    public bool? IsAttended { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Phonenumber { get; set; }
}
