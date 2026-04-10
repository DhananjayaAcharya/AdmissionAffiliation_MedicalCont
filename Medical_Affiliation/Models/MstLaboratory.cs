using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstLaboratory
{
    public int LabId { get; set; }

    public string? Laboratories { get; set; }

    public long? SizeofLab { get; set; }

    public int Facultyid { get; set; }

    public int? NoOfLabs { get; set; }

    public string? CourseCode { get; set; }

    public int? Intakeid { get; set; }
}
