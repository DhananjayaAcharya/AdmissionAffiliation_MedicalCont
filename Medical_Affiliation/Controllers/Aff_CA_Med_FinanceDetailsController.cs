using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;
using System.IO;

namespace Medical_Affiliation.Controllers
{
    public class Aff_CA_Med_FinanceDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Aff_CA_Med_FinanceDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Med_CA_AccountAndFeeDetails()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var regNo = HttpContext.Session.GetString("RegistrationNo");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var acc = await _context.MedCaAccountAndFeeDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            var vm = acc == null
                ? new Med_CA_AccountAndFeeDetailsViewModel
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    RegistrationNo = regNo,
                    CourseLevel = courseLevel

                }
                : new Med_CA_AccountAndFeeDetailsViewModel
                {
                    CollegeCode = acc.CollegeCode,
                    FacultyCode = acc.FacultyCode,
                    CourseLevel = acc.CourseLevel,
                    SubFacultyCode = acc.SubFacultyCode,
                    RegistrationNo = acc.RegistrationNo,
                    AuthorityNameAddress = acc.AuthorityNameAddress ?? "",
                    AuthorityContact = acc.AuthorityContact ?? "",
                    RecurrentAnnual = acc.RecurrentAnnual,
                    NonRecurrentAnnual = acc.NonRecurrentAnnual,
                    Deposits = acc.Deposits,
                    TuitionFee = acc.TuitionFee,
                    SportsFee = acc.SportsFee,
                    UnionFee = acc.UnionFee,
                    LibraryFee = acc.LibraryFee,

                    OtherFee = acc.OtherFee,
                    TotalFee = acc.TotalFee,
                    AccountBooksMaintained = acc.AccountBooksMaintained ?? "",
                    AccountsAudited = acc.AccountsAudited ?? "",
                    GoverningCouncilPdfName = acc.GoverningCouncilPdfName,
                    AccountSummaryPdfName = acc.AccountSummaryPdfName,
                    AuditedStatementPdfName = acc.AuditedStatementPdfName,
                    DonationLevied = acc.DonationLevied ?? "",
                    DonationPdfName = acc.DonationPdfName
                };

            // ★★★ THIS FIXES THE ERROR ★★★
            // Explicitly tell it to use your actual view file name

            ModelState.Clear();
            return View("Med_CA_FinanceDetails", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Med_CA_AccountAndFeeDetails(
                                    Med_CA_AccountAndFeeDetailsViewModel model,
                                    IFormFile? GoverningCouncilPdf,
                                    IFormFile? AccountSummaryPdf,
                                    IFormFile? AuditedStatementPdf,
                                    IFormFile? DonationPdf)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(courseLevel))
                return RedirectToAction("Login", "Login");

            // Force set from session
            model.CollegeCode = collegeCode;
            model.FacultyCode = facultyCode;
            model.CourseLevel = courseLevel;

            // Remove validation for session fields
            ModelState.Remove("CollegeCode");
            ModelState.Remove("FacultyCode");
            ModelState.Remove("CourseLevel");

            // Get existing record first (needed to validate file requirements)
            var db = await _context.MedCaAccountAndFeeDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            bool isNew = db == null;

            // ==========================================================
            // ✅ SERVER-SIDE VALIDATION (FILES)
            // ==========================================================

            // ✅ 1) Governing Council PDF is REQUIRED if DB doesn't already have it
            bool alreadyHasGoverningCouncilPdf =
                db != null &&
                db.GoverningCouncilPdf != null &&
                db.GoverningCouncilPdf.Length > 0;

            if (!alreadyHasGoverningCouncilPdf)
            {
                if (GoverningCouncilPdf == null || GoverningCouncilPdf.Length == 0)
                {
                    ModelState.AddModelError("GoverningCouncilPdf", "Governing Council PDF is required.");
                }
            }

            // ✅ 2) If AccountBooksMaintained = Y then AccountSummaryPdf required (if not already in DB)
            bool alreadyHasAccountSummaryPdf =
                db != null &&
                db.AccountSummaryPdf != null &&
                db.AccountSummaryPdf.Length > 0;

            if (model.AccountBooksMaintained == "Y" && !alreadyHasAccountSummaryPdf)
            {
                if (AccountSummaryPdf == null || AccountSummaryPdf.Length == 0)
                {
                    ModelState.AddModelError("AccountSummaryPdf", "Account Summary PDF is required when YES is selected.");
                }
            }

            // ✅ 3) If AccountsAudited = Y then AuditedStatementPdf required (if not already in DB)
            bool alreadyHasAuditedPdf =
                db != null &&
                db.AuditedStatementPdf != null &&
                db.AuditedStatementPdf.Length > 0;

            if (model.AccountsAudited == "Y" && !alreadyHasAuditedPdf)
            {
                if (AuditedStatementPdf == null || AuditedStatementPdf.Length == 0)
                {
                    ModelState.AddModelError("AuditedStatementPdf", "Audited Statement PDF is required when YES is selected.");
                }
            }

            // 4. Donation PDF (Only for PG and only if DonationLevied = Y)
            if (courseLevel?.ToUpper() == "PG")
            {
                bool hasDonationPdf = db?.DonationPdf != null && db.DonationPdf.Length > 0;

                if (model.DonationLevied == "Y" && !hasDonationPdf)
                {
                    if (DonationPdf == null || DonationPdf.Length == 0)
                    {
                        ModelState.AddModelError("DonationPdf", "Donation related document is required when 'Yes' is selected.");
                    }
                }
            }

            // ==========================================================
            // ✅ STOP IF VALIDATION FAILS
            // ==========================================================
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fix the errors and submit again.";
                return View("Med_CA_FinanceDetails", model);
            }

            // ==========================================================
            // ✅ INSERT IF NEW
            // ==========================================================
            if (isNew)
            {
                db = new MedCaAccountAndFeeDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = courseLevel,
                };

                _context.MedCaAccountAndFeeDetails.Add(db);
            }

            if (db != null)
            {
                if (courseLevel?.ToUpper() == "PG")
                    db.DonationLevied = model.DonationLevied;
                else
                    db.DonationLevied = null;
            }

            // ==========================================================
            // ✅ UPDATE NORMAL FIELDS
            // ==========================================================
            db.AuthorityNameAddress = model.AuthorityNameAddress;
            db.AuthorityContact = model.AuthorityContact;

            //db.RecurrentAnnual = model.RecurrentAnnual;
            //db.NonRecurrentAnnual = model.NonRecurrentAnnual;
            //db.Deposits = model.Deposits;

            //db.TuitionFee = model.TuitionFee;
            //db.SportsFee = model.SportsFee;
            //db.UnionFee = model.UnionFee;
            //db.LibraryFee = model.LibraryFee;
            //db.OtherFee = model.OtherFee;

            db.RecurrentAnnual = model.RecurrentAnnual ?? 0m;
            db.NonRecurrentAnnual = model.NonRecurrentAnnual ?? 0m;
            db.Deposits = model.Deposits ?? 0m;

            db.TuitionFee = model.TuitionFee ?? 0m;
            db.SportsFee = model.SportsFee ?? 0m;
            db.UnionFee = model.UnionFee ?? 0m;
            db.LibraryFee = model.LibraryFee ?? 0m;
            db.OtherFee = model.OtherFee ?? 0m;



            //db.TotalFee = model.TuitionFee + model.SportsFee + model.UnionFee + model.LibraryFee + model.OtherFee;

            db.TotalFee =
            (model.TuitionFee ?? 0m) +
            (model.SportsFee ?? 0m) +
            (model.UnionFee ?? 0m) +
            (model.LibraryFee ?? 0m) +
            (model.OtherFee ?? 0m);



            db.AccountBooksMaintained = model.AccountBooksMaintained;
            db.AccountsAudited = model.AccountsAudited;

            // ==========================================================
            // ✅ EXPLICIT FILE SAVE LOGIC
            // ==========================================================

            // Governing Council PDF (save only if new uploaded)
            if (GoverningCouncilPdf != null && GoverningCouncilPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await GoverningCouncilPdf.CopyToAsync(ms);
                db.GoverningCouncilPdf = ms.ToArray();
                db.GoverningCouncilPdfName = GoverningCouncilPdf.FileName;
            }

            // Account Summary PDF
            if (model.AccountBooksMaintained == "N")
            {
                db.AccountSummaryPdf = null;
                db.AccountSummaryPdfName = null;
            }
            else if (AccountSummaryPdf != null && AccountSummaryPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await AccountSummaryPdf.CopyToAsync(ms);
                db.AccountSummaryPdf = ms.ToArray();
                db.AccountSummaryPdfName = AccountSummaryPdf.FileName;
            }

            // Audited Statement PDF
            if (model.AccountsAudited == "N")
            {
                db.AuditedStatementPdf = null;
                db.AuditedStatementPdfName = null;
            }
            else if (AuditedStatementPdf != null && AuditedStatementPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await AuditedStatementPdf.CopyToAsync(ms);
                db.AuditedStatementPdf = ms.ToArray();
                db.AuditedStatementPdfName = AuditedStatementPdf.FileName;
            }

            // Donation PDF (PG only)
            if (courseLevel?.ToUpper() == "PG")
            {
                if (model.DonationLevied == "N")
                {
                    db.DonationPdf = null;
                    db.DonationPdfName = null;
                }
                else if (DonationPdf != null && DonationPdf.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await DonationPdf.CopyToAsync(ms);
                    db.DonationPdf = ms.ToArray();
                    db.DonationPdfName = DonationPdf.FileName;
                }
            }

            await _context.SaveChangesAsync();
            ContinuousAffiliationController.MarkDone(HttpContext, "FinancialDetails");

            TempData["Info"] = "Account and fee details saved successfully!";
            return RedirectToAction(nameof(Med_CA_AccountAndFeeDetails));
        }



        // View PDF actions (keep these)
        [HttpGet]
        public async Task<IActionResult> ViewGoverningCouncilPdf()
        {
            return await GetPdf("GoverningCouncil");
        }

        [HttpGet]
        public async Task<IActionResult> ViewAccountSummaryPdf()
        {
            return await GetPdf("AccountSummary");
        }

        [HttpGet]
        public async Task<IActionResult> ViewAuditedStatementPdf()
        {
            return await GetPdf("AuditedStatement");
        }

        [HttpGet]
        public async Task<IActionResult> ViewDonationPdf()
        {
            return await GetPdf("Donation");
        }

        private async Task<IActionResult> GetPdf(string type)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var record = await _context.MedCaAccountAndFeeDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (record == null) return NotFound();

            byte[]? file = type switch
            {
                "GoverningCouncil" => record.GoverningCouncilPdf,
                "AccountSummary" => record.AccountSummaryPdf,
                "AuditedStatement" => record.AuditedStatementPdf,
                "Donation" => record.DonationPdf,
                _ => null
            };

            string? name = type switch
            {
                "GoverningCouncil" => record.GoverningCouncilPdfName,
                "AccountSummary" => record.AccountSummaryPdfName,
                "AuditedStatement" => record.AuditedStatementPdfName,
                "Donation" => record.DonationPdfName,  // ✅ ADDED
                _ => null
            };

            if (file == null || string.IsNullOrEmpty(name)) return NotFound();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{name}\"";
            return File(file, "application/pdf");
        }
    }
}