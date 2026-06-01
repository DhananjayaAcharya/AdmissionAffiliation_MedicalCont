using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicTaDaEditLog
{
    public int LogId { get; set; }

    public int ApprovalId { get; set; }

    public string? AcademicYear { get; set; }

    public int? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? MemberName { get; set; }

    public string? TypeOfMembers { get; set; }

    public string? MobileNo { get; set; }

    public bool? IsBanglore { get; set; }

    public string? Division { get; set; }

    public decimal? OldKilometers { get; set; }

    public decimal? NewKilometers { get; set; }

    public decimal? OldReturnKilometers { get; set; }

    public decimal? NewReturnKilometers { get; set; }

    public decimal? OldTravelCost { get; set; }

    public decimal? NewTravelCost { get; set; }

    public decimal? OldDacost { get; set; }

    public decimal? NewDacost { get; set; }

    public decimal? OldLcacost { get; set; }

    public decimal? NewLcacost { get; set; }

    public bool? OldIsLca { get; set; }

    public bool? NewIsLca { get; set; }

    public decimal? OldAirRoadCost { get; set; }

    public decimal? NewAirRoadCost { get; set; }

    public decimal? OldAirFair { get; set; }

    public decimal? NewAirFair { get; set; }

    public decimal? OldTotalClaimAmount { get; set; }

    public decimal? NewTotalClaimAmount { get; set; }

    public string? ChangedFields { get; set; }

    public string? EditedBy { get; set; }

    public string? EditorDesignation { get; set; }

    public DateTime EditedAt { get; set; }

    public string? EditedAtStage { get; set; }
}
