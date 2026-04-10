using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsExpectedIntakeMaster
{
    public int IntakeId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string? ExpectedIntake { get; set; }

    public int? MinSeats { get; set; }

    public int? MaxSeats { get; set; }

    public string? MedcolexpectedIntake { get; set; }

    public int? MedcolmaxSeats { get; set; }

    public string? IsMedicalCollege { get; set; }
}
