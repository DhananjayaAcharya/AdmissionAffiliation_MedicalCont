using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string? Description { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
