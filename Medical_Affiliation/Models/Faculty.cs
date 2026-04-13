using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Faculty
{
    public int FacultyId { get; set; }

    public string FacultyName { get; set; } = null!;

    public int? EmsFacultyId { get; set; }

    public string? FacultyAbbre { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<AffiliationPayment> AffiliationPayments { get; set; } = new List<AffiliationPayment>();

    public virtual ICollection<HealthCenterChp> HealthCenterChps { get; set; } = new List<HealthCenterChp>();

    public virtual ICollection<HospitalDocumentDetail> HospitalDocumentDetails { get; set; } = new List<HospitalDocumentDetail>();

    public virtual ICollection<HospitalFacility> HospitalFacilities { get; set; } = new List<HospitalFacility>();

    public virtual ICollection<IndoorBedsOccupancy> IndoorBedsOccupancies { get; set; } = new List<IndoorBedsOccupancy>();

    public virtual ICollection<IndoorInfrastructureRequirementsCompliance> IndoorInfrastructureRequirementsCompliances { get; set; } = new List<IndoorInfrastructureRequirementsCompliance>();

    public virtual ICollection<MstAdministration> MstAdministrations { get; set; } = new List<MstAdministration>();

    public virtual ICollection<MstFieldTypeChp> MstFieldTypeChps { get; set; } = new List<MstFieldTypeChp>();

    public virtual ICollection<MstFpaAdopAffType> MstFpaAdopAffTypes { get; set; } = new List<MstFpaAdopAffType>();

    public virtual ICollection<MstHospitalDocument> MstHospitalDocuments { get; set; } = new List<MstHospitalDocument>();

    public virtual ICollection<MstHospitalOwnedBy> MstHospitalOwnedBies { get; set; } = new List<MstHospitalOwnedBy>();

    public virtual ICollection<MstHospitalType> MstHospitalTypes { get; set; } = new List<MstHospitalType>();

    public virtual ICollection<MstIndoorBedsDepartmentMaster> MstIndoorBedsDepartmentMasters { get; set; } = new List<MstIndoorBedsDepartmentMaster>();

    public virtual ICollection<MstIndoorBedsOccupancyMaster> MstIndoorBedsOccupancyMasters { get; set; } = new List<MstIndoorBedsOccupancyMaster>();

    public virtual ICollection<MstIndoorInfrastructureRequirementsMaster> MstIndoorInfrastructureRequirementsMasters { get; set; } = new List<MstIndoorInfrastructureRequirementsMaster>();

    public virtual ICollection<SuperVisionInFieldPracticeArea> SuperVisionInFieldPracticeAreas { get; set; } = new List<SuperVisionInFieldPracticeArea>();
}
