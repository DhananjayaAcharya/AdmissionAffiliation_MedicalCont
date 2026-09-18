using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ActionTakenDeficiencyReport
{
    public int ActionTakenDeficiencyReportId { get; set; }

    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int TypeId { get; set; }

    public string CourseLevel { get; set; } = null!;

    public string DeficiencyPointedOut { get; set; } = null!;

    public string ExtentRemedied { get; set; } = null!;

    public string? RelevantReportPath { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
