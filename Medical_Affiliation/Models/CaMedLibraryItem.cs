using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedLibraryItem
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string ItemName { get; set; } = null!;

    public int? CurrentForeign { get; set; }

    public int? CurrentIndian { get; set; }

    public int? PreviousForeign { get; set; }

    public int? PreviousIndian { get; set; }

    public string CourseLevel { get; set; } = null!;
}
