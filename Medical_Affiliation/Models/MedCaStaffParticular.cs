using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedCaStaffParticular
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public int DesignationSlNo { get; set; }

    public decimal PayScale { get; set; }

    public string CourseLevel { get; set; } = null!;
}
