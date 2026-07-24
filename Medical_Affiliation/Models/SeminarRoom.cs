using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class SeminarRoom
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string? SpaceAndFacility { get; set; }

    public string? InternetFacility { get; set; }

    public string? AudiovisualEquipmentDetails { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
