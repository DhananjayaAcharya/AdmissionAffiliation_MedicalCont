using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class WorkShopDetail
{
    public int WorkShopDetailsId { get; set; }

    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int TypeId { get; set; }

    public string CourseLevel { get; set; } = null!;

    public string? Staff { get; set; }

    public string? Equipment { get; set; }

    public string? ScopeOfWork { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
