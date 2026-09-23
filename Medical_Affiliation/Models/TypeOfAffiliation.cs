using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TypeOfAffiliation
{
    public int TypeId { get; set; }

    public string TypeDescription { get; set; } = null!;

    public virtual ICollection<ActionTakenDeficiencyReport> ActionTakenDeficiencyReports { get; set; } = new List<ActionTakenDeficiencyReport>();

    public virtual ICollection<AdditionalInformationInAcademicActivity> AdditionalInformationInAcademicActivities { get; set; } = new List<AdditionalInformationInAcademicActivity>();

    public virtual ICollection<AffiliationFinalDeclaration> AffiliationFinalDeclarations { get; set; } = new List<AffiliationFinalDeclaration>();

    public virtual ICollection<AffiliationPayment> AffiliationPayments { get; set; } = new List<AffiliationPayment>();

    public virtual ICollection<AnimalHouseDetail> AnimalHouseDetails { get; set; } = new List<AnimalHouseDetail>();

    public virtual ICollection<DentalChair> DentalChairs { get; set; } = new List<DentalChair>();

    public virtual ICollection<DentalCollegeLandBuildingDetail> DentalCollegeLandBuildingDetails { get; set; } = new List<DentalCollegeLandBuildingDetail>();

    public virtual ICollection<DentalConferencesAttended> DentalConferencesAttendeds { get; set; } = new List<DentalConferencesAttended>();

    public virtual ICollection<DentalConferencesConducted> DentalConferencesConducteds { get; set; } = new List<DentalConferencesConducted>();

    public virtual ICollection<DentalFieldPracticeArea> DentalFieldPracticeAreas { get; set; } = new List<DentalFieldPracticeArea>();

    public virtual ICollection<DentalInfrastructure> DentalInfrastructures { get; set; } = new List<DentalInfrastructure>();

    public virtual ICollection<DentalService> DentalServices { get; set; } = new List<DentalService>();

    public virtual ICollection<DepartmentWiseResearchProject> DepartmentWiseResearchProjects { get; set; } = new List<DepartmentWiseResearchProject>();

    public virtual ICollection<HospitalDetailsForAffiliation> HospitalDetailsForAffiliations { get; set; } = new List<HospitalDetailsForAffiliation>();

    public virtual ICollection<IndoorBedsOccupancy> IndoorBedsOccupancies { get; set; } = new List<IndoorBedsOccupancy>();

    public virtual ICollection<IndoorInfrastructureRequirementsCompliance> IndoorInfrastructureRequirementsCompliances { get; set; } = new List<IndoorInfrastructureRequirementsCompliance>();

    public virtual ICollection<LibraryStaffDetail> LibraryStaffDetails { get; set; } = new List<LibraryStaffDetail>();

    public virtual ICollection<MedicalAlliedDisciplineDetail> MedicalAlliedDisciplineDetails { get; set; } = new List<MedicalAlliedDisciplineDetail>();

    public virtual ICollection<MedicalSkillsLaboratory> MedicalSkillsLaboratories { get; set; } = new List<MedicalSkillsLaboratory>();

    public virtual ICollection<MedicalUgbedDistribution> MedicalUgbedDistributions { get; set; } = new List<MedicalUgbedDistribution>();

    public virtual ICollection<MstDentalLibraryService> MstDentalLibraryServices { get; set; } = new List<MstDentalLibraryService>();

    public virtual ICollection<MstIndoorBedsDepartmentMaster> MstIndoorBedsDepartmentMasters { get; set; } = new List<MstIndoorBedsDepartmentMaster>();

    public virtual ICollection<MstIndoorBedsOccupancyMaster> MstIndoorBedsOccupancyMasters { get; set; } = new List<MstIndoorBedsOccupancyMaster>();

    public virtual ICollection<MstIndoorInfrastructureRequirementsMaster> MstIndoorInfrastructureRequirementsMasters { get; set; } = new List<MstIndoorInfrastructureRequirementsMaster>();

    public virtual ICollection<MstLibraryExpenditure> MstLibraryExpenditures { get; set; } = new List<MstLibraryExpenditure>();

    public virtual ICollection<SuperVisionInFieldPracticeArea> SuperVisionInFieldPracticeAreas { get; set; } = new List<SuperVisionInFieldPracticeArea>();

    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    public virtual ICollection<WorkShopDetail> WorkShopDetails { get; set; } = new List<WorkShopDetail>();
}
