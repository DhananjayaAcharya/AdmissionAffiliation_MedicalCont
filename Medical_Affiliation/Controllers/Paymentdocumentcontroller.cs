using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
    public class PaymentDocumentController : BaseController
    {
        private readonly IUserContext _userContext;
        private readonly ApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IPrincipalContactLookupService _principalContactLookupService;
        private readonly IPaymentReceiptPdfService _receiptPdfService;
        private readonly IConfiguration _configuration;

        public PaymentDocumentController(
            IUserContext userContext,
            ApplicationDbContext context,
            IWhatsAppService whatsAppService,
            IPrincipalContactLookupService principalContactLookupService,
            IPaymentReceiptPdfService receiptPdfService,
            IConfiguration configuration) : base(context)
        {
            _userContext = userContext;
            _context = context;
            _whatsAppService = whatsAppService;
            _principalContactLookupService = principalContactLookupService;
            _receiptPdfService = receiptPdfService;
            _configuration = configuration;
        }

        // Reuses BaseController.SaveFileAndReturnPath, which already picks E:\ vs D:\
        // and the Medical/Dental root based on FacultyCode — same convention as every
        // other document upload in this app. Screenshots are grouped under
        // "PaymentDocuments\{CollegeCode}" and given a GUID filename automatically.
        private async Task<(string? filePath, string? fileName)> SavePaymentScreenshotAsync(
            IFormFile? file,
            string collegeCode)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            string subFolder = Path.Combine("PaymentDocuments", collegeCode);
            string? fullPath = await SaveFileAndReturnPath(file, subFolder);

            return (fullPath, file.FileName);
        }

        private void ValidateFileSize(IFormFile file, string fieldName, double maxMb = 2)
        {
            if (file == null)
                return; // No file uploaded, skip validation

            if (file.Length <= 0)
            {
                ModelState.AddModelError(fieldName, "File cannot be empty.");
                return;
            }

            var sizeInMb = file.Length / (1024.0 * 1024.0);
            if (sizeInMb > maxMb)
            {
                ModelState.AddModelError(fieldName, $"File size must not exceed {maxMb} MB.");
            }
        }

        private void ValidatePaymentDocument(PaymentDocumentPostVM model, PaymentAffiliationDocument? existing)
        {
            if (model == null)
            {
                ModelState.AddModelError("Model", "Payment details are required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(model.TransactionId))
                ModelState.AddModelError("TransactionId", "Transaction ID is required.");

            // Screenshot required only on first submission — on update, existing file can be kept
            if (existing == null && (model.ScreenshotFile == null || model.ScreenshotFile.Length == 0))
            {
                ModelState.AddModelError("ScreenshotFile", "Payment screenshot is required.");
            }

            if (model.ScreenshotFile != null && model.ScreenshotFile.Length > 0)
            {
                ValidateFileSize(model.ScreenshotFile, nameof(model.ScreenshotFile), maxMb: 2);

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var ext = Path.GetExtension(model.ScreenshotFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ScreenshotFile", "Only JPG, PNG, or PDF files are allowed.");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentDocument(
            string collegeCode,
            int facultyCode,
            string courseLevel,
            int affiliationTypeId = 0)
        {
            try
            {
                Console.WriteLine($"[GetPaymentDocument] Called with collegeCode={collegeCode}, facultyCode={facultyCode}, courseLevel={courseLevel}");

                var resolvedAffiliationTypeId = ResolveAffiliationTypeId(affiliationTypeId);

                Console.WriteLine($"[GetPaymentDocument] Resolved affiliationTypeId={resolvedAffiliationTypeId}");

                if (resolvedAffiliationTypeId == null || resolvedAffiliationTypeId == 0)
                {
                    Console.WriteLine($"[GetPaymentDocument] Returning BadRequest: affiliationTypeId is null or 0");
                    return BadRequest(new { success = false, message = "Affiliation type not found in session. Please restart the workflow." });
                }

                var record = await _context.PaymentAffiliationDocuments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.CollegeCode == collegeCode &&
                        p.FacultyCode == facultyCode &&
                        p.CourseLevel == courseLevel &&
                        p.AffiliationTypeId == resolvedAffiliationTypeId);

                Console.WriteLine($"[GetPaymentDocument] Query result: Record found={record != null}");

                if (record == null)
                {
                    Console.WriteLine($"[GetPaymentDocument] No payment document found, returning empty data");
                    return Json(new
                    {
                        success = true,
                        data = (object?)null
                    });
                }

                Console.WriteLine($"[GetPaymentDocument] Returning payment document data");
                return Json(new
                {
                    success = true,
                    data = new PaymentDocumentViewVM
                    {
                    PaymentDocumentId = record.PaymentDocumentId,
                    CollegeCode = record.CollegeCode,
                    FacultyCode = record.FacultyCode,
                    CourseLevel = record.CourseLevel,
                    AffiliationTypeId = record.AffiliationTypeId,
                    TransactionId = record.TransactionId,
                    PaymentAmount = record.PaymentAmount,
                    PaymentDate = record.PaymentDate,
                    ScreenshotFileName = record.ScreenshotFileName,
                    HasScreenshot = !string.IsNullOrEmpty(record.ScreenshotFilePath)
                }
            });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetPaymentDocument] ❌ EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[GetPaymentDocument] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error loading payment document: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePaymentDocument([FromForm] PaymentDocumentPostVM model)
        {
            try
            {
                var affiliationTypeId = ResolveAffiliationTypeId(model?.AffiliationTypeId ?? 0);

                if (affiliationTypeId == null || affiliationTypeId == 0)
                {
                    return BadRequest(new { success = false, message = "Affiliation type not found in session. Please restart the workflow." });
                }

                model.AffiliationTypeId = affiliationTypeId.Value;
                HttpContext.Session.SetString("AffiliationTypeId", affiliationTypeId.Value.ToString());

                var existing = await _context.PaymentAffiliationDocuments
                    .FirstOrDefaultAsync(p =>
                        p.CollegeCode == model.CollegeCode &&
                        p.FacultyCode == model.FacultyCode &&
                        p.CourseLevel == model.CourseLevel &&
                        p.AffiliationTypeId == model.AffiliationTypeId);

                ValidatePaymentDocument(model, existing);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .Select(ms => new
                        {
                            Field = ms.Key,
                            Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        }).ToArray();

                    return BadRequest(new { errors });
                }

                string? filePath = null;
                string? fileName = null;

                if (model.ScreenshotFile != null && model.ScreenshotFile.Length > 0)
                {
                    var (savedPath, savedName) = await SavePaymentScreenshotAsync(model.ScreenshotFile, model.CollegeCode);
                    filePath = savedPath;
                    fileName = savedName;
                }

                if (existing != null)
                {
                    existing.TransactionId = model.TransactionId;
                    existing.PaymentAmount = model.PaymentAmount;
                    existing.PaymentDate = model.PaymentDate;
                    existing.UpdatedOn = DateTime.Now;

                    if (filePath != null)
                    {
                        // delete old file before replacing
                        if (!string.IsNullOrEmpty(existing.ScreenshotFilePath) &&
                            System.IO.File.Exists(existing.ScreenshotFilePath))
                        {
                            System.IO.File.Delete(existing.ScreenshotFilePath);
                        }

                        existing.ScreenshotFilePath = filePath;
                        existing.ScreenshotFileName = fileName;
                    }
                }
                else
                {
                    _context.PaymentAffiliationDocuments.Add(new PaymentAffiliationDocument
                    {
                        CollegeCode = model.CollegeCode,
                        FacultyCode = model.FacultyCode,
                        CourseLevel = model.CourseLevel,
                        AffiliationTypeId = model.AffiliationTypeId,
                        TransactionId = model.TransactionId,
                        PaymentAmount = model.PaymentAmount,
                        PaymentDate = model.PaymentDate,
                        ScreenshotFilePath = filePath,
                        ScreenshotFileName = fileName,
                        CreatedOn = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                var savedRecord = existing ?? await _context.PaymentAffiliationDocuments
                    .FirstOrDefaultAsync(p =>
                        p.CollegeCode == model.CollegeCode &&
                        p.FacultyCode == model.FacultyCode &&
                        p.CourseLevel == model.CourseLevel &&
                        p.AffiliationTypeId == model.AffiliationTypeId);

                bool whatsAppSent = false;
                string? whatsAppMessage = null;

                try
                {
                    (whatsAppSent, whatsAppMessage) = await SendPaymentDetailsToWhatsAppAsync(model, savedRecord);
                }
                catch (Exception whatsAppEx)
                {
                    whatsAppMessage = whatsAppEx.Message;
                }

                return Ok(new
                {
                    success = true,
                    message = "Payment details saved successfully",
                    whatsAppSent,
                    whatsAppMessage,
                    whatsAppStatus = whatsAppSent
                        ? "WhatsApp template sent successfully."
                        : $"WhatsApp was not sent: {whatsAppMessage ?? "Unknown error"}"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private async Task<(bool sent, string? message)> SendPaymentDetailsToWhatsAppAsync(
    PaymentDocumentPostVM model,
    PaymentAffiliationDocument? savedRecord)
        {
            if (savedRecord == null)
            {
                Console.WriteLine($"[WhatsApp] Payment record not found for save ID: {model.CollegeCode}");
                return (false, "Payment record not found after save.");
            }

            // Get college code from session first, then fall back to model
            string? collegeCodeToUse = HttpContext.Session.GetString("CollegeCode")
                ?? model?.CollegeCode;

            collegeCodeToUse = collegeCodeToUse?.Trim();

            Console.WriteLine($"[WhatsApp] ===== COLLEGE CODE RESOLUTION =====");
            Console.WriteLine($"[WhatsApp] CollegeCode from session: '{HttpContext.Session.GetString("CollegeCode")}'");
            Console.WriteLine($"[WhatsApp] CollegeCode from model: '{model?.CollegeCode}'");
            Console.WriteLine($"[WhatsApp] CollegeCode to use: '{collegeCodeToUse}'");
            Console.WriteLine($"[WhatsApp] CollegeCode is null? {collegeCodeToUse == null}");
            Console.WriteLine($"[WhatsApp] CollegeCode is empty? {string.IsNullOrWhiteSpace(collegeCodeToUse)}");
            Console.WriteLine($"[WhatsApp] ===== END COLLEGE CODE RESOLUTION =====");

            if (string.IsNullOrWhiteSpace(collegeCodeToUse))
            {
                Console.WriteLine($"[WhatsApp] BLOCKED: CollegeCode is null/empty - cannot look up principal contact");
                return (false, "College code is missing. Cannot determine principal contact.");
            }

            Console.WriteLine($"[WhatsApp] ===== PRINCIPAL LOOKUP START =====");
            var (principalMobile, nameOfInstitution) =
                await _principalContactLookupService.GetPrincipalContactAsync(collegeCodeToUse);

            principalMobile = principalMobile?.Trim();
            nameOfInstitution = nameOfInstitution?.Trim();

            Console.WriteLine($"[WhatsApp] College Code: '{collegeCodeToUse}'");
            Console.WriteLine($"[WhatsApp] Principal Mobile (raw from DB): '{principalMobile}'");
            Console.WriteLine($"[WhatsApp] Institution Name: '{nameOfInstitution}'");
            Console.WriteLine($"[WhatsApp] Mobile is null? {principalMobile == null}");
            Console.WriteLine($"[WhatsApp] Mobile is empty? {string.IsNullOrWhiteSpace(principalMobile)}");
            Console.WriteLine($"[WhatsApp] ===== PRINCIPAL LOOKUP END =====");

            if (string.IsNullOrWhiteSpace(principalMobile))
            {
                Console.WriteLine($"[WhatsApp] BLOCKED: Cannot send because principal mobile number is null/empty for college {collegeCodeToUse}");
                return (false, "Principal mobile number not found for this college. Please ensure Principal_Mob_No is populated in AFF_InstitutionsDetails table.");
            }

            Console.WriteLine($"[WhatsApp] ✅ VERIFIED: Principal mobile number is present and ready to send: {principalMobile}");
            var paymentMessage = $"Your payment has been successfully completed.\n\nTransaction ID: {savedRecord.TransactionId}\nAmount Paid: {(savedRecord.PaymentAmount.HasValue ? $"INR {savedRecord.PaymentAmount.Value:0.00}" : "Not provided")}\nDate & Time: {(savedRecord.PaymentDate.HasValue ? savedRecord.PaymentDate.Value.ToString("dd-MM-yyyy HH:mm") : "Not provided")}\n\nThank you for your payment. Your transaction has been recorded successfully.";

            var messageResult = await _whatsAppService.SendMessageAsync(
                phoneNumber: principalMobile,
                message: paymentMessage);

            Console.WriteLine($"[WhatsApp] Payment message send result: Success={messageResult.Success}, Message={messageResult.Message ?? "OK"}");
            return (messageResult.Success, messageResult.Message ?? "Payment message sent successfully.");
        }

        public async Task<IActionResult> ViewPaymentDocument(int paymentDocumentId, string mode = "view")
        {
            var doc = await _context.PaymentAffiliationDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PaymentDocumentId == paymentDocumentId);

            if (doc == null ||
                string.IsNullOrEmpty(doc.ScreenshotFilePath) ||
                !System.IO.File.Exists(doc.ScreenshotFilePath))
                return NotFound("File not found");

            var fileName = Path.GetFileName(doc.ScreenshotFilePath);

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(doc.ScreenshotFilePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            if (mode == "download")
            {
                return PhysicalFile(doc.ScreenshotFilePath, contentType, fileName);
            }

            return PhysicalFile(doc.ScreenshotFilePath, contentType);
        }
    }
}