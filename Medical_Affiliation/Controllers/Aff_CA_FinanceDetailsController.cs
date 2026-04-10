using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class Aff_CA_FinanceDetailsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public Aff_CA_FinanceDetailsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // helper to build VM (used by GET and when ModelState invalid)
        private async Task<CA_FinancialDetailsViewModel> BuildFinancialViewModel(string collegeCode, string facultyCode, string regNo)
        {
            // main record (create if missing)
            var main = await _context.CaFinancialDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (main == null)
            {
                main = new CaFinancialDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    RegistrationNo = regNo
                };

                _context.CaFinancialDetails.Add(main);
                await _context.SaveChangesAsync();
            }

            // courses master
            int fCodeInt = Convert.ToInt32(facultyCode);
            var courseList = await _context.MstCourses
                .Where(x => x.FacultyCode == fCodeInt)
                .OrderBy(x => x.CourseName)
                .ToListAsync();

            // saved rows
            var savedRows = await _context.CaCourseDetailsInFinancialDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            // build rows for view
            var rows = new List<CA_FinancialCourseRowViewModel>();
            foreach (var c in courseList)
            {
                var existing = savedRows.FirstOrDefault(x => x.CourseCode == c.CourseCode.ToString());

                rows.Add(new CA_FinancialCourseRowViewModel
                {
                    Id = existing?.Id,
                    CourseCode = c.CourseCode.ToString(),
                    CourseName = c.CourseName,
                    YearOfStarting = existing?.YearOfStarting,
                    AdmissionsSanctioned = existing?.AdmissionsSanctioned,
                    AdmissionsAdmitted = existing?.AdmissionsAdmitted,
                    // show file NAMES (file bytes are stored in DB columns)
                    GOKSanctionIntakeFileName = existing?.GoksanctionIntakeFileName,
                    ApexBodyPermissionAndIntakeFileName = existing?.ApexBodyPermissionAndIntakeFileName,
                    RGUHSSanctionIntakeFileName = existing?.RguhssanctionIntakeFileName,
                    GOIPermissionFileName = existing?.GoipermissionFileName
                });
            }

            var vm = new CA_FinancialDetailsViewModel
            {
                RegistrationNo = main.RegistrationNo,
                CollegeCode = main.CollegeCode,
                FacultyCode = main.FacultyCode,

                // populate top-form fields so they appear on the page and remain editable
                AnnualBudget = main.AnnualBudget,
                AuditedExpenditureFileName = main.AuditedExpenditureFileName,
                DepositsHeld = main.DepositsHeld,
                TuitionFee = main.TuitionFee,
                UnionFee = main.UnionFee,
                SportsFee = main.SportsFee,
                LibraryFee = main.LibraryFee,
                OthersFee = main.OthersFee,
                AccountBooksMaintained = main.AccountBooksMaintained,
                AccountsDulyAudited = main.AccountsDulyAudited,

                // rows
                Rows = rows,
                CourseMasterList = courseList,
                CourseRows = savedRows,
                MainForm = main
            };

            return vm;
        }

        // -----------------------------------------------------------
        // GET : MAIN PAGE
        // -----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> CA_FinancialDetails()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");
            string regNo = HttpContext.Session.GetString("RegistrationNo");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var vm = await BuildFinancialViewModel(collegeCode, facultyCode, regNo);
            return View(vm);
        }

        [HttpGet]
        public IActionResult ViewMainPDF(string type)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");

            var data = _context.CaFinancialDetails.FirstOrDefault(x => x.CollegeCode == collegeCode);
            if (data == null) return NotFound();

            byte[] fileBytes = null;
            string fileName = "";

            if (type == "MainAudit")
            {
                fileBytes = data.AuditedExpenditureFile;
                fileName = data.AuditedExpenditureFileName;
            }

            if (fileBytes == null) return Content("No file uploaded.");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return File(fileBytes, GetContentType(fileName));
        }

        [HttpGet]
        public async Task<IActionResult> ViewGOIPermission(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);
            if (row == null || row.GoipermissionFile == null)
                return NotFound();

            Response.Headers.Add("Content-Disposition", $"inline; filename={row.GoipermissionFileName}");

            return File(row.GoipermissionFile, GetContentType(row.GoipermissionFileName));
        }

        [HttpGet]
        public async Task<IActionResult> ViewGOKSanction(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);
            if (row == null || row.GoksanctionIntakeFile == null) return NotFound();
            Response.Headers["Content-Disposition"] = $"inline; filename={row.GoksanctionIntakeFileName}";
            return File(row.GoksanctionIntakeFile, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ViewApexBodyPermission(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);
            if (row == null || row.ApexBodyPermissionAndIntakeFile == null) return NotFound();
            Response.Headers["Content-Disposition"] = $"inline; filename={row.ApexBodyPermissionAndIntakeFileName}";
            return File(row.ApexBodyPermissionAndIntakeFile, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ViewRGUHSSanction(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);
            if (row == null || row.RguhssanctionIntakeFile == null) return NotFound();
            Response.Headers["Content-Disposition"] = $"inline; filename={row.RguhssanctionIntakeFileName}";
            return File(row.RguhssanctionIntakeFile, "application/pdf");
        }


        // -----------------------------------------------------------
        // POST : SAVE MAIN FINANCIAL FORM
        // -----------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]




        public async Task<IActionResult> SaveMainFinancialDetails(
        CA_FinancialDetailsViewModel model,
        IFormFile? AuditedExpenditureUpload)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");
            string regNo = HttpContext.Session.GetString("RegistrationNo");

            // Manual validation for file (since [Required] doesn't work on IFormFile)
            var mainRecord = await _context.CaFinancialDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if ((AuditedExpenditureUpload == null || AuditedExpenditureUpload.Length == 0) &&
                (mainRecord == null || mainRecord.AuditedExpenditureFile == null || mainRecord.AuditedExpenditureFile.Length == 0))
            {
                ModelState.AddModelError("AuditedExpenditureUpload", "Audited expenditure statement PDF is required.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate full view model
                var vm = await BuildFinancialViewModel(collegeCode, facultyCode, regNo);
                // Copy user-entered values back
                vm.AnnualBudget = model.AnnualBudget;
                vm.DepositsHeld = model.DepositsHeld;
                vm.TuitionFee = model.TuitionFee;
                vm.UnionFee = model.UnionFee;
                vm.SportsFee = model.SportsFee;
                vm.LibraryFee = model.LibraryFee;
                vm.OthersFee = model.OthersFee;
                vm.AccountBooksMaintained = model.AccountBooksMaintained;
                vm.AccountsDulyAudited = model.AccountsDulyAudited;
                return View("CA_FinancialDetails", vm);
            }

            var db = await _context.CaFinancialDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (db == null)
            {
                db = new CaFinancialDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    RegistrationNo = regNo
                };
                _context.CaFinancialDetails.Add(db);
            }

            db.AnnualBudget = model.AnnualBudget;
            db.DepositsHeld = model.DepositsHeld;
            db.TuitionFee = model.TuitionFee;
            db.UnionFee = model.UnionFee;
            db.SportsFee = model.SportsFee;
            db.LibraryFee = model.LibraryFee;
            db.OthersFee = model.OthersFee;
            db.AccountBooksMaintained = model.AccountBooksMaintained;
            db.AccountsDulyAudited = model.AccountsDulyAudited;

            if (AuditedExpenditureUpload != null && AuditedExpenditureUpload.Length > 0)
            {
                using var ms = new MemoryStream();
                await AuditedExpenditureUpload.CopyToAsync(ms);
                db.AuditedExpenditureFile = ms.ToArray();
                db.AuditedExpenditureFileName = AuditedExpenditureUpload.FileName;
            }

            _context.Entry(db).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Financial Details Saved Successfully!";
            return RedirectToAction("CA_FinancialDetails");
        }


        // -----------------------------------------------------------
        // POST : SAVE COURSE ROW (ADD or UPDATE) — stores file bytes in DB
        // -----------------------------------------------------------
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SaveCourseRow(
        //    CA_FinancialCourseRowViewModel model,
        //    IFormFile? GOKSanctionIntakeFile,
        //    IFormFile? ApexBodyPermissionAndIntakeFile,
        //    IFormFile? RGUHSSanctionIntakeFile,
        //    IFormFile? GOIPermissionFile
        //)
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");
        //    string regNo = HttpContext.Session.GetString("RegistrationNo");

        //    if (!ModelState.IsValid)
        //    {
        //        TempData["Error"] = "Please fix validation errors.";
        //        return RedirectToAction("CA_FinancialDetails");
        //    }

        //    CaCourseDetailsInFinancialDetail row;

        //    // --- Update Existing Row ---
        //    if (model.Id.HasValue && model.Id > 0)
        //    {
        //        row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(model.Id.Value);
        //        if (row == null)
        //            return RedirectToAction("CA_FinancialDetails");
        //    }
        //    else
        //    {
        //        // --- New Row ---
        //        row = new CaCourseDetailsInFinancialDetail
        //        {
        //            CollegeCode = collegeCode,
        //            FacultyCode = facultyCode,
        //            RegistrationNo = regNo,
        //            CourseCode = model.CourseCode
        //        };

        //        _context.CaCourseDetailsInFinancialDetails.Add(row);
        //    }

        //    // ---- Update common fields ----
        //    row.YearOfStarting = model.YearOfStarting;
        //    row.AdmissionsSanctioned = model.AdmissionsSanctioned;
        //    row.AdmissionsAdmitted = model.AdmissionsAdmitted;

        //    // ------------------ FILES: store in DB (varbinary) + save filename ------------------

        //    // 1. GOK Sanction Intake File
        //    if (GOKSanctionIntakeFile != null)
        //    {
        //        using var ms = new MemoryStream();
        //        await GOKSanctionIntakeFile.CopyToAsync(ms);
        //        row.GoksanctionIntakeFile = ms.ToArray(); // store bytes
        //        row.GoksanctionIntakeFileName = GOKSanctionIntakeFile.FileName;
        //    }

        //    // 2. Apex Body Permission
        //    if (ApexBodyPermissionAndIntakeFile != null)
        //    {
        //        using var ms = new MemoryStream();
        //        await ApexBodyPermissionAndIntakeFile.CopyToAsync(ms);
        //        row.ApexBodyPermissionAndIntakeFile = ms.ToArray();
        //        row.ApexBodyPermissionAndIntakeFileName = ApexBodyPermissionAndIntakeFile.FileName;
        //    }

        //    // 3. RGUHS Sanction File
        //    if (RGUHSSanctionIntakeFile != null)
        //    {
        //        using var ms = new MemoryStream();
        //        await RGUHSSanctionIntakeFile.CopyToAsync(ms);
        //        row.RguhssanctionIntakeFile = ms.ToArray();
        //        row.RguhssanctionIntakeFileName = RGUHSSanctionIntakeFile.FileName;
        //    }

        //    // 4. GOI Permission File
        //    if (GOIPermissionFile != null)
        //    {
        //        using var ms = new MemoryStream();
        //        await GOIPermissionFile.CopyToAsync(ms);
        //        row.GoipermissionFile = ms.ToArray();
        //        row.GoipermissionFileName = GOIPermissionFile.FileName;
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Course row saved successfully!";
        //    return RedirectToAction("CA_FinancialDetails");
        //}

        // -----------------------------------------------------------
        // DELETE COURSE ROW (kept GET for simple link navigation)
        // -----------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> DeleteCourseRow(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);

            if (row != null)
            {
                _context.CaCourseDetailsInFinancialDetails.Remove(row);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CA_FinancialDetails");
        }

        private async Task<IActionResult> GetPdf(string type)
        {
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;
            var level = CourseLevel; // ✅ ADD THIS

            var record = await _context.MedCaAccountAndFeeDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == level); // ✅ ADD CourseLevel filter

            if (record == null) return NotFound();

            byte[]? file = type switch
            {
                "GoverningCouncil" => record.GoverningCouncilPdf,
                "AccountSummary" => record.AccountSummaryPdf,
                "AuditedStatement" => record.AuditedStatementPdf,
                _ => null
            };

            string? name = type switch
            {
                "GoverningCouncil" => record.GoverningCouncilPdfName,
                "AccountSummary" => record.AccountSummaryPdfName,
                "AuditedStatement" => record.AuditedStatementPdfName,
                _ => null
            };

            if (file == null || string.IsNullOrEmpty(name)) return NotFound();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{name}\"";
            return File(file, "application/pdf");
        }

        // -----------------------------------------------------------
        // DOWNLOADS — fetch bytes from DB and return File(...)
        // -----------------------------------------------------------





        [HttpGet]
        public async Task<IActionResult> DownloadGOIPermission(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);
            if (row == null || row.GoipermissionFileName == null) return NotFound();
            return File(row.GoipermissionFile, "application/pdf", row.GoipermissionFileName);
        }

        // Download for top audited file
        [HttpGet]
        public async Task<IActionResult> DownloadAuditedExpenditureFile()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            var main = await _context.CaFinancialDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (main == null || main.AuditedExpenditureFile == null) return NotFound();
            return File(main.AuditedExpenditureFile, "application/pdf", main.AuditedExpenditureFileName);
        }

        private string GetContentType(string fileName)
        {
            if (fileName == null) return "application/octet-stream";

            string ext = Path.GetExtension(fileName).ToLower();

            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        [HttpGet]
        public async Task<IActionResult> DownloadGOKSanction(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);

            if (row == null || row.GoksanctionIntakeFile == null)
                return NotFound("GOK Sanction file not found.");

            Response.Headers.Add("Content-Disposition", $"inline; filename={row.GoksanctionIntakeFileName}");

            return File(row.GoksanctionIntakeFile, GetContentType(row.GoksanctionIntakeFileName));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadApexBodyPermission(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);

            if (row == null || row.ApexBodyPermissionAndIntakeFile == null)
                return NotFound("Apex Body Permission file not found.");

            Response.Headers.Add("Content-Disposition", $"inline; filename={row.ApexBodyPermissionAndIntakeFileName}");

            return File(row.ApexBodyPermissionAndIntakeFile,
                        GetContentType(row.ApexBodyPermissionAndIntakeFileName));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadRGUHSSanction(int id)
        {
            var row = await _context.CaCourseDetailsInFinancialDetails.FindAsync(id);

            if (row == null || row.RguhssanctionIntakeFile == null)
                return NotFound("RGUHS Sanction file not found.");

            Response.Headers.Add("Content-Disposition", $"inline; filename={row.RguhssanctionIntakeFileName}");

            return File(row.RguhssanctionIntakeFile,
                        GetContentType(row.RguhssanctionIntakeFileName));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]



        public async Task<IActionResult> SaveCourseRow(
        CA_FinancialCourseRowViewModel model,
        IFormFile? GOKSanctionIntakeFile,
        IFormFile? ApexBodyPermissionAndIntakeFile,
        IFormFile? RGUHSSanctionIntakeFile,
        IFormFile? GOIPermissionFile)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");
            string regNo = HttpContext.Session.GetString("RegistrationNo");

            // === Validation ===
            var rowErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.YearOfStarting) ||
                model.YearOfStarting.Length != 4 ||
                !int.TryParse(model.YearOfStarting, out _))
            {
                rowErrors.Add("Valid 4-digit Year of Starting is required.");
            }

            if (!model.AdmissionsSanctioned.HasValue || model.AdmissionsSanctioned < 0)
            {
                rowErrors.Add("Admissions Sanctioned must be 0 or greater.");
            }

            if (!model.AdmissionsAdmitted.HasValue || model.AdmissionsAdmitted < 0)
            {
                rowErrors.Add("Admissions Admitted must be 0 or greater.");
            }

            // === File validation (make all 4 PDFs required) ===
            if (GOKSanctionIntakeFile == null || GOKSanctionIntakeFile.Length == 0)
                rowErrors.Add("GOK Sanction Intake PDF is required.");

            if (ApexBodyPermissionAndIntakeFile == null || ApexBodyPermissionAndIntakeFile.Length == 0)
                rowErrors.Add("Apex Body Permission PDF is required.");

            if (RGUHSSanctionIntakeFile == null || RGUHSSanctionIntakeFile.Length == 0)
                rowErrors.Add("RGUHS Sanction Intake PDF is required.");

            if (GOIPermissionFile == null || GOIPermissionFile.Length == 0)
                rowErrors.Add("GOI Permission PDF is required.");

            // === If validation fails ===
            if (rowErrors.Any())
            {
                string rowKey = model.Id.HasValue && model.Id > 0 ? $"row_{model.Id}" : $"new_{model.CourseCode}";

                if (ViewBag.RowErrors == null)
                    ViewBag.RowErrors = new Dictionary<string, List<string>>();

                ViewBag.RowErrors[rowKey] = rowErrors;

                var vm = await BuildFinancialViewModel(collegeCode, facultyCode, regNo ?? "");
                return View("CA_FinancialDetails", vm);
            }

            // === Save logic ===
            CaCourseDetailsInFinancialDetail row;

            bool isNew = !model.Id.HasValue || model.Id <= 0;

            if (!isNew)
            {
                // Editing existing row
                row = await _context.CaCourseDetailsInFinancialDetails
                    .FirstOrDefaultAsync(x => x.Id == model.Id.Value);

                if (row == null)
                {
                    TempData["Error"] = "Course row not found.";
                    return RedirectToAction("CA_FinancialDetails");
                }
            }
            else
            {
                // Adding new row
                row = new CaCourseDetailsInFinancialDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    RegistrationNo = regNo,
                    CourseCode = model.CourseCode
                };
                _context.CaCourseDetailsInFinancialDetails.Add(row);
            }

            // Update fields
            row.YearOfStarting = model.YearOfStarting?.Trim();
            row.AdmissionsSanctioned = model.AdmissionsSanctioned;
            row.AdmissionsAdmitted = model.AdmissionsAdmitted;

            // === Upload files (all 4 are now required and validated above) ===
            using var ms1 = new MemoryStream();
            await GOKSanctionIntakeFile.CopyToAsync(ms1);
            row.GoksanctionIntakeFile = ms1.ToArray();
            row.GoksanctionIntakeFileName = GOKSanctionIntakeFile.FileName;

            using var ms2 = new MemoryStream();
            await ApexBodyPermissionAndIntakeFile.CopyToAsync(ms2);
            row.ApexBodyPermissionAndIntakeFile = ms2.ToArray();
            row.ApexBodyPermissionAndIntakeFileName = ApexBodyPermissionAndIntakeFile.FileName;

            using var ms3 = new MemoryStream();
            await RGUHSSanctionIntakeFile.CopyToAsync(ms3);
            row.RguhssanctionIntakeFile = ms3.ToArray();
            row.RguhssanctionIntakeFileName = RGUHSSanctionIntakeFile.FileName;

            using var ms4 = new MemoryStream();
            await GOIPermissionFile.CopyToAsync(ms4);
            row.GoipermissionFile = ms4.ToArray();
            row.GoipermissionFileName = GOIPermissionFile.FileName;

            // === Correct state handling ===
            if (isNew)
            {
                // Let EF handle insert automatically — do NOT set Modified
                // Entity is already in Added state from .Add(row)
            }
            else
            {
                // Only set Modified for existing rows
                _context.Entry(row).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Course details saved successfully!";
            return RedirectToAction("CA_FinancialDetails");
        }

    }
}
