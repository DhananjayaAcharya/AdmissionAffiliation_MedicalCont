using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class RguhsIntakeChangeAndApproval
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? CourseCode { get; set; }

    public int? RguhsIntake { get; set; }

    public int? ApprovalByAdmin { get; set; }

    public string? RemarksForIntakeChange { get; set; }
}
