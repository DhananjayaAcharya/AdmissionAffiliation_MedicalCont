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

        public Aff_CA_Med_FinanceDetailsController(ApplicationDbContext context) : base(context)
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

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var selectedCourseLevel = HttpContext.Session.GetString("SelectedCourseLevel")?
                                        .Trim().ToUpper()
                                     ?? HttpContext.Session.GetString("CourseLevel")?
                                        .Trim().ToUpper();

            List<string> levels;
            var validLevels = new[] { "UG", "PG", "SS" };

            if (!string.IsNullOrEmpty(selectedCourseLevel) && validLevels.Contains(selectedCourseLevel))
            {
                levels = new List<string> { selectedCourseLevel };
            }
            else
            {
                // First try from CollegeCourseIntakeDetails
                levels = await (
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
                    levels = await GetSortedCourseLevels();
                }

                levels = levels
                    .Select(l => l?.Trim().ToUpper())
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Distinct()
                    .OrderBy(l => l == "UG" ? 1 :
                                  l == "PG" ? 2 :
                                  l == "SS" ? 3 : 99)
                    .ToList();
            }

            var vm = new Med_CA_AccountAndFeeDetailsPageVM();
            var accQuery = _context.MedCaAccountAndFeeDetails
                                .Where(x => x.CollegeCode == collegeCode
                                         && x.FacultyCode == facultyCode);

            if (levels.Count == 1)
            {
                accQuery = accQuery.Where(x => x.CourseLevel == levels[0]);
            }

            var accList = await accQuery.ToListAsync();


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

            // ============================================================
            // 2. Donation / Capitation Fee Details
            // Question 6
            //
            // Only load records for the current college + faculty.
            // CourseLevel is stored with each record.
            // ============================================================

            vm.DonationFees = await _context.CollegeAdditionalFeeDetails
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId.ToString() == facultyCode &&
                    !x.IsDeleted && 
                    x.CourseLevel == selectedCourseLevel)
                .Select(x => new DonationFeeDetailVm
                {
                    Id = x.Id,

                    CollegeCode = x.CollegeCode,

                    FacultyCode = x.FacultyId.ToString(),

                    CourseLevel = x.CourseLevel,

                    FeeType = x.FeeType,

                    FeeAmount = x.FeeAmount
                })
                .ToListAsync();

            // ============================================================
            // 3. Courses Offered
            // Question 6
            //
            // Course level based.
            // ============================================================

            vm.CoursesOffered = await _context.CollegeCoursesOffereds
            .Where(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyId.ToString() == facultyCode &&
                !x.IsDeleted &&
                x.CourseLevel == selectedCourseLevel)
            .OrderBy(x => x.CourseLevel)
            .ThenBy(x => x.CourseName)
            .Select(x => new CollegeCourseOfferedVm
            {
                // =====================================================
                // BASIC DETAILS
                // =====================================================

                Id = x.Id,

                CollegeCode = x.CollegeCode,

                FacultyCode = x.FacultyId.ToString(),

                CourseLevel = x.CourseLevel,

                CourseCode = x.CourseCode,

                CourseName = x.CourseName,


                // =====================================================
                // ADMISSION DETAILS
                // =====================================================

                YearOfStarting = x.YearOfStarting,

                SanctionedAdmissions = x.SanctionedAdmissions,

                AdmittedAdmissions = x.AdmittedAdmissions,

                Remarks = x.Remarks,


                // =====================================================
                // GOVERNMENT OF KARNATAKA
                // =====================================================

                KarnatakaGovernmentPermissionNo =
                    x.GovtKarnatakaPermissionNumber,

                KarnatakaGovernmentPermissionFileName =
                    x.GovtKarnatakaDocumentName,


                // =====================================================
                // COUNCIL / APEX BODY
                // =====================================================

                CouncilPermissionNo =
                    x.CouncilPermissionNumber,

                CouncilPermissionFileName =
                    x.CouncilDocumentName,


                // =====================================================
                // LAST RGUHS AFFILIATION
                // =====================================================

                RGUHSLastAffiliationNo =
                    x.RguhslastAffiliationNumber,

                RGUHSLastAffiliationFileName =
                    x.RguhslastAffiliationDocumentName,


                // =====================================================
                // GOVERNMENT OF INDIA
                // =====================================================

                GovernmentOfIndiaPermissionNo =
                    x.GovtIndiaPermissionNumber,

                GovernmentOfIndiaPermissionFileName =
                    x.GovtIndiaDocumentName
            })
            .ToListAsync();

            vm.IsFeeLevied = await _context.CollegeAdditionalFeeDetails
                .AnyAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId.ToString() == facultyCode &&
                    !x.IsDeleted &&
                    levels.Contains(x.CourseLevel));

            // ============================================================
            // 4. If a particular course level was selected,
            //    keep only that level's data.
            // ============================================================

            if (levels.Count == 1)
            {
                var selectedLevel = levels[0];

                vm.Sections = vm.Sections
                    .Where(x =>
                        x.CourseLevel.Trim().ToUpper() == selectedLevel)
                    .ToList();

                vm.DonationFees = vm.DonationFees
                    .Where(x =>
                        x.CourseLevel.Trim().ToUpper() == selectedLevel)
                    .ToList();

                vm.CoursesOffered = vm.CoursesOffered
                    .Where(x =>
                        x.CourseLevel.Trim().ToUpper() == selectedLevel)
                    .ToList();
            }

            //ModelState.Clear()/*;*/
            return View("Med_CA_FinanceDetails", vm);
            //return View("Med_CA_FinanceDetails", new Med_CA_AccountAndFeeDetailsPageVM()); // Simplified for brevity, keep your full code
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesByLevel(string courseLevel)
        {
            if (string.IsNullOrWhiteSpace(courseLevel))
            {
                return Json(new List<object>());
            }

            var courses = await _context.MstCourses
                .Where(x => x.CourseLevel == courseLevel && x.FacultyCode == 2)
                .OrderBy(x => x.CourseName)
                .Select(x => new
                {
                    id = x.Id,
                    courseCode = x.CourseCode,
                    courseName = x.CourseName,
                    courseLevel = x.CourseLevel
                })
                .ToListAsync();

            return Json(courses);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCourseDocument(int id, string documentType)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) ||
                string.IsNullOrEmpty(facultyCode))
            {
                return Unauthorized();
            }

            var course = await _context.CollegeCoursesOffereds
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyId.ToString() == facultyCode &&
                    !x.IsDeleted);

            if (course == null)
                return NotFound();

            string? fileName = documentType switch
            {
                "KarnatakaGovernment" =>
                    course.GovtKarnatakaDocumentName,

                "Council" =>
                    course.CouncilDocumentPath,

                "RGUHS" =>
                    course.RguhslastAffiliationDocumentName,

                "GovernmentOfIndia" =>
                    course.GovtIndiaDocumentName,

                _ => null
            };

            if (string.IsNullOrWhiteSpace(fileName))
                return NotFound();

            // Use your actual document storage path here
            var filePath = Path.Combine(
                BaseMedicalPath,
                fileName
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(
                fileBytes,
                "application/pdf",
                enableRangeProcessing: true
            );
        }

        private void DeletePhysicalFileIfExists(string? filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) &&
                System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
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
            var selectedCourseLevel = HttpContext.Session.GetString("CourseLevel");

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
                //if (courseLevel == "PG")
                //    db.DonationLevied = item.DonationLevied;
                //else
                //    db.DonationLevied = null;

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
                //if (courseLevel == "PG")
                //{
                //    if (item.DonationLevied == "N")
                //    {
                //        if (!string.IsNullOrEmpty(db.DonationPdfPath) &&
                //            System.IO.File.Exists(db.DonationPdfPath))
                //        {
                //            System.IO.File.Delete(db.DonationPdfPath);
                //        }

                //        db.DonationPdfPath = null;
                //        db.DonationPdfName = null;
                //    }
                //    else if (item.DonationPdf != null && item.DonationPdf.Length > 0)
                //    {
                //        var path = await SaveFinanceFileAsync(item.DonationPdf, "DonationDocs");

                //        if (!string.IsNullOrEmpty(db.DonationPdfPath) &&
                //            System.IO.File.Exists(db.DonationPdfPath))
                //        {
                //            System.IO.File.Delete(db.DonationPdfPath);
                //        }

                //        db.DonationPdfPath = path;
                //        db.DonationPdfName = item.DonationPdf.FileName;
                //    }
                //}

                // --------------------------------------------------------
                // Donation fields
                // NOTE:
                // Question 6 / additional fee is handled separately below.
                // --------------------------------------------------------

                db.DonationLevied = null;
                db.DonationPdfPath = null;
                db.DonationPdfName = null;

            }

            // ============================================================
            // 2. ADDITIONAL FEE / DONATION / CAPITATION
            // ============================================================
            //
            // Only save when fee is actually levied.
            //
            // If IsFeeLevied = false:
            //     existing record is SOFT DELETED.
            //
            // If IsFeeLevied = true:
            //     create/update active record.
            //
            // ============================================================


            // ============================================================
            // 2. ADDITIONAL FEES - INSERT / UPDATE MULTIPLE ROWS
            // ============================================================

            if (model.DonationFees != null &&
                model.DonationFees.Any())
            {
                foreach (var fee in model.DonationFees)
                {
                    // Ignore invalid rows
                    if (string.IsNullOrWhiteSpace(fee.FeeType))
                        continue;

                    var feeCourseLevel = fee.CourseLevel?
                        .Trim()
                        .ToUpperInvariant();

                    if (string.IsNullOrWhiteSpace(feeCourseLevel))
                        continue;


                    CollegeAdditionalFeeDetail? dbFee = null;


                    // ========================================================
                    // EXISTING FEE
                    // Id > 0 = existing database record
                    // ========================================================

                    if (fee.Id > 0)
                    {
                        dbFee = await _context.CollegeAdditionalFeeDetails
                            .FirstOrDefaultAsync(x =>
                                x.Id == fee.Id &&
                                x.CollegeCode == collegeCode &&
                                x.CourseLevel == feeCourseLevel &&
                                x.FacultyId.ToString() == facultyCode &&
                                !x.IsDeleted);
                    }


                    // ========================================================
                    // NEW FEE
                    // Id = 0 = newly added UI row
                    // ========================================================

                    if (dbFee == null)
                    {
                        dbFee = new CollegeAdditionalFeeDetail
                        {
                            CollegeCode = collegeCode,
                            FacultyId = int.Parse(facultyCode),
                            CourseLevel = feeCourseLevel,
                            CreatedOn = DateTime.Now,
                            IsDeleted = false
                        };

                        _context.CollegeAdditionalFeeDetails.Add(dbFee);
                    }


                    // ========================================================
                    // UPDATE COMMON FIELDS
                    // ========================================================

                    dbFee.CourseLevel = feeCourseLevel;

                    dbFee.IsFeeLevied = model.IsFeeLevied;

                    dbFee.FeeType = fee.FeeType;

                    dbFee.FeeAmount = fee.FeeAmount;

                    dbFee.IsDeleted = false;

                    dbFee.ModifiedOn = DateTime.Now;
                }
            }


            // ============================================================
            // 3. COURSES OFFERED - INSERT / UPDATE MULTIPLE ROWS
            // ============================================================

            if (model.CoursesOffered != null &&
                model.CoursesOffered.Any())
            {
                foreach (var course in model.CoursesOffered)
                {
                    var courseLevel = course.CourseLevel?
                        .Trim()
                        .ToUpperInvariant();

                    if (string.IsNullOrWhiteSpace(courseLevel))
                        continue;

                    if (string.IsNullOrWhiteSpace(course.CourseCode))
                        continue;


                    CollegeCoursesOffered? dbCourse = null;


                    // ========================================================
                    // EXISTING COURSE
                    // ========================================================

                    if (course.Id > 0)
                    {
                        dbCourse = await _context.CollegeCoursesOffereds
                            .FirstOrDefaultAsync(x =>
                                x.Id == course.Id &&
                                x.CollegeCode == collegeCode &&
                                x.CourseLevel == courseLevel &&
                                x.FacultyId.ToString() == facultyCode &&
                                !x.IsDeleted);
                    }


                    // ========================================================
                    // NEW COURSE
                    // ========================================================

                    if (dbCourse == null)
                    {
                        dbCourse = new CollegeCoursesOffered
                        {
                            CollegeCode = collegeCode,
                            FacultyId = int.Parse(facultyCode),
                            CreatedOn = DateTime.Now,
                            CourseLevel = courseLevel,
                            IsDeleted = false
                        };

                        _context.CollegeCoursesOffereds.Add(dbCourse);
                    }


                    // ========================================================
                    // GET COURSE NAME FROM MASTER
                    // ========================================================

                    var courseMaster = await _context.MstCourses
                        .FirstOrDefaultAsync(x =>
                            x.CourseCode.ToString() == course.CourseCode &&
                            x.CourseLevel == courseLevel &&
                            x.FacultyCode == int.Parse(facultyCode));


                    // ========================================================
                    // COMMON COURSE FIELDS
                    // ========================================================

                    dbCourse.CourseCode =
                        course.CourseCode;

                    dbCourse.CourseName =
                        courseMaster?.CourseName ?? course.CourseName;

                    dbCourse.YearOfStarting =
                        course.YearOfStarting;

                    dbCourse.SanctionedAdmissions =
                        course.SanctionedAdmissions;

                    dbCourse.AdmittedAdmissions =
                        course.AdmittedAdmissions;

                    dbCourse.Remarks =
                        course.Remarks;

                    dbCourse.IsDeleted = false;

                    dbCourse.ModifiedOn = DateTime.Now;


                    // ========================================================
                    // GOVERNMENT OF KARNATAKA
                    // ========================================================

                    dbCourse.GovtKarnatakaPermissionNumber =
                        course.KarnatakaGovernmentPermissionNo;

                    if (course.KarnatakaGovernmentPermissionFile != null &&
                        course.KarnatakaGovernmentPermissionFile.Length > 0)
                    {
                        var path = await SaveFinanceFileAsync(
                            course.KarnatakaGovernmentPermissionFile,
                            "CourseDocuments/GovernmentKarnataka");

                        DeletePhysicalFileIfExists(
                            dbCourse.GovtKarnatakaDocumentPath);

                        dbCourse.GovtKarnatakaDocumentPath = path;

                        dbCourse.GovtKarnatakaDocumentName =
                            course.KarnatakaGovernmentPermissionFile.FileName;

                        dbCourse.GovtKarnatakaDocumentContentType =
                            course.KarnatakaGovernmentPermissionFile.ContentType;
                    }


                    // ========================================================
                    // COUNCIL / APEX BODY
                    // ========================================================

                    dbCourse.CouncilPermissionNumber =
                        course.CouncilPermissionNo;

                    if (course.CouncilPermissionFile != null &&
                        course.CouncilPermissionFile.Length > 0)
                    {
                        var path = await SaveFinanceFileAsync(
                            course.CouncilPermissionFile,
                            "CourseDocuments/Council");

                        DeletePhysicalFileIfExists(
                            dbCourse.CouncilDocumentPath);

                        dbCourse.CouncilDocumentPath = path;

                        dbCourse.CouncilDocumentName =
                            course.CouncilPermissionFile.FileName;

                        dbCourse.CouncilDocumentContentType =
                            course.CouncilPermissionFile.ContentType;
                    }


                    // ========================================================
                    // LAST RGUHS AFFILIATION
                    // ========================================================

                    dbCourse.RguhslastAffiliationNumber =
                        course.RGUHSLastAffiliationNo;

                    if (course.RGUHSLastAffiliationFile != null &&
                        course.RGUHSLastAffiliationFile.Length > 0)
                    {
                        var path = await SaveFinanceFileAsync(
                            course.RGUHSLastAffiliationFile,
                            "CourseDocuments/RGUHS");

                        DeletePhysicalFileIfExists(
                            dbCourse.RguhslastAffiliationDocumentPath);

                        dbCourse.RguhslastAffiliationDocumentPath = path;

                        dbCourse.RguhslastAffiliationDocumentName =
                            course.RGUHSLastAffiliationFile.FileName;

                        dbCourse.RguhslastAffiliationDocumentContentType =
                            course.RGUHSLastAffiliationFile.ContentType;
                    }


                    // ========================================================
                    // GOVERNMENT OF INDIA
                    // ========================================================

                    dbCourse.GovtIndiaPermissionNumber =
                        course.GovernmentOfIndiaPermissionNo;

                    if (course.GovernmentOfIndiaPermissionFile != null &&
                        course.GovernmentOfIndiaPermissionFile.Length > 0)
                    {
                        var path = await SaveFinanceFileAsync(
                            course.GovernmentOfIndiaPermissionFile,
                            "CourseDocuments/GovernmentIndia");

                        DeletePhysicalFileIfExists(
                            dbCourse.GovtIndiaDocumentPath);

                        dbCourse.GovtIndiaDocumentPath = path;

                        dbCourse.GovtIndiaDocumentName =
                            course.GovernmentOfIndiaPermissionFile.FileName;

                        dbCourse.GovtIndiaDocumentContentType =
                            course.GovernmentOfIndiaPermissionFile.ContentType;
                    }
                }
            }


            // ============================================================
            // SAVE ALL CHANGES ONCE
            // ============================================================

            await _context.SaveChangesAsync();

            ContinuousAffiliationController.MarkDone(HttpContext, "FinancialDetails");

            return RedirectToAction(nameof(Med_CA_AccountAndFeeDetails));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseOffered(int id)
        {
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Session expired."
                });
            }

            var course = await _context.CollegeCoursesOffereds
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyId.ToString() == facultyCode &&
                    !x.IsDeleted);

            if (course == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Course record not found."
                });
            }

            course.IsDeleted = true;
            course.ModifiedOn = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Course removed successfully."
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdditionalFee(int id)
        {
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid session."
                });
            }

            var fee = await _context.CollegeAdditionalFeeDetails
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyId.ToString() == facultyCode &&
                    !x.IsDeleted);

            if (fee == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Fee record not found."
                });
            }

            // Soft delete
            fee.IsDeleted = true;
            fee.ModifiedOn = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Additional fee deleted successfully."
            });
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