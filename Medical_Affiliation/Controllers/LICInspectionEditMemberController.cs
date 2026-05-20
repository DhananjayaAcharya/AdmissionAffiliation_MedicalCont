using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Controllers
{
    public class LICInspectionEditMemberController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public LICInspectionEditMemberController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // HELPERS  (shared)
        // ─────────────────────────────────────────────────────────────────────

        private async Task BuildMemberListsAsync(LicInspectionViewModel vm)
        {
            var acMembers = await _context.LicInspections
                .Where(e => e.TypeofMember == "Academic Council")
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name
                }).ToListAsync();

            var senateMembers = await _context.LicInspections
                .Where(e => e.TypeofMember == "Senate Members")
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name
                }).ToListAsync();

            acMembers.Insert(0, new SelectListItem { Value = "", Text = "-- Select AC Member --" });
            senateMembers.Insert(0, new SelectListItem { Value = "", Text = "-- Select Senate Member --" });

            vm.ACMemberList = acMembers;
            vm.SenateMemberList = senateMembers;
        }

        private async Task<List<SelectListItem>> BuildCollegeListAsync()
        {
            return await _context.LicInspectionCollegeDetails
                .AsNoTracking()
                .OrderBy(c => c.Collegename)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Collegename} ({c.Collegecode})"
                })
                .ToListAsync();
        }

        private async Task PopulateCollegeDetailsAsync(LicInspectionViewModel vm, int collegeId)
        {
            var college = await _context.LicInspectionCollegeDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == collegeId);

            if (college == null) return;

            vm.Collegename = college.Collegename;
            vm.CollegePlace = college.CollegePlace;
            vm.Collegecode = college.Collegecode;
            vm.ACMember = college.Acmember;
            vm.AcMember_Phno = college.AcMemberPhno;
            vm.SenetMember = college.SenetMember;
            vm.SenetMember_PhNo = college.SenetMemberPhNo;
            vm.HasExistingFile = college.SeRevisedOrder != null && college.SeRevisedOrder.Length > 0;
            vm.SubjectExperts = ParseExperts(college.SubjectExpertise, college.SubjectExpertisePhNo.ToString());
        }

        private static List<SubjectExpertItem> ParseExperts(string? names, string? phones)
        {
            const char delimiter = '|';

            var nameList = (names ?? string.Empty).Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            var phoneList = (phones ?? string.Empty).Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<SubjectExpertItem>();
            for (int i = 0; i < nameList.Length; i++)
            {
                result.Add(new SubjectExpertItem
                {
                    Name = nameList[i].Trim(),
                    Phone = i < phoneList.Length ? phoneList[i].Trim() : "—"
                });
            }
            return result;
        }

        private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        // ─────────────────────────────────────────────────────────────────────
        // API ENDPOINTS
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetExpertPhone(int id)
        {
            var expert = await _context.LicInspections
                .Where(e => e.Id == id)
                .Select(e => new { phone = e.PhoneNumber })
                .FirstOrDefaultAsync();

            if (expert == null) return NotFound();
            return Json(expert);
        }

        [HttpGet]
        public IActionResult GetLICInspectionMembers(string type)
        {
            var members = _context.LicInspections
                .Where(x => x.TypeofMember == type)
                .Select(x => new {
                    id = x.Id,
                    name = x.Name,
                    phone = x.PhoneNumber
                })
                .Distinct()
                .OrderBy(x => x.name)
                .ToList();

            return Json(members);
        }

        [HttpGet]
        public async Task<IActionResult> GetCollegeDetails(int collegeId)
        {
            var college = await _context.LicInspectionCollegeDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == collegeId);

            if (college == null) return NotFound();

            var experts = ParseExperts(college.SubjectExpertise, college.SubjectExpertisePhNo.ToString());

            return Json(new
            {
                collegename = college.Collegename,
                collegePlace = college.CollegePlace,
                collegecode = college.Collegecode,
                acMember = college.Acmember,
                acPhone = college.AcMemberPhno,
                senateMember = college.SenetMember,
                senatePhone = college.SenetMemberPhNo,
                hasExistingFile = college.SeRevisedOrder != null && college.SeRevisedOrder.Length > 0,
                subjectExperts = experts
            });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadSERevisedOrder(int collegeId)
        {
            var college = await _context.LicInspectionCollegeDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == collegeId);

            if (college == null || college.SeRevisedOrder == null || college.SeRevisedOrder.Length == 0)
                return NotFound("No SE Revised Order file found for this college.");

            var fileName = $"SERevisedOrder_{college.Collegecode}.pdf";
            return File(college.SeRevisedOrder, "application/octet-stream", fileName);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SE EDIT  (GET)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> SEedit(int? collegeId)
        {
            var vm = new LicInspectionViewModel
            {
                CollegeList = await BuildCollegeListAsync(),
                SelectedCollegeId = collegeId
            };

            if (collegeId.HasValue)
                await PopulateCollegeDetailsAsync(vm, collegeId.Value);

            if (TempData["Success"] != null)
                vm.SuccessMessage = TempData["Success"]!.ToString();

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SE EDIT  (POST)
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SEedit([FromForm] LicInspectionViewModel vm)
        {
            // Capture submitted values FIRST, before anything overwrites them
            var submittedName = vm.SubjectExperts?.FirstOrDefault()?.Name?.Trim();
            var submittedPhone = vm.SubjectExperts?.FirstOrDefault()?.Phone?.Trim();

            // Capture selected member IDs from hidden fields
            var selectedAcMemberId = vm.SelectedAcMemberId;
            var selectedSenateMemberId = vm.SelectedSenateMemberId;

            vm.CollegeList = await BuildCollegeListAsync();

            if (!vm.SelectedCollegeId.HasValue)
            {
                ModelState.AddModelError(nameof(vm.SelectedCollegeId), "Please select a college.");
                return View(vm);
            }

            var college = await _context.LicInspectionCollegeDetails
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == vm.SelectedCollegeId.Value);

            if (college == null)
            {
                ModelState.AddModelError(string.Empty, "Selected college was not found.");
                return View(vm);
            }

            await PopulateCollegeDetailsAsync(vm, college.Id);

            // ── File validation ──────────────────────────────────────────────
            bool hasNewFile = vm.SE_RevisedOrderFile != null && vm.SE_RevisedOrderFile.Length > 0;
            bool hasExistingFile = college.SeRevisedOrder != null && college.SeRevisedOrder.Length > 0;

            if (!hasNewFile && !hasExistingFile)
            {
                ModelState.AddModelError(nameof(vm.SE_RevisedOrderFile),
                    "Please upload the SE Revised Order document.");
                return View(vm);
            }

            if (hasNewFile)
            {
                var ext = Path.GetExtension(vm.SE_RevisedOrderFile!.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError(nameof(vm.SE_RevisedOrderFile),
                        $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}");
                    return View(vm);
                }

                if (vm.SE_RevisedOrderFile.Length > MaxFileSizeBytes)
                {
                    ModelState.AddModelError(nameof(vm.SE_RevisedOrderFile),
                        "File size must not exceed 10 MB.");
                    return View(vm);
                }

                await using var memoryStream = new MemoryStream();
                await vm.SE_RevisedOrderFile.CopyToAsync(memoryStream);
                college.SeRevisedOrder = memoryStream.ToArray();
            }

            // ── Apply Subject Expert values ──────────────────────────────────
            college.SubjectExpertise = submittedName;
            college.SubjectExpertisePhNo = long.TryParse(submittedPhone, out var phNo) ? phNo : (long?)null;

            _context.Entry(college).Property(c => c.SubjectExpertise).IsModified = true;
            _context.Entry(college).Property(c => c.SubjectExpertisePhNo).IsModified = true;

            // ── Apply AC Member if a new one was selected ────────────────────
            if (selectedAcMemberId.HasValue)
            {
                var acMember = await _context.LicInspections
                    .FirstOrDefaultAsync(m => m.Id == selectedAcMemberId.Value
                                           && m.TypeofMember == "Academic Council");

                if (acMember != null)
                {
                    college.Acmember = acMember.Name;
                    college.AcMemberPhno = long.TryParse(acMember.PhoneNumber, out var acPhNo) ? acPhNo : (long?)null;

                    _context.Entry(college).Property(c => c.Acmember).IsModified = true;
                    _context.Entry(college).Property(c => c.AcMemberPhno).IsModified = true;
                }
            }

            // ── Apply Senate Member if a new one was selected ────────────────
            if (selectedSenateMemberId.HasValue)
            {
                var senateMember = await _context.LicInspections
                    .FirstOrDefaultAsync(m => m.Id == selectedSenateMemberId.Value
                                           && m.TypeofMember == "Senate Members");

                if (senateMember != null)
                {
                    college.SenetMember = senateMember.Name;
                    college.SenetMemberPhNo = long.TryParse(senateMember.PhoneNumber, out var senPhNo) ? senPhNo : (long?)null;

                    _context.Entry(college).Property(c => c.SenetMember).IsModified = true;
                    _context.Entry(college).Property(c => c.SenetMemberPhNo).IsModified = true;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Details updated successfully for {college.Collegename}.";
            return RedirectToAction(nameof(SEedit), new { collegeId = college.Id });
        }

        // ─────────────────────────────────────────────────────────────────────
        // LIC SE ADMIN  (subject expertise member form + list)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> LICSEADMIN()
        {
            var pageVM = new LicInspectionPageVM
            {
                Form = await BuildFormVM(),
                Records = await GetRecordsAsync()
            };
            return View(pageVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Form")] LicInspectionFormVM formVM)
        {
            ModelState.Remove("Form.TypeofMember");

            if (!ModelState.IsValid)
            {
                formVM.MemberTypeOptions = await GetMemberTypeOptionsAsync();
                var pageVM = new LicInspectionPageVM
                {
                    Form = formVM,
                    Records = await GetRecordsAsync()
                };
                return View("LICSEADMIN", pageVM);
            }

            var entity = new LicInspection
            {
                TypeofMember = "Subject Expertise",
                Name = formVM.Name.Trim(),
                Dob = formVM.DOB,
                PhoneNumber = formVM.PhoneNumber?.Trim(),
                Email = formVM.Email?.Trim(),
                Address = formVM.Address?.Trim(),
                Pannumber = formVM.PANNumber?.ToUpper().Trim(),
                AadhaarNumber = formVM.AadhaarNumber?.Trim(),
                AccountHolderName = formVM.AccountHolderName?.Trim(),
                AccountNumber = formVM.AccountNumber?.Trim(),
                Ifsccode = formVM.IFSCCode?.ToUpper().Trim(),
                BankName = formVM.BankName?.Trim(),
                BranchName = formVM.BranchName?.Trim()
            };

            _context.LicInspections.Add(entity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Record for '{entity.Name}' saved successfully!";
            return RedirectToAction(nameof(LICSEADMIN));
        }

        private async Task<List<SelectListItem>> GetMemberTypeOptionsAsync()
        {
            return await _context.MstLicInspectionMembers
                .Where(e => e.TypeofMemebers == "Subject Expertise")
                .OrderBy(m => m.TypeofMemebers)
                .Select(m => new SelectListItem
                {
                    Value = m.TypeofMemebers,
                    Text = m.TypeofMemebers
                })
                .ToListAsync();
        }

        private async Task<LicInspectionFormVM> BuildFormVM()
        {
            return new LicInspectionFormVM
            {
                MemberTypeOptions = await GetMemberTypeOptionsAsync()
            };
        }

        private async Task<List<LicInspectionListItemVM>> GetRecordsAsync()
        {
            var records = await _context.LicInspections
                .OrderByDescending(r => r.Id)
                .Take(1000)
                .Select(r => new LicInspectionListItemVM
                {
                    Id = r.Id,
                    TypeofMember = r.TypeofMember,
                    Name = r.Name,
                    DOB = r.Dob.ToString(),
                    PhoneNumber = r.PhoneNumber,
                    Email = r.Email,
                    PANNumber = r.Pannumber,
                    AadhaarNumber = r.AadhaarNumber,
                    AccountHolderName = r.AccountHolderName,
                    AccountNumber = r.AccountNumber,
                    IFSCCode = r.Ifsccode,
                    BankName = r.BankName,
                    BranchName = r.BranchName
                })
                .ToListAsync();

            // Format date in-memory — EF cannot translate custom format strings to SQL
            records.ForEach(r =>
            {
                if (DateTime.TryParse(r.DOB, out var dt))
                    r.DOB = dt.ToString("dd-MM-yyyy");
            });

            return records;
        }

        // ─────────────────────────────────────────────────────────────────────
        // REPORTS
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var members = await _context.LicInspections.AsNoTracking().ToListAsync();
            var collegeAssignments = await _context.LicInspectionCollegeDetails.AsNoTracking().ToListAsync();
            var claims = await _context.LicclaimDetails.AsNoTracking().ToListAsync();

            // Build faculty name lookup from master table
            var facultyLookup = await _context.Faculties
                .AsNoTracking()
                .ToDictionaryAsync(
                    f => f.FacultyId.ToString(),
                    f => f.FacultyName ?? f.FacultyId.ToString()
                );

            // Safely resolve faculty name from code
            string FacultyName(string? code) =>
                !string.IsNullOrEmpty(code) && facultyLookup.TryGetValue(code, out var name)
                    ? name
                    : (code ?? "Unknown");

            var model = new LICReportsViewModel
            {
                // ── Stats ──────────────────────────────────────────────────
                TotalMembers = members.Count,
                CompletedCount = members.Count(x => x.IsCompleted == true),
                PendingCount = members.Count(x => x.IsCompleted != true),
                TotalClaims = claims.Count,
                TotalColleges = collegeAssignments.Select(x => x.Collegecode).Distinct().Count(),

                // ── Table data ─────────────────────────────────────────────
                Members = members,
                CollegeAssignments = collegeAssignments,
                Claims = claims,

                // ── Filter dropdowns ───────────────────────────────────────
                AcademicYears = collegeAssignments
                    .Where(x => !string.IsNullOrEmpty(x.AcademicYear))
                    .Select(x => x.AcademicYear!)
                    .Distinct().OrderBy(x => x).ToList(),

                FacultyList = members
                    .Where(x => !string.IsNullOrEmpty(x.Facultycode))
                    .Select(x => x.Facultycode!)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(code => new LICReportsViewModel.FacultyItem
                    {
                        FacultyCode = code,
                        FacultyName = FacultyName(code)
                    })
                    .ToList(),

                Colleges = collegeAssignments
                    .Where(x => !string.IsNullOrEmpty(x.Collegecode))
                    .Select(x => new CollegeDropdownItem
                    {
                        CollegeCode = x.Collegecode!,
                        CollegeName = x.Collegename!
                    })
                    .DistinctBy(x => x.CollegeCode)
                    .OrderBy(x => x.CollegeName).ToList(),

                TravelModes = claims
                    .Where(x => !string.IsNullOrEmpty(x.ModeOfTravel))
                    .Select(x => x.ModeOfTravel!)
                    .Distinct().OrderBy(x => x).ToList(),

                // ── Charts ─────────────────────────────────────────────────
                MemberTypeCounts = members
                    .GroupBy(x => x.TypeofMember ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),

                TravelModeCounts = claims
                    .Where(x => !string.IsNullOrEmpty(x.ModeOfTravel))
                    .GroupBy(x => x.ModeOfTravel!)
                    .ToDictionary(g => g.Key, g => g.Count()),

                MonthlyInspectionCounts = claims
                    .Where(x => x.InspectionDate.HasValue)
                    .GroupBy(x => x.InspectionDate!.Value.ToString("MMM yyyy"))
                    .OrderBy(g => DateTime.ParseExact(g.Key, "MMM yyyy", null))
                    .ToDictionary(g => g.Key, g => g.Count()),

                // Faculty name as chart label key
                FacultyCollegeCounts = collegeAssignments
                    .Where(x => x.Facultycode.HasValue)
                    .GroupBy(x => x.Facultycode!.Value)
                    .ToDictionary(
                        g => FacultyName(g.Key.ToString()),
                        g => g.Select(x => x.Collegecode).Distinct().Count()
                    ),

                CostSummary = new CostSummaryData
                {
                    TravelCost = claims.Sum(x => x.TravelCost ?? 0),
                    DACost = claims.Sum(x => x.Dacost ?? 0),
                    LCACost = claims.Sum(x => x.Lcacost ?? 0),
                    AirFareCost = claims.Sum(x => x.AirFareCost ?? 0),
                    CollegeCost = claims.Sum(x => x.CollegeCost ?? 0),
                    TotalCost = claims.Sum(x => x.TotalCost ?? 0),
                }
            };

            return View(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // NESTED TYPES
        // ─────────────────────────────────────────────────────────────────────

        public class SubjectExpertItem
        {
            public string? Name { get; set; }
            public string? Phone { get; set; }
        }

        // ── LicInspectionViewModel ────────────────────────────────────────────
        public class LicInspectionViewModel
        {
            public List<SelectListItem> CollegeList { get; set; } = new();

            [Required(ErrorMessage = "Please select a college.")]
            [Display(Name = "College")]
            public int? SelectedCollegeId { get; set; }

            // Selected college info
            public string? Collegename { get; set; }
            public string? CollegePlace { get; set; }
            public string? Collegecode { get; set; }
            public string? ACMember { get; set; }
            public long? AcMember_Phno { get; set; }
            public string? SenetMember { get; set; }
            public long? SenetMember_PhNo { get; set; }
            public string? SeRevisedOrderFileName { get; set; }

            // Subject expertise
            public List<SubjectExpertItem> SubjectExperts { get; set; } = new();

            // Upload
            [Display(Name = "SE Revised Order Document")]
            public IFormFile? SE_RevisedOrderFile { get; set; }

            public bool HasExistingFile { get; set; }
            public string? ExistingFileName { get; set; }

            // Success message
            public string? SuccessMessage { get; set; }

            // AC Member
            public int? SelectedAcMemberId { get; set; }
            public int? SelectedACMemberId { get; set; }   // alias kept for view compatibility
            public string? ACMemberPhone { get; set; }

            // Senate Member
            public int? SelectedSenateMemberId { get; set; }
            public string? SenateMemberPhone { get; set; }

            // Dropdowns
            public List<SelectListItem> ACMemberList { get; set; } = new();
            public List<SelectListItem> SenateMemberList { get; set; } = new();
        }

        // ── LicInspectionFormVM ───────────────────────────────────────────────
        public class LicInspectionFormVM
        {
            [Required(ErrorMessage = "Please select a Member Type.")]
            [Display(Name = "Type of Member")]
            public int TypeofMember { get; set; }

            [Required(ErrorMessage = "Name is required.")]
            [StringLength(200)]
            [Display(Name = "Full Name")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Date of Birth is required.")]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            public DateOnly? DOB { get; set; }

            [Phone]
            [StringLength(15)]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [EmailAddress]
            [StringLength(150)]
            [Display(Name = "Email")]
            public string? Email { get; set; }

            [StringLength(500)]
            [Display(Name = "Address")]
            public string? Address { get; set; }

            [StringLength(10)]
            [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Invalid PAN format (e.g. ABCDE1234F).")]
            [Display(Name = "PAN Number")]
            public string? PANNumber { get; set; }

            [StringLength(12)]
            [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar must be 12 digits.")]
            [Display(Name = "Aadhaar Number")]
            public string? AadhaarNumber { get; set; }

            [StringLength(200)]
            [Display(Name = "Account Holder Name")]
            public string? AccountHolderName { get; set; }

            [StringLength(30)]
            [Display(Name = "Account Number")]
            public string? AccountNumber { get; set; }

            [StringLength(11)]
            [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Invalid IFSC format (e.g. SBIN0001234).")]
            [Display(Name = "IFSC Code")]
            public string? IFSCCode { get; set; }

            [StringLength(150)]
            [Display(Name = "Bank Name")]
            public string? BankName { get; set; }

            [StringLength(150)]
            [Display(Name = "Branch Name")]
            public string? BranchName { get; set; }

            public List<SelectListItem> MemberTypeOptions { get; set; } = new();
        }

        // ── LicInspectionListItemVM ───────────────────────────────────────────
        public class LicInspectionListItemVM
        {
            public int Id { get; set; }
            public string TypeofMember { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string DOB { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? PANNumber { get; set; }
            public string? AadhaarNumber { get; set; }
            public string? AccountHolderName { get; set; }
            public string? AccountNumber { get; set; }
            public string? IFSCCode { get; set; }
            public string? BankName { get; set; }
            public string? BranchName { get; set; }
        }

        // ── LicInspectionPageVM ───────────────────────────────────────────────
        public class LicInspectionPageVM
        {
            public LicInspectionFormVM Form { get; set; } = new();
            public List<LicInspectionListItemVM> Records { get; set; } = new();
        }

        // ── LICReportsViewModel ───────────────────────────────────────────────
        public class LICReportsViewModel
        {
            // Stats
            public int TotalMembers { get; set; }
            public int CompletedCount { get; set; }
            public int PendingCount { get; set; }
            public int TotalClaims { get; set; }
            public int TotalColleges { get; set; }

            // Filter options
            public List<string> AcademicYears { get; set; } = new();
            public List<FacultyItem> FacultyList { get; set; } = new();
            public List<CollegeDropdownItem> Colleges { get; set; } = new();
            public List<string> TravelModes { get; set; } = new();

            // Table data
            public List<LicInspection> Members { get; set; } = new();
            public List<LicInspectionCollegeDetail> CollegeAssignments { get; set; } = new();
            public List<LicclaimDetail> Claims { get; set; } = new();

            // Chart data
            public Dictionary<string, int> MemberTypeCounts { get; set; } = new();
            public Dictionary<string, int> TravelModeCounts { get; set; } = new();
            public Dictionary<string, int> MonthlyInspectionCounts { get; set; } = new();
            public Dictionary<string, int> FacultyCollegeCounts { get; set; } = new();
            public CostSummaryData CostSummary { get; set; } = new();

            public class FacultyItem
            {
                public string FacultyCode { get; set; } = string.Empty;
                public string FacultyName { get; set; } = string.Empty;
            }
        }

        // ── CostSummaryData ───────────────────────────────────────────────────
        public class CostSummaryData
        {
            public decimal TravelCost { get; set; }
            public decimal DACost { get; set; }
            public decimal LCACost { get; set; }
            public decimal AirFareCost { get; set; }
            public decimal CollegeCost { get; set; }
            public decimal TotalCost { get; set; }
        }
    }
}