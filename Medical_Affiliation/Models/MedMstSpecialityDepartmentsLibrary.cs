using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedMstSpecialityDepartmentsLibrary
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string SpecialityDepartments { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public virtual ICollection<MedicalCentralLibrarySpeciality> MedicalCentralLibrarySpecialities { get; set; } = new List<MedicalCentralLibrarySpeciality>();
}
