using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalBedDistributionService : ICADentalBedDistributionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADentalBedDistributionService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<CADentalBedDistributionPreviewViewModel?>
            GetDentalBedDistributionAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var affiliationTypeId = _userContext.TypeOfAffiliation;
            var courseLevel = _userContext.CourseLevel;

            if (string.IsNullOrWhiteSpace(collegeCode))
                return null;

            var existing = await _context.MedicalUgbedDistributions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString() &&
                    x.AffiliationTypeId == affiliationTypeId &&
                    x.CourseLevel == courseLevel
                );

            if (existing == null)
                return null;

            return new CADentalBedDistributionPreviewViewModel
            {
                Id = existing.Id,
                OralMaxillofacialSurgery =
                    existing.OralMaxillofacialSurgery
            };
        }
    }
}