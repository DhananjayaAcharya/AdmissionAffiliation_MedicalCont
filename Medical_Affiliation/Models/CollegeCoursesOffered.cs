using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegeCoursesOffered
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public int? YearOfStarting { get; set; }

    public int? SanctionedAdmissions { get; set; }

    public int? AdmittedAdmissions { get; set; }

    public string? Remarks { get; set; }

    public string? GovtKarnatakaPermissionNumber { get; set; }

    public string? GovtKarnatakaDocumentName { get; set; }

    public string? GovtKarnatakaDocumentPath { get; set; }

    public string? GovtKarnatakaDocumentContentType { get; set; }

    public string? CouncilPermissionNumber { get; set; }

    public string? CouncilDocumentName { get; set; }

    public string? CouncilDocumentPath { get; set; }

    public string? CouncilDocumentContentType { get; set; }

    public string? RguhslastAffiliationNumber { get; set; }

    public string? RguhslastAffiliationDocumentName { get; set; }

    public string? RguhslastAffiliationDocumentPath { get; set; }

    public string? RguhslastAffiliationDocumentContentType { get; set; }

    public string? GovtIndiaPermissionNumber { get; set; }

    public string? GovtIndiaDocumentName { get; set; }

    public string? GovtIndiaDocumentPath { get; set; }

    public string? GovtIndiaDocumentContentType { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;
}
