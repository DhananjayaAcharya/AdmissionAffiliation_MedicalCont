using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAPaymentService: ICAPaymentService
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAPaymentService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<AffiliationPaymentViewModel> GetPaymentDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var affiliationTypeId = _userContext.TypeOfAffiliation;

            var payment = await _context.AffiliationPayments
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.AffiliationTypeId == affiliationTypeId &&
                    x.IsActive)
                .Select(x => new AffiliationPaymentViewModel
                {
                    Id = x.Id,
                    CollegeCode = x.CollegeCode,
                    FacultyCode = x.FacultyCode,
                    AffiliationTypeId = x.AffiliationTypeId,
                    PaymentDate = x.PaymentDate,
                    Amount = x.Amount,
                    TransactionReferenceNo = x.TransactionReferenceNo,
                    SupportingDocument = x.SupportingDocument
                })
                .FirstOrDefaultAsync();

            return payment ?? new AffiliationPaymentViewModel();
        }
    }
}
