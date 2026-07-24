using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CollegePrincipalDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? CollegeName { get; set; }

    public string? PrincipalName { get; set; }

    public string? PrincipalMobileNo { get; set; }

    public string? PrincipalMailId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
