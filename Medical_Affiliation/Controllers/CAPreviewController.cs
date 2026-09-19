using Medical_Affiliation.DATA;
using Medical_Affiliation.Services.Faculty;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;

namespace Medical_Affiliation.Controllers
{
    public class CAPreviewController : Controller
    {
        private readonly ICAPreviewService _capreviewService;
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly PaymentCalculationController _paymentCalculationController;
        public CAPreviewController(
            ApplicationDbContext context,
            ICAPreviewService capreviewService,
            PaymentCalculationController paymentCalculationController)
        {
            _context = context;
            _capreviewService = capreviewService;
            _paymentCalculationController = paymentCalculationController;
        }
        public async Task<IActionResult> Preview()
        {
            var model = await _capreviewService.GetPreviewAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePdf(bool declarationConsent)
        {
            var completion = await _capreviewService.GetPreviewAsync();
            if (!completion.IsApplicationComplete)
            {
                TempData["PreviewError"] = $"Complete the application sections before submitting. Current completion: {completion.CompletionPercentage}%.";
                return RedirectToAction(nameof(Preview));
            }

            if (!declarationConsent)
            {
                TempData["PreviewError"] = "Please accept the declaration before generating the preview PDF.";
                return RedirectToAction(nameof(Preview));
            }

            HttpContext.Session.SetString("CAApplicationReadOnly", "true");

            var model = await _capreviewService.GetPreviewAsync();
            _paymentCalculationController.ControllerContext = ControllerContext;
            model.PaymentCalculation = await _paymentCalculationController.GetCurrentCalculationAsync();
            return GeneratePreviewPdf(model);
        }

        public async Task<IActionResult> GetCurriculumFile(int id)
        {
            var file = await _context.CaCourseCurricula
                .AsNoTracking()
                .Where(x => x.CourseCurriculumId == id)
                .Select(x => new
                {
                    x.CurriculumPdfPath,
                    x.PdfFileName
                })
                .FirstOrDefaultAsync();

            if (file == null || file.CurriculumPdfPath == null)
                return NotFound();

            return PhysicalFile(file.CurriculumPdfPath, "application/pdf");
        }
        public async Task<IActionResult> PreviewHospitalDocument(int id, string mode = "view")
        {
            var file = await _context.AffiliatedHospitalDocuments
                .AsNoTracking()
                .Where(x => x.DocumentId == id)
                .Select(x => new
                {
                    x.DocumentName,
                    x.DocumentFilePth
                })
                .FirstOrDefaultAsync();

            if (file == null ||
                string.IsNullOrEmpty(file.DocumentFilePth) ||
                !System.IO.File.Exists(file.DocumentFilePth))
                return NotFound("File not found");

            var fileName = Path.GetFileName(file.DocumentFilePth);

            // 🔥 Detect content type dynamically
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.DocumentFilePth, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // 📥 Download mode
            if (mode == "download")
            {
                return PhysicalFile(file.DocumentFilePth, contentType, fileName);
            }

            // 👀 Preview mode (inline)
            return PhysicalFile(file.DocumentFilePth, contentType);
        }
        public async Task<IActionResult> ViewServiceFile(int id)
        {
            var file = await _context.CaMedicalLibraryServices
                .AsNoTracking()
                .Where(x => x.LibraryServiceId == id)
                .Select(x => new
                {
                    x.UploadedFileName,
                    x.UploadedPdfPath
                })
                .FirstOrDefaultAsync();

            if (file == null || file.UploadedPdfPath == null)
                return NotFound();

            return PhysicalFile(file.UploadedPdfPath, "application/pdf");
        }
        public async Task<IActionResult> ViewSpecialFeaturesPdf(int id)
        {
            var file = await _context.CaMedicalLibraryOtherDetails
                .AsNoTracking()
                .Where(x => x.DigitalValuationId == id)
                .Select(x => new
                {
                    x.UploadedFileName,
                    x.SpecialFeaturesAchievementsPdfPath
                })
                .FirstOrDefaultAsync();

            if (file == null || file.SpecialFeaturesAchievementsPdfPath == null)
                return NotFound();

            return File(file.SpecialFeaturesAchievementsPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewGoverningCouncilPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.GoverningCouncilPdfPath,
                    e.GoverningCouncilPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.GoverningCouncilPdfPath == null) return NotFound();

            return File(gov.GoverningCouncilPdfPath, "application/pdf");

        }

        public async Task<IActionResult> ViewAccountSummaryPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AccountSummaryPdfPath,
                    e.AccountSummaryPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AccountSummaryPdfPath == null) return NotFound();

            return File(gov.AccountSummaryPdfPath, "application/pdf");

        }

        public async Task<IActionResult> ViewAuditedStatementPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AuditedStatementPdfPath,
                    e.AuditedStatementPdfName
                })
                .FirstOrDefaultAsync();
                
            if (gov == null || gov.AuditedStatementPdfPath == null) return NotFound();

            return File(gov.AuditedStatementPdfPath, "application/pdf");

        }
        public async Task<IActionResult> ViewExaminerDetailsPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.ExaminerDetailsPdfPath,
                    e.ExaminerDetailsPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.ExaminerDetailsPdfPath == null) return NotFound();

            return File(gov.ExaminerDetailsPdfPath, "application/pdf");

        }
        public async Task<IActionResult> ViewAebasLastThreeMonthsPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AebaslastThreeMonthsPdfPath,
                    e.AebaslastThreeMonthsPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AebaslastThreeMonthsPdfPath == null) return NotFound();

            return File(gov.AebaslastThreeMonthsPdfPath, "application/pdf");

        }
        public async Task<IActionResult> ViewAebasInspectionDayPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AebasinspectionDayPdfPath,
                    e.AebasinspectionDayPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AebasinspectionDayPdfPath == null) return NotFound();

            return File(gov.AebasinspectionDayPdfPath, "application/pdf");

        }
        public async Task<IActionResult> ViewProvidentFundPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.ProvidentFundPdfPath,
                    e.ProvidentFundPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.ProvidentFundPdfPath == null) return NotFound();

            return File(gov.ProvidentFundPdfPath, "application/pdf");

        }
        public async Task<IActionResult> ViewEsipdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.EsipdfPath,
                    e.EsipdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.EsipdfPath == null) return NotFound();

            return File(gov.EsipdfPath, "application/pdf");

        }

        public async Task<IActionResult> TestPdf()
        {
            return RedirectToAction(nameof(Preview));
        }

        private IActionResult GeneratePreviewPdf(Medical_Affiliation.Models.CApreviewViewModel model)
        {
            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "newLogoBg2.png"
            );

            var clgLogoPath = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "newLogo.jpeg");

            var logoBytes = System.IO.File.ReadAllBytes(logoPath);
            var clglogoBytes = System.IO.File.ReadAllBytes(clgLogoPath);

            var pdf = new PreviewReportPdf(model, logoBytes, clglogoBytes);
            var bytes = pdf.GeneratePdf();

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var affiliationType = HttpContext.Session.GetString("TypeOfAffiliation");
            var courseLevel = HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel");
            var fileName = string.Join("_", new[]
            {
                NormalizeFileNamePart(collegeCode, "College"),
                NormalizeFileNamePart(affiliationType, "Affiliation"),
                NormalizeFileNamePart(courseLevel, "CourseLevel")
            }) + ".pdf";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(bytes, "application/pdf");
        }

        private static string NormalizeFileNamePart(string? value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            var invalidCharacters = Path.GetInvalidFileNameChars();
            var normalized = new string(value.Trim()
                .Select(character => invalidCharacters.Contains(character) || char.IsWhiteSpace(character) ? '_' : character)
                .ToArray());

            while (normalized.Contains("__", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("__", "_", StringComparison.Ordinal);
            }

            return normalized.Trim('_');
        }


    }
}
