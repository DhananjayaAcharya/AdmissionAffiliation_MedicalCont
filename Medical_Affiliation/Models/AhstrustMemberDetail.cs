using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhstrustMemberDetail
{
    public int Id { get; set; }

    public string TrustMemberName { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string Qualification { get; set; } = null!;

    public int? Age { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CollegeCode { get; set; }
}
