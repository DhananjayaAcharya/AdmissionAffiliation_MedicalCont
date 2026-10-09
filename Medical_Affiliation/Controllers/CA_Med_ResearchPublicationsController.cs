using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;
using System.Net.Mime;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    public class CA_Med_ResearchPublicationsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CA_Med_ResearchPublicationsController> _logger;

        private const long MaxPdfBytes = 5 * 1024 * 1024;   // 5 MB (same as the view)
        private const string MainLevel = "ALL";              // main research row is shared by all levels

        public CA_Med_ResearchPublicationsController(
            ApplicationDbContext context,
            ILogger<CA_Med_ResearchPublicationsController> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        // ═════════════════════════════════════════════════════════════════════
        // GET  (Read)
        // ═════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> CA_Med_ResearchPublicationsDetails()
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();

            if (string.IsNullOrEmpty(collegeCode) ||
                string.IsNullOrEmpty(facultyCode) ||
                !int.TryParse(facultyCode, out var facultyCodeInt))
                return RedirectToAction("Login", "Account");

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var typeId = ReadAffiliationTypeId();
            var levels = ParseLevels(HttpContext.Session.GetString("ExistingCourseLevels"));

            // ── Main research record (one row per college/faculty) ───────────
            var commonData = await GetMainRecordAsync(collegeCode, facultyCode)
                             ?? new CaMedResearchPublicationsDetail();

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

            // ── Committees ───────────────────────────────────────────────────
            var committeeMasters = await _context.CaMstMedCommitteeNames
                .Where(x =>
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel != null &&
                    (x.CourseLevel.ToUpper() == "ALL" || x.CourseLevel.ToUpper() == "UG"))
                .OrderBy(x => x.CommitteeName)
                .ToListAsync();

            var savedCommittees = await _context.CaMedLibCommittees
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            var savedCommitteesDict = savedCommittees
                .GroupBy(x => x.CommitteeId)
                .ToDictionary(g => g.Key, g => g.First());

            // ── Departments ──────────────────────────────────────────────────
            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCodeInt)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            var savedDeptPublications = await _context.DeptWisePublications
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeInt)
                .ToListAsync();

            var savedDeptResearchProjects = await _context.DepartmentWiseResearchProjects
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCodeInt &&
                            x.CourseLevel == courseLevel &&
                            x.TypeId == typeId)
                .ToListAsync();

            var activityMasters = await _context.CaMstMedOtherAcademicActivities
                .OrderBy(x => x.ActivityName)
                .ToListAsync();

            // ── View model ───────────────────────────────────────────────────
            var vm = new CA_Med_ResearchPublicationsDetailsVM
            {
                PublicationsNo = commonData.PublicationsNo ?? 0,
                PublicationsPdfName = commonData.PublicationsPdfName,

                ClinicalTrialsPdfName = commonData.ClinicalTrialsPdfName,

                StudentsRGUHSFunded = commonData.StudentsRguhsfunded,
                StudentsExternalBodyFunding = commonData.StudentsExternalBodyFunding,
                StudentsProjectsPdfName = commonData.StudentsProjectsPdfName,

                FacultyRGUHSFunded = commonData.FacultyRguhsfunded,
                FacultyExternalBodyFunding = commonData.FacultyExternalBodyFunding,
                FacultyProjectsPdfName = commonData.FacultyProjectsPdfName,

                OtherActivities = otherActivities,
                Departments = departments,
                ActivityMasters = activityMasters,
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
                }).ToList(),

                DepartmentWisePublications = departments.Select(d =>
                {
                    var saved = savedDeptPublications.FirstOrDefault(x => x.DeptCode == d.DepartmentCode);
                    return new DepartmentWisePublicationVM
                    {
                        Id = saved?.Id ?? 0,
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCodeInt,
                        DeptCode = d.DepartmentCode,
                        DeptName = d.DepartmentName,
                        PublicationsCount = saved?.PublicationsCount ?? 0,
                        PublicationPath = saved?.PublicationPath
                    };
                }).ToList(),

                DepartmentWiseResearchProjects = departments.Select(d =>
                {
                    var saved = savedDeptResearchProjects.FirstOrDefault(e => e.DepartmentCode == d.DepartmentCode);
                    return new DepartmentWiseResearchProjectVM
                    {
                        Id = saved?.Id ?? 0,
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCodeInt,
                        CourseLevel = courseLevel,
                        DepartmentCode = d.DepartmentCode,
                        DepartmentName = d.DepartmentName,
                        NoOfResearchProjectsLast3Years = saved?.NoOfResearchProjectsLast3Years ?? 0,
                        PdfFilePath = saved?.PdfFilePath
                    };
                }).ToList()
            };

            return View(vm);
        }

        // ═════════════════════════════════════════════════════════════════════
        // POST — Main research + publications + department-wise (Create / Update)
        // ═════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_Med_ResearchPublicationsDetails(
            CA_Med_ResearchPublicationsDetailsVM model,
            IFormFile? PublicationsPdf,
            IFormFile? StudentsProjectsPdf,
            IFormFile? FacultyProjectsPdf,
            IFormFile? ClinicalTrialsPdf)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();

            if (string.IsNullOrEmpty(collegeCode) ||
                string.IsNullOrEmpty(facultyCode) ||
                !int.TryParse(facultyCode, out var facultyCodeInt))
                return RedirectToAction("Login", "Account");

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var typeId = ReadAffiliationTypeId();

            // ── Ignore model-state noise for fields that are never posted ────
            foreach (var key in new[] { "Departments", "ActivityMasters", "OtherActivities", "Committees", "ExistingCourseLevels" })
                ModelState.Remove(key);

            string[] notPosted = { ".CollegeCode", ".FacultyCode", ".CourseLevel", ".PublicationPath", ".PdfFilePath", ".PublicationPdf", ".ResearchProjectsPdf" };
            foreach (var key in ModelState.Keys.ToList())
            {
                if ((key.StartsWith("DepartmentWisePublications[") || key.StartsWith("DepartmentWiseResearchProjects[")) &&
                    notPosted.Any(s => key.EndsWith(s)))
                    ModelState.Remove(key);
            }

            // ── Load the SINGLE existing row so we UPDATE instead of INSERT ──
            var entity = await GetMainRecordAsync(collegeCode, facultyCode);

            // ── Required-file validation (file already on server counts) ─────
            if (PublicationsPdf == null && string.IsNullOrEmpty(entity?.PublicationsPdfPath))
                ModelState.AddModelError("PublicationsPdf", "Publications PDF is required.");

            if (StudentsProjectsPdf == null && string.IsNullOrEmpty(entity?.StudentsProjectsPdfPath))
                ModelState.AddModelError("StudentsProjectsPdf", "Students Projects PDF is required.");

            if (FacultyProjectsPdf == null && string.IsNullOrEmpty(entity?.FacultyProjectsPdfPath))
                ModelState.AddModelError("FacultyProjectsPdf", "Faculty Projects PDF is required.");

            // Clinical Trials is compulsory for every college (view shows it for all).
            // To make it UG-only, wrap this check in: if (ParseLevels(...).Contains("UG"))
            if (ClinicalTrialsPdf == null && string.IsNullOrEmpty(entity?.ClinicalTrialsPdfPath))
                ModelState.AddModelError("ClinicalTrialsPdf", "Clinical Trials PDF is required.");

            // ── Server-side PDF checks (type, size, header) ──────────────────
            await AddPdfErrorAsync(PublicationsPdf, "PublicationsPdf");
            await AddPdfErrorAsync(StudentsProjectsPdf, "StudentsProjectsPdf");
            await AddPdfErrorAsync(FacultyProjectsPdf, "FacultyProjectsPdf");
            await AddPdfErrorAsync(ClinicalTrialsPdf, "ClinicalTrialsPdf");

            // ── Number checks ────────────────────────────────────────────────
            if (model.PublicationsNo < 0) ModelState.AddModelError("PublicationsNo", "Value cannot be negative.");
            if (model.StudentsRGUHSFunded < 0) ModelState.AddModelError("StudentsRGUHSFunded", "Value cannot be negative.");
            if (model.StudentsExternalBodyFunding < 0) ModelState.AddModelError("StudentsExternalBodyFunding", "Value cannot be negative.");
            if (model.FacultyRGUHSFunded < 0) ModelState.AddModelError("FacultyRGUHSFunded", "Value cannot be negative.");
            if (model.FacultyExternalBodyFunding < 0) ModelState.AddModelError("FacultyExternalBodyFunding", "Value cannot be negative.");

            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCodeInt)
                .ToListAsync();

            if (model.DepartmentWisePublications != null)
            {
                for (int i = 0; i < model.DepartmentWisePublications.Count; i++)
                {
                    var it = model.DepartmentWisePublications[i];
                    if (it.PublicationsCount < 0)
                        ModelState.AddModelError($"DepartmentWisePublications[{i}].PublicationsCount", "Value cannot be negative.");
                    await AddPdfErrorAsync(it.PublicationPdf, $"DepartmentWisePublications[{i}].PublicationPdf");
                }
            }

            if (model.DepartmentWiseResearchProjects != null)
            {
                for (int i = 0; i < model.DepartmentWiseResearchProjects.Count; i++)
                {
                    var it = model.DepartmentWiseResearchProjects[i];
                    if (it.NoOfResearchProjectsLast3Years < 0)
                        ModelState.AddModelError($"DepartmentWiseResearchProjects[{i}].NoOfResearchProjectsLast3Years", "Value cannot be negative.");
                    await AddPdfErrorAsync(it.ResearchProjectsPdf, $"DepartmentWiseResearchProjects[{i}].ResearchProjectsPdf");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all required fields and upload required PDFs.";

                // Rebuild every section from the DB (keeps View PDF buttons, committees, activities).
                // The posted values stay visible because ModelState wins over Model in tag helpers.
                var vm = await LoadFullViewModel();
                return View(vm);
            }

            // ── Upsert main row (UPDATE if it exists, INSERT only the first time) ──
            if (entity == null)
            {
                entity = new CaMedResearchPublicationsDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode
                };
                _context.CaMedResearchPublicationsDetails.Add(entity);
            }

            entity.CourseLevel = MainLevel;   // normalise old "UG" rows to "ALL"
            entity.PublicationsNo = model.PublicationsNo;
            entity.StudentsRguhsfunded = model.StudentsRGUHSFunded;
            entity.StudentsExternalBodyFunding = model.StudentsExternalBodyFunding;
            entity.FacultyRguhsfunded = model.FacultyRGUHSFunded;
            entity.FacultyExternalBodyFunding = model.FacultyExternalBodyFunding;

            var newFiles = new List<string>();   // rolled back if SaveChanges fails
            var oldFiles = new List<string>();   // deleted only after SaveChanges succeeds

            try
            {
                // ── Main file uploads ────────────────────────────────────────
                if (HasFile(PublicationsPdf))
                {
                    var p = await SavePdfAsync(PublicationsPdf!, facultyCode, "ResearchPublications", "Publications");
                    newFiles.Add(p); QueueOld(oldFiles, entity.PublicationsPdfPath);
                    entity.PublicationsPdfPath = p; entity.PublicationsPdfName = Path.GetFileName(PublicationsPdf!.FileName);
                }

                if (HasFile(StudentsProjectsPdf))
                {
                    var p = await SavePdfAsync(StudentsProjectsPdf!, facultyCode, "ResearchPublications", "StudentProjects");
                    newFiles.Add(p); QueueOld(oldFiles, entity.StudentsProjectsPdfPath);
                    entity.StudentsProjectsPdfPath = p; entity.StudentsProjectsPdfName = Path.GetFileName(StudentsProjectsPdf!.FileName);
                }

                if (HasFile(FacultyProjectsPdf))
                {
                    var p = await SavePdfAsync(FacultyProjectsPdf!, facultyCode, "ResearchPublications", "FacultyProjects");
                    newFiles.Add(p); QueueOld(oldFiles, entity.FacultyProjectsPdfPath);
                    entity.FacultyProjectsPdfPath = p; entity.FacultyProjectsPdfName = Path.GetFileName(FacultyProjectsPdf!.FileName);
                }

                if (HasFile(ClinicalTrialsPdf))
                {
                    var p = await SavePdfAsync(ClinicalTrialsPdf!, facultyCode, "ResearchPublications", "ClinicalTrials");
                    newFiles.Add(p); QueueOld(oldFiles, entity.ClinicalTrialsPdfPath);
                    entity.ClinicalTrialsPdfPath = p; entity.ClinicalTrialsPdfName = Path.GetFileName(ClinicalTrialsPdf!.FileName);
                }

                // ── Department-wise publications ─────────────────────────────
                if (model.DepartmentWisePublications?.Any() == true)
                {
                    var existingPubs = await _context.DeptWisePublications
                        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeInt)
                        .ToListAsync();

                    foreach (var item in model.DepartmentWisePublications)
                    {
                        var dept = departments.FirstOrDefault(d => d.DepartmentCode == item.DeptCode);
                        if (dept == null) continue;   // ignore tampered / foreign departments

                        // 1) match by Id  2) match by department  3) only then create
                        var deptEntity =
                            (item.Id > 0 ? existingPubs.FirstOrDefault(x => x.Id == item.Id) : null)
                            ?? existingPubs.FirstOrDefault(x => x.DeptCode == item.DeptCode);

                        if (deptEntity == null)
                        {
                            deptEntity = new DeptWisePublication
                            {
                                CollegeCode = collegeCode,
                                FacultyCode = facultyCodeInt,
                                DeptCode = item.DeptCode,
                                CreatedDate = DateTime.Now
                            };
                            _context.DeptWisePublications.Add(deptEntity);
                            existingPubs.Add(deptEntity);
                        }

                        deptEntity.DeptName = dept.DepartmentName;
                        deptEntity.PublicationsCount = item.PublicationsCount;
                        deptEntity.UpdatedDate = DateTime.Now;

                        if (HasFile(item.PublicationPdf))
                        {
                            var p = await SavePdfAsync(item.PublicationPdf!, facultyCode, "ResearchPublications", "DepartmentWisePublications");
                            newFiles.Add(p); QueueOld(oldFiles, deptEntity.PublicationPath);
                            deptEntity.PublicationPath = p;
                        }
                    }
                }

                // ── Department-wise research projects ────────────────────────
                if (model.DepartmentWiseResearchProjects?.Any() == true)
                {
                    var existingProjects = await _context.DepartmentWiseResearchProjects
                        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeInt)
                        .ToListAsync();

                    foreach (var item in model.DepartmentWiseResearchProjects)
                    {
                        var dept = departments.FirstOrDefault(d => d.DepartmentCode == item.DepartmentCode);
                        if (dept == null) continue;

                        var projEntity =
                            (item.Id > 0 ? existingProjects.FirstOrDefault(x => x.Id == item.Id) : null)
                            ?? existingProjects.FirstOrDefault(x =>
                                    x.DepartmentCode == item.DepartmentCode &&
                                    x.CourseLevel == courseLevel &&
                                    x.TypeId == typeId);

                        if (projEntity == null)
                        {
                            projEntity = new DepartmentWiseResearchProject
                            {
                                CollegeCode = collegeCode,
                                FacultyCode = facultyCodeInt,
                                DepartmentCode = item.DepartmentCode,
                                CourseLevel = courseLevel,
                                TypeId = typeId,
                                CreatedDate = DateTime.Now
                            };
                            _context.DepartmentWiseResearchProjects.Add(projEntity);
                            existingProjects.Add(projEntity);
                        }

                        projEntity.NoOfResearchProjectsLast3Years = item.NoOfResearchProjectsLast3Years;
                        projEntity.ModifiedDate = DateTime.Now;

                        if (HasFile(item.ResearchProjectsPdf))
                        {
                            var p = await SavePdfAsync(item.ResearchProjectsPdf!, facultyCode, "ResearchPublications", "DepartmentWiseResearchProjects");
                            newFiles.Add(p); QueueOld(oldFiles, projEntity.PdfFilePath);
                            projEntity.PdfFilePath = p;
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed saving research & publication details for college {College}", collegeCode);
                TryDeleteFiles(newFiles);   // don't leave orphan files
                TempData["Error"] = "Something went wrong while saving. Please try again.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            TryDeleteFiles(oldFiles);       // replaced files are removed only after a successful save

            TempData["Success"] = "Research and Publication details saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        // ═════════════════════════════════════════════════════════════════════
        // View PDF endpoints
        // ═════════════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> ViewDepartmentPublicationPdf(int id)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (!int.TryParse(facultyCode, out var facultyCodeInt))
                return NotFound("Session expired.");

            var row = await _context.DeptWisePublications
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeInt);

            return ServePdf(row?.PublicationPath, null);
        }

        [HttpGet]
        public async Task<IActionResult> ViewDepartmentResearchProjectPdf(int id)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (!int.TryParse(facultyCode, out var facultyCodeInt))
                return NotFound("Session expired.");

            var row = await _context.DepartmentWiseResearchProjects
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeInt);

            return ServePdf(row?.PdfFilePath, null);
        }

        [HttpGet] public async Task<IActionResult> ViewPublicationsPdf() => await GetPdf("Publications");
        [HttpGet] public async Task<IActionResult> ViewProjectsPdf() => await GetPdf("Projects");
        [HttpGet] public async Task<IActionResult> ViewStudentsProjectsPdf() => await GetPdf("StudentsProjects");
        [HttpGet] public async Task<IActionResult> ViewFacultyProjectsPdf() => await GetPdf("FacultyProjects");
        [HttpGet] public async Task<IActionResult> ViewClinicalTrialsPdf() => await GetPdf("ClinicalTrials");

        private async Task<IActionResult> GetPdf(string type)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound("Session expired.");

            var record = await GetMainRecordAsync(collegeCode, facultyCode);
            if (record == null) return NotFound("Record not found.");

            return type switch
            {
                "Publications" => ServePdf(record.PublicationsPdfPath, record.PublicationsPdfName),
                "Projects" => ServePdf(record.ProjectsPdfPath, record.ProjectsPdfName),
                "StudentsProjects" => ServePdf(record.StudentsProjectsPdfPath, record.StudentsProjectsPdfName),
                "FacultyProjects" => ServePdf(record.FacultyProjectsPdfPath, record.FacultyProjectsPdfName),
                "ClinicalTrials" => ServePdf(record.ClinicalTrialsPdfPath, record.ClinicalTrialsPdfName),
                _ => NotFound("Unknown document type.")
            };
        }

        // ═════════════════════════════════════════════════════════════════════
        // Other Academic Activities  (Create / Update / Delete)
        // ═════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherAcademicActivity(CA_Med_Lib_OtherActivityPostVM model, int editId = 0)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            // PDF is optional when editing (existing file is kept) — validated manually below
            ModelState.Remove("ActivityPdf");
            ModelState.Remove("editId");
            ModelState.Remove("EditId");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m)));
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            var pdfError = await ValidatePdfAsync(model.ActivityPdf);
            if (pdfError != null)
            {
                TempData["Error"] = pdfError;
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            bool hasNewPdf = HasFile(model.ActivityPdf);

            // Load record when editing
            CaMedLibOtherAcademicActivity? dbEntity = null;
            if (editId > 0)
            {
                dbEntity = await _context.CaMedLibOtherAcademicActivities
                    .FirstOrDefaultAsync(x => x.Id == editId && x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

                if (dbEntity == null)
                {
                    TempData["Error"] = "The record you are trying to edit was not found.";
                    return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
                }
            }
            else if (!hasNewPdf)
            {
                TempData["Error"] = "Please upload the activity PDF.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            // Duplicate check (ignores the record being edited)
            bool alreadyExists = await _context.CaMedLibOtherAcademicActivities.AnyAsync(x =>
                x.Id != editId &&
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.DepartmentCode == model.DepartmentCode &&
                x.ActivityId == model.ActivityId);

            if (alreadyExists)
            {
                TempData["Error"] = "The selected Department and Activity have already been saved. Edit or delete the existing record instead.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            var departmentName = await _context.DepartmentMasters
                .Where(x => x.DepartmentCode == model.DepartmentCode)
                .Select(x => x.DepartmentName)
                .FirstOrDefaultAsync();

            if (dbEntity == null)
            {
                dbEntity = new CaMedLibOtherAcademicActivity
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = "UG"   // Other Academic Activities are UG-specific
                };
                _context.CaMedLibOtherAcademicActivities.Add(dbEntity);
            }

            dbEntity.DepartmentCode = model.DepartmentCode;
            dbEntity.DepartmentWise = departmentName ?? "Unknown Department";
            dbEntity.ActivityId = model.ActivityId;

            string? newPath = null;
            string? oldPath = null;

            try
            {
                if (hasNewPdf)
                {
                    newPath = await SavePdfAsync(model.ActivityPdf!, facultyCode, "OtherAcademicActivities");
                    oldPath = dbEntity.ActivityPdfPath;
                    dbEntity.ActivityPdfPath = newPath;
                    dbEntity.ActivityPdfName = Path.GetFileName(model.ActivityPdf!.FileName);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed saving other academic activity");
                if (newPath != null) TryDeleteFiles(new[] { newPath });
                TempData["Error"] = "Something went wrong while saving the activity. Please try again.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            if (oldPath != null) TryDeleteFiles(new[] { oldPath });

            TempData["Success"] = editId > 0 ? "Activity updated successfully." : "Activity saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOtherAcademicActivity(int id)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var activity = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (activity == null)
            {
                TempData["Error"] = "Activity not found (it may already be deleted).";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            var path = activity.ActivityPdfPath;
            _context.CaMedLibOtherAcademicActivities.Remove(activity);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(path)) TryDeleteFiles(new[] { path });

            TempData["Success"] = "Activity deleted successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        [HttpGet]
        public async Task<IActionResult> ViewOtherActivityPdf(int id)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();

            var record = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            return ServePdf(record?.ActivityPdfPath, record?.ActivityPdfName);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Committees (Create / Update / Delete-file via "No")
        // ═════════════════════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommitteeDetails(CA_Med_Lib_CommitteePostVM model)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (model.Committees == null || !model.Committees.Any())
            {
                TempData["Error"] = "No committee data was submitted.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            var ids = model.Committees.Select(c => c.CommitteeId).ToList();

            var masters = await _context.CaMstMedCommitteeNames
                .Where(x => x.FacultyCode == facultyCode && ids.Contains(x.Id))
                .ToListAsync();
            var masterDict = masters.ToDictionary(m => m.Id);

            var savedCommittees = await _context.CaMedLibCommittees
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();
            var savedDict = savedCommittees
                .GroupBy(x => x.CommitteeId)
                .ToDictionary(g => g.Key, g => g.First());

            // Restore display info so the page renders correctly if we have to go back
            foreach (var item in model.Committees)
            {
                if (savedDict.TryGetValue(item.CommitteeId, out var ex))
                    item.CommitteePdfName = ex.CommitteePdfName;
                if (masterDict.TryGetValue(item.CommitteeId, out var m))
                {
                    item.CourseLevel = m.CourseLevel;
                    item.CommitteeName = m.CommitteeName;
                }
            }

            // ── Validation ───────────────────────────────────────────────────
            string? error = null;

            if (model.Committees.Any(c => c.IsPresent != "Y" && c.IsPresent != "N"))
                error = "Please select Yes or No for all committees.";

            if (error == null)
            {
                foreach (var item in model.Committees.Where(c => c.IsPresent == "Y"))
                {
                    savedDict.TryGetValue(item.CommitteeId, out var existing);
                    bool hasExistingPdf = existing != null && !string.IsNullOrEmpty(existing.CommitteePdfPath);

                    if (!hasExistingPdf && !HasFile(item.CommitteePdf))
                    {
                        error = $"Please upload the PDF document for \"{item.CommitteeName}\" (marked YES).";
                        break;
                    }

                    var pdfErr = await ValidatePdfAsync(item.CommitteePdf);
                    if (pdfErr != null) { error = $"{item.CommitteeName}: {pdfErr}"; break; }
                }
            }

            if (error != null)
            {
                TempData["Error"] = error;
                var vm = await LoadFullViewModel();
                vm.Committees = model.Committees;   // keep what the user selected
                return View("CA_Med_ResearchPublicationsDetails", vm);
            }

            // ── Save ─────────────────────────────────────────────────────────
            var newFiles = new List<string>();
            var oldFiles = new List<string>();

            try
            {
                foreach (var item in model.Committees)
                {
                    if (!masterDict.TryGetValue(item.CommitteeId, out var master))
                        continue;   // not a committee of this faculty

                    var courseLevel = master.CourseLevel ?? MainLevel;

                    // UPDATE existing row; INSERT only if none exists yet
                    if (!savedDict.TryGetValue(item.CommitteeId, out var db))
                    {
                        db = new CaMedLibCommittee
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CommitteeId = item.CommitteeId,
                            CourseLevel = courseLevel
                        };
                        _context.CaMedLibCommittees.Add(db);
                        savedDict[item.CommitteeId] = db;
                    }

                    db.IsPresent = item.IsPresent;
                    db.CourseLevel = courseLevel;

                    if (item.IsPresent == "Y")
                    {
                        if (HasFile(item.CommitteePdf))
                        {
                            var p = await SavePdfAsync(item.CommitteePdf!, facultyCode, "CommitteeDocs");
                            newFiles.Add(p); QueueOld(oldFiles, db.CommitteePdfPath);
                            db.CommitteePdfPath = p;
                            db.CommitteePdfName = Path.GetFileName(item.CommitteePdf!.FileName);
                        }
                    }
                    else
                    {
                        QueueOld(oldFiles, db.CommitteePdfPath);
                        db.CommitteePdfPath = null;
                        db.CommitteePdfName = null;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed saving committee details");
                TryDeleteFiles(newFiles);
                TempData["Error"] = "Something went wrong while saving committees. Please try again.";
                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            TryDeleteFiles(oldFiles);

            TempData["Success"] = "Committee details saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        [HttpGet]
        public async Task<IActionResult> ViewCommitteePdf(int committeeId)
        {
            var (collegeCode, facultyCode) = ReadSessionCodes();
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound("Session expired.");

            var record = await _context.CaMedLibCommittees
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CommitteeId == committeeId);

            return ServePdf(record?.CommitteePdfPath, record?.CommitteePdfName);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Helpers
        // ═════════════════════════════════════════════════════════════════════
        private (string? College, string? Faculty) ReadSessionCodes() =>
            (HttpContext.Session.GetString("CollegeCode"), HttpContext.Session.GetString("FacultyCode"));

        private int ReadAffiliationTypeId() =>
            int.TryParse(HttpContext.Session.GetString("TypeOfAffiliationId"), out var id) ? id : 0;

        /// <summary>
        /// The ONE main research row for a college/faculty.
        /// Prefers the "ALL" row, falls back to any older row (e.g. "UG") so it is
        /// updated instead of a duplicate being inserted.
        /// </summary>
        private async Task<CaMedResearchPublicationsDetail?> GetMainRecordAsync(string collegeCode, string facultyCode)
        {
            var rows = await _context.CaMedResearchPublicationsDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            return rows.FirstOrDefault(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == MainLevel)
                   ?? rows.FirstOrDefault();
        }

        private static bool HasFile(IFormFile? file) => file != null && file.Length > 0;

        private static void QueueOld(List<string> list, string? path)
        {
            if (!string.IsNullOrWhiteSpace(path)) list.Add(path);
        }

        private static void TryDeleteFiles(IEnumerable<string> paths)
        {
            foreach (var p in paths)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(p) && System.IO.File.Exists(p))
                        System.IO.File.Delete(p);
                }
                catch { /* a leftover file must never break the request */ }
            }
        }

        private string GetRootPath(string facultyCode) =>
            facultyCode == "2" ? BaseDentalPath : BaseMedicalPath;

        /// <summary>Saves a PDF under {root}/{subFolders...}/{guid}.ext and returns the full path.</summary>
        private async Task<string> SavePdfAsync(IFormFile file, string facultyCode, params string[] subFolders)
        {
            var folder = Path.Combine(new[] { GetRootPath(facultyCode) }.Concat(subFolders).ToArray());
            Directory.CreateDirectory(folder);

            var fullPath = Path.Combine(folder, Guid.NewGuid() + ".pdf");
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return fullPath;
        }

        /// <summary>Returns an error message, or null when the file is empty/absent or a valid PDF.</summary>
        private static async Task<string?> ValidatePdfAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                return "Only PDF files are allowed.";

            if (file.Length > MaxPdfBytes)
                return "File size must not exceed 5 MB.";

            var header = new byte[4];
            using (var s = file.OpenReadStream())
            {
                var read = await s.ReadAsync(header, 0, 4);
                if (read < 4 || header[0] != 0x25 || header[1] != 0x50 || header[2] != 0x44 || header[3] != 0x46) // %PDF
                    return "The uploaded file is not a valid PDF.";
            }
            return null;
        }

        private async Task AddPdfErrorAsync(IFormFile? file, string key)
        {
            var err = await ValidatePdfAsync(file);
            if (err != null) ModelState.AddModelError(key, err);
        }

        private IActionResult ServePdf(string? path, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(path))
                return NotFound("Record not found.");

            if (!System.IO.File.Exists(path))
                return NotFound("File not found on server. Please re-upload.");

            var fileName = string.IsNullOrWhiteSpace(displayName) ? Path.GetFileName(path) : displayName;

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
                contentType = "application/octet-stream";

            Response.Headers["Content-Disposition"] =
                new ContentDisposition { FileName = fileName, Inline = true }.ToString();

            return PhysicalFile(path, contentType);
        }

        private List<string> ParseLevels(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<string> { "UG" };

            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(raw);
                if (parsed != null && parsed.Any())
                {
                    var cleaned = parsed
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim().ToUpper())
                        .Distinct()
                        .ToList();
                    if (cleaned.Any()) return cleaned;
                }
            }
            catch { /* fall back to comma-separated below */ }

            var fallback = raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpper())
                .Distinct()
                .ToList();

            return fallback.Any() ? fallback : new List<string> { "UG" };
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