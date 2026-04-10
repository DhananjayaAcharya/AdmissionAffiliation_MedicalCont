using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstStudentFacility
{
    public int FacilityTypeId { get; set; }

    public string? FacilityTypeName { get; set; }

    public int FacultyId { get; set; }
}
