using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalPreClinicalAndSkillsLabAreaReq
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int SeatIntake { get; set; }

    public int LabId { get; set; }

    public string LabName { get; set; } = null!;

    public decimal RequiredAreaSqM { get; set; }

    public decimal? ExistingAreaSqM { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual MstDentalPreClinicalAndSkillsLaboratoryAreaReq Lab { get; set; } = null!;
}
