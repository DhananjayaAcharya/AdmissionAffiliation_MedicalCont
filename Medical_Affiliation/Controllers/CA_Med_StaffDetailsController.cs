using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class CA_Med_StaffDetailsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CA_Med_StaffDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task<string?> SaveStaffFileAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = @"D:\Affiliation_Medical\StaffDetails";
            string fullFolder = Path.Combine(basePath, folder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(fullFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }
        // ✅ GET
        [HttpGet]
        public async Task<IActionResult> CA_Med_StaffDetails()
        {
            var vm = await LoadStaffViewModel();
            return View(vm);
        }

        // ✅ POST 1: Save PayScale
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePayScale(StaffDetailsCombinedViewModel model)
        {
            // ✅ validate only PayScale form
            ModelState.Remove("StaffOther.TeachersUpdatedInEMS");
            ModelState.Remove("StaffOther.ExaminerDetailsAttached");
            ModelState.Remove("StaffOther.ServiceRegisterMaintained");
            ModelState.Remove("StaffOther.AcquittanceRegisterMaintained");

            // remove file validations (these are only for Form2)
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
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(courseLevel))
                return RedirectToAction(nameof(CA_Med_StaffDetails));

            foreach (var item in model.StaffPayScaleList)
            {
                if (item.PayScale == null)
                {
                    ModelState.AddModelError("", "Please enter all Pay Scale values.");
                    var vm = await LoadStaffViewModel();
                    vm.StaffPayScaleList = model.StaffPayScaleList;
                    return View("CA_Med_StaffDetails", vm);
                }

                var entity = await _context.MedCaStaffParticulars.FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                     x.CourseLevel == courseLevel &&
                    x.DesignationSlNo == item.DesignationSlNo);

                if (entity == null)
                {
                    entity = new MedCaStaffParticular
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        DesignationSlNo = item.DesignationSlNo,
                        CourseLevel = courseLevel
                    };
                    _context.MedCaStaffParticulars.Add(entity);
                }

                // ✅ overwrite value
                entity.PayScale = item.PayScale.Value;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Staff Pay Scale saved successfully!";
            return RedirectToAction(nameof(CA_Med_StaffDetails));
        }

        // ✅ POST 2: Save Staff Other
        [HttpPost]
        [ValidateAntiForgeryToken]


        public async Task<IActionResult> SaveStaffOther(StaffDetailsCombinedViewModel model)
        {
            var staffOther = model.StaffOther;

            var ExaminerDetailsPdf = model.ExaminerDetailsPdf;
            var AEBASLastThreeMonthsPdf = model.AEBASLastThreeMonthsPdf;
            var AEBASInspectionDayPdf = model.AEBASInspectionDayPdf;
            var ProvidentFundPdf = model.ProvidentFundPdf;
            var ESIPdf = model.ESIPdf;

            ModelState.Remove("StaffOther.FacultyCode");
            ModelState.Remove("StaffOther.CollegeCode");
            ModelState.Remove("StaffOther.SubFacultyCode");
            ModelState.Remove("StaffOther.RegistrationNo");
            ModelState.Remove("StaffPayScaleList");


            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(courseLevel))
                return RedirectToAction(nameof(CA_Med_StaffDetails));


            // =====================================================
            // ✅ STEP 0: CHECK IF MAIN TABLE ALREADY COMPLETE
            // =====================================================
            var mainEntity = await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            bool mainHasAllFiles =
                mainEntity != null &&
                mainEntity.AebaslastThreeMonthsPdfPath != null &&
                mainEntity.AebasinspectionDayPdfPath != null &&
                mainEntity.ProvidentFundPdfPath != null &&
                mainEntity.EsipdfPath != null &&
                (
                    mainEntity.ExaminerDetailsAttached != "Y" ||
                    mainEntity.ExaminerDetailsPdfPath != null
                );

            // =====================================================
            // ✅ STEP 1: SAVE UPLOADED FILES INTO TEMP (ALWAYS)
            // =====================================================
            var tempEntity = await _context.CaMedStaffParticularsOtherTemps
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            if (tempEntity == null)
            {
                tempEntity = new CaMedStaffParticularsOtherTemp
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = courseLevel
                };
                _context.CaMedStaffParticularsOtherTemps.Add(tempEntity);
            }

            tempEntity.TeachersUpdatedInEms = staffOther.TeachersUpdatedInEMS;
            tempEntity.ExaminerDetailsAttached = staffOther.ExaminerDetailsAttached;
            tempEntity.ServiceRegisterMaintained = staffOther.ServiceRegisterMaintained;
            tempEntity.AcquittanceRegisterMaintained = staffOther.AcquittanceRegisterMaintained;

            async Task SaveTemp(IFormFile? f, Action<string> setPath, Action<string> setName, string folder)
            {
                if (f == null || f.Length == 0) return;

                var path = await SaveStaffFileAsync(f, folder);

                if (path != null)
                {
                    setPath(path);
                    setName(f.FileName);
                }
            }

            await SaveTemp(ExaminerDetailsPdf,
    p => tempEntity.ExaminerDetailsPdfPath = p,
    n => tempEntity.ExaminerDetailsPdfName = n,
    "ExaminerDocs");

            await SaveTemp(AEBASLastThreeMonthsPdf,
                p => tempEntity.AebaslastThreeMonthsPdfPath = p,
                n => tempEntity.AebaslastThreeMonthsPdfName = n,
                "AEBAS3Months");

            await SaveTemp(AEBASInspectionDayPdf,
                p => tempEntity.AebasinspectionDayPdfPath = p,
                n => tempEntity.AebasinspectionDayPdfName = n,
                "AEBASInspection");

            await SaveTemp(ProvidentFundPdf,
                p => tempEntity.ProvidentFundPdfPath = p,
                n => tempEntity.ProvidentFundPdfName = n,
                "PFDocs");

            await SaveTemp(ESIPdf,
                p => tempEntity.EsipdfPath = p,
                n => tempEntity.EsipdfName = n,
                "ESIDocs");

            await _context.SaveChangesAsync();

            // =====================================================
            // ✅ STEP 2: VALIDATE ONLY IF MAIN IS NOT COMPLETE
            // =====================================================
            if (!mainHasAllFiles)
            {
                bool examinerRequired = tempEntity.ExaminerDetailsAttached == "Y";

                bool hasExaminer = !examinerRequired || tempEntity.ExaminerDetailsPdfPath != null;
                bool hasAEBAS3 = tempEntity.AebaslastThreeMonthsPdfPath != null;
                bool hasAEBASDay = tempEntity.AebasinspectionDayPdfPath != null;
                bool hasPF = tempEntity.ProvidentFundPdfPath != null;
                bool hasESI = tempEntity.EsipdfPath != null;

                if (!(hasExaminer && hasAEBAS3 && hasAEBASDay && hasPF && hasESI))
                {
                    TempData["Error"] =
                        "Validation Failed. Please upload all remaining required files and proceed.";
                    return RedirectToAction(nameof(CA_Med_StaffDetails));
                }
            }

            // =====================================================
            // ✅ STEP 3: MOVE TEMP → MAIN (EDIT OR FIRST SAVE)
            // =====================================================
            if (mainEntity == null)
            {
                mainEntity = new CaMedStaffParticularsOther
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = courseLevel
                };
                _context.CaMedStaffParticularsOthers.Add(mainEntity);
            }

            mainEntity.TeachersUpdatedInEms = tempEntity.TeachersUpdatedInEms;
            mainEntity.ExaminerDetailsAttached = tempEntity.ExaminerDetailsAttached;
            mainEntity.ServiceRegisterMaintained = tempEntity.ServiceRegisterMaintained;
            mainEntity.AcquittanceRegisterMaintained = tempEntity.AcquittanceRegisterMaintained;

            if (tempEntity.ExaminerDetailsPdfPath != null)
            {
                mainEntity.ExaminerDetailsPdfPath = tempEntity.ExaminerDetailsPdfPath;
                mainEntity.ExaminerDetailsPdfName = tempEntity.ExaminerDetailsPdfName;
            }

            if (tempEntity.AebaslastThreeMonthsPdfPath != null)
            {
                mainEntity.AebaslastThreeMonthsPdfPath = tempEntity.AebaslastThreeMonthsPdfPath;
                mainEntity.AebaslastThreeMonthsPdfName = tempEntity.AebaslastThreeMonthsPdfName;
            }

            if (tempEntity.AebasinspectionDayPdfPath != null)
            {
                mainEntity.AebasinspectionDayPdfPath = tempEntity.AebasinspectionDayPdfPath;
                mainEntity.AebasinspectionDayPdfName = tempEntity.AebasinspectionDayPdfName;
            }

            if (tempEntity.ProvidentFundPdfPath != null)
            {
                mainEntity.ProvidentFundPdfPath = tempEntity.ProvidentFundPdfPath;
                mainEntity.ProvidentFundPdfName = tempEntity.ProvidentFundPdfName;
            }

            if (tempEntity.EsipdfPath != null)
            {
                mainEntity.EsipdfPath = tempEntity.EsipdfPath;
                mainEntity.EsipdfName = tempEntity.EsipdfName;
            }

            await _context.SaveChangesAsync();

            _context.CaMedStaffParticularsOtherTemps.Remove(tempEntity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Staff Particulars (Other) saved successfully!";
            return RedirectToAction(nameof(CA_Med_StaffDetails));
        }




        // ✅ Load ViewModel (PayScale blank + show saved values/files)
        private async Task<StaffDetailsCombinedViewModel> LoadStaffViewModel()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(courseLevel))
                return new StaffDetailsCombinedViewModel();

            // ================= PAY SCALE TABLE =================
            var designations = await _context.MedCaMstStaffDesignations
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedPayScales = await _context.MedCaStaffParticulars
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
                .ToListAsync();

            var payScaleList = designations.Select(d =>
            {
                var saved = savedPayScales.FirstOrDefault(s => s.DesignationSlNo == d.SlNo);

                return new Med_CA_StaffParticularsVM
                {
                    DesignationSlNo = d.SlNo,
                    Designation = d.Designation,
                    PayScale = saved?.PayScale // null -> blank
                };
            }).ToList();

            // ================= MAIN TABLE DATA =================
            var mainEntity = await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            // ================= TEMP TABLE DATA =================
            // ✅ temp table name: CA_Med_StaffPArticularsOther_Temp (scaffolded entity name may vary)
            var tempEntity = await _context.CaMedStaffParticularsOtherTemps
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            // ================= BUILD STAFF OTHER VM =================
            // ✅ priority: MAIN first, else TEMP
            var staffOther = new CA_Med_StaffParticularsOtherVM();

            if (mainEntity != null)
            {
                staffOther.TeachersUpdatedInEMS = mainEntity.TeachersUpdatedInEms;
                staffOther.ExaminerDetailsAttached = mainEntity.ExaminerDetailsAttached;
                staffOther.ServiceRegisterMaintained = mainEntity.ServiceRegisterMaintained;
                staffOther.AcquittanceRegisterMaintained = mainEntity.AcquittanceRegisterMaintained;

                staffOther.ExaminerDetailsPdfName = mainEntity.ExaminerDetailsPdfName;
                staffOther.AEBASLastThreeMonthsPdfName = mainEntity.AebaslastThreeMonthsPdfName;
                staffOther.AEBASInspectionDayPdfName = mainEntity.AebasinspectionDayPdfName;
                staffOther.ProvidentFundPdfName = mainEntity.ProvidentFundPdfName;
                staffOther.ESIPdfName = mainEntity.EsipdfName;
            }
            else if (tempEntity != null)
            {
                // ✅ show temp uploaded values during partial stage
                staffOther.TeachersUpdatedInEMS = tempEntity.TeachersUpdatedInEms;
                staffOther.ExaminerDetailsAttached = tempEntity.ExaminerDetailsAttached;
                staffOther.ServiceRegisterMaintained = tempEntity.ServiceRegisterMaintained;
                staffOther.AcquittanceRegisterMaintained = tempEntity.AcquittanceRegisterMaintained;

                staffOther.ExaminerDetailsPdfName = tempEntity.ExaminerDetailsPdfName;
                staffOther.AEBASLastThreeMonthsPdfName = tempEntity.AebaslastThreeMonthsPdfName;
                staffOther.AEBASInspectionDayPdfName = tempEntity.AebasinspectionDayPdfName;
                staffOther.ProvidentFundPdfName = tempEntity.ProvidentFundPdfName;
                staffOther.ESIPdfName = tempEntity.EsipdfName;
            }

            return new StaffDetailsCombinedViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                CourseLevel = courseLevel,
                StaffPayScaleList = payScaleList,
                StaffOther = staffOther
            };
        }


        // ✅ View PDF files
        [HttpGet]
        // ✅ View PDF files (Main first, else Temp during partial stage)

        [HttpGet]
        public async Task<IActionResult> ViewStaffOtherPdf(string type, string mode = "view")
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(courseLevel))
                return NotFound();

            string? filePath = null;
            string? fileName = null;

            // ✅ 1) MAIN TABLE FIRST
            var mainEntity = await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode &&
                                          x.FacultyCode == facultyCode &&
                                          x.CourseLevel == courseLevel);

            if (mainEntity != null)
            {
                switch (type)
                {
                    case "Examiner":
                        filePath = mainEntity.ExaminerDetailsPdfPath;
                        fileName = mainEntity.ExaminerDetailsPdfName;
                        break;

                    case "AEBAS3Months":
                        filePath = mainEntity.AebaslastThreeMonthsPdfPath;
                        fileName = mainEntity.AebaslastThreeMonthsPdfName;
                        break;

                    case "AEBASInspection":
                        filePath = mainEntity.AebasinspectionDayPdfPath;
                        fileName = mainEntity.AebasinspectionDayPdfName;
                        break;

                    case "PF":
                        filePath = mainEntity.ProvidentFundPdfPath;
                        fileName = mainEntity.ProvidentFundPdfName;
                        break;

                    case "ESI":
                        filePath = mainEntity.EsipdfPath;
                        fileName = mainEntity.EsipdfName;
                        break;
                }
            }

            // ✅ 2) IF NOT FOUND → TEMP TABLE
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                var tempEntity = await _context.CaMedStaffParticularsOtherTemps
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode &&
                                              x.FacultyCode == facultyCode &&
                                              x.CourseLevel == courseLevel);

                if (tempEntity == null)
                    return NotFound();

                switch (type)
                {
                    case "Examiner":
                        filePath = tempEntity.ExaminerDetailsPdfPath;
                        fileName = tempEntity.ExaminerDetailsPdfName;
                        break;

                    case "AEBAS3Months":
                        filePath = tempEntity.AebaslastThreeMonthsPdfPath;
                        fileName = tempEntity.AebaslastThreeMonthsPdfName;
                        break;

                    case "AEBASInspection":
                        filePath = tempEntity.AebasinspectionDayPdfPath;
                        fileName = tempEntity.AebasinspectionDayPdfName;
                        break;

                    case "PF":
                        filePath = tempEntity.ProvidentFundPdfPath;
                        fileName = tempEntity.ProvidentFundPdfName;
                        break;

                    case "ESI":
                        filePath = tempEntity.EsipdfPath;
                        fileName = tempEntity.EsipdfName;
                        break;

                    default:
                        return NotFound();
                }
            }

            // ✅ FINAL VALIDATION
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var finalName = string.IsNullOrEmpty(fileName)
                ? Path.GetFileName(filePath)
                : fileName;

            // 🔥 Detect content type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // 📥 DOWNLOAD
            if (mode == "download")
            {
                return PhysicalFile(filePath, contentType, finalName);
            }

            // 👀 PREVIEW
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{finalName}\"";
            return PhysicalFile(filePath, contentType);
        }

    }
}
