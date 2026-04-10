using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace Medical_Affiliation.Controllers
{
    public class LatestMedicalAffiliationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LatestMedicalAffiliationController(ApplicationDbContext context)
        {
            _context = context;

        }



        // GET: Intake/Index
        [HttpGet]
        public IActionResult Medical_LatestIntakeData()
        {

            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = HttpContext.Session.GetString("CollegeName");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Index", "Home");
            }

            // Safe parse for facultyId
            if (!int.TryParse(facultyCode, out int facultyId))
            {
                return BadRequest("Invalid faculty code");
            }

            var model = new IntakePageViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode
            };

            // Base data (unchanged - ToList() forces DB execution)
            var intakeDetails = _context.CollegeCourseIntakeDetails
                .Where(d => d.FacultyCode == facultyId && d.CollegeCode == collegeCode)
                .ToList();

            var allCourses = _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)
                .ToList();

            var latestIntakes = _context.IntakeDetailsLatests
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .ToList();

            // ---------- UG (UNCHANGED) ----------
            var ugintakelist = from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "UG"
                               join l in latestIntakes.Where(li => li.CourseLevel == "UG")
                                   on c.CourseCode.ToString() equals l.CourseCode into lj
                               from lrec in lj.DefaultIfEmpty()
                               where lrec == null || lrec.NewCourseSeatRequested == 0
                               select new IntakeByLevelViewModel
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   ExistingIntake = d.ExistingIntake.GetValueOrDefault(),
                                   AdditionalIntake = lrec?.AdditionalSeatRequested,
                                   IsUgDeclarationAccepted = lrec != null && lrec.IsDeclared == 1,
                                   PrincipalName = lrec != null ? lrec.PrincipalName : null
                               };

            var ugList = ugintakelist.ToList().DistinctBy(x => x.CourseCode).ToList();
            model.UgCourses = ugList;

            if (ugList.Any())
            {
                model.IsUgDeclarationAccepted = ugList.Any(x => x.IsUgDeclarationAccepted);
                model.PrincipalNameUG = ugList.FirstOrDefault(x => !string.IsNullOrEmpty(x.PrincipalName))?.PrincipalName;
            }

            // ---------- PG (UNCHANGED) ----------
            var pgintakelist = from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "PG"
                               join l in latestIntakes.Where(li => li.CourseLevel == "PG")
                                   on c.CourseCode.ToString() equals l.CourseCode into lj
                               from lrec in lj.DefaultIfEmpty()
                               where lrec == null || lrec.NewCourseSeatRequested == 0
                               select new IntakeByLevelViewModel
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   ExistingIntake = (int)(d.ExistingIntake ?? 0),
                                   AdditionalIntake = (int)(lrec?.AdditionalSeatRequested ?? 0),
                                   IsDeclared = lrec != null && lrec.IsDeclared == 1,
                                   PrincipalName = lrec != null ? lrec.PrincipalName : null
                               };

            var pgList = pgintakelist.ToList().DistinctBy(x => x.CourseCode).ToList();
            model.PgCourses = pgList;

            if (pgList.Any())
            {
                model.IsPgDeclarationAccepted = pgList.Any(x => x.IsDeclared);
                model.PrincipalNamePG = pgList.FirstOrDefault(x => !string.IsNullOrEmpty(x.PrincipalName))?.PrincipalName;
            }

            // ---------- SS (UNCHANGED) ----------
            var ssintakelist = from d in intakeDetails.Where(d => int.TryParse(d.CourseCode, out _))
                               let courseCodeInt = int.Parse(d.CourseCode!)
                               join c in allCourses on courseCodeInt equals c.CourseCode
                               where c.CourseLevel == "SS"
                               join l in latestIntakes.Where(li => li.CourseLevel == "SS")
                                   on c.CourseCode.ToString() equals l.CourseCode into lj
                               from lrec in lj.DefaultIfEmpty()
                               where lrec == null || lrec.NewCourseSeatRequested == 0
                               select new IntakeByLevelViewModel
                               {
                                   CourseCode = c.CourseCode.ToString(),
                                   CourseName = c.CourseName,
                                   ExistingIntake = (int)(d.ExistingIntake ?? 0),
                                   AdditionalIntake = (int)(lrec?.AdditionalSeatRequested ?? 0),
                                   IsDeclared = lrec != null && lrec.IsDeclared == 1,
                                   PrincipalName = lrec != null ? lrec.PrincipalName : null
                               };

            var ssList = ssintakelist.ToList().DistinctBy(x => x.CourseCode).ToList();
            model.SsCourses = ssList;

            if (ssList.Any())
            {
                model.IsSsDeclarationAccepted = ssList.Any(x => x.IsDeclared);
                model.PrincipalNameSS = ssList.FirstOrDefault(x => !string.IsNullOrEmpty(x.PrincipalName))?.PrincipalName;
            }

            // ---------- Saved data table (UPDATED with NMCDATA & NMCDOC) ----------
            var savedQuery = from l in latestIntakes.Where(l => int.TryParse(l.CourseCode, out _))
                             let courseCodeInt = int.Parse(l.CourseCode!)
                             join c in allCourses on courseCodeInt equals c.CourseCode
                             select new SavedIntakeRowViewModel
                             {
                                 Id = l.Id,
                                 CourseLevel = l.CourseLevel,
                                 CourseCode = l.CourseCode,
                                 CourseName = c.CourseName,
                                 AdditionalIntake = (int)(l.AdditionalSeatRequested),
                                 ExistingIntake = (int)(l.ExistingIntakeCa),


                                 // Total should be sum of existing + additional + new-course seats when TotalIntake not explicitly set
                                 TotalIntake = (int)(l.TotalIntake != 0
                                     ? l.TotalIntake
                                     : (l.ExistingIntakeCa) + (l.AdditionalSeatRequested) + (l.NewCourseSeatRequested)),
                                 PrincipalName = l.PrincipalName,
                                 NMCDATA = l.Nmcdata,        // ✅ ADDED
                                 HasNMCDOC = l.Nmcdoc != null, // ✅ ADDED - Boolean flag for UI
                                 CreatedOn = (DateTime)(l.CreatedOn)
                             };

            model.SavedIntakes = savedQuery
                .OrderBy(x => x.CourseLevel == "UG" ? 1
                               : x.CourseLevel == "PG" ? 2
                               : x.CourseLevel == "SS" ? 3 : 4)
                .ThenBy(x => x.CourseName)
                .ToList();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(50_000_000)]
        public IActionResult Medical_LatestIntakeData(
            IntakePageViewModel model,
            string courseLevel,
            IFormFile? bankStatement) // removed single nmcdoc parameter; per-course files come in the viewmodel
        {
            // 1️⃣ Session values
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Index", "Home");

            model.FacultyCode = facultyCode;
            model.CollegeCode = collegeCode;

            // 2️⃣ Remove unrelated validations (keep same behaviour)
            ModelState.Remove(nameof(IntakePageViewModel.IsUgDeclarationAccepted));
            ModelState.Remove(nameof(IntakePageViewModel.IsPgDeclarationAccepted));
            ModelState.Remove(nameof(IntakePageViewModel.IsSsDeclarationAccepted));
            ModelState.Remove(nameof(IntakePageViewModel.PrincipalNameUG));
            ModelState.Remove(nameof(IntakePageViewModel.PrincipalNamePG));
            ModelState.Remove(nameof(IntakePageViewModel.PrincipalNameSS));

            if (string.IsNullOrWhiteSpace(courseLevel))
                ModelState.AddModelError("courseLevel", "Invalid submission.");

            if (!ModelState.IsValid)
                return View(model);

            // 3️⃣ Read bank statement ONCE
            byte[]? collegeBankBytes = null;
            if (bankStatement != null && bankStatement.Length > 0)
            {
                using var ms = new MemoryStream();
                bankStatement.CopyTo(ms);
                collegeBankBytes = ms.ToArray();
            }

            // 4️⃣ Resolve declaration & principal name
            int declared;
            string? principalName;

            switch (courseLevel)
            {
                case "UG":
                    declared = model.IsUgDeclarationAccepted ? 1 : 0;
                    principalName = model.PrincipalNameUG;
                    break;

                case "PG":
                    declared = model.IsPgDeclarationAccepted ? 1 : 0;
                    principalName = model.PrincipalNamePG;
                    break;

                case "SS":
                    declared = model.IsSsDeclarationAccepted ? 1 : 0;
                    principalName = model.PrincipalNameSS;
                    break;

                default:
                    ModelState.AddModelError("", "Invalid course level.");
                    return View(model);
            }

            // 5️⃣ Get courses based on level
            IEnumerable<IntakeByLevelViewModel>? courses = courseLevel switch
            {
                "UG" => model.UgCourses,
                "PG" => model.PgCourses,
                "SS" => model.SsCourses,
                _ => null
            };

            if (courses == null || !courses.Any())
            {
                ModelState.AddModelError("", "No course data submitted.");
                return View(model);
            }

            // 6️⃣ Save entities (bank statement assigned once; each course can have its own NMCDATA and per-course NMCDOC)
            bool bankAssigned = false;

            foreach (var item in courses)
            {
                // parse NMCDATA (view renders numeric input but VM stores string)
                int nmcValue = 0;
                if (!string.IsNullOrWhiteSpace(item.NMCDATA))
                {
                    int.TryParse(item.NMCDATA, out nmcValue);
                }

                var entity = new IntakeDetailsLatest
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseLevel = courseLevel,
                    CourseCode = item.CourseCode,
                    ExistingIntakeCa = item.ExistingIntake.GetValueOrDefault(),
                    AdditionalSeatRequested = item.AdditionalIntake.GetValueOrDefault(),
                    NewCourseSeatRequested = 0,
                    // include NMCDATA numeric value in TotalIntake calculation
                    TotalIntake = item.ExistingIntake.GetValueOrDefault() + item.AdditionalIntake.GetValueOrDefault() + nmcValue,
                    IsDeclared = declared,
                    PrincipalName = principalName ?? string.Empty,
                    // persist the raw NMCDATA (string) and per-course file if uploaded
                    Nmcdata = string.IsNullOrWhiteSpace(item.NMCDATA) ? null : item.NMCDATA,
                    Nmcdoc = null,
                    CreatedOn = DateTime.Now
                };

                // Assign bank statement once
                if (!bankAssigned && collegeBankBytes != null)
                {
                    entity.BankStatement = collegeBankBytes;
                    bankAssigned = true;
                }

                // Per-course uploaded NMC document (bind to IntakeByLevelViewModel.NmcDocument)
                if (item is { NmcDocument: { } file } && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    file.CopyTo(ms);
                    entity.Nmcdoc = ms.ToArray();
                }

                _context.IntakeDetailsLatests.Add(entity);
            }

            // 7️⃣ Save to DB
            _context.SaveChanges();

            return RedirectToAction(nameof(Medical_LatestIntakeData));
        }


        [HttpGet]
        public IActionResult AddCourse()
        {
            if (!TryGetSessionCodes(out var facultyCode, out var collegeCode, out var facultyId))
                return RedirectToAction("Index", "Home");

            var model = PrepareViewModel(facultyId, facultyCode, collegeCode);
            return View(model);
        }


        // Ajax: get courses by level
        [HttpGet]
        public IActionResult GetCoursesByLevel(string level)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");  // Add this

            if (string.IsNullOrWhiteSpace(level))
                return BadRequest("Level is required");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return BadRequest("Session data missing");

            int facultyId = int.Parse(facultyCode);

            // All courses for this faculty/level
            var allCourses = _context.MstCourses
                .Where(c => c.CourseLevel == level && c.FacultyCode == facultyId)
                .ToList();

            // Existing intake details for this college/faculty
            var intakeDetails = _context.CollegeCourseIntakeDetails
                .Where(d => d.FacultyCode == facultyId && d.CollegeCode == collegeCode)
                .Select(d => int.Parse(d.CourseCode))  // Assuming CourseCode is string, match as int
                .ToList();

            // Courses NOT in intakeDetails
            var availableCourses = allCourses
                .Where(c => !intakeDetails.Contains(c.CourseCode))
                .OrderBy(c => c.CourseName)
                .Select(c => new
                {
                    value = c.CourseCode,
                    text = c.CourseName
                })
                .ToList();

            return Json(availableCourses);
        }

        [HttpGet]
        public IActionResult DownloadNMCDoc(int id)
        {
            var intake = _context.IntakeDetailsLatests.Find(id);
            if (intake?.Nmcdoc == null) return NotFound();

            return File(intake.Nmcdoc, "application/pdf", $"NMC_Doc_{intake.CourseCode}.pdf");
        }

        // POST: Intake/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddCourse(AddIntakeViewModel model)
        {
            if (!TryGetSessionCodes(out var facultyCode, out var collegeCode, out var facultyId))
                return RedirectToAction("Index", "Home");

            // Clean up model state for fields we control
            ModelState.Remove(nameof(model.AcademicYear));
            ModelState.Remove(nameof(model.AcademicYears));
            ModelState.Remove(nameof(model.CourseLevels));
            ModelState.Remove(nameof(model.Courses));
            ModelState.Remove(nameof(model.SavedIntakes));
            ModelState.Remove(nameof(model.FacultyCode));
            ModelState.Remove(nameof(model.CollegeCode));



            if (!ModelState.IsValid)
            {
                // Collect validation errors for debugging/logging
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Errors = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();
                TempData["ValidationErrors"] = validationErrors;

                // Repopulate for redisplay
                var vm = PrepareViewModel(facultyId, facultyCode, collegeCode, model);
                return View(vm);
            }

            // Force fixed academic year now that model is valid
            model.AcademicYear = new DateOnly(2026, 7, 1);

            // Business rule: Check for duplicates
            bool alreadyExists = _context.IntakeDetailsLatests
                .Any(d => d.FacultyCode == facultyCode &&
                          d.CollegeCode == collegeCode &&
                          d.CourseCode == model.SelectedCourseCode &&
                          d.CourseRequestingYear == model.AcademicYear);

            if (alreadyExists)
            {
                ModelState.AddModelError(nameof(model.SelectedCourseCode), "Intake for this course and academic year already exists.");
                var vm = PrepareViewModel(facultyId, facultyCode, collegeCode, model);
                return View(vm);
            }

            // Map to entity
            var entity = new IntakeDetailsLatest
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseLevel = model.SelectedCourseLevel,
                CourseCode = model.SelectedCourseCode,
                CourseRequestingYear = model.AcademicYear.Value,
                NewCourseSeatRequested = model.NewCourseSeatRequested,
                TotalIntake = model.TotalIntake,
                PrincipalName = model.PrincipalName?.Trim(),
                IsDeclared = model.IsDeclarationAccepted ? 1 : 0,
                CreatedOn = DateTime.Now
                // CreatedBy = User.Identity?.Name ?? "System", // Uncomment if tracking users
            };

            try
            {
                _context.IntakeDetailsLatests.Add(entity);
                int rowsAffected = _context.SaveChanges();
                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Intake added successfully for academic year 2026-27.";
                    return RedirectToAction(nameof(AddCourse));
                }
                else
                {
                    TempData["ErrorMessage"] = "No changes were saved to the database.";
                }
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = "Database save failed: " +
                    (dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unexpected error: " + ex.Message;
            }

            // On error, repopulate and return view
            var errorVm = PrepareViewModel(facultyId, facultyCode, collegeCode, model);
            return View(errorVm);
        }

        // ── Helper Methods ──

        private bool TryGetSessionCodes(out string? facultyCode, out string? collegeCode, out int facultyId)
        {
            facultyCode = HttpContext.Session.GetString("FacultyCode");
            collegeCode = HttpContext.Session.GetString("CollegeCode");
            facultyId = 0;

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return false;

            return int.TryParse(facultyCode, out facultyId);
        }

        private AddIntakeViewModel PrepareViewModel(int facultyId, string facultyCode, string collegeCode, AddIntakeViewModel? existing = null)
        {
            var model = existing ?? new AddIntakeViewModel
            {
                AcademicYear = new DateOnly(2026, 7, 1),
                CollegeName = HttpContext.Session.GetString("CollegeName") ?? "Your College"  // Assuming you added this to ViewModel
            };

            // Course levels
            var levels = _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)
                .Select(x => x.CourseLevel)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            model.CourseLevels = levels.Select(l => new SelectListItem
            {
                Value = l,
                Text = l,
                Selected = l == model.SelectedCourseLevel
            }).ToList();

            // Courses (cascade if level selected)
            if (!string.IsNullOrEmpty(model.SelectedCourseLevel))
            {
                model.Courses = _context.MstCourses
                    .Where(c => c.CourseLevel == model.SelectedCourseLevel && c.FacultyCode == facultyId)
                    .OrderBy(c => c.CourseName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseCode.ToString(),
                        Text = c.CourseName,
                        Selected = c.CourseCode.ToString() == model.SelectedCourseCode
                    }).ToList();
            }
            else
            {
                model.Courses = new List<SelectListItem>();
            }

            // Fixed academic year dropdown (for display consistency)
            model.AcademicYears = new List<SelectListItem>
    {
        new SelectListItem { Value = "2026-27", Text = "2026-27", Selected = true }
    };

            // Saved intakes
            model.SavedIntakes = (from d in _context.IntakeDetailsLatests
                                  join c in _context.MstCourses on d.CourseCode equals c.CourseCode.ToString()
                                  where d.FacultyCode == facultyCode && d.CollegeCode == collegeCode
                                  orderby d.CreatedOn descending
                                  select new SavedIntakeRowViewModel
                                  {
                                      Id = d.Id,
                                      CourseLevel = d.CourseLevel,
                                      CourseCode = d.CourseCode,
                                      CourseName = c.CourseName,
                                      NewCourseSeatRequested = (int)d.NewCourseSeatRequested,
                                      TotalIntake = (int)(d.TotalIntake),
                                      PrincipalName = d.PrincipalName,
                                      CreatedOn = (DateTime)d.CreatedOn
                                  }).ToList();

            return model;
        }

        // Helper method (extracted for cleaner code)
        private List<SavedIntakeRowViewModel> LoadSavedIntakes(string facultyCode, string collegeCode)
        {
            return (from d in _context.IntakeDetailsLatests
                    join c in _context.MstCourses on d.CourseCode equals c.CourseCode.ToString()
                    where d.FacultyCode == facultyCode && d.CollegeCode == collegeCode
                    orderby d.CreatedOn descending
                    select new SavedIntakeRowViewModel
                    {
                        Id = d.Id,
                        CourseLevel = d.CourseLevel,
                        CourseCode = d.CourseCode,
                        CourseName = c.CourseName,
                        NewCourseSeatRequested = (int)(d.NewCourseSeatRequested),
                        TotalIntake = (int)(d.TotalIntake),
                        PrincipalName = d.PrincipalName ?? "",
                        CreatedOn = (DateTime)d.CreatedOn
                        // AcademicYear        = d.AcademicYear   ← add when column exists
                    }).ToList();
        }


        [HttpGet]
        public IActionResult SavedIntakeData()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Index", "Home");

            int facultyId = int.Parse(facultyCode);

            var savedData = _context.IntakeDetailsLatests
                .Where(d => d.FacultyCode == facultyId.ToString() && d.CollegeCode == collegeCode)
                .OrderByDescending(d => d.CreatedOn)
                .Select(d => new
                {
                    d.CourseLevel,
                    CourseName = _context.MstCourses.Where(c => c.CourseCode.ToString() == d.CourseCode).Select(c => c.CourseName).FirstOrDefault(),
                    d.CourseCode,
                    d.NewCourseSeatRequested,
                    d.TotalIntake,
                    d.PrincipalName,
                    d.CreatedOn
                })
                .ToList();

            return View(savedData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteIntake(int id)
        {
            var entity = _context.IntakeDetailsLatests.FirstOrDefault(x => x.Id == id);
            if (entity != null)
            {
                _context.IntakeDetailsLatests.Remove(entity);
                _context.SaveChanges();
            }
            return RedirectToAction("AddCourse");
        }

        [HttpGet]
        public IActionResult EditIntake(int id)
        {
            var entity = _context.IntakeDetailsLatests.Find(id);
            if (entity == null) return NotFound();

            var vm = new SavedIntakeRowViewModel
            {
                Id = entity.Id,
                CourseLevel = entity.CourseLevel,
                CourseCode = entity.CourseCode,
                NewCourseSeatRequested = (int)(entity.NewCourseSeatRequested),
                TotalIntake = (int)(entity.TotalIntake),
                PrincipalName = entity.PrincipalName
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditIntake(SavedIntakeRowViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = _context.IntakeDetailsLatests.Find(model.Id);
            if (entity == null) return NotFound();

            entity.NewCourseSeatRequested = model.NewCourseSeatRequested;
            entity.TotalIntake = model.TotalIntake;
            entity.PrincipalName = model.PrincipalName;

            _context.Update(entity);
            _context.SaveChanges();

            return RedirectToAction("AddCourse");
        }

        private byte[]? GetFileBytes(IFormFile? file)
        {
            if (file == null) return null;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                return ms.ToArray();
            }
        }

        // Add these two actions into the LatestMedicalAffiliationController class

        // GET: /LatestMedicalAffiliation/IntakeReport
        [HttpGet]
        public IActionResult IntakeReport(string? selectedFaculty, string? selectedCollege)
        {
            // Build view model
            var vm = new IntakeReportViewModel();

            // Faculty list from CollegeCourseIntakeDetails (distinct FacultyCode)
            var facultyCodes = _context.CollegeCourseIntakeDetails
                .Select(d => d.FacultyCode)
                .Distinct()
                .OrderBy(f => f)
                .ToList();

            vm.FacultyList = facultyCodes
                .Select(f => new SelectListItem { Value = f.ToString(), Text = f.ToString() })
                .ToList();

            vm.SelectedFaculty = selectedFaculty;
            vm.SelectedCollege = selectedCollege;

            // If faculty selected, load colleges for that faculty
            if (!string.IsNullOrWhiteSpace(selectedFaculty) && int.TryParse(selectedFaculty, out int facId))
            {
                var colleges = _context.CollegeCourseIntakeDetails
                    .Where(d => d.FacultyCode == facId)
                    .Select(d => d.CollegeCode)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                vm.CollegeList = colleges
                    .Select(c => new SelectListItem { Value = c, Text = c })
                    .ToList();
            }

            // If both selected, load results
            if (!string.IsNullOrWhiteSpace(selectedFaculty) && !string.IsNullOrWhiteSpace(selectedCollege))
            {
                // IntakeDetailsLatests stores FacultyCode as string in your project code, so compare as string
                var results = from l in _context.IntakeDetailsLatests
                              join c in _context.MstCourses
                                  on l.CourseCode equals c.CourseCode.ToString() into cj
                              from cinfo in cj.DefaultIfEmpty()
                              where l.FacultyCode == selectedFaculty && l.CollegeCode == selectedCollege
                              orderby l.CourseLevel, cinfo.CourseName
                              select new SavedIntakeRowViewModel
                              {
                                  Id = l.Id,
                                  CourseLevel = l.CourseLevel,
                                  CourseCode = l.CourseCode,
                                  CourseName = cinfo != null ? cinfo.CourseName : l.CourseCode,
                                  NewCourseSeatRequested = (int)(l.NewCourseSeatRequested),
                                  AdditionalIntake = (int)(l.AdditionalSeatRequested),
                                  ExistingIntake = (int)(l.ExistingIntakeCa),
                                  TotalIntake = (int)(l.TotalIntake + l.ExistingIntakeCa + l.AdditionalSeatRequested),
                                  PrincipalName = l.PrincipalName,
                                  CreatedOn = (DateTime)l.CreatedOn
                              };

                vm.Results = results.ToList();
            }

            return View("IntakeReport", vm);
        }

        // POST: /LatestMedicalAffiliation/IntakeReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IntakeReport(IntakeReportViewModel model, string submit)
        {
            // Use POST-Redirect-GET pattern: forward selection to GET so URL can be shared/bookmarked
            return RedirectToAction(nameof(IntakeReport), new { selectedFaculty = model.SelectedFaculty, selectedCollege = model.SelectedCollege });
        }// Add these two actions into the LatestMedicalAffiliationController class

        // GET: /LatestMedicalAffiliation/IntakeReport
        // GET: /LatestMedicalAffiliation/IntakeReport
        [HttpGet]
        public IActionResult IntakeReport_Data(string? selectedFaculty, string? selectedCollege)
        {
            var vm = new IntakeReportViewModel();

            // -------- Faculty List --------
            var facultyCodes = _context.CollegeCourseIntakeDetails
                .Select(x => x.FacultyCode)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            vm.FacultyList = facultyCodes
                .Select(f => new SelectListItem
                {
                    Value = f.ToString(),
                    Text = f.ToString()
                })
                .ToList();

            vm.SelectedFaculty = selectedFaculty;
            vm.SelectedCollege = selectedCollege;

            // -------- College List --------
            vm.CollegeList = new List<SelectListItem>();

            if (!string.IsNullOrWhiteSpace(selectedFaculty)
                && int.TryParse(selectedFaculty, out int facultyCode))
            {
                var colleges = _context.CollegeCourseIntakeDetails
                    .Where(x => x.FacultyCode == facultyCode)
                    .Select(x => x.CollegeCode)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                vm.CollegeList = colleges
                    .Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = c
                    })
                    .ToList();
            }

            // -------- Results --------
            if (!string.IsNullOrWhiteSpace(selectedFaculty)
                && !string.IsNullOrWhiteSpace(selectedCollege))
            {
                vm.Results = (
                    from l in _context.IntakeDetailsLatests
                    join c in _context.MstCourses
                        on l.CourseCode equals c.CourseCode.ToString() into cj
                    from cinfo in cj.DefaultIfEmpty()
                    where l.FacultyCode == selectedFaculty
                          && l.CollegeCode == selectedCollege
                    orderby l.CourseLevel, cinfo.CourseName
                    select new SavedIntakeRowViewModel
                    {
                        Id = l.Id,
                        CourseLevel = l.CourseLevel,
                        CourseCode = l.CourseCode,
                        CourseName = cinfo != null ? cinfo.CourseName : l.CourseCode,

                        ExistingIntake = (int)(l.ExistingIntakeCa),
                        AdditionalIntake = (int)(l.AdditionalSeatRequested),
                        NewCourseSeatRequested = (int)(l.NewCourseSeatRequested),

                        TotalIntake =
                            (int)((l.ExistingIntakeCa)
                                + (l.AdditionalSeatRequested)
                                + (l.NewCourseSeatRequested)),

                        PrincipalName = l.PrincipalName,
                        CreatedOn = (DateTime)l.CreatedOn
                    }
                ).ToList();
            }

            return View("IntakeReport", vm);
        }

        // ===============================
        // POST (PRG pattern)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IntakeReport_Data(IntakeReportViewModel model)
        {
            return RedirectToAction(
                nameof(IntakeReport),
                new
                {
                    selectedFaculty = model.SelectedFaculty,
                    selectedCollege = model.SelectedCollege
                });
        }
        [HttpGet]
        public async Task<IActionResult> TeachingFacultyDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return View(new List<TeachingFacultyViewModel>());

            // ============================================================
            // 1. FETCH EXISTING MBBS INTAKE FROM IntakeDetailsLatest
            // ============================================================
            var mbbsIntake = await _context.IntakeDetailsLatests
                .Where(x => x.CollegeCode == collegeCode
                            && x.FacultyCode == facultyCode
                            && x.CourseCode == "1017") // OR CourseCode == "1017"
                .Select(x => x.TotalIntake)
                .FirstOrDefaultAsync();

            ViewBag.UGMBBSIntake = mbbsIntake.ToString() ?? "0";

            if (mbbsIntake == null)
                return View(new List<TeachingFacultyViewModel>());

            // ============================================================
            // 2. FIND SEAT SLAB ID USING TOTAL INTAKE
            // ============================================================
            var seatSlabId = await _context.SeatSlabMasters
                .Where(s => s.SeatSlab == mbbsIntake)
                .Select(s => s.SeatSlabId)
                .FirstOrDefaultAsync();

            if (seatSlabId == null)
                return View(new List<TeachingFacultyViewModel>());

            // ============================================================
            // 3. FETCH FACULTY NAME
            // ============================================================
            var facultyName = await _context.Faculties
                .Where(f => f.FacultyId == 1) // change if dynamic
                .Select(f => f.FacultyName)
                .FirstOrDefaultAsync();

            // ============================================================
            // 4. MAIN QUERY – DEPARTMENT + DESIGNATION REQUIREMENTS
            // ============================================================
            var intakeDetails = await (
                from sr in _context.DepartmentWiseFacultyMasters
                join sl in _context.SeatSlabMasters on sr.SeatSlabId equals sl.SeatSlabId
                join dm in _context.DesignationMasters on sr.DesignationCode equals dm.DesignationCode
                join dp in _context.DepartmentMasters on sr.DepartmentCode equals dp.DepartmentCode
                where sl.SeatSlabId == seatSlabId
                group sr by new
                {
                    dp.DepartmentCode,
                    dp.DepartmentName,
                    dp.FacultyCode,
                    dm.DesignationCode,
                    dm.DesignationName,
                    sl.SeatSlabId,
                    sr.Seats,
                    FacultyName = facultyName
                }
                into g
                select new TeachingFacultyViewModel
                {
                    CollegeCode = collegeCode,
                    DepartmentCode = g.Key.DepartmentCode,
                    DepartmentName = g.Key.DepartmentName,
                    FacultyCode = g.Key.FacultyCode.ToString(),
                    Faculty = g.Key.FacultyName,
                    DesignationCode = g.Key.DesignationCode,
                    DesignationName = g.Key.DesignationName,
                    SeatSlabId = g.Key.SeatSlabId,
                    ExistingSeatIntake = g.Key.Seats.ToString(),
                    PresentSeatIntake = "0"
                }
            ).ToListAsync();

            // ============================================================
            // 5. OVERLAY EXISTING SAVED DATA (EDIT MODE)
            // ============================================================
            var existingRecords = await _context.CollegeDesignationDetails
                .Where(c => c.CollegeCode == collegeCode)
                .ToListAsync();

            foreach (var item in intakeDetails)
            {
                var existing = existingRecords.FirstOrDefault(e =>
                    e.CollegeCode == collegeCode &&
                    e.DepartmentCode == item.DepartmentCode &&
                    e.DesignationCode == item.DesignationCode &&
                    e.SeatSlabId == item.SeatSlabId.ToString());

                if (existing != null)
                {
                    item.ExistingSeatIntake = existing.RequiredIntake;
                    item.PresentSeatIntake = existing.AvailableIntake;
                }
            }

            return View(intakeDetails);
        }


        [HttpPost]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public async Task<IActionResult> TeachingFacultyDetails(List<TeachingFacultyViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // Defensive: handle null model early and return with info for debugging
            if (model == null || !model.Any())
            {
                // Log the request form keys to help diagnose binding issues
                var form = HttpContext.Request.HasFormContentType ? HttpContext.Request.Form : null;
                var formKeys = form != null ? string.Join(", ", form.Keys) : "no form or not form content-type";

                // Example: use your logging mechanism; here we use TempData for visibility in UI + Console for server logs
                TempData["Error"] = "No data received from form. Form keys: " + formKeys;
                Console.WriteLine($"TeachingFacultyDetails POST received null/empty model. CollegeCode={collegeCode}. FormKeys={formKeys}");

                // Redirect back to GET so user sees the form again (or choose to return View with model)
                return RedirectToAction("TeachingFacultyDetails");
            }

            foreach (var item in model)
            {
                var existingRecord = await _context.CollegeDesignationDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.DepartmentCode == item.DepartmentCode &&
                        x.DesignationCode == item.DesignationCode &&
                        x.SeatSlabId == item.SeatSlabId);

                if (existingRecord != null)
                {
                    existingRecord.FacultyCode = item.FacultyCode;
                    existingRecord.Designation = item.DesignationName;
                    existingRecord.Department = item.DepartmentName;
                    existingRecord.RequiredIntake = item.ExistingSeatIntake;
                    existingRecord.AvailableIntake = item.PresentSeatIntake ?? "0";

                    _context.CollegeDesignationDetails.Update(existingRecord);
                }
                else
                {
                    var newRecord = new CollegeDesignationDetail
                    {
                        FacultyCode = item.FacultyCode,
                        CollegeCode = collegeCode!,
                        Designation = item.DesignationName,
                        DesignationCode = item.DesignationCode,
                        Department = item.DepartmentName,
                        DepartmentCode = item.DepartmentCode,
                        SeatSlabId = item.SeatSlabId,
                        RequiredIntake = item.ExistingSeatIntake,
                        AvailableIntake = item.PresentSeatIntake ?? "0"
                    };

                    _context.CollegeDesignationDetails.Add(newRecord);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("TeachingFacultyDetails");
        }
        [HttpGet]
        public IActionResult CollegeFacultyReport()
        {
            var vm = new IntakeReportViewModel();
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Convert Faculty entities to SelectListItem with Value/Text properties
            vm.FacultyList = _context.Faculties
                .Where(e => e.FacultyId.ToString() == facultyCode)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName,
                    Selected = !string.IsNullOrEmpty(facultyCode) &&
                              string.Equals(f.FacultyId.ToString(), facultyCode, StringComparison.OrdinalIgnoreCase)
                })
                .OrderBy(f => f.Text)
                .ToList();

            if (!string.IsNullOrEmpty(facultyCode))
            {
                vm.SelectedFaculty = facultyCode;
            }

            // Leave CollegeList empty for AJAX loading
            vm.CollegeList = new List<SelectListItem>();

            return View(vm);
        }

        [HttpGet]
        public IActionResult GetCollegesByFaculty(string facultyCode)
        {
            if (string.IsNullOrWhiteSpace(facultyCode))
                return Json(Array.Empty<object>());

            // AffiliationCollegeMaster.FacultyCode is string in your model; compare as string.
            var colleges = _context.AffiliationCollegeMasters
                .OrderBy(e => e.CollegeName)
                .Where(d => d.FacultyCode == facultyCode)
                .Select(d => new
                {
                    CollegeCode = d.CollegeCode,
                    // use real name if column exists; otherwise fall back to code
                    CollegeName = (d.GetType().GetProperty("CollegeName") != null)
                        ? EF.Property<string>(d, "CollegeName")
                        : d.CollegeCode
                })
                .Distinct()
                .OrderBy(x => x.CollegeCode)
                .ToList();

            return Json(colleges);
        }

        // POST: Load DataTable Report
        [HttpPost]
        [ValidateAntiForgeryToken]  // Add back after client token fix
        public IActionResult LoadCollegeReport(string facultyCode, string collegeCode)
        {
            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return Json(new List<CollegeReportDto>());  // Empty array for DataTables

            try
            {
                var data = _context.CollegeDesignationDetails
                    .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                    .Select(x => new CollegeReportDto
                    {
                        Id = x.Id,
                        Designation = x.Designation,
                        Department = x.Department,
                        RequiredIntake = x.RequiredIntake,
                        AvailableIntake = x.AvailableIntake
                    })
                    .ToList();

                return Json(data);  // Direct array, matches dataSrc: ''
            }
            catch (Exception ex)
            {
                // Log ex (use ILogger)
                return Json(new List<CollegeReportDto>());  // Graceful empty on error
            }
        }

    }
}