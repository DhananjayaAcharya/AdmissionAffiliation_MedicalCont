using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class HospitalDetailsForAffiliation
{
    public int HospitalDetailsId { get; set; }

    public int AffiliationTypeId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string FacultyCode { get; set; } = null!;

    public bool? ParentMedicalCollegeExists { get; set; }

    public string? HospitalType { get; set; }

    public string? HospitalOwnedBy { get; set; }

    public string? HospitalOwnerName { get; set; }

    public string HospitalDistrictId { get; set; } = null!;

    public string HospitalTalukId { get; set; } = null!;

    public string? Location { get; set; }

    public int? TotalBeds { get; set; }

    public int? OpdperDay { get; set; }

    public decimal? IpdbedOccupancyPercent { get; set; }

    public int? AnnualOpdprevYear { get; set; }

    public int? AnnualIpdprevYear { get; set; }

    public bool? IsParentHospitalForOtherNursingInstitution { get; set; }

    public decimal? DistanceBetweenCollegeAndHospitalKm { get; set; }

    public bool? IsOwnerAmemberOfTrust { get; set; }

    public string? HospitalName { get; set; }

    public byte[]? HospitalParentSupportingDoc { get; set; }

    public string? CourseLevel { get; set; }

    public virtual TypeOfAffiliation AffiliationType { get; set; } = null!;

    public virtual DistrictMaster HospitalDistrict { get; set; } = null!;

    public virtual ICollection<HospitalDocumentsToBeUploaded> HospitalDocumentsToBeUploadeds { get; set; } = new List<HospitalDocumentsToBeUploaded>();

    public virtual ICollection<HospitalFacility> HospitalFacilities { get; set; } = new List<HospitalFacility>();

    public virtual ICollection<IndoorInfrastructureRequirementsCompliance> IndoorInfrastructureRequirementsCompliances { get; set; } = new List<IndoorInfrastructureRequirementsCompliance>();

    public virtual ICollection<SuperVisionInFieldPracticeArea> SuperVisionInFieldPracticeAreas { get; set; } = new List<SuperVisionInFieldPracticeArea>();
}
