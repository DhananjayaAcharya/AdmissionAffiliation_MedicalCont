using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAdministrativeFacility
{
    public int FacilityId { get; set; }

    public string? Facilities { get; set; }

    public string? SizeofFacilities { get; set; }

    public int FacultyId { get; set; }

    public string CourseCode { get; set; } = null!;

    public int? NoOfRooms { get; set; }

    public int? IntakeId { get; set; }
}
