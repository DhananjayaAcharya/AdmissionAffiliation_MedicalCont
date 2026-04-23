using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // ← ADD THIS
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;
using System.IO;
using System.Text.Json; // For MemoryStream

namespace Medical_Affiliation.Controllers
{
    public class CA_Med_ResearchPublicationsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CA_Med_ResearchPublicationsController(ApplicationDbContext context)
        {
            _context = context;
        }
      
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

            // Load main research data
            //var mainData = await _context.CaMedResearchPublicationsDetails
            //    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            var mainDataList = await _context.CaMedResearchPublicationsDetails
                                            .Where(x =>
                                                x.CollegeCode == collegeCode &&
                                                x.FacultyCode == facultyCode
                                                )
                                            .ToListAsync();


            var commonData = mainDataList
                    .FirstOrDefault(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == "ALL");

            if (commonData == null)
            {
                commonData = new CaMedResearchPublicationsDetail(); // safe empty object
            }


            // Load Other Activities with join for ActivityName
            // ================= OTHER ACTIVITIES =================
            var masterList = await _context.CaMstMedOtherAcademicActivities.ToListAsync();

            var savedList = await _context.CaMedLibOtherAcademicActivities
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            var otherActivities = masterList
                .Select(master =>
                {
                    var saved = savedList.FirstOrDefault(s => s.ActivityId == master.Id);

                    return new CA_Med_Lib_OtherAcademicActivitiesVM
                    {
                        Id = saved?.Id ?? 0,
                        ActivityId = master.Id,
                        ActivityName = master.ActivityName,
                        DepartmentCode = saved?.DepartmentCode ?? "",
                        DepartmentWise = saved?.DepartmentWise ?? "",
                        ActivityPdfName = saved?.ActivityPdfName
                    };
                })
                .ToList();

            // Load Committees master
            var committeeMasters = await _context.CaMstMedCommitteeNames
                    .Where(x =>
                        x.FacultyCode == facultyCode &&
                        (
                            (x.CourseLevel != null && x.CourseLevel.ToUpper() == "ALL") ||
                            (x.CourseLevel != null && x.CourseLevel.ToUpper() == "UG" && levels.Contains("UG"))
                        ))
                    .OrderBy(x => x.CommitteeName)
                    .ToListAsync();



            var savedCommittees = await _context.CaMedLibCommittees
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            var savedCommitteesDict = savedCommittees.GroupBy(x => x.CommitteeId).ToDictionary(g => g.Key, g => g.First());

            // Load dropdown data
            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCodeInt)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            var activityMasters = await _context.CaMstMedOtherAcademicActivities
                .OrderBy(x => x.ActivityName)
                .ToListAsync();



            // 🔹 LEVEL DATA
            //var ugData = mainDataList
            //    .FirstOrDefault(x => x.CourseLevel != null && x.CourseLevel.ToUpper() == "ALL")
            //    ?? mainDataList.FirstOrDefault();

            // Build View Model
            var vm = new CA_Med_ResearchPublicationsDetailsVM
            {
                // ✅ COMMON → from UG
                PublicationsNo = commonData?.PublicationsNo ?? 0,
                PublicationsPdfName = commonData?.PublicationsPdfName,

                // ✅ UG ONLY
                ClinicalTrialsPdfName = commonData?.ClinicalTrialsPdfName,

                // ✅ Research Projects → from UG
                StudentsRGUHSFunded = commonData?.StudentsRguhsfunded,
                StudentsExternalBodyFunding = commonData?.StudentsExternalBodyFunding,
                StudentsProjectsPdfName = commonData?.StudentsProjectsPdfName,

                FacultyRGUHSFunded = commonData?.FacultyRguhsfunded,
                FacultyExternalBodyFunding = commonData?.FacultyExternalBodyFunding,
                FacultyProjectsPdfName = commonData?.FacultyProjectsPdfName,

                // ✅ Other
                OtherActivities = otherActivities,
                Departments = departments,
                ActivityMasters = activityMasters,

                // ✅ Committees
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
                })
                .ToList()
            };

            return View(vm);
        }

        //private async Task<CA_Med_ResearchPublicationsDetailsVM> LoadFullViewModel()
        //{
        //    // Reuse the same logic as GET
        //    var actionResult = await CA_Med_ResearchPublicationsDetails();
        //    return actionResult is ViewResult viewResult ? viewResult.Model as CA_Med_ResearchPublicationsDetailsVM : new CA_Med_ResearchPublicationsDetailsVM();
        //}

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

            //var level = "UG";
            var level = "ALL";

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            //var entity = await _context.CaMedResearchPublicationsDetails
            //    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            var entity = await _context.CaMedResearchPublicationsDetails
                .FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.CourseLevel == level);

            if (PublicationsPdf == null && string.IsNullOrEmpty(entity?.PublicationsPdfPath))
            {
                ModelState.AddModelError("PublicationsPdf", "Publications PDF is required.");
            }

            if (StudentsProjectsPdf == null && string.IsNullOrEmpty(entity?.StudentsProjectsPdfPath))
            {
                ModelState.AddModelError("StudentsProjectsPdf", "Students Projects PDF is required.");
            }

            if (FacultyProjectsPdf == null && string.IsNullOrEmpty(entity?.FacultyProjectsPdfPath))
            {
                ModelState.AddModelError("FacultyProjectsPdf", "Faculty Projects PDF is required.");
            }

            // ✅ UG only (always required in your case)
            if (ClinicalTrialsPdf == null && string.IsNullOrEmpty(entity?.ClinicalTrialsPdfPath))
            {
                ModelState.AddModelError("ClinicalTrialsPdf", "Clinical Trials PDF is required.");
            }
            // ✅ STOP if validation fails
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

                return View(model);
            }

            // ✅ Create new record if needed
            if (entity == null)
            {

                entity = new CaMedResearchPublicationsDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    //CourseLevel = level   // ✅ VERY IMPORTANT
                    CourseLevel = "ALL"
                };
                _context.CaMedResearchPublicationsDetails.Add(entity);
            }

            entity.CourseLevel = level;

            // ✅ Save non-file fields
            entity.PublicationsNo = model.PublicationsNo;

            entity.StudentsRguhsfunded = model.StudentsRGUHSFunded;
            entity.StudentsExternalBodyFunding = model.StudentsExternalBodyFunding;

            entity.FacultyRguhsfunded = model.FacultyRGUHSFunded;
            entity.FacultyExternalBodyFunding = model.FacultyExternalBodyFunding;

            // ✅ Upload Publications PDF
            if (PublicationsPdf != null && PublicationsPdf.Length > 0)
            {
                var path = await SaveResearchFileAsync(PublicationsPdf, "Publications");

                if (path != null)
                {
                    if (!string.IsNullOrEmpty(entity.PublicationsPdfPath) &&
                        System.IO.File.Exists(entity.PublicationsPdfPath))
                    {
                        System.IO.File.Delete(entity.PublicationsPdfPath);
                    }

                    entity.PublicationsPdfPath = path;
                    entity.PublicationsPdfName = PublicationsPdf.FileName;
                }
            }

            // ✅ Upload Students Projects PDF
            if (StudentsProjectsPdf != null && StudentsProjectsPdf.Length > 0)
            {
                var path = await SaveResearchFileAsync(StudentsProjectsPdf, "StudentProjects");

                if (path != null)
                {
                    if (!string.IsNullOrEmpty(entity.StudentsProjectsPdfPath) &&
                        System.IO.File.Exists(entity.StudentsProjectsPdfPath))
                    {
                        System.IO.File.Delete(entity.StudentsProjectsPdfPath);
                    }

                    entity.StudentsProjectsPdfPath = path;
                    entity.StudentsProjectsPdfName = StudentsProjectsPdf.FileName;
                }
            }

            // ✅ Upload Faculty Projects PDF
            if (FacultyProjectsPdf != null && FacultyProjectsPdf.Length > 0)
            {
                var path = await SaveResearchFileAsync(FacultyProjectsPdf, "FacultyProjects");

                if (path != null)
                {
                    if (!string.IsNullOrEmpty(entity.FacultyProjectsPdfPath) &&
                        System.IO.File.Exists(entity.FacultyProjectsPdfPath))
                    {
                        System.IO.File.Delete(entity.FacultyProjectsPdfPath);
                    }

                    entity.FacultyProjectsPdfPath = path;
                    entity.FacultyProjectsPdfName = FacultyProjectsPdf.FileName;
                }
            }

            // ✅ Upload Clinical Trials PDF
            if (ClinicalTrialsPdf != null && ClinicalTrialsPdf.Length > 0)
            {
                var path = await SaveResearchFileAsync(ClinicalTrialsPdf, "ClinicalTrials");

                if (path != null)
                {
                    if (!string.IsNullOrEmpty(entity.ClinicalTrialsPdfPath) &&
                        System.IO.File.Exists(entity.ClinicalTrialsPdfPath))
                    {
                        System.IO.File.Delete(entity.ClinicalTrialsPdfPath);
                    }

                    entity.ClinicalTrialsPdfPath = path;
                    entity.ClinicalTrialsPdfName = ClinicalTrialsPdf.FileName;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Research and Publication details saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }




        private async Task<string?> SaveResearchFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = Path.Combine(BasePath, "ResearchPublications");
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

        // View PDF methods remain the same — just change entity name to singular
        [HttpGet]
        public async Task<IActionResult> ViewPublicationsPdf() => await GetPdf("Publications");

        [HttpGet]
        public async Task<IActionResult> ViewStudentsProjectsPdf() => await GetPdf("StudentsProjects");

        [HttpGet]
        public async Task<IActionResult> ViewFacultyProjectsPdf() => await GetPdf("FacultyProjects");


        [HttpGet]
        public async Task<IActionResult> ViewClinicalTrialsPdf() => await GetPdf("ClinicalTrials");

        private async Task<IActionResult> GetPdf(string type, string mode = "view")
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            //var level = HttpContext.Session.GetString("CourseLevel");
            var level = "UG";

            var record = await _context.CaMedResearchPublicationsDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == level);

            if (record == null) return NotFound();

            // 🔥 Get file path instead of byte[]
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

            // 🔴 Validation
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var fileName = string.IsNullOrEmpty(name)
                ? Path.GetFileName(filePath)
                : name;

            // 🔥 Detect content type dynamically
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // 📥 DOWNLOAD MODE
            if (mode == "download")
            {
                return PhysicalFile(filePath, contentType, fileName);
            }

            // 👀 PREVIEW MODE
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return PhysicalFile(filePath, contentType);
        }

        //

        // DELETE - with SweetAlert confirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOtherAcademicActivity(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var activity = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (activity != null)
            {

                if (!string.IsNullOrEmpty(activity.ActivityPdfPath) && System.IO.File.Exists(activity.ActivityPdfPath))
                {
                    System.IO.File.Delete(activity.ActivityPdfPath);
                }

                _context.CaMedLibOtherAcademicActivities.Remove(activity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Activity deleted successfully";
            }

            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }

        // EDIT - loads data into form
        [HttpGet]
        public async Task<IActionResult> EditOtherActivity(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var activity = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (activity == null) return NotFound();

            var vm = await LoadFullViewModel();

            vm.OtherActivityToEdit = new CA_Med_Lib_OtherActivityPostVM
            {
                Id = activity.Id,
                ActivityId = activity.ActivityId,
                DepartmentCode = activity.DepartmentCode
            };

            return View("CA_Med_ResearchPublicationsDetails", vm);
        }

        // Helper to reload full model on edit/error
        //private async Task<CA_Med_ResearchPublicationsDetailsVM> LoadFullViewModel()
        //{
        //    var result = await CA_Med_ResearchPublicationsDetails();
        //    if (result is ViewResult viewResult && viewResult.Model is CA_Med_ResearchPublicationsDetailsVM vm)
        //    {
        //        return vm;
        //    }
        //    return new CA_Med_ResearchPublicationsDetailsVM();
        //}

        //below code added by ram for SS on 09-02-2026
        private async Task<CA_Med_ResearchPublicationsDetailsVM> LoadFullViewModel()
        {
            // Pass null → method will read from Session
            var result = await CA_Med_ResearchPublicationsDetails();

            if (result is ViewResult viewResult &&
                viewResult.Model is CA_Med_ResearchPublicationsDetailsVM vm)
            {
                return vm;
            }

            return new CA_Med_ResearchPublicationsDetailsVM();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherAcademicActivity(CA_Med_Lib_OtherActivityPostVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var fullVm = await LoadFullViewModel();
                return View("CA_Med_ResearchPublicationsDetails", fullVm);
            }

            // 🔴 DUPLICATE CHECK
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
                    "If you want to upload a new file, please delete the existing record and add again.";

                return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
            }

            // ✅ Get department name
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
                CourseLevel = "UG"
            };

            // 🔥 FILE PATH STORAGE
            if (model.ActivityPdf != null && model.ActivityPdf.Length > 0)
            {
                string basePath = Path.Combine(BasePath, "OtherAcademicActivities");

                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ActivityPdf.FileName);
                string fullPath = Path.Combine(basePath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ActivityPdf.CopyToAsync(stream);
                }

                dbEntity.ActivityPdfPath = fullPath;                 // ✅ PATH
                dbEntity.ActivityPdfName = model.ActivityPdf.FileName; // ✅ NAME
            }

            _context.CaMedLibOtherAcademicActivities.Add(dbEntity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Activity saved successfully";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCommitteeDetails(CA_Med_Lib_CommitteePostVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            //var level = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (model.Committees.Any(c => string.IsNullOrEmpty(c.IsPresent)))
            {
                TempData["Error"] = "Please select Yes or No for all committees.";
                var vm = await LoadFullViewModel();
                vm.Committees = model.Committees;
                return View("CA_Med_ResearchPublicationsDetails", vm);
            }

            // 🔴 RULE 1
            //bool hasUnselectedRows = model.Committees.Any(c => string.IsNullOrEmpty(c.IsPresent));
            //if (hasUnselectedRows)
            //{
            //    TempData["Error"] = "Please select Yes or No for ALL committees before saving.";

            //    var vm = await LoadFullViewModel();
            //    vm.Committees = model.Committees;
            //    return View("CA_Med_ResearchPublicationsDetails", vm);
            //}

            // 🔴 RULE 2
            foreach (var item in model.Committees)
            {
                var master = await _context.CaMstMedCommitteeNames
                    .FirstOrDefaultAsync(x => x.Id == item.CommitteeId);

                var courseLevel = master?.CourseLevel ?? "ALL"; // ✅ IMPORTANT

                var db = await _context.CaMedLibCommittees
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CommitteeId == item.CommitteeId);

                if (db == null)
                {
                    db = new CaMedLibCommittee
                    {
                        CollegeCode = collegeCode!,
                        FacultyCode = facultyCode!,
                        CommitteeId = item.CommitteeId,
                        CourseLevel = courseLevel   // ✅ FROM MASTER
                    };
                    _context.CaMedLibCommittees.Add(db);
                }

                db.IsPresent = item.IsPresent;
                db.CourseLevel = courseLevel;

                if (item.IsPresent == "Y")
                {
                    if (item.CommitteePdf != null && item.CommitteePdf.Length > 0)
                    {
                        string basePath = Path.Combine(BasePath, "CommitteeDocs");

                        if (!Directory.Exists(basePath))
                            Directory.CreateDirectory(basePath);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.CommitteePdf.FileName);
                        string fullPath = Path.Combine(basePath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await item.CommitteePdf.CopyToAsync(stream);
                        }

                        if (!string.IsNullOrEmpty(db.CommitteePdfPath) &&
                            System.IO.File.Exists(db.CommitteePdfPath))
                        {
                            System.IO.File.Delete(db.CommitteePdfPath);
                        }

                        db.CommitteePdfPath = fullPath;
                        db.CommitteePdfName = item.CommitteePdf.FileName;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(db.CommitteePdfPath) &&
                        System.IO.File.Exists(db.CommitteePdfPath))
                    {
                        System.IO.File.Delete(db.CommitteePdfPath);
                    }

                    db.CommitteePdfPath = null;
                    db.CommitteePdfName = null;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Committee details saved successfully";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }


        // View PDF methods (keep your existing ones + add for Other Activity)
        public async Task<IActionResult> ViewOtherActivityPdf(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var record = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x => x.Id == id &&
                                         x.CollegeCode == collegeCode &&
                                         x.FacultyCode == facultyCode);

            if (record == null ||
                string.IsNullOrEmpty(record.ActivityPdfPath))
                return NotFound("Record not found");

            // 🔥 CHECK FILE EXISTS (IMPORTANT)
            if (!System.IO.File.Exists(record.ActivityPdfPath))
            {
                return NotFound("File not found on server. Please re-upload.");
            }

            var fileName = string.IsNullOrEmpty(record.ActivityPdfName)
                ? Path.GetFileName(record.ActivityPdfPath)
                : record.ActivityPdfName;

            // 🔥 Detect content type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.ActivityPdfPath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return PhysicalFile(record.ActivityPdfPath, contentType);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCommitteePdf(int committeeId)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound("Session expired");

            var record = await _context.CaMedLibCommittees
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CommitteeId == committeeId);

            if (record == null || string.IsNullOrEmpty(record.CommitteePdfPath))
                return NotFound("Record not found");

            // 🔥 IMPORTANT FIX
            if (!System.IO.File.Exists(record.CommitteePdfPath))
                return NotFound("File not found on server. Please re-upload.");

            var fileName = string.IsNullOrEmpty(record.CommitteePdfName)
                ? Path.GetFileName(record.CommitteePdfPath)
                : record.CommitteePdfName;

            // 🔥 Dynamic content type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(record.CommitteePdfPath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // 👀 Inline preview
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return PhysicalFile(record.CommitteePdfPath, contentType);
        }

        // Helper to reload full model (used in Edit and validation errors)

    }
}


