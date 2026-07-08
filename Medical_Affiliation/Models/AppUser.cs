using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AppUser
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public int RoleId { get; set; }

    public int? FacultyId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public string? SessionToken { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Faculty? Faculty { get; set; }

    public virtual AppRole Role { get; set; } = null!;
}
