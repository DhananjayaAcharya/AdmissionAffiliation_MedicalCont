using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalFieldPracticeAreaService : ICADentalFieldPracticeAreaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CADentalFieldPracticeAreaService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<DentalFieldPracticeAreaViewModel>> GetFieldPracticeAreaAsync()
        {
            // =====================================================
            // CURRENT USER DETAILS
            // =====================================================

            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var affiliationTypeId = _userContext.TypeOfAffiliation;

            // This service is only for Dental
            if (facultyCode != 2)
            {
                return new List<DentalFieldPracticeAreaViewModel>();
            }

            // =====================================================
            // COURSE LEVEL
            // =====================================================

            var courseLevel =
                _httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                ?? _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel")
                ?? _userContext.CourseLevel;

            courseLevel = courseLevel?
                .Trim()
                .ToUpperInvariant();

            // =====================================================
            // VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                return new List<DentalFieldPracticeAreaViewModel>();
            }

            // =====================================================
            // GET FIELD PRACTICE AREA DETAILS
            // =====================================================

            var details = await _context.DentalFieldPracticeAreas
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyCode &&
                    x.TypeId == affiliationTypeId &&
                    x.CourseLevel != null &&
                    x.CourseLevel.Trim().ToUpper() == courseLevel)
                .OrderBy(x => x.DentalFieldPracticeAreaId)
                .Select(x => new DentalFieldPracticeAreaViewModel
                {
                    // =================================================
                    // PRIMARY KEY
                    // =================================================

                    DentalFieldPracticeAreaId =
                        x.DentalFieldPracticeAreaId,

                    // =================================================
                    // SESSION / REFERENCE DETAILS
                    // =================================================

                    FacultyId = x.FacultyId,

                    CollegeCode = x.CollegeCode,

                    TypeId = x.TypeId,

                    CourseLevel = x.CourseLevel,

                    // =================================================
                    // FIELD TYPE
                    // =================================================

                    FieldTypeId = x.FieldTypeId,

                    // =================================================
                    // LOCATION AND ADDRESS
                    // =================================================

                    Location = x.Location,

                    Address = x.Address,

                    // =================================================
                    // MANAGED BY
                    // =================================================

                    ManagedBy = x.ManagedBy,

                    // =================================================
                    // STAFF
                    // =================================================

                    StaffList = x.StaffList,

                    // =================================================
                    // POPULATION SERVED
                    // =================================================

                    PopulationServed = x.PopulationServed,

                    // =================================================
                    // ACTIVITIES AND SERVICES
                    // =================================================

                    ActivitiesAndServices =
                        x.ActivitiesAndServices,

                    // =================================================
                    // RECORDS MAINTAINED
                    // =================================================

                    RecordsMaintained =
                        x.RecordsMaintained,

                    // =================================================
                    // EQUIPMENTS AVAILABLE
                    // =================================================

                    EquipmentsAvailable =
                        x.EquipmentsAvailable,

                    // =================================================
                    // TRAINING ACTIVITIES
                    // =================================================

                    TrainingActivities =
                        x.TrainingActivities,

                    // =================================================
                    // SUPERVISION METHOD
                    // =================================================

                    SupervisionMethod =
                        x.SupervisionMethod,

                    // =================================================
                    // TRAINEE / SUPERVISOR ACCOMMODATION
                    // =================================================

                    TraineeSupervisorAccommodation =
                        x.TraineeSupervisorAccommodation
                })
                .ToListAsync();

            // =====================================================
            // GET FIELD TYPES
            // =====================================================

            var fieldTypes = await _context.MstFieldTypeChps
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.FieldType)
                .Select(x => new DropdownItem
                {
                    Value = x.Id.ToString(),
                    Text = x.FieldType
                })
                .ToListAsync();

            // =====================================================
            // ASSIGN FIELD TYPES TO EACH RECORD
            // =====================================================

            foreach (var item in details)
            {
                item.FieldTypes = fieldTypes;
            }

            return details;
        }
    }
}