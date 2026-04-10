using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaLibraryService
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public string? ServiceName { get; set; }

    public string? Specify { get; set; }
}
