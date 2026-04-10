using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicTaDaEditedFinanceLog
{
    public int Id { get; set; }

    public string? AcademicYear { get; set; }

    public int? FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? MemberName { get; set; }

    public string? TypeOfMembers { get; set; }

    public string? MobileNo { get; set; }

    public string? FromPlace { get; set; }

    public string? ToPlace { get; set; }

    public decimal? Kilometers { get; set; }

    public string? ReturnFromPlace { get; set; }

    public string? ReturnToPlace { get; set; }

    public decimal? ReturnKilometers { get; set; }

    public decimal TotalClaimAmount { get; set; }

    public bool? IsBanglore { get; set; }

    public decimal? AirFair { get; set; }

    public decimal? TravelCost { get; set; }

    public decimal? Dacost { get; set; }

    public decimal? Lcacost { get; set; }

    public decimal? CollegeCost { get; set; }

    public decimal? AirRoadCost { get; set; }

    public string? Division { get; set; }

    public bool? IsLca { get; set; }

    public string? EditedBy { get; set; }

    public string? EditorDesignation { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
