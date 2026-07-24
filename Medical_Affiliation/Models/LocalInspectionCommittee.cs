using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LocalInspectionCommittee
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public int SlNo { get; set; }

    public string? NameOfChairmanOrMember { get; set; }

    public string? CorrespondenceAddress { get; set; }

    public string? PhoneOffResMobile { get; set; }

    public string? Email { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
