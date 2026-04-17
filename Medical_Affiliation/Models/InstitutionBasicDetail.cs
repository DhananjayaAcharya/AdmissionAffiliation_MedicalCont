using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class InstitutionBasicDetail
{
    public int InstitutionId { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? TypeOfOrganization { get; set; }

    public string? TypeOfInstitution { get; set; }

    public string? NameOfInstitution { get; set; }

    public string? AddressOfInstitution { get; set; }

    public string? VillageTownCity { get; set; }

    public string? Taluk { get; set; }

    public string? District { get; set; }

    public string? PinCode { get; set; }

    public string? MobileNumber { get; set; }

    public string? StdCode { get; set; }

    public string? Fax { get; set; }

    public string? Website { get; set; }

    public string? EmailId { get; set; }

    public string? AltLandlineOrMobile { get; set; }

    public string? AltEmailId { get; set; }

    public string? AcademicYearStarted { get; set; }

    public bool? IsRuralInstitution { get; set; }

    public bool? IsMinorityInstitution { get; set; }

    public string? TrustName { get; set; }

    public string? PresidentName { get; set; }

    public string? AadhaarNumber { get; set; }

    public string? Pannumber { get; set; }

    public string? RegistrationNumber { get; set; }

    public DateOnly? RegistrationDate { get; set; }

    public bool? Amendments { get; set; }

    public string? ExistingTrustName { get; set; }

    public string? GokobtainedTrustName { get; set; }

    public bool? ChangesInTrustName { get; set; }

    public bool? OtherNursingCollegeInCity { get; set; }

    public string? CategoryOfOrganisation { get; set; }

    public string? ContactPersonName { get; set; }

    public string? ContactPersonRelation { get; set; }

    public string? ContactPersonMobile { get; set; }

    public bool? OtherPhysiotherapyCollegeInCity { get; set; }

    public string? CoursesAppliedText { get; set; }

    public string? HeadOfInstitutionName { get; set; }

    public string? HeadOfInstitutionAddress { get; set; }

    public string? FinancingAuthorityName { get; set; }

    public string? CollegeStatus { get; set; }

    public string? GovAutonomousCertNumber { get; set; }

    public string? KncCertificateNumber { get; set; }

    public string? GovAutonomousCertFilePath { get; set; }

    public string? GovCouncilMembershipFilePath { get; set; }

    public string? GokOrderExistingCoursesFilePath { get; set; }

    public string? FirstAffiliationNotifFilePath { get; set; }

    public string? ContinuationAffiliationFilePath { get; set; }

    public string? KncCertificateFilePath { get; set; }

    public string? AmendedDocPath { get; set; }

    public string? AadhaarFilePath { get; set; }

    public string? PanfilePath { get; set; }

    public string? BankStatementFilePath { get; set; }

    public string? RegistrationCertificateFilePath { get; set; }

    public string? RegisteredTrustMemberDetailsPath { get; set; }

    public string? AuditStatementFilePath { get; set; }

    public DateTime CreatedOn { get; set; }
}
