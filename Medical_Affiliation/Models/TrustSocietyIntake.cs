using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TrustSocietyIntake
{
    public int Id { get; set; }

    public string? TypeOfOrganization { get; set; }

    public string? TrustName { get; set; }

    public string? PresidentName { get; set; }

    public string? AadhaarNumber { get; set; }

    public string? Pannumber { get; set; }

    public string? Address { get; set; }

    public string? Village { get; set; }

    public string? State { get; set; }

    public string? District { get; set; }

    public string? Taluk { get; set; }

    public string? PinCode { get; set; }

    public string? Stdcode { get; set; }

    public string? Landline { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? Mobile { get; set; }

    public string? RegistrationNumber { get; set; }

    public DateOnly? RegistrationDate { get; set; }

    public bool? Amendments { get; set; }

    public string? ExistingTrustName { get; set; }

    public string? GokobtainedTrustName { get; set; }

    public bool? ChangesInTrustName { get; set; }

    public bool? OtherNursingCollegeInCity { get; set; }

    public byte[]? AmendedDoc { get; set; }

    public byte[]? AadhaarFile { get; set; }

    public byte[]? Panfile { get; set; }

    public byte[]? BankStatementFile { get; set; }

    public byte[]? RegistrationCertificateFile { get; set; }

    public byte[]? RegisteredTrustMemberDetails { get; set; }

    public string? CategoryOfOrganisation { get; set; }

    public string? CollegeCode { get; set; }
}
