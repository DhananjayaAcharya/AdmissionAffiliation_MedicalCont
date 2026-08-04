using Medical_Affiliation.DATA;
using Medical_Affiliation.Services.Interfaces;

namespace Medical_Affiliation.Services.Faculty
{
    public abstract class CAPreviewServiceBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IUserContext _userContext;

        protected CAPreviewServiceBase(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        protected string CollegeCode => _userContext.CollegeCode;

        protected int FacultyCode => _userContext.FacultyId;

        protected string CourseLevel => _userContext.CourseLevel;

        protected bool IsMedical => FacultyCode == 1;

        protected bool IsDental => FacultyCode == 2;

        /// <summary>
        /// Returns the seat slab by rounding up to the nearest multiple of 50.
        /// Example:
        /// 50 -> 50
        /// 75 -> 100
        /// 125 -> 150
        /// 200 -> 200
        /// </summary>
        protected int GetSeatSlab(int totalIntake)
        {
            return totalIntake > 0
                ? ((totalIntake - 1) / 50 + 1) * 50
                : 0;
        }
    }
}