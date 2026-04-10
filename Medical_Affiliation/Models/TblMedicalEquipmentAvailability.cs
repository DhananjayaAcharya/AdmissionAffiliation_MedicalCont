using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblMedicalEquipmentAvailability
{
    public int Id { get; set; }

    public int FacultyId { get; set; }

    public string? CourseCode { get; set; }

    public int EquipmentId { get; set; }

    public bool IsAvailable { get; set; }

    public int? AvailableQuantity { get; set; }

    public string? AcademicYear { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CollegeCode { get; set; }
}
