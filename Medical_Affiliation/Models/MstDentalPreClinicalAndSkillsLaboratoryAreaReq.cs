using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalPreClinicalAndSkillsLaboratoryAreaReq
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string LaboratoryName { get; set; } = null!;

    public int SeatIntake { get; set; }

    public decimal AreaRequiredSqM { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<DentalPreClinicalAndSkillsLabAreaReq> DentalPreClinicalAndSkillsLabAreaReqs { get; set; } = new List<DentalPreClinicalAndSkillsLabAreaReq>();
}
