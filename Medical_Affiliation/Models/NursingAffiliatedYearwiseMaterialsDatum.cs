using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class NursingAffiliatedYearwiseMaterialsDatum
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public int ParametersId { get; set; }

    public string ParametersName { get; set; } = null!;

    public string Year1 { get; set; } = null!;

    public string Year2 { get; set; } = null!;

    public string Year3 { get; set; } = null!;

    public string? ParentHospitalName { get; set; }

    public string? ParentHospitalAddress { get; set; }

    public byte[]? ParentHospitalMoudoc { get; set; }

    public byte[]? ParentHospitalOwnerNameDoc { get; set; }

    public byte[]? ParentHospitalKpmebedsDoc { get; set; }

    public byte[]? ParentHospitalPostBasicDoc { get; set; }

    public string? Kpmebeds { get; set; }

    public string? PostBasicBeds { get; set; }

    public string? TotalBeds { get; set; }

    public string? HospitalOwnerName { get; set; }

    public string? HospitalType { get; set; }

    public byte[]? PolutionControlDoc { get; set; }

    public byte[]? FireSafetyDoc { get; set; }

    public byte[]? BiomedicalWasteManagemenDoc { get; set; }

    public byte[]? OpdDocument { get; set; }

    public byte[]? IpdDocument { get; set; }

    public byte[]? MajorOperationsSurgeries { get; set; }

    public byte[]? MinorOperationsSurgeries { get; set; }
}
