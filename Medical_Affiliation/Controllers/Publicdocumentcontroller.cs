using Medical_Affiliation.DATA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    // Deliberately inherits plain Controller, NOT BaseController — BaseController's
    // OnActionExecuting force-redirects unauthenticated requests to the login page
    // regardless of [AllowAnonymous], which would break WhatsApp's servers trying to
    // fetch these files. Access is instead gated by an unguessable random GUID token,
    // not by sequential IDs, so nothing here is enumerable.
    [AllowAnonymous]
    [Route("public-docs")]
    public class PublicDocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicDocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("receipt/{token:guid}")]
        public async Task<IActionResult> GetReceipt(Guid token)
        {
            var receipt = await _context.PaymentReceipts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PublicAccessToken == token);

            if (receipt == null || !System.IO.File.Exists(receipt.FilePath))
                return NotFound();

            return PhysicalFile(receipt.FilePath, "application/pdf", Path.GetFileName(receipt.FilePath));
        }

        [HttpGet("screenshot/{token:guid}")]
        public async Task<IActionResult> GetScreenshot(Guid token)
        {
            var doc = await _context.PaymentAffiliationDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.PublicAccessToken == token);

            if (doc == null ||
                string.IsNullOrEmpty(doc.ScreenshotFilePath) ||
                !System.IO.File.Exists(doc.ScreenshotFilePath))
                return NotFound();

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(doc.ScreenshotFilePath, out string contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(
                doc.ScreenshotFilePath,
                contentType,
                doc.ScreenshotFileName ?? Path.GetFileName(doc.ScreenshotFilePath));
        }
    }
}