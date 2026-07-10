using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDepartmentConfiguration
{
    public int Id { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public int MaximumUnits { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual DepartmentMaster DepartmentCodeNavigation { get; set; } = null!;
}
