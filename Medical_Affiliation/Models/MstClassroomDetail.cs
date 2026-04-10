using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstClassroomDetail
{
    public int Id { get; set; }

    public string CourseCode { get; set; } = null!;

    public int NoOfClassrooms { get; set; }

    public string? SizeOfClassrooms { get; set; }

    public int? IntakeId { get; set; }

    public string? FacultyCode { get; set; }
}
