using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalInfrastructure
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int SlNo { get; set; }

    public string RequirementName { get; set; } = null!;

    public string? RequirementDescription { get; set; }

    public int SeatSlab { get; set; }

    public decimal RequiredAreaSqFt { get; set; }

    public virtual ICollection<DentalInfrastructure> DentalInfrastructures { get; set; } = new List<DentalInfrastructure>();

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
