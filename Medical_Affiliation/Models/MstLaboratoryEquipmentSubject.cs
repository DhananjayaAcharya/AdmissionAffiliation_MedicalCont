using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLaboratoryEquipmentSubject
{
    public int SubjectId { get; set; }

    public string? SubjectName { get; set; }

    public int FacultyId { get; set; }

    public string? CourseCode { get; set; }
}
