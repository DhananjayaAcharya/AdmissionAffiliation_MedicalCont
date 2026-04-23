using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstMedCommitteeName
{
    public int Id { get; set; }

    public string CommitteeName { get; set; } = null!;

    public string? FacultyCode { get; set; }

    public string? SubFacultyCode { get; set; }
    public string? CourseLevel { get; set; }
}
