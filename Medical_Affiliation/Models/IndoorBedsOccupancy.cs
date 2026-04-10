using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class IndoorBedsOccupancy
{
    public int Id { get; set; }

    public int AffiliationTypeId { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int DepartmentId { get; set; }

    public string SeatSlabId { get; set; } = null!;

    public int Rguhsintake { get; set; }

    public int CollegeIntake { get; set; }

    public string? CourseLevel { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
