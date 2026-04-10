using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ClinicalDatum
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int ParametersId { get; set; }

    public string ParametersName { get; set; } = null!;

    public DateOnly Year1 { get; set; }

    public DateOnly Year2 { get; set; }

    public DateOnly Year3 { get; set; }
}
