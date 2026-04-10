using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstClinicalDepartment
{
    public int FacultyId { get; set; }

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }
}
