using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UniversityImage
{
    public int Id { get; set; }

    public string? FileName { get; set; }

    public byte[]? Photo { get; set; }
}
