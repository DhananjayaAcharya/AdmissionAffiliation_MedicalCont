using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedLibCommittee
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public int CommitteeId { get; set; }

    public string IsPresent { get; set; } = null!;

    public string? CommitteePdfName { get; set; }

    public string? CourseLevel { get; set; }

    public string? CommitteePdfPath { get; set; }
}
