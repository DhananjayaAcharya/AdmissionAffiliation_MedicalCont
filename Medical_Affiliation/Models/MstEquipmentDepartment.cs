using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstEquipmentDepartment
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual ICollection<MstEquipmentDeptWise> MstEquipmentDeptWises { get; set; } = new List<MstEquipmentDeptWise>();
}
