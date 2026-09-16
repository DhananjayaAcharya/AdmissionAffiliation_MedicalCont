using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalLibraryService
{
    public int DentalLibraryServiceId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int ServiceId { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? CourseLevel { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual MstDentalLibraryService Service { get; set; } = null!;
}
