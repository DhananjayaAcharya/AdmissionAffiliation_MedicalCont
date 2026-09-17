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

    public string? SslcDoc { get; set; }

    public string? KmcDoc { get; set; }

    public string? ExperienceLetterDoc { get; set; }

    public string? AppointmentLetterDoc { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? ApprovalRemark { get; set; }

    public string? DrApprovalStatus { get; set; }

    public string? DrApprovalRemark { get; set; }

    public string? FatherGuardianName { get; set; }

    public string? Gender { get; set; }

    public string? ContactNumber { get; set; }

    public string? Email { get; set; }

    public string? Nationality { get; set; }

    public string? CandidateRegisteredNumber { get; set; }

    public string? UgDegree { get; set; }

    public string? PgDegree { get; set; }

    public string? ExperienceCollege { get; set; }

    public int? YearOfPassing { get; set; }

    public string? UgDegreeCertificatePath { get; set; }

    public Guid? UgDegreeCertificateGuid { get; set; }

    public string? PgDegreeCertificatePath { get; set; }

    public Guid? PgDegreeCertificateGuid { get; set; }

    public string? UgUniversityCollegeName { get; set; }

    public int? UgYearOfPassing { get; set; }

    public string? PgUniversityCollegeName { get; set; }

    public int? PgYearOfPassing { get; set; }

    public Guid? SslcDocGuid { get; set; }

    public Guid? KmcDocGuid { get; set; }

    public Guid? ExperienceLetterDocGuid { get; set; }

    public Guid? AppointmentLetterDocGuid { get; set; }
}
