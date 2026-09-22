using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AdditionalInformationInAcademicActivity
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? CourseLevel { get; set; }

    public bool HasMedicalEducationUnit { get; set; }

    public bool HasTotprogrammesConducted { get; set; }

    public bool HasTotprogrammesAttended { get; set; }

    public string? CmeprogrammePdfPath { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int TotprogrammesConducted { get; set; }

    public int TotprogrammesAttended { get; set; }

    public int? TypeId { get; set; }

    public bool? HasCmeprogrammesConducted { get; set; }

    public int? NoOfCmeprogrammesConducted { get; set; }

    public bool? HasCmeprogrammesAttended { get; set; }

    public int? NoOfCmeprogrammesAttended { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual TypeOfAffiliation? Type { get; set; }
}
