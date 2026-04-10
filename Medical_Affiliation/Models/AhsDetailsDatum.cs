using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsDetailsDatum
{
    public int Id { get; set; }

    public string CollegeAddress { get; set; } = null!;

    public string CollegeName { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public int ExistingIntake { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int CourseCode { get; set; }

    public byte[]? RguhsNotification { get; set; }

    public int? PresentIntake { get; set; }

    public byte[]? DocumentLop { get; set; }
}
