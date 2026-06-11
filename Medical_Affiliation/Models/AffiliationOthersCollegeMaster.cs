using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationOthersCollegeMaster
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string CollegeName { get; set; } = null!;

    public string CollegeTown { get; set; } = null!;

    public string? StateName { get; set; }

    public string? DistrictName { get; set; }

    public string? TalukName { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
