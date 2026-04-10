using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedicalLibraryStaff
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationType { get; set; }

    public string? RegistrationNo { get; set; }

    public string StaffName { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string Qualification { get; set; } = null!;

    public int Experience { get; set; }

    public string Category { get; set; } = null!;

    public DateTime? CreatedOn { get; set; }

    public string? CourseLevel { get; set; }
}
