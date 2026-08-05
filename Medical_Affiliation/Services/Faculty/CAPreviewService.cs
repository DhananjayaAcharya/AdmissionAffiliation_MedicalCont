using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAPreviewService : ICAPreviewService
    {
        private readonly ICAAcademicService _academicService;
        private readonly ICAHospitalAffiliationService _hospitalService;
        private readonly ICALandClassEquipmentService _landClassEqService;
        private readonly IUserContext _userContext;
        private readonly ICAPaymentService _capaymentService;
        private readonly ICADeclarationService _cADeclarationService;
        private readonly ApplicationDbContext _context;
        private readonly ICAAcademicIntakeService _academicIntakeService;
        private readonly IInstitutionPreviewService _institutionPreviewService;
        private readonly ICAInstitutionBasicDetails _basicDetailsService;
        private readonly ICADentalLandBuildingPreviewService _cADentalLandBuildingPreviewService;
        private readonly ICADentalSkillsLaboratoryPreviewService _skillsLabService;
        private readonly ICADentalChairDistributionPreviewService _chairPreviewService;
        private readonly ICAHostelPreviewService _hostelPreviewService;
        private readonly ICAMedicalUGBedDistributionPreviewService _medicalUGBedDistributionPreviewService;
        private readonly ICADepartmentOfficesMeuPreviewService _departmentOfficeMeuPreviewService;
        private readonly ICAEquipmentPreviewService _equipmentPreviewService;
        private readonly ICAVehiclePreviewService _vehiclePreviewService;
        private readonly ICAMedicalLibraryPreviewService _cALibraryService;
        private readonly ICAAcademicPerformancePreviewService _academicPerformancePreviewService;
        private readonly IHumanResourcesPreviewService _humanResourcesPreviewService;
        public CAPreviewService(
            ICAAcademicService academicService,
            ICAHospitalAffiliationService hospitalService,
            ICALandClassEquipmentService landClassEqService,
            ICAPaymentService paymentService,
            ICADeclarationService declarationService,
            ICAAcademicIntakeService academicIntakeService,
            IInstitutionPreviewService institutionPreviewService,
            ICAInstitutionBasicDetails basicDetailsService,
            ICADentalLandBuildingPreviewService cADentalLandBuildingPreviewService,
            ICADentalChairDistributionPreviewService chairPreviewService,
            ICAHostelPreviewService hostelPreviewService,
            ICAEquipmentPreviewService equipmentPreviewService,
            ICAMedicalUGBedDistributionPreviewService medicalUGBedDistributionPreviewService,
            IUserContext userContext,
            ICADepartmentOfficesMeuPreviewService departmentOfficeMeuPreviewService,
            ICADentalSkillsLaboratoryPreviewService skillsLabService,
            ICAVehiclePreviewService vehiclePreviewService,
            ICAMedicalLibraryPreviewService cAMedicalLibraryPreviewService,
            ICAAcademicPerformancePreviewService cAAcademicPerformancePreviewService,
            IHumanResourcesPreviewService humanResourcesPreviewService,
            ApplicationDbContext dbContext)
        {
            _academicService = academicService;
            _hospitalService = hospitalService;
            _landClassEqService = landClassEqService;
            _skillsLabService = skillsLabService;
            _userContext = userContext;
            _cADeclarationService = declarationService;
            _academicIntakeService = academicIntakeService;
            _capaymentService = paymentService;
            _chairPreviewService = chairPreviewService;
            _institutionPreviewService = institutionPreviewService;
            _cADentalLandBuildingPreviewService = cADentalLandBuildingPreviewService;
            _basicDetailsService = basicDetailsService;
            _hostelPreviewService = hostelPreviewService;
            _equipmentPreviewService = equipmentPreviewService;
            _medicalUGBedDistributionPreviewService = medicalUGBedDistributionPreviewService;
            _vehiclePreviewService = vehiclePreviewService;
            _departmentOfficeMeuPreviewService = departmentOfficeMeuPreviewService;
            _cALibraryService = cAMedicalLibraryPreviewService;
            _academicPerformancePreviewService = cAAcademicPerformancePreviewService;
            _humanResourcesPreviewService = humanResourcesPreviewService;
            _context = dbContext;
        }

        public async Task<CApreviewViewModel> GetPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var collegeName = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).Select(e => e.CollegeName).FirstOrDefaultAsync();

            var facultyCode = _userContext.FacultyId;
            var facultyName = await _context.Faculties.Where(e => e.FacultyId == facultyCode).Select(e => e.FacultyName).FirstOrDefaultAsync();

            return new CApreviewViewModel
            {
                CollegeCode = _userContext.CollegeCode,
                FacultyCode = _userContext.FacultyId.ToString(),
                CollegeName = collegeName,
                FacultyName = facultyName,
                InstitutionBasicVM = await _basicDetailsService.GetAllDetails(),
                InstitutionPreviewVM = await _institutionPreviewService.GetInstitutionPreviewAsync(),
                CAacademicMattersVM = await _academicService.GetAcademicMattersAsync(),
                CAHospitalAFfiliationCompVM = await _hospitalService.GetHospitalAffiliationAsync(),
                DentalLandBuildingPreview = await _cADentalLandBuildingPreviewService.GetDentalLandBuildingPreviewAsync(),
                DentalSkillsLaboratoryVM = await _skillsLabService.GetDentalSkillsLaboratoryPreviewAsync(),
                PhysicalFacilities = await _landClassEqService.GetLandClassEquipmentService(),
                DentalChairDistribution = await _chairPreviewService.GetDentalChairDistributionPreviewAsync(),
                PaymentVM = await _capaymentService.GetPaymentDetails(),
                DeclarationVM = await _cADeclarationService.GetDeclarationDetails(),
                AcademicIntakeVM = await _academicIntakeService.GetAcademicIntakePreviewAsync(),
                HostelPreviewVM = await _hostelPreviewService.GetHostelPreviewAsync(),
                EquipmentPreviewVM = await _equipmentPreviewService.GetEquipmentPreviewAsync(),
                MedicalUGBedDistributionVM = await _medicalUGBedDistributionPreviewService.GetMedicalUGBedDistributionPreviewAsync(),
                DepartmentOfficesMeuVM = await _departmentOfficeMeuPreviewService.GetDepartmentOfficesMeuPreviewAsync(),
                VehiclePreviewVM = await _vehiclePreviewService.GetVehiclePreviewAsync(),
                AcademicPerformanceDisplayVm = await _academicPerformancePreviewService.GetAcademicPerformancePreviewAsync(),
                MedicalLibraryPreviewVM = await _cALibraryService.GetMedicalLibraryPreviewAsync(),
                HumanResourcesVM = await _humanResourcesPreviewService.GetHumanResourcesPreviewAsync(),

            };
        }


    }

}
