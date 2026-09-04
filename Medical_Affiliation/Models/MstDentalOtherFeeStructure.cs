using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalOtherFeeStructure
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string FeeName { get; set; } = null!;

    public decimal AmountToBePaid { get; set; }

    public int? AffiliationTypeId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MstAffiliationType? AffiliationType { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
