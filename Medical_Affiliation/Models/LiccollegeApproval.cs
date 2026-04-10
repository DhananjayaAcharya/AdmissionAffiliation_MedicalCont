using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LiccollegeApproval
{
    public int Id { get; set; }

    public int? FacultyCode { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string? AcademicYear { get; set; }

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

    public string? NoOfDays { get; set; }

    public decimal? TravelCost { get; set; }

    public decimal? Dacost { get; set; }

    public decimal? Lcacost { get; set; }

    public decimal? CollegeCost { get; set; }

    public decimal? AirRoadCost { get; set; }

    public string? Division { get; set; }

    public bool? IsLca { get; set; }

    public string DrApprovalStatus { get; set; } = null!;

    public string? DrRemarks { get; set; }

    public string? DrapprovedBy { get; set; }

    public DateTime? DrapprovedDate { get; set; }

    public string? FoLevel1ApprovedStatus { get; set; }

    public string? FoLevel1Remarks { get; set; }

    public DateTime? FoLevel1ForwardedDate { get; set; }

    public string? FCaseWorkerApproveStatus { get; set; }

    public string? FCaseWorkerApproveRemarks { get; set; }

    public DateTime? FCaseWorkerApprovedDate { get; set; }

    public string? FAoSpApprovedStatus { get; set; }

    public string? FAoSpApprovedRemarks { get; set; }

    public DateTime? FAoSpApprovedDate { get; set; }

    public string? FoLevel2ApprovedStatus { get; set; }

    public string? FoLevel2ApprovedRemarks { get; set; }

    public DateTime? FoLevel2ApprovedDate { get; set; }

    public string? CashierUpdate { get; set; }

    public DateTime? CashierApprovedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? LicApprovalFileName { get; set; }

    public byte[]? LicApprovalFile { get; set; }

    public DateTime? LicApprovalUploadedOn { get; set; }
}
