using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaDentalLibraryRecord
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string? CourseLevel { get; set; }

    public int AffiliationType { get; set; }

    public int RecordId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public DateTime? CreatedDate { get; set; }
}
