using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalFeeType
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string FeeType { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public int AffiliationTypeId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MstAffiliationType AffiliationType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual ICollection<MstDentalFeeStructure> MstDentalFeeStructures { get; set; } = new List<MstDentalFeeStructure>();
}
