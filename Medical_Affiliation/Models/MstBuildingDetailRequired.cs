using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstBuildingDetailRequired
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public int BuildingAreaRequiredSqFt { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
