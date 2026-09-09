using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalChairDistributionPreviewService : ICADentalChairDistributionPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADentalChairDistributionPreviewService( ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<List<DentalChairVm>> GetDentalChairDistributionPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            return await _context.DentalChairs
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .OrderBy(x => x.CourseLevel)
                .ThenBy(x => x.CourseName)
                .Select(x => new DentalChairVm
                {
                    CourseCode = x.CourseCode,
                    CourseName = x.CourseName ?? string.Empty,
                    CourseLevel = x.CourseLevel ?? string.Empty,
                    SeatSlab = x.SeatSlab,
                    SeatSlabId = x.SeatSlabId,
                    ChairsRequired = x.ChairsRequired ?? 0,
                    ChairsExisting = x.ChairsExisting ?? 0
                })
                .ToListAsync();
        }
    }
}
