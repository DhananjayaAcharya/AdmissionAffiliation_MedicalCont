using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AhshospitalDetail
{
    public int Id { get; set; }

    public string? CollegeCode { get; set; }

    public string? CollegeName { get; set; }

    public int? IsParentMedicalCollegeHospitalExists { get; set; }

    public int? IsParentHospitalExists { get; set; }

    public string? HospitalName { get; set; }

    public string? HospitalDistrict { get; set; }

    public string? HospitalState { get; set; }

    public string? HospitalTaluk { get; set; }

    public string? HospitalLocation { get; set; }

    public int? OpdPerDay { get; set; }

    public int? IpdPerDay { get; set; }

    public int? TotalBedsAvailable { get; set; }

    public int? IsParentOrAffHospitalforotherAhsInstitution { get; set; }

    public string? CollegeToHospitalDistance { get; set; }

    public int? IsManagedbyTrustMember { get; set; }

    public int? IsOwnerAtrustMember { get; set; }
}
