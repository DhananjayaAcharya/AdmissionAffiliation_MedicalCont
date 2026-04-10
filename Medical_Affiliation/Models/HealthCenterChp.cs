using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class HealthCenterChp
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string NameofHealthCenter { get; set; } = null!;

    public int PlanningId { get; set; }

    public string PlanningType { get; set; } = null!;

    public int AdministrationId { get; set; }

    public string AdministrationType { get; set; } = null!;

    public int DistanceFromNursingInstitution { get; set; }

    public string ServicesRendered { get; set; } = null!;

    public string FieldType { get; set; } = null!;

    public int FieldTypeId { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
