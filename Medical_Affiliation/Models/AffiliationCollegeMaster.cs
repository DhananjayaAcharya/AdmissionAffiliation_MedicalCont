using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class AffiliationCollegeMaster
{
    public string CollegeCode { get; set; } = null!;

    public string? CollegeName { get; set; }

    public string? CollegeTown { get; set; }

    public string? FacultyCode { get; set; }

    public string? Password { get; set; }

    public string? HashedPassword { get; set; }

    public byte[]? AllDocsForCourse { get; set; }

    public string? IsDeclared { get; set; }

    public string? ChangedPassword { get; set; }

    public string? PrincipalNameDeclared { get; set; }

    public bool ShowNodalOfficerDetails { get; set; }

    public bool ShowIntakeDetails { get; set; }

    public bool ShowRepositoryDetails { get; set; }

    public string? PrincipalMobileNumber { get; set; }

    public string? DistrictId { get; set; }

    public string? TalukId { get; set; }

    public bool? Status { get; set; }

    public string? CollegeEmail { get; set; }

    public virtual ICollection<ActionTakenDeficiencyReport> ActionTakenDeficiencyReports { get; set; } = new List<ActionTakenDeficiencyReport>();

    public virtual ICollection<AffiliationFinalDeclaration> AffiliationFinalDeclarations { get; set; } = new List<AffiliationFinalDeclaration>();

    public virtual ICollection<AffiliationPayment> AffiliationPayments { get; set; } = new List<AffiliationPayment>();

    public virtual ICollection<AnimalHouseDetail> AnimalHouseDetails { get; set; } = new List<AnimalHouseDetail>();

    public virtual ICollection<DentalChair> DentalChairs { get; set; } = new List<DentalChair>();

    public virtual ICollection<DentalCollegeLandBuildingDetail> DentalCollegeLandBuildingDetails { get; set; } = new List<DentalCollegeLandBuildingDetail>();

    public virtual ICollection<DentalFieldPracticeArea> DentalFieldPracticeAreas { get; set; } = new List<DentalFieldPracticeArea>();

    public virtual ICollection<DentalInfrastructure> DentalInfrastructures { get; set; } = new List<DentalInfrastructure>();

    public virtual ICollection<DentalLibraryService> DentalLibraryServices { get; set; } = new List<DentalLibraryService>();

    public virtual ICollection<DentalService> DentalServices { get; set; } = new List<DentalService>();

    public virtual ICollection<DentalWardBedDistribution> DentalWardBedDistributions { get; set; } = new List<DentalWardBedDistribution>();

    public virtual ICollection<LibraryExpenditure> LibraryExpenditures { get; set; } = new List<LibraryExpenditure>();

    public virtual ICollection<LibraryStaffDetail> LibraryStaffDetails { get; set; } = new List<LibraryStaffDetail>();

    public virtual ICollection<MedicalAlliedDisciplineDetail> MedicalAlliedDisciplineDetails { get; set; } = new List<MedicalAlliedDisciplineDetail>();

    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    public virtual ICollection<WorkShopDetail> WorkShopDetails { get; set; } = new List<WorkShopDetail>();
}
