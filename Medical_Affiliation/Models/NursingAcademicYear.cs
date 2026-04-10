using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingAcademicYear
{
    public int Id { get; set; }

    public int? YearId { get; set; }

    public int? YearData { get; set; }
}
