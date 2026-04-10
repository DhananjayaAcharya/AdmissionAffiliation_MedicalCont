using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationCourseDetail
{
    public int Id { get; set; }

    public string Facultycode { get; set; } = null!;

    public string Collegecode { get; set; } = null!;

    public string CourseId { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public string? IntakeDuring202526 { get; set; }

    public string? IntakeSlab { get; set; }

    public string? Typeofpermission { get; set; }

    public DateOnly? YearofLop { get; set; }

    public string? DateOfLoprenewalGoimci { get; set; }

    public string? SanctionedIntakePermission { get; set; }

    public string? Dateofrecognition { get; set; }

    public DateOnly? YearofObtainingEcandFc { get; set; }

    public string? SannctionedIntakeEcFc { get; set; }

    public byte[]? Gokorder { get; set; }

    public string? YearOfLastAffiliationRguhs { get; set; }

    public string? SanctionedIntakeLastAffiliation { get; set; }

    public byte[]? LastAffiliationRguhsfile { get; set; }

    public DateOnly? DateOfPreviousLicinspection { get; set; }

    public string? ActionTakenOnDeficiencies { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
