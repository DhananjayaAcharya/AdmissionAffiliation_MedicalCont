using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalUgbedDistribution
{
    public int Id { get; set; }

    public int? GenMedicine { get; set; }

    public int? Paediatrics { get; set; }

    public int? SkinVd { get; set; }

    public int? Psychiatry { get; set; }

    public int? GenSurgery { get; set; }

    public int? Orthopaedics { get; set; }

    public int? Ophthalmology { get; set; }

    public int? Ent { get; set; }

    public int? ObstetricsAnc { get; set; }

    public int? Gynaecology { get; set; }

    public int? Postpartum { get; set; }

    public int? MajorOt { get; set; }

    public int? MinorOt { get; set; }

    public int? Iccu { get; set; }

    public int? Icu { get; set; }

    public int? PicuNicu { get; set; }

    public int? Sicu { get; set; }

    public int? TotalIcubeds { get; set; }

    public int? CasualtyBeds { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? FacultyCode { get; set; }

    public string? CollegeCode { get; set; }

    public string? CourseLevel { get; set; }
}
