using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class FellowShipMedical
{
    public int Id { get; set; }

    public string? FacultyCode { get; set; }

    public string? Collegecode { get; set; }

    public string? StudentName { get; set; }

    public DateOnly? Dob { get; set; }

    public DateOnly? Dateofjoining { get; set; }

    public DateOnly? AdmissionOpeningDate { get; set; }

    public DateOnly? EndingDate { get; set; }

    public string? Course { get; set; }

    public string? KmcCertificateNumber { get; set; }

    public string? PrincipalName { get; set; }

    public string? PrincipalDeclaration { get; set; }

    public string? FellowshipCode { get; set; }

    public byte[]? SslcDoc { get; set; }

    public byte[]? KmcDoc { get; set; }

    public byte[]? ExperienceLetterDoc { get; set; }

    public byte[]? AppointmentLetterDoc { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? ApprovalRemark { get; set; }

    public string? DrApprovalStatus { get; set; }

    public string? DrApprovalRemark { get; set; }
}
