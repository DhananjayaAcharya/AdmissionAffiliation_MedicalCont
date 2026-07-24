using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class OpdDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int? NoOfRoomsForConsultation { get; set; }

    public decimal? WaitingAreaInSqM { get; set; }

    public string? SpaceAndArrangements { get; set; }

    public string? IfNotAdequateReasons { get; set; }

    public string? DressingRoomAvailable { get; set; }

    public string? SeparateMinorOtMaleFemale { get; set; }

    public string? PerRectalExamRoomAvailable { get; set; }

    public string? DressingRoom2Available { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
