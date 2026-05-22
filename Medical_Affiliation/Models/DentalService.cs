using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalService
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public int HospitalDetailsId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int RequirementId { get; set; }

    public bool? AvailabilityStatus { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int SectionCode { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;

    public virtual MstDentalService Requirement { get; set; } = null!;
}
