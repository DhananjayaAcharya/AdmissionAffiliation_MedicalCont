using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstMedLibraryItem
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string ItemName { get; set; } = null!;
}
