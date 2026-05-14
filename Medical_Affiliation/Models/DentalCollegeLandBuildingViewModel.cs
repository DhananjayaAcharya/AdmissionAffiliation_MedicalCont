using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.ViewModels
{
    public class DentalCollegeLandBuildingViewModel
    {
        public int Id { get; set; }

        // =====================================================
        // BASIC DETAILS
        // =====================================================

        public string CollegeCode { get; set; } = null!;

        public int FacultyCode { get; set; }

        public int SeatSlab { get; set; }

        public int SeatIntake { get; set; }

        public string? LandCategory { get; set; }

        // =====================================================
        // REQUIRED NORMS (FROM MASTER TABLE)
        // =====================================================

        public decimal? RequiredLandAcres { get; set; }

        public int RequiredBuiltupAreaSqm { get; set; }

        public int RequiredLectureHallAreaSqm { get; set; }

        public int RequiredLectureHallCapacity { get; set; }

        public int RequiredExamHallAreaSqm { get; set; }

        public int RequiredLibraryAreaSqm { get; set; }

        public int RequiredHospitalAreaSqm { get; set; }
        public int RequiredLectureHallCount { get; set; }
        public string? RequiredLandRequirementText { get; set; }

        // =====================================================
        // ACTUAL SUBMITTED DETAILS
        // =====================================================

        // LAND DETAILS

        [Display(Name = "Total Land Area (Acres)")]
        public decimal? TotalLandAreaAcres { get; set; }

        [Display(Name = "Land Ownership Type")]
        public string? LandOwnershipType { get; set; }

        [Display(Name = "Future Expansion Space Available")]
        public bool? HasFutureExpansionSpace { get; set; }

        // =====================================================
        // BUILDING DETAILS
        // =====================================================

        [Display(Name = "Total Built-up Area (Sq.m)")]
        public decimal? TotalBuiltupAreaSqm { get; set; }

        [Display(Name = "Lecture Hall Count")]
        public int? LectureHallCount { get; set; }

        [Display(Name = "Lecture Hall Area (Sq.m)")]
        public decimal? LectureHallAreaSqm { get; set; }

        [Display(Name = "Lecture Hall Seating Capacity")]
        public int? LectureHallSeatingCapacity { get; set; }

        [Display(Name = "Examination Hall Area (Sq.m)")]
        public decimal? ExaminationHallAreaSqm { get; set; }

        [Display(Name = "Library Area (Sq.m)")]
        public decimal? LibraryAreaSqm { get; set; }

        [Display(Name = "Museum & Demo Rooms Area (Sq.m)")]
        public decimal? MuseumDemoRoomsAreaSqm { get; set; }

        [Display(Name = "Department-wise Area (Sq.m)")]
        public decimal? DepartmentWiseAreaSqm { get; set; }

        [Display(Name = "Preclinical & Skill Lab Area (Sq.m)")]
        public decimal? PreclinicalSkillLabAreaSqm { get; set; }

        // =====================================================
        // LAND DOCUMENTS
        // =====================================================

        public IFormFile? SaleDeedDocument { get; set; }

        public string? SaleDeedDocumentPath { get; set; }

        public IFormFile? EncumbranceCertificateDocument { get; set; }

        public string? EncumbranceCertificateDocumentPath { get; set; }

        public IFormFile? LandUseCertificateDocument { get; set; }

        public string? LandUseCertificateDocumentPath { get; set; }

        public IFormFile? ApprovedLayoutPlanDocument { get; set; }

        public string? ApprovedLayoutPlanDocumentPath { get; set; }

        public IFormFile? LandSketchDocument { get; set; }

        public string? LandSketchDocumentPath { get; set; }

        public IFormFile? DistanceCertificateDocument { get; set; }

        public string? DistanceCertificateDocumentPath { get; set; }

        // =====================================================
        // BUILDING DOCUMENTS
        // =====================================================

        public IFormFile? ApprovedBuildingPlanDocument { get; set; }

        public string? ApprovedBuildingPlanDocumentPath { get; set; }

        public IFormFile? CompletionCertificateDocument { get; set; }

        public string? CompletionCertificateDocumentPath { get; set; }

        public IFormFile? StructuralStabilityCertificateDocument { get; set; }

        public string? StructuralStabilityCertificateDocumentPath { get; set; }

        public IFormFile? FireSafetyNocDocument { get; set; }

        public string? FireSafetyNocDocumentPath { get; set; }

        public IFormFile? LiftLicenseDocument { get; set; }

        public string? LiftLicenseDocumentPath { get; set; }

        public IFormFile? ElectricalSafetyCertificateDocument { get; set; }

        public string? ElectricalSafetyCertificateDocumentPath { get; set; }

        public IFormFile? WaterSupplyCertificateDocument { get; set; }

        public string? WaterSupplyCertificateDocumentPath { get; set; }

        public IFormFile? SewageSanitationApprovalDocument { get; set; }

        public string? SewageSanitationApprovalDocumentPath { get; set; }

        [Display(Name = "Hospital Area (Sq.m)")]
        public decimal? HospitalAreaSqm { get; set; }

        // =====================================================
        // REMARKS
        // =====================================================

        // =====================================================
        // PRE CLINICAL & SKILL LABORATORIES
        // =====================================================

        public List<DentalPreClinicalAndSkillsLabAreaReqVM> PreClinicalAndSkillsLabs { get; set; }
            = new();

        public string? Remarks { get; set; }
    }
}