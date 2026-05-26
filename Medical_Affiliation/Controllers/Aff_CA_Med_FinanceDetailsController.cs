using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;
using System.IO;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    public class Aff_CA_Med_FinanceDetailsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public Aff_CA_Med_FinanceDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Med_CA_AccountAndFeeDetails()
        {
            //var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            //var regNo = HttpContext.Session.GetString("RegistrationNo");

            // First try from CollegeCourseIntakeDetails
            var levels = await (
                from cc in _context.CollegeCourseIntakeDetails
                join cm in _context.MstCourses
                    on cc.CourseCode equals cm.CourseCode.ToString()
                where cc.CollegeCode == CollegeCode
                select cm.CourseLevel
            )
            .Distinct()
            .ToListAsync();

            // If no levels found, then take from AcademicIntake
            if (!levels.Any())
            {
                levels = await (
                    from ai in _context.AcademicIntakes
                    join cm in _context.MstCourses
                        on ai.Courses equals cm.CourseCode.ToString()
                    where ai.CollegeCode == CollegeCode
                          && !string.IsNullOrEmpty(ai.Courses)
                    select cm.CourseLevel
                )
                .Distinct()
                .ToListAsync();
            }

            levels = levels
                .OrderBy(l => l == "UG" ? 1 :
                              l == "PG" ? 2 :
                              l == "SS" ? 3 : 99)
                .ToList();


            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            //var acc = await _context.MedCaAccountAndFeeDetails
            //    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            var vm = new Med_CA_AccountAndFeeDetailsPageVM();
            var accList = await _context.MedCaAccountAndFeeDetails
                                .Where(x => x.CollegeCode == collegeCode
                                         && x.FacultyCode == facultyCode)
                                .ToListAsync();

            foreach (var level in levels)
            {
                var data = accList.FirstOrDefault(x =>
                    x.CourseLevel != null &&
                    x.CourseLevel.Trim().ToUpper() == level
                );

                vm.Sections.Add(new Med_CA_AccountAndFeeDetailsViewModel
                {
                    CourseLevel = level,
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    //RegistrationNo = regNo,

                    AuthorityNameAddress = data?.AuthorityNameAddress ?? "",
                    AuthorityContact = data?.AuthorityContact ?? "",
                    RecurrentAnnual = data?.RecurrentAnnual,
                    NonRecurrentAnnual = data?.NonRecurrentAnnual,
                    Deposits = data?.Deposits,
                    TuitionFee = data?.TuitionFee,
                    SportsFee = data?.SportsFee,
                    UnionFee = data?.UnionFee,
                    LibraryFee = data?.LibraryFee,
                    OtherFee = data?.OtherFee,
                    TotalFee = data?.TotalFee ?? 0,
                    AccountBooksMaintained = data?.AccountBooksMaintained ?? "",
                    AccountsAudited = data?.AccountsAudited ?? "",
                    DonationLevied = data?.DonationLevied ?? "",

                    GoverningCouncilPdfName = data?.GoverningCouncilPdfName,
                    AccountSummaryPdfName = data?.AccountSummaryPdfName,
                    AuditedStatementPdfName = data?.AuditedStatementPdfName,
                    DonationPdfName = data?.DonationPdfName
                });
            }

            //ModelState.Clear()/*;*/
            return View("Med_CA_FinanceDetails", vm);
            //return View("Med_CA_FinanceDetails", new Med_CA_AccountAndFeeDetailsPageVM()); // Simplified for brevity, keep your full code
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Med_CA_AccountAndFeeDetails(
                                    Med_CA_AccountAndFeeDetailsPageVM model,
                                    IFormFile? GoverningCouncilPdf,
                                    IFormFile? AccountSummaryPdf,
                                    IFormFile? AuditedStatementPdf,
                                    IFormFile? DonationPdf)
        {
            //var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Login");


            // Remove validation for session fields
            ModelState.Remove("CollegeCode");
            ModelState.Remove("FacultyCode");
            ModelState.Remove("CourseLevel");

            // ===============================
            // 🔥 LOOP THROUGH EACH SECTION
            // ===============================
            foreach (var item in model.Sections)
            {
                var courseLevel = item.CourseLevel?.Trim().ToUpper();

                // ===============================
                // FETCH EXISTING RECORD
                // ===============================
                var db = await _context.MedCaAccountAndFeeDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == courseLevel
                    );

                bool isNew = db == null;

                if (isNew)
                {
                    db = new MedCaAccountAndFeeDetail
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel
                    };

                    _context.MedCaAccountAndFeeDetails.Add(db);
                }

                // ===============================
                // 🔥 NORMAL FIELD UPDATE
                // ===============================
                db.AuthorityNameAddress = item.AuthorityNameAddress;
                db.AuthorityContact = item.AuthorityContact;

                db.RecurrentAnnual = item.RecurrentAnnual ?? 0m;
                db.NonRecurrentAnnual = item.NonRecurrentAnnual ?? 0m;
                db.Deposits = item.Deposits ?? 0m;

                db.TuitionFee = item.TuitionFee ?? 0m;
                db.SportsFee = item.SportsFee ?? 0m;
                db.UnionFee = item.UnionFee ?? 0m;
                db.LibraryFee = item.LibraryFee ?? 0m;
                db.OtherFee = item.OtherFee ?? 0m;

                db.TotalFee =
                    (item.TuitionFee ?? 0m) +
                    (item.SportsFee ?? 0m) +
                    (item.UnionFee ?? 0m) +
                    (item.LibraryFee ?? 0m) +
                    (item.OtherFee ?? 0m);

                db.AccountBooksMaintained = item.AccountBooksMaintained;
                db.AccountsAudited = item.AccountsAudited;

                // PG only
                if (courseLevel == "PG")
                    db.DonationLevied = item.DonationLevied;
                else
                    db.DonationLevied = null;

                // ===============================
                // 🔥 FILE HANDLING
                // ===============================

                // 1. Governing Council PDF
                if (item.GoverningCouncilPdf != null && item.GoverningCouncilPdf.Length > 0)
                {
                    var path = await SaveFinanceFileAsync(item.GoverningCouncilPdf, "GoverningCouncil");

                    if (!string.IsNullOrEmpty(db.GoverningCouncilPdfPath) &&
                        System.IO.File.Exists(db.GoverningCouncilPdfPath))
                    {
                        System.IO.File.Delete(db.GoverningCouncilPdfPath);
                    }

                    db.GoverningCouncilPdfPath = path;
                    db.GoverningCouncilPdfName = item.GoverningCouncilPdf.FileName;
                }

                // 2. Account Summary PDF
                if (item.AccountBooksMaintained == "N")
                {
                    if (!string.IsNullOrEmpty(db.AccountSummaryPdfPath) &&
                        System.IO.File.Exists(db.AccountSummaryPdfPath))
                    {
                        System.IO.File.Delete(db.AccountSummaryPdfPath);
                    }

                    db.AccountSummaryPdfPath = null;
                    db.AccountSummaryPdfName = null;
                }
                else if (item.AccountSummaryPdf != null && item.AccountSummaryPdf.Length > 0)
                {
                    var path = await SaveFinanceFileAsync(item.AccountSummaryPdf, "AccountSummary");

                    if (!string.IsNullOrEmpty(db.AccountSummaryPdfPath) &&
                        System.IO.File.Exists(db.AccountSummaryPdfPath))
                    {
                        System.IO.File.Delete(db.AccountSummaryPdfPath);
                    }

                    db.AccountSummaryPdfPath = path;
                    db.AccountSummaryPdfName = item.AccountSummaryPdf.FileName;
                }

                // 3. Audited Statement PDF
                if (item.AccountsAudited == "N")
                {
                    if (!string.IsNullOrEmpty(db.AuditedStatementPdfPath) &&
                        System.IO.File.Exists(db.AuditedStatementPdfPath))
                    {
                        System.IO.File.Delete(db.AuditedStatementPdfPath);
                    }

                    db.AuditedStatementPdfPath = null;
                    db.AuditedStatementPdfName = null;
                }
                else if (item.AuditedStatementPdf != null && item.AuditedStatementPdf.Length > 0)
                {
                    var path = await SaveFinanceFileAsync(item.AuditedStatementPdf, "AuditedStatements");

                    if (!string.IsNullOrEmpty(db.AuditedStatementPdfPath) &&
                        System.IO.File.Exists(db.AuditedStatementPdfPath))
                    {
                        System.IO.File.Delete(db.AuditedStatementPdfPath);
                    }

                    db.AuditedStatementPdfPath = path;
                    db.AuditedStatementPdfName = item.AuditedStatementPdf.FileName;
                }

                // 4. Donation PDF (PG only)
                if (courseLevel == "PG")
                {
                    if (item.DonationLevied == "N")
                    {
                        if (!string.IsNullOrEmpty(db.DonationPdfPath) &&
                            System.IO.File.Exists(db.DonationPdfPath))
                        {
                            System.IO.File.Delete(db.DonationPdfPath);
                        }

                        db.DonationPdfPath = null;
                        db.DonationPdfName = null;
                    }
                    else if (item.DonationPdf != null && item.DonationPdf.Length > 0)
                    {
                        var path = await SaveFinanceFileAsync(item.DonationPdf, "DonationDocs");

                        if (!string.IsNullOrEmpty(db.DonationPdfPath) &&
                            System.IO.File.Exists(db.DonationPdfPath))
                        {
                            System.IO.File.Delete(db.DonationPdfPath);
                        }

                        db.DonationPdfPath = path;
                        db.DonationPdfName = item.DonationPdf.FileName;
                    }
                }
            }

            // ===============================
            // SAVE ALL
            // ===============================
            await _context.SaveChangesAsync();

            ContinuousAffiliationController.MarkDone(HttpContext, "FinancialDetails");

            //return RedirectToAction(nameof(Med_CA_AccountAndFeeDetails));
            return RedirectToAction(nameof(Med_CA_AccountAndFeeDetails));
        }



        // View PDF actions (keep these)
        [HttpGet]
        public async Task<IActionResult> ViewGoverningCouncilPdf(string courseLevel)
        {
            return await GetPdf("GoverningCouncil", courseLevel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAccountSummaryPdf(string courseLevel)
        {
            return await GetPdf("AccountSummary", courseLevel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAuditedStatementPdf(string courseLevel)
        {
            return await GetPdf("AuditedStatement", courseLevel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewDonationPdf(string courseLevel)
        {
            return await GetPdf("Donation", courseLevel);
        }

        private async Task<IActionResult> GetPdf(string type, string courseLevel)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(courseLevel))
                return NotFound("Course level not specified.");

            var record = await _context.MedCaAccountAndFeeDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel);

            if (record == null) return NotFound("Record not found.");

            string? filePath = type switch
            {
                "GoverningCouncil" => record.GoverningCouncilPdfPath,
                "AccountSummary" => record.AccountSummaryPdfPath,
                "AuditedStatement" => record.AuditedStatementPdfPath,
                "Donation" => record.DonationPdfPath,
                _ => null
            };

            string? name = type switch
            {
                "GoverningCouncil" => record.GoverningCouncilPdfName,
                "AccountSummary" => record.AccountSummaryPdfName,
                "AuditedStatement" => record.AuditedStatementPdfName,
                "Donation" => record.DonationPdfName,
                _ => null
            };

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found on server.");

            var fileName = string.IsNullOrEmpty(name) ? Path.GetFileName(filePath) : name;
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return PhysicalFile(filePath, contentType);
        }
        private async Task<string?> SaveFinanceFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // 1. Get a valid root path (handles E: -> D: -> C: -> Live Server)
                string rootPath = GetDynamicRootPath();

                // 2. Combine with "FinanceDetails" and the specific subfolder
                // Result: E:\Affiliation_Medical\FinanceDetails\GoverningCouncil
                string fullFolderPath = Path.Combine(rootPath, "FinanceDetails", folder);

                // 3. Create all directories in the path if they don't exist
                if (!Directory.Exists(fullFolderPath))
                {
                    Directory.CreateDirectory(fullFolderPath);
                }

                // 4. Generate unique filename
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fullFilePath = Path.Combine(fullFolderPath, fileName);

                // 5. Save the file
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fullFilePath;
            }
            catch (Exception ex)
            {
                // Log the error and return null so the application doesn't crash
                Console.WriteLine($"File Upload Error: {ex.Message}");
                return null;
            }
        }

        private string GetDynamicRootPath()
        {
            string folderName = "Affiliation_Medical";
            string[] drivePriorities = { "E:\\", "D:\\", "C:\\" };

            // 1. Try preferred local drives first
            foreach (var drive in drivePriorities)
            {
                try
                {
                    // Check if drive exists and is accessible
                    if (Directory.Exists(drive))
                    {
                        string targetPath = Path.Combine(drive, folderName);

                        // Try to create the folder to check for write permissions
                        if (!Directory.Exists(targetPath))
                        {
                            Directory.CreateDirectory(targetPath);
                        }
                        return targetPath;
                    }
                }
                catch { /* Ignore and try next drive */ }
            }

            // 2. FALLBACK: If no specific drive is available (typical for Live Hosting),
            //brainchild use the Application's base directory.
    // This ensures the app works on any server without needing a specific drive letter.
    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", folderName);
        }

    }
}