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
using Microsoft.AspNetCore.Hosting;

namespace Admission_Affiliation.Controllers
{
    public class CollegeloginController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CollegeloginController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ GET: Login Page
        [HttpGet]
        public IActionResult ClgLogin()
        {
            if (IsLoggedIn()) return RedirectToAction("Dashboard");

            return View();
        }

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
            HttpContext.Session.SetString("CollegeName", user.CollegeName ?? "");
            HttpContext.Session.SetString("CollegeCode", user.CollegeCode ?? "");
            HttpContext.Session.SetString("FacultyCode", user.FacultyCode ?? "");

            // Resolve faculty name (if available) and set in session for use in header
            string facultyName = string.Empty;
            if (!string.IsNullOrWhiteSpace(user.FacultyCode) && int.TryParse(user.FacultyCode, out var fcode))
            {
                facultyName = await _context.Faculties
                    .Where(f => f.FacultyId == fcode)
                    .Select(f => f.FacultyName)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }
            if (!string.IsNullOrWhiteSpace(facultyName))
                HttpContext.Session.SetString("FacultyName", facultyName);

            HttpContext.Session.SetString("TypeOfAffiliation", "College Affiliation");

            await HttpContext.SignInAsync("CollegeAuth", principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

            return RedirectToAction("Dashboard");
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [HttpGet]
        public IActionResult Dashboard()
        {
            Console.WriteLine("=== Dashboard Action Executed ===");
            Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"CollegeCode from Claims: {User.FindFirst("CollegeCode")?.Value}");
            Console.WriteLine($"FacultyCode from Claims: {User.FindFirst("FacultyCode")?.Value}");

            var collegeCode = User.FindFirst("CollegeCode")?.Value?.Trim();
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
            var detail = _context.DentalCollegeLandBuildingDetails
                            .AsNoTracking()
                            .FirstOrDefault(d => d.CollegeCode == collegeCode);

            // Load profile if exists (logo, contact info)
            // Case-insensitive, trimmed match to guard against data-entry mismatches
            var profile = _context.MedCollegeProfiles
                .AsNoTracking()
                .FirstOrDefault(p => p.CgpCollegeCode.Trim().ToUpper() == collegeCode.ToUpper());

            Console.WriteLine($"[Dashboard] Looking up Med_CollegeProfile for CollegeCode='{collegeCode}'");
            Console.WriteLine($"[Dashboard] Profile found: {profile != null}");
            if (profile != null)
            {
                Console.WriteLine($"[Dashboard] CGP_CollegeCode in DB: '{profile.CgpCollegeCode}'");
                Console.WriteLine($"[Dashboard] CGP_LogoPath in DB: '{profile.CgpLogoPath}'");

                // Check whether the physical file actually exists on disk
                if (!string.IsNullOrWhiteSpace(profile.CgpLogoPath))
                {
                    var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath,
                        profile.CgpLogoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    var fileExists = System.IO.File.Exists(physicalPath);
                    Console.WriteLine($"[Dashboard] Expected physical file: {physicalPath}");
                    Console.WriteLine($"[Dashboard] File exists on disk: {fileExists}");
                }
            }
            else
            {
                // Log all CollegeCodes in Med_CollegeProfile so you can spot-check the mismatch
                var allCodes = _context.MedCollegeProfiles
                    .AsNoTracking()
                    .Select(p => p.CgpCollegeCode)
                    .ToList();
                Console.WriteLine("[Dashboard] All CGP_CollegeCode values in DB: " + string.Join(", ", allCodes));
            }
            
            // Ensure faculty name in session (best-effort)
            var facultyName = HttpContext.Session.GetString("FacultyName");
            if (string.IsNullOrWhiteSpace(facultyName) && !string.IsNullOrWhiteSpace(college.FacultyCode))
            {
                if (int.TryParse(college.FacultyCode, out var fcode))
                {
                    facultyName = _context.Faculties
                        .Where(f => f.FacultyId == fcode)
                        .Select(f => f.FacultyName)
                        .FirstOrDefault();
                }
                if (!string.IsNullOrWhiteSpace(facultyName))
                    HttpContext.Session.SetString("FacultyName", facultyName);
            }

            // Persist CollegeName and CollegeLogoPath into session for immediate header use
            HttpContext.Session.SetString("CollegeName", college.CollegeName ?? collegeName ?? string.Empty);
            HttpContext.Session.SetString("CollegeLogoPath", profile?.CgpLogoPath ?? string.Empty);

            var model = new DashboardViewModel
            {
                CollegeCode = college.CollegeCode,
                CollegeName = college.CollegeName,
                FacultyName = facultyName,
                CollegeLogoPath = profile?.CgpLogoPath,
                PrincipalName = profile?.CgpPrincipalName,
                Address = profile?.CgpAddress,
                Email = profile?.CgpEmail,
                PhoneNumber = profile?.CgpPhoneNumber,
                Website = profile?.CgpWebsite,
                EstablishedYear = profile?.CgpEstablishedYear,
                DistrictId = college.DistrictId,
                TalukId = college.TalukId,
                Latitude = detail?.Latitude,
                Longitude = detail?.Longitude,
                Districts = _context.DistrictMasters
                    .OrderBy(x => x.DistrictName)
                    .ToList()
            };

            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (facultyCode == "2")
            {
                // Session exists → don't perform logout
                if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
                {
                    // Session does not exist → perform logout
                    HttpContext.Session.Clear();

                    return RedirectToAction("MultiLogin", "MainDashboard");
                }

                model.LateSubmissionFees = GetDentalLateSubmissionFees(facultyCode);
                // Session exists → load affiliation types
                var affiliationTypes = _context.TypeOfAffiliations
                    .AsNoTracking()
                    .OrderBy(x => x.TypeDescription)
                    .Select(x => new
                    {
                        Id = x.TypeId,
                        Name = x.TypeDescription
                    })
                    .ToList();

                ViewBag.AffiliationTypes = affiliationTypes;
            }

            // Application Overview: what was applied for, how far each is, and what was paid
            model.ApplicationOverview = BuildApplicationOverview(collegeCode, facultyCode);

            // Surface profile values to the viewbag for backwards compatibility in the view
            ViewBag.CollegeCode = college.CollegeCode;
            ViewBag.CollegeName = college.CollegeName;
            ViewBag.FacultyName = facultyName;
            ViewBag.CollegeLogoPath = profile?.CgpLogoPath ?? HttpContext.Session.GetString("CollegeLogoPath");
            ViewBag.PrincipalName = profile?.CgpPrincipalName;
            ViewBag.CollegeAddress = profile?.CgpAddress;
            ViewBag.CollegeEmail = profile?.CgpEmail;
            ViewBag.CollegePhoneNumber = profile?.CgpPhoneNumber;
            ViewBag.CollegeWebsite = profile?.CgpWebsite;
            ViewBag.EstablishedYear = profile?.CgpEstablishedYear;

            TempData["ShowWelcomePopup"] = false;
            TempData["CollegeName"] = college.CollegeName ?? collegeName;
            TempData["ShowAffiliationNotification"] = true;
            ViewBag.ShowAffiliationNotification = true;

            return View(model);
        }

        /// <summary>
        /// Builds the dashboard "Application Overview" from dbo.ApplicationSubmission
        /// (what was applied for) and dbo.PaymentAffiliationDocuments (what was paid),
        /// joined on college + type of affiliation + course level.
        /// </summary>
        private ApplicationOverviewViewModel BuildApplicationOverview(string collegeCode, string? facultyCode)
        {
            // Type of affiliation names come from dbo.MstAffiliationType.
            // If an id appears for more than one faculty, prefer this college's faculty, then active rows.
            var typeNames = _context.MstAffiliationTypes
                .AsNoTracking()
                .Select(t => new
                {
                    Id = t.AffiliationTypeId.ToString(),
                    Faculty = t.FacultyCode.ToString(),
                    t.AffiliationCategory,
                    Active = t.IsActive == true
                })
                .ToList()
                .GroupBy(t => t.Id)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Faculty == facultyCode)
                          .ThenByDescending(x => x.Active)
                          .Select(x => x.AffiliationCategory)
                          .FirstOrDefault() ?? "");

            // One row per course submitted; collapse to (type, level)
            var submissions = _context.ApplicationSubmissions
                .AsNoTracking()
                .Where(s => s.CollegeCode == collegeCode)
                .Select(s => new
                {
                    Type = s.TypeOfAffiliation.ToString(),
                    Level = (s.CourseLevel ?? "").Trim().ToUpper(),
                    s.CourseCode,
                    s.RegistrationNumber,
                    s.CreatedOn
                })
                .ToList()
                .GroupBy(s => new { s.Type, s.Level })
                .ToDictionary(
                    g => (g.Key.Type, g.Key.Level),
                    g => new
                    {
                        Courses = g.Select(x => x.CourseCode).Distinct().Count(),
                        Reg = g.Select(x => x.RegistrationNumber).FirstOrDefault(r => !string.IsNullOrWhiteSpace(r)),
                        On = g.Max(x => (DateTime?)x.CreatedOn)
                    });

            // Payments for the same (type, level)
            var payments = _context.PaymentAffiliationDocuments
                .AsNoTracking()
                .Where(p => p.CollegeCode == collegeCode)
                .Select(p => new
                {
                    Type = p.AffiliationTypeId.ToString(),
                    Level = (p.CourseLevel ?? "").Trim().ToUpper(),
                    p.PaymentAmount,
                    p.TransactionId,
                    p.PaymentDate
                })
                .ToList()
                .GroupBy(p => new { p.Type, p.Level })
                .ToDictionary(
                    g => (g.Key.Type, g.Key.Level),
                    g => new
                    {
                        Amount = g.Sum(x => (decimal)(x.PaymentAmount ?? 0)),
                        Txn = g.OrderByDescending(x => x.PaymentDate).Select(x => x.TransactionId).FirstOrDefault(),
                        On = g.Max(x => (DateTime?)x.PaymentDate)
                    });

            var keys = submissions.Keys.Union(payments.Keys)
                .OrderBy(k => k.Level).ThenBy(k => k.Type);

            var vm = new ApplicationOverviewViewModel();
            foreach (var key in keys)
            {
                submissions.TryGetValue(key, out var s);
                payments.TryGetValue(key, out var p);

                vm.Items.Add(new ApplicationOverviewItem
                {
                    TypeId = key.Type,
                    TypeName = typeNames.TryGetValue(key.Type, out var n) && n.Length > 0 ? n : $"Type {key.Type}",
                    CourseLevel = key.Level,
                    CourseCount = s?.Courses ?? 0,
                    RegistrationNumber = s?.Reg,
                    SubmittedOn = s?.On,
                    IsSubmitted = s != null,
                    IsPaid = p != null && p.Amount > 0,
                    AmountPaid = p?.Amount ?? 0,
                    TransactionId = p?.Txn,
                    PaymentDate = p?.On
                });
            }
            return vm;
        }

        private List<DashboardLateSubmissionFeeViewModel> GetDentalLateSubmissionFees(string facultyCode)
        {
            return _context.MstDentalFeeTypes
                .AsNoTracking()
                .Where(x =>
                    x.FacultyCode.ToString() == facultyCode &&
                    x.FeeType == "Late Submission Fee")
                .OrderBy(x => x.CourseLevel)
                .Select(x => new DashboardLateSubmissionFeeViewModel
                {
                    Id = x.Id,
                    AffiliationTypeId = x.AffiliationTypeId,
                    CourseLevel = x.CourseLevel,
                    ActivationDate = x.ActivationDate,
                    IsActive = x.IsActive
                })
                .ToList();
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [HttpGet]
        public async Task<IActionResult> GetDentalApplicationReport(int affiliationTypeId, string courseLevel)
        {
            try
            {
                // Get logged-in college
                var collegeCode =
                    User.FindFirst("CollegeCode")?.Value?.Trim();

                var facultyCode =
                    User.FindFirst("FacultyCode")?.Value?.Trim();

                // Dental only
                if (facultyCode != "2")
                {
                    return Json(new
                    {
                        success = false,
                        message = "This report is available only for Dental."
                    });
                }

                // Validate CollegeCode
                if (string.IsNullOrWhiteSpace(collegeCode))
                {
                    return Json(new
                    {
                        success = false,
                        message = "College code not found."
                    });
                }

                // Validate Affiliation Type
                if (affiliationTypeId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please select Type of Affiliation."
                    });
                }

                var affiliationTypeExists =
                    await _context.TypeOfAffiliations
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.TypeId == affiliationTypeId);

                if (!affiliationTypeExists)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Type of Affiliation."
                    });
                }

                // Validate Course Level
                courseLevel =
                    courseLevel?.Trim().ToUpperInvariant();

                var allowedCourseLevels = new[]
                {
                    "UG",
                    "PG",
                    "SS"
                };

                if (!allowedCourseLevels.Contains(courseLevel))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid Course Level."
                    });
                }

                // Get CaProgress records
                var progressRecords =
                    await _context.CaProgresses
                        .AsNoTracking()
                        .Where(x =>
                            x.CollegeCode == collegeCode &&
                            x.CourseLevel != null &&
                            x.CourseLevel.Trim().ToUpper() == courseLevel)
                        .Select(x => new
                        {
                            x.StepKey,
                            x.IsCompleted
                        })
                        .ToListAsync();

                // Get unique completed steps
                var completedSteps =
                    progressRecords
                        .Where(x =>
                            x.IsCompleted == true &&
                            !string.IsNullOrWhiteSpace(x.StepKey))
                        .Select(x => x.StepKey!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                // Define Dental application steps
                var totalSteps = new HashSet<string>(
                    new[]
                    {
                        "IntakeDetails",
                        "Institution",
                        "TrustDetails",
                        "TrustMemberDetails",
                        "DeanDetails",
                        "PrincipalDetails",

                        "BDSDetails",
                        "PgCourses",
                        "SsCoursesApplied",
                        "DentalFacultyDetails",

                        "CourseSubjectSelection",
                        "CourseGeneralDetails",
                        "CourseInfrastructureDetails",
                        "CourseSummaryDetails",
                        "CourseAcademicActivities",
                        "CourseServicesWorkload",
                        "CourseStaffDetails",

                        "ClinicalFacilities",
                        "ChairDistribution",
                        "BedDistribution",
                        "FacultyDetails",
                        "LandBuilding",
                        "Hostel",
                        "DepartmentUnits",
                        "DentalSkillsLab",
                        "DentalEquipmentDetails",
                        "NonTeachingStaff",
                        "TeachingStaff",

                        "AcademicMatters",
                        "Finance",
                        "PGAcademicMatters",

                        "LibraryServices",
                        "Library",
                        "DentalLibrary",
                        "DentalLibraryStaff",
                        "DentalLibraryUser",

                        "Vehicle",
                        "StaffDetails",

                        "Research",
                        "WorkshopDetails",
                        "AnimalHouseDetails",
                        "DentalFieldPracticeArea",

                        "AdditionalInformationInAcademicActivities",
                        "PaymentDetails",
                        "ActionTakenReport",
                    },
                    StringComparer.OrdinalIgnoreCase);

                // Course-level specific steps
                if (courseLevel == "UG")
                {
                    totalSteps.Remove("PgCourses");
                    totalSteps.Remove("PGAcademicMatters");

                    totalSteps.Remove("CourseSubjectSelection");
                    totalSteps.Remove("CourseGeneralDetails");
                    totalSteps.Remove("CourseInfrastructureDetails");
                    totalSteps.Remove("CourseSummaryDetails");
                    totalSteps.Remove("CourseAcademicActivities");
                    totalSteps.Remove("CourseServicesWorkload");
                    totalSteps.Remove("CourseStaffDetails");

                    totalSteps.Remove("SsCoursesApplied");
                    totalSteps.Remove("AdditionalInformationInAcademicActivities");
                }
                else if (courseLevel == "PG")
                {
                    totalSteps.Remove("BDSDetails");
                    totalSteps.Remove("AcademicMatters");
                    totalSteps.Remove("BedDistribution");

                    totalSteps.Remove("SsCoursesApplied");
                    totalSteps.Remove("CourseSubjectSelection");
                    totalSteps.Remove("CourseGeneralDetails");
                    totalSteps.Remove("CourseInfrastructureDetails");
                    totalSteps.Remove("CourseSummaryDetails");
                    totalSteps.Remove("CourseAcademicActivities");
                    totalSteps.Remove("CourseServicesWorkload");
                    totalSteps.Remove("CourseStaffDetails");
                }

                // Calculate completed count
                var completedCount =
                    totalSteps.Count(step =>
                        completedSteps.Contains(
                            step,
                            StringComparer.OrdinalIgnoreCase));

                var totalCount =
                    totalSteps.Count;

                var percentage =
                    totalCount > 0
                        ? (int)Math.Round(
                            (double)completedCount /
                            totalCount * 100)
                        : 0;

                percentage =
                    Math.Clamp(percentage, 0, 100);

                // Return response
                return Json(new
                {
                    success = true,
                    collegeCode = collegeCode,
                    facultyCode = facultyCode,
                    affiliationTypeId = affiliationTypeId,
                    courseLevel = courseLevel,
                    percentage = percentage,
                    completedSteps = completedCount,
                    totalSteps = totalCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GetDentalApplicationReport Error: {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching application status."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLocation(
            string CollegeCode,
            string DistrictId,
            string TalukId,
            decimal? Latitude,          // <-- from the visible <input> fields
            decimal? Longitude)         // <-- from the visible <input> fields
        {
            // 1️⃣  Basic validation
            if (string.IsNullOrWhiteSpace(CollegeCode))
            {
                TempData["Error"] = "Invalid College.";
                return RedirectToAction("Dashboard", "CollegeLogin");
            }

            if (string.IsNullOrWhiteSpace(DistrictId) ||
                string.IsNullOrWhiteSpace(TalukId))
            {
                TempData["Error"] = "Please select both District and Taluk.";
                return RedirectToAction("Dashboard", "CollegeLogin");
            }

            // Latitude / Longitude are required for the land‑/building record
            if (!Latitude.HasValue || !Longitude.HasValue)
            {
                TempData["Error"] = "Latitude and Longitude are required.";
                return RedirectToAction("Dashboard", "CollegeLogin");
            }

            // 2️⃣  Load the college (AffiliationCollegeMaster)
            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(x => x.CollegeCode == CollegeCode);

            if (college == null)
            {
                TempData["Error"] = "College not found.";
                return RedirectToAction("Dashboard", "CollegeLogin");
            }

            // 3️⃣  Update the college’s administrative data
            college.DistrictId = DistrictId;
            college.TalukId = TalukId;

            // 4️⃣  Load (or create) the DentalCollegeLandBuildingDetail
            var detail = await _context.DentalCollegeLandBuildingDetails
                .FirstOrDefaultAsync(d => d.CollegeCode == CollegeCode);

            if (detail == null)
            {
                // If there is no detail row yet, create a new one.
                // Adjust the property names if your table uses a different PK.
                detail = new DentalCollegeLandBuildingDetail
                {
                    CollegeCode = CollegeCode
                };
                _context.DentalCollegeLandBuildingDetails.Add(detail);
            }

            // 5️⃣  Store the latitude / longitude
            detail.Latitude = Latitude.Value;
            detail.Longitude = Longitude.Value;

            // 6️⃣  Persist changes
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", "CollegeLogin");
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
        }

        // ✅ GET: Change Password
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
        public async Task<IActionResult> GetTaluks(string districtId)
        {
            var taluks = await _context.TalukMasters
                .AsNoTracking()
                .Where(x => x.DistrictId == districtId)
                .Select(x => new
                {
                    talukId = x.TalukId,
                    talukName = x.TalukName
                })
                .ToListAsync();

            return Json(taluks);
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

        // ── GET: fetch current profile as JSON (used by the modal on open) ──
        [HttpGet]
        public async Task<IActionResult> GetCollegeProfile(string collegeCode)
        {
            if (string.IsNullOrEmpty(collegeCode))
                collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(collegeCode))
                return Json(new { success = false, message = "College code not found in session." });

            var profile = await _context.MedCollegeProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CgpCollegeCode.Trim().ToUpper() == collegeCode.ToUpper());

            var institution = await _context.AffInstitutionsDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.CollegeCode == collegeCode);

            var collegeName = HttpContext.Session.GetString("CollegeName")
                               ?? institution?.NameOfInstitution
                               ?? "";

            // Resolve the "~/" tilde path into a real browser-usable URL
            var resolvedLogoPath = !string.IsNullOrWhiteSpace(profile?.CgpLogoPath)
                ? Url.Content(profile.CgpLogoPath)
                : "";

            return Json(new
            {
                success = true,
                collegeCode,
                collegeName,
                logoPath = resolvedLogoPath,
                address = institution?.Address ?? "",
                email = institution?.EmailId ?? "",
                phoneNumber = institution?.MobileNumber ?? "",
                website = institution?.Website ?? "",
                establishedYear = institution?.YearOfEstablishment,
                principalName = institution?.PrincipalName ?? ""
            });
        }

        // ── POST: create or update the profile, optionally replacing the logo ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(5_000_000)] // 5 MB cap for logo uploads
        public async Task<IActionResult> UpdateCollegeProfile(CollegeProfileViewModel model)
        {
            var collegeCode = model.CollegeCode ?? HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
            {
                TempData["Error"] = "Could not identify the college for this session.";
                return RedirectToAction("Dashboard");
            }

            // Only proceed if a logo was actually posted — nothing else is editable here anymore
            if (model.Logo == null || model.Logo.Length == 0)
            {
                TempData["Error"] = "Please select a logo image to upload.";
                return RedirectToAction("Dashboard");
            }

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(model.Logo.FileName).ToLowerInvariant();

            if (!allowedExt.Contains(ext))
            {
                TempData["Error"] = "Logo must be a JPG, PNG, or WEBP image.";
                return RedirectToAction("Dashboard");
            }

            var profile = await _context.MedCollegeProfiles
                .FirstOrDefaultAsync(p => p.CgpCollegeCode == collegeCode);

            var isNew = profile == null;
            if (isNew)
            {
                profile = new MedCollegeProfile
                {
                    CgpCollegeCode = collegeCode,
                    CgpCreatedDate = DateTime.Now
                };
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "CollegeLogos");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{collegeCode}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await model.Logo.CopyToAsync(stream);
            }

            // Delete old logo file if one existed, to avoid orphaned files
            if (!string.IsNullOrEmpty(profile.CgpLogoPath))
            {
                var oldFullPath = Path.Combine(_webHostEnvironment.WebRootPath,
                    profile.CgpLogoPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(oldFullPath))
                {
                    try { System.IO.File.Delete(oldFullPath); } catch { /* non-fatal */ }
                }
            }

            profile.CgpLogoPath = $"/uploads/CollegeLogos/{fileName}";
            profile.CgpModifiedDate = DateTime.Now;
            profile.CgpModifiedBy = collegeCode;

            if (isNew)
                _context.MedCollegeProfiles.Add(profile);

            await _context.SaveChangesAsync();

            // Keep session in sync so the topbar logo updates immediately without re-login
            HttpContext.Session.SetString("CollegeLogoPath", profile.CgpLogoPath ?? "");

            TempData["SuccessMessage"] = "College logo updated successfully.";
            return RedirectToAction("Dashboard");
        }
    }
}