using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaAcademicPerformance
{
    public int AcademicPerformanceId { get; set; }

    public string? CollegeCode { get; set; }

    public int? FacultyId { get; set; }

    public int? AffiliationType { get; set; }

    public int? YearOfStudyId { get; set; }

    public int? RegularStudents { get; set; }

    public int? RepeaterStudents { get; set; }

    public int? NumberOfStudentsPassed { get; set; }

    public decimal? PassPercentage { get; set; }

    public int? FirstClassCount { get; set; }

    public int? DistinctionCount { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CourseLevel { get; set; }

    public virtual CaMstYearOfStudy? YearOfStudy { get; set; }
}
