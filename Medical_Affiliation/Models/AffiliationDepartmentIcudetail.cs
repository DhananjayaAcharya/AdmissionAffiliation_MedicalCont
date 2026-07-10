using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationDepartmentIcudetail
{
    public long Id { get; set; }

    public long DepartmentInformationId { get; set; }

    public string IcutypeCode { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public int? TotalBeds { get; set; }

    public int? OccupiedBeds { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationDepartmentInformation DepartmentInformation { get; set; } = null!;

    public virtual MstDepartmentIcutype IcutypeCodeNavigation { get; set; } = null!;
}
