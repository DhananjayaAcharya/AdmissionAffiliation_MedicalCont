using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalCollegePreviousIntake
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int AffiliationTypeId { get; set; }

    public int CourseCode { get; set; }

    public string IntakeSlab { get; set; } = null!;

    public short? LopYear { get; set; }

    public byte[]? NmcDocument { get; set; }

    public string? NmcDocumentName { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
