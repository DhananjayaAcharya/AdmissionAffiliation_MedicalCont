using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalService
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string? SectionName { get; set; }

    public string RequirementName { get; set; } = null!;

    public bool IsActive { get; set; }

    public int SectionCode { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<DentalService> DentalServices { get; set; } = new List<DentalService>();

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
