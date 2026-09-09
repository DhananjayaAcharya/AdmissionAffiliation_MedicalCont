using Medical_Affiliation.DATA;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalLandBuildingPreviewService : ICADentalLandBuildingPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADentalLandBuildingPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<DentalCollegeLandBuildingViewModel> GetDentalLandBuildingPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var academicIntake = await _context.AcademicIntakes
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString());

            if (academicIntake == null)
                return new DentalCollegeLandBuildingViewModel();

            int seatIntake = academicIntake.Ay2026TotalIntake;

            // Round up to nearest 50
            int seatSlab = ((seatIntake + 49) / 50) * 50;

            var slabNorm = await _context.UgSeatSlabNormMasters
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.SeatSlab == seatSlab);

            var infrastructure = await _context.DentalInfrastructures
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.SeatSlab == seatSlab)
                .OrderBy(x => x.Requirement.SlNo)
                .Select(x => new DentalInfrastructureVM
                {
                    Id = x.Id,
                    FacultyCode = x.FacultyCode,
                    AffiliationTypeId = x.AffiliationTypeId,
                    CollegeCode = x.CollegeCode,
                    HospitalDetailsId = x.HospitalDetailsId,
                    RequirementId = x.RequirementId,
                    SlNo = x.Requirement.SlNo,
                    RequirementName = x.Requirement.RequirementName,
                    RequirementDescription = x.Requirement.RequirementDescription,
                    SeatSlab = x.SeatSlab,
                    RequiredAreaSqFt = x.Requirement.RequiredAreaSqFt,
                    AvailableAreaSqFt = x.AvailableAreaSqFt,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn
                })
                .ToListAsync();

            var entity = await _context.DentalCollegeLandBuildingDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            var vm = new DentalCollegeLandBuildingViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                SeatIntake = seatIntake,
                SeatSlab = seatSlab,
                InfrastructureDetails = infrastructure
            };

            if (slabNorm != null)
            {
                vm.RequiredLandAcres = vm.LandCategory == "Tier2" ? slabNorm.LandTier2Acres : slabNorm.LandOtherAreaAcres;
                vm.RequiredBuiltupAreaSqm = slabNorm.CollegeBuiltupAreaSqm;
                vm.RequiredLectureHallAreaSqm = slabNorm.LectureHallAreaSqm;
                vm.RequiredLectureHallCapacity = slabNorm.LectureHallCapacity;
                vm.RequiredExamHallAreaSqm = slabNorm.ExaminationHallAreaSqm;
                vm.RequiredLibraryAreaSqm = slabNorm.LibraryAreaSqm;
                vm.RequiredHospitalAreaSqm = slabNorm.DentalHospitalAreaSqm;
                vm.RequiredLectureHallCount = slabNorm.LectureHallCount;
            }

            if (entity == null)
                return vm;

            vm.Id = entity.Id;
            vm.LandCategory = entity.LandCategory;
            vm.TotalLandAreaAcres = entity.TotalLandAreaAcres;
            vm.LandOwnershipType = entity.LandOwnershipType;
            vm.HasFutureExpansionSpace = entity.HasFutureExpansionSpace;

            vm.TotalBuiltupAreaSqm = entity.TotalBuiltupAreaSqm;
            vm.LectureHallCount = entity.LectureHallCount;
            vm.LectureHallAreaSqm = entity.LectureHallAreaSqm;
            vm.LectureHallSeatingCapacity = entity.LectureHallSeatingCapacity;
            vm.ExaminationHallAreaSqm = entity.ExaminationHallAreaSqm;
            vm.LibraryAreaSqm = entity.LibraryAreaSqm;
            vm.HospitalAreaSqm = entity.HospitalAreaSqm;
            vm.MuseumDemoRoomsAreaSqm = entity.MuseumDemoRoomsAreaSqm;
            vm.DepartmentWiseAreaSqm = entity.DepartmentWiseAreaSqm;
            vm.PreclinicalSkillLabAreaSqm = entity.PreclinicalSkillLabAreaSqm;
            vm.Remarks = entity.Remarks;

            vm.SaleDeedDocumentPath = entity.SaleDeedDocumentPath;
            vm.EncumbranceCertificateDocumentPath = entity.EncumbranceCertificateDocumentPath;
            vm.LandUseCertificateDocumentPath = entity.LandUseCertificateDocumentPath;
            vm.ApprovedLayoutPlanDocumentPath = entity.ApprovedLayoutPlanDocumentPath;
            vm.LandSketchDocumentPath = entity.LandSketchDocumentPath;
            vm.DistanceCertificateDocumentPath = entity.DistanceCertificateDocumentPath;

            vm.ApprovedBuildingPlanDocumentPath = entity.ApprovedBuildingPlanDocumentPath;
            vm.CompletionCertificateDocumentPath = entity.CompletionCertificateDocumentPath;
            vm.StructuralStabilityCertificateDocumentPath = entity.StructuralStabilityCertificateDocumentPath;
            vm.FireSafetyNocDocumentPath = entity.FireSafetyNocDocumentPath;
            vm.LiftLicenseDocumentPath = entity.LiftLicenseDocumentPath;
            vm.ElectricalSafetyCertificateDocumentPath = entity.ElectricalSafetyCertificateDocumentPath;
            vm.WaterSupplyCertificateDocumentPath = entity.WaterSupplyCertificateDocumentPath;
            vm.SewageSanitationApprovalDocumentPath = entity.SewageSanitationApprovalDocumentPath;

            return vm;
        }
    }
}
