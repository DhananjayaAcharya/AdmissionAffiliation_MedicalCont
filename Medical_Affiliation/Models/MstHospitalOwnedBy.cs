using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstHospitalOwnedBy
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string OwnedBy { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
