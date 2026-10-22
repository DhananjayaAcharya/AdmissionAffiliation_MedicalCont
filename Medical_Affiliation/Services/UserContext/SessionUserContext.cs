using Medical_Affiliation.Services.Interfaces;
using System.Security.Claims;

namespace Medical_Affiliation.Services.UserContext
{
    public class SessionUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionUserContext(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

        public string CollegeCode =>
            User?.FindFirst("CollegeCode")?.Value
            ?? throw new UnauthorizedAccessException("CollegeCode missing");

        public string CourseLevel
        {
            get
            {
                // Ayurveda (FacultyCode 4): level follows the UG/PG button the user clicked
                if (int.TryParse(User?.FindFirst("FacultyCode")?.Value, out var faculty) && faculty == 4)
                {
                    var ctx = _httpContextAccessor.HttpContext;

                    var level = ctx?.Session.GetString("SelectedCourseLevel");
                    if (string.IsNullOrWhiteSpace(level))
                        level = ctx?.Session.GetString("CourseLevel");
                    if (string.IsNullOrWhiteSpace(level))
                        level = User?.FindFirst("CourseLevel")?.Value;

                    if (string.IsNullOrWhiteSpace(level))
                        throw new UnauthorizedAccessException("CourseLevel missing");

                    return level.Trim().ToUpperInvariant();
                }

                // Medical / Dental / others: original behaviour, untouched
                return User?.FindFirst("CourseLevel")?.Value
                    ?? throw new UnauthorizedAccessException("CourseLevel missing");
            }
        }

        public int FacultyId =>
            int.TryParse(User?.FindFirst("FacultyCode")?.Value, out var f) ? f : throw new UnauthorizedAccessException("FacultyCode missing");

        public string SeatSlabId => User?.FindFirst("SeatSlabId")?.Value ?? "S01";

        public int TypeOfAffiliation
        {
            get
            {
                var sessionValue = _httpContextAccessor.HttpContext?.Session.GetInt32("AffiliationType");
                if (sessionValue.HasValue)
                {
                    return sessionValue.Value;
                }

                var sessionStringValue = _httpContextAccessor.HttpContext?.Session.GetString("TypeOfAffiliationId");
                if (int.TryParse(sessionStringValue, out var sessionType))
                {
                    return sessionType;
                }

                return int.TryParse(User?.FindFirst("TypeOfAffiliation")?.Value, out var claimType) ? claimType : 2;
            }
        }
    }
}