using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class CaSsPermission
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int CourseCode { get; set; }

    public string CourseName { get; set; } = null!;

    public string PermissionStatus { get; set; } = null!;

    public byte[]? FileData { get; set; }

    public string? FileName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CoursesApplied { get; set; }
}
