using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAffiliationFeeStructure
{
    public int FeeStructureId { get; set; }

    public int AffiliationTypeId { get; set; }

    public int FeeHeadId { get; set; }

    public int? SeatRangeFrom { get; set; }

    public int? SeatRangeTo { get; set; }

    public decimal Amount { get; set; }

    public bool IsPerCourse { get; set; }

    public bool IsPerSeat { get; set; }

    public string AcademicYear { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MstAffiliationType AffiliationType { get; set; } = null!;

    public virtual MstFeeHead FeeHead { get; set; } = null!;
}
