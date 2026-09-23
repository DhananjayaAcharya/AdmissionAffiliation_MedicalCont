using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class DentalPaymentService : ICADentalPaymentService
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DentalPaymentService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DentalFeeStructureViewModel> GetDentalPaymentDetails()
        {
            // =====================================================
            // SESSION / USER DETAILS
            // =====================================================

            var httpContext = _httpContextAccessor.HttpContext;

            var collegeCode =
                httpContext?.Session.GetString("CollegeCode")
                ?? _userContext.CollegeCode;

            var facultyCode = _userContext.FacultyId;

            var affiliationTypeId = _userContext.TypeOfAffiliation;

            var courseLevel =
                httpContext?.Session.GetString("CourseLevel")
                ?? httpContext?.Session.GetString("SelectedCourseLevel")
                ?? _userContext.CourseLevel;

            courseLevel = courseLevel?.Trim().ToUpperInvariant();


            // =====================================================
            // RETURN EMPTY MODEL IF REQUIRED DETAILS ARE MISSING
            // =====================================================

            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                return new DentalFeeStructureViewModel
                {
                    CollegeCode = collegeCode ?? string.Empty,
                    FacultyCode = facultyCode,
                    AffiliationTypeId = affiliationTypeId
                };
            }

            if (string.IsNullOrWhiteSpace(courseLevel))
            {
                return new DentalFeeStructureViewModel
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    AffiliationTypeId = affiliationTypeId
                };
            }


            // =====================================================
            // GET DENTAL PAYMENT
            // =====================================================

            var payment = await _context.TxnDentalPayments
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.AffiliationTypeId == affiliationTypeId &&
                    x.CourseLevel != null &&
                    x.CourseLevel.ToUpper() == courseLevel &&
                    x.IsActive
                )
                .OrderByDescending(x => x.ModifiedDate ?? x.CreatedDate)
                .FirstOrDefaultAsync();


            // =====================================================
            // FALLBACK
            //
            // If CourseLevel was not stored/matched, get the
            // latest active payment for the same college,
            // faculty and affiliation type.
            // =====================================================

            if (payment == null)
            {
                payment = await _context.TxnDentalPayments
                    .AsNoTracking()
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.AffiliationTypeId == affiliationTypeId &&
                        x.IsActive
                    )
                    .OrderByDescending(x => x.ModifiedDate ?? x.CreatedDate)
                    .FirstOrDefaultAsync();
            }


            // =====================================================
            // CREATE DENTAL PAYMENT VIEW MODEL
            // =====================================================

            var model = new DentalFeeStructureViewModel
            {
                CollegeCode = collegeCode,

                FacultyCode = facultyCode,

                AffiliationTypeId = affiliationTypeId,

                PaymentId = payment?.Id,

                TransactionId = payment?.TransactionId,

                TransactionReceiptPath =
                    payment?.TransactionReceiptPath,

                AmountPaid =
                    payment?.AmountPaid ?? 0m
            };


            // =====================================================
            // RETURN
            // =====================================================

            return model;
        }
    }
}
