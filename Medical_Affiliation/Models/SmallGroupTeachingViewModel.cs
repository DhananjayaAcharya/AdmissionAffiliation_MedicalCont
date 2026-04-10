using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class SmallGroupTeachingViewModel : IValidatableObject
    {
        // ---------------- GENERAL ----------------
        [Required(ErrorMessage = "Annual MBBS Intake is required")]
        public int AnnualMbbsIntake { get; set; }

        public int SmallGroupBatchSize { get; set; } = 15;

        public bool? TeachingAreasSharedAllDepts { get; set; }
        public bool? AvInAllTeachingAreas { get; set; }
        public bool? InternetInAllTeachingAreas { get; set; }
        public bool? DigitalLinkAllTeachingAreas { get; set; }

        // ---------------- TEACHING ROOMS ----------------
        [Required(ErrorMessage = "Number of students is required")]
        [Range(1, 500)]
        public int SmallGroupStudents { get; set; }

        public decimal RequiredAreaSqm { get; set; }

        [Required(ErrorMessage = "Available area is required")]
        public decimal AvailableAreaSqm { get; set; }

        public decimal AreaDeficiencySqm { get; set; }

        public bool? RoomsSharedByAllDepts { get; set; }
        public bool? AppropriateAreaEachSpecialty { get; set; }
        public bool? ConnectedToLectureHalls { get; set; }
        public bool? InternetInTeachingRooms { get; set; }

        // ---------------- LABS ----------------
        public bool HistologyAvailable { get; set; }
        public bool HistologyShared { get; set; }

        public bool ClinicalPhysiologyAvailable { get; set; }
        public bool ClinicalPhysiologyShared { get; set; }

        public bool BiochemistryAvailable { get; set; }
        public bool BiochemistryShared { get; set; }

        public bool HistopathCytopathAvailable { get; set; }
        public bool HistopathCytopathShared { get; set; }

        public bool ClinPathHemeAvailable { get; set; }
        public bool ClinPathHemeShared { get; set; }

        public bool MicrobiologyAvailable { get; set; }
        public bool MicrobiologyShared { get; set; }

        public bool ClinicalPharmAvailable { get; set; }
        public bool ClinicalPharmShared { get; set; }

        public bool CalPharmAvailable { get; set; }
        public bool CalPharmShared { get; set; }

        // ✅ FIXED (checkbox fields MUST be bool)
        // ---------------- COMMON ----------------
        public bool AllLabsHaveAV { get; set; }
        public bool AllLabsHaveInternet { get; set; }
        public bool TechnicalStaffFacilitiesEnsured { get; set; }

        // ---------------- MUSEUM ----------------
        public bool? SeparateAnatomyMuseumAvailable { get; set; }
        public bool? PathologyForensicSharedMuseum { get; set; }
        public bool? PharmMicroCommSharedMuseum { get; set; }

        public int SeatingCapacityPerMuseum { get; set; }
        public decimal SeatingAreaAvailableSqm { get; set; }
        public decimal SeatingAreaRequiredSqm { get; set; }
        public decimal SeatingAreaDeficiencySqm { get; set; }

        public bool? MuseumsHaveAV { get; set; }
        public bool? MuseumsHaveInternet { get; set; }
        public bool? MuseumsDigitallyLinked { get; set; }
        public bool? MuseumsHaveRacksShelves { get; set; }
        public bool? MuseumsHaveRadiologyDisplay { get; set; }
        public bool? TeachingTimeSharingProgrammed { get; set; }

        // ---------------- LAND ----------------
        public bool? IsMinimumLandAvailable { get; set; }
        public string? LandDetailsIfYes { get; set; }
        public bool? HasPurchasePlanIfNo { get; set; }
        public bool? HasBudgetProvisionIfNo { get; set; }
        public bool? HasFutureExpansionSpace { get; set; }
        public IFormFile? LandRecordsDocument { get; set; }

        // ---------------- BUILDING ----------------
        public bool? IsBuildingAsPerCouncilNorms { get; set; }
        public string? LandOwnershipType { get; set; }
        public string? BuildingOwnershipType { get; set; }
        public decimal FloorAreaSqFt { get; set; }
        public int NumberOfBlocks { get; set; }
        public int NumberOfFloors { get; set; }
        public int YearOfConstruction { get; set; }

        public IFormFile? ApprovedBuildingPlanDocument { get; set; }

        public bool HasLandRecordsFile { get; set; }
        public bool HasApprovedBuildingPlanFile { get; set; }

        // ---------------- ADMIN ----------------
        public decimal? PrincipalChamberAreaSqFt { get; set; }
        public decimal? OfficeRoomAreaSqFt { get; set; }
        public decimal? StaffRoomsAreaSqFt { get; set; }
        public decimal? LectureHallsAreaSqFt { get; set; }
        public decimal? LaboratoriesAreaSqFt { get; set; }
        public decimal? SeminarHallAreaSqFt { get; set; }
        public decimal? AuditoriumAreaSqFt { get; set; }
        public decimal? MuseumAreaSqFt { get; set; }

        public bool? ExaminationHallAvailable { get; set; }
        public bool? AnimalHouseAvailable { get; set; }

        public int? WorkshopStaffCount { get; set; }
        public string? WorkshopEquipmentDetails { get; set; }
        public string? WorkshopScopeOfWork { get; set; }

        public decimal? AnimalHouseAreaSqFt { get; set; }
        public int? AnimalHouseStaffCount { get; set; }
        public string? AnimalTypes { get; set; }

        public decimal? CommitteeRoomsAreaSqFt { get; set; }
        public bool? CommonRoomMenAvailable { get; set; }
        public bool? CommonRoomWomenAvailable { get; set; }

        public bool? StudentHostelAvailable { get; set; }
        public bool? StaffQuartersPrincipal { get; set; }
        public bool? StaffQuartersOtherStaff { get; set; }
        public bool? StaffQuartersTeachingAncillary { get; set; }

        public bool? RegisteredUnderAnatomyAct { get; set; }

        // ---------------- RELAXED VALIDATION ----------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsMinimumLandAvailable == true && string.IsNullOrWhiteSpace(LandDetailsIfYes))
            {
                yield return new ValidationResult(
                    "Tip: Enter land details when available",
                    new[] { nameof(LandDetailsIfYes) });
            }

            if (IsMinimumLandAvailable == false &&
                (HasPurchasePlanIfNo != true || HasBudgetProvisionIfNo != true))
            {
                yield return new ValidationResult(
                    "Tip: Add purchase plan & budget",
                    new[] { nameof(HasPurchasePlanIfNo) });
            }
        }
    }
}