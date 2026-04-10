using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class IndoorInfrastructureRequirementsCompliance
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public int HospitalDetailsId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int RequirementId { get; set; }

    public bool IsCompliant { get; set; }

    public string? Remarks { get; set; }

    public DateTime InspectedOn { get; set; }

    public string? SectionCode { get; set; }

    public string? CourseLevel { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;
}
