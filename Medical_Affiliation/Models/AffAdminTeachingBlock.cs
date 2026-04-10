using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffAdminTeachingBlock
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string FacilityId { get; set; } = null!;

    public string Facilities { get; set; } = null!;

    public string SizeSqFtAsPerNorms { get; set; } = null!;

    public string IsAvailable { get; set; } = null!;

    public string NoOfRooms { get; set; } = null!;

    public string SizeSqFtAvailablePerRoom { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public string? CreatedBy { get; set; }
}
