using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalCollegeLandBuildingDetail
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public int SeatSlab { get; set; }

    public int SeatIntake { get; set; }

    public bool IsTier2OrHilly { get; set; }

    public decimal? TotalLandAreaAcres { get; set; }

    public string? LandOwnershipType { get; set; }

    public bool? HasFutureExpansionSpace { get; set; }

    public string? SaleDeedDocumentPath { get; set; }

    public string? EncumbranceCertificateDocumentPath { get; set; }

    public string? LandUseCertificateDocumentPath { get; set; }

    public string? ApprovedLayoutPlanDocumentPath { get; set; }

    public string? LandSketchDocumentPath { get; set; }

    public string? DistanceCertificateDocumentPath { get; set; }

    public string? ApprovedBuildingPlanDocumentPath { get; set; }

    public string? CompletionCertificateDocumentPath { get; set; }

    public string? StructuralStabilityCertificateDocumentPath { get; set; }

    public string? FireSafetyNocDocumentPath { get; set; }

    public string? LiftLicenseDocumentPath { get; set; }

    public string? ElectricalSafetyCertificateDocumentPath { get; set; }

    public string? WaterSupplyCertificateDocumentPath { get; set; }

    public string? SewageSanitationApprovalDocumentPath { get; set; }

    public decimal? TotalBuiltupAreaSqm { get; set; }

    public int? LectureHallCount { get; set; }

    public decimal? LectureHallAreaSqm { get; set; }

    public int? LectureHallSeatingCapacity { get; set; }

    public decimal? ExaminationHallAreaSqm { get; set; }

    public decimal? LibraryAreaSqm { get; set; }

    public decimal? HospitalAreaSqm { get; set; }

    public decimal? MuseumDemoRoomsAreaSqm { get; set; }

    public decimal? DepartmentWiseAreaSqm { get; set; }

    public decimal? PreclinicalSkillLabAreaSqm { get; set; }

    public string? LandCategory { get; set; }

    public bool? IsLandInTwoPieces { get; set; }

    public decimal? DistanceBetweenCollegeAndHospitalKm { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
