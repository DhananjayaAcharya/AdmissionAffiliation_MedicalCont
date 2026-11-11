using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstAyurvedaStaff
{
    public int StaffId { get; set; }

    public string StaffName { get; set; } = null!;

    public int FacultyId { get; set; }
}
