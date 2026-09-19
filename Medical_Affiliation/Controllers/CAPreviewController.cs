using Medical_Affiliation.DATA;
using Medical_Affiliation.Services.Faculty;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System;
using System.Linq;
using System.Security.Cryptography;

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

            var model = await _capreviewService.GetPreviewAsync();
            _paymentCalculationController.ControllerContext = ControllerContext;
            model.PaymentCalculation = await _paymentCalculationController.GetCurrentCalculationAsync();
            return GeneratePreviewPdf(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApplication(bool declarationConsent)
        {
            var completion = await _capreviewService.GetPreviewAsync();
            if (!completion.IsApplicationComplete)
            {
                TempData["PreviewError"] = $"Complete the application sections before submitting. Current completion: {completion.CompletionPercentage}%.";
                return RedirectToAction(nameof(Preview));
            }

            if (!declarationConsent)
            {
                TempData["PreviewError"] = "Please accept the declaration before submitting the application.";
                return RedirectToAction(nameof(Preview));
            }

            var paymentCalculation = await GetPaymentCalculationAsync();
            var collegeCode = (completion.CollegeCode ?? HttpContext.Session.GetString("CollegeCode"))?.Trim();
            var facultyCode = (completion.FacultyCode ?? HttpContext.Session.GetString("FacultyCode"))?.Trim();
            var applicationType = (completion.ApplicationType ?? HttpContext.Session.GetString("TypeOfAffiliation"))?.Trim();
            var courseLevel = (paymentCalculation.CourseLevel ?? completion.ApplyingCourseLevel ?? HttpContext.Session.GetString("CourseLevel"))?.Trim();
            var courseCodes = paymentCalculation.MatchedCourses
                .Where(course => !string.IsNullOrWhiteSpace(course.CourseCode))
                .Select(course => course.CourseCode.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (courseCodes.Count == 0)
            {
                var sessionCourseCode = HttpContext.Session.GetString("CourseCode");
                if (!string.IsNullOrWhiteSpace(sessionCourseCode))
                {
                    courseCodes.AddRange(sessionCourseCode.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }
            }

            if (string.IsNullOrWhiteSpace(collegeCode)
                || string.IsNullOrWhiteSpace(facultyCode)
                || string.IsNullOrWhiteSpace(applicationType)
                || string.IsNullOrWhiteSpace(courseLevel)
                || courseCodes.Count == 0)
            {
                TempData["PreviewError"] = "Registration, college, faculty, application type, course level and course details are required before submission.";
                return RedirectToAction(nameof(Preview));
            }

            var courseCode = (HttpContext.Session.GetString("CourseCode") ?? courseCodes[0])
                .Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? courseCodes[0];

            var registrationNumber = await GetOrGenerateRegistrationNumberAsync(
                collegeCode,
                facultyCode,
                courseCode,
                applicationType,
                courseLevel);

            if (await HasSubmittedApplicationAsync(collegeCode, applicationType, courseLevel))
            {
                paymentCalculation = await GetPaymentCalculationAsync();
                var submittedModel = await _capreviewService.GetPreviewAsync();
                submittedModel.PaymentCalculation = paymentCalculation;
                return GeneratePreviewPdf(submittedModel);
            }

            await SaveApplicationSubmissionAsync(
                facultyCode,
                collegeCode,
                courseCode!,
                applicationType,
                courseLevel,
                registrationNumber);

            paymentCalculation = await GetPaymentCalculationAsync();
            var model = await _capreviewService.GetPreviewAsync();
            model.PaymentCalculation = paymentCalculation;
            return GeneratePreviewPdf(model);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitApplication()
        {
            var model = await _capreviewService.GetPreviewAsync();
            var collegeCode = (model.CollegeCode ?? HttpContext.Session.GetString("CollegeCode"))?.Trim();
            var applicationType = (model.ApplicationType ?? HttpContext.Session.GetString("TypeOfAffiliation"))?.Trim();
            var courseLevel = (model.ApplyingCourseLevel
                ?? HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel"))?.Trim();

            if (string.IsNullOrWhiteSpace(collegeCode)
                || string.IsNullOrWhiteSpace(applicationType)
                || string.IsNullOrWhiteSpace(courseLevel)
                || !await HasSubmittedApplicationAsync(collegeCode, applicationType, courseLevel))
            {
                return Forbid();
            }

            _paymentCalculationController.ControllerContext = ControllerContext;
            model.PaymentCalculation = await _paymentCalculationController.GetCurrentCalculationAsync();
            return GeneratePreviewPdf(model);
        }

        private async Task<bool> HasSubmittedApplicationAsync(
            string collegeCode,
            string applicationType,
            string courseLevel)
        {
            return await _context.Database.SqlQuery<int>($@"
                SELECT COUNT(*) AS [Value]
                FROM dbo.ApplicationSubmission
                WHERE CollegeCode = {collegeCode}
                  AND LTRIM(RTRIM(TypeOfAffiliation)) = LTRIM(RTRIM({applicationType}))
                  AND UPPER(LTRIM(RTRIM(CourseLevel))) = {courseLevel.Trim().ToUpperInvariant()}").FirstOrDefaultAsync() > 0;
        }

        private async Task<string> GetOrGenerateRegistrationNumberAsync(
            string? collegeCode,
            string? facultyCode,
            string? courseCode,
            string? applicationType,
            string? courseLevel)
        {
            var normalizedCollegeCode = collegeCode?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedCollegeCode)
                || string.IsNullOrWhiteSpace(facultyCode)
                || string.IsNullOrWhiteSpace(courseCode)
                || string.IsNullOrWhiteSpace(applicationType)
                || string.IsNullOrWhiteSpace(courseLevel))
                return string.Empty;

            var connectionString = _context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The application's database connection string is not configured.");

            await using var connection = new SqlConnection(connectionString);
            await using var existingCommand = new SqlCommand(@"
                SELECT TOP (1) RegistrationNumber
                FROM dbo.ApplicationSubmission
                WHERE FacultyCode = @FacultyCode
                  AND CollegeCode = @CollegeCode
                  AND CourseCode = @CourseCode
                  AND TypeOfAffiliation = @TypeOfAffiliation
                  AND CourseLevel = @CourseLevel
                ORDER BY CreatedOn DESC, Id DESC;", connection);

            existingCommand.Parameters.Add("@FacultyCode", System.Data.SqlDbType.VarChar, 20).Value = facultyCode;
            existingCommand.Parameters.Add("@CollegeCode", System.Data.SqlDbType.VarChar, 20).Value = normalizedCollegeCode;
            existingCommand.Parameters.Add("@CourseCode", System.Data.SqlDbType.VarChar, 20).Value = courseCode;
            existingCommand.Parameters.Add("@TypeOfAffiliation", System.Data.SqlDbType.VarChar, 100).Value = applicationType;
            existingCommand.Parameters.Add("@CourseLevel", System.Data.SqlDbType.VarChar, 50).Value = courseLevel;

            await connection.OpenAsync();
            var existingRegistration = await existingCommand.ExecuteScalarAsync();
            if (existingRegistration is string saved && !string.IsNullOrWhiteSpace(saved))
                return saved.Trim();

            var currentYear = DateTime.Now.Year;
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var serial = RandomNumberGenerator.GetInt32(0, 100000).ToString("D5");
                var candidate = $"{normalizedCollegeCode}_{currentYear}_{serial}";

                await using var existsCommand = new SqlCommand(
                    "SELECT COUNT(1) FROM dbo.ApplicationSubmission WHERE RegistrationNumber = @RegistrationNumber;",
                    connection);
                existsCommand.Parameters.Add("@RegistrationNumber", System.Data.SqlDbType.VarChar, 50).Value = candidate;
                var exists = Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) > 0;

                if (!exists)
                    return candidate;
            }

            throw new InvalidOperationException("Unable to generate a unique registration number.");
        }

        private async Task<PaymentCalculationViewModel> GetPaymentCalculationAsync()
        {
            _paymentCalculationController.ControllerContext = ControllerContext;
            return await _paymentCalculationController.GetCurrentCalculationAsync();
        }

        private async Task SaveApplicationSubmissionAsync(
            string facultyCode,
            string collegeCode,
            string courseCode,
            string typeOfAffiliation,
            string courseLevel,
            string registrationNumber)
        {
            var connectionString = _context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The application's database connection string is not configured.");

            await using var connection = new SqlConnection(connectionString);
            await using var command = new SqlCommand("dbo.usp_SaveApplicationSubmission", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            command.Parameters.Add("@FacultyCode", System.Data.SqlDbType.VarChar, 20).Value = facultyCode;
            command.Parameters.Add("@CollegeCode", System.Data.SqlDbType.VarChar, 20).Value = collegeCode;
            command.Parameters.Add("@CourseCode", System.Data.SqlDbType.VarChar, 20).Value = courseCode;
            command.Parameters.Add("@TypeOfAffiliation", System.Data.SqlDbType.VarChar, 100).Value = typeOfAffiliation;
            command.Parameters.Add("@CourseLevel", System.Data.SqlDbType.VarChar, 50).Value = courseLevel;
            command.Parameters.Add("@RegistrationNumber", System.Data.SqlDbType.VarChar, 50).Value = registrationNumber;

            await connection.OpenAsync();
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    // The procedure result is not required by the submission flow.
                }
            }

            // Keep the generated registration number in ApplicationSubmission
            // even when the deployment's stored procedure does not persist it.
            // The guard keeps this safe if the procedure already inserted it.
            await using var insertCommand = new SqlCommand(@"
                INSERT INTO dbo.ApplicationSubmission
                    (FacultyCode, CollegeCode, CourseCode, TypeOfAffiliation, CourseLevel, RegistrationNumber)
                SELECT @FacultyCode, @CollegeCode, @CourseCode, @TypeOfAffiliation, @CourseLevel, @RegistrationNumber
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.ApplicationSubmission
                    WHERE FacultyCode = @FacultyCode
                      AND CollegeCode = @CollegeCode
                      AND CourseCode = @CourseCode
                      AND TypeOfAffiliation = @TypeOfAffiliation
                      AND CourseLevel = @CourseLevel
                      AND RegistrationNumber = @RegistrationNumber
                );", connection);

            insertCommand.Parameters.Add("@FacultyCode", System.Data.SqlDbType.VarChar, 20).Value = facultyCode;
            insertCommand.Parameters.Add("@CollegeCode", System.Data.SqlDbType.VarChar, 20).Value = collegeCode;
            insertCommand.Parameters.Add("@CourseCode", System.Data.SqlDbType.VarChar, 20).Value = courseCode;
            insertCommand.Parameters.Add("@TypeOfAffiliation", System.Data.SqlDbType.VarChar, 100).Value = typeOfAffiliation;
            insertCommand.Parameters.Add("@CourseLevel", System.Data.SqlDbType.VarChar, 50).Value = courseLevel;
            insertCommand.Parameters.Add("@RegistrationNumber", System.Data.SqlDbType.VarChar, 50).Value = registrationNumber;
            await insertCommand.ExecuteNonQueryAsync();
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
