using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalAffiliationType
{
    public int DentalAffiliationTypeId { get; set; }

    public int FacultyCode { get; set; }

    public string AffiliationCategory { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CourseLevelGroup { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? TypeId { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual ICollection<MstDentalFeeType> MstDentalFeeTypes { get; set; } = new List<MstDentalFeeType>();
}
