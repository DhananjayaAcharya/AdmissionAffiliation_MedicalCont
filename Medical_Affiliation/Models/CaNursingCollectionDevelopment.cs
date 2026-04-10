using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaNursingCollectionDevelopment
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public int? TotalCurrentYear { get; set; }

    public int? AddedPreviousYear { get; set; }

    public string? RegistrationNo { get; set; }
}
