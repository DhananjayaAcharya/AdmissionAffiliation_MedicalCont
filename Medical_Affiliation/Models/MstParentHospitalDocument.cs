using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstParentHospitalDocument
{
    public int HospitalDocId { get; set; }

    public string? HospitalDocName { get; set; }

    public string? CourseCode { get; set; }

    public int? FacultyId { get; set; }
}
