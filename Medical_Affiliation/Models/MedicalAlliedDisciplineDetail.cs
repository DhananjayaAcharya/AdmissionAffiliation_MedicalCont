using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalAlliedDisciplineDetail
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int HospitalDetailsId { get; set; }

    public int AffiliationTypeId { get; set; }

    public string DisciplineCode { get; set; } = null!;

    public string DisciplineName { get; set; } = null!;

    public int? Intake { get; set; }

    public int SeatSlab { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;
}
