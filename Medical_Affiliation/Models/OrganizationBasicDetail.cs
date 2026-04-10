using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class OrganizationBasicDetail
{
    public int Id { get; set; }

    public string? OrganizationType { get; set; }

    public string? TrustName { get; set; }

    public string? ChairmanName { get; set; }

    public string? AadhaarNumber { get; set; }

    public string? Pannumber { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? Taluk { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public string? Stdcode { get; set; }

    public string? LandLine { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? MobileNumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public DateOnly? DateOfRegistration { get; set; }

    public bool? HasAmendments { get; set; }

    public bool? HasOtherNursingCollege { get; set; }

    public string? ExistingTrustName { get; set; }

    public string? GoktrustName { get; set; }

    public bool? TrustNameChanged { get; set; }

    public string? CertificateNumber { get; set; }

    public byte[]? AmendedDoc { get; set; }

    public byte[]? AadhaarFile { get; set; }

    public byte[]? PanFile { get; set; }

    public byte[]? BankStatementFile { get; set; }

    public byte[]? TrustCertificateFile { get; set; }

    public byte[]? MemberDetailsFile { get; set; }
}
