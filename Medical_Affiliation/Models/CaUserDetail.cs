using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaUserDetail
{
    public int Id { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string? CategoryName { get; set; }

    public int? TotalNumber { get; set; }
}
