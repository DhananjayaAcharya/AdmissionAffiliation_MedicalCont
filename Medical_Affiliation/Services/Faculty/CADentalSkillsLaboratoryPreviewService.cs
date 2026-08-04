using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalSkillsLaboratoryPreviewService : ICADentalSkillsLaboratoryPreviewService
    {
        
        private readonly IUserContext _userContext;
        private readonly ApplicationDbContext _context;
        public CADentalSkillsLaboratoryPreviewService(ApplicationDbContext dbContext, IUserContext userContext)
        {
            
            _userContext = userContext;
            _context = dbContext;
        }

        public async Task<SkillsLabViewModel> GetDentalSkillsLaboratoryPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            var lab = await _context.MedicalSkillsLaboratories
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (lab == null)
                return new SkillsLabViewModel();

            var vm = new SkillsLabViewModel
            {
                AnnualMbbsIntake = lab.AnnualMbbsIntake,
                AnnualBdsIntake = lab.AnnualBdsIntake,

                TotalAreaAvailableSqm = lab.TotalAreaAvailableSqm,
                TotalAreaRequiredSqm = lab.TotalAreaRequiredSqm,
                TotalAreaDeficiencySqm = lab.TotalAreaDeficiencySqm,

                SixWeeksTrainingCompletedBeforeClinical = lab.SixWeeksTrainingCompletedBeforeClinical,

                NumberOfExaminationRooms = lab.NumberOfExaminationRooms,
                HasMinFourExamRooms = lab.HasMinFourExamRooms,
                HasDemoRoomSmallGroups = lab.HasDemoRoomSmallGroups,
                HasDebriefArea = lab.HasDebriefArea,
                HasFacultyCoordinatorRoom = lab.HasFacultyCoordinatorRoom,
                HasSupportStaffRoom = lab.HasSupportStaffRoom,
                HasStorageForMannequins = lab.HasStorageForMannequins,
                HasVideoRecordingFacility = lab.HasVideoRecordingFacility,

                NumberOfSkillStations = lab.NumberOfSkillStations,
                HasGroupAndIndividualStations = lab.HasGroupAndIndividualStations,
                HasRequiredTrainersAndMannequins = lab.HasRequiredTrainersAndMannequins,
                HasDedicatedTechnicalOfficer = lab.HasDedicatedTechnicalOfficer,
                HasAdequateSupportStaff = lab.HasAdequateSupportStaff,

                TeachingAreasHaveAV = lab.TeachingAreasHaveAv,
                TeachingAreasHaveInternet = lab.TeachingAreasHaveInternet,
                SkillsLabEnabledForELearning = lab.SkillsLabEnabledForElearning
            };

            vm.PreClinicalAndSkillsLabs = await (
                from d in _context.DentalPreClinicalAndSkillsLabAreaReqs

                join m in _context.MstDentalPreClinicalAndSkillsLaboratoryAreaReqs
                    on d.LabId equals m.Id

                where d.CollegeCode == collegeCode
                   && d.FacultyCode == 2

                orderby m.SectionCode, m.LaboratoryName

                select new DentalPreClinicalAndSkillsLabAreaReqVM
                {
                    Id = d.Id,

                    CollegeCode = d.CollegeCode,

                    FacultyCode = d.FacultyCode,

                    SeatIntake = m.SeatIntake,

                    LabId = d.LabId,

                    LabName = m.LaboratoryName,

                    SectionCode = m.SectionCode,

                    LaboratorySection = m.LaboratorySection,

                    RequiredAreaSqFt = m.AreaRequiredSqFt,

                    ExistingAreaSqFt = d.ExistingAreaSqFt
                })
                .ToListAsync();

            return vm;
        }
    }
}
