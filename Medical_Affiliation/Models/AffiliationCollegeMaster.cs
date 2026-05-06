using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationCollegeMaster
{
    public string CollegeCode { get; set; } = null!;

    public string? CollegeName { get; set; }

    public string? CollegeTown { get; set; }

    public string? FacultyCode { get; set; }

    public string? Password { get; set; }

    public string? HashedPassword { get; set; }

    public byte[]? AllDocsForCourse { get; set; }

    public string? IsDeclared { get; set; }

    public string? ChangedPassword { get; set; }

    public string? PrincipalNameDeclared { get; set; }

    public bool ShowNodalOfficerDetails { get; set; }

    public bool ShowIntakeDetails { get; set; }

    public bool ShowRepositoryDetails { get; set; }

    public string? PrincipalMobileNumber { get; set; }

    public string? DistrictId { get; set; }

    public string? TalukId { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<AffiliationFinalDeclaration> AffiliationFinalDeclarations { get; set; } = new List<AffiliationFinalDeclaration>();

    public virtual ICollection<AffiliationPayment> AffiliationPayments { get; set; } = new List<AffiliationPayment>();
}
