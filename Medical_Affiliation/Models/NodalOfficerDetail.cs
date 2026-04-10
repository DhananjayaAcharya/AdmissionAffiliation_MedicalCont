using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NodalOfficerDetail
{
    public int Id { get; set; }

    public string? FacultyCode { get; set; }

    public string? FacultyName { get; set; }

    public string? CollegeName { get; set; }

    public string? CollegeCode { get; set; }

    public string? NodalOfficerName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? EmailId { get; set; }
}
