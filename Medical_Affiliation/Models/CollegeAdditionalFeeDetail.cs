using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegeAdditionalFeeDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? CourseLevel { get; set; }

    public int FacultyId { get; set; }

    public bool IsFeeLevied { get; set; }

    public string? FeeType { get; set; }

    public decimal? FeeAmount { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;
}
