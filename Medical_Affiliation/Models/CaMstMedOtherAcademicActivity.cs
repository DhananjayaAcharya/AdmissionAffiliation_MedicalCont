using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMstMedOtherAcademicActivity
{
    public int Id { get; set; }

    public string ActivityName { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string DepartmentCode { get; set; } = null!;
}
