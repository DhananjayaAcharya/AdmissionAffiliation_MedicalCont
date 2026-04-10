using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAdministration
{
    public int Id { get; set; }

    public string AdministrationType { get; set; } = null!;

    public int FacultyCode { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
