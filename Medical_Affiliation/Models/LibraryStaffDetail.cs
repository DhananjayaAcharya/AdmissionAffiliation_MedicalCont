using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LibraryStaffDetail
{
    public int LibraryStaffId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyId { get; set; }

    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string? Qualification { get; set; }

    public DateOnly? ExperienceFrom { get; set; }

    public DateOnly? ExperienceTo { get; set; }

    public string? CourseLevel { get; set; }

    public string? PayScale { get; set; }

    public string? Category { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
