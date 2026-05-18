using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstMedicalAlliedDiscipline
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string DisciplineCode { get; set; } = null!;

    public string DisciplineName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
