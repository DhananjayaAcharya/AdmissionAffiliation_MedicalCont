using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAPreviewDentalService : ICADentalPreviewService
    {
        private readonly ICAInstitutionBasicDetails _basicDetailsService;
        private readonly ICATrustDetailsService _cATrustDetailsService;

        private readonly ITeachingFacultyDetailsService _teachingFacultyDetailsPreviewService;
        private readonly IUGPgIntakeDetailsService _ugPgIntakeDetailsService;
        private readonly ICATrustMemberDetailsPreviewService _trustMembersDetailsService;
        private readonly ICADentalChairDistributionPreviewService _chairPreviewService;
        private readonly IInstitutionPreviewService _institutionPreviewService;
        private readonly IHumanResourcesPreviewService _humanResourcesPreviewService;
        private readonly ICAAcademicService _academicService;
        private readonly ICADentalHospitalAffiliationService _hospitalService;
        private readonly ICALandClassEquipmentService _landClassEqService;
        private readonly ICADentalLandBuildingPreviewService _cADentalLandBuildingPreviewService;

        private readonly ICAAcademicIntakeService _academicIntakeService;
        private readonly IUserContext _userContext;
        private readonly ICAHostelPreviewService _hostelPreviewService;

        private readonly ICAPaymentService _capaymentService;
        private readonly ICAVehiclePreviewService _vehiclePreviewService;
        private readonly ICADeclarationService _cADeclarationService;
        private readonly ApplicationDbContext _context;


        public CAPreviewDentalService(
            ICAAcademicService academicService,
            IInstitutionPreviewService institutionPreviewService,
            ICAInstitutionBasicDetails basicDetailsService,
            ICADentalHospitalAffiliationService hospitalService,
            ICAAcademicIntakeService academicIntakeService,
            ICADentalLandBuildingPreviewService cADentalLandBuildingPreviewService,
            ICADentalChairDistributionPreviewService chairPreviewService,
            ICALandClassEquipmentService landClassEqService,
            ICAHostelPreviewService hostelPreviewService,
            ICAPaymentService paymentService,
            ITeachingFacultyDetailsService teachingFacultyDetailsPreviewService,
            ICADeclarationService declarationService,
            ICAVehiclePreviewService vehiclePreviewService,
            IHumanResourcesPreviewService humanResourcesPreviewService,
            ICATrustDetailsService cATrustDetailsService,
            IUGPgIntakeDetailsService ugPgIntakeDetailsService,
            ICATrustMemberDetailsPreviewService cATrustMemberDetailsPreviewService,
            IUserContext userContext, 
            ApplicationDbContext dbContext)
        {
            _basicDetailsService = basicDetailsService;
            _academicIntakeService = academicIntakeService;
            _trustMembersDetailsService = cATrustMemberDetailsPreviewService;
            _cATrustDetailsService = cATrustDetailsService;
            _cADentalLandBuildingPreviewService = cADentalLandBuildingPreviewService;
            _academicService = academicService;
            _hospitalService = hospitalService;
            _hostelPreviewService = hostelPreviewService;
            _vehiclePreviewService = vehiclePreviewService;
            _institutionPreviewService = institutionPreviewService;
            _landClassEqService = landClassEqService;
            _chairPreviewService = chairPreviewService;
            _teachingFacultyDetailsPreviewService = teachingFacultyDetailsPreviewService;
            _userContext = userContext;
            _ugPgIntakeDetailsService = ugPgIntakeDetailsService;
            _humanResourcesPreviewService = humanResourcesPreviewService;
            _cADeclarationService = declarationService;
            _capaymentService = paymentService;
            _context = dbContext;
        }

        public async Task<CADentalpreviewViewModel> GetDentalPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var collegeName = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).Select(e => e.CollegeName).FirstOrDefaultAsync();

            var facultyCode = _userContext.FacultyId;
            var facultyName = await _context.Faculties.Where(e => e.FacultyId == facultyCode).Select(e => e.FacultyName).FirstOrDefaultAsync();

            return new CADentalpreviewViewModel
            {
                CollegeCode = _userContext.CollegeCode,
                FacultyCode = _userContext.FacultyId.ToString(),
                CollegeName = collegeName,
                FacultyName = facultyName,
                InstitutionBasicVM = await _basicDetailsService.GetAllDentalDetails(),
                AcademicIntakeVM = await _academicIntakeService.GetAcademicIntakePreviewAsync(),
                TrustDetailsVM = await _cATrustDetailsService.GetTrustDetailsAsync(),
                TeachingFacultyDetailsVM = await _teachingFacultyDetailsPreviewService.GetTeachingFacultyDetailsAsync(),
                TrustMemberDetailsVM = await _trustMembersDetailsService.GetTrustMemberDetailsAsync(),
                DentalChairDistribution = await _chairPreviewService.GetDentalChairDistributionPreviewAsync(),
                DentalLandBuildingPreview = await _cADentalLandBuildingPreviewService.GetDentalLandBuildingPreviewAsync(),
                UGPgIntakeDetailsVM = await _ugPgIntakeDetailsService.GetUgCourseDetailsAsync(),
                HostelPreviewVM = await _hostelPreviewService.GetHostelPreviewAsync(),
                InstitutionPreviewVM = await _institutionPreviewService.GetInstitutionPreviewAsync(),
                VehiclePreviewVM = await _vehiclePreviewService.GetVehiclePreviewAsync(),
                CAacademicMattersVM = await _academicService.GetAcademicMattersAsync(),
                CAHospitalAFfiliationCompVM = await _hospitalService.GetHospitalAffiliationAsync(),
                PhysicalFacilities = await _landClassEqService.GetLandClassEquipmentService(),
                PaymentVM = await _capaymentService.GetPaymentDetails(),
                DeclarationVM = await _cADeclarationService.GetDeclarationDetails(),
                HumanResourcesVM = await _humanResourcesPreviewService.GetHumanResourcesPreviewAsync(),

            };
        }


    }

}
