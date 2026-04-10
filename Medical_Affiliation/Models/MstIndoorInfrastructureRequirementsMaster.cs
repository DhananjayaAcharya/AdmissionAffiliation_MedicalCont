using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstIndoorInfrastructureRequirementsMaster
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string SectionName { get; set; } = null!;

    public string RequirementName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? SectionCode { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
