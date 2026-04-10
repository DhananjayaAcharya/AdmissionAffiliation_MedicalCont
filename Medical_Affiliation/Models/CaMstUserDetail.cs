using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstUserDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;
}
