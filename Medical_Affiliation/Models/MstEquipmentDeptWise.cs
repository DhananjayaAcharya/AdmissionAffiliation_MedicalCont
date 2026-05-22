using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstEquipmentDeptWise
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string EquipmentName { get; set; } = null!;

    public string? Specification { get; set; }

    public int? OneUnitRequirement { get; set; }

    public int? TwoUnitRequirement { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<DentalCollegeEquipmentDetail> DentalCollegeEquipmentDetails { get; set; } = new List<DentalCollegeEquipmentDetail>();

    public virtual MstEquipmentDepartment DepartmentCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
