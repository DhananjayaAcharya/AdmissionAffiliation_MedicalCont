using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class CA_Med_StaffDetailsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CA_Med_StaffDetailsController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // File helpers
        // ---------------------------------------------------------------
        private async Task<string?> SaveStaffFileAsync(
            IFormFile? file,
            string folder,
            string facultyCode)
        {
            if (file == null || file.Length == 0)
                return null;

            // Select root path based on faculty
            string rootPath = facultyCode == "2"
                ? BaseDentalPath
                : BaseMedicalPath;

            // StaffDetails folder
            string basePath = Path.Combine(rootPath, "StaffDetails");

            // Dynamic subfolder
            string fullFolder = Path.Combine(basePath, folder);

            // Create folder if not exists
            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            // Generate unique file name
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Full file path
            string fullPath = Path.Combine(fullFolder, fileName);

            // Save file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }

        // Block "b" (Teachers updated in EMS) document is stored at a fixed path
        // per college, so no database column is needed.
        private string GetTeachersEmsFilePath(string facultyCode, string collegeCode)
        {
            var root = facultyCode == "2" ? BaseDentalPath : BaseMedicalPath;

            var invalid = Path.GetInvalidFileNameChars();
            var safeCode = string.Concat(collegeCode.Select(c => invalid.Contains(c) ? '_' : c));

            return Path.Combine(root, "StaffDetails", "TeachersEMSDocs", $"{safeCode}.pdf");
        }

        // ---------------------------------------------------------------
        // GET
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> CA_Med_StaffDetails()
        {
            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            var levels = await GetSortedCourseLevels();

            var vm = await LoadStaffViewModel();

            // Changes by Ram on 23/04/2026
            vm.ExistingCourseLevels = levels;

            return View(vm);
        }

        // ---------------------------------------------------------------
        // Save Pay Scale
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePayScale(StaffDetailsCombinedViewModel model)
        {
            ModelState.Remove("StaffOther.TeachersUpdatedInEMS");
            ModelState.Remove("StaffOther.ExaminerDetailsAttached");
            ModelState.Remove("StaffOther.ServiceRegisterMaintained");
            ModelState.Remove("StaffOther.AcquittanceRegisterMaintained");

            ModelState.Remove("TeachersUpdatedEmsPdf");
            ModelState.Remove("ExaminerDetailsPdf");
            ModelState.Remove("AEBASLastThreeMonthsPdf");
            ModelState.Remove("AEBASInspectionDayPdf");
            ModelState.Remove("ProvidentFundPdf");
            ModelState.Remove("ESIPdf");

            if (!ModelState.IsValid)
            {
                var vm = await LoadStaffViewModel();
                vm.StaffPayScaleList = model.StaffPayScaleList;
                return View("CA_Med_StaffDetails", vm);
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction(nameof(CA_Med_StaffDetails));

            // Changes by Ram on 23/04/2026
            var levels = await GetSortedCourseLevels();

            foreach (var item in model.StaffPayScaleList)
            {
                if (item.PayScale == null)
                {
                    ModelState.AddModelError("", "Please enter all pay scales");
                    var vm = await LoadStaffViewModel();
                    vm.StaffPayScaleList = model.StaffPayScaleList;
                    return View("CA_Med_StaffDetails", vm);
                }

                foreach (var level in levels)
                {
                    var entity =
                        await _context.MedCaStaffParticulars
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == level &&
                            x.DesignationSlNo == item.DesignationSlNo);

                    if (entity == null)
                    {
                        entity = new MedCaStaffParticular
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = level,
                            DesignationSlNo = item.DesignationSlNo
                        };

                        _context.MedCaStaffParticulars.Add(entity);
                    }

                    entity.PayScale = item.PayScale.Value;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Staff Pay Scale saved successfully";
            return RedirectToAction(nameof(CA_Med_StaffDetails));
        }

        // ---------------------------------------------------------------
        // Save Staff Particulars (Other)
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStaffOther(StaffDetailsCombinedViewModel model)
        {
            var staffOther = model.StaffOther;

            var TeachersUpdatedEmsPdf = model.TeachersUpdatedEmsPdf;   // block b
            var ExaminerDetailsPdf = model.ExaminerDetailsPdf;         // block c
            var AEBASLastThreeMonthsPdf = model.AEBASLastThreeMonthsPdf;
            var AEBASInspectionDayPdf = model.AEBASInspectionDayPdf;
            var ProvidentFundPdf = model.ProvidentFundPdf;
            var ESIPdf = model.ESIPdf;

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) ||
                string.IsNullOrEmpty(facultyCode))
                return RedirectToAction(nameof(CA_Med_StaffDetails));

            IFormFile? ExaminerDetailsPdf2 = null;
            IFormFile? ExaminerDetailsPdf3 = null;
            IFormFile? ExaminerDetailsPdf4 = null;
            IFormFile? ExaminerDetailsPdf5 = null;
            IFormFile? TeachersUpdatedPdf = null;

            if (facultyCode == "2")
            {
                ExaminerDetailsPdf2 = model.ExaminerDetailsPdf2;
                ExaminerDetailsPdf3 = model.ExaminerDetailsPdf3;
                ExaminerDetailsPdf4 = model.ExaminerDetailsPdf4;
                ExaminerDetailsPdf5 = model.ExaminerDetailsPdf5;
                TeachersUpdatedPdf = model.TeachersUpdatedPdf;
            }

            /*
            OLD:
            var levels = new[]{"UG","PG","SS"};
            */

            // Changes by Ram on 23/04/2026
            var levels = await GetSortedCourseLevels();
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            // Changes by Ram on 24/04/26
            // Validate only if file not newly uploaded AND no old file exists
            var existingUG =
                await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == "UG");

            if ((AEBASLastThreeMonthsPdf == null || AEBASLastThreeMonthsPdf.Length == 0)
                && string.IsNullOrEmpty(existingUG?.AebaslastThreeMonthsPdfPath))
            {
                ModelState.AddModelError(
                    "AEBASLastThreeMonthsPdf",
                    "AEBAS last three months document is compulsory.");
            }

            if ((AEBASInspectionDayPdf == null || AEBASInspectionDayPdf.Length == 0)
                && string.IsNullOrEmpty(existingUG?.AebasinspectionDayPdfPath))
            {
                ModelState.AddModelError(
                    "AEBASInspectionDayPdf",
                    "AEBAS inspection day document is compulsory.");
            }

            if ((ProvidentFundPdf == null || ProvidentFundPdf.Length == 0)
                && string.IsNullOrEmpty(existingUG?.ProvidentFundPdfPath))
            {
                ModelState.AddModelError(
                    "ProvidentFundPdf",
                    "Provident Fund document is compulsory.");
            }

            if (!ModelState.IsValid)
            {
                var vm = await LoadStaffViewModel();
                vm.StaffOther = model.StaffOther;

                return View("CA_Med_StaffDetails", vm);
            }

            // ---- Block b: save its own document once (not per level) ----
            if (TeachersUpdatedEmsPdf != null && TeachersUpdatedEmsPdf.Length > 0)
            {
                var emsPath = GetTeachersEmsFilePath(facultyCode, collegeCode);
                Directory.CreateDirectory(Path.GetDirectoryName(emsPath)!);

                using (var stream = new FileStream(emsPath, FileMode.Create))
                {
                    await TeachersUpdatedEmsPdf.CopyToAsync(stream);
                }
            }

            if(facultyCode == "1")
            {

                foreach (var level in levels)
                {
                    var entity =
                        await _context.CaMedStaffParticularsOthers
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == level);

                    if (entity == null)
                    {
                        entity = new CaMedStaffParticularsOther
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = level
                        };

                    _context.CaMedStaffParticularsOthers.Add(entity);
                }

                // common b-g
                entity.TeachersUpdatedInEms = staffOther.TeachersUpdatedInEMS;
                entity.ExaminerDetailsAttached = staffOther.ExaminerDetailsAttached;
                entity.ServiceRegisterMaintained = staffOther.ServiceRegisterMaintained;
                entity.AcquittanceRegisterMaintained = staffOther.AcquittanceRegisterMaintained;

                // c - Examiner details
                if (ExaminerDetailsPdf != null)
                {
                    var path = await SaveStaffFileAsync(
                        ExaminerDetailsPdf, "ExaminerDocs", facultyCode);

                        entity.ExaminerDetailsPdfPath = path;
                        entity.ExaminerDetailsPdfName =
                            ExaminerDetailsPdf.FileName;
                    }

                    if (TeachersUpdatedPdf != null)
                    {
                        var path =
                         await SaveStaffFileAsync(
                            TeachersUpdatedPdf,
                            "EmsList", FacultyCode);

                        entity.TeachersUpdatedPdfPath = path;
                        entity.TeachersUpdatedPdfName =
                            TeachersUpdatedPdf.FileName;
                    }
                    if (ExaminerDetailsPdf2 != null)
                    {
                        var path = await SaveStaffFileAsync(
                            ExaminerDetailsPdf2,
                            "ExaminerDocs",
                            FacultyCode);

                        entity.ExaminerDetailsPdfPath2 = path;
                        entity.ExaminerDetailsPdfName2 = ExaminerDetailsPdf2.FileName;
                    }

                    if (ExaminerDetailsPdf3 != null)
                    {
                        var path = await SaveStaffFileAsync(
                            ExaminerDetailsPdf3,
                            "ExaminerDocs",
                            FacultyCode);

                        entity.ExaminerDetailsPdfPath3 = path;
                        entity.ExaminerDetailsPdfName3 = ExaminerDetailsPdf3.FileName;
                    }

                    if (ExaminerDetailsPdf4 != null)
                    {
                        var path = await SaveStaffFileAsync(
                            ExaminerDetailsPdf4,
                            "ExaminerDocs",
                            FacultyCode);

                        entity.ExaminerDetailsPdfPath4 = path;
                        entity.ExaminerDetailsPdfName4 = ExaminerDetailsPdf4.FileName;
                    }

                    if (ExaminerDetailsPdf5 != null)
                    {
                        var path = await SaveStaffFileAsync(
                            ExaminerDetailsPdf5,
                            "ExaminerDocs",
                            FacultyCode);

                        entity.ExaminerDetailsPdfPath5 = path;
                        entity.ExaminerDetailsPdfName5 = ExaminerDetailsPdf5.FileName;
                    }


                    if (AEBASLastThreeMonthsPdf != null)
                    {
                        var path =
                         await SaveStaffFileAsync(
                            AEBASLastThreeMonthsPdf,
                            "AEBAS3Months", FacultyCode);

                        entity.AebaslastThreeMonthsPdfPath = path;
                        entity.AebaslastThreeMonthsPdfName =
                            AEBASLastThreeMonthsPdf.FileName;
                    }


                    if (AEBASInspectionDayPdf != null)
                    {
                        var path =
                         await SaveStaffFileAsync(
                            AEBASInspectionDayPdf,
                            "AEBASInspection", facultyCode);

                        entity.AebasinspectionDayPdfPath = path;
                        entity.AebasinspectionDayPdfName =
                            AEBASInspectionDayPdf.FileName;
                    }


                    if (ProvidentFundPdf != null)
                    {
                        var path =
                         await SaveStaffFileAsync(
                            ProvidentFundPdf,
                            "PFDocs",
                            FacultyCode);

                        entity.ProvidentFundPdfPath = path;
                        entity.ProvidentFundPdfName =
                            ProvidentFundPdf.FileName;
                    }

               



                    // UG ONLY
                    if (courseLevel == "UG" && ESIPdf != null)
                    {
                        var path =
                          await SaveStaffFileAsync(
                            ESIPdf,
                            "ESIDocs",
                            FacultyCode);

                        entity.EsipdfPath = path;
                        entity.EsipdfName = ESIPdf.FileName;
                    }
                }
            }
            else if(facultyCode == "2")
            {
                var entity =
                        await _context.CaMedStaffParticularsOthers
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel);

                if (entity == null)
                {
                    entity = new CaMedStaffParticularsOther
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel
                    };

                    _context.CaMedStaffParticularsOthers.Add(entity);
                }


                // common b-g

                entity.TeachersUpdatedInEms =
                    staffOther.TeachersUpdatedInEMS;

                entity.ExaminerDetailsAttached =
                    staffOther.ExaminerDetailsAttached;

                entity.ServiceRegisterMaintained =
                    staffOther.ServiceRegisterMaintained;

                entity.AcquittanceRegisterMaintained =
                    staffOther.AcquittanceRegisterMaintained;


                if (ExaminerDetailsPdf != null)
                {
                    var path =
                     await SaveStaffFileAsync(
                        ExaminerDetailsPdf,
                        "ExaminerDocs", FacultyCode);

                    entity.ExaminerDetailsPdfPath = path;
                    entity.ExaminerDetailsPdfName =
                        ExaminerDetailsPdf.FileName;
                }

                if (TeachersUpdatedPdf != null)
                {
                    var path =
                     await SaveStaffFileAsync(
                        TeachersUpdatedPdf,
                        "EmsList", FacultyCode);

                    entity.TeachersUpdatedPdfPath = path;
                    entity.TeachersUpdatedPdfName =
                        TeachersUpdatedPdf.FileName;
                }
                if (ExaminerDetailsPdf2 != null)
                {
                    var path = await SaveStaffFileAsync(
                        ExaminerDetailsPdf2,
                        "ExaminerDocs",
                        FacultyCode);

                    entity.ExaminerDetailsPdfPath2 = path;
                    entity.ExaminerDetailsPdfName2 = ExaminerDetailsPdf2.FileName;
                }

                if (ExaminerDetailsPdf3 != null)
                {
                    var path = await SaveStaffFileAsync(
                        ExaminerDetailsPdf3,
                        "ExaminerDocs",
                        FacultyCode);

                    entity.ExaminerDetailsPdfPath3 = path;
                    entity.ExaminerDetailsPdfName3 = ExaminerDetailsPdf3.FileName;
                }

                if (ExaminerDetailsPdf4 != null)
                {
                    var path = await SaveStaffFileAsync(
                        ExaminerDetailsPdf4,
                        "ExaminerDocs",
                        FacultyCode);

                    entity.ExaminerDetailsPdfPath4 = path;
                    entity.ExaminerDetailsPdfName4 = ExaminerDetailsPdf4.FileName;
                }

                if (ExaminerDetailsPdf5 != null)
                {
                    var path = await SaveStaffFileAsync(
                        ExaminerDetailsPdf5,
                        "ExaminerDocs",
                        FacultyCode);

                    entity.ExaminerDetailsPdfPath5 = path;
                    entity.ExaminerDetailsPdfName5 = ExaminerDetailsPdf5.FileName;
                }

                // d - AEBAS last three months
                if (AEBASLastThreeMonthsPdf != null)
                {
                    var path = await SaveStaffFileAsync(
                        AEBASLastThreeMonthsPdf, "AEBAS3Months", facultyCode);

                    entity.AebaslastThreeMonthsPdfPath = path;
                    entity.AebaslastThreeMonthsPdfName = AEBASLastThreeMonthsPdf.FileName;
                }

                // e - AEBAS inspection day
                if (AEBASInspectionDayPdf != null)
                {
                    var path = await SaveStaffFileAsync(
                        AEBASInspectionDayPdf, "AEBASInspection", facultyCode);

                    entity.AebasinspectionDayPdfPath = path;
                    entity.AebasinspectionDayPdfName = AEBASInspectionDayPdf.FileName;
                }

                // g - Provident fund
                if (ProvidentFundPdf != null)
                {
                    var path = await SaveStaffFileAsync(
                        ProvidentFundPdf, "PFDocs", facultyCode);

                    entity.ProvidentFundPdfPath = path;
                    entity.ProvidentFundPdfName = ProvidentFundPdf.FileName;
                }





                // h - ESI (UG ONLY)
                if (courseLevel == "UG" && ESIPdf != null)
                {
                    var path = await SaveStaffFileAsync(
                        ESIPdf, "ESIDocs", facultyCode);

                    entity.EsipdfPath = path;
                    entity.EsipdfName = ESIPdf.FileName;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Staff particulars saved successfully";

            return RedirectToAction(nameof(CA_Med_StaffDetails));
        }

        // ---------------------------------------------------------------
        // Load view model
        // ---------------------------------------------------------------
        private async Task<StaffDetailsCombinedViewModel> LoadStaffViewModel()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) ||
                string.IsNullOrEmpty(facultyCode))
                return new StaffDetailsCombinedViewModel();

            // Changes by Ram on 23/04/2026
            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            var levels =
                string.IsNullOrEmpty(raw)
                ? new List<string> { "UG" }
                : System.Text.Json.JsonSerializer
                    .Deserialize<List<string>>(raw)!
                    .Select(x => x.Trim().ToUpper())
                    .Distinct()
                    .ToList();

            string commonLevel =
                levels.Contains("UG")
                ? "UG"
                : levels.FirstOrDefault() ?? "UG";

            var designations =
                await _context.MedCaMstStaffDesignations
                    .Where(x => x.FacultyCode == facultyCode)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

            var savedPayScales =
                await _context.MedCaStaffParticulars
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == commonLevel)
                    .ToListAsync();

            var payScaleList =
                designations.Select(d =>
                {
                    var saved = savedPayScales
                        .FirstOrDefault(x => x.DesignationSlNo == d.SlNo);

                    return new Med_CA_StaffParticularsVM
                    {
                        DesignationSlNo = d.SlNo,
                        Designation = d.Designation,
                        PayScale = saved?.PayScale
                    };
                }).ToList();

            var commonEntity =
                await _context.CaMedStaffParticularsOthers
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == commonLevel);

            var staffOther = new CA_Med_StaffParticularsOtherVM();

            if (commonEntity != null)
            {
                staffOther.TeachersUpdatedInEMS = commonEntity.TeachersUpdatedInEms;
                staffOther.ExaminerDetailsAttached = commonEntity.ExaminerDetailsAttached;
                staffOther.ServiceRegisterMaintained = commonEntity.ServiceRegisterMaintained;
                staffOther.AcquittanceRegisterMaintained = commonEntity.AcquittanceRegisterMaintained;

                staffOther.ExaminerDetailsPdfName = commonEntity.ExaminerDetailsPdfName;
                staffOther.TeachersUpdatedPdfName = commonEntity.TeachersUpdatedPdfName;
                staffOther.ExaminerDetailsPdfName2 = commonEntity.ExaminerDetailsPdfName2;
                staffOther.ExaminerDetailsPdfName3 = commonEntity.ExaminerDetailsPdfName3;
                staffOther.ExaminerDetailsPdfName4 = commonEntity.ExaminerDetailsPdfName4;
                staffOther.ExaminerDetailsPdfName5 = commonEntity.ExaminerDetailsPdfName5;

                staffOther.AEBASLastThreeMonthsPdfName = commonEntity.AebaslastThreeMonthsPdfName;
                staffOther.AEBASInspectionDayPdfName = commonEntity.AebasinspectionDayPdfName;
                staffOther.ProvidentFundPdfName = commonEntity.ProvidentFundPdfName;
                staffOther.ESIPdfName = commonEntity.EsipdfName;
            }

            // Block b document (stored on disk, no DB column)
            var emsFile = GetTeachersEmsFilePath(facultyCode, collegeCode);
            staffOther.TeachersUpdatedEmsPdfName =
                System.IO.File.Exists(emsFile) ? "EMS_Teachers_Document.pdf" : null;

            return new StaffDetailsCombinedViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                StaffPayScaleList = payScaleList,
                StaffOther = staffOther,
                ExistingCourseLevels = levels
            };
        }

        // ---------------------------------------------------------------
        // View / download PDF
        // code added by ram on 23/04/26
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ViewStaffOtherPdf(string type, string mode = "view")
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound();

            // Do not let the browser reuse a cached copy of a different document
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Block b - stored on disk at a fixed path
            if (type == "TeachersEms")
            {
                var emsPath = GetTeachersEmsFilePath(facultyCode, collegeCode);
                if (!System.IO.File.Exists(emsPath))
                    return NotFound("File not found");

                if (mode == "download")
                    return PhysicalFile(emsPath, "application/pdf", "EMS_Teachers_Document.pdf");

                Response.Headers["Content-Disposition"] = "inline; filename=\"EMS_Teachers_Document.pdf\"";
                return PhysicalFile(emsPath, "application/pdf");
            }

            // Other documents - prefer the UG record, otherwise any level
            var record = await _context.CaMedStaffParticularsOthers
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .OrderBy(x => x.CourseLevel == "UG" ? 0 : 1)
                .FirstOrDefaultAsync();

            if (record == null)
                return NotFound();

            string? filePath = null;
            string? fileName = null;

            switch (type)
            {

                case "Ems":
                    filePath = record.TeachersUpdatedPdfPath;
                    fileName = record.TeachersUpdatedPdfName;
                    break;

                case "Examiner":
                    filePath = record.ExaminerDetailsPdfPath;
                    fileName = record.ExaminerDetailsPdfName;
                    break;


                case "Examiner2":
                    filePath = record.ExaminerDetailsPdfPath2;
                    fileName = record.ExaminerDetailsPdfName2;
                    break;

                case "Examiner3":
                    filePath = record.ExaminerDetailsPdfPath3;
                    fileName = record.ExaminerDetailsPdfName3;
                    break;

                case "Examiner4":
                    filePath = record.ExaminerDetailsPdfPath4;
                    fileName = record.ExaminerDetailsPdfName4;
                    break;

                case "Examiner5":
                    filePath = record.ExaminerDetailsPdfPath5;
                    fileName = record.ExaminerDetailsPdfName5;
                    break;

                case "AEBAS3Months":
                    filePath = record.AebaslastThreeMonthsPdfPath;
                    fileName = record.AebaslastThreeMonthsPdfName;
                    break;

                case "AEBASInspection":
                    filePath = record.AebasinspectionDayPdfPath;
                    fileName = record.AebasinspectionDayPdfName;
                    break;

                case "PF":
                    filePath = record.ProvidentFundPdfPath;
                    fileName = record.ProvidentFundPdfName;
                    break;

                case "ESI":
                    filePath = record.EsipdfPath;
                    fileName = record.EsipdfName;
                    break;

                default:
                    return NotFound();
            }

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var finalName =
                string.IsNullOrEmpty(fileName)
                ? Path.GetFileName(filePath)
                : fileName;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            if (mode == "download")
            {
                return PhysicalFile(filePath, contentType, finalName);
            }

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{finalName}\"";

            return PhysicalFile(filePath, contentType);
        }

        [HttpGet]
        public async Task<IActionResult> ViewDentalStaffOtherPdf(string type, string mode = "view")
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound();

            // Do not let the browser reuse a cached copy of a different document
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Block b - stored on disk at a fixed path
            if (type == "TeachersEms")
            {
                var emsPath = GetTeachersEmsFilePath(facultyCode, collegeCode);
                if (!System.IO.File.Exists(emsPath))
                    return NotFound("File not found");

                if (mode == "download")
                    return PhysicalFile(emsPath, "application/pdf", "EMS_Teachers_Document.pdf");

                Response.Headers["Content-Disposition"] = "inline; filename=\"EMS_Teachers_Document.pdf\"";
                return PhysicalFile(emsPath, "application/pdf");
            }

            // Other documents - prefer the UG record, otherwise any level
            var record = await _context.CaMedStaffParticularsOthers
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .OrderBy(x => x.CourseLevel == "UG" ? 0 : 1)
                .FirstOrDefaultAsync();

            if (record == null)
                return NotFound();

            string? filePath = null;
            string? fileName = null;

            switch (type)
            {

                case "Ems":
                    filePath = record.TeachersUpdatedPdfPath;
                    fileName = record.TeachersUpdatedPdfName;
                    break;

                case "Examiner":
                    filePath = record.ExaminerDetailsPdfPath;
                    fileName = record.ExaminerDetailsPdfName;
                    break;


                case "Examiner2":
                    filePath = record.ExaminerDetailsPdfPath2;
                    fileName = record.ExaminerDetailsPdfName2;
                    break;

                case "Examiner3":
                    filePath = record.ExaminerDetailsPdfPath3;
                    fileName = record.ExaminerDetailsPdfName3;
                    break;

                case "Examiner4":
                    filePath = record.ExaminerDetailsPdfPath4;
                    fileName = record.ExaminerDetailsPdfName4;
                    break;

                case "Examiner5":
                    filePath = record.ExaminerDetailsPdfPath5;
                    fileName = record.ExaminerDetailsPdfName5;
                    break;

                case "AEBAS3Months":
                    filePath = record.AebaslastThreeMonthsPdfPath;
                    fileName = record.AebaslastThreeMonthsPdfName;
                    break;

                case "AEBASInspection":
                    filePath = record.AebasinspectionDayPdfPath;
                    fileName = record.AebasinspectionDayPdfName;
                    break;

                case "PF":
                    filePath = record.ProvidentFundPdfPath;
                    fileName = record.ProvidentFundPdfName;
                    break;

                case "ESI":
                    filePath = record.EsipdfPath;
                    fileName = record.EsipdfName;
                    break;

                default:
                    return NotFound();
            }

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var finalName =
                string.IsNullOrEmpty(fileName)
                ? Path.GetFileName(filePath)
                : fileName;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            if (mode == "download")
            {
                return PhysicalFile(filePath, contentType, finalName);
            }

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{finalName}\"";

            return PhysicalFile(filePath, contentType);
        }
    }
}