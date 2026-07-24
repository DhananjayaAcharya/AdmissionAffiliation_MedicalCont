using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnPgcourseGeneralDetail
{
    public int PgcourseGeneralDetailId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public DateOnly? LoPdate { get; set; }

    public int? YearsSinceStart { get; set; }

    public string? Hodname { get; set; }

    public int? ExistingSeats { get; set; }

    public int? IncreaseSeatsFrom { get; set; }

    public int? IncreaseSeatsTo { get; set; }

    public int? TotalUnits { get; set; }

    public int? DepartmentBeds { get; set; }

    public int? TotalIcuhdubeds { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TxnPgcourseIcudetail> TxnPgcourseIcudetails { get; set; } = new List<TxnPgcourseIcudetail>();

    public virtual ICollection<TxnPgcourseUnitBedDetail> TxnPgcourseUnitBedDetails { get; set; } = new List<TxnPgcourseUnitBedDetail>();
}
