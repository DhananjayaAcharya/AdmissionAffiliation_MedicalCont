using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

public class InstitutionBasicDetailsViewModel1
{
    // =========================================================
    // Core keys / shared
    // =========================================================
    public int InstitutionId { get; set; }          // InstitutionBasicDetail PK
    public int? AffInstitutionId { get; set; }      // AffInstitutionsDetail PK (optional)

    [Display(Name = "College Code")]
    public string CollegeCode { get; set; }

    [Display(Name = "Faculty Code")]
    public string FacultyCode { get; set; }

    // =========================================================
    // Basic details (InstitutionBasicDetail)
    // =========================================================

    [Display(Name = "Name of Institution")]
    public string NameOfInstitution { get; set; }

    [Display(Name = "Address of Institution")]
    public string AddressOfInstitution { get; set; }

    [Display(Name = "Village / Town / City")]
    public string VillageTownCity { get; set; }

    [Display(Name = "Taluk")]
    public string Taluk { get; set; }

    [Display(Name = "District")]
    public string District { get; set; }

    [Display(Name = "Pin Code")]
    public string PinCode { get; set; }

    [Display(Name = "Mobile Number")]
    public string MobileNumber { get; set; }

    [Display(Name = "STD Code")]
    public string StdCode { get; set; }

    [Display(Name = "Fax")]
    public string Fax { get; set; }

    [Display(Name = "Website")]
    public string Website { get; set; }

    [Display(Name = "Email")]
    public string EmailId { get; set; }

    [Display(Name = "Alt Landline / Mobile")]
    public string AltLandlineOrMobile { get; set; }

    [Display(Name = "Alt Email")]
    public string AltEmailId { get; set; }

    [Display(Name = "Academic Year Started")]
    public string AcademicYearStarted { get; set; }

    [Display(Name = "Rural Institution")]
    public bool? IsRuralInstitution { get; set; }

    [Display(Name = "Minority Institution")]
    public bool? IsMinorityInstitution { get; set; }

    [Display(Name = "Trust Name")]
    public string TrustName { get; set; }

    [Display(Name = "President Name")]
    public string PresidentName { get; set; }

    [Display(Name = "Aadhaar Number")]
    public string AadhaarNumber { get; set; }

    [Display(Name = "PAN Number")]
    public string PANNumber { get; set; }

    [Display(Name = "Registration Number")]
    public string RegistrationNumber { get; set; }

    [Display(Name = "Registration Date")]
    [DataType(DataType.Date)]
    public DateTime? RegistrationDate { get; set; }

    [Display(Name = "Amendments")]
    public bool? Amendments { get; set; }

    [Display(Name = "Existing Trust Name")]
    public string ExistingTrustName { get; set; }

    [Display(Name = "GOK Obtained Trust Name")]
    public string GOKObtainedTrustName { get; set; }

    [Display(Name = "Changes In Trust Name")]
    public bool? ChangesInTrustName { get; set; }

    [Display(Name = "Other Nursing College In City")]
    public bool? OtherNursingCollegeInCity { get; set; }

    [Display(Name = "Category Of Organisation")]
    public string CategoryOfOrganisation { get; set; }

    [Display(Name = "Contact Person Name")]
    public string ContactPersonName { get; set; }

    [Display(Name = "Contact Person Relation")]
    public string ContactPersonRelation { get; set; }

    [Display(Name = "Contact Person Mobile")]
    public string ContactPersonMobile { get; set; }

    [Display(Name = "Other Physiotherapy College In City")]
    public bool? OtherPhysiotherapyCollegeInCity { get; set; }

    [Display(Name = "Courses Applied Text")]
    public string CoursesAppliedText { get; set; }

    [Display(Name = "Head Of Institution Name")]
    public string HeadOfInstitutionName { get; set; }

    [Display(Name = "Head Of Institution Address")]
    public string HeadOfInstitutionAddress { get; set; }

    [Display(Name = "Financing Authority Name")]
    public string FinancingAuthorityName { get; set; }

    [Display(Name = "College Status")]
    public string CollegeStatus { get; set; }

    [Display(Name = "Gov. Autonomous Certificate Number")]
    public string GovAutonomousCertNumber { get; set; }

    [Display(Name = "KNC Certificate Number")]
    public string KncCertificateNumber { get; set; }

    //[Display(Name = "Type Of Organization")]
    //public string TypeOfOrganization { get; set; }

    // =========================================================
    // Dropdown / lookup
    // =========================================================

    [Display(Name = "Type Of Institution")]
    public string TypeOfInstitution { get; set; }

    public List<SelectListItem> TypeOfInstitutionList { get; set; } = new();

    // =========================================================
    // Files for InstitutionBasicDetail
    // =========================================================

    public IFormFile GovAutonomousCertFile { get; set; }
    public IFormFile GovCouncilMembershipFile { get; set; }
    public IFormFile GokOrderExistingCoursesFile { get; set; }
    public IFormFile FirstAffiliationNotifFile { get; set; }
    public IFormFile ContinuationAffiliationFile { get; set; }
    public IFormFile KncCertificateFile { get; set; }
    public IFormFile AmendedDoc { get; set; }
    public IFormFile AadhaarFile { get; set; }
    public IFormFile PANFile { get; set; }
    public IFormFile BankStatementFile { get; set; }
    public IFormFile RegistrationCertificateFile { get; set; }
    public IFormFile RegisteredTrustMemberDetails { get; set; }
    public IFormFile AuditStatementFile { get; set; }

    // =========================================================
    // InstituteDetails (AffInstitutionsDetail) merged fields
    // =========================================================

    // Note: some names overlap; using the same property where shared (e.g., NameOfInstitution, TypeOfInstitution, etc.)

    [Display(Name = "Address (Affiliated Institute)")]
    public string Address { get; set; }

    [Display(Name = "Survey No / PID No")]
    public string SurveyNoPidNo { get; set; }

    [Display(Name = "Minority Institute")]
    public bool? MinorityInstitute { get; set; }

    [Display(Name = "Attached To Medical College")]
    public bool? AttachedToMedicalClg { get; set; }

    [Display(Name = "Rural Institute")]
    public bool? RuralInstitute { get; set; }

    // Stored as string per your POST; may be "yyyy" or "yyyy-MM-dd".
    [Display(Name = "Year Of Establishment")]
    public string YearOfEstablishment { get; set; }

    [Display(Name = "Alt Landline / Mobile (Aff)")]
    public string AltLandlineMobile { get; set; }

    [Display(Name = "Head Of Institution (Aff)")]
    public string HeadOfInstitution { get; set; }

    [Display(Name = "Head Address (Aff)")]
    public string HeadAddress { get; set; }

    [Display(Name = "Financing Authority (Aff)")]
    public string FinancingAuthority { get; set; }

    [Display(Name = "Status Of College")]
    public string StatusOfCollege { get; set; }

    [Display(Name = "Course Applied")]
    public string CourseApplied { get; set; }

    // File upload for AffInstitutionsDetail
    [Display(Name = "Affiliation Document")]
    public IFormFile DocumentFile { get; set; }
}
