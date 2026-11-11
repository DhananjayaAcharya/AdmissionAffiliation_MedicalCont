using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblAyurvedaConstructedAreaAvailability
{
    public int Id { get; set; }

    public int ConstructedAreaId { get; set; }

    public int ConstructedAreaDetailId { get; set; }

    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public decimal? IntakeCapacity60 { get; set; }

    public decimal? IntakeCapacity100 { get; set; }

    public decimal? IntakeCapacity150 { get; set; }

    public decimal? IntakeCapacity200 { get; set; }

    public DateTime CreatedOn { get; set; }
}
