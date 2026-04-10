using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LandBuildingDetail
{
    public int Id { get; set; }

    public decimal? LandAcres { get; set; }

    public long? BuildingArea { get; set; }

    public string? BuildingType { get; set; }

    public string? OwnerName { get; set; }

    public string? CourtCase { get; set; }

    public string? CoursesInBuilding { get; set; }

    public int? Classrooms { get; set; }

    public int? Labs { get; set; }

    public string? Survey { get; set; }

    public string? Rr { get; set; }

    public string? WaterSupply { get; set; }

    public string? Auditorium { get; set; }

    public string? OfficeFacilities { get; set; }

    public string? Seating { get; set; }

    public string? Electricity { get; set; }

    public string? BlueprintCertNo { get; set; }

    public string? ApprovalCertNo { get; set; }

    public string? TaxCertNo { get; set; }

    public string? RtccertNo { get; set; }

    public string? OccupancyCertNo { get; set; }

    public string? SaleDeedCertNo { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public byte[]? BlueprintDoc { get; set; }

    public byte[]? ApprovalCert { get; set; }

    public byte[]? TaxReceipt { get; set; }

    public byte[]? Rtc { get; set; }

    public byte[]? OccupancyCert { get; set; }

    public byte[]? SaleDeed { get; set; }

    public bool? AgreeTerms { get; set; }
}
