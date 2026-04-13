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
        public CAPreviewController(ApplicationDbContext context, ICAPreviewService capreviewService)
        {
            _context = context;
            _capreviewService = capreviewService;
        }
        public async Task<IActionResult> Preview()
        {

            var model = await _capreviewService.GetPreviewAsync();
            return View(model);
        }

        public async Task<IActionResult> GetCurriculumFile(int id)
        {
            var file = await _context.CaCourseCurricula
                .AsNoTracking()
                .Where(x => x.CourseCurriculumId == id)
                .Select(x => new
                {
                    x.CurriculumPdf,
                    x.PdfFileName
                })
                .FirstOrDefaultAsync();

            if (file == null || file.CurriculumPdf == null)
                return NotFound();

            return File(file.CurriculumPdf, "application/pdf");
        }
        public async Task<IActionResult> PreviewHospitalDocument(int id)
        {
            var file = await _context.AffiliatedHospitalDocuments
                .AsNoTracking()
                .Where(x => x.DocumentId == id)
                .Select(x => new
                {
                    x.DocumentName,
                    x.DocumentFile
                })
                .FirstOrDefaultAsync();

            if (file == null || file.DocumentFile == null)
                return NotFound();

            return File(file.DocumentFile, "application/pdf");
        }
        public async Task<IActionResult> ViewServiceFile(int id)
        {
            var file = await _context.CaMedicalLibraryServices
                .AsNoTracking()
                .Where(x => x.LibraryServiceId == id)
                .Select(x => new
                {
                    x.UploadedFileName,
                    x.UploadedPdf
                })
                .FirstOrDefaultAsync();

            if (file == null || file.UploadedPdf == null)
                return NotFound();

            return File(file.UploadedPdf, "application/pdf");
        }
        public async Task<IActionResult> ViewSpecialFeaturesPdf(int id)
        {
            var file = await _context.CaMedicalLibraryOtherDetails
                .AsNoTracking()
                .Where(x => x.DigitalValuationId == id)
                .Select(x => new
                {
                    x.UploadedFileName,
                    x.SpecialFeaturesAchievementspdf
                })
                .FirstOrDefaultAsync();

            if (file == null || file.SpecialFeaturesAchievementspdf == null)
                return NotFound();

            return File(file.SpecialFeaturesAchievementspdf, "application/pdf");
        }

        public async Task<IActionResult> ViewGoverningCouncilPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.GoverningCouncilPdf,
                    e.GoverningCouncilPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.GoverningCouncilPdf == null) return NotFound();

            return File(gov.GoverningCouncilPdf, "application/pdf");

        }

        public async Task<IActionResult> ViewAccountSummaryPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AccountSummaryPdf,
                    e.AccountSummaryPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AccountSummaryPdf == null) return NotFound();

            return File(gov.AccountSummaryPdf, "application/pdf");

        }

        public async Task<IActionResult> ViewAuditedStatementPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AuditedStatementPdf,
                    e.AuditedStatementPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AuditedStatementPdf == null) return NotFound();

            return File(gov.AuditedStatementPdf, "application/pdf");

        }
        public async Task<IActionResult> ViewExaminerDetailsPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.ExaminerDetailsPdf,
                    e.ExaminerDetailsPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.ExaminerDetailsPdf == null) return NotFound();

            return File(gov.ExaminerDetailsPdf, "application/pdf");

        }
        public async Task<IActionResult> ViewAebasLastThreeMonthsPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AebaslastThreeMonthsPdf,
                    e.AebaslastThreeMonthsPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AebaslastThreeMonthsPdf == null) return NotFound();

            return File(gov.AebaslastThreeMonthsPdf, "application/pdf");

        }
        public async Task<IActionResult> ViewAebasInspectionDayPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.AebasinspectionDayPdf,
                    e.AebasinspectionDayPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.AebasinspectionDayPdf == null) return NotFound();

            return File(gov.AebasinspectionDayPdf, "application/pdf");

        }
        public async Task<IActionResult> ViewProvidentFundPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.ProvidentFundPdf,
                    e.ProvidentFundPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.ProvidentFundPdf == null) return NotFound();

            return File(gov.ProvidentFundPdf, "application/pdf");

        }
        public async Task<IActionResult> ViewEsipdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Esipdf,
                    e.EsipdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || gov.Esipdf == null) return NotFound();

            return File(gov.Esipdf, "application/pdf");

        }

        public async Task<IActionResult> TestPdf()
        {
            var model = await _capreviewService.GetPreviewAsync(); // you already have this
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

            return File(bytes, "application/pdf");
        }


    }
}
