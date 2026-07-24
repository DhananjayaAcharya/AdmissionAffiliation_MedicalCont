using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AcademicSummaryDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string? PublicationsPast3Years { get; set; }

    public DateOnly? AssessmentDate { get; set; }

    public string? LiccommitteeObservations { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
