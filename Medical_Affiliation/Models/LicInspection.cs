using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicInspection
{
    public int Id { get; set; }

    public string TypeofMember { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateOnly? Dob { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Pannumber { get; set; }

    public string? AadhaarNumber { get; set; }

    public string? AccountHolderName { get; set; }

    public string? AccountNumber { get; set; }

    public string? Ifsccode { get; set; }

    public string? BankName { get; set; }

    public string? CreatedPassword { get; set; }

    public string? BranchName { get; set; }

    public string? CollegeName { get; set; }

    public DateOnly? DateOfInspection { get; set; }

    public byte[]? AttendenceDoc { get; set; }

    public bool? IsCompleted { get; set; }

    public string? AttendanceFilePath { get; set; }

    public string? ModeOfTravel { get; set; }

    public string? FromPlace { get; set; }

    public string? ToPlace { get; set; }

    public string? ReturnFromPlace { get; set; }

    public string? ReturnToPlace { get; set; }

    public string? ReturnKilometers { get; set; }

    public double? Kilometers { get; set; }

    public decimal? TotalCost { get; set; }

    public string? MemberCode { get; set; }
}
