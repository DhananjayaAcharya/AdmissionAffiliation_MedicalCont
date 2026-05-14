using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaMedicalDepartmentLibrary
{
    public int DepartmentalLibraryId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int AffiliationType { get; set; }

    public string? RegistrationNo { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public int TotalBooks { get; set; }

    public int BooksAddedInYear { get; set; }

    public int CurrentJournals { get; set; }

    public string LibraryStaff { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public string? CourseLevel { get; set; }

    public int? Titles { get; set; }

    public int? InternationalJournals { get; set; }

    public int? BackVolumes { get; set; }

    public int? PrintJournalPercentage { get; set; }
}
