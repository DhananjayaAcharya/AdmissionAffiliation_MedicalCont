using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblRguhsFacultyUser
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Password { get; set; }

    public string? PasswordHash { get; set; }

    public string? UserName { get; set; }

    public bool IsActive { get; set; }

    public int? Faculty { get; set; }

    public bool? IsFinance { get; set; }

    public string? FinanceDesignation { get; set; }

    public string? DesignationDescription { get; set; }

    public bool? IsSection { get; set; }

    public bool? IsAdmin { get; set; }
}
