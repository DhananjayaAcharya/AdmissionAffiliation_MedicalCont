using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ClinicalWorkloadDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string ParticularName { get; set; } = null!;

    public int ParticularSequence { get; set; }

    public decimal? EntireHospital { get; set; }

    public decimal? OnDayOfAssessment { get; set; }

    public decimal? Random3Days { get; set; }

    public decimal? PreviousYear { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
