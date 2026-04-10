using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstParentHospitalEquipment
{
    public int HospitalEquipmentId { get; set; }

    public string? HospitalEquipmentName { get; set; }

    public int RequiredAsPerNorm { get; set; }

    public string? CourseCode { get; set; }

    public int FacultyId { get; set; }
}
