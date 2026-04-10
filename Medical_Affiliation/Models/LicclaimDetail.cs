using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicclaimDetail
{
    public int Id { get; set; }

    public string? MemberName { get; set; }

    public string? TypeofMember { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ModeOfTravel { get; set; }

    public string? FromPlace { get; set; }

    public string? ToPlace { get; set; }

    public decimal? Kilometers { get; set; }

    public string? ReturnFromPlace { get; set; }

    public string? ReturnToPlace { get; set; }

    public decimal? ReturnKilometers { get; set; }

    public decimal? TotalCost { get; set; }

    public string? CollegeCode { get; set; }

    public string? Faculty { get; set; }

    public byte[]? UploadBills { get; set; }

    public string? AssignedColleges { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsBanglore { get; set; }

    public string? AirFare { get; set; }

    public string? NoofDays { get; set; }

    public decimal? TravelCost { get; set; }

    public decimal? Dacost { get; set; }

    public decimal? Lcacost { get; set; }

    public decimal? CollegeCost { get; set; }

    public decimal? AirFareCost { get; set; }

    public decimal? AirRoadCost { get; set; }

    public string? Division { get; set; }

    public bool? IsLca { get; set; }

    public int? NumberOfDays { get; set; }

    public byte[]? AttendenceDoc { get; set; }

    public DateOnly? InspectionDate { get; set; }
}
