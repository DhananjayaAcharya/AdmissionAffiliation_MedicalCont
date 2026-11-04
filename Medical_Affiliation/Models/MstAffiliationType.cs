using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAffiliationType
{
    public int AffiliationTypeId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string AffiliationCategory { get; set; } = null!;

    public string FormNo { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? CourseLevelGroup { get; set; }

    public virtual ICollection<MstDentalFeeStructure> MstDentalFeeStructures { get; set; } = new List<MstDentalFeeStructure>();

    public virtual ICollection<MstDentalOtherFeeStructure> MstDentalOtherFeeStructures { get; set; } = new List<MstDentalOtherFeeStructure>();

    public virtual ICollection<TxnDentalFeeStructure> TxnDentalFeeStructures { get; set; } = new List<TxnDentalFeeStructure>();

    public virtual ICollection<TxnDentalOtherFeeStructure> TxnDentalOtherFeeStructures { get; set; } = new List<TxnDentalOtherFeeStructure>();
}
