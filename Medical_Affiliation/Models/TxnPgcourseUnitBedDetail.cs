using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TxnPgcourseUnitBedDetail
{
    public int PgcourseUnitBedDetailId { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int PgcourseGeneralDetailId { get; set; }

    public string UnitName { get; set; } = null!;

    public int UnitSequence { get; set; }

    public int? NumberOfBeds { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual TxnPgcourseGeneralDetail PgcourseGeneralDetail { get; set; } = null!;
}
