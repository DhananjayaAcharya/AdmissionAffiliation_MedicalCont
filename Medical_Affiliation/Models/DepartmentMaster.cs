using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentMaster
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public virtual ICollection<AffiliationDepartmentInformation> AffiliationDepartmentInformations { get; set; } = new List<AffiliationDepartmentInformation>();

    public virtual ICollection<MstDepartmentConfiguration> MstDepartmentConfigurations { get; set; } = new List<MstDepartmentConfiguration>();

    public virtual ICollection<MstDepartmentIcumapping> MstDepartmentIcumappings { get; set; } = new List<MstDepartmentIcumapping>();
}
