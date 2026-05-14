using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstDentalLibraryRecord
{
    public int RecordId { get; set; }

    public string RecordName { get; set; } = null!;

    public int DisplayOrder { get; set; }
}
