using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // ← ADD THIS
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;
using System.IO; // For MemoryStream

namespace Medical_Affiliation.Controllers
{
    public class CA_Med_ResearchPublicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CA_Med_ResearchPublicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CA_Med_ResearchPublicationsDetails(string level)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");



            //Enter this code after  var facultyCode
            // ✅ If not coming from URL, get from Session
            //if (string.IsNullOrEmpty(level))
            //{
            //    level = HttpContext.Session.GetString("CourseLevel");
            //}

            //// ✅ If still empty, go back to first page
            //if (string.IsNullOrEmpty(level))
            //    return RedirectToAction("Institution_Details", "ContinuesAffiliation_Facultybased");

            //// ✅ Save again in session (safety)
            //HttpContext.Session.SetString("CourseLevel", level);

            // 1. If URL has level → use it (TOP PRIORITY)
            if (!string.IsNullOrEmpty(level))
            {
                HttpContext.Session.SetString("CourseLevel", level);
            }
            else
            {
                // 2. Else use session
                level = HttpContext.Session.GetString("CourseLevel");
            }

            // 3. If still empty → redirect
            if (string.IsNullOrEmpty(level))
            {
                return RedirectToAction("Institution_Details", "ContinuesAffiliation_Facultybased");
            }

            // 4. Send to View
            ViewBag.CourseLevel = level;


            // ✅ Send to View
            //ViewBag.CourseLevel = level;

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            int facultyCodeInt = Convert.ToInt32(facultyCode);

            // Load main research data
            //var mainData = await _context.CaMedResearchPublicationsDetails
            //    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            var mainData = await _context.CaMedResearchPublicationsDetails
            .FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.CourseLevel == level);



            // Load Other Activities with join for ActivityName
            var otherActivities = await _context.CaMedLibOtherAcademicActivities
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .Join(_context.CaMstMedOtherAcademicActivities,
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

            // Load Committees master + saved values
            //var committeeMasters = await _context.CaMstMedCommitteeNames
            //    .Where(x => x.FacultyCode == facultyCode)
            //    .OrderBy(x => x.CommitteeName)
            //    .ToListAsync();

            // Load Committees master
            IQueryable<CaMstMedCommitteeName> committeeMastersQuery =
            _context.CaMstMedCommitteeNames
            .Where(x => x.FacultyCode == facultyCode);

            // Apply order first
            committeeMastersQuery = committeeMastersQuery.OrderBy(x => x.CommitteeName);

            // ✅ If SS → take only first 5
            if (level == "SS")
            {
                committeeMastersQuery = committeeMastersQuery.Take(5);
            }

            var committeeMasters = await committeeMastersQuery.ToListAsync();


            var savedCommittees = await _context.CaMedLibCommittees
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            // Load dropdown data
            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCodeInt)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            var activityMasters = await _context.CaMstMedOtherAcademicActivities
                .OrderBy(x => x.ActivityName)
                .ToListAsync();

            // Build View Model
            var vm = new CA_Med_ResearchPublicationsDetailsVM
            {
                PublicationsNo = mainData?.PublicationsNo ?? 0,
                PublicationsPdfName = mainData?.PublicationsPdfName,

                // ✅ Students
                StudentsRGUHSFunded = mainData?.StudentsRguhsfunded,
                StudentsExternalBodyFunding = mainData?.StudentsExternalBodyFunding,
                StudentsProjectsPdfName = mainData?.StudentsProjectsPdfName,

                // ✅ Faculty
                FacultyRGUHSFunded = mainData?.FacultyRguhsfunded,
                FacultyExternalBodyFunding = mainData?.FacultyExternalBodyFunding,
                FacultyProjectsPdfName = mainData?.FacultyProjectsPdfName,

                ClinicalTrialsPdfName = mainData?.ClinicalTrialsPdfName,

                OtherActivities = otherActivities,
                Departments = departments,
                ActivityMasters = activityMasters,

                Committees = committeeMasters.Select(m => new CA_Med_Lib_CommitteeVM
                {
                    CommitteeId = m.Id,
                    CommitteeName = m.CommitteeName,
                    IsPresent = savedCommittees.FirstOrDefault(s => s.CommitteeId == m.Id)?.IsPresent ?? "",
                    CommitteePdfName = savedCommittees.FirstOrDefault(s => s.CommitteeId == m.Id)?.CommitteePdfName
                }).ToList()
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

            var level = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            //var entity = await _context.CaMedResearchPublicationsDetails
            //    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            var entity = await _context.CaMedResearchPublicationsDetails
            .FirstOrDefaultAsync(x =>
            x.CollegeCode == collegeCode &&
            x.FacultyCode == facultyCode &&
            x.CourseLevel == level);

            bool isNew = entity == null;

            // ✅ FIRST SAVE → PDFs mandatory
            //if (isNew)
            //{
            //    if (PublicationsPdf == null || PublicationsPdf.Length == 0)
            //        ModelState.AddModelError("PublicationsPdf", "Publications PDF is required.");

            //    if (StudentsProjectsPdf == null || StudentsProjectsPdf.Length == 0)
            //        ModelState.AddModelError("StudentsProjectsPdf", "Students Projects PDF is required.");

            //    if (FacultyProjectsPdf == null || FacultyProjectsPdf.Length == 0)
            //        ModelState.AddModelError("FacultyProjectsPdf", "Faculty Projects PDF is required.");

            //    if (ClinicalTrialsPdf == null || ClinicalTrialsPdf.Length == 0)
            //        ModelState.AddModelError("ClinicalTrialsPdf", "Clinical Trials PDF is required.");
            //}

            if (isNew)
            {
                if (PublicationsPdf == null || PublicationsPdf.Length == 0)
                    ModelState.AddModelError("PublicationsPdf", "Publications PDF is required.");

                if (StudentsProjectsPdf == null || StudentsProjectsPdf.Length == 0)
                    ModelState.AddModelError("StudentsProjectsPdf", "Students Projects PDF is required.");

                if (FacultyProjectsPdf == null || FacultyProjectsPdf.Length == 0)
                    ModelState.AddModelError("FacultyProjectsPdf", "Faculty Projects PDF is required.");

                // ✅ ONLY for UG (or non-SS)
                if (level != "SS")
                {
                    if (ClinicalTrialsPdf == null || ClinicalTrialsPdf.Length == 0)
                        ModelState.AddModelError("ClinicalTrialsPdf", "Clinical Trials PDF is required.");
                }
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
                //entity = new CaMedResearchPublicationsDetail
                //{
                //    CollegeCode = collegeCode,
                //    FacultyCode = facultyCode
                //};

                entity = new CaMedResearchPublicationsDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = level   // ✅ VERY IMPORTANT
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
                using var ms = new MemoryStream();
                await PublicationsPdf.CopyToAsync(ms);
                entity.PublicationsPdf = ms.ToArray();
                entity.PublicationsPdfName = PublicationsPdf.FileName;
            }

            // ✅ Upload Students Projects PDF
            if (StudentsProjectsPdf != null && StudentsProjectsPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await StudentsProjectsPdf.CopyToAsync(ms);
                entity.StudentsProjectsPdf = ms.ToArray();
                entity.StudentsProjectsPdfName = StudentsProjectsPdf.FileName;
            }

            // ✅ Upload Faculty Projects PDF
            if (FacultyProjectsPdf != null && FacultyProjectsPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await FacultyProjectsPdf.CopyToAsync(ms);
                entity.FacultyProjectsPdf = ms.ToArray();
                entity.FacultyProjectsPdfName = FacultyProjectsPdf.FileName;
            }

            // ✅ Upload Clinical Trials PDF
            if (ClinicalTrialsPdf != null && ClinicalTrialsPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await ClinicalTrialsPdf.CopyToAsync(ms);
                entity.ClinicalTrialsPdf = ms.ToArray();
                entity.ClinicalTrialsPdfName = ClinicalTrialsPdf.FileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Research and Publication details saved successfully.";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
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

        private async Task<IActionResult> GetPdf(string type)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            //var record = await _context.CaMedResearchPublicationsDetails
            //    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            var level = HttpContext.Session.GetString("CourseLevel");

            var record = await _context.CaMedResearchPublicationsDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == level);



            if (record == null) return NotFound();

            byte[]? file = type switch
            {
                "Publications" => record.PublicationsPdf,
                "StudentsProjects" => record.StudentsProjectsPdf,
                "FacultyProjects" => record.FacultyProjectsPdf,
                "ClinicalTrials" => record.ClinicalTrialsPdf,
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

            if (file == null || file.Length == 0 || string.IsNullOrEmpty(name))
                return NotFound();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{name}\"";
            return File(file, "application/pdf");
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
            var result = await CA_Med_ResearchPublicationsDetails(null);

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

            // 🔴 DUPLICATE CHECK (Department + Activity)
            bool alreadyExists = await _context.CaMedLibOtherAcademicActivities.AnyAsync(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == facultyCode &&
                x.DepartmentCode == model.DepartmentCode &&
                x.ActivityId == model.ActivityId
            );

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
                ActivityId = model.ActivityId
            };

            // File upload
            if (model.ActivityPdf != null && model.ActivityPdf.Length > 0)
            {
                using var ms = new MemoryStream();
                await model.ActivityPdf.CopyToAsync(ms);
                dbEntity.ActivityPdf = ms.ToArray();
                dbEntity.ActivityPdfName = model.ActivityPdf.FileName;
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
            var level = HttpContext.Session.GetString("CourseLevel");


            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            // 🔴 RULE 1: All rows must be Yes or No
            bool hasUnselectedRows = model.Committees.Any(c => string.IsNullOrEmpty(c.IsPresent));
            if (hasUnselectedRows)
            {
                TempData["Error"] =
                    "Please select Yes or No for ALL committees before saving.";

                var vm = await LoadFullViewModel();
                vm.Committees = model.Committees;
                return View("CA_Med_ResearchPublicationsDetails", vm);
            }

            // 🔴 RULE 2: If YES → document MUST exist
            foreach (var item in model.Committees)
            {
                if (item.IsPresent == "Y")
                {
                    var existingRecord = await _context.CaMedLibCommittees
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CommitteeId == item.CommitteeId);

                    bool hasExistingFile =
                        existingRecord != null &&
                        existingRecord.CommitteePdf != null &&
                        !string.IsNullOrEmpty(existingRecord.CommitteePdfName);

                    bool hasNewFile =
                        item.CommitteePdf != null &&
                        item.CommitteePdf.Length > 0;

                    if (!hasExistingFile && !hasNewFile)
                    {
                        TempData["Error"] =
                            "You have selected YES for a committee but did not upload the document. " +
                            "Please upload the document. If you do not have the document, select NO for that committee.";

                        var vm = await LoadFullViewModel();
                        vm.Committees = model.Committees;
                        return View("CA_Med_ResearchPublicationsDetails", vm);
                    }
                }
            }

            // 🔹 SAVE LOGIC (UNCHANGED)
            foreach (var item in model.Committees)
            {
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
                        CourseLevel = level   // ✅ ADD THIS
                    };
                    _context.CaMedLibCommittees.Add(db);
                }


                db.IsPresent = item.IsPresent;
                db.CourseLevel = level;   // ✅ Keep updated always


                if (item.IsPresent == "Y")
                {
                    if (item.CommitteePdf != null && item.CommitteePdf.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await item.CommitteePdf.CopyToAsync(ms);
                        db.CommitteePdf = ms.ToArray();
                        db.CommitteePdfName = item.CommitteePdf.FileName;
                    }
                    // else → keep existing file
                }
                else if (item.IsPresent == "N")
                {
                    db.CommitteePdf = null;
                    db.CommitteePdfName = null;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Committee details saved successfully";
            return RedirectToAction(nameof(CA_Med_ResearchPublicationsDetails));
        }




        // View PDF methods (keep your existing ones + add for Other Activity)
        [HttpGet]
        public async Task<IActionResult> ViewOtherActivityPdf(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var record = await _context.CaMedLibOtherAcademicActivities
                .FirstOrDefaultAsync(x => x.Id == id && x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (record == null || record.ActivityPdf == null) return NotFound();

            Response.Headers.Add("Content-Disposition", $"inline; filename={record.ActivityPdfName}");
            return File(record.ActivityPdf, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ViewCommitteePdf(int committeeId)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return NotFound();

            var record = await _context.CaMedLibCommittees
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CommitteeId == committeeId);

            if (record == null || record.CommitteePdf == null)
                return NotFound();

            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{record.CommitteePdfName}\"";

            return File(record.CommitteePdf, "application/pdf");
        }


        // Helper to reload full model (used in Edit and validation errors)

    }
}


