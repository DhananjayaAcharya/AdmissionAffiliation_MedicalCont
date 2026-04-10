using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblLaboratoryAvailability
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public int LabId { get; set; }

    public int? SizeAvailableSqFt { get; set; }

    public int? MannequinAvailable { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
