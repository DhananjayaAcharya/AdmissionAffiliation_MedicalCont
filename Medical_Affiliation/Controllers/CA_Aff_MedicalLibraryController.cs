using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Medical_Affiliation.Controllers
{
    public class CA_Aff_MedicalLibraryController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CA_Aff_MedicalLibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult MedicalLibrary()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = HttpContext.Session.GetInt32("FacultyId") ?? 1;
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var model = new CA_Aff_MedicalLibraryViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationType = affiliationType,
                CourseLevel = courseLevel
            };





            // ===================== 1. LIBRARY SERVICES =====================
            var savedServices = _context.CaMedicalLibraryServices
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel &&
                            x.AffiliationType == affiliationType)
                .ToList();

            var masterServices = _context.CaMstMediLibraryServices
                .OrderBy(s => s.ServiceId)
                .ToList();

            model.LibraryServices = masterServices.Select(m =>
            {
                var saved = savedServices.FirstOrDefault(s => s.ServiceId == m.ServiceId);
                return new LibraryServiceRowViewModel
                {
                    ServiceId = m.ServiceId,
                    IsAvailable = saved?.IsAvailable,

                    ExistingFileName = saved?.UploadedFileName,
                    UploadedPdf = null
                };
            }).ToList();

            // ===================== 2. USAGE REPORT =====================
            var usage = _context.CaMedicalLibraryUsageReports
                .FirstOrDefault(x => x.CollegeCode == collegeCode &&
                                     x.FacultyCode == facultyCode &&
                                     x.CourseLevel == courseLevel &&
                                     x.AffiliationType == affiliationType);

            if (usage != null)
                model.ExistingUsageReportFileName = usage.UploadedFileName;

            // ===================== 3. LIBRARY STAFF =====================
            var savedStaff = _context.CaMedicalLibraryStaffs
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel &&
                            x.AffiliationType == affiliationType)
                .ToList();

            model.LibraryStaff = savedStaff.Select(s => new LibraryStaffViewModel
            {
                Id = s.Id,
                StaffName = s.StaffName,
                Designation = s.Designation,
                Qualification = s.Qualification,
                Experience = s.Experience,
                Category = s.Category
            }).ToList();

            // ===================== 4. DEPARTMENTAL LIBRARY =====================
            // ===================== 4. DEPARTMENTAL LIBRARY (FIXED) =====================
            var savedDepartments = _context.CaMedicalDepartmentLibraries
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel &&
                            x.AffiliationType == affiliationType)
                .ToList();

            // If data exists → load only saved rows
            if (savedDepartments.Any())
            {
                model.DepartmentLibraries = savedDepartments.Select(s =>
                {
                    string staff1 = "";
                    string staff2 = "";

                    if (!string.IsNullOrWhiteSpace(s.LibraryStaff))
                    {
                        var parts = s.LibraryStaff.Split('|', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length > 0)
                            staff1 = parts[0].Trim();

                        if (parts.Length > 1)
                            staff2 = parts[1].Trim();
                    }

                    return new DepartmentLibraryViewModel
                    {
                        DepartmentCode = s.DepartmentCode,
                        TotalBooks = s.TotalBooks,
                        BooksAddedInYear = s.BooksAddedInYear,
                        CurrentJournals = s.CurrentJournals,
                        LibraryStaff1 = staff1,
                        LibraryStaff2 = staff2
                    };
                }).ToList();

            }
            else
            {
                // FIRST LOGIN → ONE EMPTY ROW ONLY
                model.DepartmentLibraries = new List<DepartmentLibraryViewModel>
    {
        new DepartmentLibraryViewModel()
    };
            }


            // ===================== 5. OTHER DETAILS =====================
            var otherDetails = _context.CaMedicalLibraryOtherDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode &&
                                     x.FacultyCode == facultyCode &&
                                     x.CourseLevel == courseLevel &&
                                     x.AffiliationType == affiliationType);

            if (otherDetails != null)
            {
                model.OtherDetails = new MedicalLibraryOtherDetailsViewModel
                {
                    DigitalValuationId = otherDetails.DigitalValuationId,
                    HasDigitalValuationCentre = otherDetails.HasDigitalValuationCentre,
                    NoOfSystems = otherDetails.NoOfSystems,
                    HasStableInternet = otherDetails.HasStableInternet,
                    HasCccameraSystem = otherDetails.HasCccameraSystem,

                    SpecialFeaturesQuestion =
                            otherDetails.SpecialFeaturesAchievementsPdfPath != null ? "Yes" : "No",

                    HasSpecialFeaturesPdf =
                            otherDetails.SpecialFeaturesAchievementsPdfPath != null,


                    UploadedFileName = otherDetails.UploadedFileName,
                    CreatedDate = otherDetails.CreatedDate
                };


            }

            // ===================== 6. ViewBag Masters =====================
            ViewBag.LibraryServiceMasters = masterServices;
            ViewBag.DepartmentMasters = _context.DepartmentMasters
                                     .Where(d => d.FacultyCode == facultyCode)
                                     .OrderBy(d => d.DepartmentCode)
                                     .ToList();

            bool hasLibraryServicePdf = model.LibraryServices.Any(s =>
                                 !string.IsNullOrEmpty(s.ExistingFileName));

            bool hasUsageReportPdf =
                !string.IsNullOrEmpty(model.ExistingUsageReportFileName);

            bool hasSpecialFeaturesPdf =
                model.OtherDetails?.HasSpecialFeaturesPdf == true;

            model.IsFirstLogin = !(hasLibraryServicePdf || hasUsageReportPdf || hasSpecialFeaturesPdf);


            return View("MedicalLibrary", model);
        }
        private async Task<string?> SaveLibraryFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = Path.Combine(BasePath, "MedicalLibrary");
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicalLibrary(CA_Aff_MedicalLibraryViewModel model)
        {
            if (model == null)
                return RedirectToAction(nameof(MedicalLibrary));

            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = HttpContext.Session.GetInt32("FacultyId") ?? 1;
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            model.CollegeCode = collegeCode;
            model.FacultyCode = facultyCode;
            model.AffiliationType = affiliationType;
            model.CourseLevel = courseLevel;

            // =====================================================
            // VALIDATION – ONLY WHAT IS ACTUALLY MANDATORY
            // =====================================================
            // 🔴 Library Services – ONLY mandatory section

            // Load all existing services for this college/faculty/affiliation once
            var existingServices = _context.CaMedicalLibraryServices
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel &&
                            x.AffiliationType == affiliationType)
                .ToList();

            foreach (var row in model.LibraryServices)
            {
                bool pdfExists = existingServices
    .Any(x => x.ServiceId == row.ServiceId && !string.IsNullOrEmpty(x.UploadedFileName));


                if (row.ServiceId == 6 && row.IsAvailable == "Yes" && row.UploadedPdf == null && !pdfExists)
                {
                    ModelState.AddModelError("", "User Education Programme PDF is mandatory.");
                }

            }


            // 🔴 Other Details – CONDITIONAL ONLY
            var otherDetailsVm = model.OtherDetails ?? new MedicalLibraryOtherDetailsViewModel();


            if (model.OtherDetails != null && model.OtherDetails.HasDigitalValuationCentre == "Yes")
            {
                if (!model.OtherDetails.NoOfSystems.HasValue || model.OtherDetails.NoOfSystems <= 0)
                    ModelState.AddModelError(nameof(model.OtherDetails.NoOfSystems), "Number of systems is required.");

                if (string.IsNullOrWhiteSpace(model.OtherDetails.HasStableInternet))
                    ModelState.AddModelError(nameof(model.OtherDetails.HasStableInternet), "LAN / Stable Internet is required.");

                if (string.IsNullOrWhiteSpace(model.OtherDetails.HasCccameraSystem))
                    ModelState.AddModelError(nameof(model.OtherDetails.HasCccameraSystem), "CCTV Camera System is required.");
            }

            // PDF optional, no validation needed

            // 🔴 Usage Report – mandatory only on first upload
            bool hasExistingUsagePdf = _context.CaMedicalLibraryUsageReports.Any(x =>
                                        x.CollegeCode == collegeCode &&
                                        x.FacultyCode == facultyCode &&
                                        x.CourseLevel == courseLevel &&
                                        x.AffiliationType == affiliationType &&
                                        !string.IsNullOrEmpty(x.UploadedFileName)
                                    );

            if (!hasExistingUsagePdf && model.UsageReportPdf == null)
            {
                ModelState.AddModelError(
                    nameof(model.UsageReportPdf),
                    "Usage Report PDF is mandatory."
                );
            }


            if (model.OtherDetails?.HasDigitalValuationCentre == "No")
            {
                ModelState.Remove("OtherDetails.NoOfSystems");
                ModelState.Remove("OtherDetails.HasStableInternet");
                ModelState.Remove("OtherDetails.HasCccameraSystem");
            }

            // =====================
            // SPECIAL FEATURES – CONDITIONAL VALIDATION
            // =====================

            bool hasExistingSpecialPdf = _context.CaMedicalLibraryOtherDetails.Any(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.CourseLevel == courseLevel &&
                x.AffiliationType == affiliationType &&
                x.SpecialFeaturesAchievementsPdfPath != null
            );

            if (model.OtherDetails?.SpecialFeaturesQuestion == "Yes")
            {
                if (model.OtherDetails.SpecialFeaturesPdf == null && !hasExistingSpecialPdf)
                {
                    ModelState.AddModelError(
                        "OtherDetails.SpecialFeaturesPdf",
                        "Special Features PDF is required."
                    );
                }
            }
            else
            {
                // User selected No → remove validation
                ModelState.Remove("OtherDetails.SpecialFeaturesPdf");
            }

            // 🔴 Digital Valuation Yes/No mandatory
            if (string.IsNullOrWhiteSpace(model.OtherDetails?.HasDigitalValuationCentre))
            {
                ModelState.AddModelError(
                    "OtherDetails.HasDigitalValuationCentre",
                    "Please select Yes or No for Digital Valuation Centre."
                );
            }

            // 🔴 Special Features Yes/No mandatory
            if (string.IsNullOrWhiteSpace(model.OtherDetails?.SpecialFeaturesQuestion))
            {
                ModelState.AddModelError(
                    "OtherDetails.SpecialFeaturesQuestion",
                    "Please select Yes or No for Special Features."
                );
            }



            if (!ModelState.IsValid)
            {
                LoadMedicalLibraryMasters(model);
                return View("MedicalLibrary", model);
            }

            // =====================================================
            // SAVE TRANSACTION
            // =====================================================
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 1. LIBRARY SERVICES
                foreach (var row in model.LibraryServices)
                {
                    var entity = _context.CaMedicalLibraryServices.FirstOrDefault(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == courseLevel &&
                        x.AffiliationType == affiliationType &&
                        x.ServiceId == row.ServiceId);

                    if (entity == null)
                    {
                        entity = new CaMedicalLibraryService
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            AffiliationType = affiliationType,
                            CourseLevel = courseLevel,
                            ServiceId = row.ServiceId ?? 0
                        };
                        _context.CaMedicalLibraryServices.Add(entity);
                    }

                    entity.IsAvailable = row.IsAvailable;

                    // Upload / replace PDF ONLY when user uploads a new one
                    if (row.ServiceId == 6 && row.UploadedPdf != null && row.UploadedPdf.Length > 0)
                    {
                        var path = await SaveLibraryFileAsync(row.UploadedPdf, "LibraryServices");

                        if (path != null)
                        {
                            if (!string.IsNullOrEmpty(entity.UploadedPdfPath) &&
                                System.IO.File.Exists(entity.UploadedPdfPath))
                            {
                                System.IO.File.Delete(entity.UploadedPdfPath);
                            }

                            entity.UploadedPdfPath = path;
                            entity.UploadedFileName = row.UploadedPdf.FileName;
                        }
                    }

                }

                // 2. USAGE REPORT
                var usage = _context.CaMedicalLibraryUsageReports.FirstOrDefault(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel &&
                    x.AffiliationType == affiliationType);

                if (usage == null && model.UsageReportPdf != null)
                {
                    usage = new CaMedicalLibraryUsageReport
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        AffiliationType = affiliationType,
                        CourseLevel = courseLevel
                    };
                    _context.CaMedicalLibraryUsageReports.Add(usage);
                }

                if (usage != null && model.UsageReportPdf != null && model.UsageReportPdf.Length > 0)
                {
                    var path = await SaveLibraryFileAsync(model.UsageReportPdf, "UsageReports");

                    if (path != null)
                    {
                        if (!string.IsNullOrEmpty(usage.UploadedFileDataPath) &&
                            System.IO.File.Exists(usage.UploadedFileDataPath))
                        {
                            System.IO.File.Delete(usage.UploadedFileDataPath);
                        }

                        usage.UploadedFileDataPath = path;
                        usage.UploadedFileName = model.UsageReportPdf.FileName;
                    }
                }

                // 3. LIBRARY STAFF
                var oldStaff = _context.CaMedicalLibraryStaffs
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                x.CourseLevel == courseLevel &&
                                x.AffiliationType == affiliationType)
                    .ToList();
                _context.CaMedicalLibraryStaffs.RemoveRange(oldStaff);

                foreach (var staff in model.LibraryStaff)
                {
                    if (staff.IsDeleted)
                        continue; // skip this row

                    _context.CaMedicalLibraryStaffs.Add(new CaMedicalLibraryStaff
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel,
                        AffiliationType = affiliationType,
                        StaffName = staff.StaffName,
                        Designation = staff.Designation,
                        Qualification = staff.Qualification,
                        Experience = staff.Experience ?? 0,
                        Category = staff.Category
                    });
                }


                // 4. DEPARTMENT LIBRARY
                var oldDepts = _context.CaMedicalDepartmentLibraries
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                x.CourseLevel == courseLevel &&
                                x.AffiliationType == affiliationType)
                    .ToList();
                _context.CaMedicalDepartmentLibraries.RemoveRange(oldDepts);

                foreach (var dept in model.DepartmentLibraries)
                {
                    if (string.IsNullOrWhiteSpace(dept.DepartmentCode))
                        continue;

                    // Combine staff names safely
                    var staffList = new List<string>();

                    if (!string.IsNullOrWhiteSpace(dept.LibraryStaff1))
                        staffList.Add(dept.LibraryStaff1.Trim());

                    if (!string.IsNullOrWhiteSpace(dept.LibraryStaff2))
                        staffList.Add(dept.LibraryStaff2.Trim());

                    string combinedStaff = string.Join(" | ", staffList);

                    _context.CaMedicalDepartmentLibraries.Add(new CaMedicalDepartmentLibrary
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel,
                        AffiliationType = affiliationType,
                        DepartmentCode = dept.DepartmentCode,
                        TotalBooks = dept.TotalBooks ?? 0,
                        BooksAddedInYear = dept.BooksAddedInYear ?? 0,
                        CurrentJournals = dept.CurrentJournals ?? 0,
                        LibraryStaff = combinedStaff   // 👈 SAVED IN ONE COLUMN
                    });
                }


                // 5. OTHER DETAILS
                var otherEntity = _context.CaMedicalLibraryOtherDetails.FirstOrDefault(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel &&
                    x.AffiliationType == affiliationType);

                if (otherEntity == null)
                {
                    otherEntity = new CaMedicalLibraryOtherDetail
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        AffiliationType = affiliationType,
                        CourseLevel = courseLevel,
                        CreatedDate = DateTime.Now
                    };
                    _context.CaMedicalLibraryOtherDetails.Add(otherEntity);
                }

                if (model.OtherDetails != null)
                {
                    otherEntity.HasDigitalValuationCentre = model.OtherDetails.HasDigitalValuationCentre;
                }


                // 1️⃣ Digital Valuation fields
                if (model.OtherDetails.HasDigitalValuationCentre == "Yes")
                {
                    otherEntity.NoOfSystems = model.OtherDetails.NoOfSystems;
                    otherEntity.HasStableInternet = model.OtherDetails.HasStableInternet;
                    otherEntity.HasCccameraSystem = model.OtherDetails.HasCccameraSystem;
                }
                else
                {
                    otherEntity.NoOfSystems = null;
                    otherEntity.HasStableInternet = null;
                    otherEntity.HasCccameraSystem = null;
                }

                // 2️⃣ Special Features PDF
                if (model.OtherDetails?.SpecialFeaturesQuestion == "Yes" &&
    model.OtherDetails.SpecialFeaturesPdf != null &&
    model.OtherDetails.SpecialFeaturesPdf.Length > 0)
                {
                    var path = await SaveLibraryFileAsync(model.OtherDetails.SpecialFeaturesPdf, "SpecialFeatures");

                    if (path != null)
                    {
                        if (!string.IsNullOrEmpty(otherEntity.SpecialFeaturesAchievementsPdfPath) &&
                            System.IO.File.Exists(otherEntity.SpecialFeaturesAchievementsPdfPath))
                        {
                            System.IO.File.Delete(otherEntity.SpecialFeaturesAchievementsPdfPath);
                        }

                        otherEntity.SpecialFeaturesAchievementsPdfPath = path;
                        otherEntity.UploadedFileName = model.OtherDetails.SpecialFeaturesPdf.FileName;
                    }
                }
                else if (model.OtherDetails?.SpecialFeaturesQuestion == "No")
                {
                    if (!string.IsNullOrEmpty(otherEntity.SpecialFeaturesAchievementsPdfPath) &&
                        System.IO.File.Exists(otherEntity.SpecialFeaturesAchievementsPdfPath))
                    {
                        System.IO.File.Delete(otherEntity.SpecialFeaturesAchievementsPdfPath);
                    }

                    otherEntity.SpecialFeaturesAchievementsPdfPath = null;
                    otherEntity.UploadedFileName = null;
                }


                _context.SaveChanges();
                transaction.Commit();

                TempData["Success"] = "Medical Library details saved successfully.";
                return RedirectToAction(nameof(MedicalLibrary));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ModelState.AddModelError("", "Unexpected error: " + ex.Message);
                LoadMedicalLibraryMasters(model);
                return View("MedicalLibrary", model);
            }
        }

        private void LoadMedicalLibraryMasters(CA_Aff_MedicalLibraryViewModel model)
        {
            // Reload master lists for validation failure
            ViewBag.LibraryServiceMasters = _context.CaMstMediLibraryServices.OrderBy(s => s.ServiceId).ToList();
            ViewBag.DepartmentMasters = _context.DepartmentMasters
                .Where(d => d.FacultyCode == model.FacultyCode)
                .OrderBy(d => d.DepartmentCode)
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> ViewSpecialFeaturesPdf()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = HttpContext.Session.GetInt32("FacultyId") ?? 1;
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var record = await _context.CaMedicalLibraryOtherDetails.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.CourseLevel == courseLevel &&
                x.AffiliationType == affiliationType);

            if (record == null || string.IsNullOrEmpty(record.SpecialFeaturesAchievementsPdfPath))
                return NotFound("Special Features PDF not found.");

            if (!System.IO.File.Exists(record.SpecialFeaturesAchievementsPdfPath))
                return NotFound("File not found on server.");

            var fileName = record.UploadedFileName ?? Path.GetFileName(record.SpecialFeaturesAchievementsPdfPath);

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.SpecialFeaturesAchievementsPdfPath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return PhysicalFile(record.SpecialFeaturesAchievementsPdfPath, contentType);
        }
        [HttpGet]
        public async Task<IActionResult> ViewLibraryServicePdf(int serviceId)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = HttpContext.Session.GetInt32("FacultyId") ?? 1;
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var record = await _context.CaMedicalLibraryServices.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.AffiliationType == affiliationType &&
                x.CourseLevel == courseLevel &&
                x.ServiceId == serviceId);

            if (record == null || string.IsNullOrEmpty(record.UploadedPdfPath))
                return NotFound("PDF not found.");

            if (!System.IO.File.Exists(record.UploadedPdfPath))
                return NotFound("File not found on server.");

            var fileName = record.UploadedFileName ?? Path.GetFileName(record.UploadedPdfPath);

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.UploadedPdfPath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return PhysicalFile(record.UploadedPdfPath, contentType);
        }
        [HttpGet]
        public async Task<IActionResult> ViewUsageReportPdf()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = HttpContext.Session.GetInt32("FacultyId") ?? 1;
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var record = await _context.CaMedicalLibraryUsageReports.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.CourseLevel == courseLevel &&
                x.AffiliationType == affiliationType);

            if (record == null || string.IsNullOrEmpty(record.UploadedFileDataPath))
                return NotFound("Usage Report PDF not found.");

            if (!System.IO.File.Exists(record.UploadedFileDataPath))
                return NotFound("File not found on server.");

            var fileName = record.UploadedFileName ?? Path.GetFileName(record.UploadedFileDataPath);

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.UploadedFileDataPath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return PhysicalFile(record.UploadedFileDataPath, contentType);
        }

    }
}
