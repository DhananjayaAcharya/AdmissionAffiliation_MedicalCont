using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicInspectionTeamCredential
{
    public int TeamId { get; set; }

    public string Username { get; set; } = null!;

    public string ProtectedPassword { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
