using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffInstitutionsDetail
{
    public int InstitutionId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string TypeOfInstitution { get; set; } = null!;

    public string NameOfInstitution { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string VillageTownCity { get; set; } = null!;

    public string Taluk { get; set; } = null!;

    public string District { get; set; } = null!;

    public string PinCode { get; set; } = null!;

    public string MobileNumber { get; set; } = null!;

    public string? StdCode { get; set; }

    public string? Fax { get; set; }

    public string? Website { get; set; }

    public string? SurveyNoPidNo { get; set; }

    public bool MinorityInstitute { get; set; }

    public bool AttachedToMedicalClg { get; set; }

    public bool RuralInstitute { get; set; }

    public string YearOfEstablishment { get; set; } = null!;

    public string EmailId { get; set; } = null!;

    public string? AltLandlineMobile { get; set; }

    public string? AltEmailId { get; set; }

    public string HeadOfInstitution { get; set; } = null!;

    public string HeadAddress { get; set; } = null!;

    public string FinancingAuthority { get; set; } = null!;

    public string StatusOfCollege { get; set; } = null!;

    public string CourseApplied { get; set; } = null!;

    public string? DocumentName { get; set; }

    public string? DocumentContentType { get; set; }

    public byte[]? DocumentData { get; set; }

    public string? NodalOfficerName { get; set; }

    public string? NodalOfficerMobNumber { get; set; }

    public string? NodalOfficerEmail { get; set; }

    public string? PrincipalName { get; set; }

    public string? PrincipalMobNo { get; set; }

    public string? PrincipalEmail { get; set; }

    public string? HeadOfInstitutionMobNo { get; set; }

    public string? HeadOfInstitutionEmail { get; set; }

    public string? CollegeUrl { get; set; }

    public string? TrustName { get; set; }

    public string? TrustAddress { get; set; }

    public DateOnly? TrustEstablishmentDate { get; set; }

    public string? TrustPresidentName { get; set; }

    public string? TrustPresidentContactNo { get; set; }

    public string? DeanName { get; set; }

    public string? DeanMobileNumber { get; set; }

    public string? DeanEmailId { get; set; }

    public string? PrincipalMobileNumber { get; set; }

    public string? PrincipalEmailId { get; set; }

    public string? MinorityCategory { get; set; }

    public string? RunningCourse { get; set; }

    public string? CourseLevel { get; set; }
}
