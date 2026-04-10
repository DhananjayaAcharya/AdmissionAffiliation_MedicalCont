using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaStudentRegisterRecord
{
    public int StudentRegisterRecordId { get; set; }

    public string? CollegeCode { get; set; }

    public int? FacultyId { get; set; }

    public int? AffiliationType { get; set; }

    public int? RegisterRecordId { get; set; }

    public string? IsMaintained { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CourseLevel { get; set; }

    public string? RegisterRecord { get; set; }
}
