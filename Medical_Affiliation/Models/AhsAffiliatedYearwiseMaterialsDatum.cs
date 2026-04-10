using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhsAffiliatedYearwiseMaterialsDatum
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int ParametersId { get; set; }

    public string ParametersName { get; set; } = null!;

    public string? Year1 { get; set; }

    public string? Year2 { get; set; }

    public string? Year3 { get; set; }

    public string? ParentHospitalName { get; set; }

    public string? ParentHospitalAddress { get; set; }

    public byte[]? ParentHospitalMoudoc { get; set; }

    public byte[]? ParentHospitalOwnerNameDoc { get; set; }

    public byte[]? ParentHospitalKpmebedsDoc { get; set; }

    public byte[]? ParentHospitalPostBasicDoc { get; set; }

    public byte[]? ParentHospitalNabldoc { get; set; }

    public byte[]? ParentHospitalKspcdoc { get; set; }

    public string? Kpmebeds { get; set; }

    public string? PostBasicBeds { get; set; }

    public string? TotalBeds { get; set; }

    public string? HospitalOwnerName { get; set; }

    public string? HospitalType { get; set; }
}
