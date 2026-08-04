using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADepartmentOfficesMeuPreviewService
        : ICADepartmentOfficesMeuPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADepartmentOfficesMeuPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<DepartmentOfficesMeuPreviewVM> GetDepartmentOfficesMeuPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();
            var courseLevel = _userContext.CourseLevel;

            var entity = await _context.MedicalDepartmentOfficesMeus
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (entity == null)
                return new DepartmentOfficesMeuPreviewVM();

            var vm = new DepartmentOfficesMeuPreviewVM
            {
                HasHodRoomWithOfficeAndRecords = entity.HasHodRoomWithOfficeAndRecords,
                HasRoomsForFacultyAndResidents = entity.HasRoomsForFacultyAndResidents,
                FacultyRoomsHaveCommunicationComputerInternet = entity.FacultyRoomsHaveCommunicationComputerInternet,
                HasRoomsForNonTeachingStaff = entity.HasRoomsForNonTeachingStaff
            };

            if (_userContext.FacultyId == 2)
            {
                vm.Dental = new DentalEducationUnitPreviewVM
                {
                    HasDentalEducationUnit = entity.HasDentalEducationUnit,
                    DentalEducationUnitAreaSqm = entity.DentalEducationUnitAreaSqm,
                    DentalEducationUnitHasAudioVisual = entity.DentalEducationUnitHasAudioVisual,
                    DentalEducationUnitHasInternet = entity.DentalEducationUnitHasInternet,
                    DeuCoordinatorName = entity.DeuCoordinatorName,
                    DeuCoordinatorDesignationDepartment = entity.DeuCoordinatorDesignationDepartment,
                    DeuCoordinatorPhone = entity.DeuCoordinatorPhone,
                    DeuCoordinatorEmail = entity.DeuCoordinatorEmail,
                    DeuActivitiesLastAcademicYear = entity.DeuActivitiesLastAcademicYear,
                    HasDeuMembersListFile = !string.IsNullOrWhiteSpace(entity.DeuMembersListFilePath)
                };
            }
            else
            {
                vm.Medical = new MedicalEducationUnitPreviewVM
                {
                    HasMedicalEducationUnit = entity.HasMedicalEducationUnit,
                    MedicalEducationUnitAreaSqm = entity.MedicalEducationUnitAreaSqm,
                    MedicalEducationUnitHasAudioVisual = entity.MedicalEducationUnitHasAudioVisual,
                    MedicalEducationUnitHasInternet = entity.MedicalEducationUnitHasInternet,
                    MeuCoordinatorName = entity.MeuCoordinatorName,
                    MeuCoordinatorDesignationDepartment = entity.MeuCoordinatorDesignationDepartment,
                    MeuCoordinatorPhone = entity.MeuCoordinatorPhone,
                    MeuCoordinatorEmail = entity.MeuCoordinatorEmail,
                    MeuActivitiesLastAcademicYear = entity.MeuActivitiesLastAcademicYear,
                    HasMeuMembersListFile = !string.IsNullOrWhiteSpace(entity.MeuMembersListFilePath)
                };
            }

            return vm;
        }
    }
}