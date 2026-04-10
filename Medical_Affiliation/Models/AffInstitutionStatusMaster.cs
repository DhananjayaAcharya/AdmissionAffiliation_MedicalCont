using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffInstitutionStatusMaster
{
    public byte InstitutionStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string StatusCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
