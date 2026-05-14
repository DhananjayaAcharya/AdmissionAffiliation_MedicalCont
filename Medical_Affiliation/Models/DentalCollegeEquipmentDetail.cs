using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalCollegeEquipmentDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; } = null!;

    public int? OneUnitRequirement { get; set; }

    public int? TwoUnitRequirement { get; set; }

    public int? OneUnitExisting { get; set; }

    public int? TwoUnitExisting { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual MstEquipmentDeptWise Equipment { get; set; } = null!;
}
