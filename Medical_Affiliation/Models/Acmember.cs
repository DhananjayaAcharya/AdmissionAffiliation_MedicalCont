using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Acmember
{
    public int Id { get; set; }

    public string? TypeofMemebers { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }
}
