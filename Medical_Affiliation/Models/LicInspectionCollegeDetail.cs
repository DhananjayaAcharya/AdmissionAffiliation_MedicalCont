using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class LicInspectionCollegeDetail
{
    public int Id { get; set; }

    public string? AcademicYear { get; set; }

    public string? Acmember { get; set; }

    public long? AcMemberPhno { get; set; }

    public string? SenetMember { get; set; }

    public long? SenetMemberPhNo { get; set; }

    public string? SubjectExpertise { get; set; }

    public long? SubjectExpertisePhNo { get; set; }

    public int? Facultycode { get; set; }

    public string? Collegename { get; set; }

    public string? CollegePlace { get; set; }

    public string? Collegecode { get; set; }

    public byte[]? SeRevisedOrder { get; set; }
}
