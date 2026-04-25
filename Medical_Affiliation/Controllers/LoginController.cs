//using Admission_Affiliation.Data;
using Azure;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medical_Affiliation.Controllers
{
    public class LoginController : Controller
    {
        private readonly DATA.ApplicationDbContext _context;

        public LoginController(DATA.ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Only fetch required columns
            var faculties = await _context.Faculties
                .AsNoTracking()
                .Select(f => new FacultyViewModel
                {
                    FacultyId = f.FacultyId.ToString(),
                    FacultyName = f.FacultyName,
                    EmsFacultyId = f.EmsFacultyId.ToString(),
                    FacultyAbbre = f.FacultyAbbre,
                    Status = f.Status
                })
                .ToListAsync();

            // Build the AdmissionLoginViewModel
            var model = new AdmissionLoginViewModel
            {
                Faculties = faculties
                    .Where(f => f.Status == "Active")
                    .Select(f => new SelectListItem
                    {
                        Value = f.FacultyId,
                        Text = f.FacultyName
                    })
                    .ToList(),

                Colleges = new List<SelectListItem>(), // initially empty
                CaptchaCode = GenerateCaptchaCode()
            };

            // Pass extra data to ViewBag
            ViewBag.Faculties = faculties;
            ViewBag.AffilitaionType = 2;
            //TempData["CaptchaCode"] = model.CaptchaCode;

            HttpContext.Session.SetString("CaptchaCode", model.CaptchaCode);

            return View(model);
        }

        [HttpGet]
        public JsonResult GenerateCaptcha()
        {
            string newCaptcha = GenerateCaptchaCode(); // this calls your private method

            HttpContext.Session.SetString("CaptchaCode", newCaptcha); //code added by DP on 22-04-2026
            return Json(new { newCaptcha });
        }

        //[HttpPost]
        //public IActionResult Login(AdmissionLoginViewModel model)
        //{
        //    model.Faculties = _context.Faculties
        //        .Where(f => f.Status == "Active")
        //        .OrderBy(f => f.FacultyName)
        //        .Select(f => new SelectListItem
        //        {
        //            Value = f.FacultyId.ToString(),
        //            Text = f.FacultyName ?? "Unnamed Faculty"
        //        })
        //        .ToList();


        //    model.Colleges = _context.AffiliationCollegeMasters
        //        .Where(c => c.FacultyCode == model.FacultyId)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CollegeCode.ToString(),
        //            Text = c.CollegeName
        //        }).ToList();

        //    if (!ModelState.IsValid)
        //    {
        //        model.CaptchaCode = GenerateCaptchaCode();
        //        TempData["CaptchaCode"] = model.CaptchaCode;
        //        return View(model);
        //    }

        //    if (model.Captcha != TempData["CaptchaCode"]?.ToString())
        //    {
        //        ModelState.AddModelError("Captcha", "Invalid Captcha.");
        //        model.CaptchaCode = GenerateCaptchaCode();
        //        TempData["CaptchaCode"] = model.CaptchaCode;
        //        return View(model);
        //    }

        //    // Validate user from your DB
        //    var user = _context.AffiliationCollegeMasters.FirstOrDefault(u =>
        //        u.FacultyCode == model.FacultyId &&
        //        u.CollegeCode == model.CollegeId &&
        //        u.CollegeCode == model.Username &&
        //        u.Password == model.Password);

        //    if (user == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid login credentials.");
        //        model.CaptchaCode = GenerateCaptchaCode();
        //        TempData["CaptchaCode"] = model.CaptchaCode;
        //        return View(model);
        //    }

        //    if (string.IsNullOrEmpty(user.ChangedPassword))
        //    {
        //        TempData["ShowWelcomePopup"] = true;
        //        TempData["CollegeName"] = user.CollegeName ?? "College";

        //        // Store session if needed
        //        HttpContext.Session.SetString("CollegeCode", user.CollegeCode);
        //        HttpContext.Session.SetString("CollegeName", user.CollegeName ?? "");

        //        return RedirectToAction("GetExpectedDetails", "Collegelogin", new
        //        {
        //            collegecode = user.CollegeCode,
        //            showPasswordPopup = true
        //        });

        //    }
        //    ViewBag.AffilitaionType = 2;
        //    var FacultyName = _context.Faculties.Where(e=>e.FacultyId.ToString()==user.FacultyCode).Select(e=>e.FacultyName).FirstOrDefault();
        //    // Redirect to dashboard
        //    return RedirectToAction("GetExpectedDetails", "Collegelogin" , new { collegecode = model.Username});
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out from every registered auth scheme
            await HttpContext.SignOutAsync("CollegeAuth");
            await HttpContext.SignOutAsync("AdminAuth");
            await HttpContext.SignOutAsync("SectionOfficerAuth");
            await HttpContext.SignOutAsync("LicInspectionAuth");
            await HttpContext.SignOutAsync("DirectorAuth");
            await HttpContext.SignOutAsync("DirectorAuth1");
            await HttpContext.SignOutAsync("LICSectionAuth");
            await HttpContext.SignOutAsync("FinanceAuth");

            // Clear session
            HttpContext.Session.Clear();

            // Clear TempData
            TempData.Clear();

            // Delete all cookies from the response
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie, new CookieOptions
                {
                    Path = "/",
                    SameSite = SameSiteMode.Lax
                });
            }

            // return RedirectToAction("Login", "Login");
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult KeepAlive()
        {
            HttpContext.Session.SetString("KeepAlive", DateTime.Now.ToString());
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdmissionLoginViewModel model)
        {
            // Reload dropdowns
            model.Faculties = _context.Faculties
                .Where(f => f.Status == "Active")
                .OrderBy(f => f.FacultyName)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName ?? "Unnamed Faculty"
                }).ToList();

            model.Colleges = _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode.ToString() == model.FacultyId)
                .OrderBy(c => c.CollegeName)
                .Select(c => new SelectListItem
                {
                    Value = c.CollegeCode.ToString(),
                    Text = c.CollegeName
                }).ToList();

            // Validations...
            var faculty = _context.Faculties.FirstOrDefault(f => f.FacultyId.ToString() == model.FacultyId);
            if (faculty == null)
            {
                SetCaptcha(model);
                TempData["LoginError"] = "Please select Faculty.";
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            var user = _context.AffiliationCollegeMasters.FirstOrDefault(u =>
                u.FacultyCode.ToString() == model.FacultyId && u.CollegeCode == model.CollegeId);

            if (user == null)
            {
                SetCaptcha(model);
                TempData["LoginError"] = "Invalid College.";
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            if (user.Password != model.Password)
            {
                SetCaptcha(model);
                TempData["LoginError"] = "Incorrect password.";
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            //if (model.Captcha != TempData["CaptchaCode"]?.ToString())
            var sessionCaptcha = HttpContext.Session.GetString("CaptchaCode");

            if (model.Captcha != sessionCaptcha)
            {
                SetCaptcha(model);
                TempData["LoginError"] = "Invalid captcha.";
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            // Get CourseLevel
            var courseLevel = await (from cc in _context.CollegeCourseIntakeDetails
                                     join cm in _context.MstCourses
                                     on cc.CourseCode equals cm.CourseCode.ToString()
                                     where cm.CourseLevel == "UG" && cc.CollegeCode == user.CollegeCode
                                     select cm.CourseLevel)
                                     .FirstOrDefaultAsync();

            var userIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.CollegeName ?? ""),
                new Claim(ClaimTypes.Role, "College"),
                new Claim("CollegeCode", user.CollegeCode ?? ""),
                new Claim("FacultyCode", user.FacultyCode ?? ""),
                new Claim("CourseLevel", courseLevel ?? ""),
                new Claim("UserIP", userIP ?? ""),
                new Claim("UserAgent", userAgent ?? "")
            };

            var identity = new ClaimsIdentity(claims, "CollegeAuth");
            var principal = new ClaimsPrincipal(identity);

            // Clear all other schemes
            await HttpContext.SignOutAsync("CollegeAuth");
            await HttpContext.SignOutAsync("SectionOfficerAuth");
            await HttpContext.SignOutAsync("AdminAuth");
            await HttpContext.SignOutAsync("LicInspectionAuth");
            await HttpContext.SignOutAsync("DirectorAuth");
            await HttpContext.SignOutAsync("DirectorAuth1");
            await HttpContext.SignOutAsync("LICSectionAuth");
            await HttpContext.SignOutAsync("FinanceAuth");

            // Sign in with persistent cookie
            // After successful validation...

            await HttpContext.SignInAsync("CollegeAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            Console.WriteLine("=== LOGIN SUCCESSFUL ===");
            Console.WriteLine($"Signed in with CollegeCode: {user.CollegeCode}");

            HttpContext.Session.SetString("CollegeCode", user.CollegeCode ?? "");
            HttpContext.Session.SetString("FacultyCode", user.FacultyCode ?? "");

            // Force cookie to be issued
            await HttpContext.SignInAsync("CollegeAuth", principal);

            return RedirectToAction("Dashboard", "Collegelogin");
        }

        private void SetCaptcha(AdmissionLoginViewModel model)
        {
            model.CaptchaCode = GenerateCaptchaCode();
            //TempData["CaptchaCode"] = model.CaptchaCode;
            HttpContext.Session.SetString("CaptchaCode", model.CaptchaCode); // ✅ FIX
        }

        //[HttpGet]
        //public JsonResult GetCollegesByFaculty(string facultyId)
        //{
        //    var colleges = _context.AffiliationCollegeMasters
        //        .Where(c => c.FacultyCode.ToString() == facultyId)
        //        .Select(c => new
        //        {
        //            Value = c.CollegeCode,
        //            Text = c.CollegeName,
        //            Code = c.CollegeCode
        //        })
        //        .OrderBy(c => c.Text)
        //        .ToList();

        //    return Json(colleges);
        //}

        private string GenerateCaptchaCode()
        {
            var chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [HttpPost]
        public async Task<IActionResult> FacultyToggle(int facultyId)
        {
            var faculty = await _context.Faculties.FindAsync(facultyId);

            if (faculty == null)
            {
                return NotFound(new { message = "Faculty not found." });
            }

            // Toggle logic
            faculty.Status = faculty.Status == "Active" ? "Inactive" : "Active";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                newStatus = faculty.Status,
                message = $"Faculty status changed to {faculty.Status}."
            });
        }


        public IActionResult Dashboard()
        {
            ViewBag.Username = HttpContext.Session.GetString("UserName");
            //ViewBag.Faculty = HttpContext.Session.GetString("FacultyName");
            ViewBag.College = HttpContext.Session.GetString("CollegeName");
            //ViewBag.FacultyCode = HttpContext.Session.GetInt32("FacultyCode");
            ViewBag.Faculty = HttpContext.Session.GetString("FacultyName");
            ViewBag.FacultyCode = HttpContext.Session.GetInt32("FacultyCode");


            ViewBag.TypeOfAffiliationList = _context.TypeOfAffiliations
                .Select(t => new SelectListItem
                {
                    Text = t.TypeDescription,
                    Value = t.TypeId.ToString(),
                    Selected = t.TypeId.ToString() == "2"
                }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Dashboard(int selectedTypeId)
        {
            var selectedType = _context.TypeOfAffiliations.FirstOrDefault(t => t.TypeId == selectedTypeId);

            if (selectedType != null)
            {
                ViewBag.SelectedType = selectedType.TypeDescription;
            }

            // Reload dropdown after post
            ViewBag.TypeOfAffiliationList = _context.TypeOfAffiliations
                .Select(t => new SelectListItem
                {
                    Text = t.TypeDescription,
                    Value = t.TypeId.ToString()
                }).ToList();

            return View();
        }

        public (string controller, string action) SideBarActionAndControllers(string facultyCode, out (string controller, string action) routeInfo)
        {
            switch (facultyCode)
            {
                case "1":
                    routeInfo = ("MedicalRepository", "Medical_BasicDetails");
                    break;

                case "2":
                    routeInfo = ("Aff_Dental", "Dental_BasicDetails");
                    break;


                case "3":
                    routeInfo = ("Aff_Nursing", "Nursing_BasicDetails");
                    //routeInfo = ("NursingContinuesAffiliationController", "Nursing_Courses");
                    break;

                case "4":
                    routeInfo = ("AyurvedhaRepository", "Ayurvedha_BasicDetails");
                    break;

                case "5":
                    routeInfo = ("Aff_Homeopathy", "Homeopathy_BasicDetails");
                    break;

                case "6":
                    routeInfo = ("Aff_Naturopathy", "Naturopathy_BasicDetails");
                    break;

                case "7":
                    routeInfo = ("Aff_Unani", "Unani_BasicDetails");
                    break;

                case "8":
                    routeInfo = ("Aff_Pharmacy", "Pharmacy_BasicDetails");
                    break;

                case "9":
                    routeInfo = ("Aff_AHS", "AHS_BasicDetails");
                    break;

                case "10":
                    routeInfo = ("Aff_PhysioTherapy", "PhysioTherapy_BasicDetails");
                    break;
                case "11":
                    routeInfo = ("Aff_SowaRigPai", "SowaRigPai_BasicDetails");
                    break;

                default:
                    routeInfo = ("Home", "Index");
                    break;
            }

            // Return the same tuple (controller, action)
            return routeInfo;
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        public async Task<IActionResult> NodalOfficersList()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");

            // Fetch all existing nodal officers
            var existingOfficers = await _context.NodalOfficerDetails.Where(e => e.CollegeCode == collegeCode).ToListAsync();
            HttpContext.Session.SetString("logoutController", "Login");
            var buttons = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).FirstOrDefaultAsync();

            var ShowNodalOfficer = buttons.ShowNodalOfficerDetails ? "true" : "false";
            var ShowIntakeDetails = buttons.ShowIntakeDetails ? "true" : "false";
            var ShowRepository = buttons.ShowRepositoryDetails ? "true" : "false";
            HttpContext.Session.SetString("ShowNodalOfficer", ShowNodalOfficer);
            HttpContext.Session.SetString("ShowIntakeDetails", ShowIntakeDetails);
            HttpContext.Session.SetString("ShowRepository", ShowRepository);


            // Master initiatives
            var masterInitiatives = await _context.InitiativeMasters.ToListAsync();

            // Faculty info from session
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var sidebarDetails = SideBarActionAndControllers(facultyCode, out var routeInfo);

            HttpContext.Session.SetString("sidebarActionMethod", sidebarDetails.action);
            HttpContext.Session.SetString("sidebarController", sidebarDetails.controller);

            var faculty = await _context.Faculties
                .Where(f => f.FacultyId.ToString() == facultyCode)
                .FirstOrDefaultAsync();


            // Prepare existing officers list with initiatives
            var officerDetailsList = new List<NodalOfficerDetailViewModel>();

            ViewBag.CollegeName = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).Select(e => e.CollegeName).FirstOrDefaultAsync();

            foreach (var officer in existingOfficers)
            {
                var officerInitiatives = _context.NodalOfficerInitiatives
                    .Where(noi => noi.NodalOfficerId == officer.Id);

                var initiativesForOfficer = await (
                    from im in _context.InitiativeMasters
                    join oi in officerInitiatives
                        on im.InitiativeId equals oi.InitiativeId into oiGroup
                    from subOi in oiGroup.DefaultIfEmpty()
                    orderby im.InitiativeId
                    select new InitiativeCheckboxViewModel
                    {
                        Id = im.InitiativeId,
                        Name = im.InitiativeName.Trim(),
                        IsSelected = subOi != null,
                        IsEditEnabled = subOi != null && subOi.IsEditable,
                        IsDisabled = false
                    }
                ).ToListAsync();



                officerDetailsList.Add(new NodalOfficerDetailViewModel
                {
                    Id = officer.Id,
                    FacultyCode = officer.FacultyCode,
                    FacultyName = officer.FacultyName,
                    CollegeCode = officer.CollegeCode,
                    CollegeName = officer.CollegeName,
                    NodalOfficerName = officer.NodalOfficerName.Trim(),
                    PhoneNumber = officer.PhoneNumber,
                    EmailId = officer.EmailId,
                    InitiativeList = initiativesForOfficer
                });
            }

            // 🚀 For NEW officer in modal
            var assignedForCollege = await (
                from oi in _context.NodalOfficerInitiatives
                join no in _context.NodalOfficerDetails on oi.NodalOfficerId equals no.Id
                where no.CollegeCode == collegeCode
                select oi.InitiativeId
            ).ToListAsync();

            var newOfficer = new NodalOfficerDetailViewModel
            {
                FacultyCode = faculty?.FacultyId.ToString(),
                FacultyName = faculty?.FacultyName ?? "",
                CollegeCode = collegeCode,
                CollegeName = HttpContext.Session.GetString("CollegeName") ?? "",
                InitiativeList = masterInitiatives.Select(m => new InitiativeCheckboxViewModel
                {
                    Id = m.InitiativeId.ToString(),
                    Name = m.InitiativeName,
                    // ✅ Already assigned? then check it
                    IsSelected = assignedForCollege.Contains(m.InitiativeId.ToString()),
                    // ✅ And lock it for new officer
                    IsDisabled = assignedForCollege.Contains(m.InitiativeId.ToString())
                }).ToList()
            };

            var freeInitiatives = await _context.InitiativeMasters
                .Where(im => !_context.NodalOfficerInitiatives
                    .Join(_context.NodalOfficerDetails,
                          noi => noi.NodalOfficerId,
                          nod => nod.Id,
                          (noi, nod) => new { noi, nod })
                    .Any(x => x.noi.InitiativeId == im.InitiativeId
                           && x.nod.CollegeCode == collegeCode))
                .ToListAsync();






            var model = new NodalOfficersList
            {
                NodalOfficerDetail = officerDetailsList, // only DB officers
                NewOfficer = newOfficer,                 // only for modal
                MasterInitiatives = masterInitiatives,
                FreeInitiatives = freeInitiatives,
            };

            //var user = await _context.AffiliationCollegeMasters.Where(e=>e.CollegeCode==collegeCode).FirstOrDefaultAsync();
            //if (string.IsNullOrEmpty(user.ChangedPassword))
            //{
            //    TempData["ShowWelcomePopup"] = true;
            //    TempData["CollegeName"] = user.CollegeName ?? "College";
            //}

            return View(model);
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        public async Task<IActionResult> NodalOfficerDetails()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = HttpContext.Session.GetString("CollegeName");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                // Session expired or not set, redirect to login or error
                return RedirectToAction("Login", "Login");
            }

            // Get faculty name
            var facultyName = await _context.Faculties
                .Where(e => e.FacultyId.ToString() == facultyCode)
                .Select(e => e.FacultyName)
                .FirstOrDefaultAsync();

            var courses = await (from c in _context.CollegeCourseIntakeDetails
                                 join m in _context.MstCourses
                                     on c.CourseCode equals m.CourseCode.ToString()
                                 where c.CollegeCode == collegeCode
                                 select new CourseDetail
                                 {
                                     CourseCode = c.CourseCode,
                                     CourseName = m.CourseName,
                                     ExistingIntake = c.ExistingIntake ?? 0,
                                     PresentIntake = c.PresentIntake,
                                     IsDocument1Available = c.DocumentAffiliation != null && c.DocumentAffiliation.Length > 0,
                                     IsDocument2Available = c.DocumentLop != null && c.DocumentLop.Length > 0,
                                     freezeStatus = c.FreezeFlag == 1
                                 })
            .ToListAsync();
            var submitStatus = courses.Any(c => c.freezeStatus); // true if at least one course not frozen
            ViewBag.IsSubmitDisplay = submitStatus;
            HttpContext.Session.SetString("IsSubmitDisplay", submitStatus.ToString());

            // Check if a record already exists for this college
            var existing = await _context.NodalOfficerDetails
                .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode);

            ViewBag.TypeOfAffiliationList = _context.TypeOfAffiliations
                .Select(t => new SelectListItem
                {
                    Text = t.TypeDescription,
                    Value = t.TypeId.ToString(),
                    Selected = t.TypeId.ToString() == "2"
                }).ToList();

            var selectedInitiatives = new List<string>();
            if (existing != null)
            {
                selectedInitiatives = await _context.NodalOfficerInitiatives
                    .Where(e => e.NodalOfficerId == existing.Id)
                    .Select(e => e.InitiativeId)
                    .ToListAsync();
            }

            var initiatives = await _context.InitiativeMasters.ToListAsync();


            var viewModel = new NodalOfficerDetailViewModel
            {
                FacultyCode = facultyCode,
                FacultyName = facultyName,
                CollegeCode = collegeCode,
                CollegeName = collegeName,

                // If exists, fill details; otherwise leave empty for new entry
                NodalOfficerName = existing?.NodalOfficerName,
                PhoneNumber = existing?.PhoneNumber,
                EmailId = existing?.EmailId,

                FacultyList = await _context.Faculties
                    .Select(f => new SelectListItem
                    {
                        Value = f.FacultyId.ToString(),
                        Text = f.FacultyName,
                        Selected = (f.FacultyId.ToString() == facultyCode)
                    }).ToListAsync(),

                CollegeList = await _context.AffiliationCollegeMasters
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = c.CollegeName,
                        Selected = (c.CollegeCode == collegeCode)
                    }).ToListAsync(),

                InitiativeList = initiatives.Select(i => new InitiativeCheckboxViewModel
                {
                    Id = i.InitiativeId,
                    Name = i.InitiativeName,
                    IsSelected = selectedInitiatives.Contains(i.InitiativeId)

                }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeclarePrincipalAjax([FromBody] PrincipalDeclarationModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PrincipalName) || model.IsDeclared != "Y")
                return BadRequest(new { success = false, message = "Please enter the Principal/Dean name and accept the declaration." });

            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            var college = await _context.AffiliationCollegeMasters
                                        .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);

            if (college != null)
            {
                college.PrincipalNameDeclared = model.PrincipalName;
                college.IsDeclared = model.IsDeclared;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, message = "Principal/Dean name declared successfully." });
        }




        [HttpPost]
        public async Task<IActionResult> NodalOfficerDetails(NodalOfficerDetailViewModel model)
        {
            try
            {
                var facultyName = await _context.Faculties
                    .Where(e => e.FacultyId.ToString() == model.FacultyCode)
                    .Select(e => e.FacultyName)
                    .FirstOrDefaultAsync();

                var collegeName = await _context.AffiliationCollegeMasters
                    .Where(e => e.CollegeCode == model.CollegeCode)
                    .Select(e => e.CollegeName)
                    .FirstOrDefaultAsync();

                var existing = await _context.NodalOfficerDetails
                    .FirstOrDefaultAsync(e => e.CollegeCode == model.CollegeCode);

                NodalOfficerDetail officerEntity = null;

                if (model.Id > 0)
                {
                    // Updating existing officer
                    officerEntity = await _context.NodalOfficerDetails
                        .FirstOrDefaultAsync(e => e.Id == model.Id);

                    if (officerEntity != null)
                    {
                        officerEntity.FacultyCode = model.FacultyCode;
                        officerEntity.FacultyName = facultyName.Trim();
                        officerEntity.CollegeCode = model.CollegeCode;
                        officerEntity.CollegeName = collegeName;
                        officerEntity.NodalOfficerName = model.NodalOfficerName.Trim();
                        officerEntity.PhoneNumber = model.PhoneNumber.Trim();
                        officerEntity.EmailId = model.EmailId.Trim();
                        _context.NodalOfficerDetails.Update(officerEntity);
                    }
                }
                else
                {
                    // Insert new officer
                    officerEntity = new NodalOfficerDetail
                    {
                        FacultyCode = model.FacultyCode,
                        FacultyName = facultyName.Trim(),
                        CollegeCode = model.CollegeCode,
                        CollegeName = collegeName.Trim(),
                        NodalOfficerName = model.NodalOfficerName.Trim(),
                        PhoneNumber = model.PhoneNumber.Trim(),
                        EmailId = model.EmailId.Trim()
                    };
                    _context.NodalOfficerDetails.Add(officerEntity);
                }


                await _context.SaveChangesAsync();

                var selectedIds = model.InitiativeList.Where(i => i.IsSelected).Select(i => i.Id).ToList();

                var existingInitiatives = await _context.NodalOfficerInitiatives.Where(e => e.NodalOfficerId == officerEntity.Id).ToListAsync();

                // --- Add new ones ---
                var toAdd = selectedIds.Except(existingInitiatives.Select(e => e.InitiativeId)).ToList();
                foreach (var initiativeId in toAdd)
                {
                    _context.NodalOfficerInitiatives.Add(new NodalOfficerInitiative
                    {
                        NodalOfficerId = officerEntity.Id,
                        InitiativeId = initiativeId,
                        NodalOfficerName = officerEntity.NodalOfficerName
                    });
                }

                // --- Remove unchecked ones ---
                var toRemove = existingInitiatives.Where(e => !selectedIds.Contains(e.InitiativeId)).ToList();

                if (toRemove.Count > 0)
                {
                    _context.NodalOfficerInitiatives.RemoveRange(toRemove);
                }

                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] = "Nodal Officer details saved successfully!";
                return RedirectToAction("NodalOfficersList");

            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Something went wrong. Please try again.";
                return RedirectToAction("NodalOfficersList");
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> Logout()
        //{
        //    // Sign out the authentication cookie
        //    await HttpContext.SignOutAsync("CollegeAuth");

        //    // Clear all session values
        //    HttpContext.Session.Clear();
        //    TempData.Clear();

        //    // Redirect to login page
        //    return RedirectToAction("Login");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNodalOfficer(NodalOfficerDetailViewModel model)
        {

            // Get the officer from DB
            var officer = _context.NodalOfficerDetails
                                  .FirstOrDefault(o => o.Id == model.Id);

            if (officer == null)
                return NotFound();

            // Update officer basic info
            officer.NodalOfficerName = model.NodalOfficerName;
            officer.PhoneNumber = model.PhoneNumber;
            officer.EmailId = model.EmailId;

            _context.SaveChanges();

            // Update initiatives
            if (model.InitiativeList != null)
            {
                // Get all initiatives for this officer
                var existingInitiatives = _context.NodalOfficerInitiatives
                                                  .Where(i => i.NodalOfficerId == model.Id)
                                                  .ToList();

                foreach (var initiative in model.InitiativeList)
                {
                    var existing = existingInitiatives.FirstOrDefault(i => i.InitiativeId == initiative.Id);

                    if (existing == null && initiative.IsSelected)
                    {
                        _context.NodalOfficerInitiatives.Add(new NodalOfficerInitiative
                        {
                            NodalOfficerId = model.Id,
                            InitiativeId = initiative.Id,
                            NodalOfficerName = model.NodalOfficerName,
                            IsEditable = false
                        });
                    }
                    else if (existing != null && !initiative.IsSelected)
                    {
                        _context.NodalOfficerInitiatives.Remove(existing);
                    }

                }


                _context.SaveChanges();
            }

            return RedirectToAction("NodalOfficersList");
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePassword model)
        {
            var college = await _context.AffiliationCollegeMasters.Where(c => c.CollegeCode == model.CollegeCode).FirstOrDefaultAsync();

            if (college == null) return NotFound();
            college.Password = model.UpdatedPassword;
            college.ChangedPassword = model.UpdatedPassword;
            var passwordHasher = new PasswordHasher<AffiliationCollegeMaster>();
            college.HashedPassword = passwordHasher.HashPassword(college, college.Password);

            await _context.SaveChangesAsync();

            TempData["ChangedPassword"] = "Password updated successfully";


            return RedirectToAction("NodalOfficersList");

        }


        [HttpPost]
        public async Task<IActionResult> DeleteNodalOfficer(NodalOfficerDetailViewModel model)
        {
            var nodalOfficerDetail = await _context.NodalOfficerDetails.FindAsync(model.Id);

            if (nodalOfficerDetail == null) return NotFound();

            var nodalOfficerInitiativeDetails = await _context.NodalOfficerInitiatives.Where(e => e.NodalOfficerId == model.Id).ToListAsync();             
            if (nodalOfficerInitiativeDetails.Any())
            {
                _context.NodalOfficerInitiatives.RemoveRange(nodalOfficerInitiativeDetails);
            }

            _context.NodalOfficerDetails.Remove(nodalOfficerDetail);

            await _context.SaveChangesAsync();
            TempData["DeleteNodalOfficer"] = "Nodal Officer deleted successfully";
            return RedirectToAction("NodalOfficersList");

        }
    }

}
