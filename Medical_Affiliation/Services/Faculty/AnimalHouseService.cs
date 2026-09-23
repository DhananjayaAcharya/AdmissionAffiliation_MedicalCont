using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class AnimalHouseService : IAnimalHouseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnimalHouseService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<AnimalHouseDetail>> GetAnimalHouseDetails()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // ==========================================
            // GET CURRENT USER / SESSION DETAILS
            // ==========================================

            var collegeCode =
                httpContext?.Session.GetString("CollegeCode")
                ?? _userContext.CollegeCode;

            var facultyId = _userContext.FacultyId;

            var affiliationTypeId = _userContext.TypeOfAffiliation;

            var courseLevel =
                httpContext?.Session.GetString("CourseLevel")
                ?? httpContext?.Session.GetString("SelectedCourseLevel")
                ?? _userContext.CourseLevel;

            courseLevel = courseLevel?.Trim().ToUpperInvariant();


            // ==========================================
            // VALIDATION
            // ==========================================

            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                return new List<AnimalHouseDetail>();
            }

            if (string.IsNullOrWhiteSpace(courseLevel))
            {
                return new List<AnimalHouseDetail>();
            }


            // ==========================================
            // GET ANIMAL HOUSE DETAILS
            // ==========================================
           
            var details = await _context.AnimalHouseDetails
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId &&
                    x.TypeId == affiliationTypeId &&
                    x.CourseLevel != null &&
                    x.CourseLevel.ToUpper() == courseLevel
                )
                .OrderBy(x => x.AnimalHouseDetailsId)
                .ToListAsync();

            return details;
        }
    }
}