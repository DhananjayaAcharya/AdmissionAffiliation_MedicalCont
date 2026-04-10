using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentMaster
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;
}
