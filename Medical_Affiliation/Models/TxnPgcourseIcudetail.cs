using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnPgcourseIcudetail
{
    public int PgcourseIcudetailId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int PgcourseGeneralDetailId { get; set; }

    public string Icutype { get; set; } = null!;

    public bool? IsAvailable { get; set; }

    public int? TotalBeds { get; set; }

    public int? BedOccupancyOnInspectionDay { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual TxnPgcourseGeneralDetail PgcourseGeneralDetail { get; set; } = null!;
}
