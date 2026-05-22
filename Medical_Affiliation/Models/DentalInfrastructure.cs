using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalInfrastructure
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int HospitalDetailsId { get; set; }

    public int RequirementId { get; set; }

    public int SeatSlab { get; set; }

    public decimal RequiredAreaSqFt { get; set; }

    public decimal AvailableAreaSqFt { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;

    public virtual MstDentalInfrastructure Requirement { get; set; } = null!;
}
