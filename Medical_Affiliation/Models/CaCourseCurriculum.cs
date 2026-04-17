using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaCourseCurriculum
{
    public int CourseCurriculumId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyId { get; set; }

    public int AffiliationType { get; set; }

    public int CurriculumId { get; set; }

    public string? CurriculumDetails { get; set; }

    public string? PdfFileName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CurriculumPdfPath { get; set; }

    public string? CourseLevel { get; set; }

    public virtual CaMstCourseCurriculum Curriculum { get; set; } = null!;
}
