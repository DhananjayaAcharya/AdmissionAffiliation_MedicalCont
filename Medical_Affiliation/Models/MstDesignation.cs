using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDesignation
{
    public int DesignationId { get; set; }

    public string? DesignationName { get; set; }

    public string? DesignationType { get; set; }

    public string? Constituency { get; set; }

    public int? FacultyId { get; set; }
}
