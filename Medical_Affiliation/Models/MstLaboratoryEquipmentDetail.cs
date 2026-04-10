using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLaboratoryEquipmentDetail
{
    public int EquipmentId { get; set; }

    public string? EquipmentName { get; set; }

    public int RequiredAsPerNorm { get; set; }

    public string? CourseCode { get; set; }

    public int FacultyId { get; set; }

    public string? Subjects { get; set; }

    public int? SubId { get; set; }
}
