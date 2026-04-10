using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UserManualUrl
{
    public int FacultyId { get; set; }

    public int Typeofdboiliation { get; set; }

    public string? UserManualUrl1 { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedUser { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
