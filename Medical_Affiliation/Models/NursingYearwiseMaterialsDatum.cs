using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingYearwiseMaterialsDatum
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int ParametersId { get; set; }

    public string ParametersName { get; set; } = null!;

    public string Year1 { get; set; } = null!;

    public string Year2 { get; set; } = null!;

    public string Year3 { get; set; } = null!;
}
