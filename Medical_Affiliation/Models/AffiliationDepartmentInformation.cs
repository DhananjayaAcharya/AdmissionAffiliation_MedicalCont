using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationDepartmentInformation
{
    public long Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public DateOnly? LopDate { get; set; }

    public int? YearsSinceStarted { get; set; }

    public string? HeadOfDepartment { get; set; }

    public int? ExistingPgintake { get; set; }

    public int? IncreaseAdmissionFrom { get; set; }

    public int? IncreaseAdmissionTo { get; set; }

    public int? TotalUnits { get; set; }

    public int? TotalDepartmentBeds { get; set; }

    public int? TotalIcubeds { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<AffiliationDepartmentIcudetail> AffiliationDepartmentIcudetails { get; set; } = new List<AffiliationDepartmentIcudetail>();

    public virtual ICollection<AffiliationDepartmentUnitDetail> AffiliationDepartmentUnitDetails { get; set; } = new List<AffiliationDepartmentUnitDetail>();

    public virtual DepartmentMaster DepartmentCodeNavigation { get; set; } = null!;
}
