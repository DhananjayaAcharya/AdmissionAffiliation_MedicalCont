using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UgdesignationMaster
{
    public int Id { get; set; }

    public string DesignationId { get; set; } = null!;

    public string DesignationName { get; set; } = null!;

    public int Order { get; set; }
}
