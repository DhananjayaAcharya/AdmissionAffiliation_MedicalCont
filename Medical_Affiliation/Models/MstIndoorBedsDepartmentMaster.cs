using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstIndoorBedsDepartmentMaster
{
    public int DeptId { get; set; }

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual ICollection<MstIndoorBedsOccupancyMaster> MstIndoorBedsOccupancyMasters { get; set; } = new List<MstIndoorBedsOccupancyMaster>();
}
