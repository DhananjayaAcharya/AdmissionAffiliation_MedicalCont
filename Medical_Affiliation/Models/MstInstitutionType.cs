using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstInstitutionType
{
    public int InstitutionTypeId { get; set; }

    public string? InstitutionType { get; set; }

    public string? OrganizationCategory { get; set; }

    public int? FacultyId { get; set; }
}
