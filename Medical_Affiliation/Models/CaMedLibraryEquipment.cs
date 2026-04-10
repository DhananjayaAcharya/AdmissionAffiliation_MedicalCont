using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedLibraryEquipment
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string EquipmentName { get; set; } = null!;

    public string? HasEquipment { get; set; }

    public string CourseLevel { get; set; } = null!;
}
