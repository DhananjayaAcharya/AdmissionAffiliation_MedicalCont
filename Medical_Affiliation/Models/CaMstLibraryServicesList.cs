using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstLibraryServicesList
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string ServiceName { get; set; } = null!;
}
