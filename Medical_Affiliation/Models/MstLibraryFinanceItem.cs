using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLibraryFinanceItem
{
    public int LibFinItemId { get; set; }

    public string? ItemsName { get; set; }

    public int FacultyId { get; set; }
}
