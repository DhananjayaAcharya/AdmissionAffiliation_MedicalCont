using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Edited2207CourseMasterMedicalData1
{
    public byte Id { get; set; }

    public short CourseCode { get; set; }

    public string? CourseName { get; set; }

    public byte FacultyCode { get; set; }

    public string CourseLevel { get; set; } = null!;

    public string? CoursePrefix { get; set; }

    public string? SubjectName { get; set; }
}
