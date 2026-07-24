using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Medical_Affiliation.Controllers
{
    [Authorize(AuthenticationSchemes = "LICSectionAuth")]
    public class LIC_DirectorController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ── Session key constants — change in one place, applies everywhere ──
        private const string SessionKeyUserName = "DirectorName";
        private const string SessionKeyFacultyCode = "FacultyCode";
        private const string SessionKeyUserCode = "DirectorCode";
        private const string SessionKeyLoginTime = "LoginTime";

        // ── Helper: read / write session cleanly ─────────────────────────────
        private string? SessionUserName => HttpContext.Session.GetString(SessionKeyUserName);
        private string? SessionFacultyCode => HttpContext.Session.GetString(SessionKeyFacultyCode);

        public LIC_DirectorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // ── Dashboard ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var sessionFacultyCode = SessionFacultyCode;
            if (string.IsNullOrEmpty(sessionFacultyCode))
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = SessionUserName;
            ViewBag.SessionFacultyCode = sessionFacultyCode;

            bool isAdmin = sessionFacultyCode == "300";

            // ── 1. Allowed college codes (only needed for non-admin) ──────────────
            List<string> allowedCollegeCodes = new();
            if (!isAdmin)
            {
                allowedCollegeCodes = await _context.LicInspectionCollegeDetails
                    .Where(c => c.Facultycode.ToString() == sessionFacultyCode)
                    .Select(c => c.Collegecode)
                    .ToListAsync();
            }

            // ── 2. Base queries ───────────────────────────────────────────────────
            IQueryable<LiccollegeApproval> approvalsQuery = _context.LiccollegeApprovals;
            IQueryable<LicclaimDetail> claimsQuery = _context.LicclaimDetails;

            if (!isAdmin)
            {
                approvalsQuery = approvalsQuery
                    .Where(x => allowedCollegeCodes.Contains(x.CollegeCode));
                claimsQuery = claimsQuery
                    .Where(x => allowedCollegeCodes.Contains(x.CollegeCode));
            }

            // ── 3. Approval counts ────────────────────────────────────────────────
            var pendingCount = await approvalsQuery
                .CountAsync(x => x.DrApprovalStatus == "Pending"
                              || x.DrApprovalStatus == null
                              || x.DrApprovalStatus == "");

            var approvedCount = await approvalsQuery.CountAsync(x => x.DrApprovalStatus == "Approved");
            var rejectedCount = await approvalsQuery.CountAsync(x => x.DrApprovalStatus == "Rejected");
            var totalApprovals = await approvalsQuery.CountAsync();

            var approvalRate = totalApprovals > 0
                ? Math.Round((approvedCount * 100m) / totalApprovals, 1)
                : 0m;

            var totalClaimAmount = await claimsQuery.SumAsync(x => (decimal?)x.TotalCost) ?? 0m;

            ViewBag.PendingCount = pendingCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.RejectedCount = rejectedCount;
            ViewBag.ApprovalRate = approvalRate;
            ViewBag.TotalClaimAmount = totalClaimAmount;

            // ── 4. Recent claims — use LEFT JOIN but de-duplicate with GroupJoin ──
            //
            //  ROOT CAUSE FIX: A claim with multiple approval rows produced multiple
            //  result rows. Use GroupJoin → FirstOrDefault to keep exactly one row
            //  per claim, taking the latest/most relevant approval.
            //
            var recentClaims = await (
                from claim in claimsQuery

                    // Left-join colleges
                join coll in _context.LicInspectionCollegeDetails
                    on claim.CollegeCode equals coll.Collegecode into collGroup
                from college in collGroup.DefaultIfEmpty()

                    // Left-join approvals — take only ONE approval per claim
                    // (picks the most recently approved/updated record)
                let approval = _context.LiccollegeApprovals
                    .Where(a => a.CollegeCode == claim.CollegeCode)
                    // If !isAdmin, also scope approvals to the same faculty
                    // (already handled above via approvalsQuery, so just filter CollegeCode)
                    .OrderByDescending(a => a.DrapprovedDate)
                    .FirstOrDefault()

                orderby claim.CreatedDate descending

                select new RecentClaimViewModel
                {
                    MemberName = claim.MemberName,
                    CollegeCode = claim.CollegeCode,
                    CollegeName = college != null ? college.Collegename : claim.CollegeCode,
                    ModeOfTravel = claim.ModeOfTravel,
                    TotalCost = claim.TotalCost,
                    CreatedDate = claim.CreatedDate,
                    ApprovalStatus = approval != null ? (approval.DrApprovalStatus ?? "Pending") : "Pending",
                    Remarks = approval != null ? approval.DrRemarks : null,
                    ApprovedBy = approval != null ? approval.DrapprovedBy : null,
                    ApprovedDate = approval != null ? approval.DrapprovedDate : null
                }
            )
            .Take(5)
            .ToListAsync();

            return View(recentClaims);
        }

        // ── Logout ────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("LICSectionAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("AdminLogin", "Admin");
        }

        // ── CollegeClaimsDashboard ────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CheckMemberReportExists(string phoneNumber, string collegeCode)
        {
            var phone = NormalizePhone(phoneNumber);
            collegeCode = collegeCode?.Trim();

            var claimPhones = await _context.LicclaimDetails
                .Where(x => x.CollegeCode.Trim() == collegeCode)
                .Select(x => x.PhoneNumber)
                .ToListAsync();

            var inspectionPhones = await _context.LicinspectionDetails
                .Where(x => x.SelectedCollegeCode.Trim() == collegeCode)
                .Select(x => x.PhoneNumber)
                .ToListAsync();

            bool exists = claimPhones.Any(p => NormalizePhone(p) == phone)
                       || inspectionPhones.Any(p => NormalizePhone(p) == phone);

            return Json(new { success = exists });
        }
        private static string NormalizePhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            var digitsOnly = new string(raw.Where(char.IsDigit).ToArray());
            return digitsOnly.Length > 10 ? digitsOnly.Substring(digitsOnly.Length - 10) : digitsOnly;
        }

        [HttpGet]
        public async Task<IActionResult> CollegeClaimsDashboard(string? collegeCode)
        {
            var sessionFacultyCode = SessionFacultyCode;
            if (string.IsNullOrEmpty(sessionFacultyCode))
                return RedirectToAction("Login", "Account");

            var collegesQuery = _context.LicInspectionCollegeDetails.AsNoTracking().AsQueryable();
            if (sessionFacultyCode != "300")
                collegesQuery = collegesQuery.Where(c => c.Facultycode.ToString() == sessionFacultyCode);

            var allCollegesInScope = await collegesQuery.ToListAsync();

            ViewBag.Colleges = allCollegesInScope
                .Select(c => new SelectListItem { Value = c.Collegecode, Text = c.Collegename })
                .ToList();
            ViewBag.SelectedCollege = collegeCode;
            ViewBag.SessionFacultyCode = sessionFacultyCode;

            var colleges = string.IsNullOrEmpty(collegeCode)
                ? allCollegesInScope
                : allCollegesInScope.Where(c => c.Collegecode == collegeCode).ToList();

            if (colleges.Count == 0)
                return View(new List<LICCollegeDashboardViewModel>());

            var collegeCodes = colleges.Select(c => c.Collegecode).Distinct().ToList();

            var allClaims = await _context.LicclaimDetails
                .AsNoTracking()
                .Where(x => collegeCodes.Contains(x.CollegeCode))
                .ToListAsync();

            var allInspections = await _context.LicinspectionDetails
                .AsNoTracking()
                .Where(x => collegeCodes.Contains(x.SelectedCollegeCode) && x.DateOfInspection.HasValue)
                .ToListAsync();

            var allApprovals = await _context.LiccollegeApprovals
                .AsNoTracking()
                .Where(x => collegeCodes.Contains(x.CollegeCode))
                .ToListAsync();

            // Keyed by (Phone, CollegeCode) — NOT name
            Dictionary<(string Phone, string Code), List<LicclaimDetail>> claimsLookup =
                allClaims
                    .Where(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                    .GroupBy(x => (Phone: NormalizePhone(x.PhoneNumber), Code: x.CollegeCode?.Trim()))
                    .ToDictionary(g => g.Key, g => g.ToList());

            Dictionary<(string Phone, string Code), List<DateOnly>> inspectionLookup =
                allInspections
                    .Where(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                    .GroupBy(x => (Phone: NormalizePhone(x.PhoneNumber), Code: x.SelectedCollegeCode?.Trim()))
                    .ToDictionary(g => g.Key, g => g.Select(x => x.DateOfInspection.Value).ToList());

            var approvalLookup = allApprovals
                .GroupBy(x => x.CollegeCode)
                .ToDictionary(g => g.Key, g => g.First());

            LICMemberSummary BuildSummary(string memberName, long? memberPhone, string collegeCodeForMember)
            {
                if (string.IsNullOrEmpty(memberName) || memberPhone == null || memberPhone == 0) return null;

                var phone = NormalizePhone(memberPhone.Value.ToString());
                var code = collegeCodeForMember?.Trim();
                var key = (Phone: phone, Code: code);

                claimsLookup.TryGetValue(key, out var claims);
                inspectionLookup.TryGetValue(key, out var datesFromInspectionTable);

                var datesFromClaims = claims?
                    .Where(c => c.InspectionDate.HasValue)
                    .Select(c => c.InspectionDate.Value)
                    .ToList() ?? new List<DateOnly>();

                var combinedDates = (datesFromInspectionTable ?? new List<DateOnly>())
                    .Union(datesFromClaims)
                    .Distinct()
                    .ToList();

                return new LICMemberSummary
                {
                    MemberName = memberName.Trim(),
                    PhoneNumber = phone,
                    InspectionDates = combinedDates,
                    TotalClaim = claims?.Sum(x => x.TotalCost ?? 0) ?? 0,
                    DateMismatchFlag = false
                };
            }

            var result = new List<LICCollegeDashboardViewModel>();

            foreach (var college in colleges)
            {
                var senate = BuildSummary(college.SenetMember, college.SenetMemberPhNo, college.Collegecode);
                var ac = BuildSummary(college.Acmember, college.AcMemberPhno, college.Collegecode);
                var subject = BuildSummary(college.SubjectExpertise, college.SubjectExpertisePhNo, college.Collegecode);

                var senateDate = senate?.InspectionDates?.FirstOrDefault();
                var acDate = ac?.InspectionDates?.FirstOrDefault();
                var subjectDate = subject?.InspectionDates?.FirstOrDefault();

                var validDates = new List<DateOnly?> { senateDate, acDate, subjectDate }
                    .Where(d => d.HasValue).Select(d => d.Value).ToList();

                if (validDates.Count > 0)
                {
                    var distinctDates = validDates.Distinct().ToList();
                    var baseDate = distinctDates.First();
                    bool singleDate = distinctDates.Count == 1;

                    if (senate != null) senate.DateMismatchFlag = singleDate ? !senateDate.HasValue : (!senateDate.HasValue || senateDate != baseDate);
                    if (ac != null) ac.DateMismatchFlag = singleDate ? !acDate.HasValue : (!acDate.HasValue || acDate != baseDate);
                    if (subject != null) subject.DateMismatchFlag = singleDate ? !subjectDate.HasValue : (!subjectDate.HasValue || subjectDate != baseDate);
                }

                decimal totalCollegeClaim =
                    (senate?.TotalClaim ?? 0) + (ac?.TotalClaim ?? 0) + (subject?.TotalClaim ?? 0);

                approvalLookup.TryGetValue(college.Collegecode, out var approval);

                result.Add(new LICCollegeDashboardViewModel
                {
                    CollegeName = college.Collegename,
                    CollegeCode = college.Collegecode,
                    FacultyCode = college.Facultycode.ToString(),
                    SenateMember = senate,
                    ACMember = ac,
                    SubjectExpert = subject,
                    TotalCollegeClaim = totalCollegeClaim,
                    ApprovalStatus = approval?.DrApprovalStatus ?? "Pending",
                    Remarks = approval?.DrRemarks,
                    LicApprovalFileName = approval?.LicApprovalFileName
                });
            }

            return View(result);
        }

        private async Task<LICMemberSummary> BuildMemberSummary(string memberName, string collegeCode)
        {
            if (string.IsNullOrEmpty(memberName)) return null;

            // Trim both sides to handle any DB whitespace issues
            memberName = memberName.Trim();
            collegeCode = collegeCode.Trim();

            var inspectionDates = await _context.LicinspectionDetails
                .Where(x => x.Name.Trim() == memberName &&
                            x.SelectedCollegeCode.Trim() == collegeCode &&
                            x.DateOfInspection.HasValue)
                .Select(x => x.DateOfInspection.Value)
                .ToListAsync();

            var claims = await _context.LicclaimDetails
                .Where(x => x.MemberName.Trim() == memberName &&
                            x.CollegeCode.Trim() == collegeCode)
                .ToListAsync();

            return new LICMemberSummary
            {
                MemberName = memberName,
                InspectionDates = inspectionDates,
                TotalClaim = claims.Sum(x => x.TotalCost ?? 0),
                DateMismatchFlag = false,

            };
        }

        // ── ApproveCollege ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ApproveCollege(string collegeCode, string remarks)
        {
            var sessionFacultyCode = SessionFacultyCode;
            if (string.IsNullOrEmpty(sessionFacultyCode))
                return RedirectToAction("Login", "Account");

            var approvedBy = SessionUserName ?? User.Identity?.Name;

            var college = await _context.LicInspectionCollegeDetails
                .FirstOrDefaultAsync(x => x.Collegecode == collegeCode);

            if (college == null)
                return RedirectToAction("CollegeClaimsDashboard");

            var memberRoles = new[]
            {
        new { Name = college.SenetMember,      Role = "Senate Member"  },
        new { Name = college.Acmember,         Role = "AC Member"      },
        new { Name = college.SubjectExpertise, Role = "Subject Expert" }
    };

            // ── Fetch ALL claims for this college into memory once ──────────────
            var allCollegeClaims = await _context.LicclaimDetails
                .Where(x => x.CollegeCode == collegeCode)
                .ToListAsync();

            // ── DEBUG: check Output window in Visual Studio ──────────────────────
            foreach (var c in allCollegeClaims)
                System.Diagnostics.Debug.WriteLine(
                    $"CLAIM ROW → MemberName: '{c.MemberName}' | College: '{c.CollegeCode}'");

            System.Diagnostics.Debug.WriteLine(
                $"COLLEGE → Senate: '{college.SenetMember}' | AC: '{college.Acmember}' | Subject: '{college.SubjectExpertise}'");
            // ────────────────────────────────────────────────────────────────────

            foreach (var memberRole in memberRoles)
            {
                if (string.IsNullOrWhiteSpace(memberRole.Name))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"⏭ Skipping role '{memberRole.Role}' — Name field is null/blank on college record.");
                    continue;
                }

                var memberName = memberRole.Name.Trim();

                // Match in memory, case-insensitive + trimmed, so casing differences don't break the lookup
                var claim = allCollegeClaims.FirstOrDefault(x =>
                    string.Equals(x.MemberName?.Trim(), memberName, StringComparison.OrdinalIgnoreCase));

                if (claim == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"⚠ No claim found for member: '{memberName}' (Role: {memberRole.Role})");
                    foreach (var c in allCollegeClaims)
                        System.Diagnostics.Debug.WriteLine(
                            $"   DB has: '{c.MemberName}'");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"✔ Claim found for member: '{memberName}' (Role: {memberRole.Role})");
                }

                var existing = await _context.LiccollegeApprovals
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.MemberName == memberName);

                if (existing != null)
                {
                    existing.DrApprovalStatus = "Approved";
                    existing.DrapprovedBy = approvedBy;
                    existing.DrapprovedDate = DateTime.Now;
                    existing.DrRemarks = remarks;
                    existing.UpdatedDate = DateTime.Now;
                    existing.AcademicYear = "2026-27";

                    if (claim != null)
                    {
                        existing.TypeOfMembers = memberRole.Role;
                        existing.MobileNo = claim.PhoneNumber;
                        existing.FromPlace = claim.FromPlace;
                        existing.FacultyCode = Convert.ToInt32(sessionFacultyCode);
                        existing.ToPlace = claim.ToPlace;
                        existing.Kilometers = claim.Kilometers;
                        existing.ReturnFromPlace = claim.ReturnFromPlace;
                        existing.ReturnToPlace = claim.ReturnToPlace;
                        existing.ReturnKilometers = claim.ReturnKilometers;
                        existing.TravelCost = claim.TravelCost;
                        existing.Dacost = claim.Dacost;
                        existing.Lcacost = claim.Lcacost;
                        existing.CollegeCost = claim.CollegeCost;
                        existing.AirFair = claim.AirFareCost;
                        existing.AirRoadCost = claim.AirRoadCost;
                        existing.TotalClaimAmount = claim.TotalCost ?? 0;
                        existing.IsBanglore = claim.IsBanglore;
                        existing.NoOfDays = claim.NoofDays;
                        existing.Division = claim.Division;
                        existing.IsLca = claim.IsLca;
                    }
                }
                else
                {
                    var approval = new LiccollegeApproval
                    {
                        FacultyCode = Convert.ToInt32(sessionFacultyCode),
                        MemberName = memberName,
                        CollegeCode = collegeCode,
                        TypeOfMembers = memberRole.Role,
                        DrApprovalStatus = "Approved",
                        DrapprovedBy = approvedBy,
                        DrapprovedDate = DateTime.Now,
                        DrRemarks = remarks,
                        AcademicYear = "2026-27",
                        TotalClaimAmount = claim?.TotalCost ?? 0,

                        MobileNo = claim?.PhoneNumber,
                        FromPlace = claim?.FromPlace,
                        ToPlace = claim?.ToPlace,
                        Kilometers = claim?.Kilometers,
                        ReturnFromPlace = claim?.ReturnFromPlace,
                        ReturnToPlace = claim?.ReturnToPlace,
                        ReturnKilometers = claim?.ReturnKilometers,
                        TravelCost = claim?.TravelCost,
                        Dacost = claim?.Dacost,
                        Lcacost = claim?.Lcacost,
                        CollegeCost = claim?.CollegeCost,
                        AirFair = claim?.AirFareCost,
                        AirRoadCost = claim?.AirRoadCost,
                        IsBanglore = claim?.IsBanglore,
                        NoOfDays = claim?.NoofDays,
                        Division = claim?.Division,
                        IsLca = claim?.IsLca,
                    };
                    _context.LiccollegeApprovals.Add(approval);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("CollegeClaimsDashboard");
        }

        // ── RejectCollege ─────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RejectCollege(string collegeCode)
        {
            var approval = await _context.LiccollegeApprovals
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

            var total = await _context.LicclaimDetails
                .Where(x => x.CollegeCode == collegeCode)
                .SumAsync(x => (decimal?)x.TotalCost) ?? 0;

            var rejectedBy = SessionUserName ?? User.Identity?.Name;

            if (approval == null)
            {
                approval = new LiccollegeApproval
                {
                    CollegeCode = collegeCode,
                    TotalClaimAmount = total,
                    DrApprovalStatus = "Rejected",
                    DrapprovedBy = rejectedBy,
                    DrapprovedDate = DateTime.Now
                };
                _context.LiccollegeApprovals.Add(approval);
            }
            else
            {
                approval.TotalClaimAmount = total;
                approval.DrApprovalStatus = "Rejected";
                approval.DrapprovedBy = rejectedBy;
                approval.DrapprovedDate = DateTime.Now;
                approval.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("CollegeClaimsDashboard");
        }

        // ── MemberReport ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> MemberReport(string phoneNumber, string collegeCode)
        {
            var phone = NormalizePhone(phoneNumber);
            collegeCode = collegeCode?.Trim();

            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(collegeCode))
                return BadRequest("Phone number and college code are required.");

            var college = await _context.LicInspectionCollegeDetails
                .FirstOrDefaultAsync(c => c.Collegecode.Trim() == collegeCode);

            var claims = (await _context.LicclaimDetails
                .Where(c => c.CollegeCode.Trim() == collegeCode)
                .ToListAsync())
                .Where(c => NormalizePhone(c.PhoneNumber) == phone)
                .ToList();

            var inspectionDates = (await _context.LicinspectionDetails
                .Where(i => i.SelectedCollegeCode.Trim() == collegeCode && i.DateOfInspection.HasValue)
                .ToListAsync())
                .Where(i => NormalizePhone(i.PhoneNumber) == phone)
                .Select(i => i.DateOfInspection.Value)
                .ToList();

            // Fallback: pull inspection date from claims table too, same as dashboard logic
            var datesFromClaims = claims
                .Where(c => c.InspectionDate.HasValue)
                .Select(c => c.InspectionDate.Value)
                .ToList();

            var combinedDates = inspectionDates.Union(datesFromClaims).Distinct().ToList();

            var total = claims.Sum(c => c.TotalCost ?? 0);
            var firstClaim = claims.FirstOrDefault();

            var memberName = firstClaim?.MemberName;
            if (string.IsNullOrEmpty(memberName))
            {
                var inspectionRow = (await _context.LicinspectionDetails
                    .Where(i => i.SelectedCollegeCode.Trim() == collegeCode)
                    .ToListAsync())
                    .FirstOrDefault(i => NormalizePhone(i.PhoneNumber) == phone);
                memberName = inspectionRow?.Name;
            }

            LiccollegeApproval approval = null;
            if (!string.IsNullOrEmpty(memberName))
            {
                var approvals = await _context.LiccollegeApprovals
                    .Where(x => x.CollegeCode.Trim() == collegeCode)
                    .ToListAsync();

                // Prefer phone match once MemberPhone column exists; fall back to name for now
                approval = approvals.FirstOrDefault(x => x.MobileNo != null && NormalizePhone(x.MobileNo) == phone)
                         ?? approvals.FirstOrDefault(x => x.MemberName?.Trim() == memberName.Trim());
            }

            var model = new LICMemberReportViewModel
            {
                MemberName = memberName,
                CollegeName = college?.Collegename,
                CollegeCode = collegeCode,
                TypeOfMember = firstClaim?.TypeofMember,
                PhoneNumber = phone,
                InspectionDates = combinedDates,
                Claims = claims,
                TravelCost = firstClaim?.TravelCost ?? 0,
                DACost = firstClaim?.Dacost ?? 0,
                LCACost = firstClaim?.Lcacost ?? 0,
                CollegeCost = firstClaim?.CollegeCost ?? 0,
                AirFareCost = firstClaim?.AirFareCost ?? 0,
                AirRoadCost = firstClaim?.AirRoadCost ?? 0,
                TotalClaimAmount = total,
                LicApprovalFileName = approval?.LicApprovalFileName
            };

            return View(model);
        }

        // ── UploadLicApprovalOnly ─────────────────────────────────────────────
        // ── UploadLicApprovalOnly ─────────────────────────────────────────────
        [HttpPost]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadLicApprovalOnly(string collegeCode, IFormFile licFile)
        {
            if (string.IsNullOrWhiteSpace(collegeCode))
                return BadRequest("College code is required.");

            if (licFile == null || licFile.Length == 0)
                return BadRequest("No file selected.");

            if (Path.GetExtension(licFile.FileName).ToLower() != ".pdf")
                return BadRequest("Only PDF files are allowed.");

            // Validate PDF header
            using (var stream = licFile.OpenReadStream())
            {
                byte[] header = new byte[4];
                await stream.ReadAsync(header, 0, 4);
                if (Encoding.ASCII.GetString(header) != "%PDF")
                    return BadRequest("Invalid PDF file.");
            }

            // Read file bytes once
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await licFile.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Get the college to know the 3 team members
            var college = await _context.LicInspectionCollegeDetails
                .FirstOrDefaultAsync(x => x.Collegecode == collegeCode);

            if (college == null)
                return BadRequest("College not found.");

            var memberNames = new[]
            {
        college.SenetMember?.Trim(),
        college.Acmember?.Trim(),
        college.SubjectExpertise?.Trim()
    }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

            if (!memberNames.Any())
                return BadRequest("No team members found for this college.");

            // Get existing approval rows for these 3 members
            var existingApprovals = await _context.LiccollegeApprovals
                .Where(x => x.CollegeCode == collegeCode &&
                            memberNames.Contains(x.MemberName.Trim()))
                .ToListAsync();

            foreach (var memberName in memberNames)
            {
                var approval = existingApprovals
                    .FirstOrDefault(x => x.MemberName.Trim() == memberName);

                if (approval != null)
                {
                    // Update file on existing row
                    approval.LicApprovalFile = fileBytes;
                    approval.LicApprovalFileName = licFile.FileName;
                    approval.LicApprovalUploadedOn = DateTime.Now;
                }
                else
                {
                    // Create a placeholder row with just the file
                    // (will be fully populated on approval)
                    _context.LiccollegeApprovals.Add(new LiccollegeApproval
                    {
                        CollegeCode = collegeCode,
                        MemberName = memberName,
                        DrApprovalStatus = "Pending",
                        LicApprovalFile = fileBytes,
                        LicApprovalFileName = licFile.FileName,
                        LicApprovalUploadedOn = DateTime.Now

                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                fileName = licFile.FileName,
                totalMembers = memberNames.Count,
                message = $"PDF saved for {memberNames.Count} team members."
            });
        }

        // ── GetLicApprovalFile ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetLicApprovalFile(string collegeCode)
        {
            var approval = await _context.LiccollegeApprovals
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

            if (approval?.LicApprovalFile == null)
                return NotFound();

            return Json(new { fileData = Convert.ToBase64String(approval.LicApprovalFile) });
        }

        // ── UploadMemberLicApproval ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadMemberLicApproval(
            string memberName, string collegeCode, string typeOfMember, IFormFile licFile)
        {
            if (licFile == null || licFile.Length == 0)
            {
                TempData["Error"] = "Please select a PDF file to upload.";
                return RedirectToAction("MemberReport", new { memberName, collegeCode });
            }

            if (Path.GetExtension(licFile.FileName).ToLower() != ".pdf")
            {
                TempData["Error"] = "Only PDF files are allowed.";
                return RedirectToAction("MemberReport", new { memberName, collegeCode });
            }

            using (var reader = new BinaryReader(licFile.OpenReadStream()))
            {
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "%PDF")
                {
                    TempData["Error"] = "Invalid PDF file.";
                    return RedirectToAction("MemberReport", new { memberName, collegeCode });
                }
            }

            var approval = await _context.LiccollegeApprovals
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.MemberName == memberName &&
                    x.TypeOfMembers == typeOfMember);

            if (approval == null)
            {
                TempData["Error"] = "No approval record found. Approve the college first.";
                return RedirectToAction("MemberReport", new { memberName, collegeCode });
            }

            using (var ms = new MemoryStream())
            {
                await licFile.CopyToAsync(ms);
                approval.LicApprovalFile = ms.ToArray();
            }

            approval.LicApprovalFileName = licFile.FileName;
            approval.LicApprovalUploadedOn = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "LIC Approval document uploaded successfully.";
            return RedirectToAction("MemberReport", new { memberName, collegeCode });
        }

        // ── ViewMemberLicApproval ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ViewMemberLicApproval(
            string memberName, string collegeCode, string typeOfMember)
        {
            var approval = await _context.LiccollegeApprovals
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.MemberName == memberName &&
                    x.TypeOfMembers == typeOfMember);

            if (approval?.LicApprovalFile == null || approval.LicApprovalFile.Length == 0)
                return NotFound("No approval document found for this member.");

            return File(approval.LicApprovalFile, "application/pdf");
        }
    }
}
