using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalLibraryService
{
    public int DentalLibraryServiceId { get; set; }

    public int FacultyId { get; set; }

    public int TypeId { get; set; }

    public string ServiceName { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<DentalLibraryService> DentalLibraryServices { get; set; } = new List<DentalLibraryService>();

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
