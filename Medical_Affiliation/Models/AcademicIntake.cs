using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AcademicIntake
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int Ay2024ExistingIntake { get; set; }

    public int Ay2024IncreaseIntake { get; set; }

    public int Ay2024TotalIntake { get; set; }

    public int Ay2025ExistingIntake { get; set; }

    public int Ay2025LopNmcIntake { get; set; }

    public int Ay2025TotalIntake { get; set; }

    public byte[]? Ay2025NmcDocument { get; set; }

    public DateOnly? Ay2025LopDate { get; set; }

    public byte[]? Ay2025LopDocument { get; set; }

    public int Ay2026ExistingIntake { get; set; }

    public int Ay2026AddRequestedIntake { get; set; }

    public int Ay2026TotalIntake { get; set; }

    public string? Courses { get; set; }
}
