using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TypeOfAffiliation
{
    public int TypeId { get; set; }

    public string TypeDescription { get; set; } = null!;

    public virtual ICollection<AffiliationFinalDeclaration> AffiliationFinalDeclarations { get; set; } = new List<AffiliationFinalDeclaration>();

    public virtual ICollection<AffiliationPayment> AffiliationPayments { get; set; } = new List<AffiliationPayment>();

    public virtual ICollection<HospitalDetailsForAffiliation> HospitalDetailsForAffiliations { get; set; } = new List<HospitalDetailsForAffiliation>();

    public virtual ICollection<IndoorBedsOccupancy> IndoorBedsOccupancies { get; set; } = new List<IndoorBedsOccupancy>();

    public virtual ICollection<IndoorInfrastructureRequirementsCompliance> IndoorInfrastructureRequirementsCompliances { get; set; } = new List<IndoorInfrastructureRequirementsCompliance>();

    public virtual ICollection<MstIndoorBedsDepartmentMaster> MstIndoorBedsDepartmentMasters { get; set; } = new List<MstIndoorBedsDepartmentMaster>();

    public virtual ICollection<MstIndoorBedsOccupancyMaster> MstIndoorBedsOccupancyMasters { get; set; } = new List<MstIndoorBedsOccupancyMaster>();

    public virtual ICollection<MstIndoorInfrastructureRequirementsMaster> MstIndoorInfrastructureRequirementsMasters { get; set; } = new List<MstIndoorInfrastructureRequirementsMaster>();

    public virtual ICollection<SuperVisionInFieldPracticeArea> SuperVisionInFieldPracticeAreas { get; set; } = new List<SuperVisionInFieldPracticeArea>();
}
