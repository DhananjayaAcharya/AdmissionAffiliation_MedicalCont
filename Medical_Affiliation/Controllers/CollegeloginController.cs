//using Admission_Affiliation.Data;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Xml.Linq;
using Medical_Affiliation.DATA;
using System.Security.Claims;
using Medical_Affiliation.Controllers;

namespace Admission_Affiliation.Controllers
{
    public class CollegeloginController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CollegeloginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: Login Page
        [HttpGet]
        public IActionResult ClgLogin()
        {
            if (IsLoggedIn()) return RedirectToAction("Dashboard");

            return View();
        }

        //[Authorize(AuthenticationSchemes = "CollegeAuth")]
        // ✅ POST: Handle Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClgLogin(CollegeLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.AffiliationCollegeMasters
                .FirstOrDefault(c => c.CollegeCode == model.CollegeId);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid College Code or Password.");
                return View(model);
            }
            var clgIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            var clgAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.CollegeName ?? ""),
                    new Claim(ClaimTypes.Role, "College"),

                    new Claim("CollegeCode", user.CollegeCode ?? ""),
                    new Claim("FacultyCode", user.FacultyCode ?? ""),

                    new Claim("UserIP", clgIP ?? string.Empty),
                    new Claim("UserAgent", clgAgent ?? string.Empty)
                };

            var identity = new ClaimsIdentity(claims, "CollegeAuth");
            var principal = new ClaimsPrincipal(identity);
            var buttons = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == model.CollegeId).FirstOrDefaultAsync();

            var ShowNodalOfficer = buttons.ShowNodalOfficerDetails ? "true" : "false";
            var ShowIntakeDetails = buttons.ShowIntakeDetails ? "true" : "false";
            var ShowRepository = buttons.ShowRepositoryDetails ? "true" : "false";

            HttpContext.Session.SetString("ShowNodalOfficer", ShowNodalOfficer);
            HttpContext.Session.SetString("ShowIntakeDetails", ShowIntakeDetails);
            HttpContext.Session.SetString("ShowRepository", ShowRepository);

            
            await HttpContext.SignInAsync("CollegeAuth", principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });



            //bool isPasswordValid = string.IsNullOrEmpty(user.ChangedPassword)
            //    ? user.Password == model.Password
            //    : user.ChangedPassword == model.Password;

            //if (!isPasswordValid)
            //{
            //    ModelState.AddModelError("", "Invalid College Code or Password.");
            //    return View(model);
            //}

            // ✅ Store session

            return RedirectToAction("Dashboard");
        }

        [Authorize(Policy = "CollegeOnly")]
        [HttpGet]
        public IActionResult Dashboard()
        {
            Console.WriteLine("=== Dashboard Action Executed ===");
            Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"CollegeCode from Claims: {User.FindFirst("CollegeCode")?.Value}");
            Console.WriteLine($"FacultyCode from Claims: {User.FindFirst("FacultyCode")?.Value}");

            var collegeCode = User.FindFirst("CollegeCode")?.Value;
            var collegeName = User.Identity?.Name;

            // Extra safety check
            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                Console.WriteLine("Missing CollegeCode - Redirecting to Login");
                return RedirectToAction("Login", "Login");
            }

            ViewBag.CollegeCode = collegeCode;
            ViewBag.CollegeName = collegeName;

            var college = _context.AffiliationCollegeMasters
                .FirstOrDefault(c => c.CollegeCode == collegeCode);

            if (college == null)
            {
                TempData["LoginError"] = "College record not found.";
                return RedirectToAction("Login", "Login");
            }

            TempData["ShowWelcomePopup"] = string.IsNullOrEmpty(college.ChangedPassword);
            TempData["CollegeName"] = collegeName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // optional for Logout
        public async Task<IActionResult> Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Sign out the College scheme
            await HttpContext.SignOutAsync("CollegeAuth");

            // Redirect back to login page
            return RedirectToAction("Login", "Login");

           // return RedirectToAction("MultiLogin", "MainDashboard");

        }




        //✅ GET: Change Password
        //[HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ChangePassword()
        {
            if (!IsLoggedIn()) return RedirectToAction("ClgLogin");

            return View(new ChangePasswordViewModel
            {
                CollegeCode = HttpContext.Session.GetString("CollegeCode")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("ClgLogin");

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.AffiliationCollegeMasters
                .FirstOrDefault(c => c.CollegeCode == model.CollegeCode);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid College Code.");
                return View(model);
            }

            // ✅ Check new/confirm match
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "Passwords do not match.");
                return View(model);
            }

            // ✅ Update password (use hashing only in real apps)
            var passwordHasher = new PasswordHasher<AffiliationCollegeMaster>();
            user.Password = model.NewPassword; // ⚠️ only if you need plain-text for legacy reasons
            user.ChangedPassword = model.NewPassword;
            user.HashedPassword = passwordHasher.HashPassword(user, model.NewPassword);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("GetExpectedDetails", "Collegelogin", new { collegeCode = model.CollegeCode });
        }


        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a hex string
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // lowercase hex
                }
                return builder.ToString();
            }
        }

        // ✅ GET: Update Institute
        //[HttpGet]
        //[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        //public IActionResult UpdateInstitute()
        //{
        //    if (!IsLoggedIn()) return RedirectToAction("ClgLogin");

        //    var collegeCode = HttpContext.Session.GetString("CollegeCode");

        //    var model = _context.AffiliationCollegeMasters
        //        .Where(c => c.CollegeCode == collegeCode)
        //        .Select(c => new InstituteUpdateViewModel
        //        {
        //            PrincipalName = c.PrincipalName,
        //            PrincipalMobileNo = c.PrincipalMobileNo,
        //            CollegeEmail = c.CollegeEmail,
        //            CollegeMobileNo = c.CollegeMobileNo
        //        }).FirstOrDefault();

        //    if (model == null) return NotFound();

        //    return View(model);
        //}

        // ✅ POST: Update Institute
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateInstitute(InstituteUpdateViewModel model)
        //{
        //    if (!IsLoggedIn()) return RedirectToAction("ClgLogin");

        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var collegeCode = HttpContext.Session.GetString("CollegeCode");

        //    var college = _context.AffiliationCollegeMasters
        //        .FirstOrDefault(c => c.CollegeCode == collegeCode);

        //    if (college == null) return NotFound();

        //    college.PrincipalName = model.PrincipalName;
        //    college.PrincipalMobileNo = model.PrincipalMobileNo;
        //    college.CollegeEmail = model.CollegeEmail;
        //    college.CollegeMobileNo = model.CollegeMobileNo;

        //    _context.SaveChanges();

        //    TempData["Success"] = "Institute details updated successfully!";
        //    return View(model);
        //}

        // ✅ Shared session check helper
        private bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("CollegeCode"));
        }

        [HttpGet]
        public JsonResult GetCourses(int facultyId, string courseLevel)
        {
            var courses = _context.CourseMasters
                .Distinct()
                .Where(c => c.FacultyId == facultyId && c.CourseLevel == courseLevel)
                .Select(c => new
                {
                    Value = c.CourseCode,
                    Text = c.CourseName
                })
                .OrderBy(c => c.Text)
                .ToList();

            return Json(courses);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseLevels(int facultyId)
        {
            var courseLevels = await _context.CourseMasters
                .Where(c => c.FacultyId == facultyId)
                .GroupBy(c => c.CourseLevel)
                .Select(g => new
                {
                    CourseLevel = g.Key,
                    DisplayOrder = g.Min(c => c.DisplayOrder) // or Max, depending on your logic
                })
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            var result = courseLevels.Select(cl => new SelectListItem
            {
                Value = cl.CourseLevel,
                Text = cl.CourseLevel
            });

            return Json(result);
        }


        //public async Task<IActionResult> CollegeDetails(string code)
        //{
        //    var college = await _context.AffiliationCollegeMasters
        //        .FirstOrDefaultAsync(e => e.CollegeCode == code);

        //    if (college == null)
        //    {
        //        return NotFound();
        //    }

        //    // Get intake details for this college
        //    var intakeDetails = await _context.CollegeCourseIntakeDetails
        //        .Where(i => i.CollegeCode == code)
        //        .ToListAsync();

        //    var courses = await _context.CourseMasters.ToListAsync();
        //    //var freshTypes = await _context.FreshOrIncreaseMasters.ToListAsync();
        //    var faculties = await _context.Faculties.ToListAsync();
        //    var freshOrContinuation = await _context.FreshOrIncreaseMasters.ToListAsync();
        //    var distinctCourseLevels = courses
        //        .OrderBy(e => e.DisplayOrder)
        //        .Select(e => e.CourseLevel)
        //        .Distinct()
        //        .ToList();

        //    // Map intake details to CollegeIntakeDetailViewModel
        //    var intakeViewModels = intakeDetails.Select(i => new CollegeIntakeDetailViewModel
        //    {
        //        Id = i.Id,
        //        CourseCode = i.CourseCode,
        //        CourseName = courses.FirstOrDefault(c => c.CourseCode == i.CourseCode)?.CourseName,
        //        SelectedCourse = i.CourseCode,
        //        Courses = courses,
        //        //FreshOrIncreaseMasters = freshOrContinuation,
        //        CourseLevels = distinctCourseLevels,
        //        IsCorrect = i.IsRemarksExist==false,
        //        IsIncorrect = i.IsRemarksExist==true,
        //        ExistingIntake = i.SanctionedIntakeFirstYear,
        //        FacultyCode = i.FacultyCode,
        //        //DocumentBytes = i.Document,
        //    }).ToList();

        //    // If no records, add one blank row
        //    if (!intakeViewModels.Any())
        //    {
        //        intakeViewModels.Add(new CollegeIntakeDetailViewModel
        //        {
        //            CourseLevels = distinctCourseLevels,
        //            //Courses = courses,
        //            FreshOrIncreaseMasters = freshOrContinuation
        //        });
        //    }

        //    var viewModel = new CollegeDetailsViewModel
        //    {
        //        FacultyMaster = faculties,
        //        CollegeCode = college.CollegeCode,
        //        CollegeName = college.CollegeName,
        //        CollegeIntakeDetailViewModel = intakeViewModels,
        //        CourseLevels = distinctCourseLevels,
        //        FreshOrIncreaseMasters = freshOrContinuation
        //    };

        //    return View("CollegeDetails", viewModel);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CollegeDetails(CollegeDetailsViewModel model)
        {
            // ✅ Fallback from TempData (like your BasicDetails code)
            if (model.FacultyCode == 0 && TempData["FacultyCode"] != null)
            {
                model.FacultyCode = Convert.ToInt32(TempData["FacultyCode"]);
            }

            if (string.IsNullOrEmpty(model.CollegeCode) && TempData["CollegeCode"] != null)
            {
                model.CollegeCode = TempData["CollegeCode"].ToString();
            }

            foreach (var intake in model.CollegeIntakeDetailViewModel)
            {
                byte[]? documentBytes = null;

                if (intake.DocumentFile != null && intake.DocumentFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await intake.DocumentFile.CopyToAsync(memoryStream);
                        documentBytes = memoryStream.ToArray();
                    }
                }

                // Try to fetch existing record for update
                var existingEntity = await _context.CollegeIntakeDetails
                    .FirstOrDefaultAsync(e => e.Id == intake.Id && e.CollegeCode == model.CollegeCode);

                if (existingEntity != null)
                {
                    // Update fields
                    existingEntity.CollegeName = model.CollegeName;
                    existingEntity.FacultyCode = model.FacultyCode.ToString();
                    existingEntity.CourseCode = intake.CourseCode;
                    existingEntity.CourseName = intake.SelectedCourse;
                    existingEntity.NoOfSeatsIntake = intake.NumberOfSeats.ToString();

                    if (documentBytes != null)
                        existingEntity.Document = documentBytes;

                    existingEntity.IsRemarksExist = intake.IsIncorrect == true;
                    existingEntity.Remarks = intake.IsIncorrect == true ? intake.Remarks : null;

                    _context.CollegeIntakeDetails.Update(existingEntity);
                }
                else
                {
                    var courses = await _context.CourseMasters.Where(e => e.CourseCode == intake.SelectedCourse).FirstOrDefaultAsync();
                    // Create new entity
                    var newEntity = new CollegeIntakeDetail
                    {
                        CollegeCode = model.CollegeCode,
                        CollegeName = model.CollegeName,
                        FacultyCode = model.FacultyCode.ToString(),
                        CourseCode = intake.SelectedCourse,
                        CourseName = courses.CourseName,
                        NoOfSeatsIntake = intake.NumberOfSeats.ToString(),
                        Document = documentBytes,
                        Remarks = intake.IsIncorrect == true ? intake.Remarks : null,
                        IsRemarksExist = intake.IsIncorrect == true
                    };

                    _context.CollegeIntakeDetails.Add(newEntity);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "College intake data saved successfully.";
            return RedirectToAction("CollegeDetails");
        }

        //[Authorize(AuthenticationSchemes = "CollegeAuth")]
        //[HttpGet]
        //public async Task<IActionResult> GetExpectedDetails(string collegeCode, string courseLevel, bool showPasswordPopup = false)
        //{
        //    ViewBag.ShowPasswordPopup = showPasswordPopup;

        //    // 1. Resolve collegeCode (query > session)
        //    if (string.IsNullOrWhiteSpace(collegeCode))
        //    {
        //        collegeCode = HttpContext.Session.GetString("CollegeCode");
        //        if (string.IsNullOrWhiteSpace(collegeCode))
        //        {
        //            HttpContext.Session.Clear();
        //            await HttpContext.SignOutAsync("CollegeAuth");
        //            return RedirectToAction("Login", "Login");
        //        }
        //    }

        //    // 2. Default course level
        //    if (string.IsNullOrWhiteSpace(courseLevel))
        //    {
        //        courseLevel = "PG";
        //    }

        //    // 3. Load base data
        //    var collegeDetails = await _context.CollegeCourseIntakeDetails
        //        .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode);

        //    var affiliationDetails = await _context.AffiliationCollegeMasters
        //        .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode);

        //    // 4. Courses for selected level
        //    var courses = await (
        //            from c in _context.CollegeCourseIntakeDetails
        //            join m in _context.MstCourses
        //                on c.CourseCode equals m.CourseCode.ToString()
        //            where c.CollegeCode == collegeCode
        //                  && m.CourseLevel == courseLevel
        //            orderby c.CourseName
        //            select new CourseDetail
        //            {
        //                CourseCode = c.CourseCode,
        //                CourseName = m.CourseName,
        //                ExistingIntake = c.ExistingIntake,
        //                PresentIntake = c.PresentIntake,
        //                RguhsIntake202526 = c.RguhsIntake202526,
        //                CollegeIntake202526 = c.CollegeIntake202526,

        //                // UI HIDDEN NOW – still computed if you need view flags
        //                IsDocument1Available = c.DocumentAffiliation != null && c.DocumentAffiliation.Length > 0,
        //                IsDocument2Available = c.DocumentLop != null && c.DocumentLop.Length > 0,

        //                // for disabling some inputs in view (but NOT LOP/NMC 2026-27)
        //                freezeStatus = c.FreezeFlag == 1,
        //                CourseLevel = courseLevel,

        //                // NEW docs flags if needed
        //                IsDocumentLop202627Available = c.DocumentLop202627 != null && c.DocumentLop202627.Length > 0,
        //                IsDocumentNmc202627Available = c.DocumentNmc202627 != null && c.DocumentNmc202627.Length > 0
        //            })
        //        .ToListAsync();

        //    // 5. Submit display flags per level
        //    var ugSubmitStatus = courses.Any(e => e.CourseLevel == "UG" && e.freezeStatus);
        //    var pgSubmitStatus = courses.Any(c => c.CourseLevel == "PG" && c.freezeStatus);
        //    var supSubmitStatus = courses.Any(c => c.CourseLevel == "SS" && c.freezeStatus);

        //    ViewBag.UgSubmitDisplay = !ugSubmitStatus;
        //    ViewBag.PgSubmitDisplay = !pgSubmitStatus;
        //    ViewBag.SupSubmitDisplay = !supSubmitStatus;

        //    HttpContext.Session.SetString("IsSubmitDisplay", ugSubmitStatus.ToString());

        //    // 6. Principal name existence flag (if you still need it)
        //    ViewBag.IsPrincipalNameExists = await (
        //        from cc in _context.CollegeCourseIntakeDetails
        //        join acm in _context.AffiliationCollegeMasters on cc.CollegeCode equals acm.CollegeCode
        //        where cc.PresentIntake > 0
        //        select acm.CollegeCode
        //    ).AnyAsync();

        //    // 7. Build view model
        //    var model = new CollegeCourseViewModel
        //    {
        //        CollegeCode = collegeCode,
        //        CollegeName = collegeDetails?.CollegeName,
        //        SelectedCourseLevel = courseLevel,
        //        CollegeAddress = collegeDetails?.CollegeAddress,
        //        PrincipalNameUG = affiliationDetails?.PrincipalNameDeclared,
        //        PrincipalNamePG = affiliationDetails?.PrincipalNameDeclared,
        //        PrincipalNameSS = affiliationDetails?.PrincipalNameDeclared,
        //        Courses = courses,
        //        DocumentUpload = await _context.AffiliationCollegeMasters
        //            .Where(e => e.CollegeCode == collegeCode)
        //            .Select(e => new DocumentUploadViewModel
        //            {
        //                IsDocsALlUploaded = e.AllDocsForCourse != null && e.AllDocsForCourse.Length > 0,
        //                CollegeCode = e.CollegeCode
        //            })
        //            .FirstOrDefaultAsync(),
        //        CourseLevelList = await _context.MstCourses
        //            .Select(e => e.CourseLevel.Trim())
        //            .Distinct()
        //            .Select(cl => new SelectListItem
        //            {
        //                Value = cl,
        //                Text = cl
        //            })
        //            .OrderBy(e => e.Value)
        //            .ToListAsync(),
        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //[ActionName("GetExpectedDetails")]
        //[RequestFormLimits(ValueCountLimit = 100000)]
        //public async Task<IActionResult> GetExpectedDetails(CollegeCourseViewModel model, List<IFormFile> AllDocuments)
        //{
        //    const long maxFileSize = 2 * 1024 * 1024; // 2 MB

        //    if (model == null || model.Courses == null || !model.Courses.Any())
        //    {
        //        TempData["ValidationErrors"] = JsonSerializer.Serialize(
        //            new List<string> { "No course data submitted." });
        //        return RedirectToAction("GetExpectedDetails",
        //            new { collegeCode = model?.CollegeCode, courseLevel = model?.SelectedCourseLevel });
        //    }

        //    // 0. Determine which level was submitted (UG/PG/SS) from the clicked button
        //    var submitLevel = Request.Form["SubmitLevel"].FirstOrDefault() ?? model.SelectedCourseLevel;

        //    // 1. VALIDATE COURSE-LEVEL FILES (incl. new 2026-27 docs)
        //    foreach (var course in model.Courses)
        //    {
        //        // Document1: RGUHS Affiliation Notification
        //        if (course.Document1 != null && course.Document1.Length > 0)
        //        {
        //            if (course.Document1.Length > maxFileSize)
        //                ModelState.AddModelError("", $"RGUHS Affiliation Notification for {course.CourseName} exceeds 2 MB.");
        //            if (course.Document1.ContentType != "application/pdf")
        //                ModelState.AddModelError("", $"RGUHS Affiliation Notification for {course.CourseName} must be a PDF.");
        //        }

        //        // Document2: NMC LOP (Current)
        //        if (course.Document2 != null && course.Document2.Length > 0)
        //        {
        //            if (course.Document2.Length > maxFileSize)
        //                ModelState.AddModelError("", $"NMC LOP for {course.CourseName} exceeds 2 MB.");
        //            if (course.Document2.ContentType != "application/pdf")
        //                ModelState.AddModelError("", $"NMC LOP for {course.CourseName} must be a PDF.");
        //        }

        //        // NEW DocumentLop202627: LOP 2026-27
        //        if (course.DocumentLop202627 != null && course.DocumentLop202627.Length > 0)
        //        {
        //            if (course.DocumentLop202627.Length > maxFileSize)
        //                ModelState.AddModelError("", $"LOP 2026-27 for {course.CourseName} exceeds 2 MB.");
        //            if (course.DocumentLop202627.ContentType != "application/pdf")
        //                ModelState.AddModelError("", $"LOP 2026-27 for {course.CourseName} must be a PDF.");
        //        }

        //        // NEW DocumentNmc202627: NMC 2026-27
        //        if (course.DocumentNmc202627 != null && course.DocumentNmc202627.Length > 0)
        //        {
        //            if (course.DocumentNmc202627.Length > maxFileSize)
        //                ModelState.AddModelError("", $"NMC 2026-27 for {course.CourseName} exceeds 2 MB.");
        //            if (course.DocumentNmc202627.ContentType != "application/pdf")
        //                ModelState.AddModelError("", $"NMC 2026-27 for {course.CourseName} must be a PDF.");
        //        }
        //    }

        //    // *** REMOVE COURSE-LEVEL / ROUTE CONFLICT KEYS FROM MODELSTATE ***
        //    var keysToRemove = ModelState.Keys.Where(k =>
        //        k.Contains("CourseLevel", StringComparison.OrdinalIgnoreCase) ||
        //        k.Contains("courseLevel", StringComparison.OrdinalIgnoreCase) ||
        //        k.Contains("collegeCode", StringComparison.OrdinalIgnoreCase) ||
        //        k.Contains("SelectedCourseLevel", StringComparison.OrdinalIgnoreCase)
        //    ).ToList();

        //    foreach (var key in keysToRemove)
        //        ModelState.Remove(key);

        //    // 2. Validate ModelState (only file errors remain)
        //    if (!ModelState.IsValid)
        //    {
        //        TempData["ValidationErrors"] = JsonSerializer.Serialize(
        //            ModelState.Values
        //                .SelectMany(v => v.Errors)
        //                .Select(e => e.ErrorMessage)
        //                .Where(e => !string.IsNullOrEmpty(e)));

        //        return RedirectToAction("GetExpectedDetails",
        //            new { collegeCode = model.CollegeCode, courseLevel = model.SelectedCourseLevel });
        //    }

        //    // 3. Validate Principal Name (any level)
        //    var principalName = model.PrincipalNamePG ?? model.PrincipalNameUG ?? model.PrincipalNameSS;
        //    if (string.IsNullOrWhiteSpace(principalName))
        //    {
        //        TempData["PrincipalErrorMessage"] = "Principal Name is required.";
        //        return RedirectToAction("GetExpectedDetails",
        //            new { collegeCode = model.CollegeCode, courseLevel = model.SelectedCourseLevel });
        //    }

        //    // 4. Update College Declaration Status
        //    var collegeMaster = await _context.AffiliationCollegeMasters
        //        .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

        //    if (collegeMaster != null)
        //    {
        //        string isDeclaredValue = (model.DeclarationAcceptedUG ||
        //                                  model.DeclarationAcceptedPG ||
        //                                  model.DeclarationAcceptedSS) ? "Y" : "N";

        //        collegeMaster.IsDeclared = isDeclaredValue;
        //        collegeMaster.PrincipalNameDeclared = principalName;
        //        _context.Entry(collegeMaster).State = EntityState.Modified;
        //    }

        //    // 5. Load college info once
        //    var collegeInfo = await _context.CollegeCourseIntakeDetails
        //        .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

        //    int courseAffectedRows = 0;

        //    // 6. Process EACH COURSE (CREATE/UPDATE)
        //    foreach (var course in model.Courses)
        //    {
        //        var dbCourse = await _context.CollegeCourseIntakeDetails
        //            .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode && c.CourseCode == course.CourseCode);

        //        if (dbCourse == null)
        //        {
        //            // CREATE NEW COURSE RECORD
        //            var newCourse = new CollegeCourseIntakeDetail
        //            {
        //                CollegeCode = model.CollegeCode,
        //                CollegeName = collegeInfo?.CollegeName?.Trim(),
        //                CollegeAddress = collegeInfo?.CollegeAddress,
        //                CourseCode = course.CourseCode,
        //                CourseName = course.CourseName,
        //                ExistingIntake = course.ExistingIntake,
        //                PresentIntake = course.PresentIntake,

        //                // nvarchar fields copied as string
        //                RguhsIntake202526 = course.RguhsIntake202526,
        //                CollegeIntake202526 = course.CollegeIntake202526,

        //                FreezeFlag = submitLevel == course.CourseLevel ? 1 : 0
        //            };

        //            if (course.Document1 != null && course.Document1.Length > 0)
        //            {
        //                using var ms1 = new MemoryStream();
        //                await course.Document1.CopyToAsync(ms1);
        //                newCourse.DocumentAffiliation = ms1.ToArray();
        //            }

        //            if (course.Document2 != null && course.Document2.Length > 0)
        //            {
        //                using var ms2 = new MemoryStream();
        //                await course.Document2.CopyToAsync(ms2);
        //                newCourse.DocumentLop = ms2.ToArray();
        //            }

        //            if (course.DocumentLop202627 != null && course.DocumentLop202627.Length > 0)
        //            {
        //                using var msLop = new MemoryStream();
        //                await course.DocumentLop202627.CopyToAsync(msLop);
        //                newCourse.DocumentLop202627 = msLop.ToArray();
        //            }

        //            if (course.DocumentNmc202627 != null && course.DocumentNmc202627.Length > 0)
        //            {
        //                using var msNmc = new MemoryStream();
        //                await course.DocumentNmc202627.CopyToAsync(msNmc);
        //                newCourse.DocumentNmc202627 = msNmc.ToArray();
        //            }

        //            await _context.CollegeCourseIntakeDetails.AddAsync(newCourse);
        //        }
        //        else
        //        {
        //            // UPDATE EXISTING COURSE
        //            // IMPORTANT: Do NOT touch dbCourse.RguhsIntake202526 here

        //            // 1) CollegeIntake202526 (nvarchar, from form)
        //            dbCourse.CollegeIntake202526 = course.CollegeIntake202526;

        //            // Mark ONLY CollegeIntake202526 as modified
        //            _context.Entry(dbCourse).Property(x => x.CollegeIntake202526).IsModified = true;

        //            // Explicitly ensure RguhsIntake202526 is not modified
        //            _context.Entry(dbCourse).Property(x => x.RguhsIntake202526).IsModified = false;

        //            // Freeze flag only for submitted level
        //            if (submitLevel == course.CourseLevel)
        //            {
        //                dbCourse.FreezeFlag = 1;
        //                _context.Entry(dbCourse).Property(x => x.FreezeFlag).IsModified = true;
        //            }

        //            // Existing & Present intake (if you want them editable)
        //            if (course.ExistingIntake.HasValue && course.ExistingIntake.Value > 0)
        //            {
        //                dbCourse.ExistingIntake = course.ExistingIntake.Value;
        //                _context.Entry(dbCourse).Property(x => x.ExistingIntake).IsModified = true;
        //            }

        //            if (course.PresentIntake.HasValue && course.PresentIntake.Value > 0)
        //            {
        //                dbCourse.PresentIntake = course.PresentIntake.Value;
        //                _context.Entry(dbCourse).Property(x => x.PresentIntake).IsModified = true;
        //            }

        //            // Documents – update only when new file uploaded
        //            if (course.Document1 != null && course.Document1.Length > 0)
        //            {
        //                using var ms1 = new MemoryStream();
        //                await course.Document1.CopyToAsync(ms1);
        //                dbCourse.DocumentAffiliation = ms1.ToArray();
        //                _context.Entry(dbCourse).Property(x => x.DocumentAffiliation).IsModified = true;
        //            }

        //            if (course.Document2 != null && course.Document2.Length > 0)
        //            {
        //                using var ms2 = new MemoryStream();
        //                await course.Document2.CopyToAsync(ms2);
        //                dbCourse.DocumentLop = ms2.ToArray();
        //                _context.Entry(dbCourse).Property(x => x.DocumentLop).IsModified = true;
        //            }

        //            if (course.DocumentLop202627 != null && course.DocumentLop202627.Length > 0)
        //            {
        //                using var msLop = new MemoryStream();
        //                await course.DocumentLop202627.CopyToAsync(msLop);
        //                dbCourse.DocumentLop202627 = msLop.ToArray();
        //                _context.Entry(dbCourse).Property(x => x.DocumentLop202627).IsModified = true;
        //            }

        //            if (course.DocumentNmc202627 != null && course.DocumentNmc202627.Length > 0)
        //            {
        //                using var msNmc = new MemoryStream();
        //                await course.DocumentNmc202627.CopyToAsync(msNmc);
        //                dbCourse.DocumentNmc202627 = msNmc.ToArray();
        //                _context.Entry(dbCourse).Property(x => x.DocumentNmc202627).IsModified = true;
        //            }
        //        }
        //    }

        //    // 7. Save course changes
        //    courseAffectedRows = await _context.SaveChangesAsync();

        //    // 8. Handle AllDocuments (PG level combined upload)
        //    if (AllDocuments != null && AllDocuments.Count > 0 && model.SelectedCourseLevel == "PG")
        //    {
        //        foreach (var file in AllDocuments)
        //        {
        //            if (file.Length > maxFileSize)
        //            {
        //                TempData["ValidationErrors"] = JsonSerializer.Serialize(
        //                    new List<string> { $"File {file.FileName} exceeds 2 MB." });
        //                return RedirectToAction("GetExpectedDetails",
        //                    new { collegeCode = model.CollegeCode, courseLevel = model.SelectedCourseLevel });
        //            }
        //            if (file.ContentType != "application/pdf")
        //            {
        //                TempData["ValidationErrors"] = JsonSerializer.Serialize(
        //                    new List<string> { $"File {file.FileName} must be PDF." });
        //                return RedirectToAction("GetExpectedDetails",
        //                    new { collegeCode = model.CollegeCode, courseLevel = model.SelectedCourseLevel });
        //            }
        //        }

        //        var college = await _context.AffiliationCollegeMasters
        //            .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

        //        if (college != null)
        //        {
        //            try
        //            {
        //                using var msZip = new MemoryStream();
        //                using (var archive = new ZipArchive(msZip, ZipArchiveMode.Create, true))
        //                {
        //                    foreach (var file in AllDocuments)
        //                    {
        //                        var zipEntry = archive.CreateEntry(file.FileName);
        //                        using var entryStream = zipEntry.Open();
        //                        await file.CopyToAsync(entryStream);
        //                    }
        //                }

        //                college.AllDocsForCourse = msZip.ToArray();
        //                _context.Entry(college).State = EntityState.Modified;
        //                await _context.SaveChangesAsync();
        //            }
        //            catch (Exception ex)
        //            {
        //                TempData["ValidationErrors"] = JsonSerializer.Serialize(
        //                    new List<string> { $"ZIP creation failed: {ex.Message}" });
        //                return RedirectToAction("GetExpectedDetails",
        //                    new { collegeCode = model.CollegeCode, courseLevel = model.SelectedCourseLevel });
        //            }
        //        }
        //    }

        //    // 9. Success message
        //    TempData["Success"] =
        //        $"Course details saved successfully! {courseAffectedRows} course record(s) updated with 2025-26 & 2026-27 intake data.";

        //    return RedirectToAction("GetExpectedDetails",
        //        new { collegeCode = model.CollegeCode, courseLevel = model.SelectedCourseLevel });
        //}


        //[HttpPost]
        //public async Task<IActionResult> GetExpectedDetails(CollegeCourseViewModel model)
        //{
        //    const long maxFileSize = 2 * 1024 * 1024; // 2 MB

        //    // Validate files before any saving
        //    foreach (var course in model.Courses)
        //    {
        //        if (course.Document1 == null || course.Document1.Length == 0)
        //        {
        //            ModelState.AddModelError("", $"Document1 is required for course {course.CourseName}.");
        //        }
        //        else
        //        {
        //            if (course.Document1.Length > maxFileSize)
        //                ModelState.AddModelError("", $"Document1 for {course.CourseName} exceeds 2 MB.");
        //            if (course.Document1.ContentType != "application/pdf")
        //                ModelState.AddModelError("", $"Document1 for {course.CourseName} must be a PDF.");
        //        }

        //        if (course.Document2 == null || course.Document2.Length == 0)
        //        {
        //            ModelState.AddModelError("", $"Document2 is required for course {course.CourseName}.");
        //        }
        //        else
        //        {
        //            if (course.Document2.Length > maxFileSize)
        //                ModelState.AddModelError("", $"Document2 for {course.CourseName} exceeds 2 MB.");
        //            if (course.Document2.ContentType != "application/pdf")
        //                ModelState.AddModelError("", $"Document2 for {course.CourseName} must be a PDF.");
        //        }
        //    }

        //    //if (!ModelState.IsValid)
        //    //{
        //    //    // Prepare errors and reload data
        //    //    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //    //    TempData["ValidationErrors"] = JsonSerializer.Serialize(errors);

        //    //    model.Courses = await _context.CollegeCourseIntakeDetails
        //    //        .Where(c => c.CollegeCode == model.CollegeCode)
        //    //        .Select(c => new CourseDetail
        //    //        {
        //    //            CourseName = c.CourseName,
        //    //            ExistingIntake = c.ExistingIntake ?? 0,
        //    //            PresentIntake = c.PresentIntake,
        //    //            IsDocument1Available = c.DocumentAffiliation != null && c.DocumentAffiliation.Length > 0,
        //    //            IsDocument2Available = c.DocumentLop != null && c.DocumentLop.Length > 0
        //    //        })
        //    //        .ToListAsync();

        //    //    model.DocumentUpload = await _context.AffiliationCollegeMasters
        //    //        .Where(e => e.CollegeCode == model.CollegeCode)
        //    //        .Select(e => new DocumentUploadViewModel
        //    //        {
        //    //            IsDocsALlUploaded = e.AllDocsForCourse != null && e.AllDocsForCourse.Length > 0,
        //    //            CollegeCode = e.CollegeCode
        //    //        })
        //    //        .FirstOrDefaultAsync();

        //    //    return View(model);
        //    //}

        //    // Now save/update records
        //    foreach (var course in model.Courses)
        //    {
        //        var dbCourse = await _context.CollegeCourseIntakeDetails
        //            .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode && c.CourseName == course.CourseName);

        //        if (dbCourse == null)
        //        {
        //            var newCourse = new CollegeCourseIntakeDetail
        //            {
        //                CollegeCode = model.CollegeCode,
        //                CourseName = course.CourseName,
        //                ExistingIntake = course.ExistingIntake ?? 0,
        //                PresentIntake = course.PresentIntake
        //            };

        //            if (course.Document1 != null && course.Document1.Length > 0)
        //            {
        //                using (var ms = new MemoryStream())
        //                {
        //                    await course.Document1.CopyToAsync(ms);
        //                    newCourse.DocumentAffiliation = ms.ToArray();
        //                }
        //            }

        //            if (course.Document2 != null && course.Document2.Length > 0)
        //            {
        //                using (var ms = new MemoryStream())
        //                {
        //                    await course.Document2.CopyToAsync(ms);
        //                    newCourse.DocumentLop = ms.ToArray();
        //                }
        //            }

        //            await _context.CollegeCourseIntakeDetails.AddAsync(newCourse);
        //        }
        //        else
        //        {
        //            dbCourse.PresentIntake = course.PresentIntake;

        //            if (course.Document1 != null && course.Document1.Length > 0)
        //            {
        //                using (var ms = new MemoryStream())
        //                {
        //                    await course.Document1.CopyToAsync(ms);
        //                    dbCourse.DocumentAffiliation = ms.ToArray();
        //                }
        //            }

        //            if (course.Document2 != null && course.Document2.Length > 0)
        //            {
        //                using (var ms = new MemoryStream())
        //                {
        //                    await course.Document2.CopyToAsync(ms);
        //                    dbCourse.DocumentLop = ms.ToArray();
        //                }
        //            }

        //            _context.Entry(dbCourse).State = EntityState.Modified;
        //        }
        //    }

        //    int affectedRows = await _context.SaveChangesAsync();
        //    if (affectedRows == 0)
        //    {
        //        throw new Exception("SaveChangesAsync returned 0. Nothing was saved.");
        //    }

        //    TempData["Success"] = "Details saved successfully!";
        //    return RedirectToAction("GetExpectedDetails", new { collegeCode = model.CollegeCode });
        //}
        //[HttpGet]
        //public async Task<IActionResult> ViewDocument(string collegeCode, string courseName, string documentType)
        //{
        //    if (string.IsNullOrWhiteSpace(collegeCode) ||
        //        string.IsNullOrWhiteSpace(courseName) ||
        //        string.IsNullOrWhiteSpace(documentType))
        //    {
        //        return BadRequest("Invalid request parameters.");
        //    }

        //    // courseName is actually CourseCode in your links
        //    var course = await _context.CollegeCourseIntakeDetails
        //        .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode && c.CourseCode == courseName);

        //    if (course == null)
        //    {
        //        return NotFound();
        //    }

        //    byte[] fileBytes = null;
        //    string fileName = "document.pdf";
        //    const string contentType = "application/pdf";

        //    switch (documentType)
        //    {
        //        case "Document1":   // RGUHS Affiliation Notification
        //            if (course.DocumentAffiliation != null && course.DocumentAffiliation.Length > 0)
        //            {
        //                fileBytes = course.DocumentAffiliation;
        //                fileName = "RGUHS_Affiliation.pdf";
        //            }
        //            break;

        //        case "Document2":   // NMC LOP (Current)
        //            if (course.DocumentLop != null && course.DocumentLop.Length > 0)
        //            {
        //                fileBytes = course.DocumentLop;
        //                fileName = "NMC_LOP_Current.pdf";
        //            }
        //            break;

        //        case "DocumentLop202627":   // LOP 2026-27
        //            if (course.DocumentLop202627 != null && course.DocumentLop202627.Length > 0)
        //            {
        //                fileBytes = course.DocumentLop202627;
        //                fileName = "LOP_2026_27.pdf";
        //            }
        //            break;

        //        case "DocumentNmc202627":   // NMC 2026-27
        //            if (course.DocumentNmc202627 != null && course.DocumentNmc202627.Length > 0)
        //            {
        //                fileBytes = course.DocumentNmc202627;
        //                fileName = "NMC_2026_27.pdf";
        //            }
        //            break;

        //        default:
        //            return BadRequest("Unknown document type.");
        //    }

        //    if (fileBytes == null)
        //    {
        //        return NotFound("Document not found.");
        //    }

        //    return File(fileBytes, contentType, fileName);
        //}

        [HttpPost]
        public async Task<IActionResult> UploadAllDocuments(string CollegeCode, List<IFormFile> AllDocuments)
        {
            const long maxFileSize = 2 * 1024 * 1024; // 2 MB

            if (string.IsNullOrEmpty(CollegeCode))
            {
                TempData["ValidationErrors"] = JsonSerializer.Serialize(new List<string> { "College Code is required." });
                return RedirectToAction("GetExpectedDetails");
            }

            if (AllDocuments == null || AllDocuments.Count == 0)
            {
                TempData["ValidationErrors"] = JsonSerializer.Serialize(new List<string> { "Please select at least one document to upload." });
                return RedirectToAction("GetExpectedDetails");
            }

            // Validate file size and type for each document
            foreach (var file in AllDocuments)
            {
                if (file.Length > maxFileSize)
                {
                    TempData["ValidationErrors"] = JsonSerializer.Serialize(new List<string> { $"File {file.FileName} exceeds the maximum size of 2 MB." });
                    return RedirectToAction("GetExpectedDetails");
                }
                if (file.ContentType != "application/pdf")
                {
                    TempData["ValidationErrors"] = JsonSerializer.Serialize(new List<string> { $"File {file.FileName} must be a PDF." });
                    return RedirectToAction("GetExpectedDetails");
                }
            }

            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(c => c.CollegeCode == CollegeCode);

            if (college == null)
            {
                TempData["ValidationErrors"] = JsonSerializer.Serialize(new List<string> { "College not found." });
                return RedirectToAction("GetExpectedDetails");
            }

            try
            {
                // Create ZIP archive in memory
                using var msZip = new MemoryStream();
                using (var archive = new ZipArchive(msZip, ZipArchiveMode.Create, true))
                {
                    foreach (var file in AllDocuments)
                    {
                        var zipEntry = archive.CreateEntry(file.FileName);
                        using var entryStream = zipEntry.Open();
                        await file.CopyToAsync(entryStream);
                    }
                }

                // Save ZIP bytes into AllDocsForCourse column
                college.AllDocsForCourse = msZip.ToArray();
                _context.Entry(college).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{AllDocuments.Count} documents uploaded and saved successfully as a ZIP archive!";
                return RedirectToAction("GetExpectedDetails", new { collegeCode = CollegeCode });
            }
            catch (Exception ex)
            {
                TempData["ValidationErrors"] = JsonSerializer.Serialize(new List<string> { $"Failed to create ZIP archive: {ex.Message}" });
                return RedirectToAction("GetExpectedDetails");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewDocument1(string collegeCode, string documentType)
        {
            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);

            if (college == null)
            {
                return NotFound("College not found.");
            }

            if (documentType == "AllDocuments"
                && college.AllDocsForCourse != null
                && college.AllDocsForCourse.Length > 0)
            {
                // Use octet-stream for download
                return File(college.AllDocsForCourse, "application/octet-stream", "AllDocs.zip");
            }

            return NotFound("Document not found.");
        }



        //[HttpGet]
        //public async Task<IActionResult> ViewDocument1(string collegeCode, string documentType)
        //{
        //    var course = await _context.AffiliationCollegeMasters
        //        .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);

        //    if (course == null)
        //    {
        //        return NotFound();
        //    }

        //    byte[] fileBytes = null;
        //    string fileName = "";
        //    string contentType = "application/pdf"; // or "application/octet-stream"

        //    if (documentType == "AllDocuments" && course.AllDocsForCourse != null)
        //    {
        //        fileBytes = course.AllDocsForCourse;
        //        fileName = "AllDocs.pdf";
        //    }


        //    if (fileBytes == null)
        //    {
        //        return NotFound("Document not found.");
        //    }

        //    return File(fileBytes, contentType, fileName);
        //}

        [HttpGet]
        public async Task<IActionResult> AHSCollegeVerification()
        {
            try
            {
                var model = new CollegeVerificationViewModel();

                // College & Faculty codes from Session
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCodeStr))
                {
                    TempData["Error"] = "College or Faculty information is missing in session.";
                    return RedirectToAction("Index", "Home"); // or any fallback page
                }

                model.CollegeCode = collegeCode;
                model.FacultyCode = Convert.ToInt32(facultyCodeStr);

                // Fetch college details (name + docs)
                var collegeDetails = await _context.AffiliationCollegeMasters
                    .Where(c => c.CollegeCode == model.CollegeCode)
                    .Select(c => new
                    {
                        c.CollegeName,
                        c.CollegeTown,
                        c.AllDocsForCourse
                    })
                    .FirstOrDefaultAsync();

                if (collegeDetails != null)
                {
                    model.CollegeName = collegeDetails.CollegeName;
                    model.CollegeTown = collegeDetails.CollegeTown;
                    model.AllDocsForCourse = collegeDetails.AllDocsForCourse;
                }

                // Faculty Name
                model.FacultyName = await _context.Faculties
                    .Where(f => f.FacultyId == model.FacultyCode)
                    .Select(f => f.FacultyName)
                    .FirstOrDefaultAsync();

                // Load document for course
                model.AllDocsForCourse = await _context.AffiliationCollegeMasters
                    .Where(e => e.CollegeCode == collegeCode)
                    .Select(e => e.AllDocsForCourse)
                    .FirstOrDefaultAsync();

                // Load distinct course details
                var distinctCourses = await _context.CollegeCourseIntakeDetails
                    .Where(c => c.CollegeCode == model.CollegeCode)
                    .GroupBy(c => new { c.CourseCode, c.CourseName, c.ExistingIntake, c.PresentIntake })
                    .Select(g => new
                    {
                        g.Key.CourseCode,
                        g.Key.CourseName,
                        g.Key.ExistingIntake,
                        g.Key.PresentIntake
                    })
                    .ToListAsync();

                // Assign serial numbers
                model.Courses = distinctCourses
                    .Select((x, index) => new CollegeVerificationCourseViewModel
                    {
                        SlNo = index + 1,
                        CourseCode = x.CourseCode,
                        CourseName = x.CourseName,
                        SanctionedIntakeFirstYear = x.ExistingIntake,
                        PresentIntake = x.PresentIntake,
                        Remarks = ""
                    })
                    .ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception (replace with your logging mechanism)
                Console.WriteLine(ex); // or use ILogger

                TempData["Error"] = "An error occurred while loading the college verification page. Please try again later.";

                // Optionally, redirect to an error page or fallback
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AHSCollegeVerification(CollegeVerificationViewModel model)
        {
            // Get FacultyCode from session/TempData
            if (HttpContext.Session.GetString("FacultyCode") != null)
                model.FacultyCode = Convert.ToInt32(HttpContext.Session.GetString("FacultyCode"));
            else if (TempData["FacultyCode"] != null)
                model.FacultyCode = Convert.ToInt32(TempData["FacultyCode"]);

            // Get CollegeCode from session/TempData
            if (HttpContext.Session.GetString("CollegeCode") != null)
                model.CollegeCode = HttpContext.Session.GetString("CollegeCode");
            else if (TempData["CollegeCode"] != null)
                model.CollegeCode = TempData["CollegeCode"].ToString();

            // Get CollegeName from session
            if (HttpContext.Session.GetString("CollegeName") != null)
                model.CollegeName = HttpContext.Session.GetString("CollegeName");

            // Get FacultyName from session
            if (HttpContext.Session.GetString("FacultyName") != null)
                model.FacultyName = HttpContext.Session.GetString("FacultyName");

            // Save intake details
            foreach (var course in model.Courses)
            {
                var entity = await _context.CollegeCourseIntakeDetails
                    .FirstOrDefaultAsync(e => e.CollegeCode == model.CollegeCode
                                           && e.CourseName == course.CourseName);

                if (entity != null)
                {
                    // Update existing
                    entity.PresentIntake = course.PresentIntake;
                    _context.Update(entity);
                }
                else
                {
                    // Insert new
                    _context.CollegeCourseIntakeDetails.Add(new CollegeCourseIntakeDetail
                    {
                        CollegeCode = model.CollegeCode,
                        CollegeName = model.CollegeName,
                        FacultyCode = model.FacultyCode,
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        PresentIntake = course.RguhsIntake
                    });
                }
            }
            if (model.AllDocsForCourse1 != null && model.AllDocsForCourse1.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.AllDocsForCourse1.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var collegeMaster = await _context.AffiliationCollegeMasters
                        .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

                    if (collegeMaster != null)
                    {
                        collegeMaster.AllDocsForCourse = fileBytes;
                        _context.Update(collegeMaster);
                    }
                }
            }


            await _context.SaveChangesAsync();

            TempData["Success"] = "Verification data and documents saved successfully.";
            return RedirectToAction("AHSCollegeVerification");
        }


        [HttpGet]
        public async Task<IActionResult> ViewCollegeDocument(string collegeCode)
        {
            var document = await _context.AffiliationCollegeMasters
                .Where(c => c.CollegeCode == collegeCode)
                .Select(c => c.AllDocsForCourse)
                .FirstOrDefaultAsync();

            if (document == null)
                return NotFound();

            // Assuming it's PDF. Change MIME type if Word/Image/etc.
            return File(document, "application/pdf", "CollegeDocument.pdf");
        }

        //[HttpGet]
        //public async Task<IActionResult> DownloadCollegeDoc(string collegeCode)
        //{
        //    //var college = await _context.CollegeCourseIntakeDetails
        //    //    .Where(c => c.CollegeCode == collegeCode)
        //    //    .Select(c => new { c.DocumentAffiliation })
        //    //    .FirstOrDefaultAsync();

        //    //if (college == null || college.DocumentAffiliation == null || college.DocumentAffiliation.Length == 0)
        //    //{
        //    //    return NotFound("No document found for this college.");
        //    //}

        //    //// Send file as application/pdf (or application/octet-stream for generic)
        //    //return File(college.DocumentAffiliation, "application/pdf", "CollegeDocument.pdf");

        //    var college = await _context.CollegeCourseIntakeDetails
        //        .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);

        //    if (college == null)
        //    {
        //        return NotFound();
        //    }

        //    if (college.DocumentAffiliation != null)
        //    {
        //        return File(college.DocumentAffiliation, "application/zip", "AllDocs.zip");
        //    }

        //    return NotFound("Document not found.");
        //}

        [HttpGet]
        public async Task<IActionResult> DownloadCollegeDoc(string collegeCode)
        {
            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);

            if (college == null || college.AllDocsForCourse == null || college.AllDocsForCourse.Length == 0)
            {
                return NotFound("Document not found.");
            }

            // Create ZIP in memory
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Add PDF file to ZIP
                    var zipEntry = archive.CreateEntry("CollegeDocument.pdf", CompressionLevel.Fastest);
                    using (var entryStream = zipEntry.Open())
                    {
                        await entryStream.WriteAsync(college.AllDocsForCourse, 0, college.AllDocsForCourse.Length);
                    }
                }

                return File(memoryStream.ToArray(), "application/zip", "AllDocs.zip");
            }
        }

    }
}
