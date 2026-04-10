using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DepartmentWiseFacultyMaster
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string SeatSlabId { get; set; } = null!;

    public string DesignationCode { get; set; } = null!;

    public int Seats { get; set; }
}
