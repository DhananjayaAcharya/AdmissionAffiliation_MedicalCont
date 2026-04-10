using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NodalOfficerInitiative
{
    public int Id { get; set; }

    public int NodalOfficerId { get; set; }

    public string NodalOfficerName { get; set; } = null!;

    public string? InitiativeId { get; set; }

    public bool IsEditable { get; set; }
}
