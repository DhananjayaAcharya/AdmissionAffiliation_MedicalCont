using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class StateMaster
{
    public string StateId { get; set; } = null!;

    public string StateName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
