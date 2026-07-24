using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LibraryFacility
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int? NumberOfBooksCentral { get; set; }

    public int? NumberOfBooksDepartmental { get; set; }

    public int? BooksPurchasedLast3YrsCentral { get; set; }

    public int? BooksPurchasedLast3YrsDept { get; set; }

    public bool? AnnexureAttached { get; set; }

    public int? TotalIndianJournalsCentral { get; set; }

    public int? TotalIndianJournalsDept { get; set; }

    public int? TotalForeignJournalsCentral { get; set; }

    public int? TotalForeignJournalsDept { get; set; }

    public string? ComputerWithInternetCentral { get; set; }

    public string? ComputerWithInternetDept { get; set; }

    public string? CentralLibraryTiming { get; set; }

    public string? CentralReadingRoomTiming { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
