using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstCollectionDevelopmentMaster
{
    public int DocTypeId { get; set; }

    public string? DocTypeName { get; set; }

    public int FacultyId { get; set; }
}
