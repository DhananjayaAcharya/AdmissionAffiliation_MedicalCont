using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Medical_Affiliation.Models.MedicalViewModels;

namespace Medical_Affiliation.Controllers
{
    public class MedicalRepositoryController : Controller
    {
        private readonly ApplicationDbContext _context;


        public MedicalRepositoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Medical_BasicDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(facultyCodeString))
                return RedirectToAction("Login", "Account");

            var existingEntity = _context.MedicalInstituteDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

            var model = new MedicalInstituteDetailViewModel
            {
                InstituteName = HttpContext.Session.GetString("CollegeName"),
                FacultyCode = facultyCodeString,

                // ── PG Courses ──
                PGCourses = _context.MstCourses
                    .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                    .OrderBy(c => c.CourseName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseCode.ToString(),
                        Text = c.CourseName
                    }).ToList(),

                // ── Degree List ──
                DegreeList = _context.MstCourses
                    .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                    .Select(c => c.CourseLevel)
                    .Distinct()
                    .OrderBy(level => level)
                    .Select(level => new SelectListItem
                    {
                        Value = level.ToString(),
                        Text = level.ToString()
                    }).ToList(),

                // ── Specialization List ──
                SpecializationList = _context.MstCourses
                    .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                    .OrderBy(c => c.CourseName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CourseCode.ToString(),
                        Text = c.CourseName
                    }).ToList(),

                // ── Districts from DistrictMasters ──
                // Value = DistrictName, Text = DistrictName
                DistrictDropdownList = _context.DistrictMasters
                    .Where(d => d.DistrictName != null && d.DistrictName != "")
                    .OrderBy(d => d.DistrictName)
                    .Select(d => new SelectListItem
                    {
                        Value = d.DistrictName,
                        Text = d.DistrictName
                    })
                    .ToList(),

                // ── All Taluks from TalukMasters ──
                // AJAX will filter by DistrictName on district selection
                TalukDropdownList = _context.TalukMasters
                    .OrderBy(t => t.TalukName)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TalukId.ToString(),
                        Text = t.TalukName
                    }).ToList(),
            };

            // ── Populate model from existing saved record (Edit Mode) ──
            if (existingEntity != null)
            {
                model.TrustSocietyName = existingEntity.TrustSocietyName;
                model.YearOfEstablishmentOfTrust = existingEntity.YearOfEstablishmentOfTrust;
                model.YearOfEstablishmentOfCollege = existingEntity.YearOfEstablishmentOfCollege;
                model.InstitutionType = existingEntity.InstitutionType;
                model.HODOfInstitution = existingEntity.HodofInstitution;
                model.DOB = (DateOnly)existingEntity.Dob;
                model.Age = existingEntity.Age;
                model.TeachingExperience = existingEntity.TeachingExperience;
                model.SelectedPGCourseId = int.TryParse(existingEntity.PgDegree, out int pgId) ? pgId : 0;
                model.SelectedQualification = existingEntity.SelectedSpecialities;
                model.InstituteAddress = existingEntity.InstituteAddress;
                model.SelectedDegree = existingEntity.Course;
                model.SelectedSpecialization = existingEntity.Specialisation;
                model.SelectedDistrictId = existingEntity.District;
                model.SelectedTalukId = existingEntity.Taluk;
            }

            return View(model);
        }

        // ── AJAX Endpoint: returns taluks filtered by DistrictName ──
        [HttpGet]
        public IActionResult GetTaluksByDistrict(string districtName)
        {
            if (string.IsNullOrEmpty(districtName))
                return Json(new List<object>());

            try
            {
                // Step 1: Check what DistrictMasters has for this name
                var district = _context.DistrictMasters
                    .Where(d => d.DistrictName == districtName)
                    .Select(d => new { d.DistrictId, d.DistrictName })
                    .FirstOrDefault();

                if (district == null)
                    return Json(new { error = $"No district found with name '{districtName}'" });

                // Step 2: Check TalukMasters using that DistrictId
                var taluks = _context.TalukMasters
                    .Where(t => t.DistrictId == district.DistrictId)  // join via int FK
                    .OrderBy(t => t.TalukName)
                    .Select(t => new
                    {
                        value = t.TalukId.ToString(),
                        text = t.TalukName
                    })
                    .ToList();

                return Json(taluks);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_BasicDetails(MedicalInstituteDetailViewModel model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(facultyCodeString))
                return RedirectToAction("Login", "Account");

            var existingEntity = _context.MedicalInstituteDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

            int calculatedAge = 0;
            if (model.DOB != default)
            {
                var today = DateTime.Today;
                var dobDateTime = model.DOB.ToDateTime(TimeOnly.MinValue);
                calculatedAge = today.Year - dobDateTime.Year;
                if (dobDateTime > today.AddYears(-calculatedAge))
                    calculatedAge--;
            }

            if (existingEntity != null)
            {
                existingEntity.TrustSocietyName = model.TrustSocietyName;
                existingEntity.YearOfEstablishmentOfTrust = model.YearOfEstablishmentOfTrust;
                existingEntity.YearOfEstablishmentOfCollege = model.YearOfEstablishmentOfCollege;
                existingEntity.InstitutionType = model.InstitutionType;
                existingEntity.HodofInstitution = model.HODOfInstitution;
                existingEntity.Dob = model.DOB;
                existingEntity.Age = calculatedAge.ToString();
                existingEntity.TeachingExperience = model.TeachingExperience;
                existingEntity.PgDegree = model.SelectedPGCourseId.ToString();
                existingEntity.SelectedSpecialities = model.SelectedQualification;
                existingEntity.InstituteAddress = model.InstituteAddress;
                existingEntity.Course = model.SelectedDegree;
                existingEntity.Specialisation = model.SelectedSpecialization;
                existingEntity.District = model.SelectedDistrictId;  // save district
                existingEntity.Taluk = model.SelectedTalukId;        // save taluk

                _context.Update(existingEntity);
            }
            else
            {
                var newEntity = new MedicalInstituteDetail
                {
                    CollegeCode = collegeCode ?? "0",
                    FacultyCode = facultyCodeString,
                    InstituteName = HttpContext.Session.GetString("CollegeName"),
                    TrustSocietyName = model.TrustSocietyName,
                    YearOfEstablishmentOfTrust = model.YearOfEstablishmentOfTrust,
                    YearOfEstablishmentOfCollege = model.YearOfEstablishmentOfCollege,
                    InstitutionType = model.InstitutionType,
                    HodofInstitution = model.HODOfInstitution,
                    Dob = model.DOB,
                    Age = calculatedAge.ToString(),
                    TeachingExperience = model.TeachingExperience,
                    PgDegree = model.SelectedPGCourseId.ToString(),
                    SelectedSpecialities = model.SelectedQualification,
                    InstituteAddress = model.InstituteAddress,
                    Course = model.SelectedDegree,
                    Specialisation = model.SelectedSpecialization,
                    District = model.SelectedDistrictId,   // save district
                    Taluk = model.SelectedTalukId,         // save taluk
                };

                _context.Add(newEntity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MedicalUgAndPgDetail");
        }

        public IActionResult PreviousStep()
        {
            // logic to go back to previous page/step
            return RedirectToAction("Dashboard");
        }

        public IActionResult NextStep()
        {
            // logic to go forward to next page/step
            return RedirectToAction("Medical_BasicDetails");
        }


        // Helper method to get dropdown data
        private (List<SelectListItem> subjects, List<SelectListItem> designations, List<SelectListItem> departments) GetDropdownLists(string facultyCode)
        {
            var subjectsList = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName ?? ""
                })
                .Distinct()
                .ToList();

            //var designationsList = _context.DesignationMasters
            //    .Select(d => new SelectListItem
            //    {
            //        Value = d.DesignationCode,
            //        Text = d.DesignationName ?? ""
            //    })
            //    .ToList();

            var designationsList = _context.DesignationMasters
            .Where(c => c.FacultyCode.ToString() == facultyCode)
            .Select(d => new SelectListItem
            {
                Value = d.DesignationCode,
                Text = d.DesignationName ?? ""
            })
            .ToList();

            var departmentsList = _context.MstCourses
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.CourseCode.ToString(),
                    Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
                })
                .Distinct()
                .ToList();

            return (subjectsList, designationsList, departmentsList);
        }

        [HttpGet]
        public IActionResult FacultyDetails()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var subjectsList = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName ?? ""
                })
                .Distinct()
                .ToList();

            var designationsList = _context.DesignationMasters
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.DesignationCode,
                    Text = d.DesignationName ?? ""
                })
                .ToList();

            var departmentsList = _context.MstCourses
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.CourseCode.ToString(),
                    Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
                })
                .ToList();

            var facultyDetails = _context.FacultyDetails
                .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
                .ToList();

            var ahsFacultyWithCollege = _context.NursingFacultyWithColleges // Change table name as appropriate
                .Where(f => f.CollegeCode == collegeCode && f.FacultyCode.ToString() == facultyCode)
                .ToList();

            List<FacultyDetailsViewModel> vmList = new List<FacultyDetailsViewModel>();

            if (!facultyDetails.Any() && !ahsFacultyWithCollege.Any())
            {
                TempData["Info"] = "No faculty records found for this faculty.";
                vmList.Add(new FacultyDetailsViewModel
                {
                    Subjects = subjectsList,
                    Designations = designationsList,
                    DepartmentDetails = departmentsList
                });
                return View(vmList);
            }

            // Join existing faculty details with college faculty data
            vmList = (from f1 in facultyDetails
                      join f2 in ahsFacultyWithCollege
                          on new { f1.Aadhaar, f1.Pan, f1.Designation }
                          equals new { Aadhaar = f2.AadhaarNumber, Pan = f2.Pannumber, Designation = f2.Designation }
                          into gj
                      from sub in gj.DefaultIfEmpty()
                      select new FacultyDetailsViewModel
                      {
                          FacultyDetailId = f1.Id,
                          NameOfFaculty = sub?.TeachingFacultyName ?? f1.NameOfFaculty,
                          Designation = sub?.Designation ?? f1.Designation,
                          Aadhaar = sub?.AadhaarNumber ?? f1.Aadhaar,
                          PAN = sub?.Pannumber ?? f1.Pan,
                          DepartmentDetail = f1.DepartmentDetails,
                          SelectedDepartment = f1.DepartmentDetails,
                          Subject = f1.Subject,
                          RecognizedPGTeacher = f1.RecognizedPgTeacher,
                          Mobile = f1.Mobile,
                          Email = f1.Email,
                          Subjects = subjectsList,
                          Designations = designationsList,
                          DepartmentDetails = departmentsList
                      }).ToList();

            // Add missing faculty from college data
            var missingFaculty = ahsFacultyWithCollege
                .Where(f2 => !vmList.Any(v => v.Aadhaar == f2.AadhaarNumber && v.PAN == f2.Pannumber))
                .Select(f2 => new FacultyDetailsViewModel
                {
                    NameOfFaculty = f2.TeachingFacultyName,
                    Designation = f2.Designation,
                    Aadhaar = f2.AadhaarNumber,
                    PAN = f2.Pannumber,
                    Subjects = subjectsList,
                    Designations = designationsList,
                    DepartmentDetails = departmentsList
                })
                .ToList();

            vmList.AddRange(missingFaculty);

            if (!vmList.Any())
            {
                vmList.Add(new FacultyDetailsViewModel
                {
                    Subjects = subjectsList,
                    Designations = designationsList,
                    DepartmentDetails = departmentsList
                });
            }

            return View(vmList);
        }
        private async Task<string?> SaveFacultyFileAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = @"D:\Affiliation_Medical\FacultyDetails";
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
        public async Task<IActionResult> FacultyDetails(IList<FacultyDetailsViewModel> model)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            // ✅ Session validation
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Empty list validation
            if (model == null || !model.Any())
            {
                TempData["Error"] = "No data to save.";

                // Repopulate dropdown lists before returning to view
                model = new List<FacultyDetailsViewModel>
                {
                     new FacultyDetailsViewModel
                     {
                         Subjects = _context.MstCourses
                             .Where(c => c.FacultyCode.ToString() == facultyCode)
                             .Select(c => new SelectListItem { Value = c.CourseCode.ToString(), Text = c.CourseName ?? "" })
                             .Distinct()
                             .ToList(),
                         Designations = _context.DesignationMasters
                             .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? "" })
                             .ToList(),
                         DepartmentDetails = _context.MstCourses
                             .Where(e => e.FacultyCode.ToString() == facultyCode)
                             .Select(d => new SelectListItem
                             {
                                 Value = d.CourseCode.ToString(),
                                 Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
                             })
                             .ToList()
                     }
                };

                return View(model);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Extract all IDs coming from the frontend
                    var incomingIds = model
                        .Where(m => m.FacultyDetailId > 0) // only valid existing IDs
                        .Select(m => m.FacultyDetailId)
                        .ToHashSet();

                    // Get all existing faculty for this college/faculty code
                    var existingFaculty = _context.FacultyDetails
                        .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
                        .ToList();

                    // 🔹 1. DELETE records that are NOT in the incoming model
                    var toDelete = existingFaculty
                        .Where(f => !incomingIds.Contains(f.Id))
                        .ToList();

                    if (toDelete.Any())
                    {
                        _context.FacultyDetails.RemoveRange(toDelete);
                    }

                    // 🔹 2. ADD or UPDATE incoming data
                    foreach (var m in model)
                    {
                        string name = m.NameOfFaculty?.Trim() ?? "N/A";
                        string designation = m.Designation?.Trim() ?? "N/A";
                        string subject = m.Subject?.Trim() ?? "N/A";
                        string mobile = string.IsNullOrWhiteSpace(m.Mobile) ? "N/A" : m.Mobile.Trim();
                        string email = string.IsNullOrWhiteSpace(m.Email) ? "N/A" : m.Email.Trim();
                        string pan = m.PAN?.Trim() ?? "N/A";
                        string aadhaar = m.Aadhaar?.Trim() ?? "N/A";
                        string dept = m.SelectedDepartment?.Trim() ?? "N/A";
                        string recognizedPG = m.RecognizedPGTeacher?.Trim() ?? "N/A";

                        string guidePath = null;

                        if (m.GuideRecognitionDoc != null && m.GuideRecognitionDoc.Length > 0)
                        {
                            guidePath = await SaveFacultyFileAsync(m.GuideRecognitionDoc, "GuideDocs");
                        }

                        var existing = existingFaculty.FirstOrDefault(f => f.Id == m.FacultyDetailId);

                        if (existing != null)
                        {
                            // ✅ Update
                            existing.NameOfFaculty = name;
                            existing.Designation = designation;
                            existing.RecognizedPgTeacher = recognizedPG;
                            existing.Mobile = mobile;
                            existing.Email = email;
                            existing.Pan = pan;
                            existing.Aadhaar = aadhaar;
                            existing.DepartmentDetails = dept;
                            existing.Subject = subject;

                            if (guidePath != null)
                            {
                                if (!string.IsNullOrEmpty(existing.GuideRecognitionDocPath) &&
                                    System.IO.File.Exists(existing.GuideRecognitionDocPath))
                                {
                                    System.IO.File.Delete(existing.GuideRecognitionDocPath);
                                }

                                existing.GuideRecognitionDocPath = guidePath;
                            }

                            _context.FacultyDetails.Update(existing);
                        }
                        else
                        {
                            // ✅ Insert new
                            var faculty = new FacultyDetail
                            {
                                CollegeCode = collegeCode,
                                FacultyCode = facultyCode,
                                NameOfFaculty = name,
                                Subject = subject,
                                Designation = designation,
                                RecognizedPgTeacher = recognizedPG,
                                Mobile = mobile,
                                Email = email,
                                Pan = pan,
                                Aadhaar = aadhaar,
                                DepartmentDetails = dept,
                                GuideRecognitionDocPath = guidePath
                            };

                            _context.FacultyDetails.Add(faculty);
                        }
                    }

                    // 🔹 3. Save all changes once
                    _context.SaveChanges();

                    transaction.Commit();

                    TempData["Success"] = "Faculty records saved successfully!";
                    return RedirectToAction("TeachingFacultyDetails");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // ✅ Log and show detailed error
                    TempData["Error"] = "Error saving faculty records: " + ex.Message;

                    // ✅ Repopulate dropdowns again so View doesn’t break
                    foreach (var m in model)
                    {
                        m.Subjects = _context.MstCourses
                            .Where(c => c.FacultyCode.ToString() == facultyCode)
                            .Select(c => new SelectListItem { Value = c.CourseCode.ToString(), Text = c.CourseName ?? "" })
                            .Distinct()
                            .ToList();

                        m.Designations = _context.DesignationMasters
                            .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? "" })
                            .ToList();

                        m.DepartmentDetails = _context.MstCourses
                            .Where(e => e.FacultyCode.ToString() == facultyCode)
                            .Select(d => new SelectListItem
                            {
                                Value = d.CourseCode.ToString(),
                                Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
                            })
                            .ToList();
                    }

                    return View(model);
                }
            }
        }



        [HttpGet]
        public async Task<IActionResult> TeachingFacultyDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var mbbsIntake = await _context.CollegeCourseIntakeDetails
                .Where(x => x.CollegeCode == collegeCode
                            && x.FacultyCode.ToString() == facultyCode
                            && x.CourseName == "MBBS") // CourseName filter
                .Select(x => x.ExistingIntake)
                .FirstOrDefaultAsync();

            ViewBag.UGMBBSIntake = mbbsIntake?.ToString() ?? "0";

            // 1. Get MBBS intake (for M001, CourseCode = 1017)
            var mbbsSeats = await _context.CollegeCourseIntakeDetails
                .Where(e => e.CourseCode == "1017" && e.CollegeCode == collegeCode)
                .FirstOrDefaultAsync();

            if (mbbsSeats == null)
                return View(new List<TeachingFacultyViewModel>()); // no data case

            // 2. Find corresponding SeatSlabId
            var seatSlabId = await _context.SeatSlabMasters
                .Where(s => s.SeatSlab == mbbsSeats.ExistingIntake) // exact match
                .Select(s => s.SeatSlabId)
                .FirstOrDefaultAsync();

            // 3. Fetch Faculty name separately
            var facultyName = await _context.Faculties
                .Where(e => e.FacultyId == 1)
                .Select(e => e.FacultyName)
                .FirstOrDefaultAsync();

            // 4. Main query with GROUP BY
            var intakeDetails = await (
                from sr in _context.DepartmentWiseFacultyMasters
                join sl in _context.SeatSlabMasters on sr.SeatSlabId equals sl.SeatSlabId
                join dm in _context.DesignationMasters on sr.DesignationCode equals dm.DesignationCode
                join dp in _context.DepartmentMasters on sr.DepartmentCode equals dp.DepartmentCode
                join cw in _context.CollegeCourseIntakeDetails on sl.SeatSlab equals cw.ExistingIntake
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
                    ExistingSeatIntake = g.Key.Seats.ToString()
                }
            ).ToListAsync();

            // 5. Overlay with existing saved data (so updates work)
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
                    // Pre-fill user-entered values
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
        public async Task<IActionResult> Medical_YearwiseMaterials()
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                // form model (parameters + existing values)
                var model = await (from param in _context.MstNursingAffiliatedMaterialData
                                   where param.FacultyCode == facultyCode
                                   join yw in _context.NursingAffiliatedYearwiseMaterialsData
                                       .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
                                       on param.ParametersId equals yw.ParametersId into ywGroup
                                   from ywItem in ywGroup.DefaultIfEmpty()
                                   select new YearwiseDocViewModel
                                   {
                                       Id = ywItem != null ? ywItem.Id : 0,
                                       ParametersId = param.ParametersId,
                                       ParametersName = param.ParametersName,
                                       CollegeCode = collegeCode,
                                       FacultyCode = facultyCode,
                                       Year1 = ywItem != null ? (ywItem.Year1 ?? "0") : "0",
                                       Year2 = ywItem != null ? (ywItem.Year2 ?? "0") : "0",
                                       Year3 = ywItem != null ? (ywItem.Year3 ?? "0") : "0",
                                       ParentHospitalName = ywItem != null ? ywItem.ParentHospitalName : null,
                                       ParentHospitalAddress = ywItem != null ? ywItem.ParentHospitalAddress : null,
                                       HospitalOwnerName = ywItem != null ? ywItem.HospitalOwnerName : null,
                                       HospitalType = ywItem != null ? ywItem.HospitalType : null,
                                       ParentHospitalMoudocPresent = ywItem != null && ywItem.ParentHospitalMoudoc != null,
                                       ParentHospitalKpmebedsDocPresent = ywItem != null && ywItem.ParentHospitalKpmebedsDoc != null,
                                       ParentHospitalOwnerNameDocPresent = ywItem != null && ywItem.ParentHospitalOwnerNameDoc != null,
                                       ParentHospitalPostBasicDocPresent = ywItem != null && ywItem.ParentHospitalPostBasicDoc != null
                                   }).ToListAsync();

                // saved rows: fetch directly and project so view can render
                var savedRows = await _context.NursingAffiliatedYearwiseMaterialsData
                    .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
                    .Select(y => new
                    {
                        y.Id,
                        y.ParametersId,
                        y.ParametersName,
                        Year1 = y.Year1,
                        Year2 = y.Year2,
                        Year3 = y.Year3,
                        parentHospitalName = y.ParentHospitalName,
                        parentHospitalAddress = y.ParentHospitalAddress,
                        KPMEBeds = y.Kpmebeds,
                        PostBasicBeds = y.PostBasicBeds,
                        TotalBeds = y.TotalBeds,
                        HospitalOwnerName = y.HospitalOwnerName,
                        HospitalType = y.HospitalType,
                        ParentHospitalMoudocPresent = y.ParentHospitalMoudoc != null,
                        ParentHospitalOwnerNameDocPresent = y.ParentHospitalOwnerNameDoc != null,
                        ParentHospitalKpmebedsDocPresent = y.ParentHospitalKpmebedsDoc != null,
                        ParentHospitalPostBasicDocPresent = y.ParentHospitalPostBasicDoc != null
                    })
                    .ToListAsync();

                // debug: expose counts and raw data for quick troubleshooting
                ViewBag.YearwiseData = savedRows;
                ViewBag.YearwiseDataCount = savedRows?.Count ?? 0;
                Console.WriteLine($"Medical_YearwiseMaterials GET: parameters={model?.Count ?? 0}, savedRows={ViewBag.YearwiseDataCount}, college={collegeCode}, faculty={facultyCode}");

                ViewBag.HospitalTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "ParentHospital", Text = "Parent Hospital", Selected = true },
                    new SelectListItem { Value = "AffiliatedHospital", Text = "Affiliated Hospital" }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "Error loading data!";
                return View(new List<YearwiseDocViewModel>());
            }
        }

        [HttpPost]
        public IActionResult DeleteYearwiseMaterial1(int parametersId)
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                var record = _context.NursingAffiliatedYearwiseMaterialsData.FirstOrDefault(x =>
                    x.ParametersId == parametersId &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

                if (record != null)
                {
                    _context.NursingAffiliatedYearwiseMaterialsData.Remove(record);
                    _context.SaveChanges();
                    TempData["Success"] = "Record deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Record not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting record: " + ex.Message;
            }

            return RedirectToAction("Medical_YearwiseMaterials");
        }


        private byte[] ConvertFileToBytes(IFormFile formFile)
        {
            using (var ms = new MemoryStream())
            {
                formFile.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Medical_YearwiseMaterials(List<YearwiseMaterialViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (model == null || model.Count == 0)
            {
                TempData["Error"] = "No data to save.";
                return RedirectToAction("Medical_YearwiseMaterials");
            }

            if (!HttpContext.Request.HasFormContentType)
            {
                TempData["Error"] = "Invalid form submission.";
                return RedirectToAction("Medical_YearwiseMaterials");
            }

            var form = HttpContext.Request.Form;
            var parentHospitalName = form["ParentHospitalName"].ToString();
            var parentHospitalAddress = form["ParentHospitalAddress"].ToString();
            var hospitalOwnerName = form["HospitalOwnerName"].ToString();
            var hospitalType = form["HospitalType"].ToString();

            int.TryParse(form["Kpmebeds"].ToString(), out int kpmeBeds);
            int.TryParse(form["PostBasicBeds"].ToString(), out int postBasicBeds);
            int.TryParse(form["TotalBeds"].ToString(), out int totalBeds);

            var files = HttpContext.Request.Form.Files;
            IFormFile kpmeFile = files.FirstOrDefault(f => f.Name == "ParentHospitalKpmebedsDocFile");
            IFormFile mouFile = files.FirstOrDefault(f => f.Name == "ParentHospitalMoudocFile");
            IFormFile ownerFile = files.FirstOrDefault(f => f.Name == "ParentHospitalOwnerNameDocFile");
            IFormFile postBasicFile = files.FirstOrDefault(f => f.Name == "ParentHospitalPostBasicDocFile");

            byte[] kpmeBedsDoc = kpmeFile != null ? ConvertFileToBytes(kpmeFile) : null;
            byte[] mouDoc = mouFile != null ? ConvertFileToBytes(mouFile) : null;
            byte[] ownerDoc = ownerFile != null ? ConvertFileToBytes(ownerFile) : null;
            byte[] postBasicDoc = postBasicFile != null ? ConvertFileToBytes(postBasicFile) : null;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in model)
                {
                    item.Year1 = string.IsNullOrWhiteSpace(item.Year1) ? "0" : item.Year1.Trim();
                    item.Year2 = string.IsNullOrWhiteSpace(item.Year2) ? "0" : item.Year2.Trim();
                    item.Year3 = string.IsNullOrWhiteSpace(item.Year3) ? "0" : item.Year3.Trim();

                    var existing = await _context.NursingAffiliatedYearwiseMaterialsData
                        .FirstOrDefaultAsync(y => y.CollegeCode == collegeCode &&
                                                  y.FacultyCode == facultyCode &&
                                                  y.ParametersId == item.ParametersId);

                    var parametersName = string.IsNullOrWhiteSpace(item.ParametersName)
                        ? _context.MstNursingAffiliatedMaterialData.Where(p => p.ParametersId == item.ParametersId).Select(p => p.ParametersName).FirstOrDefault()
                        : item.ParametersName;

                    if (existing != null)
                    {
                        existing.Year1 = item.Year1;
                        existing.Year2 = item.Year2;
                        existing.Year3 = item.Year3;
                        existing.ParametersName = parametersName;

                        existing.ParentHospitalName = parentHospitalName;
                        existing.ParentHospitalAddress = parentHospitalAddress;
                        existing.HospitalOwnerName = hospitalOwnerName;
                        existing.HospitalType = hospitalType;
                        existing.Kpmebeds = kpmeBeds.ToString();
                        existing.PostBasicBeds = postBasicBeds.ToString();
                        existing.TotalBeds = totalBeds.ToString();

                        if (kpmeBedsDoc != null) existing.ParentHospitalKpmebedsDoc = kpmeBedsDoc;
                        if (mouDoc != null) existing.ParentHospitalMoudoc = mouDoc;
                        if (ownerDoc != null) existing.ParentHospitalOwnerNameDoc = ownerDoc;
                        if (postBasicDoc != null) existing.ParentHospitalPostBasicDoc = postBasicDoc;

                        _context.NursingAffiliatedYearwiseMaterialsData.Update(existing);
                    }
                    else
                    {
                        var newRecord = new NursingAffiliatedYearwiseMaterialsDatum
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            ParametersId = item.ParametersId,
                            ParametersName = parametersName,
                            Year1 = item.Year1,
                            Year2 = item.Year2,
                            Year3 = item.Year3,
                            ParentHospitalName = parentHospitalName,
                            ParentHospitalAddress = parentHospitalAddress,
                            HospitalOwnerName = hospitalOwnerName,
                            HospitalType = hospitalType,
                            Kpmebeds = kpmeBeds.ToString(),
                            PostBasicBeds = postBasicBeds.ToString(),
                            TotalBeds = totalBeds.ToString(),
                            ParentHospitalKpmebedsDoc = kpmeBedsDoc,
                            ParentHospitalMoudoc = mouDoc,
                            ParentHospitalOwnerNameDoc = ownerDoc,
                            ParentHospitalPostBasicDoc = postBasicDoc
                        };

                        _context.NursingAffiliatedYearwiseMaterialsData.Add(newRecord);
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Data saved successfully!";
                return RedirectToAction("Medical_YearwiseMaterials");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine(ex);
                TempData["Error"] = "Error saving data: " + ex.Message;
                return RedirectToAction("Medical_YearwiseMaterials");
            }
        }

        public class UgdetailViewModel
        {
            public IFormFile Rguhsnotification { get; set; }
            public IFormFile Gmc { get; set; }
            public IFormFile Nmc { get; set; }
            public IFormFile KSNC { get; set; }
            public string AffiliationType { get; set; } // fresh or increase

            // Changed to int to match typical select value (course id/code)
            public int CourseId { get; set; }

            public DateOnly FirstLopdate { get; set; }
            public string NumberOfSeats { get; set; }
            public string PermittedYear { get; set; }
            public string RecognizedYear { get; set; }
            public string Courselevel { get; set; }
            public string CourseCode { get; set; }
        }

        [HttpGet]
        public IActionResult MedicalUgAndPgDetail()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // Fetch courses for this faculty (for dropdowns)
            ViewBag.Courses = _context.MstCourses
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .ToList() ?? new List<MstCourse>();

            // Fetch distinct course levels for this faculty (for dropdowns)
            ViewBag.CourseLevels = _context.MstCourses
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(e => e.CourseLevel)
                .Distinct()
                .ToList() ?? new List<string>();

            // Filter Ugdetails by CollegeCode and FacultyCode (current user's data only)
            var ugdetails = _context.Ugdetails
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode)
                .ToList();

            return View(ugdetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  // Security fix
        public async Task<IActionResult> MedicalUgAndPgDetail(UgdetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input. Please correct and try again.";

                // Repopulate ViewBag and return matching List<Ugdetail>
                var facultyCodeForDropdown = HttpContext.Session.GetString("FacultyCode");
                var collegeCodeForTable = HttpContext.Session.GetString("CollegeCode");

                ViewBag.Courses = _context.MstCourses
                    .Where(e => e.FacultyCode.ToString() == facultyCodeForDropdown)
                    .ToList() ?? new List<MstCourse>();

                ViewBag.CourseLevels = _context.MstCourses
                    .Where(e => e.FacultyCode.ToString() == facultyCodeForDropdown)
                    .Select(e => e.CourseLevel)
                    .Distinct()
                    .ToList() ?? new List<string>();

                var ugdetails = _context.Ugdetails
                    .Where(e => e.CollegeCode == collegeCodeForTable && e.FacultyCode == facultyCodeForDropdown)
                    .ToList();
                return View(ugdetails);  // Critical: Matches GET signature
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var ugDetail = new Ugdetail
            {
                Rguhsnotification = await GetBytesFromFileAsync(model.Rguhsnotification),
                Gmc = await GetBytesFromFileAsync(model.Gmc),
                Nmc = await GetBytesFromFileAsync(model.Nmc),
                Ksnc = await GetBytesFromFileAsync(model.KSNC),
                FreshOrIncrease = model.AffiliationType,
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                Course = model.CourseId.ToString(),
                CourseCode = model.CourseCode,
                FirstLopdate = model.FirstLopdate,
                NumberOfSeats = model.NumberOfSeats,
                PermittedYear = model.PermittedYear,
                RecognizedYear = model.RecognizedYear,
                CourseLevel = model.Courselevel
            };

            _context.Ugdetails.Add(ugDetail);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Record saved successfully!";
            return RedirectToAction("MedicalUgAndPgDetail");
        }



        public IActionResult DownloadUgFile(int id, string fileType)
        {
            var ugdetail = _context.Ugdetails.Find(id);
            byte[] fileBytes = null;
            string fileName = fileType + "_" + id + ".pdf";
            string contentType = "application/pdf"; // Or infer type as needed

            if (fileType == "Rguhsnotification")
                fileBytes = ugdetail.Rguhsnotification;
            else if (fileType == "Gmc")
                fileBytes = ugdetail.Gmc;
            else if (fileType == "Nmc")
                fileBytes = ugdetail.Nmc;
            else if (fileType == "Ksnc")
                fileBytes = ugdetail.Ksnc;

            if (fileBytes == null)
                return NotFound();

            return File(fileBytes, contentType, fileName);
        }

        private async Task<byte[]> GetBytesFromFileAsync(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
            return null;
        }

        [HttpPost]
        public IActionResult DeleteUgdetail(int id)
        {
            var item = _context.Ugdetails.Find(id);
            if (item != null)
            {
                _context.Ugdetails.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("MedicalUgAndPgDetail");
        }

        [HttpGet]
        public IActionResult DownloadYearwiseDocument(int id, string docType)
        {
            if (id <= 0 || string.IsNullOrEmpty(docType))
                return BadRequest();

            var record = _context.NursingAffiliatedYearwiseMaterialsData
                .FirstOrDefault(x => x.Id == id);

            if (record == null)
                return NotFound();

            byte[] fileBytes = docType switch
            {
                "ParentHospitalMoudoc" => record.ParentHospitalMoudoc,
                "ParentHospitalKpmebedsDoc" => record.ParentHospitalKpmebedsDoc,
                "ParentHospitalOwnerNameDoc" => record.ParentHospitalOwnerNameDoc,
                "ParentHospitalPostBasicDoc" => record.ParentHospitalPostBasicDoc,
                _ => null
            };

            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound();

            // Basic MIME sniff: detect PDF/JPEG/PNG, fall back to octet-stream
            string contentType = "application/octet-stream";
            if (fileBytes.Length >= 4 &&
                fileBytes[0] == 0x25 && fileBytes[1] == 0x50 && fileBytes[2] == 0x44 && fileBytes[3] == 0x46) // %PDF
            {
                contentType = "application/pdf";
            }
            else if (fileBytes.Length >= 3 &&
                     fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF) // JPEG
            {
                contentType = "image/jpeg";
            }
            else if (fileBytes.Length >= 8 &&
                     fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47) // PNG
            {
                contentType = "image/png";
            }

            // Force inline display in browser. Many browsers will display PDFs/images inline when content-type supports it.
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{docType}_{id}\"";

            return File(fileBytes, contentType);
        }

    }
}
