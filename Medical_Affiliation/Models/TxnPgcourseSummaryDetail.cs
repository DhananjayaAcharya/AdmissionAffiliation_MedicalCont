using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnPgcourseSummaryDetail
{
    public int PgcourseSummaryDetailId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public DateOnly? DateOfAssessment { get; set; }

    public string? AssessorName { get; set; }

    public string? InstitutionName { get; set; }

    public string? InstitutionCategory { get; set; }

    public string? HeadOfInstitutionDesignation { get; set; }

    public string? HeadOfInstitutionName { get; set; }

    public string? HeadOfInstitutionAgeDob { get; set; }

    public string? HeadOfInstitutionTeachingExp { get; set; }

    public string? HeadOfInstitutionPgdegree { get; set; }

    public string? HeadOfInstitutionPgrecognition { get; set; }

    public string? HeadOfInstitutionSubject { get; set; }

    public string? DepartmentInspected { get; set; }

    public string? Hodname { get; set; }

    public string? HodageDob { get; set; }

    public string? HodteachingExp { get; set; }

    public string? HodpgDegree { get; set; }

    public string? HodpgRecognition { get; set; }

    public int? NumberOfUgseats { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TxnPgcourseSummaryContactDetail> TxnPgcourseSummaryContactDetails { get; set; } = new List<TxnPgcourseSummaryContactDetail>();

    public virtual ICollection<TxnPgcourseSummaryInspectionDetail> TxnPgcourseSummaryInspectionDetails { get; set; } = new List<TxnPgcourseSummaryInspectionDetail>();
}
