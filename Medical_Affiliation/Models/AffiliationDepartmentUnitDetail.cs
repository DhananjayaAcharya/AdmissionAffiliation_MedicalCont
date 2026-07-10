using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationDepartmentUnitDetail
{
    public long Id { get; set; }

    public long DepartmentInformationId { get; set; }

    public int UnitNumber { get; set; }

    public int NumberOfBeds { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationDepartmentInformation DepartmentInformation { get; set; } = null!;
}
