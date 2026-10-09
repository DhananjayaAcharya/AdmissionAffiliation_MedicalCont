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

        public CA_Aff_MedicalLibraryController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        private async Task<(string CourseLevel, int AffiliationType)> ResolveLibraryContextAsync(
            int? postedAffiliationType = null,
            string? postedCourseLevel = null)
        {
            var queryCourseLevel = HttpContext.Request.Query["courseLevel"].FirstOrDefault();
            var courseLevel = (queryCourseLevel
                ?? HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel")
                ?? postedCourseLevel
                ?? CourseLevel)
                .Trim()
                .ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                HttpContext.Session.SetString("CourseLevel", courseLevel);
                HttpContext.Session.SetString("SelectedCourseLevel", courseLevel);
            }

            var queryAffiliationTypeId = HttpContext.Request.Query["affiliationTypeId"].FirstOrDefault();
            int? affiliationType = int.TryParse(queryAffiliationTypeId, out var queryTypeId) && queryTypeId > 0
                ? queryTypeId
                : null;

            var queryAffiliationName = HttpContext.Request.Query["typeOfAffiliation"].FirstOrDefault();
            if (!affiliationType.HasValue && !string.IsNullOrWhiteSpace(queryAffiliationName))
            {
                var normalizedName = queryAffiliationName.Trim().ToUpperInvariant();
                affiliationType = await _context.TypeOfAffiliations
                    .AsNoTracking()
                    .Where(x => x.TypeDescription.Trim().ToUpper() == normalizedName)
                    .Select(x => (int?)x.TypeId)
                    .FirstOrDefaultAsync();
            }

            affiliationType ??= HttpContext.Session.GetInt32("AffiliationType");
            if (!affiliationType.HasValue &&
                int.TryParse(HttpContext.Session.GetString("AffiliationTypeId"), out var sessionTypeId) &&
                sessionTypeId > 0)
            {
                affiliationType = sessionTypeId;
            }
            affiliationType ??= postedAffiliationType;
            affiliationType ??= 2;

            HttpContext.Session.SetInt32("AffiliationType", affiliationType.Value);
            HttpContext.Session.SetString("AffiliationTypeId", affiliationType.Value.ToString());

            if (!string.IsNullOrWhiteSpace(queryAffiliationName))
            {
                HttpContext.Session.SetString("TypeOfAffiliation", queryAffiliationName.Trim());
            }
            else
            {
                var affiliationName = await _context.TypeOfAffiliations
                    .AsNoTracking()
                    .Where(x => x.TypeId == affiliationType.Value)
                    .Select(x => x.TypeDescription)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(affiliationName))
                    HttpContext.Session.SetString("TypeOfAffiliation", affiliationName);
            }

            return (courseLevel, affiliationType.Value);
        }

        // Removes every ModelState entry that belongs to one posted row
        private void ClearModelStateFor(string prefix)
        {
            var keys = ModelState.Keys
                .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var key in keys)
                ModelState.Remove(key);
        }

        // =====================================================================
        // GET
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> MedicalLibrary()
        {
            var libraryContext = await ResolveLibraryContextAsync();
            var courseLevel = libraryContext.CourseLevel;
            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode"));
            int affiliationType = libraryContext.AffiliationType;

            var model = new CA_Aff_MedicalLibraryViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationType = affiliationType,
                CourseLevel = courseLevel
            };

            ViewBag.IsDentalFaculty = facultyCode == 2;

            // ===================== 1. LIBRARY SERVICES =====================
            var savedServices = _context.CaMedicalLibraryServices
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                           (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(x => x.LibraryServiceId)
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
            var usageCandidates = _context.CaMedicalLibraryUsageReports
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(x => x.UsageReportId)
                .ToList();
            var usage = usageCandidates.FirstOrDefault();

            if (usage != null)
                model.ExistingUsageReportFileName = usage.UploadedFileName;

            // ===================== 3. LIBRARY STAFF =====================
            var savedStaffCandidates = _context.CaMedicalLibraryStaffs
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenBy(x => x.Id)
                .ToList();
            var savedStaff = savedStaffCandidates.Any(x =>
                    x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                ? savedStaffCandidates.Where(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel).ToList()
                : savedStaffCandidates;

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
            var savedDepartmentCandidates = _context.CaMedicalDepartmentLibraries
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenBy(x => x.DepartmentCode)
                .ThenByDescending(x => x.DepartmentalLibraryId)
                .ToList();
            var savedDepartments = savedDepartmentCandidates.Any(x =>
                    x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                ? savedDepartmentCandidates.Where(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel).ToList()
                : savedDepartmentCandidates;

            if (savedDepartments.Any())
            {
                model.DepartmentLibraries = savedDepartments.Select(s =>
                {
                    string staff1 = "";
                    string staff2 = "";

                    if (!string.IsNullOrWhiteSpace(s.LibraryStaff))
                    {
                        var parts = s.LibraryStaff.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0) staff1 = parts[0].Trim();
                        if (parts.Length > 1) staff2 = parts[1].Trim();
                    }

                    return new DepartmentLibraryViewModel
                    {
                        Id = s.DepartmentalLibraryId,
                        DepartmentCode = s.DepartmentCode,
                        TotalBooks = s.TotalBooks,
                        BooksAddedInYear = s.BooksAddedInYear,
                        CurrentJournals = s.CurrentJournals,
                        LibraryStaff1 = staff1,
                        LibraryStaff2 = staff2,
                        Titles = s.Titles,
                        InternationalJournals = s.InternationalJournals,
                        BackVolumes = s.BackVolumes,
                        PrintJournalPercentage = s.PrintJournalPercentage,
                    };
                }).ToList();
            }
            else
            {
                // First visit: one empty row only
                model.DepartmentLibraries = new List<DepartmentLibraryViewModel>
                {
                    new DepartmentLibraryViewModel()
                };
            }

            // ===================== 5. OTHER DETAILS =====================
            var otherDetailsCandidates = _context.CaMedicalLibraryOtherDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .ToList();

            var otherDetails = otherDetailsCandidates.FirstOrDefault(x =>
                                   x.CourseLevel != null &&
                                   x.CourseLevel.Trim().ToUpper() == courseLevel)
                ?? otherDetailsCandidates.FirstOrDefault(x => string.IsNullOrEmpty(x.CourseLevel));

            if (otherDetails != null)
            {
                model.OtherDetails = new MedicalLibraryOtherDetailsViewModel
                {
                    DigitalValuationId = otherDetails.DigitalValuationId,
                    HasDigitalValuationCentre = NormalizeYesNo(otherDetails.HasDigitalValuationCentre),
                    NoOfSystems = otherDetails.NoOfSystems,
                    HasStableInternet = NormalizeYesNo(otherDetails.HasStableInternet),
                    HasCccameraSystem = NormalizeYesNo(otherDetails.HasCccameraSystem),
                    SpecialFeaturesQuestion = otherDetails.SpecialFeaturesAchievementsPdfPath != null ? "Yes" : "No",
                    HasSpecialFeaturesPdf = otherDetails.SpecialFeaturesAchievementsPdfPath != null,
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

            bool hasLibraryServicePdf = model.LibraryServices.Any(s => !string.IsNullOrEmpty(s.ExistingFileName));
            bool hasUsageReportPdf = !string.IsNullOrEmpty(model.ExistingUsageReportFileName);
            bool hasSpecialFeaturesPdf = model.OtherDetails?.HasSpecialFeaturesPdf == true;

            model.IsFirstLogin = !(hasLibraryServicePdf || hasUsageReportPdf || hasSpecialFeaturesPdf);

            // ===================== DENTAL LIBRARY RECORDS =====================
            if (facultyCode == 2)
            {
                var masterRecords = _context.CaMstDentalLibraryRecords
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

                var uploadedRecords = _context.CaDentalLibraryRecords
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                x.AffiliationType == affiliationType)
                    .ToList();

                model.DentalLibraryRecords = masterRecords.Select(m =>
                {
                    var uploaded = uploadedRecords.FirstOrDefault(x => x.RecordId == m.RecordId);
                    return new DentalLibraryRecordViewModel
                    {
                        RecordId = m.RecordId,
                        RecordName = m.RecordName,
                        ExistingFileName = uploaded?.FileName
                    };
                }).ToList();
            }

            return View("MedicalLibrary", model);
        }

        private async Task<string?> SaveLibraryFileAsync(
            IFormFile? file,
            string folder,
            string facultyCode)
        {
            if (file == null || file.Length == 0)
                return null;

            string rootPath = facultyCode == "2" ? BaseDentalPath : BaseMedicalPath;
            string basePath = Path.Combine(rootPath, "MedicalLibrary");
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

        // =====================================================================
        // POST
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicalLibrary(CA_Aff_MedicalLibraryViewModel model)
        {
            if (model == null)
                return RedirectToAction(nameof(MedicalLibrary));

            var libraryContext = await ResolveLibraryContextAsync(model.AffiliationType, model.CourseLevel);
            var courseLevel = libraryContext.CourseLevel;

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode"));
            int affiliationType = libraryContext.AffiliationType;
            string facultyCodeText = facultyCode.ToString(); // NEW

            model.CollegeCode = collegeCode;
            model.FacultyCode = facultyCode;
            model.AffiliationType = affiliationType;
            model.CourseLevel = courseLevel;

            model.LibraryServices ??= new List<LibraryServiceRowViewModel>();
            model.LibraryStaff ??= new List<LibraryStaffViewModel>();
            model.DepartmentLibraries ??= new List<DepartmentLibraryViewModel>();
            model.DentalLibraryRecords ??= new List<DentalLibraryRecordViewModel>();

            // NEW: PDF rules are checked against the database below, so drop the stale
            // view-model errors (ExistingFileName / HasSpecialFeaturesPdf are not posted back)
            var stalePdfKeys = ModelState.Keys
                .Where(k => k.EndsWith(".UploadedPdf", StringComparison.OrdinalIgnoreCase)
                         || k.EndsWith("SpecialFeaturesPdf", StringComparison.OrdinalIgnoreCase)
                         || k.Equals("UsageReportPdf", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var key in stalePdfKeys)
                ModelState.Remove(key);

            // =====================================================
            // MEDICAL VALIDATIONS
            // =====================================================
            if (facultyCode != 2)
            {
                var existingServices = _context.CaMedicalLibraryServices
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                                x.AffiliationType == affiliationType)
                    .ToList();

                // CHANGED: for loop + field key so the message shows next to the right input
                for (var i = 0; i < model.LibraryServices.Count; i++)
                {
                    var row = model.LibraryServices[i];

                    bool pdfExists = existingServices.Any(x =>
                        x.ServiceId == row.ServiceId &&
                        !string.IsNullOrEmpty(x.UploadedFileName));

                    if (row.ServiceId == 6 &&
                        row.IsAvailable == "Yes" &&
                        row.UploadedPdf == null &&
                        !pdfExists)
                    {
                        ModelState.AddModelError($"LibraryServices[{i}].UploadedPdf",
                            "User Education Programme PDF is mandatory.");
                    }
                }

                if (model.OtherDetails != null &&
                    model.OtherDetails.HasDigitalValuationCentre == "Yes")
                {
                    if (!model.OtherDetails.NoOfSystems.HasValue || model.OtherDetails.NoOfSystems <= 0)
                        ModelState.AddModelError("OtherDetails.NoOfSystems", "Number of systems is required.");

                    if (string.IsNullOrWhiteSpace(model.OtherDetails.HasStableInternet))
                        ModelState.AddModelError("OtherDetails.HasStableInternet", "LAN / Stable Internet is required.");

                    if (string.IsNullOrWhiteSpace(model.OtherDetails.HasCccameraSystem))
                        ModelState.AddModelError("OtherDetails.HasCccameraSystem", "CCTV Camera System is required.");
                }

                bool hasExistingUsagePdf = _context.CaMedicalLibraryUsageReports.Any(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                    x.AffiliationType == affiliationType &&
                    !string.IsNullOrEmpty(x.UploadedFileName));

                if (!hasExistingUsagePdf && model.UsageReportPdf == null)
                    ModelState.AddModelError(nameof(model.UsageReportPdf), "Usage Report PDF is mandatory.");

                if (model.OtherDetails?.HasDigitalValuationCentre == "No")
                {
                    ModelState.Remove("OtherDetails.NoOfSystems");
                    ModelState.Remove("OtherDetails.HasStableInternet");
                    ModelState.Remove("OtherDetails.HasCccameraSystem");
                }

                bool hasExistingSpecialPdf = _context.CaMedicalLibraryOtherDetails.Any(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                    x.AffiliationType == affiliationType &&
                    x.SpecialFeaturesAchievementsPdfPath != null);

                if (model.OtherDetails?.SpecialFeaturesQuestion == "Yes")
                {
                    if (model.OtherDetails.SpecialFeaturesPdf == null && !hasExistingSpecialPdf)
                        ModelState.AddModelError("OtherDetails.SpecialFeaturesPdf", "Special Features PDF is required.");
                }
                else
                {
                    ModelState.Remove("OtherDetails.SpecialFeaturesPdf");
                }

                if (string.IsNullOrWhiteSpace(model.OtherDetails?.HasDigitalValuationCentre))
                    ModelState.AddModelError("OtherDetails.HasDigitalValuationCentre", "Please select Yes or No for Digital Valuation Centre.");

                if (string.IsNullOrWhiteSpace(model.OtherDetails?.SpecialFeaturesQuestion))
                    ModelState.AddModelError("OtherDetails.SpecialFeaturesQuestion", "Please select Yes or No for Special Features.");
            }

            // =====================================================
            // DENTAL VALIDATIONS
            // =====================================================
            if (facultyCode == 2)
            {
                var activeDepartments = model.DepartmentLibraries.Where(d => !d.IsDeleted).ToList();

                if (!activeDepartments.Any())
                    ModelState.AddModelError("", "At least one department is required.");

                foreach (var dept in activeDepartments)
                {
                    if (string.IsNullOrWhiteSpace(dept.DepartmentCode))
                        ModelState.AddModelError("", "Department is required.");
                }

                ModelState.Remove("OtherDetails.SpecialFeaturesQuestion");
                ModelState.Remove("OtherDetails.HasDigitalValuationCentre");
                ModelState.Remove("OtherDetails.NoOfSystems");
                ModelState.Remove("OtherDetails.HasStableInternet");
                ModelState.Remove("OtherDetails.HasCccameraSystem");
                ModelState.Remove("UsageReportPdf");
            }

            // Blank template rows and rows marked as deleted must not produce validation errors
            for (var index = 0; index < model.DepartmentLibraries.Count; index++)
            {
                var department = model.DepartmentLibraries[index];
                var isBlank = string.IsNullOrWhiteSpace(department.DepartmentCode)
                    && !department.TotalBooks.HasValue
                    && !department.BooksAddedInYear.HasValue
                    && !department.CurrentJournals.HasValue
                    && string.IsNullOrWhiteSpace(department.LibraryStaff1)
                    && string.IsNullOrWhiteSpace(department.LibraryStaff2);

                if (isBlank || department.IsDeleted)
                    ClearModelStateFor($"DepartmentLibraries[{index}].");
            }

            for (var index = 0; index < model.LibraryStaff.Count; index++)
            {
                var staff = model.LibraryStaff[index];
                var isBlank = string.IsNullOrWhiteSpace(staff.StaffName)
                    && string.IsNullOrWhiteSpace(staff.Designation)
                    && string.IsNullOrWhiteSpace(staff.Qualification)
                    && !staff.Experience.HasValue
                    && string.IsNullOrWhiteSpace(staff.Category);

                if (isBlank || staff.IsDeleted)
                    ClearModelStateFor($"LibraryStaff[{index}].");
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
                // ---------- 1. LIBRARY SERVICES ----------
                foreach (var row in model.LibraryServices)
                {
                    var entity = _context.CaMedicalLibraryServices
                        .FirstOrDefault(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
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

                    if (row.ServiceId == 6 &&
                        row.UploadedPdf != null &&
                        row.UploadedPdf.Length > 0)
                    {
                        var path = await SaveLibraryFileAsync(row.UploadedPdf, "LibraryServices", facultyCodeText); // CHANGED

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

                // ---------- 2. USAGE REPORT ----------
                var usage = _context.CaMedicalLibraryUsageReports
                    .FirstOrDefault(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
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

                if (usage != null &&
                    model.UsageReportPdf != null &&
                    model.UsageReportPdf.Length > 0)
                {
                    var path = await SaveLibraryFileAsync(model.UsageReportPdf, "UsageReports", facultyCodeText); // CHANGED

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

                // ---------- 3. LIBRARY STAFF (update in place) ----------
                var existingStaff = _context.CaMedicalLibraryStaffs
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                                x.AffiliationType == affiliationType)
                    .ToList();

                foreach (var staff in model.LibraryStaff)
                {
                    var entity = staff.Id > 0
                        ? existingStaff.FirstOrDefault(x => x.Id == staff.Id)
                        : null;

                    if (staff.IsDeleted)
                    {
                        if (entity != null)
                            _context.CaMedicalLibraryStaffs.Remove(entity);
                        continue;
                    }

                    var isBlank = string.IsNullOrWhiteSpace(staff.StaffName)
                        && string.IsNullOrWhiteSpace(staff.Designation)
                        && string.IsNullOrWhiteSpace(staff.Qualification)
                        && !staff.Experience.HasValue
                        && string.IsNullOrWhiteSpace(staff.Category);
                    if (isBlank)
                        continue;

                    if (entity == null)
                    {
                        entity = new CaMedicalLibraryStaff
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            AffiliationType = affiliationType
                        };
                        _context.CaMedicalLibraryStaffs.Add(entity);
                    }

                    entity.CourseLevel = courseLevel;
                    entity.StaffName = staff.StaffName;
                    entity.Designation = staff.Designation;
                    entity.Qualification = staff.Qualification;
                    entity.Experience = staff.Experience ?? 0;
                    entity.Category = staff.Category;
                }

                // ---------- 4. DEPARTMENT LIBRARY (update in place) ----------
                var existingDepts = _context.CaMedicalDepartmentLibraries
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                                x.AffiliationType == affiliationType)
                    .ToList();

                foreach (var dept in model.DepartmentLibraries)
                {
                    var entity = dept.Id > 0
                        ? existingDepts.FirstOrDefault(x => x.DepartmentalLibraryId == dept.Id)
                        : null;

                    if (dept.IsDeleted)
                    {
                        if (entity != null)
                            _context.CaMedicalDepartmentLibraries.Remove(entity);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(dept.DepartmentCode))
                        continue;

                    var staffList = new List<string>();
                    if (!string.IsNullOrWhiteSpace(dept.LibraryStaff1)) staffList.Add(dept.LibraryStaff1.Trim());
                    if (!string.IsNullOrWhiteSpace(dept.LibraryStaff2)) staffList.Add(dept.LibraryStaff2.Trim());

                    if (entity == null)
                    {
                        entity = new CaMedicalDepartmentLibrary
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            AffiliationType = affiliationType
                        };
                        _context.CaMedicalDepartmentLibraries.Add(entity);
                    }

                    entity.CourseLevel = courseLevel;
                    entity.DepartmentCode = dept.DepartmentCode;
                    entity.TotalBooks = dept.TotalBooks ?? 0;
                    entity.BooksAddedInYear = dept.BooksAddedInYear ?? 0;
                    entity.CurrentJournals = dept.CurrentJournals ?? 0;
                    entity.Titles = dept.Titles;
                    entity.InternationalJournals = dept.InternationalJournals;
                    entity.BackVolumes = dept.BackVolumes;
                    entity.PrintJournalPercentage = dept.PrintJournalPercentage;
                    entity.LibraryStaff = string.Join(" | ", staffList);
                }

                // ---------- 5. OTHER DETAILS (medical only) ----------
                if (facultyCode != 2)
                {
                    var otherEntityCandidates = _context.CaMedicalLibraryOtherDetails
                        .Where(x => x.CollegeCode == collegeCode &&
                                    x.FacultyCode == facultyCode &&
                                    (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                                    x.AffiliationType == affiliationType)
                        .ToList();

                    var otherEntity = otherEntityCandidates.FirstOrDefault(x =>
                                           x.CourseLevel != null &&
                                           x.CourseLevel.Trim().ToUpper() == courseLevel)
                        ?? otherEntityCandidates.FirstOrDefault(x => string.IsNullOrEmpty(x.CourseLevel));

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
                        otherEntity.HasDigitalValuationCentre = model.OtherDetails.HasDigitalValuationCentre;

                    if (model.OtherDetails?.HasDigitalValuationCentre == "Yes")
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

                    if (model.OtherDetails?.SpecialFeaturesQuestion == "Yes" &&
                        model.OtherDetails.SpecialFeaturesPdf != null &&
                        model.OtherDetails.SpecialFeaturesPdf.Length > 0)
                    {
                        var path = await SaveLibraryFileAsync(
                            model.OtherDetails.SpecialFeaturesPdf, "SpecialFeatures", facultyCodeText); // CHANGED

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
                }

                // ---------- DENTAL LIBRARY RECORDS ----------
                if (facultyCode == 2)
                {
                    var oldRecords = _context.CaDentalLibraryRecords
                        .Where(x => x.CollegeCode == collegeCode &&
                                    x.FacultyCode == facultyCode &&
                                    x.AffiliationType == affiliationType)
                        .ToList();

                    foreach (var row in model.DentalLibraryRecords)
                    {
                        var entity = oldRecords.FirstOrDefault(x => x.RecordId == row.RecordId);

                        if (entity == null)
                        {
                            entity = new CaDentalLibraryRecord
                            {
                                CollegeCode = collegeCode,
                                FacultyCode = facultyCode,
                                CourseLevel = courseLevel,
                                AffiliationType = affiliationType,
                                RecordId = row.RecordId
                            };
                            _context.CaDentalLibraryRecords.Add(entity);
                        }

                        if (row.UploadFile != null && row.UploadFile.Length > 0)
                        {
                            var path = await SaveLibraryFileAsync(row.UploadFile, "DentalLibraryRecords", facultyCodeText); // CHANGED

                            if (path != null)
                            {
                                if (!string.IsNullOrEmpty(entity.FilePath) &&
                                    System.IO.File.Exists(entity.FilePath))
                                {
                                    System.IO.File.Delete(entity.FilePath);
                                }

                                entity.FilePath = path;
                                entity.FileName = row.UploadFile.FileName;
                            }
                        }
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

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

        [HttpGet]
        public async Task<IActionResult> ViewDentalLibraryRecord(int recordId)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode"));
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var courseLevel = (HttpContext.Session.GetString("CourseLevel") ?? CourseLevel)
                .Trim().ToUpperInvariant();

            var record = await _context.CaDentalLibraryRecords
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                    x.AffiliationType == affiliationType &&
                    x.RecordId == recordId);

            if (record == null || string.IsNullOrEmpty(record.FilePath))
                return NotFound();

            if (!System.IO.File.Exists(record.FilePath))
                return NotFound();

            return PhysicalFile(record.FilePath, "application/pdf");
        }

        private void LoadMedicalLibraryMasters(CA_Aff_MedicalLibraryViewModel model)
        {
            ViewBag.LibraryServiceMasters = _context.CaMstMediLibraryServices.OrderBy(s => s.ServiceId).ToList();
            ViewBag.DepartmentMasters = _context.DepartmentMasters
                .Where(d => d.FacultyCode == model.FacultyCode)
                .OrderBy(d => d.DepartmentCode)
                .ToList();
            ViewBag.IsDentalFaculty = model.FacultyCode == 2;

            RestoreExistingFiles(model);
        }

        // Existing-file info is not posted back, so reload it before re-rendering the view
        private void RestoreExistingFiles(CA_Aff_MedicalLibraryViewModel model)
        {
            var collegeCode = model.CollegeCode;
            var facultyCode = model.FacultyCode;
            var affiliationType = model.AffiliationType;
            var courseLevel = (model.CourseLevel ?? string.Empty).Trim().ToUpperInvariant();

            // Library services
            var savedServices = _context.CaMedicalLibraryServices
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .ToList();

            foreach (var row in model.LibraryServices ?? new List<LibraryServiceRowViewModel>())
            {
                row.ExistingFileName = savedServices
                    .Where(s => s.ServiceId == row.ServiceId && !string.IsNullOrEmpty(s.UploadedFileName))
                    .OrderByDescending(s => s.LibraryServiceId)
                    .Select(s => s.UploadedFileName)
                    .FirstOrDefault();
            }

            // Usage report
            model.ExistingUsageReportFileName = _context.CaMedicalLibraryUsageReports
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType &&
                            !string.IsNullOrEmpty(x.UploadedFileName))
                .OrderByDescending(x => x.UsageReportId)
                .Select(x => x.UploadedFileName)
                .FirstOrDefault();

            // Special features
            var other = _context.CaMedicalLibraryOtherDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .FirstOrDefault();

            if (model.OtherDetails != null)
            {
                model.OtherDetails.HasSpecialFeaturesPdf = other?.SpecialFeaturesAchievementsPdfPath != null;
                model.OtherDetails.UploadedFileName = other?.UploadedFileName;
            }

            // Dental library records (name + existing file)
            if (facultyCode == 2 && model.DentalLibraryRecords != null)
            {
                var masters = _context.CaMstDentalLibraryRecords.ToList();
                var uploaded = _context.CaDentalLibraryRecords
                    .Where(x => x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode &&
                                x.AffiliationType == affiliationType)
                    .ToList();

                foreach (var rec in model.DentalLibraryRecords)
                {
                    rec.RecordName = masters.FirstOrDefault(m => m.RecordId == rec.RecordId)?.RecordName;
                    rec.ExistingFileName = uploaded.FirstOrDefault(u => u.RecordId == rec.RecordId)?.FileName;
                }
            }
        }

        private static string? NormalizeYesNo(string? value)
        {
            var normalized = value?.Trim();

            if (string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "y", StringComparison.OrdinalIgnoreCase))
                return "Yes";

            if (string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "n", StringComparison.OrdinalIgnoreCase))
                return "No";

            return normalized;
        }

        [HttpGet]
        public async Task<IActionResult> ViewSpecialFeaturesPdf()
        {
            var courseLevel = (HttpContext.Session.GetString("CourseLevel") ?? CourseLevel)
                .Trim().ToUpperInvariant();

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode") ?? "1");
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var record = await _context.CaMedicalLibraryOtherDetails.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
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
            var courseLevel = (HttpContext.Session.GetString("CourseLevel") ?? CourseLevel)
                .Trim().ToUpperInvariant();

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode") ?? "1");
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var record = await _context.CaMedicalLibraryServices.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.AffiliationType == affiliationType &&
                (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
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
            var courseLevel = (HttpContext.Session.GetString("CourseLevel") ?? CourseLevel)
                .Trim().ToUpperInvariant();

            string collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            int facultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode") ?? "1");
            int affiliationType = HttpContext.Session.GetInt32("AffiliationType") ?? 2;

            var record = await _context.CaMedicalLibraryUsageReports.FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
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