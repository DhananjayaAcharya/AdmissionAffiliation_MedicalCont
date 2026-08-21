using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CATrustMemberDetailsPreviewService : ICATrustMemberDetailsPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CATrustMemberDetailsPreviewService( ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<CATrustMemberDetailsDisplayVM?> GetTrustMemberDetailsAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            var members = await _context.ContinuationTrustMemberDetails
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .Select(x => new CATrustMemberDisplayRowVM
                {
                    SlNo = x.SlNo,
                    TrustMemberName = x.TrustMemberName,
                    Designation = x.Designation,
                    Qualification = x.Qualification,
                    MobileNumber = x.MobileNumber,
                    Age = x.Age,
                    JoiningDate = x.JoiningDate
                })
                .ToListAsync();

            // Registered Trust Member Details PDF is stored
            // in InstitutionBasicDetails
            var institution = await _context.InstitutionBasicDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            return new CATrustMemberDetailsDisplayVM
            {
                Members = members,

                HasRegisteredTrustMemberDetails =
                    institution != null &&
                    !string.IsNullOrWhiteSpace(
                        institution.RegisteredTrustMemberDetailsPath)
            };
        }
    }
}