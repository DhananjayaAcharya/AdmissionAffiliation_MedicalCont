using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Services.UserContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AffiliationPaymentController : Controller
    {
        public readonly ApplicationDbContext _context;
        public readonly IUserContext _userContext;

        public AffiliationPaymentController(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> Payment()
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
                    SupportingDocument = string.IsNullOrEmpty(x.SupportingDocument)
                                                ? null
                                                : Path.GetFileName(x.SupportingDocument),
                })
                .FirstOrDefaultAsync();

            return View(data ?? new AffiliationPaymentViewModel());
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
            string existingFilePath = null;

            if (model.Id > 0)
            {
                // 🔁 UPDATE
                entity = await _context.AffiliationPayments.FindAsync(model.Id);

                if (entity == null)
                    return NotFound("Payment not found");

                // ✅ store existing file path
                existingFilePath = entity.SupportingDocument;
            }
            else
            {
                // ➕ INSERT
                entity = new AffiliationPayment
                {
                    CollegeCode = _userContext.CollegeCode,
                    FacultyCode = _userContext.FacultyId,
                    AffiliationTypeId = _userContext.TypeOfAffiliation,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.AffiliationPayments.Add(entity);
            }

            // 🔁 Common fields
            entity.PaymentDate = model.PaymentDate;
            entity.Amount = model.Amount;
            entity.TransactionReferenceNo = model.TransactionReferenceNo;

            // 📁 FILE HANDLING
            if (model.File != null && model.File.Length > 0)
            {
                // ❌ Size validation
                if (model.File.Length > 1 * 1024 * 1024)
                {
                    TempData["Error"] = "File size must be less than 1MB";
                    return RedirectToAction("Payment");
                }

                // ❌ Extension validation
                var allowedExtensions = new[] { ".pdf", ".jpg", ".png" };
                var ext = Path.GetExtension(model.File.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Invalid file type";
                    return RedirectToAction("Payment");
                }

                // ✅ Folder path
                var basePath = @"D:\Affiliation_Medical";
                var folderPath = Path.Combine(basePath, "Payment");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 🔥 DELETE OLD FILE (only if exists)
                if (!string.IsNullOrEmpty(existingFilePath))
                {
                    var oldFullPath = Path.Combine(folderPath, existingFilePath);

                    if (System.IO.File.Exists(oldFullPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFullPath);
                        }
                        catch
                        {
                            // log if needed
                        }
                    }
                }

                // ✅ Save new file
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
                var fullPath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                entity.SupportingDocument = uniqueFileName;
            }
            else if (model.Id > 0)
            {
                // ✅ KEEP OLD FILE
                entity.SupportingDocument = existingFilePath;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment saved successfully";

            return RedirectToAction("Payment");
        }

        public IActionResult ViewFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var folderPath = @"D:\Affiliation_Medical\Payment";
            var fullPath = Path.Combine(folderPath, fileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            var contentType = GetContentType(fullPath);

            return File(fileBytes, contentType);
        }

        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLower();

            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
