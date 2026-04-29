using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class Aff_CA_SS_CoursesAppliedSSController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Aff_CA_SS_CoursesAppliedSSController(ApplicationDbContext context)
        {
            _context = context;
        }

        private const long MaxFileSize = 2 * 1024 * 1024;

        private async Task<string?> SaveSSFileAsync(
        IFormFile? file,
        string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath =
                @"D:\Affiliation_Medical\SSCoursesApplied";

            string fullFolder =
                Path.Combine(basePath, folder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string savedName =
                Guid.NewGuid().ToString() +
                Path.GetExtension(file.FileName);

            string fullPath =
                Path.Combine(fullFolder, savedName);

            using (var stream =
               new FileStream(
                   fullPath,
                   FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;   // returns D:\....pdf
        }

        private bool FileTooLarge(IFormFile file)
        {
            return file != null && file.Length > MaxFileSize;
        }

        // ===========================
        // GET : Applied Courses (DM + MCH)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> CA_SS_CoursesApplied()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            //var courseLevel = HttpContext.Session.GetString("CourseLevel");

            //if (string.IsNullOrEmpty(courseLevel))
            //{
            //    TempData["Error"] = "Session expired. Please select the course level from the menu again.";
            //    return RedirectToAction("Index", "Home");
            //}


            // OLD
            // var courseLevel = HttpContext.Session.GetString("CourseLevel");

            // NEW
            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            if (string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Login", "Account");

            List<string> existingCourseLevels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                existingCourseLevels = new List<string> { "SS" };
            }
            else
            {
                try
                {
                    existingCourseLevels =
                        System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(raw)
                        ?? new List<string> { "SS" };
                }
                catch
                {
                    existingCourseLevels =
                        raw.Split(',')
                           .Select(x => x.Trim().ToUpper())
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();
                }
            }

            if (!existingCourseLevels.Any())
            {
                TempData["Error"] =
                    "Session expired. Please select the course level from the menu again.";

                return RedirectToAction("Index", "Home");
            }

            var vm = new CA_SS_FullViewVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                CautionMessage =
                "Existing Cardio Vascular & Thoracic Surgery (CTVS) course is being bifurcated into " +
                "(1) Cardiac Surgery and (2) Thoracic Surgery. There will be a transition period of three years. " +
                "After 2026, there will be no admission."
            };

            // =====================================================
            // Get College Courses
            // =====================================================
            var courseList = await _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode)
                .ToListAsync();

            // ✅ NEW
            //var courseList = await (
            //    from c in _context.CollegeCourseIntakeDetails
            //    join m in _context.MstCourses
            //        on new { CourseCode = c.CourseCode, FacultyCode = c.FacultyCode }
            //        equals new { CourseCode = m.CourseCode.ToString(), FacultyCode = m.FacultyCode }
            //    where c.CollegeCode == collegeCode
            //        && m.FacultyCode == 1
            //        && m.CourseLevel == courseLevel
            //    select new
            //    {
            //        CourseCode = c.CourseCode,
            //        CourseName = m.CourseName,
            //        PresentIntake = c.PresentIntake,
            //        FacultyCode = c.FacultyCode
            //    }
            //).ToListAsync();

            // =====================================================
            // DM COURSES
            // =====================================================
            vm.DMCourses = await
            (
                from m in _context.MstCourses
                join c in _context.CollegeCourseIntakeDetails
                    on new { CourseCode = m.CourseCode.ToString(), m.FacultyCode }
                    equals new { CourseCode = c.CourseCode, c.FacultyCode }
                    into gj
                from c in gj.Where(x => x.CollegeCode == collegeCode).DefaultIfEmpty()
                where m.FacultyCode == 1
                      //&& m.CourseLevel == courseLevel
                      && existingCourseLevels.Contains(m.CourseLevel)
                      && m.CoursePrefix == "D.M."
                select new SSCourseRow
                {
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    PresentIntake = c != null ? c.PresentIntake : 0
                }
            ).ToListAsync();

            // =====================================================
            // MCH COURSES
            // =====================================================
            vm.MChCourses = await
            (
                from m in _context.MstCourses
                join c in _context.CollegeCourseIntakeDetails
                    on new { CourseCode = m.CourseCode.ToString(), m.FacultyCode }
                    equals new { CourseCode = c.CourseCode, c.FacultyCode }
                    into gj
                from c in gj.Where(x => x.CollegeCode == collegeCode).DefaultIfEmpty()
                where m.FacultyCode == 1
                      && existingCourseLevels.Contains(m.CourseLevel)
                      && m.CoursePrefix.Replace(".", "").Replace(" ", "").ToUpper() == "MCH"
                select new SSCourseRow
                {
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    PresentIntake = c != null ? c.PresentIntake : 0
                }
            ).ToListAsync();

            // =====================================================
            // 3.a LOP
            // =====================================================
            var savedLop = await _context.CaSsLopsavedDates
               .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            vm.LopList.Clear();

            foreach (var c in courseList)
            {
                int code = int.Parse(c.CourseCode);

                var existing = savedLop.FirstOrDefault(x => x.CourseCode == code);

                vm.LopList.Add(new CA_SS_LOPSavedDateVM
                {
                    CourseCode = code,
                    CourseName = c.CourseName,
                    SanctionedIntake = c.PresentIntake ?? 0,
                    LopDate = existing?.LopDate?.ToDateTime(TimeOnly.MinValue),
                    RecognitionDate = existing?.RecognitionDate?.ToDateTime(TimeOnly.MinValue)
                });
            }

            // =====================================================
            // 3.b.i Particulars
            // =====================================================
            var savedPerm = await _context.CaSsPermissions
               .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            vm.PermissionList.Clear();

            foreach (var c in courseList)
            {
                var existing = savedPerm.FirstOrDefault(x => x.CourseCode == int.Parse(c.CourseCode));

                //vm.PermissionList.Add(new CA_SS_PermissionVM
                //{
                //    CourseCode = int.TryParse(c.CourseCode, out var code) ? code : 0,
                //    CourseName = c.CourseName,
                //    Id = existing?.Id ?? 0,
                //    PermissionStatus = existing?.PermissionStatus,
                //    ExistingFileName = existing?.FileName,
                //    HasFile = existing?.FileData != null && existing.FileData.Length > 0
                //});

                vm.PermissionList.Add(new CA_SS_PermissionVM
                {
                    CourseCode = int.TryParse(c.CourseCode, out var code) ? code : 0,
                    CourseName = c.CourseName,
                    Id = existing?.Id ?? 0,
                    PermissionStatus = existing?.PermissionStatus,
                    ExistingFileName = existing?.FileName,
                    HasFile = !string.IsNullOrEmpty(existing?.FilePath)
                });
            }

            // =====================================================
            // 3.b.ii Affiliation
            // =====================================================
            var savedAff = await _context.CaSsAffiliationGrantedYears
                .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            vm.AffiliationList.Clear();

            foreach (var c in courseList)
            {
                var existing = savedAff.FirstOrDefault(x => x.CourseCode == int.Parse(c.CourseCode));

                vm.AffiliationList.Add(new CA_SS_AffiliationGrantedYearVM
                {
                    Id = existing?.Id ?? 0,
                    CourseCode = int.Parse(c.CourseCode),
                    CourseName = c.CourseName,
                    AffiliationDate = existing?.AffiliationDate?.ToDateTime(TimeOnly.MinValue),
                    SanctionedIntake = c.PresentIntake ?? 0,
                    FileName = existing?.FileName,
                    HasFile = !string.IsNullOrEmpty(existing?.FilePath)
                });
            }

            // =====================================================
            // 3.b.iii LIC
            // =====================================================
            var savedLIC = await _context.CaSsLicpreviousInspections
              .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            vm.LICInspections.Clear();

            foreach (var c in courseList)
            {
                var existing = savedLIC.FirstOrDefault(x => x.CourseName == c.CourseName);

                vm.LICInspections.Add(new CA_SS_LICPreviousInspectionVM
                {
                    Id = existing?.Id ?? 0,
                    CollegeCode = collegeCode,
                    CourseName = c.CourseName,
                    InspectionDate = existing?.InspectionDate?.ToDateTime(TimeOnly.MinValue),
                    ActionTaken = existing?.ActionTaken
                });
            }

            // =====================================================
            // 3.b.iv Other Courses
            // =====================================================
            //var mstCourses = await _context.MstCourses
            //    .Where(x => x.CollegeCode == collegeCode &&
            //existingCourseLevels.Contains(x.CoursesApplied))
            //    .ToListAsync();

            var mstCourses = await _context.MstCourses
               .Where(x => x.FacultyCode == 1
                        && existingCourseLevels.Contains(x.CourseLevel))
               .ToListAsync();

            var allCourseNames = mstCourses.Select(x => x.CourseName).Distinct().ToList();

            var savedOther = await _context.CaSsOtherCoursesConducteds
               .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            var savedCourseNames = savedOther.Select(x => x.CourseName).ToList();

            vm.OtherCourses.Clear();

            foreach (var row in savedOther)
            {
                var availableForThisRow = allCourseNames
                    .Where(c => c == row.CourseName || !savedCourseNames.Contains(c))
                    .ToList();

                vm.OtherCourses.Add(new CA_SS_OtherCoursesConductedVM
                {
                    Id = row.Id,
                    CourseName = row.CourseName,
                    ExistingFileName =
                                       !string.IsNullOrEmpty(row.DocumentPath)
                                          ? row.FileName
                                          : null,
                    CourseList = availableForThisRow
                });
            }

            var availableForNewRow = allCourseNames
                .Where(c => !savedCourseNames.Contains(c))
                .ToList();

            vm.OtherCourses.Add(new CA_SS_OtherCoursesConductedVM
            {
                CourseList = availableForNewRow
            });

            return View("CA_SS_TwoAppliedCourses", vm);
        }// ✅ GET action closes here — all POST methods are now correctly outside




        //public async Task<List<PgCourseParticularsVm>> GetSSCoursesParticulars()
        //{
        //    var collegeCode = _userContext.CollegeCode;

        //    // 1️⃣ Existing affiliation data (may be empty)
        //    var existingData = await _context.AffiliationPgSsCourseDetails
        //        .Where(e => e.CollegeCode == collegeCode)
        //        .ToDictionaryAsync(e => e.CourseCode);


        //    // 2️⃣ All PG courses for the college
        //    var allCourses = await (
        //        from cc in _context.CollegeCourseIntakeDetails
        //        join ms in _context.MstCourses
        //            on cc.CourseCode equals ms.CourseCode.ToString()
        //        where cc.CollegeCode == collegeCode
        //        select new PgCourseVm
        //        {
        //            CourseCode = cc.CourseCode,
        //            CourseName = ms.CourseName,
        //            CourseLevel = ms.CourseLevel,
        //            CoursePrefix = ms.CoursePrefix,
        //            CollegeIntake = cc.PresentIntake,
        //            RguhsIntake = cc.ExistingIntake,
        //        }
        //    ).ToListAsync();

        //    var result = new List<PgCourseParticularsVm>();

        //    // 3️⃣ Overlay existing data (if any)
        //    foreach (var course in allCourses)
        //    {
        //        if (existingData.TryGetValue(course.CourseCode, out var existing))
        //        {
        //            result.Add(new PgCourseParticularsVm
        //            {
        //                CourseCode = course.CourseCode,
        //                DateofLOP = existing.Lopdate,
        //                DateofRecognitionByNMC = existing.DateofRecognitionByNmc,
        //                CourseLevel = course.CourseLevel,
        //                CourseName = course.CourseName,
        //                CoursePrefix = course.CoursePrefix,
        //                CollegeIntake = existing.PresentIntake,
        //                RguhsIntake = existing.RguhsIntake
        //            });

        //        }
        //        else
        //        {
        //            result.Add(new PgCourseParticularsVm
        //            {
        //                CourseCode = course.CourseCode,
        //                CourseLevel = course.CourseLevel,
        //                CourseName = course.CourseName,
        //                CoursePrefix = course.CoursePrefix,
        //                CollegeIntake = course.CollegeIntake,
        //                RguhsIntake = course.RguhsIntake
        //            });
        //        }
        //    }

        //    // 4️⃣ Existing first (optional ordering)
        //    return result
        //        .OrderByDescending(c => c.DateofLOP.HasValue)
        //        .ToList();
        //}





        //===========================
        //POST : Save Course Particulars
        //===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_SS_SSCourseParticulars(CA_SS_FullViewVM model)
        {
            bool hasError = false;
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            //var courseLevel = HttpContext.Session.GetString("CourseLevel"); // ADD


            // ---------- 3.b.i ----------
            //foreach (var row in model.PermissionList)
            //{
            //    if (row.PermissionStatus == "Yes" &&
            //        row.PermissionFile == null)
            //    {
            //        ModelState.AddModelError("",
            //            $"File required for {row.CourseName}");

            //        hasError = true;
            //    }
            //}

            if (model.PermissionList != null)
            {
                foreach (var row in model.PermissionList)
                {
                    if (row.PermissionStatus == "Yes" &&
                        row.PermissionFile == null)
                    {
                        ModelState.AddModelError("",
                            $"File required for {row.CourseName}");

                        hasError = true;
                    }
                }
            }



            // ---------- 3.b.ii ----------
            if (model.AffiliationList != null)
            {
                foreach (var row in model.AffiliationList)
                {
                    if (row.AffiliationDate != null &&
                        row.SupportingDoc == null)
                    {
                        ModelState.AddModelError("",
                            $"File required for {row.CourseName}");

                        hasError = true;
                    }
                }
            }

            if (model.OtherCourses != null)
            {
                // ---------- 3.b.iv ----------
                foreach (var row in model.OtherCourses)
                {
                    if (string.IsNullOrEmpty(row.CourseName) ||
                        row.SupportingDoc == null)
                    {
                        ModelState.AddModelError("",
                            "All Other Course fields required");

                        hasError = true;
                    }
                }
            }


            ////if (!ModelState.IsValid || hasError)
            ////{
            ////    return View("CA_SS_TwoAppliedCourses", model);
            ////}
            ///

            if (!ModelState.IsValid || hasError)
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");

                await PopulateFullViewModel(model, collegeCode);

                TempData["Error"] = "Please correct errors";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));

            }


            // SAVE LOGIC HERE

            await _context.SaveChangesAsync();

            TempData["Success"] = "Saved Successfully";

            return RedirectToAction(nameof(CA_SS_SSCourseParticulars));
        }



        // ===========================
        // POST : Save LIC Inspection
        // ===========================
        // ===========================
        // POST : Save LIC Inspection (3.b.iii)
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLICInspection(CA_SS_FullViewVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            List<string> existingCourseLevels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                existingCourseLevels = new List<string> { "SS" };
            }
            else
            {
                try
                {
                    existingCourseLevels =
                        System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(raw)
                        ?? new List<string> { "SS" };
                }
                catch
                {
                    existingCourseLevels =
                        raw.Split(',')
                           .Select(x => x.Trim().ToUpper())
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();
                }
            }

            // ================= VALIDATION =================

            bool hasAnyData = false;
            foreach (var row in model.LICInspections)
            {
                bool hasDate =
                    row.InspectionDate.HasValue;

                bool hasAction =
                    !string.IsNullOrWhiteSpace(
                        row.ActionTaken);

                if (hasDate || hasAction)
                    hasAnyData = true;

                if (hasDate && !hasAction)
                {
                    TempData["Error"] =
                        $"Action is required for {row.CourseName}";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                if (!hasDate && hasAction)
                {
                    TempData["Error"] =
                        $"Inspection Date is required for {row.CourseName}";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                if (!hasDate)
                    continue;

                var entity =
                  await _context.CaSsLicpreviousInspections
                  .FirstOrDefaultAsync(x =>
                     x.CollegeCode == collegeCode &&
                     x.CourseName == row.CourseName);

                if (entity == null)
                {
                    entity =
                      new CaSsLicpreviousInspection
                      {
                          CollegeCode = collegeCode,
                          CourseName = row.CourseName
                      };

                    _context.CaSsLicpreviousInspections
                        .Add(entity);
                }

                entity.CoursesApplied = "SS";

                entity.InspectionDate =
                  DateOnly.FromDateTime(
                     row.InspectionDate.Value);

                entity.ActionTaken =
                   row.ActionTaken;
            }
            if (!hasAnyData)
            {
                TempData["Info"] = "No new data entered to save.";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));
            }
            // If validation failed → Reload page with data
            if (!ModelState.IsValid)
            {
                await PopulateFullViewModel(model, collegeCode);
                TempData["Error"] = "Please correct LIC errors";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));
                //return View("CA_SS_TwoAppliedCourses", model);
            }

            // ================= SAVE =================



            await _context.SaveChangesAsync();

            TempData["Success"] = "LIC inspection details saved successfully!";

            return RedirectToAction(nameof(CA_SS_CoursesApplied));
        }


        //3.a LOP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLOP(CA_SS_FullViewVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            List<string> existingCourseLevels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                existingCourseLevels = new List<string> { "SS" };
            }
            else
            {
                try
                {
                    existingCourseLevels =
                        System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(raw)
                        ?? new List<string> { "SS" };
                }
                catch
                {
                    existingCourseLevels =
                        raw.Split(',')
                           .Select(x => x.Trim().ToUpper())
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();
                }
            }

            bool hasAnyData = false;
            foreach (var row in model.LopList)
            {
                bool hasLop = row.LopDate.HasValue;
                bool hasRecognition = row.RecognitionDate.HasValue;

                if (hasLop || hasRecognition)
                    hasAnyData = true;

                // both blank → ignore
                if (!hasLop && !hasRecognition)
                    continue;

                if (hasLop && !hasRecognition)
                {
                    TempData["Error"] =
                        $"Recognition Date is required for {row.CourseName}";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                if (!hasLop && hasRecognition)
                {
                    TempData["Error"] =
                        $"Date of LOP is required for {row.CourseName}";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                var entity = await _context.CaSsLopsavedDates
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.CourseCode == row.CourseCode);

                if (entity == null)
                {
                    entity = new CaSsLopsavedDate
                    {
                        CollegeCode = collegeCode,
                        CourseCode = row.CourseCode,
                        CourseName = row.CourseName
                    };

                    _context.CaSsLopsavedDates.Add(entity);
                }

                entity.CoursesApplied = "SS";

                entity.LopDate =
                    DateOnly.FromDateTime(row.LopDate.Value);

                entity.RecognitionDate =
                    DateOnly.FromDateTime(row.RecognitionDate.Value);

                entity.SanctionedIntake =
                    row.SanctionedIntake;
            }
            if (!hasAnyData)
            {
                TempData["Info"] = "No new data entered to save.";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "LOP details saved successfully!";

            return RedirectToAction(nameof(CA_SS_CoursesApplied));
        }


        //3.b.i Permission
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SavePermission(CA_SS_FullViewVM model)
        //{
        //    var collegeCode = HttpContext.Session.GetString("CollegeCode");

        //    foreach (var row in model.PermissionList)
        //    {
        //        if (string.IsNullOrEmpty(row.PermissionStatus))
        //            continue;

        //        byte[] fileData = null;
        //        string fileName = null;

        //        if (row.PermissionFile != null)
        //        {
        //            using var ms = new MemoryStream();
        //            await row.PermissionFile.CopyToAsync(ms);
        //            fileData = ms.ToArray();
        //            fileName = row.PermissionFile.FileName;
        //        }

        //        var entity = await _context.CaSsPermissions
        //            .FirstOrDefaultAsync(x =>
        //                x.CollegeCode == collegeCode &&
        //                x.CourseCode == row.CourseCode);

        //        if (entity == null)
        //        {
        //            entity = new CaSsPermission
        //            {
        //                CollegeCode = collegeCode,
        //                CourseCode = row.CourseCode,
        //                CourseName = row.CourseName
        //            };

        //            _context.CaSsPermissions.Add(entity);
        //        }

        //        entity.PermissionStatus = row.PermissionStatus;
        //        entity.FileData = fileData;
        //        entity.FileName = fileName;
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Permission saved successfully!";

        //    return RedirectToAction(nameof(CA_SS_CoursesApplied));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermission(CA_SS_FullViewVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            List<string> existingCourseLevels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                existingCourseLevels = new List<string> { "SS" };
            }
            else
            {
                try
                {
                    existingCourseLevels =
                        System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(raw)
                        ?? new List<string> { "SS" };
                }
                catch
                {
                    existingCourseLevels =
                        raw.Split(',')
                           .Select(x => x.Trim().ToUpper())
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();
                }
            }


            bool hasAnyData = false;
            foreach (var row in model.PermissionList)
            {
                if (!string.IsNullOrEmpty(row.PermissionStatus)
                    || row.PermissionFile != null)
                {
                    hasAnyData = true;
                }

                if (string.IsNullOrEmpty(row.PermissionStatus))
                    continue;

                string fileName = null;
                string filePath = null;

                if (FileTooLarge(row.PermissionFile))
                {
                    TempData["Error"] = "File size cannot exceed 2 MB";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                if (row.PermissionFile != null &&
                   row.PermissionFile.Length > 0)
                {
                    var path =
                        await SaveSSFileAsync(
                            row.PermissionFile,
                            "ParticularsDocs");

                    filePath = path;
                    fileName =
                        row.PermissionFile.FileName;
                }

                var entity =
                   await _context.CaSsPermissions
                   .FirstOrDefaultAsync(x =>
                      x.CollegeCode == collegeCode &&
                      x.CourseCode == row.CourseCode);

                if (entity == null)
                {
                    entity =
                      new CaSsPermission
                      {
                          CollegeCode = collegeCode,
                          CourseCode = row.CourseCode,
                          CourseName = row.CourseName,
                          CreatedOn = DateTime.Now
                      };

                    _context.CaSsPermissions.Add(entity);
                }

                entity.CoursesApplied = "SS";
                entity.PermissionStatus =
                   row.PermissionStatus;

                if (row.PermissionStatus == "No")
                {
                    entity.FilePath = null;
                    entity.FileName = null;
                }
                else if (row.PermissionStatus == "Yes")
                {
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        entity.FilePath = filePath;
                        entity.FileName = fileName;
                    }
                }
            }
            if (!hasAnyData)
            {
                TempData["Info"] = "No new data entered to save.";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Permission saved successfully!";
            return RedirectToAction(nameof(CA_SS_CoursesApplied));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAffiliation(CA_SS_FullViewVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            List<string> existingCourseLevels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                existingCourseLevels = new List<string> { "SS" };
            }
            else
            {
                try
                {
                    existingCourseLevels =
                        System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(raw)
                        ?? new List<string> { "SS" };
                }
                catch
                {
                    existingCourseLevels =
                        raw.Split(',')
                           .Select(x => x.Trim().ToUpper())
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();
                }
            }

            bool hasAnyData = false;
            foreach (var row in model.AffiliationList)
            {
                if (row.AffiliationDate != null ||
                   row.SupportingDoc != null)
                {
                    hasAnyData = true;
                }

                if (row.AffiliationDate == null)
                    continue;

                string fileName = null;
                string filePath = null;

                if (FileTooLarge(row.SupportingDoc))
                {
                    TempData["Error"] = "File size cannot exceed 2 MB";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                if (row.SupportingDoc != null &&
                   row.SupportingDoc.Length > 0)
                {
                    var path =
                      await SaveSSFileAsync(
                         row.SupportingDoc,
                         "AffiliationDocs");

                    filePath = path;
                    fileName = row.SupportingDoc.FileName;
                }

                var entity =
                  await _context.CaSsAffiliationGrantedYears
                  .FirstOrDefaultAsync(x =>
                     x.CollegeCode == collegeCode &&
                     x.CourseCode == row.CourseCode &&
                     x.CoursesApplied == "SS");

                if (entity == null)
                {
                    entity =
                      new CaSsAffiliationGrantedYear
                      {
                          CollegeCode = collegeCode,
                          CourseCode = row.CourseCode,
                          CourseName = row.CourseName,
                          CoursesApplied = "SS",
                          CreatedOn = DateTime.Now
                      };

                    _context.CaSsAffiliationGrantedYears
                        .Add(entity);
                }

                entity.AffiliationDate =
                  DateOnly.FromDateTime(
                    row.AffiliationDate.Value);

                entity.SanctionedIntake =
                  row.SanctionedIntake;

                entity.CoursesApplied = "SS";

                if (row.SupportingDoc != null &&
                   row.SupportingDoc.Length > 0)
                {
                    entity.FilePath = filePath;
                    entity.FileName = fileName;
                }
            }
            if (!hasAnyData)
            {
                TempData["Info"] = "No new data entered to save.";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Affiliation saved successfully!";
            return RedirectToAction(nameof(CA_SS_CoursesApplied));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherCourse(CA_SS_FullViewVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            // ================= VALIDATION =================
            foreach (var row in model.OtherCourses)
            {
                // ignore completely blank new row
                if (string.IsNullOrWhiteSpace(row.CourseName) &&
                    row.SupportingDoc == null)
                    continue;

                // Course selected but no file uploaded
                if (!string.IsNullOrWhiteSpace(row.CourseName) &&
                    row.SupportingDoc == null &&
                    row.Id == 0) // only new rows
                {
                    TempData["Error"] = "Upload document for selected course.";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }

                // File uploaded but no course selected
                if (string.IsNullOrWhiteSpace(row.CourseName) &&
                    row.SupportingDoc != null)
                {
                    TempData["Error"] = "Select course for uploaded file.";
                    return RedirectToAction(nameof(CA_SS_CoursesApplied));
                }
            }

            bool hasAnyData = false;
            // ================= SAVE =================
            foreach (var row in model.OtherCourses)
            {
                if (!string.IsNullOrWhiteSpace(row.CourseName)
                   || row.SupportingDoc != null)
                {
                    hasAnyData = true;
                }

                if (string.IsNullOrWhiteSpace(row.CourseName))
                    continue;

                CaSsOtherCoursesConducted existing = null;

                if (row.Id > 0)
                {
                    existing =
                     await _context.CaSsOtherCoursesConducteds
                     .FirstOrDefaultAsync(x => x.Id == row.Id);
                }
                else
                {
                    existing =
                     await _context.CaSsOtherCoursesConducteds
                     .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.CourseName == row.CourseName &&
                        x.CourseLevel == "SS");
                }

                if (existing != null)
                {
                    existing.CourseName = row.CourseName;
                    existing.CourseLevel = "SS";
                    existing.CoursesApplied = "SS";

                    if (row.SupportingDoc != null &&
                       row.SupportingDoc.Length > 0)
                    {
                        if (FileTooLarge(row.SupportingDoc))
                        {
                            TempData["Error"] = "File size cannot exceed 2 MB";
                            return RedirectToAction(nameof(CA_SS_CoursesApplied));
                        }

                        var path =
                           await SaveSSFileAsync(
                               row.SupportingDoc,
                               "OtherCoursesDocs");

                        existing.DocumentPath = path;
                        existing.FileName =
                           row.SupportingDoc.FileName;
                    }

                    continue;
                }

                if (row.SupportingDoc != null &&
                   row.SupportingDoc.Length > 0)
                {
                    var path =
                       await SaveSSFileAsync(
                          row.SupportingDoc,
                          "OtherCoursesDocs");

                    _context.CaSsOtherCoursesConducteds.Add(
                      new CaSsOtherCoursesConducted
                      {
                          CollegeCode = collegeCode,
                          CourseName = row.CourseName,
                          CourseLevel = "SS",
                          CoursesApplied = "SS",
                          DocumentPath = path,
                          FileName = row.SupportingDoc.FileName,
                          CreatedOn = DateTime.Now
                      });
                }
            }
            if (!hasAnyData)
            {
                TempData["Info"] = "No new data entered to save.";
                return RedirectToAction(nameof(CA_SS_CoursesApplied));
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Other Course saved successfully";

            return RedirectToAction(nameof(CA_SS_CoursesApplied));
        }
        //3.b.iii LIC Inspection

        //3.b.iii LIC Inspection
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SaveOtherCourse(CA_SS_FullViewVM model)
        //{
        //    var collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    //var courseLevel = "SS"; // ✅ ADD THIS LINE
        //    var courseLevel = HttpContext.Session.GetString("CourseLevel");

        //    foreach (var row in model.OtherCourses)
        //    {
        //        if (row.SupportingDoc == null || string.IsNullOrEmpty(row.CourseName))
        //            continue;

        //        // ✅ Check if this course already has a saved record
        //        var existing = await _context.CaSsOtherCoursesConducteds
        //            .FirstOrDefaultAsync(x =>
        //                x.CollegeCode == collegeCode &&
        //                x.CourseName == row.CourseName &&
        //                x.CourseLevel == courseLevel); // ✅ ADD CourseLevel condition

        //        byte[] fileData = null;
        //        using (var ms = new MemoryStream())
        //        {
        //            await row.SupportingDoc.CopyToAsync(ms);
        //            fileData = ms.ToArray();
        //        }

        //        if (existing != null)
        //        {
        //            // Course already saved → skip to avoid duplicate
        //            continue;
        //        }

        //        // Insert new record
        //        _context.CaSsOtherCoursesConducteds.Add(new CaSsOtherCoursesConducted
        //        {
        //            CollegeCode = collegeCode,
        //            CourseName = row.CourseName,
        //            CourseLevel = courseLevel,
        //            DocumentData = fileData,
        //            FileName = row.SupportingDoc.FileName,
        //            CreatedOn = DateTime.Now
        //        });
        //    }

        //    await _context.SaveChangesAsync();
        //    TempData["Success"] = "Other Course saved successfully!";
        //    return RedirectToAction(nameof(CA_SS_CoursesApplied));
        //}




        private async Task PopulateFullViewModel(CA_SS_FullViewVM vm, string collegeCode)
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            List<string> existingCourseLevels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                existingCourseLevels = new List<string> { "SS" };
            }
            else
            {
                try
                {
                    existingCourseLevels =
                        System.Text.Json.JsonSerializer
                        .Deserialize<List<string>>(raw)
                        ?? new List<string> { "SS" };
                }
                catch
                {
                    existingCourseLevels =
                        raw.Split(',')
                           .Select(x => x.Trim().ToUpper())
                           .Where(x => !string.IsNullOrEmpty(x))
                           .Distinct()
                           .ToList();
                }
            }

            // ================= Get Courses =================
            var courseList = await _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode)
                .ToListAsync();

            // ================= DM =================
            vm.DMCourses = await
            (
                from m in _context.MstCourses
                join c in _context.CollegeCourseIntakeDetails
                    on new { CourseCode = m.CourseCode.ToString(), m.FacultyCode }
                    equals new { CourseCode = c.CourseCode, c.FacultyCode }
                    into gj
                from c in gj.Where(x => x.CollegeCode == collegeCode).DefaultIfEmpty()

                where m.FacultyCode == 1
                      && existingCourseLevels.Contains(m.CourseLevel)
                      && m.CoursePrefix == "D.M."

                select new SSCourseRow
                {
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    PresentIntake = c != null ? c.PresentIntake : 0
                }

            ).ToListAsync();

            // ================= MCH =================
            vm.MChCourses = await
            (
                from m in _context.MstCourses
                join c in _context.CollegeCourseIntakeDetails
                    on new { CourseCode = m.CourseCode.ToString(), m.FacultyCode }
                    equals new { CourseCode = c.CourseCode, c.FacultyCode }
                    into gj
                from c in gj.Where(x => x.CollegeCode == collegeCode).DefaultIfEmpty()

                where m.FacultyCode == 1
                      && existingCourseLevels.Contains(m.CourseLevel)
                      && m.CoursePrefix
                            .Trim()
                            .Replace(".", "")
                            .Replace(" ", "")
                            .ToUpper() == "MCH"

                select new SSCourseRow
                {
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    PresentIntake = c != null ? c.PresentIntake : 0
                }

            ).ToListAsync();

            // ================= 3.a LOP =================
            vm.LopList.Clear();

            var savedLop = await _context.CaSsLopsavedDates
                .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            foreach (var c in courseList)
            {
                var existing = savedLop.FirstOrDefault(x => x.CourseCode == int.Parse(c.CourseCode));

                vm.LopList.Add(new CA_SS_LOPSavedDateVM
                {
                    CourseCode = int.Parse(c.CourseCode),
                    CourseName = c.CourseName,
                    SanctionedIntake = c.PresentIntake ?? 0,
                    LopDate = existing?.LopDate?.ToDateTime(TimeOnly.MinValue),
                    RecognitionDate = existing?.RecognitionDate?.ToDateTime(TimeOnly.MinValue)
                });
            }

            // ================= 3.b.i Permission =================
            vm.PermissionList.Clear();

            var savedPerm = await _context.CaSsPermissions
                .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            foreach (var c in courseList)
            {
                var existing = savedPerm.FirstOrDefault(x => x.CourseCode == int.Parse(c.CourseCode));

                vm.PermissionList.Add(new CA_SS_PermissionVM
                {
                    CourseCode = int.Parse(c.CourseCode),
                    CourseName = c.CourseName,
                    Id = existing?.Id ?? 0,
                    PermissionStatus = existing?.PermissionStatus,
                    ExistingFileName = existing?.FileName,
                    HasFile = existing?.FilePath != null && existing.FilePath.Length > 0
                });
            }

            // ================= 3.b.ii Affiliation =================
            vm.AffiliationList.Clear();

            var savedAff = await _context.CaSsAffiliationGrantedYears
               .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            foreach (var course in courseList)
            {
                var existing = savedAff.FirstOrDefault(x => x.CourseCode == int.Parse(course.CourseCode));

                vm.AffiliationList.Add(new CA_SS_AffiliationGrantedYearVM
                {
                    Id = existing?.Id ?? 0,
                    CourseCode = int.Parse(course.CourseCode),
                    CourseName = course.CourseName,
                    AffiliationDate = existing?.AffiliationDate?.ToDateTime(TimeOnly.MinValue),
                    SanctionedIntake = course.PresentIntake ?? 0,
                    FileName = existing?.FileName,
                    HasFile = existing?.FilePath != null && existing.FilePath.Length > 0
                });
            }

            // ================= 3.b.iii LIC =================
            vm.LICInspections.Clear();

            var savedLIC = await _context.CaSsLicpreviousInspections
                .Where(x => x.CollegeCode == collegeCode &&
            existingCourseLevels.Contains(x.CoursesApplied))
                .ToListAsync();

            foreach (var c in courseList)
            {
                var existing = savedLIC.FirstOrDefault(x => x.CourseName == c.CourseName);

                vm.LICInspections.Add(new CA_SS_LICPreviousInspectionVM
                {
                    Id = existing?.Id ?? 0,
                    CollegeCode = collegeCode,
                    CourseName = c.CourseName,
                    InspectionDate = existing?.InspectionDate?.ToDateTime(TimeOnly.MinValue),
                    ActionTaken = existing?.ActionTaken
                });
            }

            // ================= 3.b.iv Other Courses =================
            vm.OtherCourses.Clear();

            //var mstCourses = await _context.MstCourses
            //    .Where(x => x.FacultyCode == 1 && x.CourseLevel == courseLevel)
            //    .ToListAsync();

            var mstCourses = await _context.MstCourses
            .Where(x => x.FacultyCode == 1 &&
                        existingCourseLevels.Contains(x.CourseLevel))
            .ToListAsync();

            var allCourseNames = mstCourses.Select(x => x.CourseName).Distinct().ToList();

            //var savedOther = await _context.CaSsOtherCoursesConducteds
            //    .Where(x => x.CollegeCode == collegeCode && x.CourseLevel == courseLevel)
            //    .ToListAsync();

            var savedOther = await _context.CaSsOtherCoursesConducteds
                .Where(x => x.CollegeCode == collegeCode &&
                            existingCourseLevels.Contains(x.CourseLevel))
                .ToListAsync();
            var savedCourseNames = savedOther.Select(x => x.CourseName).ToList();

            foreach (var row in savedOther)
            {
                var availableForThisRow = allCourseNames
                    .Where(c => c == row.CourseName || !savedCourseNames.Contains(c))
                    .ToList();

                vm.OtherCourses.Add(new CA_SS_OtherCoursesConductedVM
                {
                    Id = row.Id,
                    CourseName = row.CourseName,
                    ExistingFileName = row.FileName ?? "View File",
                    CourseList = availableForThisRow
                });
            }

            var availableForNewRow = allCourseNames
                .Where(c => !savedCourseNames.Contains(c))
                .ToList();

            vm.OtherCourses.Add(new CA_SS_OtherCoursesConductedVM
            {
                CourseList = availableForNewRow
            });
        }

        public async Task<IActionResult> ViewAffiliationFile(int id)
        {
            var file =
                await _context.CaSsAffiliationGrantedYears
                    .FindAsync(id);

            if (file == null ||
                string.IsNullOrWhiteSpace(file.FilePath))
            {
                return NotFound();
            }

            // file physically missing on disk
            if (!System.IO.File.Exists(file.FilePath))
            {
                return NotFound();
            }

            return PhysicalFile(
                file.FilePath,
                "application/pdf",
                file.FileName
            );
        }

        public async Task<IActionResult> ViewOtherCourseFile(int id)
        {
            var file =
                await _context.CaSsOtherCoursesConducteds
                    .FindAsync(id);

            if (file == null ||
                string.IsNullOrWhiteSpace(file.DocumentPath))
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(file.DocumentPath))
            {
                return NotFound();
            }

            return PhysicalFile(
                file.DocumentPath,
                "application/pdf",
                file.FileName
            );
        }

        public async Task<IActionResult> ViewPermissionFile(int id)
        {
            var file =
                await _context.CaSsPermissions
                    .FindAsync(id);

            if (file == null ||
                string.IsNullOrWhiteSpace(file.FilePath))
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                return NotFound();
            }

            return PhysicalFile(
                file.FilePath,
                "application/pdf",
                file.FileName
            );
        }

        //public async Task<IActionResult> ViewDonationPdf()
        //{
        //    var collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    var facultyCode = HttpContext.Session.GetString("FacultyCode");
        //    var level = HttpContext.Session.GetString("CourseLevel");

        //    var record = await _context.MedCaAccountAndFeeDetails
        //        .FirstOrDefaultAsync(x =>
        //            x.CollegeCode == collegeCode &&
        //            x.FacultyCode == facultyCode &&
        //            x.CourseLevel == level);

        //    if (record == null || record.DonationPdf == null)
        //        return NotFound();

        //    return File(record.DonationPdf, "application/pdf", record.DonationPdfName);
        //}



    }
}