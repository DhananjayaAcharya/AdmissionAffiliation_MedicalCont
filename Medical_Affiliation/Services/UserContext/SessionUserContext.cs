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

        public string CourseLevel =>
            User?.FindFirst("CourseLevel")?.Value
            ?? throw new UnauthorizedAccessException("CourseLevel missing");

        public int FacultyId =>
            int.TryParse(User?.FindFirst("FacultyCode")?.Value, out var f) ? f : throw new UnauthorizedAccessException("FacultyCode missing");


        public string SeatSlabId => User?.FindFirst("SeatSlabId")?.Value ?? "S01";

        public int TypeOfAffiliation =>int.TryParse(User?.FindFirst("TypeOfAffiliation")?.Value, out var t) ? t : 2;
    }

}
