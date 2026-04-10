using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TrustMemberDetail
{
    public int Id { get; set; }

    public int FacultyId { get; set; }

    public string TrustMemberName { get; set; } = null!;

    public string? Designation { get; set; }

    public string? Qualification { get; set; }

    public bool? ExistingMember { get; set; }

    public bool? NewMember { get; set; }

    public int? Age { get; set; }

    public DateOnly? JoiningDate { get; set; }
}
