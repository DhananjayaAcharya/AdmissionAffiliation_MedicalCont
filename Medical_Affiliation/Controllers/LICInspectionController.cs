using BCrypt.Net;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Medical_Affiliation.Controllers
{
    public class LICInspectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private object _webHostEnvironment;

        public LICInspectionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: AUTH PAGE
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Auth(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            await LoadMemberTypesAsync();
            return View("Auth");
        }

        // =========================================================
        // AJAX: GET NAMES BASED ON ROLE
        // =========================================================
        [HttpGet]
        public async Task<JsonResult> GetNamesByRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return Json(new List<object>());

            role = role.Trim();

            var names = await _context.LicInspections
                .Where(x => x.TypeofMember != null &&
                            x.TypeofMember.Trim() == role)
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return Json(names);
        }


        // =========================================================
        // SIGNUP
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMemberTypesAsync();
                return View("Auth", model);
            }

            // 🔎 Validate Role + Name from LIC_Inspection table
            var record = await _context.LicInspections
                .FirstOrDefaultAsync(x =>
                    x.TypeofMember == model.TypeofMember &&
                    x.Name == model.Name);

            if (record == null)
            {
                ModelState.AddModelError("", "Invalid member selection. " +
                    "Please select a valid designation and name.");
                await LoadMemberTypesAsync();
                return View("Auth", model);
            }

            // 📱 Phone number verification
            if (record.PhoneNumber != model.PhoneNumber?.Trim())
            {
                ModelState.AddModelError("PhoneNumber",
                    "Entered phone number does not match our records.");
                await LoadMemberTypesAsync();
                return View("Auth", model);
            }

            // 📧 Email format already validated by model annotations
            // Check if email already used by another account
            bool emailTaken = await _context.LicInspections
                .AnyAsync(x =>
                    x.Email == model.Email.Trim().ToLowerInvariant() &&
                    x.Name != model.Name);

            if (emailTaken)
            {
                ModelState.AddModelError("Email",
                    "This email address is already registered to another account.");
                await LoadMemberTypesAsync();
                return View("Auth", model);
            }

            // 🚫 Prevent duplicate registration
            if (!string.IsNullOrWhiteSpace(record.CreatedPassword))
            {
                ModelState.AddModelError("",
                    "An account is already registered for this member. " +
                    "Please login instead.");
                await LoadMemberTypesAsync();
                return View("Auth", model);
            }

            // 🔐 Hash password and save
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password.Trim());

            record.Email = model.Email?.Trim()?.ToLowerInvariant();
            record.CreatedPassword = hashedPassword;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Account created successfully! Please login.";
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        // =========================================================
        // LOGIN
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var model = new LoginViewModel();
            await LoadMemberTypesAsync();
            return View("Auth", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ModelState.Remove("RememberMe");

            if (!ModelState.IsValid)
            {
                await LoadMemberTypesAsync();
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            // ✅ Find user by phone number
            var user = await _context.LicInspections
                .FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber);

            if (user == null || string.IsNullOrWhiteSpace(user.CreatedPassword))
            {
                ModelState.AddModelError("", "Invalid phone number or account not registered.");
                await LoadMemberTypesAsync();
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            // ✅ Verify password
            bool passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.CreatedPassword);
            if (!passwordValid)
            {
                ModelState.AddModelError("", "Invalid password.");
                await LoadMemberTypesAsync();
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            // ✅ Build Claims
            // "PhoneNumber" as plain string — matches what LicInspectionSessionMiddleware expects
            // "UserIP" and "UserAgent" — enables session hijacking protection in middleware
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,  user.Name ?? user.PhoneNumber),
                new Claim("PhoneNumber",    user.PhoneNumber),
                new Claim(ClaimTypes.Role,  user.TypeofMember ?? "Unknown"),
                new Claim("TypeofMember",   user.TypeofMember ?? "Unknown"),
                new Claim("UserIP",         HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""),
                new Claim("UserAgent",      HttpContext.Request.Headers["User-Agent"].ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                "LicInspectionAuth");   // ✅ must match the scheme name

            var principal = new ClaimsPrincipal(identity);

            // ✅ Sign in with "LicInspectionAuth" — matches Program.cs and middleware
            await HttpContext.SignInAsync(
                "LicInspectionAuth",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                });

            // ✅ Redirect to returnUrl if safe, else fall back to Dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Dashboard", "LICInspection");
        }

        // =========================================================
        // LOGOUT
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        // =========================================================
        // LOAD MEMBER TYPES
        // =========================================================
        private async Task LoadMemberTypesAsync()
        {
            try
            {
                var types = await _context.MstLicInspectionMembers
                    .Where(m => !string.IsNullOrWhiteSpace(m.TypeofMemebers))
                    .Select(m => m.TypeofMemebers.Trim())
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                ViewBag.MemberTypes = types;
            }
            catch
            {
                ViewBag.MemberTypes = new List<string>();
            }
        }

        // =========================================================
        // REDIRECT HELPER
        // =========================================================
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> InspectionDetails()
        {
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var member = await _context.LicInspections
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);

            if (member == null)
                return RedirectToAction("Login");

            string role = member.TypeofMember?.Trim();

            var collegesQuery = _context.LicInspectionCollegeDetails.AsNoTracking();
            List<InspectionCollegeItem> assignedColleges;

            if (role == "Academic Council")
            {
                assignedColleges = await collegesQuery
                    .Where(x => x.AcMemberPhno.ToString() == phone)
                    .Select(x => new InspectionCollegeItem
                    {
                        CollegeName = x.Collegename,
                        CollegeCode = x.Collegecode,
                    })
                    .OrderBy(x => x.CollegeName)
                    .ToListAsync();
            }
            else if (role == "Senate Members")
            {
                assignedColleges = await collegesQuery
                    .Where(x => x.SenetMemberPhNo.ToString() == phone)
                    .Select(x => new InspectionCollegeItem
                    {
                        CollegeName = x.Collegename,
                        CollegeCode = x.Collegecode,
                    })
                    .OrderBy(x => x.CollegeName)
                    .ToListAsync();
            }
            else if (role == "Subject Expertise")
            {
                assignedColleges = await collegesQuery
                    .Where(x => x.SubjectExpertisePhNo.ToString() == phone)
                    .Select(x => new InspectionCollegeItem
                    {
                        CollegeName = x.Collegename,
                        CollegeCode = x.Collegecode,
                    })
                    .OrderBy(x => x.CollegeName)
                    .ToListAsync();
            }
            else
            {
                assignedColleges = new List<InspectionCollegeItem>();
            }

            var model = new LICInspectionDetailsViewModel
            {
                Id = member.Id,
                TypeofMember = member.TypeofMember,
                Name = member.Name,
                DOB = member.Dob,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                PANNumber = member.Pannumber,
                AadhaarNumber = member.AadhaarNumber,
                AccountHolderName = member.AccountHolderName ?? member.Name,
                AccountNumber = member.AccountNumber,
                IFSCCode = member.Ifsccode,
                BankName = member.BankName,
                BranchName = member.BranchName,
                Colleges = assignedColleges,
                DateOfInspection = member.DateOfInspection,
                Kilometers = member.Kilometers,
                TotalCost = member.TotalCost,
                ModeOfTravel = member.ModeOfTravel,
                FromPlace = member.FromPlace,
                ToPlace = member.ToPlace,
                IsCompleted = member.IsCompleted ?? false,

                // Professional
                ProfessionalCollegeCode = member.CollegeCode,
                ProfessionalDesignation = member.DesignationCode,
                ProfessionalDepartment = member.DepartmentCode,
            };

            // ── Mode of Travel ────────────────────────────────────────────────────
            var travelModes = await _context.Database
                .SqlQuery<string>(
                    $"SELECT Modeoftravel FROM [Admission_Affiliation].[dbo].[LIC_ModeofTravel] ORDER BY Modeoftravel"
                )
                .ToListAsync();

            model.ModeOfTravelOptions = travelModes
                .Select(m => new SelectListItem
                {
                    Value = m,
                    Text = m,
                    Selected = string.Equals(m, model.ModeOfTravel, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = "", Text = "-- Select Mode of Travel --" })
                .ToList();

            // ── Colleges ──────────────────────────────────────────────────────────
            var colleges = await _context.Database
                .SqlQuery<CollegeDropdownItem>(
                    $@"SELECT CollegeCode, CollegeName 
                       FROM [Admission_Affiliation].[dbo].[Affiliation_College_Master]
                       ORDER BY CollegeName"
                )
                .ToListAsync();

            model.CollegeOptions = colleges
                .Select(c => new SelectListItem
                {
                    Value = c.CollegeCode,
                    Text = c.CollegeName,
                    Selected = string.Equals(c.CollegeCode, model.ProfessionalCollegeCode, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = "", Text = "-- Select College --" })
                .ToList();

            // ── Designations ──────────────────────────────────────────────────────
            var designations = await _context.Database
                .SqlQuery<string>(
                    $"SELECT DesignationName FROM [Admission_Affiliation].[dbo].[DesignationMaster] WHERE FacultyCode = 1 ORDER BY DesignationName"
                )
                .ToListAsync();

            model.DesignationOptions = designations
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d,
                    Selected = string.Equals(d, model.ProfessionalDesignation, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = "", Text = "-- Select Designation --" })
                .ToList();

            // ── Departments ───────────────────────────────────────────────────────
            var departments = await _context.Database
                .SqlQuery<string>(
                    $"SELECT SubjectName FROM [Admission_Affiliation].[dbo].[Mst_Course] ORDER BY SubjectName"
                )
                .ToListAsync();

            model.DepartmentOptions = departments
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d,
                    Selected = string.Equals(d, model.ProfessionalDepartment, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = "", Text = "-- Select Department --" })
                .ToList();

            return View(model);
        }

        public static async Task<byte[]> ConvertFileToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InspectionDetails(
    LICInspectionDetailsViewModel model,
    string submitType)
        {
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var member = await _context.LicInspections
                .FirstOrDefaultAsync(x => x.PhoneNumber == phone);

            if (member == null)
                return RedirectToAction("Login");

            if (submitType == "Basic")
            {
                member.Dob = model.DOB;
                member.Pannumber = model.PANNumber;
                member.AadhaarNumber = model.AadhaarNumber;

                TempData["Message"] = "Basic details updated successfully.";
            }
            else if (submitType == "Bank")
            {
                member.AccountHolderName = model.AccountHolderName;
                member.AccountNumber = model.AccountNumber;
                member.Ifsccode = model.IFSCCode;
                member.BankName = model.BankName;
                member.BranchName = model.BranchName;

                TempData["Message"] = "Bank details updated successfully.";
            }
            else if (submitType == "Professional")
            {
                member.CollegeCode = model.ProfessionalCollegeCode;
                member.DesignationCode = model.ProfessionalDesignation;
                member.DepartmentCode = model.ProfessionalDepartment;

                TempData["Message"] = "Professional details updated successfully.";
            }
            else if (submitType == "Claim")
            {
                member.ModeOfTravel = model.ModeOfTravel;
                member.FromPlace = model.FromPlace;
                member.ToPlace = model.ToPlace;
                member.Kilometers = model.Kilometers;
                member.TotalCost = model.TotalCost;

                TempData["Message"] = "Claim details updated successfully.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("InspectionDetails");
        }


        [HttpGet]
        public async Task<IActionResult> InspectionCollegeDetails(string collegeCode)
        {
            var phone = User.FindFirst(ClaimTypes.MobilePhone)?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var member = await _context.LicInspections
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);

            if (member == null)
                return RedirectToAction("Login");

            string role = member.TypeofMember?.Trim();

            // ✅ Fetch already saved college codes for this member
            var savedCollegeCodes = await _context.LicinspectionDetails
                .AsNoTracking()
                .Where(x => x.PhoneNumber.Trim() == phone)
                .Select(x => x.SelectedCollegeCode)
                .ToHashSetAsync();

            // ✅ If editing an existing record, allow its own college to still show in dropdown
            if (!string.IsNullOrWhiteSpace(collegeCode))
                savedCollegeCodes.Remove(collegeCode);

            IQueryable<LicInspectionCollegeDetail> query = _context.LicInspectionCollegeDetails
                .AsNoTracking();

            List<SelectListItem> assignedCollegeOptions = new();

            if (role == "Academic Council")
            {
                assignedCollegeOptions = await query
                    .Where(x => x.AcMemberPhno.ToString().Trim() == phone)
                    .OrderBy(x => x.Collegename)
                    .Select(x => new SelectListItem { Value = x.Collegecode, Text = x.Collegename })
                    .ToListAsync();
            }
            else if (role == "Senate Members")
            {
                assignedCollegeOptions = await query
                    .Where(x => x.SenetMemberPhNo.ToString().Trim() == phone)
                    .OrderBy(x => x.Collegename)
                    .Select(x => new SelectListItem { Value = x.Collegecode, Text = x.Collegename })
                    .ToListAsync();
            }
            else if (role == "Subject Expertise")
            {
                assignedCollegeOptions = await query
                    .Where(x => x.SubjectExpertisePhNo.ToString().Trim() == phone)
                    .OrderBy(x => x.Collegename)
                    .Select(x => new SelectListItem { Value = x.Collegecode, Text = x.Collegename })
                    .ToListAsync();
            }

            // ✅ Remove already submitted colleges from the dropdown
            assignedCollegeOptions = assignedCollegeOptions
                .Where(x => !savedCollegeCodes.Contains(x.Value))
                .ToList();

            assignedCollegeOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select College --",
                Selected = true
            });

            if (!string.IsNullOrWhiteSpace(collegeCode))
            {
                foreach (var item in assignedCollegeOptions)
                    item.Selected = item.Value == collegeCode;
            }

            // ✅ Faculty options
            var facultyList = await _context.Faculties
                .AsNoTracking()
                .OrderBy(f => f.FacultyName)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName
                })
                .ToListAsync();

            facultyList.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select Faculty --",
                Selected = true
            });

            LicinspectionDetail existingRecord = null;
            if (!string.IsNullOrWhiteSpace(collegeCode))
            {
                existingRecord = await _context.LicinspectionDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone
                                           && x.SelectedCollegeCode == collegeCode);
            }

            if (existingRecord?.FacultyId != null)
            {
                var existingFacultyId = existingRecord.FacultyId.ToString();
                foreach (var item in facultyList)
                    item.Selected = item.Value == existingFacultyId;
            }

            var savedInspections = await _context.LicinspectionDetails
                .AsNoTracking()
                .Where(x => x.PhoneNumber.Trim() == phone)
                .Join(
                    _context.AffiliationCollegeMasters,
                    inspection => inspection.SelectedCollegeCode,
                    college => college.CollegeCode,
                    (inspection, college) => new LicinspectionDetail
                    {
                        Id = inspection.Id,
                        Name = inspection.Name,
                        PhoneNumber = inspection.PhoneNumber,
                        TypeofMember = inspection.TypeofMember,
                        CollegeName = college.CollegeName,
                        SelectedCollegeCode = inspection.SelectedCollegeCode,
                        DateOfInspection = inspection.DateOfInspection,
                        IsCompleted = inspection.IsCompleted,
                        AttendenceDoc = inspection.AttendenceDoc,
                        CreatedDate = inspection.CreatedDate,
                        UpdatedDate = inspection.UpdatedDate,
                        FacultyId = inspection.FacultyId,
                        Faculty = inspection.Faculty
                    })
                .OrderByDescending(x => x.UpdatedDate)
                .ToListAsync();

            var model = new LICInspectionDetailsViewModel
            {
                Id = member.Id,
                Name = member.Name,
                PhoneNumber = member.PhoneNumber,
                TypeofMember = member.TypeofMember,
                Collegename = existingRecord?.CollegeName ?? member.CollegeName,
                DateOfInspection = existingRecord?.DateOfInspection ?? member.DateOfInspection,
                IsCompleted = existingRecord?.IsCompleted ?? member.IsCompleted ?? false,
                SelectedCollegeCode = collegeCode,
                SelectedFacultyId = existingRecord?.FacultyId,
                SelectedFacultyName = existingRecord?.Faculty,
                CollegeOptions = assignedCollegeOptions,
                FacultyOptions = facultyList,
                SavedInspections = savedInspections
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InspectionCollegeDetails(LICInspectionDetailsViewModel model)
        {
            var phone = User.FindFirst(ClaimTypes.MobilePhone)?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var member = await _context.LicInspections
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);

            if (member == null)
                return RedirectToAction("Login");

            // ✅ FIX 4: Guard against null SelectedFacultyId before querying
            Faculty selectedFaculty = null;
            if (model.SelectedFacultyId.HasValue && model.SelectedFacultyId.Value > 0)
            {
                selectedFaculty = await _context.Faculties
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FacultyId == model.SelectedFacultyId.Value);
            }

            byte[]? attendenceDocBytes = null;
            if (model.AttendenceDoc != null && model.AttendenceDoc.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await model.AttendenceDoc.CopyToAsync(memoryStream);
                attendenceDocBytes = memoryStream.ToArray();
            }

            var existingRecord = await _context.LicinspectionDetails
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone
                                       && x.SelectedCollegeCode == model.SelectedCollegeCode);

            if (existingRecord != null)
            {
                existingRecord.Name = member.Name;
                existingRecord.TypeofMember = member.TypeofMember;
                existingRecord.CollegeName = model.Collegename;
                existingRecord.DateOfInspection = model.DateOfInspection;
                existingRecord.IsCompleted = model.IsCompleted;
                existingRecord.SelectedCollegeCode = model.SelectedCollegeCode;
                existingRecord.UpdatedDate = DateTime.Now;
                existingRecord.FacultyId = model.SelectedFacultyId;
                existingRecord.Faculty = selectedFaculty?.FacultyName;

                if (attendenceDocBytes != null)
                    existingRecord.AttendenceDoc = attendenceDocBytes;
            }
            else
            {
                var detail = new LicinspectionDetail
                {
                    Name = member.Name,
                    PhoneNumber = phone,
                    TypeofMember = member.TypeofMember,
                    CollegeName = model.Collegename,
                    DateOfInspection = model.DateOfInspection,
                    IsCompleted = model.IsCompleted,
                    SelectedCollegeCode = model.SelectedCollegeCode,
                    AttendenceDoc = attendenceDocBytes,
                    FacultyId = model.SelectedFacultyId,
                    Faculty = selectedFaculty?.FacultyName,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _context.LicinspectionDetails.AddAsync(detail);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Inspection details submitted successfully.";
            return RedirectToAction("InspectionCollegeDetails");
        }


        [HttpGet]
        public async Task<IActionResult> DownloadAttendanceDoc(int id)
        {
            var record = await _context.LicinspectionDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null || record.AttendenceDoc == null)
                return NotFound();

            return File(record.AttendenceDoc, "application/octet-stream", $"AttendanceDoc_{id}.pdf");
        }
        public class LICClaimDetailsViewModel
        {
            // ── Member Info ───────────────────────────────────────────────────────
            public string? MemberName { get; set; }
            public string? TypeofMember { get; set; }
            public string? PhoneNumber { get; set; }

            // ── Selected College ──────────────────────────────────────────────────
            [Required(ErrorMessage = "Please select a college.")]
            public string? CollegeCode { get; set; }
            public string? CollegeName { get; set; }
            public string? getAirFare { get; set; }

            // ── Mode of Travel ────────────────────────────────────────────────────
            [Required(ErrorMessage = "Please select at least one mode of travel.")]
            public List<string> ModeOfTravel { get; set; } = new();

            // ── Onward Journey ────────────────────────────────────────────────────
            [Required(ErrorMessage = "From place is required.")]
            public string? FromPlace { get; set; }

            [Required(ErrorMessage = "To place is required.")]
            public string? ToPlace { get; set; }

            [Required(ErrorMessage = "Kilometers is required.")]
            [Range(0.1, double.MaxValue, ErrorMessage = "Kilometers must be greater than 0.")]
            public double? Kilometers { get; set; }

            // ── Return Journey ────────────────────────────────────────────────────
            [Required(ErrorMessage = "Return from place is required.")]
            public string? ReturnFromPlace { get; set; }

            [Required(ErrorMessage = "Return to place is required.")]
            public string? ReturnToPlace { get; set; }

            [Required(ErrorMessage = "Return kilometers is required.")]
            public string? ReturnKilometers { get; set; }

            // ── Misc ──────────────────────────────────────────────────────────────
            public string? IsBackToBack { get; set; }
            public int NumberOfDays { get; set; } = 1;
            public bool? isBanglore { get; set; }

            // ── Totals (recalculated server-side) ─────────────────────────────────
            public decimal? TotalCost { get; set; }

            // ── Claim Summary Breakdown (saved to DB, shown in records table) ─────
            public decimal? TravelCost { get; set; }   // TA component
            public decimal? DACost { get; set; }   // Daily Allowance
            public decimal? LCACost { get; set; }   // Local Conveyance Allowance
            public decimal? CollegeCost { get; set; }   // Inspection allowance × colleges
            public decimal? AirFareCost { get; set; }   // Actual air ticket amount
            public decimal? AirRoadCost { get; set; }   // Airport → college fixed rate
            public string? Division { get; set; }   // "bangalore" | "other"
            public bool? IsLCA { get; set; }   // true = LCA rule was applied
            public int? NumberOfDaysSaved { get; set; } // Days stored in DB (avoids clash with form field)

            // ── Assigned Colleges (role-based lookup) ─────────────────────────────
            public List<AssignedCollegeItem> AssignedColleges { get; set; } = new();

            // ── Dropdowns ─────────────────────────────────────────────────────────
            public List<SelectListItem> CollegeOptions { get; set; } = new();
            public List<SelectListItem> ModeOfTravelOptions { get; set; } = new();

            // ── Saved Claims (one row per college) ────────────────────────────────
            public List<LicclaimDetail> SavedClaims { get; set; } = new();
            public bool HasUploadedBill { get; set; }

            // ── Constants ─────────────────────────────────────────────────────────
            public const decimal RatePerKm = 15.00m;
            public const decimal CollegeAllowance = 2500.00m;

            // ── New scalar fields ─────────────────────────────────────────────────────
            public string? Faculty { get; set; }
            public DateOnly? InspectionDate { get; set; }
            public bool HasUploadedAttendence { get; set; }

            public List<SelectListItem> AllCollegeOptions { get; set; } = new();
            // ── New dropdown ──────────────────────────────────────────────────────────
            public List<SelectListItem> FacultyOptions { get; set; } = new();
        }

        public class AssignedCollegeItem
        {
            public string? CollegeName { get; set; }
            public string? CollegeCode { get; set; }
        }

        // ──────────────────────────────────────────────────────────────────────────────
        // LICClaimDetailsViewModel  –  add these new properties to your existing model
        // ──────────────────────────────────────────────────────────────────────────────
        /*
        public class LICClaimDetailsViewModel
        {
            // ... existing properties ...

            // Return journey
            public string?  ReturnFromPlace    { get; set; }
            public string?  ReturnToPlace      { get; set; }
            public double?  ReturnKilometers   { get; set; }

            // TotalCost = (Kilometers + ReturnKilometers) × RatePerKm
            //           + (number of assigned colleges × CollegeAllowance)
            // Keep TotalCost as-is; it will be recalculated server-side.

            public const decimal RatePerKm           = 8.00m;    // adjust as needed
            public const decimal CollegeAllowance    = 2500.00m; // ₹2500 per college
        }
        */

        // ──────────────────────────────────────────────────────────────────────────────
        // Also add to your DB entity (LicInspection) and run a migration:
        //   public string?  ReturnFromPlace  { get; set; }
        //   public string?  ReturnToPlace    { get; set; }
        //   public double?  ReturnKilometers { get; set; }
        // ──────────────────────────────────────────────────────────────────────────────

        #region Claim Details

        [HttpGet]
        public async Task<IActionResult> ClaimDetails()
        {
            // ── 1. Resolve phone ──────────────────────────────────────────────────
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            // ── 2. Load member ────────────────────────────────────────────────────
            var member = await _context.LicInspections
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);
            if (member == null)
                return RedirectToAction("Login");

            string role = member.TypeofMember?.Trim() ?? "";

            // ── 3. Assigned colleges ──────────────────────────────────────────────
            var assignedColleges = await GetAssignedCollegesAsync(role, phone);

            // ── 4. All saved claims for this inspector ────────────────────────────
            var savedClaims = await _context.LicclaimDetails
                .AsNoTracking()
                .Where(x => x.PhoneNumber.Trim() == phone)
                .ToListAsync();

            // ── 5. Build model — identity + collections only ──────────────────────
            // Form fields intentionally left blank so the form is always fresh.
            // Saved data is visible in the Saved Claim Details table below.
            var model = new LICClaimDetailsViewModel
            {
                // Identity — always from LicInspections
                MemberName = member.Name,
                TypeofMember = member.TypeofMember,
                PhoneNumber = member.PhoneNumber,

                // Default days to 1
                NumberOfDays = 1,

                // Collections
                SavedClaims = savedClaims,
                HasUploadedBill = savedClaims.Any(r => r.UploadBills?.Length > 0),
                HasUploadedAttendence = savedClaims.Any(r => r.AttendenceDoc?.Length > 0),
                AssignedColleges = assignedColleges
            };

            // ── 6. Submitted college codes (to exclude from dropdowns) ────────────
            var submittedCodes = savedClaims
                .Select(x => x.CollegeCode)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToHashSet();

            // ── 7. College dropdown ───────────────────────────────────────────────
            model.CollegeOptions = assignedColleges
                .Select(c => new SelectListItem
                {
                    Value = c.CollegeCode,
                    Text = c.CollegeName
                })
                .Where(x => !submittedCodes.Contains(x.Value))
                .Prepend(new SelectListItem { Value = "", Text = "-- Select College --" })
                .ToList();

            // ── 8. All colleges for saved-claims name lookup ──────────────────────
            // Full list — no faculty filter, no exclusions — so every saved claim
            // row can resolve its college code to a display name.
            model.AllCollegeOptions = assignedColleges
                .Select(c => new SelectListItem
                {
                    Value = c.CollegeCode,
                    Text = c.CollegeName
                })
                .ToList();

            // ── 9. Mode of travel options ─────────────────────────────────────────
            model.ModeOfTravelOptions = await GetTravelModesAsync(null);

            // ── 10. Faculty options ───────────────────────────────────────────────
            model.FacultyOptions = await GetFacultyOptionsAsync(null);

            return View(model);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ClaimDetails(
    LICClaimDetailsViewModel model,
    IFormFile? UploadBillsFile,
    IFormFile? AttendenceDocFile,
    [FromForm] decimal DACost,
    [FromForm] decimal LCACost,
    [FromForm] decimal AirFareCost,
    [FromForm] decimal AirRoadCost,
    [FromForm] string? Division,
    [FromForm] bool IsLCA)
{
    // ── 1. Resolve phone ──────────────────────────────────────────────────
    var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
    if (string.IsNullOrWhiteSpace(phone))
        return RedirectToAction("Login");

    // ── 2. Load member ────────────────────────────────────────────────────
    var memberInfo = await _context.LicInspections
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);
    if (memberInfo == null)
        return RedirectToAction("Login");

    string role       = memberInfo.TypeofMember?.Trim() ?? "";
    string memberName = memberInfo.Name?.Trim() ?? "";

    var assignedCols = await GetAssignedCollegesAsync(role, phone);
    string assignedCsv = string.Join(",", assignedCols.Select(c => c.CollegeCode));

    // ── 3. Determine IsBangalore ──────────────────────────────────────────
    bool isBangaloreChk = Request.Form["IsBangalore"]
        .ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

    bool isBangaloreAuto = !string.IsNullOrWhiteSpace(model.ToPlace) &&
        Regex.IsMatch(model.ToPlace.Trim(), "bangalore|bengaluru", RegexOptions.IgnoreCase);

    bool isBangalore = isBangaloreChk || isBangaloreAuto;

    // ── 4. Distances ──────────────────────────────────────────────────────
    double onwardKm = model.Kilometers ?? 0;
    double returnKm = double.TryParse(model.ReturnKilometers, out var rk) ? rk : 0;
    double totalKm  = onwardKm + returnKm;

    // ── 5. Rates ──────────────────────────────────────────────────────────
    decimal daRate      = isBangalore ? 1500m : 900m;
    decimal lcaRate     = isBangalore ? 500m  : 300m;
    decimal airRoadRate = isBangalore ? 2000m : 750m;

    // ── 6. Days ───────────────────────────────────────────────────────────
    int numberOfDays = model.NumberOfDays > 0 ? model.NumberOfDays : 1;

    // ── 7. Apply 40 km rule ───────────────────────────────────────────────
    const double lcaThreshold = 40.0;
    bool applyLCA = totalKm > 0 && totalKm <= lcaThreshold;
    bool applyTA  = totalKm > lcaThreshold;

    // ── 8. Calculate components ───────────────────────────────────────────
    decimal travelCost  = applyTA  ? (decimal)totalKm * LICClaimDetailsViewModel.RatePerKm : 0m;
    decimal daCostCalc  = applyTA  ? numberOfDays * daRate  : 0m;
    decimal lcaCostCalc = applyLCA ? numberOfDays * lcaRate : 0m;

    // ── Always 1 college per claim — never multiply by total assigned count ──
    decimal collegeCost = LICClaimDetailsViewModel.CollegeAllowance; // ₹2,500 × 1

    decimal airFare = AirFareCost > 0 ? AirFareCost : 0m;
    decimal airRoad = 0m;

    var selectedModes = model.ModeOfTravel ?? new List<string>();
    bool airSelected = selectedModes.Any(m => m.Equals("Air", StringComparison.OrdinalIgnoreCase));
    if (airSelected)
        airRoad = AirRoadCost > 0 ? AirRoadCost : airRoadRate;

    // ── 9. Grand total — only this single claim's components ──────────────
    model.TotalCost = travelCost + daCostCalc + lcaCostCalc
                    + collegeCost + airFare + airRoad;

    // ── 10. Populate summary on model ─────────────────────────────────────
    model.TravelCost      = travelCost;
    model.DACost          = daCostCalc;
    model.LCACost         = lcaCostCalc;
    model.CollegeCost     = collegeCost;
    model.AirFareCost     = airFare;
    model.AirRoadCost     = airRoad;
    model.Division        = isBangalore ? "bangalore" : "other";
    model.IsLCA           = applyLCA;
    model.NumberOfDaysSaved = numberOfDays;

    // ── 11. Validate uploaded files ───────────────────────────────────────
    if (UploadBillsFile != null && UploadBillsFile.Length > 0)
    {
        var ext = Path.GetExtension(UploadBillsFile.FileName).ToLowerInvariant();
        var allowedExts = new HashSet<string> { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        if (!allowedExts.Contains(ext))
            ModelState.AddModelError("UploadBills", "Only PDF, JPG, PNG, DOC, or DOCX files are allowed.");
        if (UploadBillsFile.Length > 5 * 1024 * 1024)
            ModelState.AddModelError("UploadBills", "File size must not exceed 5 MB.");
    }

    if (AttendenceDocFile != null && AttendenceDocFile.Length > 0)
    {
        var ext = Path.GetExtension(AttendenceDocFile.FileName).ToLowerInvariant();
        var allowedExts = new HashSet<string> { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        if (!allowedExts.Contains(ext))
            ModelState.AddModelError("AttendenceDoc", "Only PDF, JPG, PNG, DOC, or DOCX files are allowed.");
        if (AttendenceDocFile.Length > 5 * 1024 * 1024)
            ModelState.AddModelError("AttendenceDoc", "File size must not exceed 5 MB.");
    }

    ModelState.Remove("NumberOfDays");

    // ── 12. Return view on validation failure ─────────────────────────────
    if (!ModelState.IsValid)
    {
        model.AssignedColleges = assignedCols;

        var collegesForDropdown = string.IsNullOrWhiteSpace(model.Faculty)
            ? assignedCols
            : await GetCollegesByFacultyAsync(model.Faculty, assignedCols);

        model.CollegeOptions = collegesForDropdown
            .Select(c => new SelectListItem { Value = c.CollegeCode, Text = c.CollegeName })
            .Prepend(new SelectListItem { Value = "", Text = "-- Select College --" })
            .ToList();

        // ── All colleges for name lookup in saved-claims table ────────────
        model.AllCollegeOptions = assignedCols
            .Select(c => new SelectListItem
            {
                Value = c.CollegeCode,
                Text  = c.CollegeName
            })
            .ToList();

        model.ModeOfTravelOptions = await GetTravelModesAsync(null);
        model.FacultyOptions      = await GetFacultyOptionsAsync(model.Faculty);
        model.SavedClaims         = await _context.LicclaimDetails
            .AsNoTracking()
            .Where(x => x.PhoneNumber.Trim() == phone)
            .ToListAsync();
        model.HasUploadedBill       = model.SavedClaims.Any(r => r.UploadBills?.Length > 0);
        model.HasUploadedAttendence = model.SavedClaims.Any(r => r.AttendenceDoc?.Length > 0);
        model.isBanglore            = isBangalore;
        return View(model);
    }

    // ── 13. Resolve selected college ──────────────────────────────────────
    var selectedCollege = await _context.LicInspectionCollegeDetails
        .AsNoTracking()
        .Where(x => x.Collegecode == model.CollegeCode)
        .Select(x => new { x.Collegecode, x.Collegename })
        .FirstOrDefaultAsync();

    // ── 14. Find or create record (one row per inspector per college) ──────
    var existing = await _context.LicclaimDetails
        .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone
                               && x.CollegeCode == model.CollegeCode);
    if (existing == null)
    {
        existing = new LicclaimDetail
        {
            PhoneNumber = phone,
            CreatedDate = DateTime.Now
        };
        _context.LicclaimDetails.Add(existing);
    }

    // ── 15. Map all fields ────────────────────────────────────────────────
    existing.MemberName      = memberName;
    existing.TypeofMember    = role;
    existing.PhoneNumber     = phone;
    existing.ModeOfTravel    = string.Join(",", selectedModes);
    existing.CollegeCode     = selectedCollege?.Collegecode;
    existing.AssignedColleges = assignedCsv;
    existing.AirFare         = airFare > 0 ? airFare.ToString("F2") : null;

    // Onward
    existing.FromPlace  = model.FromPlace;
    existing.ToPlace    = model.ToPlace;
    existing.Kilometers = (decimal?)model.Kilometers;

    // Return
    existing.ReturnFromPlace    = model.ReturnFromPlace;
    existing.ReturnToPlace      = model.ReturnToPlace;
    existing.ReturnKilometers   = returnKm > 0 ? (decimal?)returnKm : null;
    existing.NoofDays           = numberOfDays.ToString();

    existing.IsBanglore  = isBangalore;
    existing.TotalCost   = model.TotalCost;
    existing.UpdatedDate = DateTime.Now;

    // Summary breakdown
    existing.TravelCost  = travelCost;
    existing.Dacost      = daCostCalc;
    existing.Lcacost     = lcaCostCalc;
    existing.CollegeCost = collegeCost;   // always ₹2,500
    existing.AirFareCost = airFare;
    existing.AirRoadCost = airRoad;
    existing.Division    = isBangalore ? "bangalore" : "other";
    existing.IsLca       = applyLCA;
    existing.NumberOfDays = numberOfDays;

    // New fields
    existing.Faculty        = model.Faculty;
    existing.InspectionDate = model.InspectionDate;

    // ── 16. File uploads ──────────────────────────────────────────────────
    if (UploadBillsFile != null && UploadBillsFile.Length > 0)
    {
        using var ms = new MemoryStream();
        await UploadBillsFile.CopyToAsync(ms);
        existing.UploadBills = ms.ToArray();
    }

    if (AttendenceDocFile != null && AttendenceDocFile.Length > 0)
    {
        using var ms = new MemoryStream();
        await AttendenceDocFile.CopyToAsync(ms);
        existing.AttendenceDoc = ms.ToArray();
    }

    // ── 17. Save ──────────────────────────────────────────────────────────
    await _context.SaveChangesAsync();

    string allowanceType = applyLCA
        ? $"LCA ({numberOfDays} day(s) × ₹{lcaRate:N0}): ₹{lcaCostCalc:N2}"
        : $"DA  ({numberOfDays} day(s) × ₹{daRate:N0}): ₹{daCostCalc:N2}";

    string divisionLabel = isBangalore ? "Bangalore Division" : "Other Division";

    TempData["Message"] =
        $"Claim saved for {selectedCollege?.Collegename} [{divisionLabel}] — " +
        $"Onward: {onwardKm} km | Return: {returnKm} km | " +
        $"TA: ₹{travelCost:N2} | {allowanceType} | " +
        $"College Allowance (1 × ₹2,500): ₹{collegeCost:N2} | " +
        $"Air Fare: ₹{airFare:N2} | Air Road: ₹{airRoad:N2} | " +
        $"Grand Total: ₹{model.TotalCost:N2}";

    return RedirectToAction("ClaimDetails");
}


        [HttpGet]
        public async Task<IActionResult> DownloadBill()
        {
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var record = await _context.LicclaimDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);

            if (record?.UploadBills == null || record.UploadBills.Length == 0)
                return NotFound("No bill uploaded.");

            var (contentType, extension) = DetectFileType(record.UploadBills);

            return File(record.UploadBills, contentType, $"Bill_{phone}{extension}");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAttendenceDoc()
        {
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var record = await _context.LicclaimDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);

            if (record?.AttendenceDoc == null || record.AttendenceDoc.Length == 0)
                return NotFound("No attendance document uploaded.");

            var (contentType, extension) = DetectFileType(record.AttendenceDoc);

            return File(record.AttendenceDoc, contentType, $"Attendance_{phone}{extension}");
        }

        private static (string ContentType, string Extension) DetectFileType(byte[] data)
        {
            if (data == null || data.Length < 4)
                return ("application/octet-stream", ".bin");

            // PDF — %PDF
            if (data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46)
                return ("application/pdf", ".pdf");

            // JPG — FF D8 FF
            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                return ("image/jpeg", ".jpg");

            // PNG — 89 50 4E 47
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                return ("image/png", ".png");

            // DOC — D0 CF 11 E0 (OLE2 — old .doc format)
            if (data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0)
                return ("application/msword", ".doc");

            // DOCX — 50 4B 03 04 (ZIP-based — .docx is a ZIP)
            if (data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04)
                return ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx");

            // Fallback
            return ("application/octet-stream", ".bin");
        }

        // ── AJAX: colleges filtered by selected faculty ───────────────────────────
        // Called by JS when the user picks a faculty.
        // Returns only the inspector's assigned colleges that belong to that faculty.
        [HttpGet]
        public async Task<IActionResult> GetCollegesByFaculty(string? facultyId)
        {
            // ── Fix: use same claim name as Login/ClaimDetails ────────────────────
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(phone))
                return Json(new List<object>());

            var member = await _context.LicInspections
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber.Trim() == phone);
            if (member == null)
                return Json(new List<object>());

            // All colleges assigned to this inspector
            // Senate Member filtering is now handled inside GetAssignedCollegesAsync
            var assignedColleges = await GetAssignedCollegesAsync(
                member.TypeofMember?.Trim() ?? "", phone);

            // If no faculty selected, return all assigned colleges
            if (string.IsNullOrWhiteSpace(facultyId))
            {
                return Json(assignedColleges
                    .Select(c => new { value = c.CollegeCode, text = c.CollegeName }));
            }

            // Filter by faculty
            var filtered = await GetCollegesByFacultyAsync(facultyId, assignedColleges);
            return Json(filtered
                .Select(c => new { value = c.CollegeCode, text = c.CollegeName }));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the list of colleges assigned to this inspector based on their role.
        ///
        /// TABLE  : LicInspectionCollegeDetails
        /// FILTER : depends on TypeofMember (role)
        ///
        ///   role = "Academic Council"  → filter by AcMemberPhno
        ///   role = "Senate Members"    → filter by SenetMemberPhNo
        ///   role = "Subject Expertise" → filter by SubjectExpertisePhNo
        ///   role = anything else       → returns empty list
        ///
        /// Each matching row gives one college (Collegecode + Collegename).
        /// Results are ordered A→Z by college name.
        /// </summary>
        private async Task<List<AssignedCollegeItem>> GetAssignedCollegesAsync(string role, string phone)
        {
            var query = _context.LicInspectionCollegeDetails.AsNoTracking();

            // ── Senate Members ────────────────────────────────────────────────────
            // Only show colleges where their data exists in LicInspectionOtherDetails
            // (IsAttended check removed here — just existence in the table is enough)
            if (string.Equals(role, "Senate Members", StringComparison.OrdinalIgnoreCase))
            {
                // Step 1: Get college codes that exist for this Senate Member
                //         in LicInspectionOtherDetails
                var senatCollegeCodes = await _context.LicInspectionOtherDetails
                    .AsNoTracking()
                    .Where(x => x.Phonenumber.Trim() == phone)
                    .Select(x => x.CollegeCode.Trim())
                    .Distinct()
                    .ToListAsync();

                // Step 2: If no data in LicInspectionOtherDetails → return empty
                if (!senatCollegeCodes.Any())
                    return new List<AssignedCollegeItem>();

                // Step 3: Return only assigned colleges that exist in OtherDetails
                return await query
                    .Where(x => x.SenetMemberPhNo.ToString() == phone
                             && senatCollegeCodes.Contains(x.Collegecode.Trim()))
                    .OrderBy(x => x.Collegename)
                    .Select(x => new AssignedCollegeItem
                    {
                        CollegeName = x.Collegename,
                        CollegeCode = x.Collegecode
                    })
                    .ToListAsync();
            }

            // ── Academic Council & Subject Expertise ──────────────────────────────
            // Display all assigned colleges directly — no extra condition
            IQueryable<LicInspectionCollegeDetail> filtered = role switch
            {
                "Academic Council" => query.Where(x => x.AcMemberPhno.ToString() == phone),
                "Subject Expertise" => query.Where(x => x.SubjectExpertisePhNo.ToString() == phone),
                _ => Enumerable.Empty<LicInspectionCollegeDetail>().AsQueryable()
            };

            return await filtered
                .OrderBy(x => x.Collegename)
                .Select(x => new AssignedCollegeItem
                {
                    CollegeName = x.Collegename,
                    CollegeCode = x.Collegecode
                })
                .ToListAsync();
        }



        /// <summary>
        /// Returns all available travel modes for the Mode-of-Travel multi-select dropdown.
        ///
        /// TABLE  : [Admission_Affiliation].[dbo].[LIC_ModeofTravel]
        /// COLUMN : Modeoftravel
        ///
        /// The optional `selected` parameter pre-selects one item (used when
        /// the controller wants a single-select scenario; for multi-select the
        /// view handles selection via its own JS, so null is passed).
        ///
        /// A blank "-- Select --" placeholder is prepended.
        /// </summary>
        private async Task<List<SelectListItem>> GetTravelModesAsync(string? selected)
        {
            var modes = await _context.Database
                .SqlQuery<string>(
                    $"SELECT Modeoftravel FROM [Admission_Affiliation].[dbo].[LIC_ModeofTravel] ORDER BY Modeoftravel"
                )
                .ToListAsync();

            return modes
                .Select(m => new SelectListItem
                {
                    Value = m,
                    Text = m,
                    Selected = string.Equals(m, selected, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = "", Text = "-- Select Mode of Travel --" })
                .ToList();
        }


        /// <summary>
        /// Returns ALL active faculty for the Faculty dropdown.
        /// No college filter — faculty is the INDEPENDENT (parent) dropdown.
        /// College is the DEPENDENT (child) dropdown that reacts to faculty selection.
        ///
        /// TABLE  : [Admission_Affiliation].[dbo].[Faculty]
        /// COLUMNS used:
        ///   FacultyId   → SelectListItem.Value
        ///   FacultyName → SelectListItem.Text
        ///   Status      → filter: only rows where Status == "true"
        ///
        /// The optional `selectedFacultyId` pre-selects the saved value on page load.
        /// A blank "-- Select Faculty --" placeholder is inserted at position 0.
        /// </summary>
        private async Task<List<SelectListItem>> GetFacultyOptionsAsync(string? selectedFacultyId)
        {
            var list = await _context.Faculties
                .AsNoTracking()
                .Where(f => f.Status == "Active" && f.FacultyId == 9)          // only active faculty
                .OrderBy(f => f.FacultyName)
                .Select(f => new SelectListItem
                {
                    Value = f.FacultyId.ToString(),
                    Text = f.FacultyName,
                    Selected = f.FacultyId.ToString() == selectedFacultyId
                })
                .ToListAsync();

            list.Insert(0, new SelectListItem { Value = "", Text = "-- Select Faculty --" });
            return list;
        }


        /// <summary>
        /// Returns the subset of an inspector's assigned colleges that belong
        /// to the selected faculty.
        ///
        /// HOW THE LINK WORKS:
        ///
        ///   Faculty table                   LicInspectionCollegeDetails table
        ///   ─────────────────               ──────────────────────────────────────────
        ///   FacultyId  (PK)      ────────►  FacultyId  (FK)   ← the join column
        ///   FacultyName                      Collegecode
        ///   Status                           Collegename
        ///                                    AcMemberPhno
        ///                                    SenetMemberPhNo
        ///                                    SubjectExpertisePhNo
        ///
        /// STEPS:
        ///   1. Query LicInspectionCollegeDetails for all rows where FacultyId == facultyId
        ///      → gives us a list of college codes linked to that faculty
        ///   2. Intersect that list with `assignedColleges` (already fetched by
        ///      GetAssignedCollegesAsync) so the inspector only sees colleges
        ///      they are personally assigned to — never the full university list.
        ///
        /// ⚠️  IMPORTANT — adjust the property name if needed:
        ///     c.FacultyId  →  replace with the actual EF property on
        ///     LicInspectionCollegeDetail that holds the faculty reference.
        ///     Common alternatives: c.FacultyCode, c.EMS_FacultyId, c.FacultyRefId
        ///     Check your DbContext / entity class to confirm the exact name.
        /// </summary>
        private async Task<List<AssignedCollegeItem>> GetCollegesByFacultyAsync(
            string facultyId,
            List<AssignedCollegeItem> assignedColleges)
        {
            // Step 1 — college codes linked to this faculty in the inspection table
            var linkedCollegeCodes = await _context.LicInspectionCollegeDetails
                .AsNoTracking()
                .Where(c => c.Facultycode.ToString() == facultyId)  // ← adjust property name
                .Select(c => c.Collegecode)
                .Distinct()
                .ToListAsync();

            // Step 2 — keep only the inspector's own assigned colleges
            return assignedColleges
                .Where(c => linkedCollegeCodes.Contains(c.CollegeCode))
                .ToList();
        }

        #endregion



        public class LICInspectionIndexViewModel
        {
            public List<LICInspectionCollegeViewModel> Colleges { get; set; } = new();
            public int? SelectedCollegeId { get; set; }
            public LICInspectionCollegeViewModel? SelectedCollege { get; set; }
        }

        // College data ViewModel
        public class LICInspectionCollegeViewModel
        {
            public int Id { get; set; }

            [Display(Name = "College Name")]
            public string Collegename { get; set; } = string.Empty;

            [Display(Name = "College Place")]
            public string CollegePlace { get; set; } = string.Empty;

            [Display(Name = "College Code")]
            public string Collegecode { get; set; } = string.Empty;

            // Members
            public CollegeMemberViewModel? ACMember { get; set; }
            public CollegeMemberViewModel? SenetMember { get; set; }
            public CollegeMemberViewModel? SubjectExpertiseMember { get; set; }

            // Raw data for updates
            public string RawACMember { get; set; } = string.Empty;
            public string RawAcMemberPhno { get; set; } = string.Empty;
            public string RawSenetMember { get; set; } = string.Empty;
            public string RawSenetMemberPhno { get; set; } = string.Empty;
            public string RawSubjectExpertise { get; set; } = string.Empty;
            public string RawSubjectExpertisePhno { get; set; } = string.Empty;
        }

        public class CollegeMemberViewModel
        {
            public string Designation { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }

        // POST ViewModel for updates
        public class UpdateCollegeMemberViewModel
        {
            public int CollegeId { get; set; }
            public string Designation { get; set; } = string.Empty;
            public string MemberName { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }



        // GET: Load colleges list for dropdown
        [HttpGet]
        public IActionResult LIC_Admin()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetColleges()
        {
            try
            {
                var colleges = await _context.LicInspectionCollegeDetails
                    .AsNoTracking()
                    .Select(c => new
                    {
                        c.Id,
                        c.Collegename,
                        c.CollegePlace,
                        c.Collegecode,
                        c.Acmember,
                        c.AcMemberPhno,
                        c.SenetMember,
                        c.SenetMemberPhNo,
                        c.SubjectExpertise,
                        c.SubjectExpertisePhNo
                    })
                    .OrderBy(c => c.Collegename)
                    .Take(1000)
                    .ToListAsync();

                return Json(new { success = true, data = colleges });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCollegeDetails([FromBody] CollegeIdRequest request)
        {
            if (request == null || request.CollegeId <= 0)
                return Json(new { success = false, message = "Invalid college ID." });

            try
            {
                var college = await _context.LicInspectionCollegeDetails
                    .AsNoTracking()
                    .Where(c => c.Id == request.CollegeId)
                    .Select(c => new
                    {
                        c.Id,
                        c.Collegename,
                        c.CollegePlace,
                        c.Collegecode,
                        c.Acmember,
                        c.AcMemberPhno,
                        c.SenetMember,
                        c.SenetMemberPhNo,
                        c.SubjectExpertise,
                        c.SubjectExpertisePhNo
                    })
                    .FirstOrDefaultAsync();

                if (college == null)
                    return Json(new { success = false, message = "College not found." });

                return Json(new { success = true, data = college });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMember([FromBody] UpdateMemberRequest request)
        {
            if (request == null)
                return Json(new { success = false, message = "Request body is missing." });

            if (request.CollegeId <= 0)
                return Json(new { success = false, message = "Invalid college ID." });

            if (string.IsNullOrWhiteSpace(request.MemberName))
                return Json(new { success = false, message = "Member name is required." });

            if (string.IsNullOrWhiteSpace(request.Designation))
                return Json(new { success = false, message = "Designation is required." });

            if (request.PhoneNumber.HasValue &&
                (request.PhoneNumber < 1000000000 || request.PhoneNumber > 9999999999))
                return Json(new { success = false, message = "Phone number must be 10 digits." });

            try
            {
                var college = await _context.LicInspectionCollegeDetails
                    .FirstOrDefaultAsync(c => c.Id == request.CollegeId);

                if (college == null)
                    return Json(new { success = false, message = "College not found." });

                switch (request.Designation.Trim().ToLower())
                {
                    case "ac member":
                        college.Acmember = request.MemberName.Trim();
                        college.AcMemberPhno = request.PhoneNumber.HasValue
                            ? (long?)Convert.ToInt64(request.PhoneNumber.Value) : null;
                        break;

                    case "senet member":
                        college.SenetMember = request.MemberName.Trim();
                        college.SenetMemberPhNo = request.PhoneNumber.HasValue
                            ? (long?)Convert.ToInt64(request.PhoneNumber.Value) : null;
                        break;

                    case "subject expertise":
                        college.SubjectExpertise = request.MemberName.Trim();
                        college.SubjectExpertisePhNo = request.PhoneNumber.HasValue
                            ? (long?)Convert.ToInt64(request.PhoneNumber.Value) : null;
                        break;

                    default:
                        return Json(new
                        {
                            success = false,
                            message = $"Unknown designation '{request.Designation}'. " +
                                       "Valid: ac member, senet member, subject expertise."
                        });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Member updated successfully!" });
            }
            catch (DbUpdateException dbEx)
            {
                return Json(new
                {
                    success = false,
                    message = "Database error: " + (dbEx.InnerException?.Message ?? dbEx.Message)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class CollegeIdRequest
        {
            public int CollegeId { get; set; }
        }

        public class UpdateMemberRequest
        {
            public int CollegeId { get; set; }
            public string Designation { get; set; } = string.Empty;
            public string MemberName { get; set; } = string.Empty;
            public double? PhoneNumber { get; set; }
        }
        public async Task<IActionResult> OtherDetails()
        {
            // ✅ Use plain "PhoneNumber" string — matches claim stored in Login
            var phoneNumber = User.FindFirst("PhoneNumber")?.Value?.Trim();
            var typeofMember = User.FindFirstValue("TypeofMember");

            if (string.IsNullOrEmpty(phoneNumber))
                return RedirectToAction("Login");

            if (typeofMember != "Senate Members")
                return RedirectToAction("AccessDenied");

            var licInspectionUser = await _context.LicInspections
                .Where(e => e.PhoneNumber == phoneNumber)
                .FirstOrDefaultAsync();

            if (licInspectionUser == null)
                return RedirectToAction("Login");

            // ✅ Fetch all college rows where this senate member's phone matches SenetMember_PhNo
            var mappedCollegeDetails = await _context.LicInspectionCollegeDetails
                .Where(e => e.SenetMemberPhNo.ToString() == phoneNumber)
                .OrderBy(e => e.Collegename)
                .ToListAsync();

            if (!mappedCollegeDetails.Any())
            {
                TempData["Error"] = "No college details found for this Senate Member.";
                return RedirectToAction("InspectionDetails");
            }

            // ✅ Collect all phone numbers in this senate member's group
            var mappedPhoneNumbers = mappedCollegeDetails
                .SelectMany(x => new[]
                {
                    x.SenetMemberPhNo?.ToString(),
                    x.AcMemberPhno?.ToString(),
                    x.SubjectExpertisePhNo?.ToString()
                })
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            // ✅ College codes assigned to this senate member
            var collegeCodes = mappedCollegeDetails
                .Where(c => !string.IsNullOrWhiteSpace(c.Collegecode))
                .Select(c => c.Collegecode.Trim())
                .Distinct()
                .ToList();

            // ✅ Fetch all claims for everyone in this group
            var claimDetails = await _context.LicclaimDetails
                .Where(c => mappedPhoneNumbers.Contains(c.PhoneNumber))
                .ToListAsync();

            // ✅ PhoneNumber → Total claim amount
            var claimAmountMap = claimDetails
                .GroupBy(c => c.PhoneNumber)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.TotalCost ?? 0)
                );

            // ✅ CollegeCode → Latest DateOfInspection from LicclaimDetails
            var inspectionDateMap = claimDetails
                .Where(c => c.CollegeCode != null && c.InspectionDate != null)
                .GroupBy(c => c.CollegeCode.Trim())
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.InspectionDate).First().InspectionDate
                );

            // ✅ Other details for completed tracking
            var otherDetails = await _context.LicInspectionOtherDetails
                .Where(e => collegeCodes.Contains(e.CollegeCode))
                .ToListAsync();

            var completedSet = otherDetails
                .Select(x => $"{x.CollegeCode}_{x.MemberCode}")
                .ToHashSet();

            // ✅ Group by college
            var groupedColleges = mappedCollegeDetails
                .GroupBy(e => new
                {
                    Collegecode = e.Collegecode?.Trim(),
                    e.Collegename,
                    e.CollegePlace
                })
                .Select(g => new CollegeGroupVM
                {
                    CollegeCode = g.Key.Collegecode,
                    CollegeName = g.Key.Collegename,
                    CollegePlace = g.Key.CollegePlace,

                    SenetMember = g.First().SenetMember,
                    SenetMemberPhNo = g.First().SenetMemberPhNo?.ToString(),

                    DateOfInspection = inspectionDateMap.TryGetValue(
                        g.Key.Collegecode ?? "", out var date) ? date : null,

                    ACMembers = g
                        .Where(e => !string.IsNullOrEmpty(e.Acmember))
                        .Select(e => new ACMemberVM
                        {
                            Id = e.Id,
                            Name = e.Acmember,
                            PhoneNo = e.AcMemberPhno?.ToString(),
                            InspectionAmount = claimAmountMap.TryGetValue(
                                e.AcMemberPhno?.ToString() ?? "", out var acAmt) ? acAmt : 0
                        })
                        .DistinctBy(x => x.PhoneNo)
                        .OrderBy(x => x.Name)
                        .ToList(),

                    SubjectExpertiseMembers = g
                        .Where(e => !string.IsNullOrEmpty(e.SubjectExpertise))
                        .Select(e => new SubjectExpertiseVM
                        {
                            Id = e.Id,
                            Name = e.SubjectExpertise,
                            PhoneNo = e.SubjectExpertisePhNo?.ToString(),
                            InspectionAmount = claimAmountMap.TryGetValue(
                                e.SubjectExpertisePhNo?.ToString() ?? "", out var seAmt) ? seAmt : 0
                        })
                        .DistinctBy(x => x.PhoneNo)
                        .OrderBy(x => x.Name)
                        .ToList()
                })
                .OrderBy(x => x.CollegeName)
                .ToList();

            var viewModel = new LicInspectionOtherDetailsViewModel
            {
                SenetMemberName = licInspectionUser.Name,
                GroupedColleges = groupedColleges,
                CompletedSet = completedSet,
                OtherDetailsList = otherDetails.Select(e => new OtherDetails
                {
                    Id = e.Id,
                    SelectedMemberCode = e.MemberCode,
                    collegeName = e.CollegeName,
                    IsAttended = (bool)e.IsAttended,
                    SelectedMemberName = e.MemberName,
                    DateOfInspection = e.InspectionDate,
                    MemberType = e.MemberCode.Contains("AC") ? "AC Member" : "SE Member"
                }).ToList()
            };

            return View(viewModel);
        }


        // ✅ Popup: Fetch claim breakdown for a specific member + college
        public async Task<IActionResult> ClaimDetailsPopup(string phoneNumber, string collegeCode)
        {
            var claims = await _context.LicclaimDetails
                .Where(x => x.PhoneNumber == phoneNumber && x.CollegeCode == collegeCode)
                .Select(x => new ClaimsAmountvm
                {
                    PhoneNumber = x.PhoneNumber,
                    CollegeCode = x.CollegeCode,
                    ModeOfTransport = x.ModeOfTravel,
                    FromPlace = x.FromPlace,
                    ToPlace = x.ToPlace,
                    KiloMeters = x.Kilometers.ToString(),
                    ReturnFromPlace = x.ReturnFromPlace,
                    ReturnToPlace = x.ReturnToPlace,
                    ReturnKilometers = x.ReturnKilometers.ToString(),
                    TotalCost = (int)(x.TotalCost ?? 0),
                    NoOfDays = x.NumberOfDays ?? 0,
                    TravelCost = x.TravelCost ?? 0,
                    DACost = x.Dacost ?? 0,
                    LCACost = x.Lcacost ?? 0,
                    CollegeCost = x.CollegeCost ?? 0,
                    Airfare = x.AirFareCost ?? 0
                })
                .ToListAsync();

            return Json(claims);
        }

        [HttpPost]
        public async Task<IActionResult> SaveOtherDetails(LicInspectionOtherDetailsViewModel vm)
        {
            // ── Auth ──────────────────────────────────────────────────────────────
            var phoneNumber = User.FindFirst("PhoneNumber")?.Value?.Trim();
            if (string.IsNullOrEmpty(phoneNumber))
                return RedirectToAction("Login");

            // ── Filter submitted rows ─────────────────────────────────────────────
            var validItems = vm.PendingList?
                .Where(x => !string.IsNullOrWhiteSpace(x.SelectedMemberCode))
                .ToList() ?? new List<OtherDetails>();

            if (!validItems.Any())
            {
                TempData["Error"] = "No valid member details found in the submitted form.";
                return RedirectToAction(nameof(OtherDetails));
            }

            // ── College codes in this form (one per-college form = one code) ───────
            var submittedCollegeCodes = validItems
                .Select(x => x.collegeCode?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            // ── College details for submitted colleges ────────────────────────────
            var collegeDetails = await _context.LicInspectionCollegeDetails
                .Where(e => submittedCollegeCodes.Contains(e.Collegecode))
                .ToListAsync();

            // ── All mapped phones (senate + AC + SE) — same as GET ────────────────
            var mappedPhoneNumbers = collegeDetails
                .SelectMany(x => new[]
                {
            x.SenetMemberPhNo?.ToString(),
            x.AcMemberPhno?.ToString(),
            x.SubjectExpertisePhNo?.ToString()
                })
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            // ── Date lookup: LicclaimDetails — EXACTLY mirrors the GET action ─────
            // GET:  inspectionDateMap = claimDetails grouped by c.CollegeCode → c.InspectionDate
            // Previous POSTs queried LicinspectionDetails which had no matching rows → always empty
            var claimDetails = await _context.LicclaimDetails
                .Where(c => mappedPhoneNumbers.Contains(c.PhoneNumber) &&
                            c.CollegeCode != null &&
                            c.InspectionDate != null)
                .ToListAsync();

            var inspectionDateMap = claimDetails
                .GroupBy(c => c.CollegeCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.InspectionDate).First().InspectionDate,
                    StringComparer.OrdinalIgnoreCase
                );

            // ── Member name map ───────────────────────────────────────────────────
            var memberNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in collegeDetails)
            {
                if (c.AcMemberPhno.HasValue && !string.IsNullOrEmpty(c.Acmember))
                    memberNameMap.TryAdd(c.AcMemberPhno.Value.ToString(), c.Acmember);

                if (c.SubjectExpertisePhNo.HasValue && !string.IsNullOrEmpty(c.SubjectExpertise))
                    memberNameMap.TryAdd(c.SubjectExpertisePhNo.Value.ToString(), c.SubjectExpertise);
            }

            // ── Existing records for upsert ───────────────────────────────────────
            var memberPhones = validItems
                .Select(x => x.SelectedMemberCode?.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            var existingRecords = await _context.LicInspectionOtherDetails
                .Where(x =>
                    x.Phonenumber == phoneNumber &&
                    memberPhones.Contains(x.MemberCode) &&
                    submittedCollegeCodes.Contains(x.CollegeCode))
                .ToListAsync();

            var existingMap = existingRecords
                .ToDictionary(x => (x.MemberCode?.Trim(), x.CollegeCode?.Trim()));

            // ── Process rows ──────────────────────────────────────────────────────
            var newRecords = new List<LicInspectionOtherDetail>();
            var hasErrors = false;

            for (int i = 0; i < validItems.Count; i++)
            {
                var item = validItems[i];
                var trimmedCollegeCode = item.collegeCode?.Trim();

                if (!inspectionDateMap.TryGetValue(trimmedCollegeCode, out var resolvedDate) || resolvedDate == null)
                {
                    ModelState.AddModelError(
                        $"PendingList[{i}].DateOfInspection",
                        $"Inspection date not found for {item.collegeName}. Please contact admin."
                    );
                    hasErrors = true;
                    continue;
                }

                if (!memberNameMap.TryGetValue(item.SelectedMemberCode, out var memberName))
                    memberName = item.SelectedMemberName;

                var collegeName = collegeDetails
                    .FirstOrDefault(c => string.Equals(
                        c.Collegecode?.Trim(), trimmedCollegeCode,
                        StringComparison.OrdinalIgnoreCase))
                    ?.Collegename ?? item.collegeName;

                var key = (item.SelectedMemberCode?.Trim(), trimmedCollegeCode);

                if (existingMap.TryGetValue(key, out var existing))
                {
                    existing.IsAttended = item.IsAttended;
                    existing.MemberName = memberName;
                    existing.CollegeName = collegeName;
                }
                else
                {
                    newRecords.Add(new LicInspectionOtherDetail
                    {
                        CollegeCode = trimmedCollegeCode,
                        CollegeName = collegeName,
                        Phonenumber = phoneNumber,
                        MemberCode = item.SelectedMemberCode?.Trim(),
                        MemberName = memberName,
                        InspectionDate = resolvedDate,
                        IsAttended = item.IsAttended,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            // ── Validation error → reload full page ───────────────────────────────
            if (hasErrors)
            {
                await ReloadViewModel(vm, phoneNumber);
                return View("OtherDetails", vm);
            }

            // ── Persist ───────────────────────────────────────────────────────────
            if (newRecords.Any())
                _context.LicInspectionOtherDetails.AddRange(newRecords);

            await _context.SaveChangesAsync();

            var savedCollegeName = validItems
                .Select(x => x.collegeName)
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "College";

            TempData["Success"] = $"Inspection details for {savedCollegeName} saved successfully.";
            return RedirectToAction(nameof(OtherDetails));
        }

        // ── ReloadViewModel — mirrors GET exactly ─────────────────────────────────
        private async Task ReloadViewModel(LicInspectionOtherDetailsViewModel vm, string phoneNumber)
        {
            var licInspectionUser = await _context.LicInspections
                .Where(e => e.PhoneNumber == phoneNumber)
                .FirstOrDefaultAsync();

            var mappedCollegeDetails = await _context.LicInspectionCollegeDetails
                .Where(e => e.SenetMemberPhNo.ToString() == phoneNumber)
                .OrderBy(e => e.Collegename)
                .ToListAsync();

            var mappedPhoneNumbers = mappedCollegeDetails
                .SelectMany(x => new[]
                {
            x.SenetMemberPhNo?.ToString(),
            x.AcMemberPhno?.ToString(),
            x.SubjectExpertisePhNo?.ToString()
                })
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .ToList();

            var collegeCodes = mappedCollegeDetails
                .Where(c => !string.IsNullOrWhiteSpace(c.Collegecode))
                .Select(c => c.Collegecode.Trim())
                .Distinct()
                .ToList();

            // Mirror GET: LicclaimDetails for both amounts and dates
            var claimDetails = await _context.LicclaimDetails
                .Where(c => mappedPhoneNumbers.Contains(c.PhoneNumber))
                .ToListAsync();

            var claimAmountMap = claimDetails
                .GroupBy(c => c.PhoneNumber)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalCost ?? 0));

            var inspectionDateMap = claimDetails
                .Where(c => c.CollegeCode != null && c.InspectionDate != null)
                .GroupBy(c => c.CollegeCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.InspectionDate).First().InspectionDate,
                    StringComparer.OrdinalIgnoreCase
                );

            var otherDetails = await _context.LicInspectionOtherDetails
                .Where(e =>
                    e.Phonenumber == phoneNumber &&
                    collegeCodes.Contains(e.CollegeCode))
                .ToListAsync();

            var acPhoneSet = mappedCollegeDetails
                .Where(c => c.AcMemberPhno.HasValue)
                .Select(c => c.AcMemberPhno.Value.ToString())
                .ToHashSet();

            vm.SenetMemberName = licInspectionUser?.Name;

            vm.GroupedColleges = mappedCollegeDetails
                .GroupBy(e => new { Collegecode = e.Collegecode?.Trim(), e.Collegename, e.CollegePlace })
                .Select(g => new CollegeGroupVM
                {
                    CollegeCode = g.Key.Collegecode,
                    CollegeName = g.Key.Collegename,
                    CollegePlace = g.Key.CollegePlace,
                    SenetMember = g.First().SenetMember,
                    SenetMemberPhNo = g.First().SenetMemberPhNo?.ToString(),
                    DateOfInspection = inspectionDateMap.TryGetValue(
                        g.Key.Collegecode ?? "", out var d) ? d : null,
                    ACMembers = g
                        .Where(e => !string.IsNullOrEmpty(e.Acmember))
                        .Select(e => new ACMemberVM
                        {
                            Id = e.Id,
                            Name = e.Acmember,
                            PhoneNo = e.AcMemberPhno?.ToString(),
                            InspectionAmount = claimAmountMap.TryGetValue(
                                e.AcMemberPhno?.ToString() ?? "", out var acAmt) ? acAmt : 0
                        })
                        .DistinctBy(x => x.PhoneNo)
                        .OrderBy(x => x.Name)
                        .ToList(),
                    SubjectExpertiseMembers = g
                        .Where(e => !string.IsNullOrEmpty(e.SubjectExpertise))
                        .Select(e => new SubjectExpertiseVM
                        {
                            Id = e.Id,
                            Name = e.SubjectExpertise,
                            PhoneNo = e.SubjectExpertisePhNo?.ToString(),
                            InspectionAmount = claimAmountMap.TryGetValue(
                                e.SubjectExpertisePhNo?.ToString() ?? "", out var seAmt) ? seAmt : 0
                        })
                        .DistinctBy(x => x.PhoneNo)
                        .OrderBy(x => x.Name)
                        .ToList()
                })
                .OrderBy(x => x.CollegeName)
                .ToList();

            vm.OtherDetailsList = otherDetails
                .Select(e => new OtherDetails
                {
                    Id = e.Id,
                    SelectedMemberCode = e.MemberCode,
                    SelectedMemberName = e.MemberName,
                    collegeName = e.CollegeName,
                    DateOfInspection = e.InspectionDate,
                    IsAttended = (bool)e.IsAttended,
                    MemberType = acPhoneSet.Contains(e.MemberCode) ? "AC Member" : "SE Member"
                })
                .ToList();

            vm.CompletedSet = otherDetails
                .Select(x => $"{x.CollegeCode}_{x.MemberCode}")
                .ToHashSet();
        }

        // ── Shared reload helper — keeps the error path DRY ──────────────────────
        //private async Task ReloadViewModel(LicInspectionOtherDetailsViewModel vm, string phoneNumber)
        //{
        //    var licInspectionUser = await _context.LicInspections
        //        .Where(e => e.PhoneNumber == phoneNumber)
        //        .FirstOrDefaultAsync();

        //    var allCollegeDetails = await _context.LicInspectionCollegeDetails
        //        .Where(e => e.SenetMemberPhNo.ToString() == phoneNumber)
        //        .OrderBy(e => e.Collegename)
        //        .ToListAsync();

        //    var allCollegeCodes = allCollegeDetails
        //        .Select(c => c.Collegecode?.Trim())
        //        .Where(c => !string.IsNullOrWhiteSpace(c))
        //        .Distinct()
        //        .ToList();

        //    // FIX: use senate member's phone only — same fix as the save path above
        //    var allInspectionRows = await _context.LicinspectionDetails
        //        .Where(e =>
        //            e.SelectedCollegeCode != null &&
        //            allCollegeCodes.Contains(e.SelectedCollegeCode.Trim()) &&
        //            e.PhoneNumber == phoneNumber)
        //        .OrderByDescending(e => e.CreatedDate)
        //        .ToListAsync();

        //    var allDateMap = allInspectionRows
        //        .GroupBy(e => e.SelectedCollegeCode?.Trim(), StringComparer.OrdinalIgnoreCase)
        //        .ToDictionary(g => g.Key, g => g.First().DateOfInspection, StringComparer.OrdinalIgnoreCase);

        //    // Claim amounts still use all mapped phones (AC + SE submitted their own claims)
        //    var allMappedPhones = allCollegeDetails
        //        .SelectMany(x => new[]
        //        {
        //    x.SenetMemberPhNo?.ToString(),
        //    x.AcMemberPhno?.ToString(),
        //    x.SubjectExpertisePhNo?.ToString()
        //        })
        //        .Where(p => !string.IsNullOrWhiteSpace(p))
        //        .Distinct()
        //        .ToList();

        //    var allClaimDetails = await _context.LicclaimDetails
        //        .Where(c => allMappedPhones.Contains(c.PhoneNumber))
        //        .ToListAsync();

        //    var allClaimAmountMap = allClaimDetails
        //        .GroupBy(c => c.PhoneNumber)
        //        .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalCost ?? 0));

        //    var allOtherDetails = await _context.LicInspectionOtherDetails
        //        .Where(x =>
        //            x.Phonenumber == phoneNumber &&
        //            allCollegeCodes.Contains(x.CollegeCode))
        //        .ToListAsync();

        //    var allAcPhoneSet = allCollegeDetails
        //        .Where(c => c.AcMemberPhno.HasValue)
        //        .Select(c => c.AcMemberPhno.Value.ToString())
        //        .ToHashSet();

        //    vm.SenetMemberName = licInspectionUser?.Name;

        //    vm.GroupedColleges = allCollegeDetails
        //        .GroupBy(e => new { Collegecode = e.Collegecode?.Trim(), e.Collegename, e.CollegePlace })
        //        .Select(g => new CollegeGroupVM
        //        {
        //            CollegeCode = g.Key.Collegecode,
        //            CollegeName = g.Key.Collegename,
        //            CollegePlace = g.Key.CollegePlace,
        //            SenetMember = g.First().SenetMember,
        //            SenetMemberPhNo = g.First().SenetMemberPhNo?.ToString(),
        //            DateOfInspection = allDateMap.TryGetValue(g.Key.Collegecode ?? "", out var d) ? d : null,
        //            ACMembers = g
        //                .Where(e => !string.IsNullOrEmpty(e.Acmember))
        //                .Select(e => new ACMemberVM
        //                {
        //                    Id = e.Id,
        //                    Name = e.Acmember,
        //                    PhoneNo = e.AcMemberPhno?.ToString(),
        //                    InspectionAmount = allClaimAmountMap.TryGetValue(
        //                        e.AcMemberPhno?.ToString() ?? "", out var acAmt) ? acAmt : 0
        //                })
        //                .DistinctBy(x => x.PhoneNo)
        //                .OrderBy(x => x.Name)
        //                .ToList(),
        //            SubjectExpertiseMembers = g
        //                .Where(e => !string.IsNullOrEmpty(e.SubjectExpertise))
        //                .Select(e => new SubjectExpertiseVM
        //                {
        //                    Id = e.Id,
        //                    Name = e.SubjectExpertise,
        //                    PhoneNo = e.SubjectExpertisePhNo?.ToString(),
        //                    InspectionAmount = allClaimAmountMap.TryGetValue(
        //                        e.SubjectExpertisePhNo?.ToString() ?? "", out var seAmt) ? seAmt : 0
        //                })
        //                .DistinctBy(x => x.PhoneNo)
        //                .OrderBy(x => x.Name)
        //                .ToList()
        //        })
        //        .OrderBy(x => x.CollegeName)
        //        .ToList();

        //    vm.OtherDetailsList = allOtherDetails
        //        .Select(e => new OtherDetails
        //        {
        //            Id = e.Id,
        //            SelectedMemberCode = e.MemberCode,
        //            SelectedMemberName = e.MemberName,
        //            collegeName = e.CollegeName,
        //            DateOfInspection = e.InspectionDate,
        //            IsAttended = (bool)e.IsAttended,
        //            MemberType = allAcPhoneSet.Contains(e.MemberCode) ? "AC Member" : "SE Member"
        //        })
        //        .ToList();

        //    vm.CompletedSet = allOtherDetails
        //        .Select(x => $"{x.CollegeCode}_{x.MemberCode}")
        //        .ToHashSet();
        //}

        [HttpDelete]
        public async Task<IActionResult> DeleteOtherDetails(int id)
        {
            var record = await _context.LicInspectionOtherDetails.FindAsync(id);
            if (record == null)
                return NotFound();

            _context.LicInspectionOtherDetails.Remove(record);
            await _context.SaveChangesAsync();

            return Ok();
        }


        ///////////////////////////////////////////////////////////////////////////////
        ///

        // =========================================================
        // GET: DASHBOARD
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Dashboard(string searchTerm = "", string statusFilter = "", int pageNumber = 1)
        {
            // ✅ Use plain "PhoneNumber" string — matches claim stored in Login
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            var name = User.FindFirst(ClaimTypes.Name)?.Value?.Trim();
            var memberType = User.FindFirst("TypeofMember")?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            // Fallback: re-fetch from DB if memberType is somehow missing
            if (string.IsNullOrWhiteSpace(memberType))
            {
                var user = await _context.LicInspections
                    .FirstOrDefaultAsync(x => x.PhoneNumber == phone);

                if (user == null)
                    return RedirectToAction("Login");

                name = user.Name ?? user.PhoneNumber;
                memberType = user.TypeofMember ?? "Unknown";
            }

            var vm = await BuildDashboardAsync(phone, name, memberType, searchTerm, statusFilter, pageNumber);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dashboard(LICDashboardViewModel form)
        {
            // ✅ Use plain "PhoneNumber" string — matches claim stored in Login
            var phone = User.FindFirst("PhoneNumber")?.Value?.Trim();
            var name = User.FindFirstValue(ClaimTypes.Name);
            var memberType = User.FindFirstValue("TypeofMember");

            if (string.IsNullOrWhiteSpace(phone))
                return RedirectToAction("Login");

            var vm = await BuildDashboardAsync(
                phone, name, memberType,
                form.SearchTerm ?? "",
                form.StatusFilter ?? "",
                form.PageNumber < 1 ? 1 : form.PageNumber
            );
            return View(vm);
        }

        private async Task<LICDashboardViewModel> BuildDashboardAsync(
            string phone, string name, string memberType,
            string searchTerm, string statusFilter, int pageNumber)
        {
            string role = memberType?.Trim() ?? "";

            // ── 1. Assigned college rows based on role ────────────────────────────
            IQueryable<LicInspectionCollegeDetail> baseQuery = _context.LicInspectionCollegeDetails
                .AsNoTracking();

            IQueryable<LicInspectionCollegeDetail> filtered = role switch
            {
                "Academic Council" => baseQuery.Where(x => x.AcMemberPhno.ToString() == phone),
                "Senate Members" => baseQuery.Where(x => x.SenetMemberPhNo.ToString() == phone),
                "Subject Expertise" => baseQuery.Where(x => x.SubjectExpertisePhNo.ToString() == phone),
                _ => Enumerable.Empty<LicInspectionCollegeDetail>().AsQueryable()
            };

            var collegeRows = await filtered.OrderBy(x => x.Collegename).ToListAsync();

            // ── 2. Faculty lookup — keyed by FacultyId ────────────────────────────
            var facultyMap = await _context.Faculties
                .AsNoTracking()
                .ToDictionaryAsync(f => f.FacultyId.ToString(), f => f);

            // ── 3. Claims for this phone ──────────────────────────────────────────
            var claimMap = await _context.LicclaimDetails
                .AsNoTracking()
                .Where(c => c.PhoneNumber.Trim() == phone)
                .ToDictionaryAsync(c => c.CollegeCode ?? "", c => c);

            // ── 4. Map to view items ──────────────────────────────────────────────
            var colleges = collegeRows.Select(c =>
            {
                facultyMap.TryGetValue(c.Facultycode.ToString() ?? "", out var fac);
                claimMap.TryGetValue(c.Collegecode ?? "", out var claim);

                // ── Fix: claim can be null — use null-conditional everywhere ──────
                string claimStatus = (claim != null && claim.TotalCost > 0)
                    ? "Completed"
                    : "Pending";

                return new InspectionCollegeItem2
                {
                    CollegeName = c.Collegename,
                    CollegeCode = c.Collegecode,
                    CollegePlace = c.CollegePlace,
                    ACMember = c.Acmember,
                    AcMemberPhone = c.AcMemberPhno?.ToString(),
                    SenetMember = c.SenetMember,
                    SenetMemberPhone = c.SenetMemberPhNo?.ToString(),
                    SubjectExpertise = c.SubjectExpertise,
                    SubjectExpertisePhone = c.SubjectExpertisePhNo?.ToString(),
                    FacultyName = fac?.FacultyName,
                    FacultyAbbr = fac?.FacultyAbbre,
                    ClaimStatus = claimStatus,
                    Totalcost = claim?.TotalCost?.ToString() ?? "0",  // ← Fix
                    ClaimMobile = claim?.PhoneNumber                    // ← already safe
                };
            }).ToList();

            // ── 5. Search filter ──────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string s = searchTerm.ToLower();
                colleges = colleges.Where(c =>
                    (c.CollegeName?.ToLower().Contains(s) ?? false) ||
                    (c.CollegePlace?.ToLower().Contains(s) ?? false) ||
                    (c.ACMember?.ToLower().Contains(s) ?? false) ||
                    (c.FacultyName?.ToLower().Contains(s) ?? false)
                ).ToList();
            }

            // ── 6. Status filter ──────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(statusFilter))
                colleges = colleges
                    .Where(c => c.ClaimStatus == statusFilter)
                    .ToList();

            // ── 7. Summary counts — before paging ────────────────────────────────
            int totalCompleted = colleges.Count(c => c.ClaimStatus == "Completed");
            int totalPending = colleges.Count(c => c.ClaimStatus == "Pending");

            // ── 8. Pagination ─────────────────────────────────────────────────────
            int pageSize = 10;
            int total = colleges.Count;
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (pageNumber > totalPages && totalPages > 0) pageNumber = totalPages;

            var paged = colleges
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new LICDashboardViewModel
            {
                Name = name,
                TypeofMember = memberType,
                MobileNumber = phone,
                Colleges = paged,
                TotalRecords = total,
                ClaimsCompleted = totalCompleted,
                ClaimsPending = totalPending,
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        // ── ViewModels ────────────────────────────────────────────────────────────────

        public class LICDashboardViewModel
        {
            public string? Name { get; set; }
            public string? TypeofMember { get; set; }
            public string? MobileNumber { get; set; }

            public List<InspectionCollegeItem2> Colleges { get; set; } = new();

            // FIX: no longer computed from Colleges (which is paged);
            // set explicitly from the full filtered list before paging
            public int TotalRecords { get; set; }
            public int ClaimsCompleted { get; set; }
            public int ClaimsPending { get; set; }

            public string? SearchTerm { get; set; }
            public string? StatusFilter { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalPages { get; set; }
        }

        public class InspectionCollegeItem2
        {
            public string? CollegeName { get; set; }
            public string? CollegeCode { get; set; }
            public string? CollegePlace { get; set; }
            public string? ACMember { get; set; }
            public string? AcMemberPhone { get; set; }
            public string? SenetMember { get; set; }
            public string? SenetMemberPhone { get; set; }
            public string? SubjectExpertise { get; set; }
            public string? SubjectExpertisePhone { get; set; }
            public string? FacultyName { get; set; }
            public string? FacultyAbbr { get; set; }
            public string? Totalcost { get; set; }
            public string? ClaimStatus { get; set; }
            public string? ClaimMobile { get; set; }
        }

    }

}


