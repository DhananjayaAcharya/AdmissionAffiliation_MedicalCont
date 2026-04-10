using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsCourseCollegeDatum
{
    public short SlNo { get; set; }

    public string NameOfTheCollege { get; set; } = null!;

    public string NameOfTheCourse { get; set; } = null!;

    public string? CourseCode { get; set; }

    public byte SanctionedIntakeOf1stYear { get; set; }

    public byte TotalAdmissionMade { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public byte[]? Rguhsnotification { get; set; }
}
