using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentWiseResearchProject
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? CourseLevel { get; set; }

    public string? DepartmentCode { get; set; }

    public int NoOfResearchProjectsLast3Years { get; set; }

    public string? PdfFilePath { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? TypeId { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual TypeOfAffiliation? Type { get; set; }
}
