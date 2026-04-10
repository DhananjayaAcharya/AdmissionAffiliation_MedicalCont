using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalCentralLibrary
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string? RegistrationNo { get; set; }

    public string? IsAcinCentralLibrary { get; set; }

    public string? HasRoomsForStaff { get; set; }

    public string? HasStudentReadingRooms { get; set; }

    public string? HasFacultyReadingRoom { get; set; }

    public string? HasOldBooksRoom { get; set; }

    public string? HasComputerRoom { get; set; }

    public int? BooksPerAnnum { get; set; }

    public string? IsJournalsOnePercent { get; set; }

    public string? IsIndexedJournals { get; set; }

    public string? HasSubscriptionEvidence { get; set; }

    public string? SubscriptionFileName { get; set; }

    public byte[]? SubscriptionFileBytes { get; set; }

    public string? Is60PercentHardCopies { get; set; }

    public string? Is40PercentElectronic { get; set; }

    public string? IsJournalsVarietySame { get; set; }

    public int? Students { get; set; }

    public int? Journals { get; set; }

    public int? Books { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? IsBooksConditionSatisfied { get; set; }

    public virtual ICollection<MedicalCentralLibrarySpeciality> MedicalCentralLibrarySpecialities { get; set; } = new List<MedicalCentralLibrarySpeciality>();
}
