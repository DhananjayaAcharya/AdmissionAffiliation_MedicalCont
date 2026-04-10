using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ClinicalFacilityDocMaster
{
    public int DocId { get; set; }

    public string? DocumentName { get; set; }

    public int FacultyId { get; set; }
}
