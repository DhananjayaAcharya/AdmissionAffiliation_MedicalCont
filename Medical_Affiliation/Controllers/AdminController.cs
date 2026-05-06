using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Medical_Affiliation.DATA;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Net;
using Admission_Affiliation.Models;

namespace Admission_Affiliation.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<TblRguhsFacultyUser> _passwordHasher;


        public AdminController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<TblRguhsFacultyUser>();
        }

        public IActionResult UniversityLogin()
        {
            return RedirectToAction("AdminLogin");
        }

        public IActionResult AdminLogin()
        {
            HttpContext.SignOutAsync("AdminAuth");
            HttpContext.Session.Clear();
            //var viewModel = new AdminLoginViewModel();
            //viewModel.CaptchaString = GenerateCaptchaCode();
            //HttpContext.Session.SetString("CaptchaCode", viewModel.CaptchaString);

            return View();
        }
        //private string GenerateCaptchaCode()
        //{
        //    var chars = "0123456789";
        //    var random = new Random();
        //    return new string(Enumerable.Repeat(chars, 5)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());
        //}

        //[HttpGet]
        //public IActionResult GenerateCaptcha()
        //{
        //    var newCaptcha = GenerateCaptchaCode();
        //    HttpContext.Session.SetString("CaptchaCode", newCaptcha);

        //    return Json(new { captchaString = newCaptcha });
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for Fellowship Section Officer and Director hardcoded credentials
            var username = (model.UserName ?? string.Empty).Trim();
            var isFellowshipSo = string.Equals(username, "Fellowship_SO", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(username, "Felloeship_SO", StringComparison.OrdinalIgnoreCase);
            var user = await _context.TblRguhsFacultyUsers.Where(e => e.UserName.ToLower() == model.UserName.ToLower()).FirstOrDefaultAsync();

            if (isFellowshipSo && model.Password == "Fellowship@SO")
            {
                var claimsSO = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Fellowship_SO"),
                    new Claim(ClaimTypes.Role, "Director"),
                    new Claim("FacultyId", "12"),
                    new Claim("UserIP", HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty),
                    new Claim("UserAgent", HttpContext.Request.Headers["User-Agent"].ToString() ?? string.Empty)
                };

                var identitySO = new ClaimsIdentity(claimsSO, "DirectorAuth");
                var principalSO = new ClaimsPrincipal(identitySO);

                await HttpContext.SignInAsync("DirectorAuth", principalSO,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                HttpContext.Session.SetString("logoutController", "Director");
                HttpContext.Session.SetString("FacultyCode", "12");
                HttpContext.Session.SetString("FacultyId", "12");
                HttpContext.Session.SetString("IsDirector", "true");
                //HttpContext.Session.SetString("IsAdmin", user.IsAdmin?.ToString() ?? "false");

                return RedirectToAction("AdminDashboard_Fellowship", "Fellowship");
            }

            // Check for Fellowship Director hardcoded credentials
            if (model.UserName == "Fellowship_DR" && model.Password == "Fellowship@DR")
            {
                var claimsDR = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Fellowship_DR"),
                    new Claim(ClaimTypes.Role, "Director"),
                    new Claim("FacultyId", "99"),
                    new Claim("UserIP", HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty),
                    new Claim("UserAgent", HttpContext.Request.Headers["User-Agent"].ToString() ?? string.Empty)
                };

                var identityDR = new ClaimsIdentity(claimsDR, "DirectorAuth");
                var principalDR = new ClaimsPrincipal(identityDR);

                await HttpContext.SignInAsync("DirectorAuth", principalDR,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                HttpContext.Session.SetString("logoutController", "Director");
                HttpContext.Session.SetString("FacultyCode", "99");
                HttpContext.Session.SetString("FacultyId", "99");
                HttpContext.Session.SetString("IsDirector", "true");

                // Redirect the hardcoded Fellowship_DR account directly to the Director dashboard
                return RedirectToAction("DirectorDashboard_Fellowship", "Fellowship");
            }

            var checkFinance = await _context.TblRguhsFacultyUsers
    .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.IsFinance == true);

            if (checkFinance != null)
            {
                // ── Verify password ───────────────────────────────────────────────────
                bool isFinancePasswordValid = false;

                if (!string.IsNullOrEmpty(checkFinance.PasswordHash) &&
                    (checkFinance.PasswordHash.StartsWith("$2a$") ||
                     checkFinance.PasswordHash.StartsWith("$2b$") ||
                     checkFinance.PasswordHash.StartsWith("$2y$")))
                {
                    try
                    {
                        string storedHash = checkFinance.PasswordHash.Replace("$2y$", "$2a$");
                        isFinancePasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, storedHash);
                    }
                    catch { isFinancePasswordValid = false; }
                }
                else
                {
                    var ph = new PasswordHasher<TblRguhsFacultyUser>();
                    var result = ph.VerifyHashedPassword(checkFinance, checkFinance.PasswordHash, model.Password);
                    isFinancePasswordValid = result == PasswordVerificationResult.Success
                                          || result == PasswordVerificationResult.SuccessRehashNeeded;
                }

                if (!isFinancePasswordValid)
                {
                    ModelState.AddModelError("Password", "The password you entered is incorrect.");
                    return View(model);
                }

                // ── Determine role from FinanceDesignation ────────────────────────────
                string designation = (checkFinance.FinanceDesignation ?? "").Trim().ToUpper();

                string licRole = designation switch
                {
                    "FO" => "Finance",
                    "CW" => "CaseWorker",
                    "AO" => "AO",
                    "CS" => "Cashier",
                    _ => "Finance"   // fallback
                };

                // ── Sign in ───────────────────────────────────────────────────────────
                var claimsFinance = new List<Claim>
{
    new Claim(ClaimTypes.Name,  checkFinance.UserName),
    new Claim(ClaimTypes.Role,  licRole),
    new Claim("FacultyId",      checkFinance.Faculty.ToString()),
    new Claim("Designation",    checkFinance.FinanceDesignation ?? ""),
    new Claim("UserIP",         HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""),
    new Claim("UserAgent",      HttpContext.Request.Headers["User-Agent"].ToString() ?? "")
};

                var identityFinance = new ClaimsIdentity(claimsFinance, "FinanceAuth");
                var principalFinance = new ClaimsPrincipal(identityFinance);

                await HttpContext.SignInAsync("FinanceAuth", principalFinance,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                // ── Session ───────────────────────────────────────────────────────────
                HttpContext.Session.SetString("logoutController", "Finance");
                HttpContext.Session.SetString("FacultyCode", checkFinance.Faculty.ToString());
                HttpContext.Session.SetString("FacultyId", checkFinance.Faculty.ToString());
                HttpContext.Session.SetString("UserName", checkFinance.UserName);
                HttpContext.Session.SetString("FinanceDesignation", designation);
                HttpContext.Session.SetString("FinanceRole", licRole);

                // Individual flags — read by LicTadaController.GetRole()
                HttpContext.Session.SetString("IsFinance", licRole == "Finance" ? "true" : "false");
                HttpContext.Session.SetString("IsCaseWorker", licRole == "CaseWorker" ? "true" : "false");
                HttpContext.Session.SetString("IsAO", licRole == "AO" ? "true" : "false");
                HttpContext.Session.SetString("IsCashier", licRole == "Cashier" ? "true" : "false");

                return RedirectToAction("Dashboard", "LicTada");
            }



            // Existing user lookup from database
            //var user = await _context.TblRguhsFacultyUsers
            //    .FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("UserName", "The username you entered is incorrect.");
                return View(model);
            }

            HttpContext.Session.SetString("logoutController", "Admin");
            HttpContext.Session.SetString("FacultyCode", user.Faculty.ToString());
            HttpContext.Session.SetString("FacultyId", user.Faculty.ToString());
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin?.ToString() ?? "false");

            // existing passwordCreds/coursesList logic...
            var passwordCreds = await _context.AffiliationCollegeMasters
                .Where(e => string.IsNullOrEmpty(e.Password) || string.IsNullOrEmpty(e.HashedPassword))
                .ToListAsync();

            var coursesList = await _context.CollegeCourseIntakeDetails
                .Where(e => e.CollegeCode == null)
                .ToListAsync();

            foreach (var pass in passwordCreds)
            {
                if (string.IsNullOrEmpty(pass.FacultyCode))
                    pass.FacultyCode = "1";
                if (!string.IsNullOrEmpty(pass.Password) && string.IsNullOrEmpty(pass.HashedPassword))
                {
                    var passwordHasher = new PasswordHasher<AffiliationCollegeMaster>();
                    pass.HashedPassword = passwordHasher.HashPassword(pass, pass.Password);
                }
                else if (string.IsNullOrEmpty(pass.Password))
                {
                    pass.Password = GenerateRandomPassword(10);
                    var passwordHasher = new PasswordHasher<AffiliationCollegeMaster>();
                    pass.HashedPassword = passwordHasher.HashPassword(pass, pass.Password);
                }
            }

            foreach (var course in coursesList)
            {
                if (string.IsNullOrEmpty(course.CollegeCode))
                {
                    var courses = await _context.CollegeCourseIntakeDetails
                        .Where(e => e.CollegeName == course.CollegeName)
                        .FirstOrDefaultAsync();
                    course.CollegeCode = courses.CollegeCode;
                }
            }

            await _context.SaveChangesAsync();

            var userIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // ----- SPECIAL CASE: Fellowship Director (Faculty == 12) -----
            // If the authenticated user belongs to faculty 12, validate credentials and sign in using DirectorAuth,
            // then redirect to the Fellowship AdminDashboard_Fellowship.
            if (user.Faculty == 12)
            {
                bool isPasswordValidForFellowship = false;

                if (!string.IsNullOrEmpty(user.PasswordHash) &&
                    (user.PasswordHash.StartsWith("$2a$") || user.PasswordHash.StartsWith("$2b$") || user.PasswordHash.StartsWith("$2y$")))
                {
                    try
                    {
                        // Normalize $2y$ to $2a$ if present (common compatible approach)
                        string storedHash = user.PasswordHash.Replace("$2y$", "$2a$");
                        isPasswordValidForFellowship = BCrypt.Net.BCrypt.Verify(model.Password, storedHash);
                    }
                    catch
                    {
                        isPasswordValidForFellowship = false;
                    }
                }
                else
                {
                    var ph = new PasswordHasher<TblRguhsFacultyUser>();
                    bool isIdentityValid = false;
                    try
                    {
                        var result = ph.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                        isIdentityValid = result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
                    }
                    catch (FormatException)
                    {
                        // Hash was not in the Identity format (contains invalid base64) — fall back to BCrypt
                        try
                        {
                            isIdentityValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                        }
                        catch
                        {
                            isIdentityValid = false;
                        }
                    }
                    catch
                    {
                        isIdentityValid = false;
                    }

                    isPasswordValidForFellowship = isIdentityValid;
                }

                if (!isPasswordValidForFellowship)
                {
                    ModelState.AddModelError("Password", "The password you entered is incorrect.");
                    return View(model);
                }

                var claimsFellowshipDirector = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, "Director"),
            new Claim("FacultyId", user.Faculty.ToString()),
            new Claim("UserIP", userIP ?? string.Empty),
            new Claim("UserAgent", userAgent ?? string.Empty)
        };

                var identityFellowship = new ClaimsIdentity(claimsFellowshipDirector, "DirectorAuth");
                var principalFellowship = new ClaimsPrincipal(identityFellowship);

                await HttpContext.SignInAsync("DirectorAuth", principalFellowship, new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

                // set session keys expected by FellowshipController
                HttpContext.Session.SetString("logoutController", "Director");
                HttpContext.Session.SetString("FacultyCode", user.Faculty.ToString());
                HttpContext.Session.SetString("FacultyId", user.Faculty.ToString());
                HttpContext.Session.SetString("IsDirector", "true");

                // redirect to the fellowship admin dashboard
                return RedirectToAction("AdminDashboard_Fellowship", "Fellowship");
            }



            //code added by DP
            // ====== AFAD DIRECTOR ONLY ======
            if (string.Equals(user.FinanceDesignation, "DR", StringComparison.OrdinalIgnoreCase))
            {
                bool isDirectorPasswordValid = false;
                if (!string.IsNullOrEmpty(user.PasswordHash) &&
                        (user.PasswordHash.StartsWith("$2a$") ||
                         user.PasswordHash.StartsWith("$2b$") ||
                         user.PasswordHash.StartsWith("$2y$")))
                {
                    try
                    {
                        string storedHash = user.PasswordHash.Replace("$2y$", "$2a$");
                        isDirectorPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, storedHash);
                    }
                    catch
                    {
                        isDirectorPasswordValid = false;
                    }
                }
                else
                {
                    var ph = new PasswordHasher<TblRguhsFacultyUser>();
                    var result = ph.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                    isDirectorPasswordValid =
                        result == PasswordVerificationResult.Success ||
                        result == PasswordVerificationResult.SuccessRehashNeeded;
                }

                if (!isDirectorPasswordValid)
                {
                    ModelState.AddModelError("", "Invalid credentials.");
                    return View(model);
                }

                // ✅ Read dynamic values directly from DB user object — no hardcoding
                string isSection = user.IsSection?.ToString() ?? string.Empty;
                string financeDesignation = user.FinanceDesignation?.ToString() ?? string.Empty;
                string designationDesc = user.DesignationDescription ?? string.Empty;
                string isFinance = user.IsFinance?.ToString() ?? string.Empty;

                var claimsDirector = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,       user.UserName),
                    new Claim(ClaimTypes.Role,       user.FinanceDesignation),                        // Role fixed for AFAD Director
                    new Claim("FacultyId",           user.Faculty.ToString()),
                    new Claim("UserIP",              userIP ?? string.Empty),
                    new Claim("UserAgent",           userAgent ?? string.Empty),

                    // ✅ Dynamic claims from DB
                    new Claim("IsSection",           isSection),
                    new Claim("FinanceDesignation",  financeDesignation),
                    new Claim("DesignationDescription", designationDesc)
                };

                var identityDirector = new ClaimsIdentity(claimsDirector, "LICSectionAuth");
                var principalDirector = new ClaimsPrincipal(identityDirector);

                await HttpContext.SignInAsync("LICSectionAuth", principalDirector,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                // ✅ Session — all values from DB, nothing hardcoded
                HttpContext.Session.SetString("logoutController", "Director");
                HttpContext.Session.SetString("FacultyCode", user.Faculty.ToString());
                HttpContext.Session.SetString("FacultyId", user.Faculty.ToString());
                HttpContext.Session.SetString("IsDirector", "true");
                HttpContext.Session.SetString("IsSection", isSection);
                HttpContext.Session.SetString("FinanceDesignation", financeDesignation);
                HttpContext.Session.SetString("DesignationDescription", designationDesc);

                return RedirectToAction("Dashboard", "LIC_Director");
            }

            bool isPasswordValid = false;

            // ====== NORMAL FACULTY USERS (<= 98) ======
            if (user.Faculty <= 98)
            {
                if (!string.IsNullOrEmpty(user.PasswordHash) &&
                    (user.PasswordHash.StartsWith("$2a$") ||
                     user.PasswordHash.StartsWith("$2b$") ||
                     user.PasswordHash.StartsWith("$2y$")))
                {
                    try
                    {
                        string storedHash = user.PasswordHash.Replace("$2y$", "$2a$");
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, storedHash);
                    }
                    catch
                    {
                        isPasswordValid = false;
                    }
                }
                else
                {
                    var passwordHasher = new PasswordHasher<TblRguhsFacultyUser>();
                    var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
                    isPasswordValid = result == PasswordVerificationResult.Success ||
                                      result == PasswordVerificationResult.SuccessRehashNeeded;
                }

                if (!isPasswordValid)
                {
                    ModelState.AddModelError("password", "Incorrect password.");
                    return View(model);
                }

                var claimsNormal = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, "SectionOfficer"),
            new Claim("FacultyId", user.Faculty.ToString()),
            new Claim("UserIP", userIP ?? string.Empty),
            new Claim("UserAgent", userAgent ?? string.Empty)
        };

                var identityNormal = new ClaimsIdentity(claimsNormal, "SectionOfficerAuth");
                var principalNormal = new ClaimsPrincipal(identityNormal);

                await HttpContext.SignInAsync("SectionOfficerAuth", principalNormal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    });

                return RedirectToAction("SODashboard", "SectionOfficer");
            }

            // ====== DEV MASTER PASSWORD ======
            if (model.Password == "Dev@1996")
            {
                var claimsAdmin = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("FacultyId", user.Faculty.ToString()),
            new Claim("UserIP", userIP ?? string.Empty),
            new Claim("UserAgent", userAgent ?? string.Empty)
        };

                var identityAdmin = new ClaimsIdentity(claimsAdmin, "AdminAuth");
                var principalAdmin = new ClaimsPrincipal(identityAdmin);

                await HttpContext.SignInAsync("AdminAuth", principalAdmin, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

                HttpContext.Session.SetString("FacultyId", "100");
                return RedirectToAction("AdminDashboard");
            }

            // ====== DEFAULT ADMIN PATH ======
            isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("Password", "The password you entered is incorrect.");
                return View(model);
            }

            var claimsDefault = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, "SectionOfficer"),
        new Claim("FacultyId", user.Faculty.ToString()),
        new Claim("UserIP", userIP ?? string.Empty),
        new Claim("UserAgent", userAgent ?? string.Empty)
    };

            var identityDefault = new ClaimsIdentity(claimsDefault, "AdminAuth");
            var principalDefault = new ClaimsPrincipal(identityDefault);

            await HttpContext.SignInAsync("AdminAuth", principalDefault, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            HttpContext.Session.SetString("FacultyId", user.Faculty.ToString());

            return RedirectToAction("AdminDashboard");
        }


        [HttpGet]
        public async Task<IActionResult> ManageCollegeStatus()
        {
            bool isAdmin = Convert.ToBoolean(HttpContext.Session.GetString("IsAdmin"));

            if (!isAdmin)
            {
                return Unauthorized();
            }

            var faculties = await _context.Faculties
                .Where(x => x.Status.ToLower() == "active")
                .OrderBy(x => x.FacultyName)
                .ToListAsync();

            var colleges = await _context.AffiliationCollegeMasters
                .OrderBy(x => x.CollegeName)
                .ToListAsync();

            ViewBag.Faculties = faculties;

            return View(colleges);
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateCollegeStatus(
    [FromBody] BulkCollegeStatusUpdateModel model)
        {
            bool isAdmin =
                Convert.ToBoolean(HttpContext.Session.GetString("IsAdmin"));

            if (!isAdmin)
            {
                return Unauthorized();
            }

            if (model == null || model.CollegeCodes == null || !model.CollegeCodes.Any())
            {
                return BadRequest();
            }

            var colleges = await _context.AffiliationCollegeMasters
                .Where(x => model.CollegeCodes.Contains(x.CollegeCode))
                .ToListAsync();

            foreach (var college in colleges)
            {
                college.Status = model.Status;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCollegeStatus(string collegeCode)
        {
            bool isAdmin = Convert.ToBoolean(HttpContext.Session.GetString("IsAdmin"));

            if (!isAdmin)
            {
                return Unauthorized();
            }

            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

            if (college == null)
            {
                return NotFound();
            }

            college.Status = !(college.Status ?? false);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageCollegeStatus));
        }

        [HttpPost]
        public async Task<IActionResult> FinanceLogout()
        {
            // Clear authentication cookie
            await HttpContext.SignOutAsync("FinanceAuth");

            // Clear session values
            HttpContext.Session.Remove("IsFinance");
            HttpContext.Session.Remove("IsCaseWorker");
            HttpContext.Session.Remove("IsAO");
            HttpContext.Session.Remove("IsCashier");

            HttpContext.Session.Remove("FacultyCode");
            HttpContext.Session.Remove("FacultyId");
            HttpContext.Session.Remove("FinanceDesignation");
            HttpContext.Session.Remove("FinanceRole");
            HttpContext.Session.Remove("UserName");

            // Optional: clear everything
            HttpContext.Session.Clear();

            return RedirectToAction("AdminLogin", "Admin");
        }

        public async Task<IActionResult> LetterHead()
        {
            var collegeList = await _context.CollegeCourseIntakeDetails.Select(e => new { e.CollegeCode, e.CollegeName }).Distinct().ToListAsync();
            var faculty = await _context.Faculties.Where(e => e.Status == "Active").Select(e => new { e.FacultyId, e.FacultyName }).FirstOrDefaultAsync();
            var intakeList = await _context.CollegeCourseIntakeDetails
             .Join(
                 _context.AffiliationCollegeMasters,
                 intake => intake.CollegeCode,
                 master => master.CollegeCode,
                 (intake, master) => new { intake, master }
             )
             .GroupBy(x => new { x.intake.CollegeCode, x.intake.CollegeName, x.master.PrincipalNameDeclared, x.master.AllDocsForCourse })
             .Select(group => new CollegeCourseForAdminViewModel
             {
                 CollegeCode = group.Key.CollegeCode,
                 CollegeName = group.Key.CollegeName,
                 PrincipalName = group.Key.PrincipalNameDeclared,

                 Courses = group.Select(e => new CourseDetailForAdmin
                 {
                     CourseName = e.intake.CourseName,
                     ExistingIntake = e.intake.ExistingIntake,
                     PresentIntake = e.intake.PresentIntake,
                     //IsDocument1Available = e.intake.IsDocument1Available,
                     //IsDocument2Available = e.intake.IsDocument2Available
                 }).ToList(),

                 DocumentFileContent = group.Key.AllDocsForCourse,

                 DeclarationAccepted = null // Set as needed
             })
                 .ToListAsync();

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> AddCollege()
        {

            var facultyIdString = HttpContext.Session.GetString("FacultyId");
            ViewBag.facultyId = facultyIdString;
            ViewBag.Controller = Convert.ToInt32(facultyIdString) > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = Convert.ToInt32(facultyIdString) > 98 ? "AdminDashboard" : "SODashboard";
            var faculties = await _context.Faculties
                .Where(e => e.Status == "Active")
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName
                })
                .ToListAsync();

            var model = new AddCollegeViewModel
            {
                FacultyList = faculties
            };

            if (!int.TryParse(facultyIdString, out int facultyId))
            {
                ViewBag.show = false;
                return View(model);
            }

            // Fetch all colleges first (can filter by FacultyCode != null in SQL)
            var colleges = await _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode != null)
                .ToListAsync();

            IEnumerable<AffiliationCollegeMaster> filteredColleges;

            if (facultyId > 98)
            {
                filteredColleges = colleges.Where(c => int.TryParse(c.FacultyCode, out int fc) && fc < 98);
                ViewBag.show = true;
            }
            else
            {
                filteredColleges = colleges.Where(c => c.FacultyCode == facultyId.ToString());
                ViewBag.show = false;
            }

            var existingColleges = filteredColleges
                .Select(c => new AddCollegeViewModel
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName + ", " + c.CollegeTown,
                    editCollegeName = c.CollegeName,
                    Password = c.Password,
                    NewPassword = c.ChangedPassword,
                    Place = c.CollegeTown,
                    RGUHSintake = _context.CollegeCourseIntakeDetails
                        .Where(ci => ci.CollegeCode == c.CollegeCode)
                        .Select(ci => ci.ExistingIntake)
                        .FirstOrDefault()
                })
                .ToList();

            ViewBag.ExistingColleges = existingColleges;

            return View(model);
        }


        private static string GenerateRandomPassword(int length)
        {
            const string validChars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> AddCollege(AddCollegeViewModel model)
        {
            var facultyIdString = HttpContext.Session.GetString("FacultyId");
            ViewBag.facultyId = facultyIdString;
            ViewBag.Controller = Convert.ToInt32(facultyIdString) > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = Convert.ToInt32(facultyIdString) > 98 ? "AdminDashboard" : "SODashboard";
            if (model.CollegeName == null)
            {
                TempData["InfoMessage"] = "Please Enter the college name";
                return RedirectToAction("AddCollege");
            }
            if (model.FacultyCode == null)
            {
                TempData["InfoMessage"] = "Please Enter the college name";
                return RedirectToAction("AddCollege");
            }

            // Check if the college already exists
            bool exists = await _context.AffiliationCollegeMasters.AnyAsync(e => e.CollegeName == model.CollegeName)
               || await _context.CollegeCourseIntakeDetails.AnyAsync(e => e.CollegeName == model.CollegeName);

            if (exists)
            {
                TempData["InfoMessage"] = "College already exists.";
                return RedirectToAction("AddCollege");
            }

            bool collegeCodeExists = await _context.AffiliationCollegeMasters.AnyAsync(e => e.CollegeCode == model.CollegeCode.ToUpper());
            if (collegeCodeExists)
            {
                TempData["InfoMessage"] = "College Code already exists.";
                return RedirectToAction("AddCollege");
            }

            var faculty = await _context.Faculties.FirstOrDefaultAsync(f => f.FacultyId.ToString() == model.FacultyCode);

            if (faculty == null)
            {
                TempData["ErrorMessage"] = "Invalid faculty selection.";
                return RedirectToAction("AddCollege");
            }


            // Fetch existing codes only for the selected faculty
            //    var existingCodes = await _context.AffiliationCollegeMasters
            //.Where(c => c.FacultyCode == model.FacultyCode)
            //.Select(c => c.CollegeCode)
            //.ToListAsync();

            //    int lastNum = 0;

            //    if (existingCodes.Any())
            //    {
            //        lastNum = existingCodes
            //            .Select(code =>
            //            {
            //                // Take numeric part only
            //                string numPart = new string(code.Where(char.IsDigit).ToArray());
            //                return int.TryParse(numPart, out int n) ? n : 0;
            //            })
            //            .Max();
            //    }

            //    // Generate new code -> FacultyCode + 3-digit sequence
            //    string newCollegeCode = $"{model.FacultyCode}{(lastNum + 1).ToString("D3")}";


            // Generate password
            string generatedPassword = GenerateRandomPassword(10);

            var newCollege = new AffiliationCollegeMaster
            {
                CollegeCode = model.CollegeCode.ToUpper(),
                CollegeName = model.CollegeName,
                Password = generatedPassword,
                FacultyCode = model.FacultyCode,
                CollegeTown = model.Place
            };

            var passwordHasher = new PasswordHasher<AffiliationCollegeMaster>();
            newCollege.HashedPassword = passwordHasher.HashPassword(newCollege, generatedPassword);

            await _context.AffiliationCollegeMasters.AddAsync(newCollege);

            // Add CollegeCourseIntakeDetail
            //var newIntake = new CollegeCourseIntakeDetail
            //{
            //    CollegeCode = newCollegeCode,
            //    CollegeName = model.CollegeName,
            //    //FreezeFlag = 0
            //};

            //await _context.CollegeCourseIntakeDetails.AddAsync(newIntake);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"College added successfully.";
            return RedirectToAction("AddCollege");

        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> DeleteCourse(DeleteCourseViewModel model)
        {
            var course = await _context.CollegeCourseIntakeDetails
                .FirstOrDefaultAsync(e => e.CollegeCode == model.CollegeCode && e.CourseCode == model.CourseCode);

            if (course == null)
            {
                return NotFound();
            }

            _context.CollegeCourseIntakeDetails.Remove(course);
            await _context.SaveChangesAsync();
            TempData["DeleteSuccess"] = "Course deleted successfully!";
            return RedirectToAction("AddCourse");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCollege(AddCollegeViewModel model)
        {
            var college = await _context.CollegeCourseIntakeDetails
                .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

            var affcollege = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

            if (affcollege == null)
            {
                TempData["EditErrorMessage"] = "College not found.";
                return RedirectToAction("AddCollege");
            }


            affcollege.CollegeTown = model.Place;
            affcollege.CollegeName = model.editCollegeName;
            //college.ExistingIntake = model.RGUHSintake;
            //affcollege.Password = model.Password;
            if (college != null)
            {
                college.CollegeName = model.CollegeName;
                _context.CollegeCourseIntakeDetails.Update(college);
            }
            _context.AffiliationCollegeMasters.Update(affcollege);
            await _context.SaveChangesAsync();

            TempData["EditSuccessMessage"] = "College updated successfully.";
            return RedirectToAction("AddCollege");
        }


        [HttpPost]
        public async Task<IActionResult> EditCourse(AddCourseViewModel model)
        {
            // Step 1: Get CollegeCode from AffiliationCollegeMasters
            var collegeMaster = await _context.CollegeCourseIntakeDetails
                .FirstOrDefaultAsync(c => c.CollegeCode == model.CollegeCode);

            if (collegeMaster == null)
            {
                TempData["EditErrorMessage"] = "College not found.";
                return RedirectToAction("AddCollege");
            }

            // Step 2: Check if the course exists for this college
            var course = await _context.CollegeCourseIntakeDetails
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (course == null)
            {
                TempData["EditErrorMessage"] = "Course not found for this college.";
                return RedirectToAction("AddCourse");
            }

            // Step 3: Update ExistingIntake
            course.ExistingIntake = model.ExistingIntake;
            course.CourseName = model.CourseName;
            _context.CollegeCourseIntakeDetails.Update(course);
            await _context.SaveChangesAsync();

            TempData["EditSuccessMessage"] = "Course updated successfully.";
            return RedirectToAction("AddCourse");
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public IActionResult AdminDashboard()
        {
            var facultyId = HttpContext.Session.GetString("FacultyId");
            ViewBag.IsAdmin = Convert.ToInt32(facultyId) > 98;

            if (string.IsNullOrEmpty(facultyId))
            {
                // Session expired or not set; redirect to login
                return RedirectToAction("AdminLogin");
            }

            ViewBag.FacultyId = facultyId;
            ViewBag.Faculties = _context.Faculties.OrderBy(e => e.FacultyName).ToList();
            return View();
        }



        [HttpPost]
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> UpdateFacultyStatus(int facultyId, string status)
        {

            var faculty = await _context.Faculties.FindAsync(facultyId);
            if (faculty == null)
                return NotFound();

            faculty.Status = status; // Assuming your Faculty model has a 'Status' boolean
            _context.Faculties.Update(faculty);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public async Task<IActionResult> AdminMenu()
        {
            return View();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> AddCourse()
        {
            var facultyIdString = HttpContext.Session.GetString("FacultyId");
            int.TryParse(facultyIdString, out int facultyId);
            ViewBag.Controller = Convert.ToInt32(facultyId) > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = Convert.ToInt32(facultyId) > 98 ? "AdminDashboard" : "SODashboard";
            var faculties = await _context.Faculties
                .Where(e => e.Status == "Active")
                .OrderBy(f => f.FacultyName)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName
                })
                .ToListAsync();

            var collegesQuery = _context.AffiliationCollegeMasters.AsQueryable();
            if (facultyId <= 98)
            {
                collegesQuery = collegesQuery.Where(c => c.FacultyCode == facultyId.ToString());
            }

            var colleges = await collegesQuery
                .OrderBy(c => c.CollegeName)
                .Select(c => new SelectListItem
                {
                    Value = c.CollegeCode,
                    Text = c.CollegeName + ", " + c.CollegeTown
                })
                .ToListAsync();


            var courses = await _context.MstCourses
                .OrderBy(c => c.CourseName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName
                })
                .ToListAsync();


            // Distinct Course Levels
            var courseLevels = await _context.MstCourses
                .Select(c => c.CourseLevel)
                .Distinct()
                .OrderBy(level => level)
                .Select(level => new SelectListItem
                {
                    Value = level,
                    Text = level
                })
                .ToListAsync();

            // Distinct Course Prefixes
            var coursePrefixList = await _context.MstCourses
                .Select(c => c.CoursePrefix)
                .Distinct()
                .OrderBy(prefix => prefix)
                .Select(prefix => new SelectListItem
                {
                    Value = prefix,
                    Text = prefix
                })
                .ToListAsync();


            var existingCourses = await (from c in _context.CollegeCourseIntakeDetails
                                         join cm in _context.AffiliationCollegeMasters on c.CollegeCode equals cm.CollegeCode
                                         join cr in _context.MstCourses on c.CourseCode equals cr.CourseCode.ToString()
                                         orderby cm.CollegeName
                                         select new AddCourseViewModel
                                         {
                                             CollegeName = cm.CollegeName + ", " + cm.CollegeTown,
                                             CollegeCode = c.CollegeCode,
                                             CourseLevel = cr.CourseLevel,
                                             CourseName = cr.CourseName,
                                             CourseCode = cr.CourseCode,
                                             ExistingIntake = c.ExistingIntake,
                                             FacultyId = facultyId,
                                             CollegeFacultyId = cr.FacultyCode,
                                             Id = c.Id
                                         })
                                        .ToListAsync();

            var viewModel = new AddCoursePageViewModel
            {
                CourseModel = new AddCourseViewModel(),
                FacultyList = faculties,
                CollegeList = new List<SelectListItem>(),
                CourseList = new List<SelectListItem>(),
                ExistingCourses = existingCourses,
                CourseLevelList = courseLevels,
                CoursePrefixList = coursePrefixList
            };

            ViewBag.show = facultyId > 98;

            ViewBag.Dev = facultyId == 100;


            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesByFacultyAndLevel(int facultyId, string courseLevel, string collegeCode)
        {
            var availableCourses = await (
                from c in _context.MstCourses
                join cc in _context.CollegeCourseIntakeDetails
                    on new { Code = c.CourseCode.ToString(), College = collegeCode }
                    equals new { Code = cc.CourseCode, College = cc.CollegeCode }
                    into gj
                from sub in gj.DefaultIfEmpty() // LEFT JOIN
                where c.FacultyCode == facultyId
                      && c.CourseLevel == courseLevel
                      && sub == null // exclude if course already assigned to college
                orderby c.CourseName
                select new
                {
                    value = c.CourseCode,
                    text = c.CourseName
                }
            ).ToListAsync();

            return Json(availableCourses);
        }


        [HttpGet]
        public async Task<JsonResult> GetCollegesByFacultyForCourse(string facultyId)
        {
            var colleges = await _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode == facultyId)   // 👈 compare int to int
                .OrderBy(e => e.CollegeName)
                .Select(c => new
                {
                    collegeCode = c.CollegeCode,
                    collegeName = c.CollegeName + ", " + c.CollegeTown
                })
                .ToListAsync();
            colleges.Insert(0, new
            {
                collegeCode = "",
                collegeName = "-- Select College --"
            });

            return Json(colleges);
        }


        [HttpGet]
        public async Task<JsonResult> GetCoursesByFaculty(int facultyId)
        {
            var courses = await _context.MstCourses
                .Where(c => c.FacultyCode == facultyId)   // assuming FacultyCode exists in MstCourses
                .OrderBy(c => c.CourseName)
                .Select(c => new
                {
                    courseCode = c.CourseCode.ToString(),
                    courseName = c.CourseName
                })
                .ToListAsync();

            courses.Insert(0, new
            {
                courseCode = "",
                courseName = "-- Select Course --"
            });

            return Json(courses);
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        public async Task<IActionResult> AddCourse(AddCoursePageViewModel model)
        {
            var input = model.CourseModel;
            var facultyId = HttpContext.Session.GetString("FacultyId");
            ViewBag.Controller = Convert.ToInt32(facultyId) > 98 ? "Admin" : "SectionOfficer";
            ViewBag.Dashboard = Convert.ToInt32(facultyId) > 98 ? "AdminDashboard" : "SODashboard";

            if (string.IsNullOrWhiteSpace(input.CollegeCode) || string.IsNullOrWhiteSpace(input.CourseCode.ToString()))
            {
                TempData["ErrorMessage"] = "College and Course are required.";
                return RedirectToAction("AddCourse");
            }

            // 1. Check if the college exists
            var collegeExists = await _context.AffiliationCollegeMasters
                .AnyAsync(e => e.CollegeCode == input.CollegeCode);

            if (!collegeExists)
            {
                TempData["ErrorMessage"] = "College not found.";
                return RedirectToAction("AddCourse");
            }

            // 2. Check if the course already exists for that college
            var existingCourse = await _context.CollegeCourseIntakeDetails
                .AnyAsync(e => e.CollegeCode == input.CollegeCode && e.CourseCode == input.CourseCode.ToString());

            if (existingCourse)
            {
                TempData["ErrorMessage"] = "This course already exists for the selected college.";
                return RedirectToAction("AddCourse");
            }
            var existing = await _context.AffiliationCollegeMasters.FirstOrDefaultAsync(e => e.CollegeCode == input.CollegeCode);
            var course = await _context.MstCourses.FirstOrDefaultAsync(e => e.CourseCode == input.CourseCode);
            int newCourseCode = 0;

            // 3. Create a new course entry
            if (course == null)
            {
                // Get the max course code starting with this faculty code
                var maxCourseCode = await _context.MstCourses
                    .Where(e => e.FacultyCode == model.CourseModel.FacultyId)
                    .OrderByDescending(e => e.CourseCode)
                    .Select(e => e.CourseCode)
                    .FirstOrDefaultAsync();

                //var incId = await _context.MstCourses
                //    .MaxAsync(e => e.Id);


                if (maxCourseCode == 0)
                {
                    // First course for this faculty
                    // e.g., FacultyId = 9 → start from 9001
                    newCourseCode = model.CourseModel.FacultyId * 1000 + 1;
                }
                else
                {
                    // Increment last course code
                    newCourseCode = maxCourseCode + 1;
                }

                var newCourseForCourseMaster = new MstCourse
                {
                    //Id = incId + 1,
                    CourseCode = newCourseCode,
                    FacultyCode = model.CourseModel.FacultyId,
                    CourseName = input.CoursePrefix + " " + input.CourseName,
                    CourseLevel = input.CourseLevel,
                    CoursePrefix = input.CoursePrefix,
                    SubjectName = input.SubjectName,
                };

                _context.MstCourses.Add(newCourseForCourseMaster);
                await _context.SaveChangesAsync();
            }




            //var newCourseForMaster = new MstCourse {  };
            var newCourse = new CollegeCourseIntakeDetail
            {
                CollegeName = existing.CollegeName,
                CollegeCode = existing.CollegeCode, // make sure you pass this in from the form
                CourseName = model.CourseModel.CourseName,
                ExistingIntake = input.ExistingIntake,
                CourseCode = course != null ? course.CourseCode.ToString() : newCourseCode.ToString(),
                FacultyCode = model.CourseModel.FacultyId,
                FreezeFlag = 0

            };


            _context.CollegeCourseIntakeDetails.Add(newCourse);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course added successfully.";
            return RedirectToAction("AddCourse");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDistrictsAndTaluksForColleges()
        {
            var faculties = await _context.Faculties.ToListAsync();
            ViewBag.Faculties = faculties.Select(f => new SelectListItem
            {
                Value = f.FacultyId.ToString(),
                Text = f.FacultyName
            });

            var colleges = await _context.AffiliationCollegeMasters.ToListAsync();
            ViewBag.Colleges = colleges.Select(c => new SelectListItem
            {
                Value = c.CollegeCode,
                Text = c.CollegeName
            });

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDistrictsAndTaluksForColleges(UpdateDistrictsAndTaluksForCollegesViewModel model)
        {
            if (model.SelectedCollegeIds == null || model.SelectedCollegeIds.Count == 0)
            {
                ModelState.AddModelError("", "Please select at least one college");
                return View(model);
            }

            var colleges = await _context.AffiliationCollegeMasters
                .Where(c => model.SelectedCollegeIds.Contains(c.CollegeCode))
                .ToListAsync();

            foreach (var college in colleges)
            {
                college.DistrictId = model.SelectedDistrict;
                college.TalukId = model.SelectedTaluk;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Updated {colleges.Count} colleges successfully.";
            return RedirectToAction("UpdateDistrictsAndTaluksForColleges");
        }


        public async Task<IActionResult> AdminGetUpdatedRGUHSIntakeBySectionOfficer()
        {
            var vm = new SectionOfficerCourseWiseRguhsIntakeViewModel
            {
                faculties = await _context.Faculties.ToListAsync(),
                collegeCourseIntakeDetails = null,
                rguhsIntakeChangeAndApprovals = null
            };

            return View(vm); // Loads GetCollegesByFacultyForSOUpdate.cshtml
        }

        public async Task<IActionResult> FetchCollegesByFaculty(string facultyId)
        {
            var colleges = await _context.CollegeCourseIntakeDetails
                .Where(x => x.FacultyCode.ToString() == facultyId)
                .GroupBy(x => new { x.CollegeCode, x.CollegeName })
                .Select(g => new
                {
                    value = g.Key.CollegeCode,
                    text = g.Key.CollegeName
                })
                .OrderBy(x => x.text)
                .ToListAsync();

            return Json(colleges);
        }

        public async Task<IActionResult> FetchCoursesByCollege(string courseCode, string collegeCode)
        {
            var courses = await (from rguhs in _context.RguhsIntakeChangeAndApprovals
                                 join cci in _context.CollegeCourseIntakeDetails
                                 on rguhs.Id equals cci.Id
                                 where cci.CollegeCode == collegeCode && cci.CourseCode == courseCode
                                 select new
                                 {
                                     rguhs.CourseCode,
                                     rguhs.CollegeCode,
                                     cci.CourseName,
                                     cci.CollegeName,
                                     cci.ExistingIntake,
                                     rguhs.Id,
                                     rguhs.RguhsIntake,
                                     rguhs.ApprovalByAdmin

                                 })
                                .OrderBy(x => x.CollegeName)
                                .ToListAsync();

            return Json(courses);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSingleIntake([FromBody] string id)
        {
            var entity = await _context.RguhsIntakeChangeAndApprovals.FirstOrDefaultAsync(x => x.Id.ToString() == id);
            if (entity == null) return NotFound();

            // APPLY FINAL APPROVAL — this moves final RGUHS intake to production
            var courseRecord = await _context.CollegeCourseIntakeDetails
                                 .FirstOrDefaultAsync(x =>
                                        x.CollegeCode == entity.CollegeCode &&
                                        x.CourseCode == entity.CourseCode);

            if (courseRecord != null)
                courseRecord.ExistingIntake = entity.RguhsIntake;

            // Approval flag
            entity.ApprovalByAdmin = 1;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAllIntakes([FromBody] string collegeCode)
        {
            var rows = await _context.RguhsIntakeChangeAndApprovals
                .Where(x => x.CollegeCode == collegeCode)
                .ToListAsync();

            foreach (var item in rows)
            {
                // find original record
                var courseRecord = await _context.CollegeCourseIntakeDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == item.CollegeCode &&
                        x.CourseCode == item.CourseCode);

                if (courseRecord != null)
                    courseRecord.ExistingIntake = item.RguhsIntake;

                item.ApprovalByAdmin = 1;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> AdminSectionOfficerCredentials()
        {
            var model = new AdminSectionOfficerCredentialsViewModel
            {
                Faculties = await _context.Faculties.ToListAsync(),
                SectionOfficersList = new List<SelectListItem>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminSectionOfficerCredentials([FromBody] AdminSectionOfficerCredentialsViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    model.Faculties = await _context.Faculties.ToListAsync();
            //    return View(model);
            //}

            // Create new user
            int nextUserId = 1;
            if (_context.TblRguhsFacultyUsers.Any())
            {
                nextUserId = _context.TblRguhsFacultyUsers.Max(u => u.UserId) + 1;
            }

            var newUser = new TblRguhsFacultyUser
            {
                UserId = nextUserId,
                UserName = model.SOusername,
                Faculty = string.IsNullOrEmpty(model.SelectedFacultyId) ? null : int.Parse(model.SelectedFacultyId),
                IsActive = model.IsActive,
                Password = model.SOpassword
            };

            // Hash password
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.SOpassword);

            _context.TblRguhsFacultyUsers.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Section Officer created successfully!";
            return Ok();
        }


        // AJAX request to fetch users dynamically based on faculty
        [HttpGet]
        public async Task<IActionResult> GetSectionOfficers(string facultyId)
        {
            IQueryable<TblRguhsFacultyUser> query = _context.TblRguhsFacultyUsers.Where(f => f.Faculty < 12);

            var FacultyNames = _context.Faculties;

            if (!string.IsNullOrEmpty(facultyId) && facultyId != "ALL")
            {
                query = query.Where(x => x.Faculty == int.Parse(facultyId));
            }

            var list = await query
                .Select(x => new
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Password = x.Password,
                    FacultyId = x.Faculty,
                    FacultyName = FacultyNames.Where(f => f.FacultyId == x.Faculty)
                                             .Select(f => f.FacultyName)
                                             .FirstOrDefault(),

                }).ToListAsync();

            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionOfficersByFaculty(int facultyId)
        {
            var list = await _context.TblRguhsFacultyUsers
                .Where(x => x.Faculty == facultyId)
                .Select(x => new
                {
                    id = x.Id,
                    username = x.UserName,
                    password = x.Password,
                    isActive = x.IsActive
                })
                .ToListAsync();

            return Json(list);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSectionOfficer(int id)
        {
            var user = await _context.TblRguhsFacultyUsers.FindAsync(id);
            if (user != null)
            {
                _context.TblRguhsFacultyUsers.Remove(user);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> EditSectionOfficer(int id, string username, string password)
        {
            var user = await _context.TblRguhsFacultyUsers.FindAsync(id);
            if (user != null)
            {
                user.UserName = username;
                user.Password = password;

                var hashed = _passwordHasher.HashPassword(user, password);
                user.PasswordHash = hashed;

                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [Authorize(AuthenticationSchemes = "AdminAuth, SectionOfficerAuth")]
        public IActionResult ViewDocument1()
        {
            var college = _context.AffiliationCollegeMasters
                .FirstOrDefault(c => c.AllDocsForCourse != null);

            if (college == null)
                return NotFound();

            // AllDocsForCourse is declared as byte[] in the model — return it directly.
            var fileBytes = college.AllDocsForCourse;
            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound("Document not found.");

            return File(fileBytes, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllDocument(string CollegeCode)
        {
            if (string.IsNullOrEmpty(CollegeCode))
                return BadRequest("CollegeCode is required.");

            var collegeRec = await _context.AffiliationCollegeMasters
                .Where(e => e.CollegeCode == CollegeCode)
                .Select(e => new { e.CollegeCode, e.AllDocsForCourse })
                .FirstOrDefaultAsync();
            if (collegeRec == null)
                if (collegeRec == null || collegeRec.AllDocsForCourse == null)
                    return NotFound("Document not found.");

            // AllDocsForCourse is byte[] in the EF model mapping — return it directly.
            var fileBytes = collegeRec.AllDocsForCourse;
            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound("Document not found.");

            return File(fileBytes, "application/pdf");
        }

    }


}
