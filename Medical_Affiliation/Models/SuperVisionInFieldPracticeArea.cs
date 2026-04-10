using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class SuperVisionInFieldPracticeArea
{
    public int Id { get; set; }

    public string Post { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Qualification { get; set; } = null!;

    public DateOnly YearOfQualification { get; set; }

    public string University { get; set; } = null!;

    public DateOnly UgFromDate { get; set; }

    public DateOnly UgToDate { get; set; }

    public DateOnly PgFromDate { get; set; }

    public DateOnly PgToDate { get; set; }

    public string Responsibilities { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public int HospitalDetailsId { get; set; }

    public string? CourseLevel { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;
}
