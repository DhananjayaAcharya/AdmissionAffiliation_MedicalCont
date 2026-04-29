using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaSsAffiliationGrantedYear
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int CourseCode { get; set; }

    public string CourseName { get; set; } = null!;

    public DateOnly? AffiliationDate { get; set; }

    public int? SanctionedIntake { get; set; }

    public string? FileName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CoursesApplied { get; set; }

    public string? FilePath { get; set; }

    


}
