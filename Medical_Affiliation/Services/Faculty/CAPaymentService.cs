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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CAPaymentService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AffiliationPaymentViewModel> GetPaymentDetails()
        {
            var collegeCode = _httpContextAccessor.HttpContext?.Session.GetString("CollegeCode")
                              ?? _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var affiliationTypeId = _userContext.TypeOfAffiliation;
            var courseLevel = (_httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                               ?? _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel")
                               ?? _userContext.CourseLevel)
                .Trim()
                .ToUpperInvariant();

            var payment = await _context.PaymentAffiliationDocuments
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.AffiliationTypeId == affiliationTypeId &&
                    x.CourseLevel.ToUpper() == courseLevel)
                .OrderByDescending(x => x.UpdatedOn ?? x.CreatedOn)
                .Select(x => new AffiliationPaymentViewModel
                {
                    Id = x.PaymentDocumentId,
                    CollegeCode = x.CollegeCode,
                    FacultyCode = x.FacultyCode,
                    AffiliationTypeId = x.AffiliationTypeId,
                    PaymentDate = x.PaymentDate ?? default,
                    Amount = x.PaymentAmount ?? 0,
                    TransactionReferenceNo = x.TransactionId,
                    SupportingDocument = x.ScreenshotFilePath ?? x.ScreenshotFileName
                })
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                payment = await _context.PaymentAffiliationDocuments
                    .AsNoTracking()
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                x.CourseLevel.ToUpper() == courseLevel)
                    .OrderByDescending(x => x.UpdatedOn ?? x.CreatedOn)
                    .Select(x => new AffiliationPaymentViewModel
                    {
                        Id = x.PaymentDocumentId,
                        CollegeCode = x.CollegeCode,
                        FacultyCode = x.FacultyCode,
                        AffiliationTypeId = x.AffiliationTypeId,
                        PaymentDate = x.PaymentDate ?? default,
                        Amount = x.PaymentAmount ?? 0,
                        TransactionReferenceNo = x.TransactionId,
                        SupportingDocument = x.ScreenshotFilePath ?? x.ScreenshotFileName
                    })
                    .FirstOrDefaultAsync();
            }

            return payment ?? new AffiliationPaymentViewModel();
        }
    }
}
