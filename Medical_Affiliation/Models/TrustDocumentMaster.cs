using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TrustDocumentMaster
{
    public int DocumentId { get; set; }

    public string DocumentName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public int? FacultyCode { get; set; }
}
