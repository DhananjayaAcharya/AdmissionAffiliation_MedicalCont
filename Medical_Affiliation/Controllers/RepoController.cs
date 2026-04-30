//using Medical_Affiliation.DATA;
//using Medical_Affiliation.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//namespace Medical_Affiliation.Controllers
//{
//    public class RepoController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public RepoController(ApplicationDbContext context)
//        {
//            _context = context;
//        }


//        [HttpGet]
//        public IActionResult Repo_BasicDetails()
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(facultyCodeString))
//                return RedirectToAction("Login", "Account");

//            var nc = _context.NursingCollegeRegistrations
//                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

//            var ni = _context.MedicalInstituteDetails
//                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

//            var viewModel = new NursingInstituteDetailViewModel
//            {
//                CollegeCode = ni?.CollegeCode,
//                FacultyCode = ni?.FacultyCode,
//                InstituteName = ni?.InstituteName ?? HttpContext.Session.GetString("CollegeName"),
//                InstituteAddress = ni?.InstituteAddress,
//                TrustSocietyName = ni?.TrustSocietyName,
//                YearOfEstablishmentOfTrust = DateOnly.TryParse(ni?.YearOfEstablishmentOfTrust, out var y1)
//                                                ? y1 : (DateOnly?)null,
//                YearOfEstablishmentOfCollege = DateOnly.TryParse(ni?.YearOfEstablishmentOfCollege, out var y2)
//                                                ? y2 : (DateOnly?)null,
//                InstitutionType = ni?.InstitutionType,
//                HodofInstitution = ni?.HodofInstitution,
//                Dob = ni?.Dob,
//                Age = ni?.Age,
//                TeachingExperience = ni?.TeachingExperience,
//                Degree = (!string.IsNullOrEmpty(ni?.PgDegree))
//                    ? ni.PgDegree.Split(',').Select(d => d.Trim()).ToList()
//                    : new List<string>(),
//                CourseCode = ni?.Course,
//                CourseSelectedSpecialities = ni?.SelectedSpecialities,
//                OtherDegreeText = ni?.OtherDegree,
//                TrustDocData = ni?.TrustDoc,
//                EstablishmentDocData = ni?.EshtablishmentDoc,

//                // ── District & Taluk (edit mode restore) ──
//                SelectedDistrictId = ni?.District,
//                SelectedTalukId = ni?.Taluk,

//                // ── District Dropdown ──
//                DistrictDropdownList = _context.DistrictMasters
//                    .Where(d => d.DistrictName != null && d.DistrictName != "")
//                    .OrderBy(d => d.DistrictName)
//                    .Select(d => new SelectListItem
//                    {
//                        Value = d.DistrictName,
//                        Text = d.DistrictName
//                    }).ToList(),

//                // ── Taluk Dropdown (full list; AJAX filters by district) ──
//                TalukDropdownList = _context.TalukMasters
//                    .OrderBy(t => t.TalukName)
//                    .Select(t => new SelectListItem
//                    {
//                        Value = t.TalukId.ToString(),
//                        Text = t.TalukName
//                    }).ToList(),
//            };

//            if (viewModel.CollegeCode == null && viewModel.FacultyCode == null)
//            {
//                viewModel = new NursingInstituteDetailViewModel
//                {
//                    CollegeCode = collegeCode,
//                    FacultyCode = facultyCodeString,
//                    InstituteName = HttpContext.Session.GetString("CollegeName"),
//                    Degree = new List<string>(),

//                    // ── District Dropdown for fresh form ──
//                    DistrictDropdownList = _context.DistrictMasters
//                        .Where(d => d.DistrictName != null && d.DistrictName != "")
//                        .OrderBy(d => d.DistrictName)
//                        .Select(d => new SelectListItem
//                        {
//                            Value = d.DistrictName,
//                            Text = d.DistrictName
//                        }).ToList(),

//                    TalukDropdownList = _context.TalukMasters
//                        .OrderBy(t => t.TalukName)
//                        .Select(t => new SelectListItem
//                        {
//                            Value = t.TalukId.ToString(),
//                            Text = t.TalukName
//                        }).ToList(),
//                };
//            }

//            ViewBag.Courses = _context.MstCourses
//                .Where(c => c.FacultyCode.ToString() == facultyCodeString)
//                .OrderBy(c => c.CourseName)
//                .ToList();

//            return View(viewModel);
//        }

//        [HttpGet]
//        public IActionResult GetTaluksByDistrict(string districtName)
//        {
//            if (string.IsNullOrEmpty(districtName))
//                return Json(new List<object>());

//            try
//            {
//                // Step 1: Check what DistrictMasters has for this name
//                var district = _context.DistrictMasters
//                    .Where(d => d.DistrictName == districtName)
//                    .Select(d => new { d.DistrictId, d.DistrictName })
//                    .FirstOrDefault();

//                if (district == null)
//                    return Json(new { error = $"No district found with name '{districtName}'" });

//                // Step 2: Check TalukMasters using that DistrictId
//                var taluks = _context.TalukMasters
//                    .Where(t => t.DistrictId == district.DistrictId)  // join via int FK
//                    .OrderBy(t => t.TalukName)
//                    .Select(t => new
//                    {
//                        value = t.TalukId.ToString(),
//                        text = t.TalukName
//                    })
//                    .ToList();

//                return Json(taluks);
//            }
//            catch (Exception ex)
//            {
//                return Json(new
//                {
//                    error = ex.Message,
//                    inner = ex.InnerException?.Message
//                });
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Repo_BasicDetails(NursingInstituteDetailViewModel model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(facultyCodeString))
//                return RedirectToAction("Login", "Account");

//            var existingEntity = _context.MedicalInstituteDetails
//                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

//            if (existingEntity != null)
//            {
//                // ── Basic Info ──
//                existingEntity.InstituteAddress = model.InstituteAddress;
//                existingEntity.TrustSocietyName = model.TrustSocietyName;
//                existingEntity.YearOfEstablishmentOfTrust = model.YearOfEstablishmentOfTrust?.ToString();
//                existingEntity.YearOfEstablishmentOfCollege = model.YearOfEstablishmentOfCollege?.ToString();
//                existingEntity.InstitutionType = model.InstitutionType;
//                existingEntity.HodofInstitution = model.HodofInstitution;
//                existingEntity.Dob = model.Dob;
//                existingEntity.Age = model.Age;
//                existingEntity.TeachingExperience = model.TeachingExperience;
//                existingEntity.PgDegree = model.Degree != null ? string.Join(",", model.Degree) : "";
//                existingEntity.SelectedSpecialities = model.CourseSelectedSpecialities;
//                existingEntity.Course = model.CourseCode;
//                existingEntity.OtherDegree = model.OtherDegreeText;

//                // ── District & Taluk ──
//                existingEntity.District = model.SelectedDistrictId;
//                existingEntity.Taluk = model.SelectedTalukId;

//                // ── Trust Document ──
//                if (model.TrustEstablishmentDocument != null)
//                {
//                    using (var ms = new MemoryStream())
//                    {
//                        model.TrustEstablishmentDocument.CopyTo(ms);
//                        existingEntity.TrustDoc = ms.ToArray();
//                    }
//                }

//                // ── Establishment Document ──
//                if (model.CollegeEstablishmentDocument != null)
//                {
//                    using (var ms = new MemoryStream())
//                    {
//                        model.CollegeEstablishmentDocument.CopyTo(ms);
//                        existingEntity.EshtablishmentDoc = ms.ToArray();
//                    }
//                }

//                _context.Update(existingEntity);
//            }
//            else
//            {
//                var entity = new MedicalInstituteDetail
//                {
//                    CollegeCode = collegeCode,
//                    FacultyCode = facultyCodeString,
//                    InstituteName = model.InstituteName,
//                    InstituteAddress = model.InstituteAddress,
//                    TrustSocietyName = model.TrustSocietyName,
//                    YearOfEstablishmentOfTrust = model.YearOfEstablishmentOfTrust?.ToString(),
//                    YearOfEstablishmentOfCollege = model.YearOfEstablishmentOfCollege?.ToString(),
//                    InstitutionType = model.InstitutionType,
//                    HodofInstitution = model.HodofInstitution,
//                    Dob = model.Dob,
//                    Age = model.Age,
//                    TeachingExperience = model.TeachingExperience,
//                    PgDegree = model.Degree != null ? string.Join(",", model.Degree) : "",
//                    SelectedSpecialities = model.CourseSelectedSpecialities,
//                    Course = model.CourseCode,
//                    OtherDegree = model.OtherDegreeText,

//                    // ── District & Taluk ──
//                    District = model.SelectedDistrictId,
//                    Taluk = model.SelectedTalukId,
//                };

//                // ── Trust Document ──
//                if (model.TrustEstablishmentDocument != null)
//                {
//                    using (var ms = new MemoryStream())
//                    {
//                        model.TrustEstablishmentDocument.CopyTo(ms);
//                        entity.TrustDoc = ms.ToArray();
//                    }
//                }

//                // ── Establishment Document ──
//                if (model.CollegeEstablishmentDocument != null)
//                {
//                    using (var ms = new MemoryStream())
//                    {
//                        model.CollegeEstablishmentDocument.CopyTo(ms);
//                        entity.EshtablishmentDoc = ms.ToArray();
//                    }
//                }

//                _context.Add(entity);
//            }

//            _context.SaveChanges();

//            return RedirectToAction("Repo_UGPG");
//        }

//        [HttpGet]
//        public IActionResult Repo_Institute_Report()
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");
//            var model = _context.NursingInstituteDetails
//                .FirstOrDefault(x => x.FacultyCode == facultyCode);

//            if (model == null)
//                return RedirectToAction("Nursing_BasicDetails");

//            return View(model);
//        }

//        public IActionResult PreviousStep()
//        {
//            // logic to go back to previous page/step
//            return RedirectToAction("Dashboard");
//        }

//        public IActionResult NextStep()
//        {
//            // logic to go forward to next page/step
//            return RedirectToAction("Repo_BasicDetails");
//        }


//        [HttpGet]
//        public async Task<IActionResult> Repo_UGPG()
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
//            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(facultyCodeStr) || !int.TryParse(facultyCodeStr, out int facultyCode))
//            {
//                TempData["ErrorMessage"] = "FacultyCode not found in session or invalid.";
//                return RedirectToAction("Error"); // or return the page with the error message
//            }

//            // Add "Additional" to dropdown options
//            ViewBag.FreshIncreaseOptions = new[] { "Fresh", "Increase", "Additional" };

//            // Get distinct CourseLevels for this faculty
//            var levels = await _context.MstCourses
//                .Where(c => c.FacultyCode == facultyCode && !string.IsNullOrEmpty(c.CourseLevel))
//                .Select(c => c.CourseLevel.Trim())
//                .Distinct()
//                .OrderBy(l => l)
//                .ToListAsync();

//            ViewBag.CourseLevels = levels;

//            // ✅ Fetch records filtered by CollegeCode
//            var repositoryRows = await _context.UgandPgrepositories
//                .Where(x => x.CollegeCode == collegeCode)     // Filter data by logged-in CollegeCode
//                .OrderByDescending(x => x.Id)
//                .Take(1000)
//                .ToListAsync();

//            ViewBag.RepositoryRows = repositoryRows;

//            return View(new FacultyIntakeViewModel());
//        }

//        public async Task<IActionResult> DownloadDoc(int id, string type)
//        {
//            var row = await _context.UgandPgrepositories.FindAsync(id);
//            if (row == null) return NotFound();

//            byte[] fileBytes = null;
//            string fileName = $"{type}_{id}.pdf"; // can infer real name and mime type if needed
//            string contentType = "application/pdf"; // adjust mime type if needed

//            switch (type)
//            {
//                case "RGUHS": fileBytes = row.Rguhsnotification; break;
//                case "INC": fileBytes = row.Inc; break;
//                case "KNMC": fileBytes = row.Knmc; break;
//                case "GOK": fileBytes = row.Gok; break;
//            }
//            if (fileBytes == null) return NotFound("File not found");

//            return File(fileBytes, contentType, fileName);
//        }

//        // AJAX: return courses for a given level
//        [HttpGet]
//        public async Task<IActionResult> GetCoursesByLevel(string level)
//        {
//            if (string.IsNullOrWhiteSpace(level))
//                return Json(new { success = false, data = new object[0], message = "Level not provided" });

//            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");
//            if (string.IsNullOrEmpty(facultyCodeStr) || !int.TryParse(facultyCodeStr, out int facultyCode))
//                return Json(new { success = false, data = new object[0], message = "Invalid FacultyCode in session" });

//            var courses = await _context.MstCourses
//                .Where(c => c.CourseLevel != null &&
//                            c.CourseLevel.Trim().ToUpper() == level.Trim().ToUpper() &&
//                            c.FacultyCode == facultyCode)
//                .Select(c => new { c.CourseCode, c.CourseName })
//                .OrderBy(c => c.CourseName)
//                .ToListAsync();

//            return Json(new { success = true, data = courses });
//        }
//        [HttpGet]
//        public async Task<IActionResult> IsCourseFresh(string courseName)
//        {
//            var exists = await _context.UgandPgrepositories
//                .AnyAsync(r => r.CourseName == courseName && r.FreshOrIncrease == "Fresh");
//            return Json(new { exists });
//        }

//        [HttpPost]
//        public async Task<IActionResult> DeleteFacultyIntake(int id)
//        {
//            var row = await _context.UgandPgrepositories.FindAsync(id);
//            if (row == null)
//                return Json(new { success = false, message = "Record not found." });

//            _context.UgandPgrepositories.Remove(row);
//            await _context.SaveChangesAsync();
//            return Json(new { success = true });
//        }

//        // POST: FacultyIntake/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Repo_UGPG(FacultyIntakeViewModel model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            // If validation error, reload dropdowns
//            if (!ModelState.IsValid)
//            {
//                ViewBag.CourseLevels = await _context.MstCourses
//                    .Where(c => !string.IsNullOrEmpty(c.CourseLevel))
//                    .Select(c => c.CourseLevel.Trim())
//                    .Distinct()
//                    .OrderBy(l => l)
//                    .ToListAsync();

//                ViewBag.FreshIncreaseOptions = new[] { "Fresh", "Increase", "Additional" };
//                // Also reload table for view
//                ViewBag.RepositoryRows = await _context.UgandPgrepositories
//                    .OrderByDescending(x => x.Id)
//                    .Take(1000)
//                    .ToListAsync();

//                return View(model);
//            }

//            // Find the selected course name for display/save
//            var selectedCourseName = await _context.MstCourses
//                .Where(c => c.CourseCode.ToString() == model.CourseCode)
//                .Select(c => c.CourseName)
//                .FirstOrDefaultAsync();

//            var entity = new UgandPgrepository
//            {
//                IntakeDetails = model.IntakeDetails,         // textbox value
//                FreshOrIncrease = model.FreshOrIncrease,     // dropdown value
//                Course = model.CourseCode,                   // CourseCode from dropdown
//                CollegeCode = collegeCode,                   // session college code
//                FacultyCode = facultyCode,                   // session faculty code
//                CourseName = selectedCourseName,             // store CourseName
//            };

//            // Helper to convert IFormFile to byte[]
//            async Task<byte[]> ToBytes(IFormFile file)
//            {
//                if (file == null || file.Length == 0) return null;
//                using var ms = new MemoryStream();
//                await file.CopyToAsync(ms);
//                return ms.ToArray();
//            }

//            entity.Rguhsnotification = await ToBytes(model.RGUHSNotificationFile);
//            entity.Inc = await ToBytes(model.INCUploadFile);
//            entity.Knmc = await ToBytes(model.KNMCUploadFile);
//            entity.Gok = await ToBytes(model.GOKUploadFile);

//            _context.UgandPgrepositories.Add(entity);
//            await _context.SaveChangesAsync();

//            //TempData["SuccessMessage"] = "Faculty intake saved successfully.";
//            return RedirectToAction(nameof(Repo_UGPG));
//        }

//        [HttpGet]
//        public IActionResult Repo_ManageDesignationIntake()
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            // Get all designations from master table
//            var designationList = _context.DesignationMasters
//                .Where(e => e.FacultyCode.ToString() == facultyCode)
//                .Select(d => new CollegeDesignationDetailsViewModel
//                {
//                    DesignationCode = d.DesignationCode,
//                    Designation = d.DesignationName,

//                }).ToList();

//            // Optionally fetch already saved intake to pre-fill (if updating)
//            var savedIntake = _context.CollegeDesignationDetails
//                .Where(c => c.CollegeCode == collegeCode && c.FacultyCode == facultyCode)
//                .ToList();

//            foreach (var item in designationList)
//            {
//                var exist = savedIntake.FirstOrDefault(x => x.DesignationCode == item.DesignationCode);

//                if (exist != null && int.TryParse(exist.AvailableIntake, out int intake))
//                    item.PresentIntake = intake;
//                else
//                    item.PresentIntake = 0;
//            }
//            return View(designationList);
//        }

//        [HttpPost]
//        public IActionResult Repo_ManageDesignationIntake(List<CollegeDesignationDetailsViewModel> model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            foreach (var item in model)
//            {
//                var existing = _context.CollegeDesignationDetails.FirstOrDefault(x =>
//                    x.CollegeCode == collegeCode && x.FacultyCode == facultyCode &&
//                    x.DesignationCode == item.DesignationCode);

//                string requiredIntakeValue = item.PresentIntake.ToString() ?? "0";

//                if (existing != null)
//                {
//                    existing.AvailableIntake = item.PresentIntake.ToString();
//                    existing.RequiredIntake = requiredIntakeValue;
//                    _context.Update(existing);
//                }
//                else
//                {
//                    _context.CollegeDesignationDetails.Add(new CollegeDesignationDetail
//                    {
//                        CollegeCode = collegeCode,
//                        FacultyCode = facultyCode,
//                        DesignationCode = item.DesignationCode,
//                        Designation = item.Designation,
//                        AvailableIntake = item.PresentIntake.ToString(),
//                        RequiredIntake = requiredIntakeValue,

//                    });
//                }
//            }


//            _context.SaveChanges();
//            return RedirectToAction("Repo_FacultyDetails");
//        }


//        [HttpGet]
//        public IActionResult Repo_Affiliated_YearwiseMaterials()
//        {
//            try
//            {
//                var collegeCode = HttpContext.Session.GetString("CollegeCode");
//                var facultyCode = HttpContext.Session.GetString("FacultyCode");

//                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
//                    return RedirectToAction("Login", "Account");

//                // Normalize comparisons: some DB FacultyCode may be int; compare by string to avoid mismatches
//                var model = (from param in _context.MstNursingAffiliatedMaterialData
//                             where param.FacultyCode.ToString() == facultyCode
//                             join yw in _context.NursingAffiliatedYearwiseMaterialsData
//                                .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
//                                on param.ParametersId equals yw.ParametersId into ywGroup
//                             from ywItem in ywGroup.DefaultIfEmpty()
//                             select new Nursing_YearwiseMaterialsDataViewModel
//                             {
//                                 ParametersId = param.ParametersId,
//                                 ParametersName = param.ParametersName,
//                                 CollegeCode = collegeCode,
//                                 FacultyCode = facultyCode,
//                                 Year1 = ywItem != null ? (ywItem.Year1 ?? "0") : "0",
//                                 Year2 = ywItem != null ? (ywItem.Year2 ?? "0") : "0",
//                                 Year3 = ywItem != null ? (ywItem.Year3 ?? "0") : "0",
//                                 ParentHospitalName = ywItem != null ? ywItem.ParentHospitalName : null,
//                                 ParentHospitalAddress = ywItem != null ? ywItem.ParentHospitalAddress : null
//                             }).ToList();

//                // Provide saved rows with Id and document presence flags so view can render "View" links
//                // 1. Get saved data
//                var savedRows = _context.NursingAffiliatedYearwiseMaterialsData
//                    .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
//                    .Join(_context.MstNursingAffiliatedMaterialData,
//                          y => y.ParametersId,
//                          p => p.ParametersId,
//                          (y, p) => new
//                          {
//                              y.Id,
//                              y.ParametersId,
//                              ParametersName = p.ParametersName,
//                              y.Year1,
//                              y.Year2,
//                              y.Year3,
//                              y.ParentHospitalName,
//                              y.ParentHospitalAddress,
//                              y.Kpmebeds,
//                              y.PostBasicBeds,
//                              y.TotalBeds,
//                              y.HospitalOwnerName,
//                              y.HospitalType,
//                              ParentHospitalMoudocPresent = y.ParentHospitalMoudoc != null,
//                              ParentHospitalOwnerNameDocPresent = y.ParentHospitalOwnerNameDoc != null,
//                              ParentHospitalKpmebedsDocPresent = y.ParentHospitalKpmebedsDoc != null,
//                              ParentHospitalPostBasicDocPresent = y.ParentHospitalPostBasicDoc != null
//                          })
//                    .ToList();


//                // 2️⃣ Unique parameters (guaranteed non-null)
//                var parametersList = savedRows.Select(x => x.ParametersName).Distinct().ToList();

//                // 3️⃣ Pivot data: group by hospital (Id), each hospital has a dictionary of parameters → Year1/2/3
//                var pivotData = savedRows
//                    .GroupBy(x => new { x.HospitalType, x.ParentHospitalName })
//                    .Select(g => new YearwisePivotRecord
//                    {
//                        HospitalType = g.Key.HospitalType,
//                        ParentHospitalName = g.Key.ParentHospitalName,
//                        ParentHospitalAddress = g.First().ParentHospitalAddress,
//                        Kpmebeds = g.First().Kpmebeds,
//                        PostBasicBeds = g.First().PostBasicBeds,
//                        TotalBeds = g.First().TotalBeds,
//                        HospitalOwnerName = g.First().HospitalOwnerName,
//                        ParentHospitalMoudocPresent = g.First().ParentHospitalMoudocPresent,
//                        ParentHospitalOwnerNameDocPresent = g.First().ParentHospitalOwnerNameDocPresent,
//                        ParentHospitalKpmebedsDocPresent = g.First().ParentHospitalKpmebedsDocPresent,
//                        ParentHospitalPostBasicDocPresent = g.First().ParentHospitalPostBasicDocPresent,

//                        Parameters = g.GroupBy(r => r.ParametersName)
//                                      .ToDictionary(
//                                          grp => grp.Key,
//                                          grp => new YearwiseValues
//                                          {
//                                              ParametersId = grp.First().ParametersId,
//                                              Year1 = grp.First().Year1,
//                                              Year2 = grp.First().Year2,
//                                              Year3 = grp.First().Year3
//                                          })
//                    })
//                    .ToList();



//                // 4️⃣ Pass to ViewBag (guaranteed non-null)
//                ViewBag.ParametersList = parametersList ?? new List<string>();
//                ViewBag.PivotData = pivotData;
//                ViewBag.YearwiseData = savedRows;
//                ViewBag.YearwiseDataCount = savedRows.Count;

//                var hospitalTypes = new List<SelectListItem>
//                    {
//                        new SelectListItem { Value = "ParentHospital", Text = "Parent Hospital" },
//                        new SelectListItem { Value = "AffiliatedHospital", Text = "Affiliated Hospital" }
//                    };

//                // Check if ParentHospital already exists
//                bool parentHospitalExists = _context.NursingAffiliatedYearwiseMaterialsData
//                    .Any(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.HospitalType == "ParentHospital");

//                if (parentHospitalExists)
//                {
//                    var parentItem = hospitalTypes.FirstOrDefault(ht => ht.Value == "ParentHospital");
//                    if (parentItem != null) parentItem.Disabled = true;
//                }

//                ViewBag.HospitalTypes = hospitalTypes;


//                // Debug log for quick verification (remove in production)
//                Console.WriteLine($"Repo_Affiliated_YearwiseMaterials GET: parameters={model.Count}, savedRows={savedRows.Count}, college={collegeCode}, faculty={facultyCode}");

//                return View(model);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                TempData["Error"] = "Error loading data!";
//                return View(new List<Nursing_YearwiseMaterialsDataViewModel>());
//            }
//        }
//        public IActionResult DownloadDocument(int id, string type)
//        {
//            // Fetch the record by id
//            var record = _context.NursingAffiliatedYearwiseMaterialsData.FirstOrDefault(x => x.ParametersId == id);
//            if (record == null) return NotFound();

//            byte[] fileBytes = null;
//            string fileName = "document.pdf";

//            // Choose the document based on type
//            switch (type)
//            {
//                case "MOU":
//                    fileBytes = record.ParentHospitalMoudoc;
//                    fileName = "MOU.pdf";
//                    break;
//                case "Owner":
//                    fileBytes = record.ParentHospitalOwnerNameDoc;
//                    fileName = "Owner.pdf";
//                    break;
//                case "KPME":
//                    fileBytes = record.ParentHospitalKpmebedsDoc;
//                    fileName = "KPME.pdf";
//                    break;
//                case "PostBasic":
//                    fileBytes = record.ParentHospitalPostBasicDoc;
//                    fileName = "PostBasic.pdf";
//                    break;
//                default:
//                    return BadRequest();
//            }

//            if (fileBytes == null || fileBytes.Length == 0)
//                return NotFound();

//            return File(fileBytes, "application/pdf", fileName);
//        }

//        [HttpPost]
//        public IActionResult DeleteYearwiseMaterial1(int id)
//        {
//            try
//            {
//                var collegeCode = HttpContext.Session.GetString("CollegeCode");
//                var facultyCode = HttpContext.Session.GetString("FacultyCode");

//                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
//                    return RedirectToAction("Login", "Account");

//                // 1️⃣ Fetch the selected record by its Id
//                var selectedRecord = _context.NursingAffiliatedYearwiseMaterialsData
//                    .FirstOrDefault(x => x.Id == id);

//                if (selectedRecord == null)
//                {
//                    TempData["Error"] = "Record not found!";
//                    return RedirectToAction("Repo_Affiliated_YearwiseMaterials");
//                }

//                // Extract group keys for deletion
//                var hospitalType = selectedRecord.HospitalType;
//                var hospitalName = selectedRecord.ParentHospitalName;

//                // 2️⃣ Delete ALL records for this hospital (same college, hospital type, and name)
//                var recordsToDelete = _context.NursingAffiliatedYearwiseMaterialsData
//                    .Where(x =>
//                        x.CollegeCode == collegeCode &&
//                        x.FacultyCode == facultyCode &&
//                        x.HospitalType == hospitalType &&
//                        x.ParentHospitalName == hospitalName
//                    )
//                    .ToList();

//                if (recordsToDelete.Any())
//                {
//                    _context.NursingAffiliatedYearwiseMaterialsData.RemoveRange(recordsToDelete);
//                    _context.SaveChanges();
//                    TempData["Success"] = "Hospital records deleted successfully!";
//                }
//                else
//                {
//                    TempData["Error"] = "No records found for this hospital!";
//                }
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = "Error deleting record: " + ex.Message;
//            }

//            return RedirectToAction("Repo_Affiliated_YearwiseMaterials");
//        }

//        [HttpPost]
//        public IActionResult Repo_Affiliated_YearwiseMaterials(List<YearwiseMaterialViewModel> model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (model == null || model.Count == 0)
//            {
//                TempData["Error"] = "No data to save.";
//                return RedirectToAction("Repo_Affiliated_YearwiseMaterials");
//            }

//            var form = HttpContext.Request.Form;

//            // Read hospital fields from form, including HospitalType dropdown
//            string parentHospitalName = form["ParentHospitalName"];
//            string parentHospitalAddress = form["ParentHospitalAddress"];
//            string hospitalOwnerName = form["HospitalOwnerName"];
//            string hospitalType = form["HospitalType"]; // <-- read selected HospitalType here

//            int.TryParse(form["Kpmebeds"], out int kpmeBeds);
//            int.TryParse(form["PostBasicBeds"], out int postBasicBeds);
//            int.TryParse(form["TotalBeds"], out int totalBeds);

//            var files = HttpContext.Request.Form.Files;
//            byte[] kpmeBedsDoc = files["ParentHospitalKpmebedsDocFile"] != null ? ConvertFileToBytes(files["ParentHospitalKpmebedsDocFile"]) : null;
//            byte[] mouDoc = files["ParentHospitalMoudocFile"] != null ? ConvertFileToBytes(files["ParentHospitalMoudocFile"]) : null;
//            byte[] ownerDoc = files["ParentHospitalOwnerNameDocFile"] != null ? ConvertFileToBytes(files["ParentHospitalOwnerNameDocFile"]) : null;
//            byte[] postBasicDoc = files["ParentHospitalPostBasicDocFile"] != null ? ConvertFileToBytes(files["ParentHospitalPostBasicDocFile"]) : null;

//            if (hospitalType == "AffiliatedHospital")
//            {
//                var existingHospitalTypes = _context.NursingAffiliatedYearwiseMaterialsData
//                    .Where(x => x.HospitalType.StartsWith("AffiliatedHospital"))
//                    .Select(x => x.HospitalType)
//                    .ToList();

//                int nextIndex = 0;
//                if (existingHospitalTypes.Any())
//                {
//                    var numbers = existingHospitalTypes
//                        .Select(ht => ht.Substring("AffiliatedHospital".Length))
//                        .Select(sfx => int.TryParse(sfx, out int n) ? n : 0)
//                        .ToList();

//                    nextIndex = numbers.Max() + 1;
//                }

//                hospitalType = nextIndex == 0 ? "AffiliatedHospital1" : "AffiliatedHospital" + nextIndex;
//            }

//            foreach (var item in model)
//            {
//                var parametersName = string.IsNullOrEmpty(item.ParametersName)
//                    ? _context.ClinicalMaterialData.Where(p => p.ParametersId == item.ParametersId).Select(p => p.ParametersName).FirstOrDefault()
//                    : item.ParametersName;

//                var existingRecord = _context.NursingAffiliatedYearwiseMaterialsData
//                    .FirstOrDefault(y => y.CollegeCode == collegeCode
//                                      && y.FacultyCode == facultyCode
//                                      && y.ParametersId == item.ParametersId
//                                      && y.ParentHospitalName == parentHospitalName);

//                if (existingRecord != null)
//                {
//                    existingRecord.Year1 = item.Year1;
//                    existingRecord.Year2 = item.Year2;
//                    existingRecord.Year3 = item.Year3;
//                    existingRecord.ParametersName = parametersName;

//                    existingRecord.ParentHospitalName = parentHospitalName;
//                    existingRecord.ParentHospitalAddress = parentHospitalAddress;
//                    existingRecord.HospitalOwnerName = hospitalOwnerName;
//                    existingRecord.HospitalType = hospitalType; // <-- assign hospital type here
//                    existingRecord.Kpmebeds = kpmeBeds.ToString();
//                    existingRecord.PostBasicBeds = postBasicBeds.ToString();
//                    existingRecord.TotalBeds = totalBeds.ToString();

//                    if (kpmeBedsDoc != null) existingRecord.ParentHospitalKpmebedsDoc = kpmeBedsDoc;
//                    if (mouDoc != null) existingRecord.ParentHospitalMoudoc = mouDoc;
//                    if (ownerDoc != null) existingRecord.ParentHospitalOwnerNameDoc = ownerDoc;
//                    if (postBasicDoc != null) existingRecord.ParentHospitalPostBasicDoc = postBasicDoc;
//                }
//                else
//                {
//                    var newRecord = new NursingAffiliatedYearwiseMaterialsDatum
//                    {
//                        CollegeCode = collegeCode,
//                        FacultyCode = facultyCode,
//                        ParametersId = item.ParametersId,
//                        ParametersName = parametersName,

//                        Year1 = item.Year1,
//                        Year2 = item.Year2,
//                        Year3 = item.Year3,

//                        ParentHospitalName = parentHospitalName,
//                        ParentHospitalAddress = parentHospitalAddress,
//                        HospitalOwnerName = hospitalOwnerName,
//                        HospitalType = hospitalType, // <-- assign hospital type here
//                        Kpmebeds = kpmeBeds.ToString(),
//                        PostBasicBeds = postBasicBeds.ToString(),
//                        TotalBeds = totalBeds.ToString(),

//                        ParentHospitalKpmebedsDoc = kpmeBedsDoc,
//                        ParentHospitalMoudoc = mouDoc,
//                        ParentHospitalOwnerNameDoc = ownerDoc,
//                        ParentHospitalPostBasicDoc = postBasicDoc
//                    };

//                    _context.NursingAffiliatedYearwiseMaterialsData.Add(newRecord);
//                }
//            }

//            _context.SaveChanges();
//            TempData["Success"] = "Data saved successfully!";
//            return RedirectToAction("Repo_Affiliated_YearwiseMaterials");
//        }
//        private byte[] ConvertFileToBytes(IFormFile formFile)
//        {
//            using (var ms = new MemoryStream())
//            {
//                formFile.CopyTo(ms);
//                return ms.ToArray();
//            }
//        }

//        [HttpGet]
//        public IActionResult ManageDesignationIntake()
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            // Get all designations from master table
//            var designationList = _context.DesignationMasters
//                .Where(e => e.FacultyCode.ToString() == facultyCode)
//                .Select(d => new CollegeDesignationDetailsViewModel
//                {
//                    DesignationCode = d.DesignationCode,
//                    Designation = d.DesignationName,

//                }).ToList();

//            // Optionally fetch already saved intake to pre-fill (if updating)
//            var savedIntake = _context.CollegeDesignationDetails
//                .Where(c => c.CollegeCode == collegeCode && c.FacultyCode == facultyCode)
//                .ToList();

//            foreach (var item in designationList)
//            {
//                var exist = savedIntake.FirstOrDefault(x => x.DesignationCode == item.DesignationCode);

//                if (exist != null && int.TryParse(exist.AvailableIntake, out int intake))
//                    item.PresentIntake = intake;
//                else
//                    item.PresentIntake = 0;
//            }
//            return View(designationList);
//        }

//        [HttpPost]
//        public IActionResult ManageDesignationIntake(List<CollegeDesignationDetailsViewModel> model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            foreach (var item in model)
//            {
//                var existing = _context.CollegeDesignationDetails.FirstOrDefault(x =>
//                    x.CollegeCode == collegeCode && x.FacultyCode == facultyCode &&
//                    x.DesignationCode == item.DesignationCode);

//                string requiredIntakeValue = item.PresentIntake.ToString() ?? "0";

//                if (existing != null)
//                {
//                    existing.AvailableIntake = item.PresentIntake.ToString();
//                    existing.RequiredIntake = requiredIntakeValue;
//                    _context.Update(existing);
//                }
//                else
//                {
//                    _context.CollegeDesignationDetails.Add(new CollegeDesignationDetail
//                    {
//                        CollegeCode = collegeCode,
//                        FacultyCode = facultyCode,
//                        DesignationCode = item.DesignationCode,
//                        Designation = item.Designation,
//                        AvailableIntake = item.PresentIntake.ToString(),
//                        RequiredIntake = requiredIntakeValue,

//                    });
//                }
//            }


//            _context.SaveChanges();
//            return RedirectToAction("Repo_FacultyDetails");
//        }


//        //  HELPER — shared dropdown population (DRY)
//        // ──────────────────────────────────────────────────────────
//        private (List<SelectListItem> subjects,
//                 List<SelectListItem> designations,
//                 List<SelectListItem> departments)
//            GetDropdowns(string facultyCode)
//        {
//            var subjects = _context.MstCourses
//                .Where(c => c.FacultyCode.ToString() == facultyCode)
//                .Select(c => new SelectListItem
//                {
//                    Value = c.CourseCode.ToString(),
//                    Text = c.CourseName ?? ""
//                })
//                .Distinct()
//                .ToList();

//            var designations = _context.DesignationMasters
//                .Where(d => d.FacultyCode.ToString() == facultyCode)
//                .Select(d => new SelectListItem
//                {
//                    Value = d.DesignationCode,
//                    Text = d.DesignationName ?? ""
//                })
//                .ToList();

//            var departments = _context.MstCourses
//                .Where(e => e.FacultyCode.ToString() == facultyCode)
//                .Select(d => new SelectListItem
//                {
//                    Value = d.CourseCode.ToString(),
//                    Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
//                })
//                .ToList();

//            return (subjects, designations, departments);
//        }

//        // ──────────────────────────────────────────────────────────
//        //  GET
//        // ──────────────────────────────────────────────────────────
//        [HttpGet]
//        public IActionResult Repo_FacultyDetails()
//        {
//            string collegeCode = HttpContext.Session.GetString("CollegeCode");
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please log in again.";
//                return RedirectToAction("Login", "Account");
//            }

//            var (subjectsList, designationsList, departmentsList) = GetDropdowns(facultyCode);

//            // ✅ FIX 1: Exclude already-removed records
//            var facultyDetails = _context.FacultyDetails
//                .Where(f => f.CollegeCode == collegeCode
//                         && f.FacultyCode == facultyCode
//                         && f.IsRemoved != true)
//                .ToList();

//            var ahsFacultyWithCollege = _context.NursingFacultyWithColleges
//                .Where(f => f.CollegeCode == collegeCode
//                         && f.FacultyCode.ToString() == facultyCode)
//                .ToList();

//            var vmList = new List<FacultyDetailsViewModel>();

//            if (!facultyDetails.Any() && !ahsFacultyWithCollege.Any())
//            {
//                TempData["Info"] = "No faculty records found for this faculty.";
//                vmList.Add(new FacultyDetailsViewModel
//                {
//                    Subjects = subjectsList,
//                    Designations = designationsList,
//                    DepartmentDetails = departmentsList
//                });
//                return View(vmList);
//            }

//            // ✅ FIX 2: Join existing DB records with college data
//            vmList = (from f1 in facultyDetails
//                      join f2 in ahsFacultyWithCollege
//                          on new { f1.Aadhaar, f1.Pan, f1.Designation }
//                          equals new
//                          {
//                              Aadhaar = f2.AadhaarNumber,
//                              Pan = f2.Pannumber,
//                              Designation = f2.Designation
//                          }
//                          into gj
//                      from sub in gj.DefaultIfEmpty()
//                      select new FacultyDetailsViewModel
//                      {
//                          FacultyDetailId = f1.Id,           // ✅ Always carry DB Id
//                          NameOfFaculty = sub?.TeachingFacultyName ?? f1.NameOfFaculty,
//                          Designation = sub?.Designation ?? f1.Designation,
//                          Aadhaar = sub?.AadhaarNumber ?? f1.Aadhaar,
//                          PAN = sub?.Pannumber ?? f1.Pan,
//                          DepartmentDetail = f1.DepartmentDetails,
//                          SelectedDepartment = f1.DepartmentDetails,
//                          RecognizedPGTeacher = f1.RecognizedPgTeacher,
//                          Mobile = f1.Mobile,
//                          Email = f1.Email,
//                          Subjects = subjectsList,
//                          Designations = designationsList,
//                          DepartmentDetails = departmentsList,
//                          RecognizedPhDTeacher = f1.RecognizedPhDteacher,
//                          LitigationPending = f1.LitigationPending,
//                          PhDRecognitionDocData = f1.PhDrecognitionDoc,
//                          LitigationDocData = f1.LitigationDoc,
//                          PGRecognitionDocData = f1.GuideRecognitionDoc,
//                          IsExaminer = f1.IsExaminer,
//                          ExaminerFor = f1.ExaminerFor,
//                          ExaminerForList = !string.IsNullOrEmpty(f1.ExaminerFor)
//                                                    ? f1.ExaminerFor.Split(',').ToList()
//                                                    : new List<string>(),
//                          RemoveRemarks = f1.RemoveRemarks
//                      }).ToList();

//            // ✅ FIX 3: For missingFaculty — try to find existing DB Id by Aadhaar+PAN
//            var missingFaculty = ahsFacultyWithCollege
//                .Where(f2 => !vmList.Any(v =>
//                        v.Aadhaar == f2.AadhaarNumber &&
//                        v.PAN == f2.Pannumber))
//                .Select(f2 =>
//                {
//                    // Look up DB record by Aadhaar + PAN to get the Id
//                    var dbRecord = facultyDetails.FirstOrDefault(f =>
//                        f.Aadhaar == f2.AadhaarNumber &&
//                        f.Pan == f2.Pannumber);

//                    return new FacultyDetailsViewModel
//                    {
//                        FacultyDetailId = dbRecord?.Id ?? 0,   // ✅ Carry Id if found
//                        NameOfFaculty = f2.TeachingFacultyName,
//                        Designation = f2.Designation,
//                        Aadhaar = f2.AadhaarNumber,
//                        PAN = f2.Pannumber,
//                        Subjects = subjectsList,
//                        Designations = designationsList,
//                        DepartmentDetails = departmentsList
//                    };
//                })
//                .ToList();

//            vmList.AddRange(missingFaculty);

//            if (!vmList.Any())
//            {
//                vmList.Add(new FacultyDetailsViewModel
//                {
//                    Subjects = subjectsList,
//                    Designations = designationsList,
//                    DepartmentDetails = departmentsList
//                });
//            }

//            return View(vmList);
//        }

//        // ──────────────────────────────────────────────────────────
//        //  POST
//        // ──────────────────────────────────────────────────────────
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Repo_FacultyDetails(IList<FacultyDetailsViewModel> model)
//        {
//            string collegeCode = HttpContext.Session.GetString("CollegeCode");
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please log in again.";
//                return RedirectToAction("Login", "Account");
//            }

//            if (model == null || !model.Any())
//            {
//                return RedirectToAction("Repo_ExamResults");
//            }

//            var activeRows = model.Where(m => string.IsNullOrWhiteSpace(m.RemoveRemarks)).ToList();
//            var removedRows = model.Where(m => !string.IsNullOrWhiteSpace(m.RemoveRemarks)).ToList();

//            using var transaction = _context.Database.BeginTransaction();
//            try
//            {
//                var existingFaculty = _context.FacultyDetails
//                    .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
//                    .ToList();

//                foreach (var m in removedRows)
//                {
//                    var existing = FindExistingRecord(existingFaculty, m);
//                    if (existing != null)
//                    {
//                        existing.IsRemoved = true;
//                        existing.RemoveRemarks = m.RemoveRemarks.Trim();
//                        _context.FacultyDetails.Update(existing);
//                    }
//                }

//                foreach (var m in activeRows)
//                {
//                    string name = m.NameOfFaculty?.Trim() ?? "";
//                    string designation = m.Designation?.Trim() ?? "";
//                    string mobile = m.Mobile?.Trim() ?? "";
//                    string email = m.Email?.Trim() ?? "";
//                    string pan = m.PAN?.Trim() ?? "";
//                    string aadhaar = m.Aadhaar?.Trim() ?? "";
//                    string dept = m.SelectedDepartment?.Trim() ?? "";
//                    string recognizedPG = m.RecognizedPGTeacher?.Trim() ?? "";
//                    string recognizedPhD = m.RecognizedPhDTeacher?.Trim() ?? "";
//                    string litigation = m.LitigationPending?.Trim() ?? "";

//                    byte[] guideDocBytes = m.GuideRecognitionDoc != null ? ConvertFileToBytes(m.GuideRecognitionDoc) : null;
//                    byte[] phdDocBytes = m.PhDRecognitionDoc != null ? ConvertFileToBytes(m.PhDRecognitionDoc) : null;
//                    byte[] litigDocBytes = m.LitigationDoc != null ? ConvertFileToBytes(m.LitigationDoc) : null;

//                    string examinerFor = m.ExaminerForList != null && m.ExaminerForList.Any()
//                                            ? string.Join(",", m.ExaminerForList)
//                                            : null;

//                    var existing = FindExistingRecord(existingFaculty, m);

//                    if (existing != null)
//                    {
//                        existing.NameOfFaculty = name;
//                        existing.Designation = designation;
//                        existing.RecognizedPgTeacher = recognizedPG;
//                        existing.Mobile = mobile;
//                        existing.Email = email;
//                        existing.Pan = pan;
//                        existing.Aadhaar = aadhaar;
//                        existing.DepartmentDetails = dept;
//                        existing.RecognizedPhDteacher = recognizedPhD;
//                        existing.LitigationPending = litigation;
//                        existing.IsExaminer = m.IsExaminer;
//                        existing.ExaminerFor = examinerFor;
//                        existing.IsRemoved = false;
//                        existing.RemoveRemarks = null;

//                        if (guideDocBytes != null) existing.GuideRecognitionDoc = guideDocBytes;
//                        if (phdDocBytes != null) existing.PhDrecognitionDoc = phdDocBytes;
//                        if (litigDocBytes != null) existing.LitigationDoc = litigDocBytes;

//                        _context.FacultyDetails.Update(existing);
//                    }
//                    else
//                    {
//                        var faculty = new FacultyDetail
//                        {
//                            CollegeCode = collegeCode,
//                            FacultyCode = facultyCode,
//                            NameOfFaculty = name,
//                            Designation = designation,
//                            RecognizedPgTeacher = recognizedPG,
//                            RecognizedPhDteacher = recognizedPhD,
//                            LitigationPending = litigation,
//                            Mobile = mobile,
//                            Email = email,
//                            Pan = pan,
//                            Aadhaar = aadhaar,
//                            DepartmentDetails = dept,
//                            GuideRecognitionDoc = guideDocBytes,
//                            PhDrecognitionDoc = phdDocBytes,
//                            LitigationDoc = litigDocBytes,
//                            IsExaminer = m.IsExaminer,
//                            ExaminerFor = examinerFor,
//                            IsRemoved = false,
//                            RemoveRemarks = null,
//                            Subject = "N/A",
//                        };
//                        _context.FacultyDetails.Add(faculty);
//                    }
//                }

//                _context.SaveChanges();
//                transaction.Commit();

//                return RedirectToAction("Repo_ExamResults");
//            }
//            catch (Exception ex)
//            {
//                transaction.Rollback();
//                TempData["Error"] = "Error saving faculty records: " + ex.Message;
//                return RedirectToAction(nameof(Repo_FacultyDetails));
//            }
//        }

//        private FacultyDetail FindExistingRecord(List<FacultyDetail> existingFaculty, FacultyDetailsViewModel m)
//        {
//            if (m.FacultyDetailId > 0)
//                return existingFaculty.FirstOrDefault(f => f.Id == m.FacultyDetailId);

//            string aadhaar = m.Aadhaar?.Trim();
//            string pan = m.PAN?.Trim();

//            if (!string.IsNullOrWhiteSpace(aadhaar) && !string.IsNullOrWhiteSpace(pan))
//                return existingFaculty.FirstOrDefault(f =>
//                    !string.IsNullOrWhiteSpace(f.Aadhaar) &&
//                    !string.IsNullOrWhiteSpace(f.Pan) &&
//                    f.Aadhaar.Trim() == aadhaar &&
//                    f.Pan.Trim() == pan);

//            return null;
//        }

//        public IActionResult ViewFacultyDocument(int id, string type, string mode = "view")
//        {
//            var faculty = _context.FacultyDetails.FirstOrDefault(f => f.Id == id);
//            if (faculty == null)
//                return NotFound();

//            byte[] fileBytes = null;
//            string fileName = $"{type}_document.pdf";

//            switch (type.ToLower())
//            {
//                case "pg":
//                    fileBytes = faculty.GuideRecognitionDoc;
//                    break;

//                case "phd":
//                    fileBytes = faculty.PhDrecognitionDoc;
//                    break;

//                case "litig":
//                    fileBytes = faculty.LitigationDoc;
//                    break;

//                default:
//                    return BadRequest("Invalid document type.");
//            }

//            if (fileBytes == null)
//                return NotFound("Document not uploaded.");

//            if (mode == "download")
//            {
//                // 📥 FORCE DOWNLOAD
//                return File(fileBytes, "application/octet-stream", fileName);
//            }

//            // 👀 VIEW IN BROWSER
//            return File(fileBytes, "application/pdf");
//        }


//        [HttpGet]
//        public async Task<IActionResult> Repo_ExamResults()
//        {
//            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");

//            // ✅ Handle missing session
//            if (string.IsNullOrEmpty(facultyCodeStr) || string.IsNullOrEmpty(collegeCode))
//            {
//                TempData["ErrorMessage"] = "Session expired! Please log in again.";
//                return RedirectToAction("Login", "Account");
//            }

//            if (!int.TryParse(facultyCodeStr, out int facultyCode))
//            {
//                TempData["ErrorMessage"] = "Invalid Faculty Code!";
//                return RedirectToAction("Login", "Account");
//            }

//            // ✅ Base model with all possible rows
//            var model = new List<NursingExamResultViewModel>
//            {
//                new() { Course = "B.Sc", Year = "1st Year" },
//                new() { Course = "B.Sc", Year = "2nd Year" },
//                new() { Course = "B.Sc", Year = "3rd Year" },
//                new() { Course = "B.Sc", Year = "4th Year" },
//                new() { Course = "P.B.B.Sc", Year = "1st Year" },
//                new() { Course = "P.B.B.Sc", Year = "2nd Year" },
//                new() { Course = "M.Sc", Year = "1st Year" },
//                new() { Course = "M.Sc", Year = "2nd Year" }
//            };

//            // ✅ Get saved records from DB
//            var savedResults = await _context.FacultyExamResults
//                .Where(x => x.Facultycode == facultyCode.ToString() && x.Collegecode == collegeCode)
//                .ToListAsync();

//            // ✅ Merge DB values into default list
//            foreach (var item in model)
//            {
//                int year = 0;
//                int.TryParse(new string(item.Year.Where(char.IsDigit).ToArray()), out year);

//                // Find match in DB (same course + same year)
//                var existing = savedResults.FirstOrDefault(r =>
//                    r.Course.Equals(item.Course, StringComparison.OrdinalIgnoreCase) &&
//                    r.Year == year);

//                if (existing != null)
//                {
//                    item.ExamAppearedCount = existing.ExamappearedCount.Value;
//                    item.PassedOutCount = existing.Passedoutcount.Value;
//                    item.YearOfPercentage = existing.Yearofpercentage.ToString();
//                }
//            }

//            return View(model);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Repo_ExamResults(List<NursingExamResultViewModel> model)
//        {
//            try
//            {
//                var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");
//                var collegeCode = HttpContext.Session.GetString("CollegeCode");

//                // ✅ Validate session and model
//                if (model == null || !model.Any())
//                {
//                    TempData["ErrorMessage"] = "No data submitted!";
//                    return View(model ?? new List<NursingExamResultViewModel>());
//                }

//                if (string.IsNullOrEmpty(facultyCodeStr) || string.IsNullOrEmpty(collegeCode))
//                {
//                    TempData["ErrorMessage"] = "Session expired! Please log in again.";
//                    return RedirectToAction("Login", "Account");
//                }

//                if (!int.TryParse(facultyCodeStr, out int facultyCode))
//                {
//                    TempData["ErrorMessage"] = "Invalid Faculty Code!";
//                    return View(model);
//                }

//                // ✅ Make sure DbSet is not null
//                if (_context.FacultyExamResults == null)
//                {
//                    TempData["ErrorMessage"] = "Database context not initialized properly!";
//                    return View(model);
//                }

//                // ✅ Remove old records safely
//                var oldRecords = await _context.FacultyExamResults
//                    .Where(x => x.Facultycode == facultyCode.ToString() && x.Collegecode == collegeCode)
//                    .ToListAsync();

//                if (oldRecords?.Any() == true)
//                {
//                    _context.FacultyExamResults.RemoveRange(oldRecords);
//                    await _context.SaveChangesAsync();
//                }

//                // ✅ Prepare new data safely
//                var newRecords = model
//                .Where(item => !string.IsNullOrWhiteSpace(item.Course) && !string.IsNullOrWhiteSpace(item.Year))
//                .Select(item =>
//                {
//                    int.TryParse(new string(item.Year.Where(char.IsDigit).ToArray()), out int yr);

//                    return new FacultyExamResult
//                    {
//                        Facultycode = facultyCode.ToString(),
//                        Collegecode = collegeCode,
//                        Course = item.Course?.Trim(),
//                        Year = yr,
//                        ExamappearedCount = item.ExamAppearedCount,
//                        Passedoutcount = item.PassedOutCount,
//                        Yearofpercentage = !string.IsNullOrWhiteSpace(item.YearOfPercentage)
//                            ? decimal.TryParse(item.YearOfPercentage, out var percent) ? percent : 0
//                            : 0
//                    };
//                })
//                .ToList();


//                // ✅ Prevent AddRangeAsync from null source
//                if (newRecords.Any())
//                {
//                    await _context.FacultyExamResults.AddRangeAsync(newRecords);
//                    await _context.SaveChangesAsync();

//                    TempData["SuccessMessage"] = "🌟 Success! Your records have been updated successfully.";
//                }
//                else
//                {
//                    TempData["ErrorMessage"] = "No valid records found to save!";
//                }

//                return RedirectToAction(nameof(Repo_ExamResults));
//            }
//            catch (Exception ex)
//            {
//                TempData["ErrorMessage"] = $"An error occurred while saving data: {ex.Message}";
//                return View(model ?? new List<NursingExamResultViewModel>());
//            }
//        }

//    }
//}
