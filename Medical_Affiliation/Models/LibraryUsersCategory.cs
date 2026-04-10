using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LibraryUsersCategory
{
    public string RegistrationNumber { get; set; } = null!;

    public int UsersCategoryId { get; set; }

    public long? TotalNumber { get; set; }

    public string? CreatedUser { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? LastModifiedUser { get; set; }

    public DateTime? LastModifiedDate { get; set; }
}
