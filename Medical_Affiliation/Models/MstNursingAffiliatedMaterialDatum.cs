using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstNursingAffiliatedMaterialDatum
{
    public int Id { get; set; }

    public int ParametersId { get; set; }

    public string ParametersName { get; set; } = null!;

    public string? FacultyCode { get; set; }
}
