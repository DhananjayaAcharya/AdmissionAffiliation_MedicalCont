using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDepartmentIcutype
{
    public int Id { get; set; }

    public string IcutypeCode { get; set; } = null!;

    public string IcutypeName { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<AffiliationDepartmentIcudetail> AffiliationDepartmentIcudetails { get; set; } = new List<AffiliationDepartmentIcudetail>();

    public virtual ICollection<MstDepartmentIcumapping> MstDepartmentIcumappings { get; set; } = new List<MstDepartmentIcumapping>();
}
