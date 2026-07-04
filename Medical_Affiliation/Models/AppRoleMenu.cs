using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AppRoleMenu
{
    public int RoleMenuId { get; set; }

    public int RoleId { get; set; }

    public int MenuItemId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual AppMenuItem MenuItem { get; set; } = null!;

    public virtual AppRole Role { get; set; } = null!;
}
