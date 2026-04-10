using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalCentralLibrarySpeciality
{
    public int Id { get; set; }

    public int CentralLibraryId { get; set; }

    public string DepartmentId { get; set; } = null!;

    public string? CollegeCode { get; set; }

    public string? FacultyCode { get; set; }

    public string? RegistrationNo { get; set; }

    public virtual MedicalCentralLibrary CentralLibrary { get; set; } = null!;

    public virtual MedMstSpecialityDepartmentsLibrary Department { get; set; } = null!;
}
