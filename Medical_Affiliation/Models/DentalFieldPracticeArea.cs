using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalFieldPracticeArea
{
    public int DentalFieldPracticeAreaId { get; set; }

    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int TypeId { get; set; }

    public string CourseLevel { get; set; } = null!;

    public int FieldTypeId { get; set; }

    public string Location { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string ManagedBy { get; set; } = null!;

    public string? StaffList { get; set; }

    public int? PopulationServed { get; set; }

    public string? ActivitiesAndServices { get; set; }

    public string? RecordsMaintained { get; set; }

    public string? EquipmentsAvailable { get; set; }

    public string? TrainingActivities { get; set; }

    public string? SupervisionMethod { get; set; }

    public string? TraineeSupervisorAccommodation { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual MstFieldTypeChp FieldType { get; set; } = null!;

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
