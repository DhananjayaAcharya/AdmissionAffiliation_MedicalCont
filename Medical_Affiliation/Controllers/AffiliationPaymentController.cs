using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.UserContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AffiliationPaymentController : Controller
    {
        public readonly ApplicationDbContext _context;
        public readonly SessionUserContext _userContext;

        [HttpGet]
        public async Task<IActionResult> AffiliationPayment()
        {
            var collegeCode = _userContext.CollegeCode;
            int facultyCode = _userContext.FacultyId;
            var affiliationTypeId = _userContext.TypeOfAffiliation;
            var data = await _context.AffiliationPayments
                .Where(x => x.CollegeCode == collegeCode &&
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
                    SupportingDocument = x.SupportingDocument,
                    RegistrationNumber = x.RegistrationNumber
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> SavePayment(AffiliationPaymentViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            // 🔍 Check duplicate transaction
            var duplicate = await _context.AffiliationPayments
                .FirstOrDefaultAsync(x => x.TransactionReferenceNo == model.TransactionReferenceNo
                                         && x.Id != model.Id);

            if (duplicate != null)
                return BadRequest("Transaction Reference already exists");

            AffiliationPayment entity;

            if (model.Id > 0)
            {
                // 🔁 UPDATE
                entity = await _context.AffiliationPayments.FindAsync(model.Id);

                if (entity == null)
                    return NotFound("Payment not found");

                entity.PaymentDate = model.PaymentDate;
                entity.Amount = model.Amount;
                entity.TransactionReferenceNo = model.TransactionReferenceNo;
                entity.SupportingDocument = model.SupportingDocument;
                entity.RegistrationNumber = model.RegistrationNumber;
            }
            else
            {
                // ➕ INSERT
                entity = new AffiliationPayment
                {
                    CollegeCode = model.CollegeCode,
                    FacultyCode = model.FacultyCode,
                    AffiliationTypeId = model.AffiliationTypeId,
                    PaymentDate = model.PaymentDate,
                    Amount = model.Amount,
                    TransactionReferenceNo = model.TransactionReferenceNo,
                    SupportingDocument = model.SupportingDocument,
                    RegistrationNumber = model.RegistrationNumber,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.AffiliationPayments.Add(entity);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment saved successfully" });
        }
    }
}
