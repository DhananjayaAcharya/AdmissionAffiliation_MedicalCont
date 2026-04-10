using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedLibraryGeneral
{
    public int SlNo { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string? SubFacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string? LibraryEmailId { get; set; }

    public string? DigitalLibrary { get; set; }

    public string? HelinetServices { get; set; }

    public string? DepartmentWiseLibrary { get; set; }

    public string CourseLevel { get; set; } = null!;
}
