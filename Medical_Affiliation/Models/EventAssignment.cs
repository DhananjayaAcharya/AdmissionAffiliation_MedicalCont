using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class EventAssignment
{
    public int AssignmentId { get; set; }

    public int EventId { get; set; }

    public int? AssignedToUserId { get; set; }

    public int? AssignedToRoleId { get; set; }

    public int AssignedBy { get; set; }

    public DateTime AssignedDate { get; set; }

    public string? Notes { get; set; }

    public virtual AppUser AssignedByNavigation { get; set; } = null!;

    public virtual AppRole? AssignedToRole { get; set; }

    public virtual AppUser? AssignedToUser { get; set; }

    public virtual Event Event { get; set; } = null!;
}
