using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;


using Microsoft.EntityFrameworkCore;

public class Aff_MedicalController : Controller
{
    private readonly ApplicationDbContext _context;

    public Aff_MedicalController(ApplicationDbContext context)
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
            PGCourses = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName
                }).ToList(),
            DegreeList = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                .Select(c => c.CourseLevel)   // select just the property you care about
                .Distinct()                   // distinct now works as intended
                .OrderBy(level => level)
                .Select(level => new SelectListItem
                {
                    Value = level.ToString(),
                    Text = level.ToString()
                })
                .ToList(),
            SpecializationList = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName
                }).ToList(),


        };

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
            model.SelectedQualification = existingEntity.SelectedSpecialities; // Store as single string
            model.CollegeAddress = existingEntity.InstituteAddress;
            model.SelectedDegree = existingEntity.Course;
            model.SelectedSpecialization = existingEntity.Specialisation;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Medical_BasicDetails(MedicalInstituteDetailViewModel model)
    {
        var collegeCode = HttpContext.Session.GetString("CollegeCode");
        var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

        if (string.IsNullOrEmpty(facultyCodeString))
            return RedirectToAction("Login", "Account");

        //if (!ModelState.IsValid)
        //{
        //    model.PGCourses = _context.MstCourses
        //        .Where(c =>  c.FacultyCode.ToString() == facultyCodeString )
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CourseCode.ToString(),
        //            Text = c.CourseName
        //        }).ToList();

        //    model.DegreeList = _context.MstCourses
        //            .Where(c => c.FacultyCode.ToString() == facultyCodeString)
        //            .Select(c => c.CourseLevel)   // select just the property you care about
        //            .Distinct()                   // distinct now works as intended
        //            .OrderBy(level => level)
        //            .Select(level => new SelectListItem
        //            {
        //                Value = level.ToString(),
        //                Text = level.ToString()
        //            })
        //            .ToList();

        //    model.SpecializationList = _context.MstCourses
        //        .Where(c => c.FacultyCode.ToString() == facultyCodeString)
        //        .OrderBy(c => c.CourseName)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CourseCode.ToString(),
        //            Text = c.CourseName
        //        }).ToList();

        //    return View(model);
        //}

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
            existingEntity.SelectedSpecialities = model.SelectedQualification; // Save as single string
            existingEntity.InstituteAddress = model.InstituteAddress;
            existingEntity.Course = model.SelectedDegree;
            existingEntity.Specialisation = model.SelectedSpecialization;

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
            };

            _context.Add(newEntity);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("ManageCourses");
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

    //Year wise clinical material




    //public async Task<IActionResult> CourseDetails()
    //{
    //    var coursesList = await _context.MstCourses.ToListAsync();
    //    var existingCourses = await _context.MedicalCourseDetails.ToListAsync();

    //    var viewModel = new Medical_CourseViewModel
    //    {
    //        CoursesDropdown = coursesList.Select(c => new SelectListItem
    //        {
    //            Value = c.CourseCode.ToString(),
    //            Text = c.CourseName
    //        }).ToList(),

    //        ExistingCourses = existingCourses
    //    };

    //    return View(viewModel);
    //}

    //// POST: Add new course
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Add(Medical_CourseViewModel model)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        var course = new MedicalCourseDetail
    //        {
    //            CollegeCode = model.CollegeCode,
    //            FacultyCode = model.FacultyCode,
    //            CourseCode = model.CourseCode,
    //            FreshOrIncrease = model.FreshOrIncrease,
    //            FirstLopdate = model.FirstLOPDate,
    //            NoOfSeats = model.NoOfSeats,
    //            PermittedYear = model.PermittedYear,
    //            RecognizedYear = model.RecognizedYear
    //        };

    //        // Handle file uploads
    //        if (model.RGUHSNotificationFile != null)
    //        {
    //            using var ms = new MemoryStream();
    //            await model.RGUHSNotificationFile.CopyToAsync(ms);
    //            course.Rguhsnotification = ms.ToArray();
    //        }

    //        if (model.GMCFile != null)
    //        {
    //            using var ms = new MemoryStream();
    //            await model.GMCFile.CopyToAsync(ms);
    //            course.Gmc = ms.ToArray();
    //        }

    //        if (model.NMCFile != null)
    //        {
    //            using var ms = new MemoryStream();
    //            await model.NMCFile.CopyToAsync(ms);
    //            course.Nmc = ms.ToArray();
    //        }

    //        _context.MedicalCourseDetails.Add(course);
    //        await _context.SaveChangesAsync();

    //        return RedirectToAction(nameof(Index));
    //    }

    //    // Reload dropdown and existing courses if validation fails
    //    var coursesList = await _context.MstCourses.ToListAsync();
    //    model.CoursesDropdown = coursesList.Select(c => new SelectListItem
    //    {
    //        Value = c.CourseCode.ToString(),
    //        Text = c.CourseName
    //    }).ToList();

    //    model.ExistingCourses = await _context.MedicalCourseDetails.ToListAsync();

    //    return View("Index", model);
    //}

    //// POST: Delete course by ID
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Delete(int id)
    //{
    //    var course = await _context.MedicalCourseDetails.FindAsync(id);
    //    if (course != null)
    //    {
    //        _context.MedicalCourseDetails.Remove(course);
    //        await _context.SaveChangesAsync();
    //    }
    //    return RedirectToAction(nameof(Index));
    //}


    //[HttpGet]
    //public IActionResult Clinical_Material()
    //{
    //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
    //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

    //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
    //    {
    //        // Redirect to login if session expired
    //        return RedirectToAction("Login", "Dashboard");
    //    }

    //    // Load parameter master data
    //    var parameters = _context.ClinicalMaterialData.ToList();

    //    // Check for existing saved data
    //    var existingData = _context.YearwiseMaterialsData
    //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
    //        .ToList();

    //    if (existingData.Any())
    //    {
    //        return View(existingData);
    //    }

    //    // Initialize blank data based on parameters
    //    var dataList = parameters.Select(p => new YearwiseMaterialsDatum
    //    {
    //        ParametersId = p.Id,
    //        ParametersName = p.ParametersName,
    //        Year1 = "",
    //        Year2 = "",
    //        Year3 = ""
    //    }).ToList();

    //    return View(dataList);
    //}

    //// POST: Save year-wise data into yearwise_MaterialsData
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public IActionResult Clinical_Material(List<YearwiseMaterialsDatum> dataList)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        string collegeCode = HttpContext.Session.GetString("CollegeCode");
    //        string facultyCode = HttpContext.Session.GetString("FacultyCode");

    //        if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
    //        {
    //            ModelState.AddModelError("", "Session expired. Please login again.");
    //            return View(dataList);
    //        }

    //        // Remove existing records for this CollegeCode and FacultyCode
    //        var existingRecords = _context.YearwiseMaterialsData
    //            .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
    //            .ToList();

    //        if (existingRecords.Any())
    //        {
    //            _context.YearwiseMaterialsData.RemoveRange(existingRecords);
    //            _context.SaveChanges();
    //        }

    //        // Populate data before saving
    //        foreach (var item in dataList)
    //        {
    //            item.CollegeCode = collegeCode;
    //            item.FacultyCode = facultyCode;

    //            var parameter = _context.ClinicalMaterialData
    //                .FirstOrDefault(p => p.Id == item.ParametersId);

    //            if (parameter != null)
    //            {
    //                item.ParametersName = parameter.ParametersName;
    //            }
    //            else
    //            {
    //                ModelState.AddModelError("", "Invalid Parameter ID.");
    //                return View(dataList);
    //            }
    //        }

    //        _context.YearwiseMaterialsData.AddRange(dataList);
    //        _context.SaveChanges();

    //        // Redirect to next step (adjust as necessary)
    //        return RedirectToAction("TeachingFacultyDetails");
    //    }

    //    return View(dataList);
    //}


    //Teachers data


    [HttpGet]
    public IActionResult FacultyDetails()
    {
        string collegeCode = HttpContext.Session.GetString("CollegeCode");
        string facultyCode = HttpContext.Session.GetString("FacultyCode");

        // Fetch dropdown lists
        var subjectsList = _context.MstCourses
            .Where(c => c.FacultyCode.ToString() == facultyCode)
            .Select(c => new SelectListItem
            {
                Value = c.CourseCode.ToString(),
                Text = c.CourseName
            })
            .ToList();

        //code updated by ram on 01-12-2025
        var designationsList = _context.DesignationMasters
        .Where(c => c.FacultyCode.ToString() == facultyCode)
        .Select(d => new SelectListItem
        {
            Value = d.DesignationCode,
            Text = d.DesignationName ?? ""
        })
        .ToList();

        var departmentsList = _context.DepartmentMasters
            .Select(d => new SelectListItem
            {
                Value = d.DepartmentCode,
                Text = d.DepartmentName
            })
            .ToList();

        // Fetch existing faculty details
        var existingFaculty = _context.FacultyDetails
            .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
            .ToList();

        var vmList = existingFaculty.Select(f => new FacultyDetailsViewModel
        {
            NameOfFaculty = f.NameOfFaculty,
            DepartmentDetail = f.DepartmentDetails,
            SelectedDepartment = f.DepartmentDetails,
            Subject = f.Subject?.Trim(),
            Designation = f.Designation?.Trim(),
            RecognizedPGTeacher = f.RecognizedPgTeacher?.Trim() ?? "",
            Mobile = f.Mobile,
            Email = f.Email,
            PAN = f.Pan,
            Aadhaar = f.Aadhaar,
            Subjects = subjectsList,
            Designations = designationsList,
            DepartmentDetails = departmentsList
        }).ToList();


        // Ensure at least one empty row is present for user input
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FacultyDetails(List<FacultyDetailsViewModel> FacultyList)
    {
        string collegeCode = HttpContext.Session.GetString("CollegeCode");
        string facultyCode = HttpContext.Session.GetString("FacultyCode");

        if (FacultyList == null || !FacultyList.Any())
        {
            TempData["Error"] = "No data to save.";
            return View(FacultyList);
        }

        //if (!ModelState.IsValid)
        //{
        //    // Return view with validation errors
        //    return View(FacultyList);
        //}

        foreach (var model in FacultyList)
        {
            var existing = _context.FacultyDetails.FirstOrDefault(f =>
                f.CollegeCode == collegeCode &&
                f.FacultyCode == facultyCode &&
                f.Subject == model.Subject);

            if (existing != null)
            {
                // Update existing record
                existing.NameOfFaculty = model.NameOfFaculty;
                existing.Designation = model.Designation;
                existing.RecognizedPgTeacher = model.RecognizedPGTeacher;
                existing.Mobile = model.Mobile;
                existing.Email = model.Email;
                existing.Pan = model.PAN;
                existing.Aadhaar = model.Aadhaar;
                existing.DepartmentDetails = model.SelectedDepartment;  // Important mapping
            }
            else
            {
                var faculty = new FacultyDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    NameOfFaculty = model.NameOfFaculty,
                    Subject = model.Subject,
                    Designation = model.Designation,
                    RecognizedPgTeacher = model.RecognizedPGTeacher,
                    Mobile = model.Mobile,
                    Email = model.Email,
                    Pan = model.PAN,
                    Aadhaar = model.Aadhaar,
                    DepartmentDetails = model.SelectedDepartment  // Correct mapping
                };
                _context.FacultyDetails.Add(faculty);
            }

        }

        _context.SaveChanges();
        TempData["Success"] = "College records saved successfully!";
        return RedirectToAction("FacultyDetails");
    }

    [HttpGet]
    public IActionResult ManageCourses()
    {
        string facultyCode = HttpContext.Session.GetString("FacultyCode");
        string collegeCode = HttpContext.Session.GetString("CollegeCode");

        if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            return RedirectToAction("Login", "CollegeLogin");

        int facultyCodeInt = int.Parse(facultyCode);

        var seatSlabs = _context.SeatSlabMasters
            .Where(e => e.FacultyCode == facultyCodeInt)
            .Select(e => e.SeatSlab)
            .ToList();

        var intakeOptions = seatSlabs
            .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() })
            .ToList();

        // ===== Dropdowns =====
        var ugCourses = _context.MstCourses
            .Where(c => c.CourseLevel == "UG" && c.FacultyCode == facultyCodeInt)
            .Select(c => new CourseDropdownVM { CourseCode = c.CourseCode.ToString(), CourseName = c.CourseName })
            .ToList();

        var pgCourses = _context.MstCourses
            .Where(c => c.CourseLevel == "PG" && c.FacultyCode == facultyCodeInt)
            .Select(c => new CourseDropdownVM { CourseCode = c.CourseCode.ToString(), CourseName = c.CourseName })
            .ToList();

        var ssCourses = _context.MstCourses
            .Where(c => c.CourseLevel == "SS" && c.FacultyCode == facultyCodeInt)
            .Select(c => new CourseDropdownVM { CourseCode = c.CourseCode.ToString(), CourseName = c.CourseName })
            .ToList();

        // ===== Saved details filtered by CourseLevel =====
        var savedUG = (from u in _context.Ugdetails
                       join c in _context.MstCourses on u.Course equals c.CourseCode.ToString()
                       where u.FacultyCode == facultyCode &&
                             u.CollegeCode == collegeCode &&
                             c.CourseLevel == "UG"
                       select new UGDetailsViewModel
                       {
                           Id = u.Id,
                           Course = u.Course,
                           UGIntake = u.Ugintake,
                           FreshOrIncrease = u.FreshOrIncrease,
                           FirstLOPDate = u.FirstLopdate,
                           NumberOfSeats = u.NumberOfSeats,
                           PermittedYear = u.PermittedYear,
                           RecognizedYear = u.RecognizedYear,
                           UGIntakeOptions = intakeOptions
                           //Rguhsnotification = u.Rguhsnotification != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Rguhsnotification) : null,
                           //Gmc = u.Gmc != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Gmc) : null,
                           //Nmc = u.Nmc != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Nmc) : null
                       }).ToList();

        var savedPG = (from u in _context.Ugdetails
                       join c in _context.MstCourses on u.Course equals c.CourseCode.ToString()
                       where u.FacultyCode == facultyCode &&
                             u.CollegeCode == collegeCode &&
                             c.CourseLevel == "PG"
                       select new PGDetailsViewModel
                       {
                           Id = u.Id,
                           Course = u.Course,
                           PGIntake = u.Ugintake,
                           FreshOrIncrease = u.FreshOrIncrease,
                           FirstLOPDate = u.FirstLopdate,
                           NumberOfSeats = u.NumberOfSeats,
                           PermittedYear = u.PermittedYear,
                           RecognizedYear = u.RecognizedYear,
                           //Rguhsnotification = u.Rguhsnotification != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Rguhsnotification) : null,
                           //Gmc = u.Gmc != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Gmc) : null,
                           //Nmc = u.Nmc != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Nmc) : null
                       }).ToList();

        var savedSS = (from u in _context.Ugdetails
                       join c in _context.MstCourses on u.Course equals c.CourseCode.ToString()
                       where u.FacultyCode == facultyCode &&
                             u.CollegeCode == collegeCode &&
                             c.CourseLevel == "SS"
                       select new SSDetailsViewModel
                       {
                           Id = u.Id,
                           Course = u.Course,
                           SSIntake = u.Ugintake,
                           FreshOrIncrease = u.FreshOrIncrease,
                           FirstLOPDate = u.FirstLopdate,
                           NumberOfSeats = u.NumberOfSeats,
                           PermittedYear = u.PermittedYear,
                           RecognizedYear = u.RecognizedYear,
                           //Rguhsnotification = u.Rguhsnotification != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Rguhsnotification) : null,
                           //Gmc = u.Gmc != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Gmc) : null,
                           //Nmc = u.Nmc != null ?
                           //"data:application/pdf;base64," + Convert.ToBase64String(u.Nmc) : null
                       }).ToList();

        // ===== Prepare VM =====
        var vm = new UGPGSSDetailsFormViewModel
        {
            FacultyCode = facultyCode,
            CollegeCode = collegeCode,
            UGCourses = ugCourses,
            PGCourses = pgCourses,
            SSCourses = ssCourses,
            UGDetailsList = savedUG.Any()
                                    ? savedUG
                                    : new List<UGDetailsViewModel> { new UGDetailsViewModel { UGIntakeOptions = intakeOptions } },
            PGDetailsList = savedPG.Any() ? savedPG : new List<PGDetailsViewModel> { new PGDetailsViewModel() },
            SSDetailsList = savedSS.Any() ? savedSS : new List<SSDetailsViewModel> { new SSDetailsViewModel() }
        };

        // ===== JS intake maps: include all courses =====
        ViewBag.UGIntakes = _context.CollegeCourseIntakeDetails
            .Where(i => i.CollegeCode == collegeCode && ugCourses.Select(c => c.CourseCode).Contains(i.CourseCode))
            .ToDictionary(i => i.CourseCode, i => i.ExistingIntake ?? 0);

        ViewBag.PGIntakes = _context.CollegeCourseIntakeDetails
            .Where(i => i.CollegeCode == collegeCode && pgCourses.Select(c => c.CourseCode).Contains(i.CourseCode))
            .ToDictionary(i => i.CourseCode, i => i.ExistingIntake ?? 0);

        ViewBag.SSIntakes = _context.CollegeCourseIntakeDetails
            .Where(i => i.CollegeCode == collegeCode && ssCourses.Select(c => c.CourseCode).Contains(i.CourseCode))
            .ToDictionary(i => i.CourseCode, i => i.ExistingIntake ?? 0);

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ManageCourses(UGPGSSDetailsFormViewModel model)
    {
        string facultyCode = HttpContext.Session.GetString("FacultyCode");
        string collegeCode = HttpContext.Session.GetString("CollegeCode");

        if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            return RedirectToAction("Login", "CollegeLogin");

        // ---------- Save UG ----------
        foreach (var item in model.UGDetailsList)
        {
            if (!string.IsNullOrEmpty(item.Course) && !string.IsNullOrEmpty(item.UGIntake))
            {
                // Check for existing record
                var existing = _context.Ugdetails
                    .FirstOrDefault(u => u.FacultyCode == facultyCode
                                      && u.CollegeCode == collegeCode
                                      && u.Course == item.Course
                                      && u.Ugintake == item.UGIntake);

                if (existing != null)
                {
                    // Update existing fields
                    existing.FreshOrIncrease = item.FreshOrIncrease;
                    existing.FirstLopdate = item.FirstLOPDate;
                    existing.NumberOfSeats = item.NumberOfSeats;
                    existing.PermittedYear = item.PermittedYear;
                    existing.RecognizedYear = item.RecognizedYear;

                    // Preserve existing files if no new upload
                    if (item.RGUHSNotificationFile != null)
                        existing.Rguhsnotification = ConvertToBytes(item.RGUHSNotificationFile);

                    if (item.GMCFile != null)
                        existing.Gmc = ConvertToBytes(item.GMCFile);

                    if (item.NMCFile != null)
                        existing.Nmc = ConvertToBytes(item.NMCFile);
                }
                else
                {
                    // Insert new
                    var entity = new Ugdetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        Ugintake = item.UGIntake,
                        Course = item.Course,
                        FreshOrIncrease = item.FreshOrIncrease,
                        FirstLopdate = item.FirstLOPDate,
                        NumberOfSeats = item.NumberOfSeats,
                        PermittedYear = item.PermittedYear,
                        RecognizedYear = item.RecognizedYear,
                        Rguhsnotification = item.RGUHSNotificationFile != null ? ConvertToBytes(item.RGUHSNotificationFile) : null,
                        Gmc = item.GMCFile != null ? ConvertToBytes(item.GMCFile) : null,
                        Nmc = item.NMCFile != null ? ConvertToBytes(item.NMCFile) : null
                    };
                    _context.Ugdetails.Add(entity);
                }
            }
        }

        // ---------- Save PG ----------
        foreach (var item in model.PGDetailsList)
        {
            if (!string.IsNullOrEmpty(item.Course) && !string.IsNullOrEmpty(item.PGIntake))
            {
                var existing = _context.Ugdetails
                    .FirstOrDefault(u => u.FacultyCode == facultyCode
                                      && u.CollegeCode == collegeCode
                                      && u.Course == item.Course
                                      && u.Ugintake == item.PGIntake);

                if (existing != null)
                {
                    existing.FreshOrIncrease = item.FreshOrIncrease;
                    existing.FirstLopdate = item.FirstLOPDate;
                    existing.NumberOfSeats = item.NumberOfSeats;
                    existing.PermittedYear = item.PermittedYear;
                    existing.RecognizedYear = item.RecognizedYear;

                    if (item.RGUHSNotificationFile != null)
                        existing.Rguhsnotification = ConvertToBytes(item.RGUHSNotificationFile);

                    if (item.GMCFile != null)
                        existing.Gmc = ConvertToBytes(item.GMCFile);

                    if (item.NMCFile != null)
                        existing.Nmc = ConvertToBytes(item.NMCFile);
                }
                else
                {
                    var entity = new Ugdetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        Ugintake = item.PGIntake,
                        Course = item.Course,
                        FreshOrIncrease = item.FreshOrIncrease,
                        FirstLopdate = item.FirstLOPDate,
                        NumberOfSeats = item.NumberOfSeats,
                        PermittedYear = item.PermittedYear,
                        RecognizedYear = item.RecognizedYear,
                        Rguhsnotification = item.RGUHSNotificationFile != null ? ConvertToBytes(item.RGUHSNotificationFile) : null,
                        Gmc = item.GMCFile != null ? ConvertToBytes(item.GMCFile) : null,
                        Nmc = item.NMCFile != null ? ConvertToBytes(item.NMCFile) : null
                    };
                    _context.Ugdetails.Add(entity);
                }
            }
        }

        // ---------- Save SS ----------
        foreach (var item in model.SSDetailsList)
        {
            if (!string.IsNullOrEmpty(item.Course) && !string.IsNullOrEmpty(item.SSIntake))
            {
                var existing = _context.Ugdetails
                    .FirstOrDefault(u => u.FacultyCode == facultyCode
                                      && u.CollegeCode == collegeCode
                                      && u.Course == item.Course
                                      && u.Ugintake == item.SSIntake);

                if (existing != null)
                {
                    existing.FreshOrIncrease = item.FreshOrIncrease;
                    existing.FirstLopdate = item.FirstLOPDate;
                    existing.NumberOfSeats = item.NumberOfSeats;
                    existing.PermittedYear = item.PermittedYear;
                    existing.RecognizedYear = item.RecognizedYear;

                    if (item.RGUHSNotificationFile != null)
                        existing.Rguhsnotification = ConvertToBytes(item.RGUHSNotificationFile);

                    if (item.GMCFile != null)
                        existing.Gmc = ConvertToBytes(item.GMCFile);

                    if (item.NMCFile != null)
                        existing.Nmc = ConvertToBytes(item.NMCFile);
                }
                else
                {
                    var entity = new Ugdetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        Ugintake = item.SSIntake,
                        Course = item.Course,
                        FreshOrIncrease = item.FreshOrIncrease,
                        FirstLopdate = item.FirstLOPDate,
                        NumberOfSeats = item.NumberOfSeats,
                        PermittedYear = item.PermittedYear,
                        RecognizedYear = item.RecognizedYear,
                        Rguhsnotification = item.RGUHSNotificationFile != null ? ConvertToBytes(item.RGUHSNotificationFile) : null,
                        Gmc = item.GMCFile != null ? ConvertToBytes(item.GMCFile) : null,
                        Nmc = item.NMCFile != null ? ConvertToBytes(item.NMCFile) : null
                    };
                    _context.Ugdetails.Add(entity);
                }
            }
        }

        _context.SaveChanges();
        return RedirectToAction("YearwiseMaterials");
    }


    public async Task<IActionResult> ViewFile(int id, string fileType)
    {
        var ug = await _context.Ugdetails.FindAsync(id);
        if (ug == null) return NotFound();

        byte[] fileData = null;

        switch (fileType)
        {
            case "RGUHS": fileData = ug.Rguhsnotification; break;
            case "GMC": fileData = ug.Gmc; break;
            case "NMC": fileData = ug.Nmc; break;
        }

        if (fileData == null)
            return NotFound();

        // Browser will automatically open PDF in new tab
        return File(fileData, "application/pdf");
    }

    [HttpGet]
    public IActionResult CollegeDesignation()
    {
        var facultyCode = HttpContext.Session.GetString("FacultyCode");
        var collegeCode = HttpContext.Session.GetString("CollegeCode");

        var model = new CollegeDesignationDetailViewModel
        {
            FacultyCode = facultyCode,
            CollegeCode = collegeCode,
            Designations = _context.DesignationMasters
                                   .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName })
                                   .ToList()
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CollegeDesignation(CollegeDesignationDetailViewModel model)
    {
        var facultyCode = HttpContext.Session.GetString("FacultyCode");
        var collegeCode = HttpContext.Session.GetString("CollegeCode");

        if (ModelState.IsValid)
        {
            var entity = new CollegeDesignationDetail
            {
                FacultyCode = facultyCode,   // Always from Session
                CollegeCode = collegeCode,   // Always from Session
                Designation = model.Designation,
                RequiredIntake = model.RequiredIntake.ToString(),
                AvailableIntake = model.AvailableIntake.ToString()
            };

            _context.CollegeDesignationDetails.Add(entity);
            _context.SaveChanges();

            TempData["Success"] = "College Designation Details saved successfully!";
            return RedirectToAction("Create");
        }

        // repopulate dropdown if validation fails
        model.Designations = _context.DesignationMasters
                                     .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName })
                                     .ToList();

        return View(model);
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
    public async Task<IActionResult> TeachingFacultyDetails(List<TeachingFacultyViewModel> model)
    {
        var collegeCode = HttpContext.Session.GetString("CollegeCode");

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
                // Update existing record
                existingRecord.FacultyCode = item.FacultyCode;
                existingRecord.Designation = item.DesignationName;
                existingRecord.Department = item.DepartmentName;
                existingRecord.RequiredIntake = item.ExistingSeatIntake;
                existingRecord.AvailableIntake = item.PresentSeatIntake ?? "0";

                _context.CollegeDesignationDetails.Update(existingRecord);
            }
            else
            {
                // Insert new record
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

        return RedirectToAction("FacultyDetails");
    }

    [HttpGet]
    public IActionResult YearwiseMaterials()
    {
        var collegeCode = HttpContext.Session.GetString("CollegeCode");
        var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

        // Fetch all parameters
        var parameters = _context.ClinicalMaterialData.ToList();

        // Fetch existing Yearwise data for this college & faculty
        var yearwiseList = _context.YearwiseMaterialsData
            .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCodeString)
            .Select(y => new YearwiseMaterialViewModel
            {
                Id = y.Id,
                CollegeCode = y.CollegeCode,
                FacultyCode = y.FacultyCode,
                ParametersId = y.ParametersId,
                ParametersName = y.ParametersName,
                Year1 = y.Year1,
                Year2 = y.Year2,
                Year3 = y.Year3
            })
            .ToList();

        // If no data exists, create default list based on parameters
        if (!yearwiseList.Any())
        {
            yearwiseList = parameters.Select(p => new YearwiseMaterialViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCodeString,
                ParametersId = p.ParametersId,
                ParametersName = p.ParametersName
            }).ToList();
        }

        return View(yearwiseList);
    }

    [HttpPost]
    public IActionResult YearwiseMaterials(List<YearwiseMaterialViewModel> model)
    {

        var collegeCode = HttpContext.Session.GetString("CollegeCode");
        var facultyCode = HttpContext.Session.GetString("FacultyCode");

        if (model == null || !model.Any())
        {
            TempData["Error"] = "No data to save.";
            return RedirectToAction("TeachingFacultyDetails");
        }

        foreach (var item in model)
        {
            // Try to find existing record
            var existing = _context.YearwiseMaterialsData
                .FirstOrDefault(y => y.CollegeCode == item.CollegeCode
                                  && y.FacultyCode == item.FacultyCode
                                  && y.ParametersId == item.ParametersId);

            // Ensure ParametersName is not null
            string parametersName = item.ParametersName;
            if (string.IsNullOrEmpty(parametersName))
            {
                parametersName = _context.ClinicalMaterialData
                    .Where(p => p.ParametersId == item.ParametersId)
                    .Select(p => p.ParametersName)
                    .FirstOrDefault() ?? "Unknown";
            }

            if (existing != null)
            {
                // Update existing record
                existing.Year1 = item.Year1;
                existing.Year2 = item.Year2;
                existing.Year3 = item.Year3;
                existing.ParametersName = parametersName; // Ensure ParametersName is always set
            }
            else
            {
                // Insert new record
                var newRecord = new YearwiseMaterialsDatum
                {
                    CollegeCode = item.CollegeCode,
                    FacultyCode = item.FacultyCode,
                    ParametersId = item.ParametersId,
                    ParametersName = parametersName,
                    Year1 = item.Year1,
                    Year2 = item.Year2,
                    Year3 = item.Year3
                };
                _context.YearwiseMaterialsData.Add(newRecord);
            }
        }

        //try
        //{
        //    _context.SaveChanges();
        //    //TempData["Success"] = "Data saved successfully!";
        //}
        ////catch (Exception ex)
        ////{
        ////    // Log exception if needed
        ////    //TempData["Error"] = "Error saving data: " + ex.Message;
        ////}
        _context.SaveChanges();
        return RedirectToAction("TeachingFacultyDetails");
    }


    private byte[] ConvertToBytes(IFormFile file)
    {
        using var ms = new MemoryStream();
        file.CopyTo(ms);
        return ms.ToArray();
    }



    public IActionResult ViewDocument(string collegeCode, string fileType)
    {
        var record = _context.YearwiseMaterialsData
            .FirstOrDefault(y => y.CollegeCode == collegeCode);

        if (record == null)
            return NotFound();

        byte[] fileBytes = null;
        string fileName = "";
        string contentType = "application/octet-stream";

        switch (fileType)
        {
            case "Kpmebeds":
                fileBytes = record.ParentHospitalKpmebedsDoc;
                fileName = "KpmeBedsDoc.pdf"; // adjust file name and type if known
                contentType = "application/pdf"; // adjust accordingly
                break;
            case "MOU":
                fileBytes = record.ParentHospitalMoudoc;
                fileName = "MouDoc.pdf";
                contentType = "application/pdf";
                break;
            case "OwnerName":
                fileBytes = record.ParentHospitalOwnerNameDoc;
                fileName = "OwnerNameDoc.pdf";
                contentType = "application/pdf";
                break;
            case "PostBasic":
                fileBytes = record.ParentHospitalPostBasicDoc;
                fileName = "PostBasicDoc.pdf";
                contentType = "application/pdf";
                break;
        }

        if (fileBytes == null)
            return NotFound();

        return File(fileBytes, contentType, fileName);
    }



    // Helper method to convert IFormFile to byte[]
    private byte[] ConvertFileToBytes(IFormFile formFile)
    {
        using (var ms = new MemoryStream())
        {
            formFile.CopyTo(ms);
            return ms.ToArray();
        }
    }


}



