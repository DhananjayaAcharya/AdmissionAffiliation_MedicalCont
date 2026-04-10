using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstBooksSubject
{
    public long SubjectId { get; set; }

    public string? Subjects { get; set; }

    public int? SpecializationId { get; set; }

    public string? CourseCode { get; set; }

    public int? FacultyId { get; set; }
}
