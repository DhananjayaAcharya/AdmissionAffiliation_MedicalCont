using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstRegisterRecord
{
    public int RegisterRecordId { get; set; }

    public string RegisterName { get; set; } = null!;

    public int? FacultyId { get; set; }

    public string? CourseLevel { get; set; }

    public int? AffiliationType { get; set; }
}
