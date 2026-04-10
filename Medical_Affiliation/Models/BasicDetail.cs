using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class BasicDetail
{
    public int Id { get; set; }

    public string OrganizationType { get; set; } = null!;

    public string TrustName { get; set; } = null!;

    public string ChairmanName { get; set; } = null!;

    public string AadhaarNumber { get; set; } = null!;

    public string Pannumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Taluk { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Pincode { get; set; } = null!;

    public string Stdcode { get; set; } = null!;

    public string? LandLine { get; set; }

    public string? Fax { get; set; }

    public string Email { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string RegistrationNumber { get; set; } = null!;

    public DateOnly DateOfRegistration { get; set; }

    public string HasAmendments { get; set; } = null!;

    public string HasOtherNursingCollege { get; set; } = null!;

    public string? ExistingTrustName { get; set; }

    public string? GoktrustName { get; set; }

    public string? TrustNameChanged { get; set; }

    public string? CertificateNumber { get; set; }

    public byte[]? AmendedDoc { get; set; }

    public byte[]? AadhaarFile { get; set; }

    public byte[]? Panfile { get; set; }

    public byte[]? BankStatementFile { get; set; }

    public byte[]? TrustCertificateFile { get; set; }

    public byte[]? MemberDetailsFile { get; set; }
}
