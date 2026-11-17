using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicInspectionTeam
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string SenateMemberName { get; set; } = null!;

    public string SenateMemberPhone { get; set; } = null!;

    public string AcademicCouncilName { get; set; } = null!;

    public string AcademicCouncilPhone { get; set; } = null!;

    public string SubjectExpertName { get; set; } = null!;

    public string SubjectExpertPhone { get; set; } = null!;

    public DateOnly? InspectionDate { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? TeamUsername { get; set; }

    public string? ProtectedTeamPassword { get; set; }
}
