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

            string basePath = Path.Combine(BasePath, "StaffDetails");
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
            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            var levels = string.IsNullOrEmpty(raw)
                ? new List<string> { "UG" }
                : System.Text.Json.JsonSerializer
                    .Deserialize<List<string>>(raw)
                    .Select(x => x.Trim().ToUpper())
                    .Distinct()
                    .OrderBy(x =>
                        x == "UG" ? 1 :
                        x == "PG" ? 2 :
                        x == "SS" ? 3 : 99)
                    .ToList();

            var vm = await LoadStaffViewModel();

            // Changes by Ram on 23/04/2026
            vm.ExistingCourseLevels = levels;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePayScale(StaffDetailsCombinedViewModel model)
        {
            ModelState.Remove("StaffOther.TeachersUpdatedInEMS");
            ModelState.Remove("StaffOther.ExaminerDetailsAttached");
            ModelState.Remove("StaffOther.ServiceRegisterMaintained");
            ModelState.Remove("StaffOther.AcquittanceRegisterMaintained");

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

            /*
            OLD:
            var levels = new[]{"UG","PG","SS"};
            */

            // Changes by Ram on 23/04/2026

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            var levels = string.IsNullOrEmpty(raw)
                ? new[] { "UG" }
                : System.Text.Json.JsonSerializer
                    .Deserialize<List<string>>(raw)
                    .Select(x => x.Trim().ToUpper())
                    .Distinct()
                    .ToArray();


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

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) ||
               string.IsNullOrEmpty(facultyCode))
                return RedirectToAction(nameof(CA_Med_StaffDetails));


            /*
            OLD:
            var levels = new[]{"UG","PG","SS"};
            */

            // Changes by Ram on 23/04/2026

            var raw =
                HttpContext.Session.GetString("ExistingCourseLevels");

            var levels =
                string.IsNullOrEmpty(raw)
                ? new[] { "UG" }
                : System.Text.Json.JsonSerializer
                    .Deserialize<List<string>>(raw)
                    .Select(x => x.Trim().ToUpper())
                    .Distinct()
                    .ToArray();

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
                    "AEBAS last three months document is compulsory."
                );
            }

            if ((AEBASInspectionDayPdf == null || AEBASInspectionDayPdf.Length == 0)
                && string.IsNullOrEmpty(existingUG?.AebasinspectionDayPdfPath))
            {
                ModelState.AddModelError(
                    "AEBASInspectionDayPdf",
                    "AEBAS inspection day document is compulsory."
                );
            }

            if ((ProvidentFundPdf == null || ProvidentFundPdf.Length == 0)
                && string.IsNullOrEmpty(existingUG?.ProvidentFundPdfPath))
            {
                ModelState.AddModelError(
                    "ProvidentFundPdf",
                    "Provident Fund document is compulsory."
                );
            }

            if (!ModelState.IsValid)
            {
                var vm = await LoadStaffViewModel();
                vm.StaffOther = model.StaffOther;

                return View(
                    "CA_Med_StaffDetails",
                    vm
                );
            }

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
                        "ExaminerDocs");

                    entity.ExaminerDetailsPdfPath = path;
                    entity.ExaminerDetailsPdfName =
                        ExaminerDetailsPdf.FileName;
                }


                if (AEBASLastThreeMonthsPdf != null)
                {
                    var path =
                     await SaveStaffFileAsync(
                        AEBASLastThreeMonthsPdf,
                        "AEBAS3Months");

                    entity.AebaslastThreeMonthsPdfPath = path;
                    entity.AebaslastThreeMonthsPdfName =
                        AEBASLastThreeMonthsPdf.FileName;
                }


                if (AEBASInspectionDayPdf != null)
                {
                    var path =
                     await SaveStaffFileAsync(
                        AEBASInspectionDayPdf,
                        "AEBASInspection");

                    entity.AebasinspectionDayPdfPath = path;
                    entity.AebasinspectionDayPdfName =
                        AEBASInspectionDayPdf.FileName;
                }


                if (ProvidentFundPdf != null)
                {
                    var path =
                     await SaveStaffFileAsync(
                        ProvidentFundPdf,
                        "PFDocs");

                    entity.ProvidentFundPdfPath = path;
                    entity.ProvidentFundPdfName =
                        ProvidentFundPdf.FileName;
                }

               



                // UG ONLY
                if (level == "UG" && ESIPdf != null)
                {
                    var path =
                      await SaveStaffFileAsync(
                        ESIPdf,
                        "ESIDocs");

                    entity.EsipdfPath = path;
                    entity.EsipdfName = ESIPdf.FileName;
                }

            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
              "Staff particulars saved successfully";

            return RedirectToAction(nameof(CA_Med_StaffDetails));
        }







        // ✅ View PDF files
        [HttpGet]
        // ✅ View PDF files (Main first, else Temp during partial stage)

        private async Task<StaffDetailsCombinedViewModel>
        LoadStaffViewModel()
        {
            var collegeCode =
              HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
              HttpContext.Session.GetString("FacultyCode");


            if (string.IsNullOrEmpty(collegeCode) ||
               string.IsNullOrEmpty(facultyCode))
                return new StaffDetailsCombinedViewModel();


            // Changes by Ram on 23/04/2026

            var raw =
               HttpContext.Session.GetString("ExistingCourseLevels");

            var levels =
               string.IsNullOrEmpty(raw)
               ? new List<string> { "UG" }
               : System.Text.Json.JsonSerializer
                  .Deserialize<List<string>>(raw)
                  .Select(x => x.Trim().ToUpper())
                  .Distinct()
                  .ToList();


            string commonLevel =
               levels.Contains("UG")
               ? "UG"
               : levels.First();


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
                  var saved =
                     savedPayScales
                     .FirstOrDefault(
                        x => x.DesignationSlNo == d.SlNo);

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


            var staffOther =
              new CA_Med_StaffParticularsOtherVM();

            if (commonEntity != null)
            {
                staffOther.TeachersUpdatedInEMS =
                    commonEntity.TeachersUpdatedInEms;

                staffOther.ExaminerDetailsAttached =
                    commonEntity.ExaminerDetailsAttached;

                staffOther.ServiceRegisterMaintained =
                    commonEntity.ServiceRegisterMaintained;

                staffOther.AcquittanceRegisterMaintained =
                    commonEntity.AcquittanceRegisterMaintained;

                staffOther.ExaminerDetailsPdfName =
                    commonEntity.ExaminerDetailsPdfName;

                staffOther.AEBASLastThreeMonthsPdfName =
                    commonEntity.AebaslastThreeMonthsPdfName;

                staffOther.AEBASInspectionDayPdfName =
                    commonEntity.AebasinspectionDayPdfName;

                staffOther.ProvidentFundPdfName =
                    commonEntity.ProvidentFundPdfName;

                staffOther.ESIPdfName =
                    commonEntity.EsipdfName;
            }

            return new StaffDetailsCombinedViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                StaffPayScaleList = payScaleList,
                StaffOther = staffOther,
                ExistingCourseLevels = levels
            };
        }

        //code added by ram on 23/04/26 

        [HttpGet]
        public async Task<IActionResult> ViewStaffOtherPdf(string type, string mode = "view")
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound();

            // Changes by Ram on 23/04/2026
            // Common records are loaded from UG master
            var courseLevel = "UG";

            var record = await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel);

            if (record == null)
                return NotFound();

            string? filePath = null;
            string? fileName = null;

            switch (type)
            {
                case "Examiner":
                    filePath = record.ExaminerDetailsPdfPath;
                    fileName = record.ExaminerDetailsPdfName;
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

            var provider =
               new Microsoft.AspNetCore.StaticFiles
                  .FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(
                filePath,
                out string contentType))
            {
                contentType = "application/octet-stream";
            }

            if (mode == "download")
            {
                return PhysicalFile(
                    filePath,
                    contentType,
                    finalName);
            }

            Response.Headers["Content-Disposition"] =
              $"inline; filename=\"{finalName}\"";

            return PhysicalFile(filePath, contentType);
        }

    }
}
