using Medical_Affiliation.Services.Interfaces;

namespace Medical_Affiliation.Services.UserContext
{
    public class SessionUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionUserContext(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public string CollegeCode
        {
            get
            {
                var code = _httpContextAccessor.HttpContext?
                    .Session.GetString("CollegeCode") ?? "M001";

                if (string.IsNullOrEmpty(code))
                    throw new UnauthorizedAccessException();

                return code;
            }
        }

        public string CourseLevel
        {
            get
            {
                var code = _httpContextAccessor.HttpContext?
                    .Session.GetString("CourseLevel") ;

                if (string.IsNullOrEmpty(code))
                    throw new UnauthorizedAccessException();

                return code;
            }
        }

        public int FacultyId
        {
            get
            {
                var faculty = _httpContextAccessor.HttpContext?
                    .Session.GetString("FacultyCode");

                return int.TryParse(faculty, out var f) ? f : 1;
            }
        }

        public string SeatSlabId
        {
            get
            {
                var SeatSlabId = _httpContextAccessor.HttpContext?
                    .Session.GetString("SeatSlabId");

                return SeatSlabId ?? "S01";
            }
        }

        public int TypeOfAffiliation
        {
            get
            {
                var TypeOfAffiliation = _httpContextAccessor.HttpContext?
                    .Session.GetString("TypeOfAffiliation");
                return int.TryParse(TypeOfAffiliation, out var f) ? f: 2;
            }
        }
    }

}
