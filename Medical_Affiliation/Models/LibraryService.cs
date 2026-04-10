using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LibraryService
{
    public string RegistrationNumber { get; set; } = null!;

    public int LibraryServiceId { get; set; }

    public string? IsAvailable { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedUser { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
