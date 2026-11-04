using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
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

        private string? CurrentCollegeCode => HttpContext.Session.GetString("CollegeCode");
        private string? CurrentFacultyCode => HttpContext.Session.GetString("FacultyCode");
        private int? CurrentFacultyId => int.TryParse(CurrentFacultyCode, out var facultyId) ? facultyId : null;
        private int CurrentAffiliationType => HttpContext.Session.GetInt32("AffiliationType")
            ?? (int.TryParse(HttpContext.Session.GetString("TypeOfAffiliation"), out var affiliationType) ? affiliationType : 2);
        private string CurrentCourseLevel => (HttpContext.Session.GetString("CourseLevel")
            ?? HttpContext.Session.GetString("SelectedCourseLevel")
            ?? string.Empty).Trim().ToUpperInvariant();

        public async Task<IActionResult> Preview()
        {
            var requestedCourseLevel = Request.Query["courseLevel"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(requestedCourseLevel))
            {
                HttpContext.Session.SetString("CourseLevel", requestedCourseLevel.Trim());
                HttpContext.Session.SetString("SelectedCourseLevel", requestedCourseLevel.Trim());
            }

            var requestedAffiliationType = Request.Query["typeOfAffiliation"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(requestedAffiliationType))
                HttpContext.Session.SetString("TypeOfAffiliation", requestedAffiliationType.Trim());

            if (int.TryParse(Request.Query["affiliationTypeId"], out var requestedAffiliationTypeId)
                && requestedAffiliationTypeId > 0)
            {
                HttpContext.Session.SetInt32("AffiliationType", requestedAffiliationTypeId);
                HttpContext.Session.SetString("AffiliationTypeId", requestedAffiliationTypeId.ToString());
            }

            _paymentCalculationController.ControllerContext = ControllerContext;
            var paymentCalculation = await _paymentCalculationController.GetCurrentCalculationAsync();

            var model = await _capreviewService.GetPreviewAsync();
            DedupeFaculty(model);
            model.PaymentCalculation = paymentCalculation;

            await ApplySkillsLabAsync(model);          // Section 11 data from Medical_SkillsLaboratory
            await ApplySmallGroupTeachingAsync(model); // Building Details data from SmallGroupTeachings
            await ApplyAnnualMbbsIntakeAsync(model);   // AnnualMbbsIntake from Intake_26_27 (UG only)
            await ApplyDesignationIntakeAsync(model);  // Designation & Intake: Available + Seat Slab

            // Sections 08A - 08F (PG only): General Details, Summary, Infrastructure, Services,
            // Staff & Exam Results, Academic Activities. Grouped by department. Empty list for UG / SS.
            ViewBag.PgSections = await LoadPgSectionsAsync(model);

            // Section 08 - PG Course Affiliation (PG only): course details, GOK, RGUHS, NMC-permitted
            // courses, filtered by college code + course level.
            ViewBag.PgCourseAffiliation = await LoadPgCourseAffiliationAsync(model);

            // Section 12 - Associated Institutions (college name + course name from master tables)
            ViewBag.AssociatedInstitutions = await LoadAssociatedInstitutionsAsync(model);

            // Section 17 - Vehicle Registration & Documents (RC / Insurance / DL availability)
            ViewBag.VehicleDocs = await LoadVehicleDocsAsync();

            // Section 13B - Outpatient Area (IndoorInfrastructureRequirementsCompliance, SectionCode = 15)
            ViewBag.OutpatientArea = await LoadOutpatientAreaAsync();

            // Section 21 - Academic Matters PG
            ViewBag.PgAcademicPerformance = await LoadPgAcademicPerformanceAsync(model);

            var reuploadPath = await FindSealedSignedReuploadPathAsync(
                model.FacultyCode ?? CurrentFacultyCode,
                model.CollegeCode ?? CurrentCollegeCode,
                model.ApplicationType ?? HttpContext.Session.GetString("TypeOfAffiliation"));

            model.HasSealedSignedReupload = !string.IsNullOrWhiteSpace(reuploadPath)
                && System.IO.File.Exists(reuploadPath);

            return View(model);
        }

        private static void DedupeFaculty(CApreviewViewModel model)
        {
            var list = model?.FacultyDesigNonTeachDisplayVM?.FacultyDetailDisplayVM;
            if (list == null) return;

            model.FacultyDesigNonTeachDisplayVM.FacultyDetailDisplayVM = list
                .GroupBy(f => !string.IsNullOrWhiteSpace(f.AadhaarNo)
                    ? "A:" + f.AadhaarNo.Trim()
                    : "N:" + f.NameOfFaculty?.Trim().ToUpper() + "|" + f.Mobile?.Trim())
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToList();
        }

        // ------------------------------------------------------------------
        // Section 12 - Associated Institutions
        // Source: dbo.AssociatedInstitutions (filtered by this college's code)
        //   College name <- Affiliation_College_Master (CollegeCode -> CollegeName)
        //   Course name  <- Mst_Course (CourseCode -> CourseName), falling back to
        //                   the name stored in AssociatedInstitutions
        // OUTER APPLY TOP (1) keeps one row per association even when a master
        // code appears under several faculties (the faculty match is preferred).
        // ------------------------------------------------------------------
        private async Task<List<AssociatedInstitutionRow>> LoadAssociatedInstitutionsAsync(CApreviewViewModel model)
        {
            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim();
            if (string.IsNullOrWhiteSpace(collegeCode))
                return new List<AssociatedInstitutionRow>();

            try
            {
                return await _context.Database.SqlQuery<AssociatedInstitutionRow>($@"
                    SELECT a.Id,
                           CAST(a.TypeOfAffiliation AS varchar(100))      AS TypeOfAffiliation,
                           CAST(a.CollegeCode AS varchar(50))             AS CollegeCode,
                           CAST(a.AssociatedCollegeCode AS varchar(50))   AS AssociatedCollegeCode,
                           CAST(cm.CollegeName AS varchar(300))           AS AssociatedCollegeName,
                           CAST(a.AssociatedFacultyCode AS varchar(50))   AS AssociatedFacultyCode,
                           CAST(a.FacultyCode AS varchar(50))             AS FacultyCode,
                           CAST(a.CourseCode AS varchar(50))              AS CourseCode,
                           CAST(COALESCE(NULLIF(LTRIM(RTRIM(mc.CourseName)), ''), a.CourseName) AS varchar(300)) AS CourseName,
                           CAST(a.CourseLevel AS varchar(50))             AS CourseLevel,
                           a.AnnualIntake,
                           a.CreatedOn
                    FROM dbo.AssociatedInstitutions a
                    OUTER APPLY (
                        SELECT TOP (1) m.CollegeName
                        FROM [Admission_Affiliation].[dbo].[Affiliation_College_Master] m
                        WHERE LTRIM(RTRIM(CAST(m.CollegeCode AS varchar(50)))) = LTRIM(RTRIM(CAST(a.AssociatedCollegeCode AS varchar(50))))
                        ORDER BY CASE WHEN LTRIM(RTRIM(CAST(m.FacultyCode AS varchar(50)))) = LTRIM(RTRIM(CAST(a.AssociatedFacultyCode AS varchar(50)))) THEN 0 ELSE 1 END
                    ) cm
                    OUTER APPLY (
                        SELECT TOP (1) c.CourseName
                        FROM [Admission_Affiliation].[dbo].[Mst_Course] c
                        WHERE LTRIM(RTRIM(CAST(c.CourseCode AS varchar(50)))) = LTRIM(RTRIM(CAST(a.CourseCode AS varchar(50))))
                        ORDER BY CASE WHEN LTRIM(RTRIM(CAST(c.FacultyCode AS varchar(50)))) = LTRIM(RTRIM(CAST(a.AssociatedFacultyCode AS varchar(50)))) THEN 0 ELSE 1 END
                    ) mc
                    WHERE LTRIM(RTRIM(CAST(a.CollegeCode AS varchar(50)))) = {collegeCode}
                    ORDER BY a.AssociatedCollegeCode, a.CourseName").ToListAsync();
            }
            catch (SqlException)
            {
                // A table/column name did not match in this deployment: show an empty
                // section instead of breaking the whole preview.
                return new List<AssociatedInstitutionRow>();
            }
        }

        // ------------------------------------------------------------------
        // Section 21 - Academic Matters PG
        // Source: [Admission_Affiliation].[dbo].[CA_AcademicPerformance]
        // Same rule as the AcademicMattersPG page: college + PG, prefer the
        // session faculty/affiliation, otherwise use whatever was actually saved.
        // Subject code and year id are shown as names.
        // Reuses PgSectionTable so no new DTO is needed.
        // ------------------------------------------------------------------
        private async Task<PgSectionTable> LoadPgAcademicPerformanceAsync(CApreviewViewModel model)
        {
            var table = new PgSectionTable
            {
                Title = "Academic Performance",
                Headers = new List<string>
                {
                    "Subject", "Year of Study", "Regular Students", "Repeater Students",
                    "Students Passed", "Pass %", "First Class", "Distinction", "Remarks"
                }
            };

            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(collegeCode))
                return table;

            int sessFaculty = int.TryParse((model.FacultyCode ?? CurrentFacultyCode)?.Trim(), out var f) ? f : 1;
            int sessAff = CurrentAffiliationType;

            try
            {
                var all = await _context.CaAcademicPerformances
                    .AsNoTracking()
                    .Where(x => x.CollegeCode != null
                                && x.CollegeCode.Trim().ToUpper() == collegeCode
                                && x.CourseLevel != null
                                && x.CourseLevel.Trim().ToUpper() == "PG"
                                && x.Subject != null && x.Subject != "")
                    .ToListAsync();

                var scoped = all.Where(x => x.FacultyId == sessFaculty && x.AffiliationType == sessAff).ToList();
                var rows = scoped.Any() ? scoped : all;

                // Subject code -> subject name
                var subjectNames = (await _context.MstCourses
                        .AsNoTracking()
                        .Where(c => c.CourseLevel.ToUpper() == "PG")
                        .Select(c => new { c.CourseCode, c.SubjectName })
                        .ToListAsync())
                    .GroupBy(c => c.CourseCode.ToString())
                    .ToDictionary(g => g.Key, g => g.First().SubjectName);

                // Year id -> year name
                var yearNames = (await _context.CaMstYearOfStudies
                        .AsNoTracking()
                        .ToListAsync())
                    .GroupBy(y => y.YearOfStudyId)
                    .ToDictionary(g => g.Key, g => g.First().YearName);

                static string N(object? v) => v == null ? "—" : Convert.ToString(v, CultureInfo.InvariantCulture) ?? "—";

                foreach (var r in rows
                             .OrderBy(x => subjectNames.TryGetValue(x.Subject!.Trim(), out var n) ? n : x.Subject)
                             .ThenBy(x => x.YearOfStudyId))
                {
                    var key = r.Subject!.Trim();
                    var subject = subjectNames.TryGetValue(key, out var sn) && !string.IsNullOrWhiteSpace(sn) ? sn : key;
                    var year = r.YearOfStudyId.HasValue && yearNames.TryGetValue(r.YearOfStudyId.Value, out var yn)
                        ? yn
                        : N(r.YearOfStudyId);

                    table.Rows.Add(new List<string>
                    {
                        subject,
                        year,
                        N(r.RegularStudents),
                        N(r.RepeaterStudents),
                        N(r.NumberOfStudentsPassed),
                        r.PassPercentage.HasValue ? r.PassPercentage.Value.ToString("0.##", CultureInfo.InvariantCulture) : "—",
                        N(r.FirstClassCount),
                        N(r.DistinctionCount),
                        string.IsNullOrWhiteSpace(r.Remarks) ? "—" : r.Remarks.Trim()
                    });
                }
            }
            catch (Exception)
            {
                table.Rows.Clear();
            }

            return table;
        }

        // ------------------------------------------------------------------
        // PG-only sections 08A - 08F (General Details, Summary, Infrastructure Details,
        // Services & Clinical Material, Staff & Exam Results, Academic Activities).
        // Filters by college code + faculty code + application type (+ course code).
        // Grouped by department (name from Mst_MedicalCollegeCourseIntake).
        // Returns an empty list when the applying course level is not PG.
        // ------------------------------------------------------------------
        private Task<List<PgSectionGroup>> LoadPgSectionsAsync(CApreviewViewModel model) =>
            PgSectionLoader.LoadForApplicationAsync(
                model,
                HttpContext.Session,
                _context.Database.GetConnectionString());

        // ------------------------------------------------------------------
        // Section 08 - PG Course Affiliation
        // Reads four tables from [Admission_Affiliation].[dbo] filtered by
        // CollegeCode + CourseLevel (as requested):
        //   Affiliation_PgSsCourseDetails          -> PG Course Details
        //   Affiliation_PgSsCourseDetailsForGOK    -> Details for GOK
        //   Affiliation_PgSsCourseDetailsRGUHS     -> Details for RGUHS
        //   Affiliation_OtherCoursesPermittedByNMC -> Other Courses Permitted by NMC
        // Document path columns are shown as "Uploaded" / "Not Uploaded".
        //
        // If one college can have several application types, add
        //   AND LTRIM(RTRIM(CAST(TypeOfAffiliation AS varchar(100)))) = @t
        // so rows from past applications are not listed too.
        // ------------------------------------------------------------------
        private async Task<List<PgSectionTable>> LoadPgCourseAffiliationAsync(CApreviewViewModel model)
        {
            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim();
            var courseLevel = (!string.IsNullOrWhiteSpace(CurrentCourseLevel)
                    ? CurrentCourseLevel
                    : model.ApplyingCourseLevel ?? "PG")
                .Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(collegeCode))
                return new List<PgSectionTable>();

            var connStr = _context.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connStr))
                return new List<PgSectionTable>();

            // (title, table, columns, headers)
            var defs = new (string Title, string Table, string[] Cols, string[] Heads)[]
            {
                ("PG Course Details", "Affiliation_PgSsCourseDetails",
                    new[] { "CourseName", "CourseCode", "CoursePrefix", "PresentIntake", "RguhsIntake", "LOPDate", "DateofRecognitionByNMC", "DateofRecognitionByDCI" },
                    new[] { "Course Name", "Course Code", "Prefix", "Present Intake", "RGUHS Intake", "LoP Date", "Recognition by NMC", "Recognition by DCI" }),

                ("Details for GOK", "Affiliation_PgSsCourseDetailsForGOK",
                    new[] { "CourseName", "CourseCode", "CoursePrefix", "PresentIntake", "SanctionedIntake", "GOKdate", "AcademicYear", "DocumentofGOKPath" },
                    new[] { "Course Name", "Course Code", "Prefix", "Present Intake", "Sanctioned Intake", "GOK Date", "Academic Year", "GOK Document" }),

                ("Details for RGUHS", "Affiliation_PgSsCourseDetailsRGUHS",
                    new[] { "CourseName", "CourseCode", "RguhsIntake", "RGUHSsupportingDocumentPath" },
                    new[] { "Course Name", "Course Code", "RGUHS Intake", "Supporting Document" }),

                ("Other Courses Permitted by NMC", "Affiliation_OtherCoursesPermittedByNMC",
                    new[] { "CourseName", "CourseCode", "PermissionByNMC", "NumberOfAdmissionsPerYear", "NMCsupportingDocumentPath" },
                    new[] { "Course Name", "Course Code", "Permission by NMC", "Admissions per Year", "Supporting Document" })
            };

            var result = new List<PgSectionTable>();
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            foreach (var d in defs)
            {
                var table = new PgSectionTable { Title = d.Title, Headers = d.Heads.ToList() };
                try
                {
                    var cols = string.Join(", ", d.Cols.Select(c => $"[{c}]"));
                    await using var cmd = new SqlCommand($@"
                        SELECT {cols}
                        FROM [Admission_Affiliation].[dbo].[{d.Table}]
                        WHERE LTRIM(RTRIM(CAST(CollegeCode AS varchar(50)))) = @c
                          AND UPPER(LTRIM(RTRIM(CAST(CourseLevel AS varchar(50))))) = @l
                        ORDER BY CourseName, Id", conn);
                    cmd.Parameters.Add("@c", SqlDbType.VarChar, 50).Value = collegeCode;
                    cmd.Parameters.Add("@l", SqlDbType.VarChar, 50).Value = courseLevel;

                    await using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync())
                    {
                        var row = new List<string>();
                        for (var i = 0; i < d.Cols.Length; i++)
                        {
                            var v = r.GetValue(i);
                            var isDoc = d.Cols[i].EndsWith("Path", StringComparison.OrdinalIgnoreCase);
                            row.Add(v is DBNull ? "—"
                                : isDoc ? (string.IsNullOrWhiteSpace(Convert.ToString(v)) ? "Not Uploaded" : "Uploaded")
                                : v is DateTime dt ? dt.ToString("dd-MM-yyyy")
                                : v is bool b ? (b ? "Yes" : "No")
                                : Convert.ToString(v, CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } s ? s : "—");
                        }
                        table.Rows.Add(row);
                    }
                }
                catch (SqlException)
                {
                    // Table / column missing in this deployment: leave this table empty
                    // instead of breaking the whole preview.
                    table.Rows.Clear();
                }
                result.Add(table);
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> ViewSignedReupload()
        {
            var preview = await _capreviewService.GetPreviewAsync();
            var filePath = await FindSealedSignedReuploadPathAsync(
                preview.FacultyCode ?? CurrentFacultyCode,
                preview.CollegeCode ?? CurrentCollegeCode,
                preview.ApplicationType ?? HttpContext.Session.GetString("TypeOfAffiliation"));

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("Uploaded document not found.");

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType);
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
            DedupeFaculty(model);
            _paymentCalculationController.ControllerContext = ControllerContext;
            model.PaymentCalculation = await _paymentCalculationController.GetCurrentCalculationAsync();
            await ApplySkillsLabAsync(model);
            await ApplySmallGroupTeachingAsync(model);
            await ApplyAnnualMbbsIntakeAsync(model);
            await ApplyDesignationIntakeAsync(model);
            return await GeneratePreviewPdf(model);
        }

        // ------------------------------------------------------------------
        // Principal seal & signature re-upload.
        // Intentionally has NO "already submitted" check: the re-upload must
        // stay available before AND after the application is submitted.
        // Uploading again replaces the previously saved document.
        // ------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReuploadSignedDocument(IFormFile? sealedSignedReupload)
        {
            if (sealedSignedReupload == null || sealedSignedReupload.Length == 0)
            {
                TempData["PreviewError"] = "Choose a signed application document to upload.";
                return RedirectToAction(nameof(Preview));
            }

            var preview = await _capreviewService.GetPreviewAsync();
            var collegeCode = (preview.CollegeCode ?? HttpContext.Session.GetString("CollegeCode"))?.Trim();
            var facultyCode = (preview.FacultyCode ?? HttpContext.Session.GetString("FacultyCode"))?.Trim();
            var applicationType = (preview.ApplicationType ?? HttpContext.Session.GetString("TypeOfAffiliation"))?.Trim();

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                string.IsNullOrWhiteSpace(applicationType))
            {
                TempData["PreviewError"] = "College, faculty, and application details are required to save the re-upload.";
                return RedirectToAction(nameof(Preview));
            }

            var error = await SaveSealedSignedReuploadAsync(
                sealedSignedReupload,
                facultyCode,
                collegeCode,
                applicationType);

            if (error != null)
            {
                TempData["PreviewError"] = error;
                return RedirectToAction(nameof(Preview));
            }

            TempData["PreviewSuccess"] = "Signed application document uploaded successfully.";
            return RedirectToAction(nameof(Preview));
        }

        private async Task<string?> SaveSealedSignedReuploadAsync(
            IFormFile? sealedSignedReupload,
            string? facultyCode,
            string? collegeCode,
            string? applicationType)
        {
            if (sealedSignedReupload == null || sealedSignedReupload.Length == 0)
                return null;

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(sealedSignedReupload.FileName);
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return "Only PDF, DOC, DOCX, JPG, JPEG, and PNG files are allowed for the re-upload.";

            var baseFolder = Directory.Exists(@"E:\") ? @"E:\Affiliation_Medical" : @"D:\Affiliation_Medical";
            var sealedFolder = Path.Combine(baseFolder, "SealedSignedReupload");
            Directory.CreateDirectory(sealedFolder);

            var savedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(sealedFolder, savedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await sealedSignedReupload.CopyToAsync(stream);
            }

            var existing = await _context.AffiliationSealedSignedReuploads
                .Where(x => x.FacultyCode == facultyCode
                    && x.CollegeCode == collegeCode
                    && x.TypeOfApplication == applicationType)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            var oldFilePath = existing?.ReuploadedDoc;
            try
            {
                if (existing == null)
                {
                    _context.AffiliationSealedSignedReuploads.Add(new AffiliationSealedSignedReupload
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        TypeOfApplication = applicationType,
                        ReuploadedDoc = fullPath,
                        CreatedOn = DateTime.Now
                    });
                }
                else
                {
                    existing.ReuploadedDoc = fullPath;
                    existing.CreatedOn = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(oldFilePath)
                && !string.Equals(oldFilePath, fullPath, StringComparison.OrdinalIgnoreCase)
                && System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            return null;
        }

        private async Task<string?> FindSealedSignedReuploadPathAsync(
            string? facultyCode,
            string? collegeCode,
            string? applicationType)
        {
            if (string.IsNullOrWhiteSpace(facultyCode)
                || string.IsNullOrWhiteSpace(collegeCode)
                || string.IsNullOrWhiteSpace(applicationType))
            {
                return null;
            }

            return await _context.AffiliationSealedSignedReuploads
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode.Trim()
                    && x.CollegeCode == collegeCode.Trim()
                    && x.TypeOfApplication == applicationType.Trim())
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Select(x => x.ReuploadedDoc)
                .FirstOrDefaultAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApplication(bool declarationConsent, IFormFile? sealedSignedReupload)
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

            if (sealedSignedReupload != null && sealedSignedReupload.Length > 0)
            {
                var reuploadError = await SaveSealedSignedReuploadAsync(
                    sealedSignedReupload,
                    facultyCode,
                    collegeCode,
                    applicationType);
                if (reuploadError != null)
                {
                    TempData["PreviewError"] = reuploadError;
                    return RedirectToAction(nameof(Preview));
                }
            }

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
                DedupeFaculty(submittedModel);
                submittedModel.PaymentCalculation = paymentCalculation;
                await ApplySkillsLabAsync(submittedModel);
                await ApplySmallGroupTeachingAsync(submittedModel);
                await ApplyAnnualMbbsIntakeAsync(submittedModel);
                await ApplyDesignationIntakeAsync(submittedModel);
                return await GeneratePreviewPdf(submittedModel);
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
            DedupeFaculty(model);
            model.PaymentCalculation = paymentCalculation;
            await ApplySkillsLabAsync(model);
            await ApplySmallGroupTeachingAsync(model);
            await ApplyAnnualMbbsIntakeAsync(model);
            await ApplyDesignationIntakeAsync(model);
            return await GeneratePreviewPdf(model);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitApplication()
        {
            var model = await _capreviewService.GetPreviewAsync();
            DedupeFaculty(model);
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
            await ApplySkillsLabAsync(model);
            await ApplySmallGroupTeachingAsync(model);
            await ApplyAnnualMbbsIntakeAsync(model);
            await ApplyDesignationIntakeAsync(model);
            return await GeneratePreviewPdf(model);
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
                .Where(x => x.CourseCurriculumId == id &&
                            x.CollegeCode == CurrentCollegeCode &&
                            x.FacultyId == CurrentFacultyId &&
                            x.AffiliationType == CurrentAffiliationType)
                .Select(x => new
                {
                    x.CurriculumPdfPath,
                    x.PdfFileName
                })
                .FirstOrDefaultAsync();

            if (file == null || string.IsNullOrWhiteSpace(file.CurriculumPdfPath) || !System.IO.File.Exists(file.CurriculumPdfPath))
                return NotFound();

            return PhysicalFile(file.CurriculumPdfPath, "application/pdf");
        }

        public async Task<IActionResult> PreviewHospitalDocument(int id, string mode = "view")
        {
            var file = await _context.AffiliatedHospitalDocuments
                .AsNoTracking()
                .Where(x => x.DocumentId == id &&
                            x.CollegeCode == CurrentCollegeCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
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

            // Detect content type dynamically
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.DocumentFilePth, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // Download mode
            if (mode == "download")
            {
                return PhysicalFile(file.DocumentFilePth, contentType, fileName);
            }

            // Preview mode (inline)
            return PhysicalFile(file.DocumentFilePth, contentType);
        }

        public async Task<IActionResult> ViewServiceFile(int id)
        {
            var file = await _context.CaMedicalLibraryServices
                .AsNoTracking()
                .Where(x => x.LibraryServiceId == id &&
                            x.CollegeCode == CurrentCollegeCode &&
                            x.FacultyCode.ToString() == CurrentFacultyCode &&
                            x.AffiliationType == CurrentAffiliationType &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(x => new
                {
                    x.UploadedFileName,
                    x.UploadedPdfPath
                })
                .FirstOrDefaultAsync();

            if (file == null || string.IsNullOrWhiteSpace(file.UploadedPdfPath) || !System.IO.File.Exists(file.UploadedPdfPath))
                return NotFound();

            return PhysicalFile(file.UploadedPdfPath, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ViewUsageReportPdf()
        {
            var filePath = await _context.CaMedicalLibraryUsageReports
                .AsNoTracking()
                .Where(x => x.CollegeCode == CurrentCollegeCode &&
                            x.FacultyCode.ToString() == CurrentFacultyCode &&
                            x.AffiliationType == CurrentAffiliationType &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(x => x.UploadedFileDataPath)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return NotFound();

            return PhysicalFile(filePath, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ViewLibraryCommitteePdf(int id)
        {
            var filePath = await _context.CaMedLibCommittees
                .AsNoTracking()
                .Where(x => x.Id == id &&
                            x.CollegeCode == CurrentCollegeCode &&
                            x.FacultyCode == CurrentFacultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(x => x.CommitteePdfPath)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return NotFound();

            return PhysicalFile(filePath, "application/pdf");
        }

        public async Task<IActionResult> ViewSpecialFeaturesPdf(int id)
        {
            var file = await _context.CaMedicalLibraryOtherDetails
                .AsNoTracking()
                .Where(x => x.DigitalValuationId == id &&
                            x.CollegeCode == CurrentCollegeCode &&
                            x.FacultyCode == CurrentFacultyId &&
                            x.AffiliationType == CurrentAffiliationType &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(x => new
                {
                    x.UploadedFileName,
                    x.SpecialFeaturesAchievementsPdfPath
                })
                .FirstOrDefaultAsync();

            if (file == null || string.IsNullOrWhiteSpace(file.SpecialFeaturesAchievementsPdfPath) || !System.IO.File.Exists(file.SpecialFeaturesAchievementsPdfPath))
                return NotFound();

            return File(file.SpecialFeaturesAchievementsPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewGoverningCouncilPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode && e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel)
                .Select(e => new
                {
                    e.GoverningCouncilPdfPath,
                    e.GoverningCouncilPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.GoverningCouncilPdfPath) || !System.IO.File.Exists(gov.GoverningCouncilPdfPath)) return NotFound();

            return File(gov.GoverningCouncilPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewAccountSummaryPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode && e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel)
                .Select(e => new
                {
                    e.AccountSummaryPdfPath,
                    e.AccountSummaryPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.AccountSummaryPdfPath) || !System.IO.File.Exists(gov.AccountSummaryPdfPath)) return NotFound();

            return File(gov.AccountSummaryPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewAuditedStatementPdf(int id)
        {
            var gov = await _context.MedCaAccountAndFeeDetails
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode && e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel)
                .Select(e => new
                {
                    e.AuditedStatementPdfPath,
                    e.AuditedStatementPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.AuditedStatementPdfPath) || !System.IO.File.Exists(gov.AuditedStatementPdfPath)) return NotFound();

            return File(gov.AuditedStatementPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewExaminerDetailsPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode &&
                            (string.IsNullOrEmpty(e.CourseLevel) || e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(e => new
                {
                    e.ExaminerDetailsPdfPath,
                    e.ExaminerDetailsPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.ExaminerDetailsPdfPath) || !System.IO.File.Exists(gov.ExaminerDetailsPdfPath)) return NotFound();

            return File(gov.ExaminerDetailsPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewAebasLastThreeMonthsPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode &&
                            (string.IsNullOrEmpty(e.CourseLevel) || e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(e => new
                {
                    e.AebaslastThreeMonthsPdfPath,
                    e.AebaslastThreeMonthsPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.AebaslastThreeMonthsPdfPath) || !System.IO.File.Exists(gov.AebaslastThreeMonthsPdfPath)) return NotFound();

            return File(gov.AebaslastThreeMonthsPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewAebasInspectionDayPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode &&
                            (string.IsNullOrEmpty(e.CourseLevel) || e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(e => new
                {
                    e.AebasinspectionDayPdfPath,
                    e.AebasinspectionDayPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.AebasinspectionDayPdfPath) || !System.IO.File.Exists(gov.AebasinspectionDayPdfPath)) return NotFound();

            return File(gov.AebasinspectionDayPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewProvidentFundPdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode &&
                            (string.IsNullOrEmpty(e.CourseLevel) || e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(e => new
                {
                    e.ProvidentFundPdfPath,
                    e.ProvidentFundPdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.ProvidentFundPdfPath) || !System.IO.File.Exists(gov.ProvidentFundPdfPath)) return NotFound();

            return File(gov.ProvidentFundPdfPath, "application/pdf");
        }

        public async Task<IActionResult> ViewEsipdf(int id)
        {
            var gov = await _context.CaMedStaffParticularsOthers
                .AsNoTracking()
                .Where(e => e.Id == id && e.CollegeCode == CurrentCollegeCode && e.FacultyCode == CurrentFacultyCode &&
                            (string.IsNullOrEmpty(e.CourseLevel) || e.CourseLevel.Trim().ToUpper() == CurrentCourseLevel))
                .Select(e => new
                {
                    e.EsipdfPath,
                    e.EsipdfName
                })
                .FirstOrDefaultAsync();

            if (gov == null || string.IsNullOrWhiteSpace(gov.EsipdfPath) || !System.IO.File.Exists(gov.EsipdfPath)) return NotFound();

            return File(gov.EsipdfPath, "application/pdf");
        }

        // ------------------------------------------------------------------
        // Section 17 - Vehicle Registration & Documents
        // The vehicle table stores RC / Insurance / Driving Licence as a status
        // flag: 'A' = Available, 'N' = Not available. No file is involved, so
        // the preview simply translates the flag.
        //
        // >>> RENAME "dbo.CA_VehicleDetails" below to your real table name
        // >>> (the table that has VehicleRegNo, RcBookStatus, InsuranceStatus ...).
        // ------------------------------------------------------------------
        private async Task<List<VehicleDocRow>> LoadVehicleDocsAsync()
        {
            var collegeCode = CurrentCollegeCode?.Trim();
            var facultyCode = CurrentFacultyCode?.Trim();
            var courseLevel = CurrentCourseLevel;

            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
                return new List<VehicleDocRow>();

            try
            {
                return await _context.Database.SqlQuery<VehicleDocRow>($@"
                    SELECT Id,
                           CAST(VehicleRegNo AS varchar(100))            AS VehicleRegNo,
                           CAST(VehicleForCode AS varchar(20))           AS VehicleForCode,
                           TRY_CAST(SeatingCapacity AS int)              AS SeatingCapacity,
                           TRY_CAST(ValidityDate AS date)                AS ValidityDate,
                           CAST(RcBookStatus AS varchar(5))              AS RcBookStatus,
                           CAST(InsuranceStatus AS varchar(5))           AS InsuranceStatus,
                           CAST(DrivingLicenseStatus AS varchar(5))      AS DrivingLicenseStatus
                    FROM dbo.CA_VehicleDetails                                      -- RENAME table
                    WHERE LTRIM(RTRIM(CAST(CollegeCode AS varchar(50)))) = {collegeCode}
                      AND LTRIM(RTRIM(CAST(FacultyCode AS varchar(20)))) = {facultyCode}
                      AND (ISNULL(LTRIM(RTRIM(CAST(CourseLevel AS varchar(50)))), '') = ''
                           OR UPPER(LTRIM(RTRIM(CAST(CourseLevel AS varchar(50))))) = {courseLevel})
                    ORDER BY Id").ToListAsync();
            }
            catch (SqlException)
            {
                // Table name not matched yet: return nothing so the view falls back
                // to the original VehicleDetailsPreview partial.
                return new List<VehicleDocRow>();
            }
        }

        // ------------------------------------------------------------------
        // Section 13B - Outpatient Area
        // Source: [Admission_Affiliation].[dbo].[IndoorInfrastructureRequirementsCompliance]
        //         WHERE SectionCode = 15
        // Keeps only the newest row per (HospitalDetailsId, RequirementId) so a
        // re-saved requirement is not listed twice.
        //
        // If rows do not show up, the usual cause is the AffiliationTypeId or
        // CourseLevel filter not matching what is stored. Remove that filter
        // line to test.
        //
        // >>> OPTIONAL: to show the requirement NAME instead of its Id, replace
        // >>> CAST(NULL AS varchar(500)) AS RequirementName with a LEFT JOIN to
        // >>> your requirement master table.
        // ------------------------------------------------------------------
        private async Task<List<OutpatientAreaRow>> LoadOutpatientAreaAsync()
        {
            var collegeCode = CurrentCollegeCode?.Trim();
            var facultyCode = CurrentFacultyCode?.Trim();
            var courseLevel = CurrentCourseLevel;
            var affiliationTypeId = CurrentAffiliationType;

            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
                return new List<OutpatientAreaRow>();

            try
            {
                return await _context.Database.SqlQuery<OutpatientAreaRow>($@"
                    SELECT Id,
                           HospitalDetailsId,
                           RequirementId,
                           CAST(NULL AS varchar(500))               AS RequirementName,
                           CAST(IsCompliant AS varchar(20))         AS IsCompliantRaw,
                           CAST(Remarks AS varchar(1000))           AS Remarks,
                           TRY_CAST(InspectedOn AS datetime)        AS InspectedOn
                    FROM (
                        SELECT *,
                               ROW_NUMBER() OVER (
                                   PARTITION BY HospitalDetailsId, RequirementId
                                   ORDER BY Id DESC) AS rn
                        FROM [Admission_Affiliation].[dbo].[IndoorInfrastructureRequirementsCompliance]
                        WHERE SectionCode = 15
                          AND LTRIM(RTRIM(CAST(CollegeCode AS varchar(50)))) = {collegeCode}
                          AND LTRIM(RTRIM(CAST(FacultyCode AS varchar(20)))) = {facultyCode}
                          AND AffiliationTypeId = {affiliationTypeId}
                          AND (ISNULL(LTRIM(RTRIM(CAST(CourseLevel AS varchar(50)))), '') = ''
                               OR UPPER(LTRIM(RTRIM(CAST(CourseLevel AS varchar(50))))) = {courseLevel})
                    ) x
                    WHERE rn = 1
                    ORDER BY HospitalDetailsId, RequirementId").ToListAsync();
            }
            catch (SqlException)
            {
                return new List<OutpatientAreaRow>();
            }
        }

        public async Task<IActionResult> TestPdf()
        {
            return RedirectToAction(nameof(Preview));
        }

        // Async because the PG-only sections (08A - 08F) are loaded here so they
        // appear in the generated PDF as well as on the preview page.
        private async Task<IActionResult> GeneratePreviewPdf(Medical_Affiliation.Models.CApreviewViewModel model)
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

            // The PDF still uses the flat tables (group.Tables). Where several departments are
            // present, the flat tables get a leading "Department" column with the department name.
            var pgSections = await LoadPgSectionsAsync(model);

            // Section 08 (PG Course Affiliation) data for the PDF. Uncomment once
            // PreviewReportPdf has a WithPgCourseAffiliation(List<PgSectionTable>) method:
            // var pgCourseTables = await LoadPgCourseAffiliationAsync(model);

            // Section 12 (Associated Institutions) data for the PDF. Pass it to PreviewReportPdf
            // once it has a method for it, e.g. .WithAssociatedInstitutions(associatedInstitutions):
            // var associatedInstitutions = await LoadAssociatedInstitutionsAsync(model);

            // Section 21 (Academic Matters PG) data for the PDF. Loaded directly here because
            // ViewBag is only filled by the Preview() action. Pass it to PreviewReportPdf once it
            // has a method for it, e.g. .WithPgAcademic(pgAcademicTable):
            var pgAcademicTable = await LoadPgAcademicPerformanceAsync(model);

            var pdf = new PreviewReportPdf(model, logoBytes, clglogoBytes).WithPgSections(pgSections);
            // .WithPgCourseAffiliation(pgCourseTables)
            // .WithAssociatedInstitutions(associatedInstitutions)
            // .WithPgAcademic(pgAcademicTable);
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

        // ------------------------------------------------------------------
        // Section 11 - Skills Laboratory: load the row from Medical_SkillsLaboratory
        // and copy it onto model.PhysicalFacilities.SkillsLab (matched by property name).
        // ------------------------------------------------------------------
        private async Task ApplySkillsLabAsync(Medical_Affiliation.Models.CApreviewViewModel model)
        {
            if (model?.PhysicalFacilities == null)
                return;

            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim();
            var facultyCode = (model.FacultyCode ?? CurrentFacultyCode)?.Trim();
            var affiliationType = CurrentAffiliationType;

            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
                return;

            var row = await _context.Database.SqlQuery<SkillsLabRow>($@"
                SELECT TOP (1) *
                FROM dbo.Medical_SkillsLaboratory
                WHERE LTRIM(RTRIM(CollegeCode)) = {collegeCode}
                  AND LTRIM(RTRIM(CAST(FacultyCode AS varchar(20)))) = {facultyCode}
                ORDER BY Id DESC").FirstOrDefaultAsync();

            if (row == null)
                return;

            var pf = model.PhysicalFacilities;
            var prop = pf.GetType().GetProperty("SkillsLab");
            if (prop == null) return;

            var target = prop.GetValue(pf) ?? Activator.CreateInstance(prop.PropertyType);
            if (target == null) return;

            // Copy by matching property name (handles bool? -> bool, int? -> int, etc.)
            foreach (var src in typeof(SkillsLabRow).GetProperties())
            {
                var dst = target.GetType().GetProperty(src.Name);
                if (dst == null || !dst.CanWrite) continue;

                var value = src.GetValue(row);
                var dstType = Nullable.GetUnderlyingType(dst.PropertyType) ?? dst.PropertyType;

                if (value == null)
                {
                    if (!dst.PropertyType.IsValueType || Nullable.GetUnderlyingType(dst.PropertyType) != null)
                        dst.SetValue(target, null);
                    continue;
                }

                try { dst.SetValue(target, Convert.ChangeType(value, dstType)); }
                catch { /* type mismatch - skip this field */ }
            }

            prop.SetValue(pf, target);
        }

        // ------------------------------------------------------------------
        // Building Details > General Details (Section 10 / PDF): load the row from
        // dbo.SmallGroupTeachings by college code and copy it onto
        // model.PhysicalFacilities.SmallGroupTeaching (matched by property name).
        // Columns are read generically, so int/bit/decimal/varchar differences
        // between the table and the model cannot cause cast errors.
        // Best row = same college, preferring matching faculty + course level, newest Id.
        // ------------------------------------------------------------------
        private async Task ApplySmallGroupTeachingAsync(Medical_Affiliation.Models.CApreviewViewModel model)
        {
            if (model?.PhysicalFacilities == null)
                return;

            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim();
            var facultyCode = (model.FacultyCode ?? CurrentFacultyCode)?.Trim() ?? string.Empty;
            var courseLevel = (!string.IsNullOrWhiteSpace(CurrentCourseLevel)
                    ? CurrentCourseLevel
                    : model.ApplyingCourseLevel ?? string.Empty)
                .Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(collegeCode))
                return;

            var connectionString = _context.Database.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            await using var connection = new SqlConnection(connectionString);
            await using var command = new SqlCommand(@"
                SELECT TOP (1) *
                FROM dbo.SmallGroupTeachings
                WHERE LTRIM(RTRIM(CAST(CollegeCode AS varchar(50)))) = @CollegeCode
                ORDER BY
                    CASE WHEN LTRIM(RTRIM(CAST(FacultyCode AS varchar(50)))) = @FacultyCode THEN 0 ELSE 1 END,
                    CASE WHEN UPPER(LTRIM(RTRIM(ISNULL(CAST(CourseLevel AS varchar(50)), '')))) = @CourseLevel THEN 0 ELSE 1 END,
                    Id DESC;", connection);

            command.Parameters.Add("@CollegeCode", System.Data.SqlDbType.VarChar, 50).Value = collegeCode;
            command.Parameters.Add("@FacultyCode", System.Data.SqlDbType.VarChar, 50).Value = facultyCode;
            command.Parameters.Add("@CourseLevel", System.Data.SqlDbType.VarChar, 50).Value = courseLevel;

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return;

            var pf = model.PhysicalFacilities;
            var prop = pf.GetType().GetProperty("SmallGroupTeaching");
            if (prop == null) return;

            var target = prop.GetValue(pf) ?? Activator.CreateInstance(prop.PropertyType);
            if (target == null) return;

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var dst = target.GetType().GetProperty(reader.GetName(i));
                if (dst == null || !dst.CanWrite) continue;

                var raw = reader.GetValue(i);
                var nullable = !dst.PropertyType.IsValueType || Nullable.GetUnderlyingType(dst.PropertyType) != null;
                var dstType = Nullable.GetUnderlyingType(dst.PropertyType) ?? dst.PropertyType;

                try
                {
                    if (raw == null || raw is DBNull)
                    {
                        if (nullable) dst.SetValue(target, null);
                        continue;
                    }

                    object converted;
                    if (dstType == typeof(bool))
                    {
                        var text = Convert.ToString(raw)?.Trim();
                        converted = text != null && (text.Equals("1") || text.Equals("true", StringComparison.OrdinalIgnoreCase)
                            || text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text.Equals("y", StringComparison.OrdinalIgnoreCase));
                    }
                    else if (dstType == typeof(string))
                    {
                        converted = Convert.ToString(raw)?.Trim() ?? string.Empty;
                    }
                    else
                    {
                        converted = Convert.ChangeType(raw, dstType, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    dst.SetValue(target, converted);
                }
                catch { /* type mismatch - skip this field */ }
            }

            prop.SetValue(pf, target);
        }

        private async Task ApplyAnnualMbbsIntakeAsync(Medical_Affiliation.Models.CApreviewViewModel model)
        {
            if (model == null)
                return;

            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim();
            var facultyCode = (model.FacultyCode ?? CurrentFacultyCode)?.Trim();
            var courseLevel = (!string.IsNullOrWhiteSpace(CurrentCourseLevel)
                    ? CurrentCourseLevel
                    : model.ApplyingCourseLevel ?? string.Empty)
                .Trim().ToUpperInvariant();

            // Annual MBBS intake only applies to UG
            if (string.IsNullOrWhiteSpace(collegeCode)
                || string.IsNullOrWhiteSpace(facultyCode)
                || courseLevel != "UG")
                return;

            // Matches on college code + faculty code + course level (ug_pg) + MBBS,
            // and reads only the Intake_26_27 column.
            var intake = await _context.Database.SqlQuery<int>($@"
        SELECT TOP (1) COALESCE(TRY_CAST(Intake_26_27 AS int), 0) AS [Value]
        FROM dbo.Mst_MedicalCollegeCourseIntake
        WHERE LTRIM(RTRIM(coll_code)) = {collegeCode}
          AND LTRIM(RTRIM(CAST(Facultycode AS varchar(20)))) = {facultyCode}
          AND UPPER(LTRIM(RTRIM(ug_pg))) = {courseLevel}
          AND UPPER(LTRIM(RTRIM(course))) = 'MBBS'
        ORDER BY SLNO DESC").FirstOrDefaultAsync();

            // Section 06 - Details of MBBS Course - UG: "Intake 2026-27" comes from Intake_26_27.
            // The model property keeps its old name (IntakeDuring202526) but now carries the 2026-27 intake.
            if (intake > 0)
            {
                var courseDetail = model.InstitutionBasicVM?.AffiliationCourseDetailVM;
                var intakeProp = courseDetail?.GetType().GetProperty("IntakeDuring202526");
                if (courseDetail != null && intakeProp != null && intakeProp.CanWrite)
                {
                    var intakeType = Nullable.GetUnderlyingType(intakeProp.PropertyType) ?? intakeProp.PropertyType;
                    try
                    {
                        object value = intakeType == typeof(string)
                            ? intake.ToString()
                            : Convert.ChangeType(intake, intakeType);
                        intakeProp.SetValue(courseDetail, value);
                    }
                    catch { /* unexpected type - leave as is */ }
                }
            }

            if (model.PhysicalFacilities == null)
                return;

            // Section 10 - Land, Building & Teaching Facilities
            if (model.PhysicalFacilities.SmallGroupTeaching != null)
                model.PhysicalFacilities.SmallGroupTeaching.AnnualMbbsIntake = intake;

            // Section 11 - Skills Laboratory
            if (model.PhysicalFacilities.SkillsLab != null)
                model.PhysicalFacilities.SkillsLab.AnnualMbbsIntake = intake;
        }

        // ------------------------------------------------------------------
        // Designation & Intake Details (PDF)
        //   Available  -> count of rows in dbo.UG_Faculty_Details, grouped by
        //                 DepartmentCode + DesignationCode, for this college + faculty.
        //   Seat Slab  -> rows of dbo.Mst_MedicalCollegeCourseIntake for this
        //                 college code (coll_code) + course level (ug_pg).
        //
        // REQUIRED: add these two properties to CApreviewViewModel, then have
        // PreviewReportPdf read them in the "Designation & Intake Details" table:
        //     public List<FacultyAvailableRow> AvailableFaculty { get; set; } = new();
        //     public List<SeatSlabRow> SeatSlabs { get; set; } = new();
        // ------------------------------------------------------------------
        private async Task ApplyDesignationIntakeAsync(Medical_Affiliation.Models.CApreviewViewModel model)
        {
            if (model == null)
                return;

            var collegeCode = (model.CollegeCode ?? CurrentCollegeCode)?.Trim();
            var facultyCode = (model.FacultyCode ?? CurrentFacultyCode)?.Trim();
            var courseLevel = (!string.IsNullOrWhiteSpace(CurrentCourseLevel)
                    ? CurrentCourseLevel
                    : model.ApplyingCourseLevel ?? string.Empty)
                .Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(collegeCode))
                return;

            // ---- Available: faculty actually present in UG_Faculty_Details ----
            if (!string.IsNullOrWhiteSpace(facultyCode))
            {
                model.AvailableFaculty = await _context.Database.SqlQuery<FacultyAvailableRow>($@"
                    SELECT LTRIM(RTRIM(CAST(DepartmentCode  AS varchar(50)))) AS DepartmentCode,
                           LTRIM(RTRIM(CAST(DesignationCode AS varchar(50)))) AS DesignationCode,
                           COUNT(*) AS Available
                    FROM dbo.UG_Faculty_Details
                    WHERE LTRIM(RTRIM(CAST(CollegeCode AS varchar(50)))) = {collegeCode}
                      AND LTRIM(RTRIM(CAST(FacultyCode AS varchar(20)))) = {facultyCode}
                    GROUP BY LTRIM(RTRIM(CAST(DepartmentCode  AS varchar(50)))),
                             LTRIM(RTRIM(CAST(DesignationCode AS varchar(50))))").ToListAsync();
            }

            // ---- Seat Slab: intake rows for this college + course level ----
            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                model.SeatSlabs = await _context.Database.SqlQuery<SeatSlabRow>($@"
                    SELECT course                                  AS Course,
                           CAST(CourseCode AS varchar(50))         AS CourseCode,
                           UPPER(LTRIM(RTRIM(ug_pg)))              AS CourseLevel,
                           TRY_CAST(Intake_26_27 AS int)           AS Intake_26_27,
                           TRY_CAST(Increased_Intake AS int)       AS Increased_Intake
                    FROM dbo.Mst_MedicalCollegeCourseIntake
                    WHERE LTRIM(RTRIM(coll_code)) = {collegeCode}
                      AND UPPER(LTRIM(RTRIM(ug_pg))) = {courseLevel}
                    ORDER BY SLNO").ToListAsync();
            }
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

// ----------------------------------------------------------------------
// Row DTO for dbo.Medical_SkillsLaboratory.
// Lives in Medical_Affiliation.Models so Preview.cshtml (which has
// "@using Medical_Affiliation.Models") can see it.
// If you already created SkillsLabRow in your Models folder, delete this
// copy to avoid a duplicate-type error.
// If a column type differs (e.g. FacultyCode is int, or area columns are
// float), change the property type to match the column.
// ----------------------------------------------------------------------
namespace Medical_Affiliation.Models
{
    public class SkillsLabRow
    {
        public int Id { get; set; }
        public string? FacultyCode { get; set; }
        public int? AnnualMbbsIntake { get; set; }
        public decimal? TotalAreaAvailableSqm { get; set; }
        public decimal? TotalAreaRequiredSqm { get; set; }
        public decimal? TotalAreaDeficiencySqm { get; set; }
        public bool? SixWeeksTrainingCompletedBeforeClinical { get; set; }
        public int? NumberOfExaminationRooms { get; set; }
        public bool? HasMinFourExamRooms { get; set; }
        public bool? HasDemoRoomSmallGroups { get; set; }
        public bool? HasDebriefArea { get; set; }
        public bool? HasFacultyCoordinatorRoom { get; set; }
        public bool? HasSupportStaffRoom { get; set; }
        public bool? HasStorageForMannequins { get; set; }
        public bool? HasVideoRecordingFacility { get; set; }
        public int? NumberOfSkillStations { get; set; }
        public bool? HasGroupAndIndividualStations { get; set; }
        public bool? HasRequiredTrainersAndMannequins { get; set; }
        public bool? HasDedicatedTechnicalOfficer { get; set; }
        public bool? HasAdequateSupportStaff { get; set; }
        public bool? TeachingAreasHaveAV { get; set; }
        public bool? TeachingAreasHaveInternet { get; set; }
        public bool? SkillsLabEnabledForELearning { get; set; }
        public string? CollegeCode { get; set; }
        public int? AnnualBdsIntake { get; set; }
        public int? AffiliationTypeId { get; set; }
    }

    // ------------------------------------------------------------------
    // Row DTO for the existing dbo.AssociatedInstitutions table
    // (used by Section 12 - Associated Institutions in Preview.cshtml).
    // Read-only: the table already exists, nothing here creates or alters it.
    // AssociatedCollegeName comes from Affiliation_College_Master and CourseName
    // from Mst_Course (see LoadAssociatedInstitutionsAsync), so the preview shows
    // names instead of codes.
    // If you already have a class with this name in your Models folder,
    // delete this copy. If a column type differs (e.g. FacultyCode is int),
    // change the matching property type.
    // ------------------------------------------------------------------
    public class AssociatedInstitutionRow
    {
        public int Id { get; set; }
        public string? TypeOfAffiliation { get; set; }
        public string? CollegeCode { get; set; }
        public string? AssociatedCollegeCode { get; set; }
        public string? AssociatedCollegeName { get; set; }
        public string? AssociatedFacultyCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public int? AnnualIntake { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    // ------------------------------------------------------------------
    // Designation & Intake Details (PDF) row DTOs.
    // Available: UG_Faculty_Details grouped by department + designation.
    // Seat Slab: Mst_MedicalCollegeCourseIntake rows by college + course level.
    // ------------------------------------------------------------------
    public class FacultyAvailableRow
    {
        public string? DepartmentCode { get; set; }
        public string? DesignationCode { get; set; }
        public int Available { get; set; }
    }

    public class SeatSlabRow
    {
        public string? Course { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseLevel { get; set; }
        public int? Intake_26_27 { get; set; }
        public int? Increased_Intake { get; set; }
    }

    // ------------------------------------------------------------------
    // Section 17 - Vehicle Registration & Documents row DTO.
    // Status columns hold 'A' (Available) or 'N' (Not available).
    // Has* / VehicleTypeName are get-only (computed), so EF does not read them from SQL.
    // ------------------------------------------------------------------
    public class VehicleDocRow
    {
        public int Id { get; set; }
        public string? VehicleRegNo { get; set; }
        public string? VehicleForCode { get; set; }
        public int? SeatingCapacity { get; set; }
        public DateTime? ValidityDate { get; set; }
        public string? RcBookStatus { get; set; }
        public string? InsuranceStatus { get; set; }
        public string? DrivingLicenseStatus { get; set; }

        private static bool IsA(string? s) => string.Equals(s?.Trim(), "A", StringComparison.OrdinalIgnoreCase);

        public bool HasRc => IsA(RcBookStatus);
        public bool HasInsurance => IsA(InsuranceStatus);
        public bool HasDl => IsA(DrivingLicenseStatus);

        public string VehicleTypeName => (VehicleForCode ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "STU" => "Student",
            "INT" => "Institution",
            "AMB" => "Ambulance",
            "" => "—",
            var other => other
        };
    }

    // ------------------------------------------------------------------
    // Section 13B - Outpatient Area row DTO.
    // Source: IndoorInfrastructureRequirementsCompliance (SectionCode = 15).
    // Only PUBLIC settable properties matching the SQL aliases are read by EF.
    // If a column type differs, change the property type to match.
    // ------------------------------------------------------------------
    public class OutpatientAreaRow
    {
        public int Id { get; set; }
        public int? HospitalDetailsId { get; set; }
        public int? RequirementId { get; set; }
        public string? RequirementName { get; set; }

        // Raw stored value (works whether the column is bit, int or varchar).
        public string? IsCompliantRaw { get; set; }

        // Computed (get-only, so EF does not read it from SQL).
        // 1 / true / yes / y / a  => true ;  0 / false / no / n => false ; anything else => null
        public bool? IsCompliant
        {
            get
            {
                var v = (IsCompliantRaw ?? string.Empty).Trim().ToLowerInvariant();
                return v switch
                {
                    "1" or "true" or "yes" or "y" or "a" or "available" or "compliant" => true,
                    "0" or "false" or "no" or "n" or "not available" => false,
                    _ => null
                };
            }
        }
        public string? Remarks { get; set; }
        public DateTime? InspectedOn { get; set; }
    }

    // ==================================================================
    // PG-only preview / PDF sections 08A - 08F (Applying Course Level = PG)
    //   08A General Details, 08B Summary, 08C Infrastructure Details,
    //   08D Services & Clinical Material / Investigative Workload,
    //   08E Staff Details & Student Exam Results, 08F Academic Activities.
    // Data comes from [Admission_Affiliation].[dbo].* tables and is filtered by
    // CollegeCode + FacultyCode + TypeOfAffiliation (+ CourseCode when known).
    // The preview groups every section by department (Departments); the PDF
    // keeps using the flat tables (Tables).
    // ==================================================================
    public class PgSectionTable
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }

    public class PgDepartmentBlock
    {
        public string CourseCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public List<PgSectionTable> Tables { get; set; } = new();
    }

    public class PgSectionGroup
    {
        public string Anchor { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<PgSectionTable> Tables { get; set; } = new();          // flat tables (kept for the PDF)
        public List<PgDepartmentBlock> Departments { get; set; } = new();  // same data grouped by department
        public bool HasData => Tables.Any(t => t.Rows.Count > 0);
    }

    public static class PgSectionLoader
    {
        private const string Db = "[Admission_Affiliation].[dbo].";

        // Order = comma separated column list (first column is used for "latest row" tables).
        // Latest = keep only the newest row per CourseCode (single-record tables that may be re-saved).
        private sealed record TableDef(string Title, string Table, string Order, bool Latest, string[] Columns);
        private sealed record GroupDef(string Anchor, string Number, string Title, TableDef[] Tables);

        private static TableDef T(string title, string table, string order, bool latest, params string[] cols)
            => new(title, table, order, latest, cols);

        private static readonly GroupDef[] Groups =
        {
            new("PgGeneralDetails", "08A", "PG General Details", new[]
            {
                T("General Details", "TxnPGCourseGeneralDetail", "PGCourseGeneralDetailId", true,
                    "AcademicYear","LoPDate","YearsSinceStart","HODName","ExistingSeats","IncreaseSeatsFrom",
                    "IncreaseSeatsTo","TotalUnits","DepartmentBeds","TotalICUHDUBeds")
            }),

            new("PgSummary", "08B", "PG Summary", new[]
            {
                T("Summary", "TxnPGCourseSummaryDetail", "PGCourseSummaryDetailId", true,
                    "DateOfAssessment","AssessorName","InstitutionName","InstitutionCategory",
                    "HeadOfInstitutionDesignation","HeadOfInstitutionName","HeadOfInstitutionAgeDob",
                    "HeadOfInstitutionTeachingExp","HeadOfInstitutionPgdegree","HeadOfInstitutionPgrecognition",
                    "HeadOfInstitutionSubject","DepartmentInspected","Hodname","HodageDob","HodteachingExp",
                    "HodpgDegree","HodpgRecognition","NumberOfUgseats")
            }),

            new("PgInfrastructure", "08C", "PG Infrastructure Details", new[]
            {
                T("Local Inspection Committee", "LocalInspectionCommittee", "SlNo", false,
                    "NameOfChairmanOrMember","CorrespondenceAddress","PhoneOffResMobile","Email"),
                T("Fee Paid Details", "FeePaidDetails", "SlNo", false,
                    "Particulars","Amount","TransactionId","PaymentDate","BankName","BankBranch"),
                T("Other Courses / Observership", "OtherCourseObservership", "Id", false,
                    "NameOfQualificationCourse","PermittedByMciNmc","NumberOfAdmissionsPerYear"),
                T("OPD Details", "OpdDetails", "Id", true,
                    "NoOfRoomsForConsultation","WaitingAreaInSqM","SpaceAndArrangements","IfNotAdequateReasons",
                    "DressingRoomAvailable","SeparateMinorOtMaleFemale","PerRectalExamRoomAvailable","DressingRoom2Available"),
                T("OPD Room Area", "OpdRoomArea", "Id", false, "RoomType","AreaInSqM"),
                T("Wards", "WardsHeader", "Id", true, "NoOfWardsMale","NoOfWardsFemale"),
                T("Ward Parameters", "WardsParameters", "Id", false, "ParameterName","Details"),
                T("Operation Theatre Distribution", "OperationTheatreDistribution", "IsTotalRow,SlNo", false,
                    "DepartmentName","MajorOtTables","MinorOtTables"),
                T("Seminar Room", "SeminarRoom", "Id", true,
                    "SpaceAndFacility","InternetFacility","AudiovisualEquipmentDetails"),
                T("Departmental Museum", "DepartmentalMuseum", "Id", true,
                    "Space","TotalNumberOfSpecimens","TotalNumberOfChartDiagram"),
                T("Library Facility", "LibraryFacility", "Id", true,
                    "NumberOfBooksCentral","NumberOfBooksDepartmental","BooksPurchasedLast3YrsCentral",
                    "BooksPurchasedLast3YrsDept","AnnexureAttached","TotalIndianJournalsCentral",
                    "TotalIndianJournalsDept","TotalForeignJournalsCentral","TotalForeignJournalsDept",
                    "ComputerWithInternetCentral","ComputerWithInternetDept","CentralLibraryTiming",
                    "CentralReadingRoomTiming"),
                T("Departmental Research Lab", "DepartmentalResearchLab", "Id", true,
                    "Space","Equipment","ResearchProjectsCompletedPast3Yrs","ResearchProjectsInProgress"),
                T("Department Equipment", "DepartmentEquipment", "Id", false,
                    "NameOfEquipment","NumbersAvailable","FunctionalStatus","IsAdequate")
            }),

            new("PgServices", "08D", "PG Services & Clinical Material / Investigative Workload", new[]
            {
                T("Specialty Clinics", "SpecialtyClinicDetail", "ClinicSequence", false,
                    "ClinicName","Weekdays","Timings","NumberOfCasesAvg","ClinicInchargeName"),
                T("Department Services", "DepartmentServiceDetail", "ServiceSequence", false,
                    "ServiceName","IsAvailable","Remarks"),
                T("Clinical / Investigative Workload", "ClinicalWorkloadDetail", "ParticularSequence", false,
                    "ParticularName","EntireHospital","OnDayOfAssessment","Random3Days","PreviousYear")
            }),

            new("PgStaffExam", "08E", "PG Staff Details & Student Exam Results", new[]
            {
                T("Eligible Faculty", "EligibleFacultyDetail", "DesignationSequence", false,
                    "Designation","NumberOfFaculty","Names","TotalAdmissionSeats","IsAdequateForAdmission"),
                T("PG Students (Year-wise)", "PgStudentsYearWiseDetail", "YearSequence", false,
                    "YearLabel","NumberOfStudents"),
                T("Student Exam Results", "StudentExamResultDetail", "SlNo", false,
                    "StudentName","Result")
            }),

            new("PgAcademic", "08F", "PG Academic Activities", new[]
            {
                T("Academic Activities", "AcademicActivityDetail", "SlNo", false,
                    "ActivityDetails","NumberInLastYear","Remarks"),
                T("Academic Summary", "AcademicSummaryDetail", "Id", true,
                    "PublicationsPast3Years","AssessmentDate","LiccommitteeObservations")
            })
        };

        // Friendly header text where plain camel-case splitting would read badly.
        private static readonly Dictionary<string, string> Labels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["LoPDate"] = "LoP Date",
            ["HODName"] = "HOD Name",
            ["TotalICUHDUBeds"] = "Total ICU / HDU Beds",
            ["HeadOfInstitutionAgeDob"] = "Head of Institution - Age / DOB",
            ["HeadOfInstitutionTeachingExp"] = "Head of Institution - Teaching Experience",
            ["HeadOfInstitutionPgdegree"] = "Head of Institution - PG Degree",
            ["HeadOfInstitutionPgrecognition"] = "Head of Institution - PG Recognition",
            ["Hodname"] = "HOD Name",
            ["HodageDob"] = "HOD Age / DOB",
            ["HodteachingExp"] = "HOD Teaching Experience",
            ["HodpgDegree"] = "HOD PG Degree",
            ["HodpgRecognition"] = "HOD PG Recognition",
            ["NumberOfUgseats"] = "Number of UG Seats",
            ["NameOfChairmanOrMember"] = "Name of Chairman / Member",
            ["PhoneOffResMobile"] = "Phone (Off / Res / Mobile)",
            ["PermittedByMciNmc"] = "Permitted by MCI / NMC",
            ["NoOfRoomsForConsultation"] = "No. of Consultation Rooms",
            ["WaitingAreaInSqM"] = "Waiting Area (Sq.m)",
            ["AreaInSqM"] = "Area (Sq.m)",
            ["SeparateMinorOtMaleFemale"] = "Separate Minor OT (Male / Female)",
            ["PerRectalExamRoomAvailable"] = "Per-rectal Exam Room Available",
            ["DressingRoom2Available"] = "Dressing Room 2 Available",
            ["NoOfWardsMale"] = "No. of Wards (Male)",
            ["NoOfWardsFemale"] = "No. of Wards (Female)",
            ["MajorOtTables"] = "Major OT Tables",
            ["MinorOtTables"] = "Minor OT Tables",
            ["AudiovisualEquipmentDetails"] = "Audio-visual Equipment Details",
            ["NumberOfBooksCentral"] = "Number of Books (Central)",
            ["NumberOfBooksDepartmental"] = "Number of Books (Departmental)",
            ["BooksPurchasedLast3YrsCentral"] = "Books Purchased in Last 3 Yrs (Central)",
            ["BooksPurchasedLast3YrsDept"] = "Books Purchased in Last 3 Yrs (Dept)",
            ["TotalIndianJournalsCentral"] = "Indian Journals (Central)",
            ["TotalIndianJournalsDept"] = "Indian Journals (Dept)",
            ["TotalForeignJournalsCentral"] = "Foreign Journals (Central)",
            ["TotalForeignJournalsDept"] = "Foreign Journals (Dept)",
            ["ComputerWithInternetCentral"] = "Computers with Internet (Central)",
            ["ComputerWithInternetDept"] = "Computers with Internet (Dept)",
            ["CentralLibraryTiming"] = "Central Library Timing",
            ["CentralReadingRoomTiming"] = "Central Reading Room Timing",
            ["ResearchProjectsCompletedPast3Yrs"] = "Research Projects Completed (Past 3 Yrs)",
            ["Random3Days"] = "Random 3 Days",
            ["LiccommitteeObservations"] = "LIC Observations",
            ["IsAvailable"] = "Available",
            ["IsAdequate"] = "Adequate",
            ["IsAdequateForAdmission"] = "Adequate for Admission",
            ["Email"] = "Email"
        };

        /// <summary>
        /// Loads the PG sections for the current application, grouped by department.
        /// Returns an empty list unless the applying course level is PG, so UG / SS
        /// previews and PDFs never get these sections.
        /// </summary>
        public static async Task<List<PgSectionGroup>> LoadForApplicationAsync(
            CApreviewViewModel? model, ISession session, string? connectionString)
        {
            var level = (session.GetString("CourseLevel")
                         ?? session.GetString("SelectedCourseLevel")
                         ?? model?.ApplyingCourseLevel
                         ?? string.Empty).Trim().ToUpperInvariant();

            if (level != "PG" || string.IsNullOrWhiteSpace(connectionString))
                return new List<PgSectionGroup>();

            var collegeCode = (model?.CollegeCode ?? session.GetString("CollegeCode"))?.Trim();
            var facultyCode = (model?.FacultyCode ?? session.GetString("FacultyCode"))?.Trim();

            // TypeOfAffiliation may be stored as text ("Continuation of Affiliation") or as the numeric id,
            // so match either.
            var typeKeys = new List<string>();
            void AddType(string? v) { if (!string.IsNullOrWhiteSpace(v) && !typeKeys.Contains(v.Trim(), StringComparer.OrdinalIgnoreCase)) typeKeys.Add(v.Trim()); }
            AddType(model?.ApplicationType);
            AddType(session.GetString("TypeOfAffiliation"));
            AddType(session.GetInt32("AffiliationType")?.ToString());
            AddType(session.GetString("AffiliationTypeId"));

            var courseCodes = new List<string>();
            void AddCourse(string? v) { if (!string.IsNullOrWhiteSpace(v) && !courseCodes.Contains(v.Trim(), StringComparer.OrdinalIgnoreCase)) courseCodes.Add(v.Trim()); }
            var matched = model?.PaymentCalculation?.MatchedCourses;
            if (matched != null)
                foreach (var c in matched) AddCourse(c.CourseCode);
            if (courseCodes.Count == 0)
                foreach (var c in (session.GetString("CourseCode") ?? string.Empty)
                             .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    AddCourse(c);

            var result = new List<PgSectionGroup>();
            foreach (var g in Groups)
            {
                var group = new PgSectionGroup { Anchor = g.Anchor, Number = g.Number, Title = g.Title };
                foreach (var def in g.Tables)
                    group.Tables.Add(new PgSectionTable { Title = def.Title, Headers = def.Columns.Select(Label).ToList() });
                result.Add(group);
            }

            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
                return result;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Department names for this college (course code -> course name)
            var deptNames = await LoadDepartmentNamesAsync(connection, collegeCode);

            foreach (var g in Groups)
            {
                var group = result.First(x => x.Anchor == g.Anchor);
                for (var i = 0; i < g.Tables.Length; i++)
                {
                    var def = g.Tables[i];
                    var table = group.Tables[i];
                    try
                    {
                        var (rows, rowCourses) = await ReadTableAsync(connection, def, collegeCode, facultyCode, typeKeys, courseCodes);
                        ApplyRows(group, table, def, rows, rowCourses, deptNames);
                    }
                    catch (SqlException)
                    {
                        // Table / column missing in this deployment: leave this table empty
                        // instead of breaking the whole preview.
                        table.Rows.Clear();
                    }
                }

                group.Departments = group.Departments
                    .OrderBy(d => d.DepartmentName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return result;
        }

        // Department names: Mst_MedicalCollegeCourseIntake (course) for this college,
        // falling back to Mst_Course.CourseName, then to the code itself.
        private static async Task<Dictionary<string, string>> LoadDepartmentNamesAsync(SqlConnection connection, string collegeCode)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                await using var cmd = new SqlCommand($@"
                    SELECT CAST(CourseCode AS varchar(50)) AS Code, MAX(course) AS Name
                    FROM {Db}[Mst_MedicalCollegeCourseIntake]
                    WHERE LTRIM(RTRIM(coll_code)) = @c AND CourseCode IS NOT NULL
                    GROUP BY CAST(CourseCode AS varchar(50))", connection);
                cmd.Parameters.Add("@c", SqlDbType.VarChar, 50).Value = collegeCode;

                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var code = r.IsDBNull(0) ? string.Empty : r.GetString(0).Trim();
                    var name = r.IsDBNull(1) ? string.Empty : r.GetString(1).Trim();
                    if (code.Length > 0 && name.Length > 0) names[code] = name;
                }
            }
            catch (SqlException) { /* fall through to the fallback below */ }

            try
            {
                await using var cmd = new SqlCommand($@"
                    SELECT CAST(CourseCode AS varchar(50)) AS Code, MAX(CourseName) AS Name
                    FROM {Db}[Mst_Course]
                    GROUP BY CAST(CourseCode AS varchar(50))", connection);

                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    var code = r.IsDBNull(0) ? string.Empty : r.GetString(0).Trim();
                    var name = r.IsDBNull(1) ? string.Empty : r.GetString(1).Trim();
                    if (code.Length > 0 && name.Length > 0 && !names.ContainsKey(code)) names[code] = name;
                }
            }
            catch (SqlException) { }

            return names;
        }

        private static string DeptName(Dictionary<string, string> names, string code) =>
            string.IsNullOrWhiteSpace(code) ? "General"
            : names.TryGetValue(code.Trim(), out var n) && !string.IsNullOrWhiteSpace(n) ? n : code.Trim();

        // Adds the rows to (1) the per-department blocks and (2) the flat table used by the PDF.
        private static void ApplyRows(
            PgSectionGroup group, PgSectionTable table, TableDef def,
            List<List<string>> rows, List<string> rowCourses, Dictionary<string, string> deptNames)
        {
            // 1. grouped by department
            for (var i = 0; i < rows.Count; i++)
            {
                var code = rowCourses[i];
                var block = group.Departments.FirstOrDefault(d => d.CourseCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (block == null)
                {
                    block = new PgDepartmentBlock { CourseCode = code, DepartmentName = DeptName(deptNames, code) };
                    group.Departments.Add(block);
                }

                var t = block.Tables.FirstOrDefault(x => x.Title == def.Title);
                if (t == null)
                {
                    t = new PgSectionTable { Title = def.Title, Headers = def.Columns.Select(Label).ToList() };
                    block.Tables.Add(t);
                }
                t.Rows.Add(new List<string>(rows[i]));
            }

            // 2. flat table (PDF): when several departments are present, prepend a Department column
            var distinct = rowCourses.Where(c => c.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            if (distinct > 1)
            {
                table.Headers.Insert(0, "Department");
                for (var i = 0; i < rows.Count; i++)
                    rows[i].Insert(0, DeptName(deptNames, rowCourses[i]));
            }

            table.Rows = rows;
        }

        private static async Task<(List<List<string>> rows, List<string> courses)> ReadTableAsync(
            SqlConnection connection, TableDef def,
            string collegeCode, string facultyCode, List<string> typeKeys, List<string> courseCodes)
        {
            var cols = string.Join(", ", def.Columns.Select(c => $"[{c}]"));
            var orderCols = def.Order.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var where = new StringBuilder(@"
LTRIM(RTRIM(CAST(CollegeCode AS varchar(50)))) = @c
AND LTRIM(RTRIM(CAST(FacultyCode AS varchar(50)))) = @f");

            await using var cmd = new SqlCommand { Connection = connection };
            cmd.Parameters.Add("@c", SqlDbType.VarChar, 50).Value = collegeCode;
            cmd.Parameters.Add("@f", SqlDbType.VarChar, 50).Value = facultyCode;

            if (typeKeys.Count > 0)
            {
                var names = new List<string>();
                for (var i = 0; i < typeKeys.Count; i++)
                {
                    names.Add("@t" + i);
                    cmd.Parameters.Add("@t" + i, SqlDbType.VarChar, 100).Value = typeKeys[i];
                }
                where.Append($"\nAND LTRIM(RTRIM(CAST(TypeOfAffiliation AS varchar(100)))) IN ({string.Join(",", names)})");
            }

            if (courseCodes.Count > 0)
            {
                var names = new List<string>();
                for (var i = 0; i < courseCodes.Count; i++)
                {
                    names.Add("@cc" + i);
                    cmd.Parameters.Add("@cc" + i, SqlDbType.VarChar, 50).Value = courseCodes[i];
                }
                where.Append($"\nAND LTRIM(RTRIM(CAST(CourseCode AS varchar(50)))) IN ({string.Join(",", names)})");
            }

            if (def.Latest)
            {
                cmd.CommandText = $@"
SELECT {cols}, CAST(CourseCode AS varchar(50)) AS __CourseCode
FROM (
    SELECT {cols}, CourseCode,
           ROW_NUMBER() OVER (PARTITION BY CourseCode ORDER BY [{orderCols[0]}] DESC) AS rn
    FROM {Db}[{def.Table}]
    WHERE {where}
) x
WHERE rn = 1
ORDER BY CourseCode";
            }
            else
            {
                var order = string.Join(", ", new[] { "CourseCode" }.Concat(orderCols.Select(o => $"[{o}]")));
                cmd.CommandText = $@"
SELECT {cols}, CAST(CourseCode AS varchar(50)) AS __CourseCode
FROM {Db}[{def.Table}]
WHERE {where}
ORDER BY {order}";
            }

            var rows = new List<List<string>>();
            var rowCourses = new List<string>();
            await using var reader = await cmd.ExecuteReaderAsync();
            var courseIndex = def.Columns.Length;
            while (await reader.ReadAsync())
            {
                var row = new List<string>(def.Columns.Length);
                for (var i = 0; i < def.Columns.Length; i++)
                    row.Add(Format(reader.GetValue(i)));
                rows.Add(row);
                rowCourses.Add(reader.IsDBNull(courseIndex) ? string.Empty : reader.GetString(courseIndex).Trim());
            }

            return (rows, rowCourses);
        }

        private static string Label(string column)
        {
            if (Labels.TryGetValue(column, out var label))
                return label;

            var text = Regex.Replace(column, "(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
            if (text.StartsWith("Is ", StringComparison.Ordinal))
                text = text[3..];
            return text;
        }

        private static string Format(object? value) => value switch
        {
            null => "—",
            DBNull => "—",
            bool b => b ? "Yes" : "No",
            DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("dd-MM-yyyy") : d.ToString("dd-MM-yyyy HH:mm"),
            TimeSpan t => t.ToString(@"hh\:mm"),
            decimal m => m.ToString("0.##", CultureInfo.InvariantCulture),
            double d => d.ToString("0.##", CultureInfo.InvariantCulture),
            float f => f.ToString("0.##", CultureInfo.InvariantCulture),
            string s => string.IsNullOrWhiteSpace(s) ? "—" : s.Trim(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "—"
        };
    }
}