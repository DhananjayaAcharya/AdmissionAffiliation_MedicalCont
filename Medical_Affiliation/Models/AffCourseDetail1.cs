using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffCourseDetail1
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string CourseId { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public bool IsRecognized { get; set; }

    public string? RguhsNotificationNo { get; set; }

    public byte[]? DocumentData { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedBy { get; set; }
}
