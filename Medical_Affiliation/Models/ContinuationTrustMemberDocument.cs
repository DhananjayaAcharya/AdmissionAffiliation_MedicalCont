using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ContinuationTrustMemberDocument
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? RegisteredTrustMemberDetailsPath { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
