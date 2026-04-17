using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADeclarationService: ICADeclarationService
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADeclarationService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<AffiliationFinalDeclarationViewModel> GetDeclarationDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var affiliationTypeId = _userContext.TypeOfAffiliation;

            var declaration = await _context.AffiliationFinalDeclarations
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.AffiliationTypeId == affiliationTypeId)
                .Select(x => new AffiliationFinalDeclarationViewModel
                {
                    Id = x.Id,
                    PrincipalName = x.PrincipalName,
                    IsSubmitted = x.IsSubmitted
                })
                .FirstOrDefaultAsync();

            // 👉 If not exists, return empty model
            return declaration ?? new AffiliationFinalDeclarationViewModel();
        }
    }
}
