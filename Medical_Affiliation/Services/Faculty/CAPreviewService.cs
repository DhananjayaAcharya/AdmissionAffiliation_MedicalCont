//using Medical_Affiliation.DATA;
//using Medical_Affiliation.Models;
//using Medical_Affiliation.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;

//namespace Medical_Affiliation.Services.Faculty
//{
//    public class CAPreviewService : ICAPreviewService
//    {
//        private readonly ICAAcademicService _academicService;
//        private readonly ICAHospitalAffiliationService _hospitalService;
//        private readonly ICALandClassEquipmentService _landClassEqService;
//        private readonly IUserContext _userContext;
//        private readonly ApplicationDbContext _context;


//        public CAPreviewService( ICAAcademicService academicService,  ICAHospitalAffiliationService hospitalService, ICALandClassEquipmentService landClassEqService, IUserContext userContext, ApplicationDbContext dbContext)
//        {
//            _academicService = academicService;
//            _hospitalService = hospitalService;
//            _landClassEqService = landClassEqService;
//            _userContext = userContext;
//            _context = dbContext;
//        }

//        public async Task<CApreviewViewModel> GetPreviewAsync()
//        {
//            var collegeCode = _userContext.CollegeCode;
//            var collegeName = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).Select(e => e.CollegeName).FirstOrDefaultAsync();

//            var facultyCode = _userContext.FacultyId;
//            var facultyName = await _context.Faculties.Where(e => e.FacultyId == facultyCode).Select(e => e.FacultyName).FirstOrDefaultAsync();

//            return new CApreviewViewModel
//            {
//                CollegeCode = _userContext.CollegeCode,
//                FacultyCode = _userContext.FacultyId.ToString(),
//                CollegeName = collegeName,
//                FacultyName = facultyName,
//                CAacademicMattersVM = await _academicService.GetAcademicMattersAsync(),
//                CAHospitalAFfiliationCompVM = await _hospitalService.GetHospitalAffiliationAsync(),
//                PhysicalFacilities = await _landClassEqService.GetLandClassEquipmentService(),
//            };
//        }


//    }

//}
