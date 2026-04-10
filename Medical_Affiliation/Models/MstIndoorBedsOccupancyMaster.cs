using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstIndoorBedsOccupancyMaster
{
    public int Id { get; set; }

    public int AffiliationTypeId { get; set; }

    public int FacultyCode { get; set; }

    public int DepartmentCode { get; set; }

    public string SeatSlabId { get; set; } = null!;

    public int RequiredBeds { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual MstIndoorBedsDepartmentMaster DepartmentCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
