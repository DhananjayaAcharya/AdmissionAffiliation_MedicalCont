using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class StaffShortageDetail
{
    public int StaffShortageId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyId { get; set; }

    public string PostName { get; set; } = null!;

    public string? ReasonForShortage { get; set; }

    public string? ArrangementMade { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;
}
