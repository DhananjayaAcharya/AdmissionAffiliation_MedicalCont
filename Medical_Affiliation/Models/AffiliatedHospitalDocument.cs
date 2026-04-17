using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliatedHospitalDocument
{
    public int DocumentId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? HospitalType { get; set; }

    public string? HospitalName { get; set; }

    public int? TotalBeds { get; set; }

    public string? DocumentName { get; set; }

    public string? DocumentFilePth { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CourseLevel { get; set; }
}
