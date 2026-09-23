using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADepartmentOfficesMeuService : ICADepartmentOfficesMeuService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CADepartmentOfficesMeuService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DepartmentOfficesMeuPreviewVM?> GetDepartmentOfficesMeuAsync()
        {
            // =========================================================
            // USER CONTEXT
            // =========================================================

            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            // This service is ONLY for Dental
            if (facultyCode != 2)
            {
                return null;
            }

            // =========================================================
            // COURSE LEVEL
            // =========================================================

            var courseLevel =
                _httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                ?? _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel")
                ?? _userContext.CourseLevel;

            courseLevel = courseLevel?
                .Trim()
                .ToUpperInvariant();

            // =========================================================
            // VALIDATION
            // =========================================================

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                return null;
            }

            // =========================================================
            // GET DENTAL DEPARTMENT / DEU DATA
            // =========================================================

            var entity = await _context.MedicalDepartmentOfficesMeus
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == "2" &&
                    x.CourseLevel != null &&
                    x.CourseLevel.Trim().ToUpper() == courseLevel)
                .OrderByDescending(x => x.UpdatedOn ?? x.CreatedOn)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return null;
            }

            // =========================================================
            // BUILD PREVIEW VIEW MODEL
            // =========================================================

            var vm = new DepartmentOfficesMeuPreviewVM
            {
                // =====================================================
                // COURSE LEVEL
                // =====================================================

                CourseLevel = courseLevel,

                // =====================================================
                // COMMON DEPARTMENT OFFICE DETAILS
                // =====================================================

                HasHodRoomWithOfficeAndRecords =
                    entity.HasHodRoomWithOfficeAndRecords,

                HasRoomsForFacultyAndResidents =
                    entity.HasRoomsForFacultyAndResidents,

                FacultyRoomsHaveCommunicationComputerInternet =
                    entity.FacultyRoomsHaveCommunicationComputerInternet,

                HasRoomsForNonTeachingStaff =
                    entity.HasRoomsForNonTeachingStaff,

                // =====================================================
                // DENTAL EDUCATION UNIT
                // =====================================================

                Dental = new DentalEducationUnitPreviewVM
                {
                    // -------------------------------------------------
                    // Dental Education Unit
                    // -------------------------------------------------

                    HasDentalEducationUnit =
                        entity.HasDentalEducationUnit,

                    DentalEducationUnitAreaSqm =
                        entity.DentalEducationUnitAreaSqm,

                    DentalEducationUnitHasAudioVisual =
                        entity.DentalEducationUnitHasAudioVisual,

                    DentalEducationUnitHasInternet =
                        entity.DentalEducationUnitHasInternet,

                    // -------------------------------------------------
                    // DEU Coordinator Details
                    // -------------------------------------------------

                    DeuCoordinatorName =
                        entity.DeuCoordinatorName,

                    DeuCoordinatorDesignationDepartment =
                        entity.DeuCoordinatorDesignationDepartment,

                    DeuCoordinatorPhone =
                        entity.DeuCoordinatorPhone,

                    DeuCoordinatorEmail =
                        entity.DeuCoordinatorEmail,

                    // -------------------------------------------------
                    // DEU Activities
                    // -------------------------------------------------

                    DeuActivitiesLastAcademicYear =
                        entity.DeuActivitiesLastAcademicYear,

                    // -------------------------------------------------
                    // DEU Members List File
                    // -------------------------------------------------

                    HasDeuMembersListFile =
                        !string.IsNullOrWhiteSpace(
                            entity.DeuMembersListFilePath)
                }
            };

            return vm;
        }
    }
}