using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class InitiativeMaster
{
    public int Id { get; set; }

    public string InitiativeId { get; set; } = null!;

    public string? InitiativeName { get; set; }
}
