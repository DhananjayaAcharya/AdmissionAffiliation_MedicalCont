using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FinalDentalSubmission
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string CourseLevel { get; set; } = null!;

    public int AffiliationTypeId { get; set; }

    public string AcademicYear { get; set; } = null!;

    public string ApplicationNumber { get; set; } = null!;

    public bool PrincipalConsent { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
