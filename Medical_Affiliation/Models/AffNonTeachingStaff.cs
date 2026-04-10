using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffNonTeachingStaff
{
    public int StaffId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public string StaffName { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public string? MobileNumber { get; set; }

    public decimal SalaryPaid { get; set; }

    public bool PfProvided { get; set; }

    public bool EsiProvided { get; set; }

    public bool ServiceRegisterMaintained { get; set; }

    public bool SalaryAcquaintanceRegister { get; set; }
}
