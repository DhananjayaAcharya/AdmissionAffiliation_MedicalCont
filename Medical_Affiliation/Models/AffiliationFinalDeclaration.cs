using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationFinalDeclaration
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string PrincipalName { get; set; } = null!;

    public bool IsSubmitted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
