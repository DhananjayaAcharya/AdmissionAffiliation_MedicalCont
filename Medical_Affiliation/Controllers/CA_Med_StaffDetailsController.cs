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
                mainEntity.AebaslastThreeMonthsPdf != null &&
                mainEntity.AebasinspectionDayPdf != null &&
                mainEntity.ProvidentFundPdf != null &&
                mainEntity.Esipdf != null &&
                (
                    mainEntity.ExaminerDetailsAttached != "Y" ||
                    mainEntity.ExaminerDetailsPdf != null
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

            async Task SaveTemp(IFormFile? f, Action<byte[]> setB, Action<string> setN)
            {
                if (f == null || f.Length == 0) return;
                using var ms = new MemoryStream();
                await f.CopyToAsync(ms);
                setB(ms.ToArray());
                setN(f.FileName);
            }

            await SaveTemp(ExaminerDetailsPdf, b => tempEntity.ExaminerDetailsPdf = b, n => tempEntity.ExaminerDetailsPdfName = n);
            await SaveTemp(AEBASLastThreeMonthsPdf, b => tempEntity.AebaslastThreeMonthsPdf = b, n => tempEntity.AebaslastThreeMonthsPdfName = n);
            await SaveTemp(AEBASInspectionDayPdf, b => tempEntity.AebasinspectionDayPdf = b, n => tempEntity.AebasinspectionDayPdfName = n);
            await SaveTemp(ProvidentFundPdf, b => tempEntity.ProvidentFundPdf = b, n => tempEntity.ProvidentFundPdfName = n);
            await SaveTemp(ESIPdf, b => tempEntity.Esipdf = b, n => tempEntity.EsipdfName = n);

            await _context.SaveChangesAsync();

            // =====================================================
            // ✅ STEP 2: VALIDATE ONLY IF MAIN IS NOT COMPLETE
            // =====================================================
            if (!mainHasAllFiles)
            {
                bool examinerRequired = tempEntity.ExaminerDetailsAttached == "Y";

                bool hasExaminer = !examinerRequired || tempEntity.ExaminerDetailsPdf != null;
                bool hasAEBAS3 = tempEntity.AebaslastThreeMonthsPdf != null;
                bool hasAEBASDay = tempEntity.AebasinspectionDayPdf != null;
                bool hasPF = tempEntity.ProvidentFundPdf != null;
                bool hasESI = tempEntity.Esipdf != null;

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

            if (tempEntity.ExaminerDetailsPdf != null)
            {
                mainEntity.ExaminerDetailsPdf = tempEntity.ExaminerDetailsPdf;
                mainEntity.ExaminerDetailsPdfName = tempEntity.ExaminerDetailsPdfName;
            }

            if (tempEntity.AebaslastThreeMonthsPdf != null)
            {
                mainEntity.AebaslastThreeMonthsPdf = tempEntity.AebaslastThreeMonthsPdf;
                mainEntity.AebaslastThreeMonthsPdfName = tempEntity.AebaslastThreeMonthsPdfName;
            }

            if (tempEntity.AebasinspectionDayPdf != null)
            {
                mainEntity.AebasinspectionDayPdf = tempEntity.AebasinspectionDayPdf;
                mainEntity.AebasinspectionDayPdfName = tempEntity.AebasinspectionDayPdfName;
            }

            if (tempEntity.ProvidentFundPdf != null)
            {
                mainEntity.ProvidentFundPdf = tempEntity.ProvidentFundPdf;
                mainEntity.ProvidentFundPdfName = tempEntity.ProvidentFundPdfName;
            }

            if (tempEntity.Esipdf != null)
            {
                mainEntity.Esipdf = tempEntity.Esipdf;
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

        public async Task<IActionResult> ViewStaffOtherPdf(string type)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(courseLevel))
                return NotFound();

            // ✅ 1) Try MAIN table first
            var mainEntity = await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            byte[]? fileBytes = null;
            string? fileName = null;

            if (mainEntity != null)
            {
                switch (type)
                {
                    case "Examiner":
                        fileBytes = mainEntity.ExaminerDetailsPdf;
                        fileName = mainEntity.ExaminerDetailsPdfName;
                        break;

                    case "AEBAS3Months":
                        fileBytes = mainEntity.AebaslastThreeMonthsPdf;
                        fileName = mainEntity.AebaslastThreeMonthsPdfName;
                        break;

                    case "AEBASInspection":
                        fileBytes = mainEntity.AebasinspectionDayPdf;
                        fileName = mainEntity.AebasinspectionDayPdfName;
                        break;

                    case "PF":
                        fileBytes = mainEntity.ProvidentFundPdf;
                        fileName = mainEntity.ProvidentFundPdfName;
                        break;

                    case "ESI":
                        fileBytes = mainEntity.Esipdf;
                        fileName = mainEntity.EsipdfName;
                        break;
                }
            }

            // ✅ 2) If not found in MAIN, try TEMP table (partial upload stage)
            if (fileBytes == null || fileBytes.Length == 0 || string.IsNullOrEmpty(fileName))
            {
                var tempEntity = await _context.CaMedStaffParticularsOtherTemps
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

                if (tempEntity == null)
                    return NotFound();

                switch (type)
                {
                    case "Examiner":
                        fileBytes = tempEntity.ExaminerDetailsPdf;
                        fileName = tempEntity.ExaminerDetailsPdfName;
                        break;

                    case "AEBAS3Months":
                        fileBytes = tempEntity.AebaslastThreeMonthsPdf;
                        fileName = tempEntity.AebaslastThreeMonthsPdfName;
                        break;

                    case "AEBASInspection":
                        fileBytes = tempEntity.AebasinspectionDayPdf;
                        fileName = tempEntity.AebasinspectionDayPdfName;
                        break;

                    case "PF":
                        fileBytes = tempEntity.ProvidentFundPdf;
                        fileName = tempEntity.ProvidentFundPdfName;
                        break;

                    case "ESI":
                        fileBytes = tempEntity.Esipdf;
                        fileName = tempEntity.EsipdfName;
                        break;

                    default:
                        return NotFound();
                }
            }

            // ✅ Still missing?
            if (fileBytes == null || fileBytes.Length == 0 || string.IsNullOrEmpty(fileName))
                return NotFound();

            // ✅ Return the PDF inline
            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
            return File(fileBytes, "application/pdf");
        }

    }
}
