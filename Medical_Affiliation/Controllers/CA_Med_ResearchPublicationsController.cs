using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;
using System.IO;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    public class CA_Med_ResearchPublicationsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CA_Med_ResearchPublicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CA_Med_ResearchPublicationsDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");
            var levels = string.IsNullOrEmpty(raw)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(raw).Select(l => l.Trim().ToUpper()).Distinct().ToList();


            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            int facultyCodeInt = Convert.ToInt32(facultyCode);

            // ── Main research record (saved with CourseLevel = "ALL") ────────
            var mainDataList = await _context.CaMedResearchPublicationsDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            // FIX: was querying "ALL" but also need fallback for older "UG" rows
            var commonData = mainDataList
                    .FirstOrDefault(x => x.CourseLevel != null &&
                                         x.CourseLevel.Trim().ToUpper() == "ALL")
                ?? mainDataList.FirstOrDefault();  // fallback to any existing row

            if (commonData == null)
                commonData = new CaMedResearchPublicationsDetail();

            // ── Other Activities ─────────────────────────────────────────────
            var otherActivities = await _context.CaMedLibOtherAcademicActivities
                        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                        .Join(
                            _context.CaMstMedOtherAcademicActivities,
                            saved => saved.ActivityId,
                            master => master.Id,
                            (saved, master) => new CA_Med_Lib_OtherAcademicActivitiesVM
                            {
                                Id = saved.Id,
                                ActivityId = saved.ActivityId,
                                ActivityName = master.ActivityName,
                                DepartmentCode = saved.DepartmentCode,
                                DepartmentWise = saved.DepartmentWise,
                                ActivityPdfName = saved.ActivityPdfName
                            })
                        .ToListAsync();

            // ── Committee masters ─────────────────────────────────────────────
            // Load ALL-level committees + UG-level committees only if college has UG
            bool hasUG = levels.Contains("UG");

            var committeeMasters = await _context.CaMstMedCommitteeNames
            .Where(x =>
                x.FacultyCode == facultyCode &&
                x.CourseLevel != null &&
                (
                    x.CourseLevel.ToUpper() == "ALL" ||
                    x.CourseLevel.ToUpper() == "UG"
                ))
            .OrderBy(x => x.CommitteeName)
            .ToListAsync();

            var savedCommittees = await _context.CaMedLibCommittees
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            var savedCommitteesDict = savedCommittees
                .GroupBy(x => x.CommitteeId)
                .ToDictionary(g => g.Key, g => g.First());

            // ── Dropdowns ─────────────────────────────────────────────────────
            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCodeInt)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            var activityMasters = await _context.CaMstMedOtherAcademicActivities
                .OrderBy(x => x.ActivityName)
                .ToListAsync();

            // ── Build ViewModel ───────────────────────────────────────────────
            var vm = new CA_Med_ResearchPublicationsDetailsVM
            {
                PublicationsNo = commonData.PublicationsNo ?? 0,
                PublicationsPdfName = commonData.PublicationsPdfName,

                ClinicalTrialsPdfName = commonData.ClinicalTrialsPdfName,   // UG only (shown conditionally)

                StudentsRGUHSFunded = commonData.StudentsRguhsfunded,
                StudentsExternalBodyFunding = commonData.StudentsExternalBodyFunding,
                StudentsProjectsPdfName = commonData.StudentsProjectsPdfName,

                FacultyRGUHSFunded = commonData.FacultyRguhsfunded,
                FacultyExternalBodyFunding = commonData.FacultyExternalBodyFunding,
                FacultyProjectsPdfName = commonData.FacultyProjectsPdfName,

                OtherActivities = otherActivities,
                Departments = departments,
                ActivityMasters = activityMasters,

                // FIX: pass levels to the view so sections show/hide correctly
                ExistingCourseLevels = levels,

                Committees = committeeMasters.Select(m =>
                {
                    savedCommitteesDict.TryGetValue(m.Id, out var saved);
                    return new CA_Med_Lib_CommitteeVM
                    {
                        CommitteeId = m.Id,
                        CommitteeName = m.CommitteeName,
                        IsPresent = saved?.IsPresent ?? "",
                        CommitteePdfName = saved?.CommitteePdfName,
                        CourseLevel = m.CourseLevel
                    };
                }).ToList()
            };

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST — Main research + publications
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_Med_ResearchPublicationsDetails(
            CA_Med_ResearchPublicationsDetailsVM model,
            IFormFile? PublicationsPdf,
            IFormFile? StudentsProjectsPdf,
            IFormFile? FacultyProjectsPdf,
            IFormFile? ClinicalTrialsPdf)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var raw = HttpContext.Session.GetString("ExistingCourseLevels");
            var levels = ParseLevels(raw);

            bool hasUG = levels.Contains("UG");

            // Always saved under "ALL" — one shared row for all course levels
            const string level = "ALL";

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            // Load existing entity to check for already-uploaded files
            var entity = await _context.CaMedResearchPublicationsDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == level);

            // ── Required-file validation ──────────────────────────────────────
            if (PublicationsPdf == null && string.IsNullOrEmpty(entity?.PublicationsPdfPath))
                ModelState.AddModelError("PublicationsPdf", "Publications PDF is required.");

            if (StudentsProjectsPdf == null && string.IsNullOrEmpty(entity?.StudentsProjectsPdfPath))
                ModelState.AddModelError("StudentsProjectsPdf", "Students Projects PDF is required.");

            if (FacultyProjectsPdf == null && string.IsNullOrEmpty(entity?.FacultyProjectsPdfPath))
                ModelState.AddModelError("FacultyProjectsPdf", "Faculty Projects PDF is required.");

            // FIX: Clinical Trials PDF is only required when the college has UG
            // Previously this ran for ALL colleges, blocking PG/SS-only colleges

           

            
                if (ClinicalTrialsPdf == null && string.IsNullOrEmpty(entity?.ClinicalTrialsPdfPath))
                    ModelState.AddModelError("ClinicalTrialsPdf", "Clinical Trials PDF is required.");
            

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all required fields and upload required PDFs.";

                model.Departments = await _context.DepartmentMasters
                    .Where(x => x.FacultyCode == Convert.ToInt32(facultyCode))
                    .OrderBy(x => x.DepartmentName)
                    .ToListAsync();

                model.ActivityMasters = await _context.CaMstMedOtherAcademicActivities
                    .OrderBy(x => x.ActivityName)
                    .ToListAsync();

                model.ExistingCourseLevels = levels;   // FIX: preserve levels on validation failure

                return View(model);
            }

            // ── Upsert entity ─────────────────────────────────────────────────
            if (entity == null)
            {
                entity = new CaMedResearchPublicationsDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = level
                };
                _context.CaMedResearchPublicationsDetails.Add(entity);
            }

            entity.CourseLevel = level;
            entity.PublicationsNo = model.PublicationsNo;
            entity.StudentsRguhsfunded = model.StudentsRGUHSFunded;
            entity.StudentsExternalBodyFunding = model.StudentsExternalBodyFunding;
            entity.FacultyRguhsfunded = model.FacultyRGUHSFunded;
            entity.FacultyExternalBodyFunding = model.FacultyExternalBodyFunding;

            // ── File uploads ──────────────────────────────────────────────────
            await UploadFileIfProvided(PublicationsPdf, "Publications", entity,
                f => entity.PublicationsPdfPath = f,
                f => entity.PublicationsPdfName = f,
                () => entity.PublicationsPdfPath,
                () => entity.PublicationsPdfName);

            await UploadFileIfProvided(StudentsProjectsPdf, "StudentProjects", entity,
                f => entity.StudentsProjectsPdfPath = f,
                f => entity.StudentsProjectsPdfName = f,
                () => entity.StudentsProjectsPdfPath,
                () => entity.StudentsProjectsPdfName);

            await UploadFileIfProvided(FacultyProjectsPdf, "FacultyProjects", entity,
                f => entity.FacultyProjectsPdfPath = f,
                f => entity.FacultyProjectsPdfName = f,
                () => entity.FacultyProjectsPdfPath,
                () => entity.FacultyProjectsPdfName);

            // Clinical Trials — only upload if UG level exists
            
                await UploadFileIfProvided(ClinicalTrialsPdf, "ClinicalTrials", entity,
                    f => entity.ClinicalTrialsPdfPath = f,
                    f => entity.ClinicalTrialsPdfName = f,
                    () => entity.ClinicalTrialsPdfPath,
                    () => entity.ClinicalTrialsPdfName);
            

            await _context.SaveChangesAsync();

            TempData["Success"] = "Research and Publication details saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        // ─────────────────────────────────────────────────────────────────────
        // File upload helper — avoids 8× repeated blocks
        // ─────────────────────────────────────────────────────────────────────
        private async Task UploadFileIfProvided(
            IFormFile? file,
            string folder,
            CaMedResearchPublicationsDetail entity,
            Action<string> setPath,
            Action<string> setName,
            Func<string?> getOldPath,
            Func<string?> getOldName)
        {
            if (file == null || file.Length == 0) return;

            var path = await SaveResearchFileAsync(file, folder);
            if (path == null) return;

            var oldPath = getOldPath();
            if (!string.IsNullOrEmpty(oldPath) && System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            setPath(path);
            setName(file.FileName);
        }

        private async Task<string?> SaveResearchFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return null;

            string fullFolder = Path.Combine(BasePath, "ResearchPublications", folder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(fullFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            return fullPath;
        }

        // ─────────────────────────────────────────────────────────────────────
        // View PDF endpoints
        // FIX: was querying CourseLevel = "UG" but data is saved as "ALL"
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet] public async Task<IActionResult> ViewPublicationsPdf() => await GetPdf("Publications");
        [HttpGet] public async Task<IActionResult> ViewStudentsProjectsPdf() => await GetPdf("StudentsProjects");
        [HttpGet] public async Task<IActionResult> ViewFacultyProjectsPdf() => await GetPdf("FacultyProjects");
        [HttpGet] public async Task<IActionResult> ViewClinicalTrialsPdf() => await GetPdf("ClinicalTrials");

        private async Task<IActionResult> GetPdf(string type)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // FIX: query "ALL" (matches how data is saved), fallback to any row
            var record = await _context.CaMedResearchPublicationsDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == "ALL")
                ?? await _context.CaMedResearchPublicationsDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode);

            if (record == null) return NotFound("Record not found.");

            string? filePath = type switch
            {
                "Publications" => record.PublicationsPdfPath,
                "StudentsProjects" => record.StudentsProjectsPdfPath,
                "FacultyProjects" => record.FacultyProjectsPdfPath,
                "ClinicalTrials" => record.ClinicalTrialsPdfPath,
                _ => null
            };

            string? name = type switch
            {
                "Publications" => record.PublicationsPdfName,
                "StudentsProjects" => record.StudentsProjectsPdfName,
                "FacultyProjects" => record.FacultyProjectsPdfName,
                "ClinicalTrials" => record.ClinicalTrialsPdfName,
                _ => null
            };

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found on server. Please re-upload.");

            var fileName = string.IsNullOrEmpty(name) ? Path.GetFileName(filePath) : name;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return PhysicalFile(filePath, contentType);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Other Academic Activities
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherAcademicActivity(CA_Med_Lib_OtherActivityPostVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            //if (!ModelState.IsValid)
            //{
            //    var fullVm = await LoadFullViewModel();
            //    return View("CA_Med_ResearchPublicationsDetails", fullVm);
            //}

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = string.Join(", ", errors);

                var fullVm = await LoadFullViewModel();
                return View("CA_Med_ResearchPublicationsDetails", fullVm);
            }

            // Duplicate check — same department + activity regardless of course level
            bool alreadyExists = await _context.CaMedLibOtherAcademicActivities.AnyAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.DepartmentCode == model.DepartmentCode &&
                x.ActivityId == model.ActivityId &&
                x.CourseLevel != null &&
                x.CourseLevel.ToUpper() == "UG");

            if (alreadyExists)
            {
                TempData["Error"] =
                    "The selected Department and Activity have already been saved. " +
                    "Delete the existing record to re-upload.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            var departmentName = await _context.DepartmentMasters
                .Where(x => x.DepartmentCode == model.DepartmentCode)
                .Select(x => x.DepartmentName)
                .FirstOrDefaultAsync();

            var dbEntity = new CaMedLibOtherAcademicActivity
            {
                CollegeCode = collegeCode!,
                FacultyCode = facultyCode!,
                DepartmentCode = model.DepartmentCode,
                DepartmentWise = departmentName ?? "Unknown Department",
                ActivityId = model.ActivityId,
                CourseLevel = "UG"   // Other Academic Activities are UG-specific
            };

            if (model.ActivityPdf != null && model.ActivityPdf.Length > 0)
            {
                string basePath = Path.Combine(BasePath, "OtherAcademicActivities");
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ActivityPdf.FileName);
                string fullPath = Path.Combine(basePath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await model.ActivityPdf.CopyToAsync(stream);

                dbEntity.ActivityPdfPath = fullPath;
                dbEntity.ActivityPdfName = model.ActivityPdf.FileName;
            }

            _context.CaMedLibOtherAcademicActivities.Add(dbEntity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activity saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOtherAcademicActivity(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var activity = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (activity != null)
            {
                if (!string.IsNullOrEmpty(activity.ActivityPdfPath) &&
                    System.IO.File.Exists(activity.ActivityPdfPath))
                    System.IO.File.Delete(activity.ActivityPdfPath);

                _context.CaMedLibOtherAcademicActivities.Remove(activity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Activity deleted successfully.";
            }

            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        [HttpGet]
        public async Task<IActionResult> ViewOtherActivityPdf(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var record = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (record == null || string.IsNullOrEmpty(record.ActivityPdfPath))
                return NotFound("Record not found.");

            if (!System.IO.File.Exists(record.ActivityPdfPath))
                return NotFound("File not found on server. Please re-upload.");

            var fileName = string.IsNullOrEmpty(record.ActivityPdfName)
                ? Path.GetFileName(record.ActivityPdfPath)
                : record.ActivityPdfName;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.ActivityPdfPath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return PhysicalFile(record.ActivityPdfPath, contentType);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Committees
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommitteeDetails(CA_Med_Lib_CommitteePostVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (model.Committees.Any(c => string.IsNullOrEmpty(c.IsPresent)))
            {
                TempData["Error"] = "Please select Yes or No for all committees.";
                var vm = await LoadFullViewModel();
                vm.Committees = model.Committees;
                return View("CA_Med_ResearchPublicationsDetails", vm);
            }

            // --- FIX: RE-POPULATE PDF NAMES TO PREVENT DISAPPEARING VIEW BUTTONS ---
            // We do this before validation check so that if we return the view, the data is there
            var savedCommittees = await _context.CaMedLibCommittees
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            foreach (var item in model.Committees)
            {
                var existing = savedCommittees.FirstOrDefault(x => x.CommitteeId == item.CommitteeId);
                if (existing != null)
                {
                    item.CommitteePdfName = existing.CommitteePdfName; // Restore the file name from DB
                }
            }

            foreach (var item in model.Committees)
            {
                if (item.IsPresent == "Y")
                {
                    var existing = savedCommittees.FirstOrDefault(x => x.CommitteeId == item.CommitteeId);
                    bool hasExistingPdf = existing != null && !string.IsNullOrEmpty(existing.CommitteePdfPath);
                    bool hasNewPdf = item.CommitteePdf != null && item.CommitteePdf.Length > 0;

                    if (!hasExistingPdf && !hasNewPdf)
                    {
                        TempData["Error"] = "Please upload PDF document for all committees marked YES.";
                        var vm = await LoadFullViewModel();
                        vm.Committees = model.Committees; // Now contains restored PDF names
                        return View("CA_Med_ResearchPublicationsDetails", vm);
                    }
                }
            }

            foreach (var item in model.Committees)
            {
                var master = await _context.CaMstMedCommitteeNames.FirstOrDefaultAsync(x => x.Id == item.CommitteeId);
                var courseLevel = master?.CourseLevel ?? "ALL";
                var db = await _context.CaMedLibCommittees.FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CommitteeId == item.CommitteeId);

                if (db == null)
                {
                    db = new CaMedLibCommittee { CollegeCode = collegeCode!, FacultyCode = facultyCode!, CommitteeId = item.CommitteeId, CourseLevel = courseLevel };
                    _context.CaMedLibCommittees.Add(db);
                }
                db.IsPresent = item.IsPresent;
                db.CourseLevel = courseLevel;

                if (item.IsPresent == "Y")
                {
                    if (item.CommitteePdf != null && item.CommitteePdf.Length > 0)
                    {
                        string basePath = Path.Combine(BasePath, "CommitteeDocs");
                        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
                        string fileName = Guid.NewGuid() + Path.GetExtension(item.CommitteePdf.FileName);
                        string fullPath = Path.Combine(basePath, fileName);
                        using (var stream = new FileStream(fullPath, FileMode.Create)) await item.CommitteePdf.CopyToAsync(stream);
                        if (!string.IsNullOrEmpty(db.CommitteePdfPath) && System.IO.File.Exists(db.CommitteePdfPath)) System.IO.File.Delete(db.CommitteePdfPath);
                        db.CommitteePdfPath = fullPath;
                        db.CommitteePdfName = item.CommitteePdf.FileName;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(db.CommitteePdfPath) && System.IO.File.Exists(db.CommitteePdfPath)) System.IO.File.Delete(db.CommitteePdfPath);
                    db.CommitteePdfPath = null;
                    db.CommitteePdfName = null;
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Committee details saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        [HttpGet]
        public async Task<IActionResult> ViewCommitteePdf(int committeeId)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound("Session expired.");

            var record = await _context.CaMedLibCommittees
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CommitteeId == committeeId);

            if (record == null || string.IsNullOrEmpty(record.CommitteePdfPath))
                return NotFound("Record not found.");

            if (!System.IO.File.Exists(record.CommitteePdfPath))
                return NotFound("File not found on server. Please re-upload.");

            var fileName = string.IsNullOrEmpty(record.CommitteePdfName)
                ? Path.GetFileName(record.CommitteePdfPath)
                : record.CommitteePdfName;

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.CommitteePdfPath, out string contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return PhysicalFile(record.CommitteePdfPath, contentType);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────
        private List<string> ParseLevels(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string> { "UG" };

            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(raw);

                if (parsed != null && parsed.Any())
                {
                    return parsed
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim().ToUpper())
                        .Distinct()
                        .ToList();
                }
            }
            catch
            {
                // fallback below-
            }

            var fallback = raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpper())
                .Distinct()
                .ToList();

            return fallback.Any()
                ? fallback
                : new List<string> { "UG" };
        }

        private async Task<CA_Med_ResearchPublicationsDetailsVM> LoadFullViewModel()
        {
            var result = await CA_Med_ResearchPublicationsDetails();
            if (result is ViewResult viewResult &&
                viewResult.Model is CA_Med_ResearchPublicationsDetailsVM vm)
                return vm;

            return new CA_Med_ResearchPublicationsDetailsVM();
        }
    }
}
