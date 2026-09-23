using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnDentalOtherFeeStructure
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public int DentalOtherFeeStructureId { get; set; }

    public string FeeName { get; set; } = null!;

    public decimal AmountToBePaid { get; set; }

    public bool IsApplicable { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MstAffiliationType AffiliationType { get; set; } = null!;

    public virtual MstDentalOtherFeeStructure DentalOtherFeeStructure { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
