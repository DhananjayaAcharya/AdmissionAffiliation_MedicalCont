using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class State
{
    public string StateId { get; set; } = null!;

    public string? StateNameEng { get; set; }

    public string? StateNameKan { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedUser { get; set; }
}
