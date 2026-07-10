using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDepartmentIcumapping
{
    public int Id { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string IcutypeCode { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual DepartmentMaster DepartmentCodeNavigation { get; set; } = null!;

    public virtual MstDepartmentIcutype IcutypeCodeNavigation { get; set; } = null!;
}
