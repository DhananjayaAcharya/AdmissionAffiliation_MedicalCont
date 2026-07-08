using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AppRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string RoleType { get; set; } = null!;

    public bool CanWrite { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanViewAll { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AppRoleMenu> AppRoleMenus { get; set; } = new List<AppRoleMenu>();

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}
