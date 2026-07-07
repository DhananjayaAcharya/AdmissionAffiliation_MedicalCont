using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AppMenuItem
{
    public int MenuItemId { get; set; }

    public string MenuName { get; set; } = null!;

    public string? Icon { get; set; }

    public string Controller { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Area { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; }

    public bool RequiresWrite { get; set; }

    public bool RequiresEdit { get; set; }

    public bool RequiresDelete { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<AppRoleMenu> AppRoleMenus { get; set; } = new List<AppRoleMenu>();

    public virtual ICollection<AppMenuItem> InverseParent { get; set; } = new List<AppMenuItem>();

    public virtual AppMenuItem? Parent { get; set; }
}
