using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstClassroom
{
    public string CourseCode { get; set; } = null!;

    public int? NoofClassrooms { get; set; }

    public long? SizeOfClassrooms { get; set; }

    public int IntakeId { get; set; }
}
