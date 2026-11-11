using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblAyurvedaOtherEquipmentAvailability
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

    public int? AvailableQuantity60 { get; set; }

    public int? AvailableQuantity100 { get; set; }

    public int? AvailableQuantity150 { get; set; }

    public int? AvailableQuantity200 { get; set; }

    public int? EquipmentSectionId { get; set; }
}
