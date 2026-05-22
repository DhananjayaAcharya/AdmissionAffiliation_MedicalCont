using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalChair
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int CourseCode { get; set; }

    public string? CourseLevel { get; set; }

    public string? CourseName { get; set; }

    public int? ChairsRequired { get; set; }

    public int? ChairsExisting { get; set; }

    public int SeatSlab { get; set; }

    public string SeatSlabId { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
