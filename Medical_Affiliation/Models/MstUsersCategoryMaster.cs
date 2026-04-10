using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstUsersCategoryMaster
{
    public int UsersCategoryId { get; set; }

    public string? UsersCategoryName { get; set; }

    public int FacultyId { get; set; }
}
