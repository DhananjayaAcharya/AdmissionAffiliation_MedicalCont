using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class WorkShopDetailsService : IWorkShopDetailsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkShopDetailsService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<WorkShopDetail>> GetWorkshopDetailsAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var collegeCode =
                httpContext?.Session.GetString("CollegeCode")
                ?? _userContext.CollegeCode;

            var facultyCode =
                _userContext.FacultyId;

            var affiliationTypeId =
                _userContext.TypeOfAffiliation;

            var courseLevel =
                httpContext?.Session.GetString("CourseLevel")
                ?? httpContext?.Session.GetString("SelectedCourseLevel")
                ?? _userContext.CourseLevel;

            courseLevel = courseLevel?
                .Trim()
                .ToUpperInvariant();

            var workshopDetails = await _context.WorkShopDetails
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyCode &&
                    x.TypeId == affiliationTypeId &&
                    x.CourseLevel != null &&
                    x.CourseLevel.ToUpper() == courseLevel)
                .OrderBy(x => x.WorkShopDetailsId)
                .ToListAsync();

            return workshopDetails;
        }
    }
}