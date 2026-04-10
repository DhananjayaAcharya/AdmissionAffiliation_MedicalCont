using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaDepartmentLibraryDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? DepartmentCode { get; set; }

    public int? TotalBooks { get; set; }

    public int? BooksAdded { get; set; }

    public int? CurrentJournals { get; set; }

    public string? RegistrationNo { get; set; }
}
