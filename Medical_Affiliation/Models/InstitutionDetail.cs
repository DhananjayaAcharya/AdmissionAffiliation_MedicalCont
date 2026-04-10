using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class InstitutionDetail
{
    public int InstitutionId { get; set; }

    public int? FacultyCode { get; set; }

    public int TypeOfInstitution { get; set; }

    public string NameOfInstitution { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Village { get; set; } = null!;

    public string StateCode { get; set; } = null!;

    public string DistrictCode { get; set; } = null!;

    public string TalukCode { get; set; } = null!;

    public string? PinCode { get; set; }

    public string MobileNumber { get; set; } = null!;

    public string EmailId { get; set; } = null!;

    public string? Stdcode { get; set; }

    public string? AlternateContactNumber { get; set; }

    public string? AlternateEmailId { get; set; }

    public string? Fax { get; set; }

    public string? Website { get; set; }

    public DateOnly AcademicYear { get; set; }

    public bool? IsRuralInstitute { get; set; }

    public bool? IsMinorityInstitute { get; set; }

    public virtual ICollection<IntakeDetail> IntakeDetails { get; set; } = new List<IntakeDetail>();
}
