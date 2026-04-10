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
            var sessionFacultyCode = SessionFacultyCode;  // ← fetch from session
            if (string.IsNullOrEmpty(sessionFacultyCode))
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = SessionUserName;
            ViewBag.SessionFacultyCode = sessionFacultyCode;  // ← pass to view if needed

            // Base college codes filtered by faculty from session
            var collegeCodesQuery = _context.LicInspectionCollegeDetails.AsQueryable();
            if (sessionFacultyCode != "300")
                collegeCodesQuery = collegeCodesQuery
                    .Where(c => c.Facultycode.ToString() == sessionFacultyCode);

            var allowedCollegeCodes = await collegeCodesQuery
                .Select(c => c.Collegecode)
                .ToListAsync();

            // Filter approvals by allowed college codes
            var approvalsQuery = _context.LiccollegeApprovals.AsQueryable();
            if (sessionFacultyCode != "300")
                approvalsQuery = approvalsQuery
                    .Where(x => allowedCollegeCodes.Contains(x.CollegeCode));

            var pendingCount = await approvalsQuery
                .CountAsync(x => x.DrApprovalStatus == "Pending"
                              || x.DrApprovalStatus == null
                              || x.DrApprovalStatus == "");

            var approvedCount = await approvalsQuery
                .CountAsync(x => x.DrApprovalStatus == "Approved");

            var rejectedCount = await approvalsQuery
                .CountAsync(x => x.DrApprovalStatus == "Rejected");

            var totalApprovals = await approvalsQuery.CountAsync();

            var approvalRate = totalApprovals > 0
                ? Math.Round((approvedCount * 100m) / totalApprovals, 1)
                : 0m;

            // Filter claims by allowed college codes
            var claimsQuery = _context.LicclaimDetails.AsQueryable();
            if (sessionFacultyCode != "300")
                claimsQuery = claimsQuery
                    .Where(x => allowedCollegeCodes.Contains(x.CollegeCode));

            var totalClaimAmount = await claimsQuery
                .SumAsync(x => (decimal?)x.TotalCost) ?? 0m;

            ViewBag.PendingCount = pendingCount;
            ViewBag.ApprovedCount = approvedCount;
            ViewBag.RejectedCount = rejectedCount;
            ViewBag.ApprovalRate = approvalRate;
            ViewBag.TotalClaimAmount = totalClaimAmount;

            // Filter recent claims by allowed college codes
            var recentClaimsQuery =
                from claim in claimsQuery
                join approval in approvalsQuery
                    on claim.CollegeCode equals approval.CollegeCode into apprGroup
                from appr in apprGroup.DefaultIfEmpty()
                join college in _context.LicInspectionCollegeDetails
                    on claim.CollegeCode equals college.Collegecode into collegeGroup
                from coll in collegeGroup.DefaultIfEmpty()
                orderby claim.CreatedDate descending
                select new RecentClaimViewModel
                {
                    MemberName = claim.MemberName,
                    CollegeCode = claim.CollegeCode,
                    CollegeName = coll != null ? coll.Collegename : claim.CollegeCode,
                    ModeOfTravel = claim.ModeOfTravel,
                    TotalCost = claim.TotalCost,
                    CreatedDate = claim.CreatedDate,
                    ApprovalStatus = appr != null ? appr.DrApprovalStatus ?? "Pending" : "Pending",
                    Remarks = appr != null ? appr.DrRemarks : null,
                    ApprovedBy = appr != null ? appr.DrapprovedBy : null,
                    ApprovedDate = appr != null ? appr.DrapprovedDate : null
                };

            var recentClaims = await recentClaimsQuery
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
        public async Task<IActionResult> CollegeClaimsDashboard(string? collegeCode)
        {
            var sessionFacultyCode = SessionFacultyCode;   // ← uses constant
            if (string.IsNullOrEmpty(sessionFacultyCode))
                return RedirectToAction("Login", "Account");

            var collegesQuery = _context.LicInspectionCollegeDetails.AsQueryable();
            if (sessionFacultyCode != "300")
                collegesQuery = collegesQuery
                    .Where(c => c.Facultycode.ToString() == sessionFacultyCode);

            if (!string.IsNullOrEmpty(collegeCode))
                collegesQuery = collegesQuery.Where(c => c.Collegecode == collegeCode);

            var colleges = await collegesQuery.ToListAsync();
            var result = new List<LICCollegeDashboardViewModel>();

            foreach (var college in colleges)
            {
                var senate = await BuildMemberSummary(college.SenetMember, college.Collegecode);
                var ac = await BuildMemberSummary(college.Acmember, college.Collegecode);
                var subject = await BuildMemberSummary(college.SubjectExpertise, college.Collegecode);

                var senateDate = senate?.InspectionDates?.FirstOrDefault();
                var acDate = ac?.InspectionDates?.FirstOrDefault();
                var subjectDate = subject?.InspectionDates?.FirstOrDefault();

                var validDates = new List<DateOnly?> { senateDate, acDate, subjectDate }
                    .Where(d => d.HasValue).Select(d => d.Value).ToList();

                if (validDates.Count > 0)
                {
                    var distinctDates = validDates.Distinct().ToList();
                    if (distinctDates.Count == 1)
                    {
                        if (senate != null) senate.DateMismatchFlag = !senateDate.HasValue;
                        if (ac != null) ac.DateMismatchFlag = !acDate.HasValue;
                        if (subject != null) subject.DateMismatchFlag = !subjectDate.HasValue;
                    }
                    else
                    {
                        var baseDate = distinctDates.First();
                        if (senate != null) senate.DateMismatchFlag = !senateDate.HasValue || senateDate != baseDate;
                        if (ac != null) ac.DateMismatchFlag = !acDate.HasValue || acDate != baseDate;
                        if (subject != null) subject.DateMismatchFlag = !subjectDate.HasValue || subjectDate != baseDate;
                    }
                }

                decimal totalCollegeClaim =
                    (senate?.TotalClaim ?? 0) +
                    (ac?.TotalClaim ?? 0) +
                    (subject?.TotalClaim ?? 0);

                var approval = await _context.LiccollegeApprovals
                    .FirstOrDefaultAsync(x => x.CollegeCode == college.Collegecode);

                result.Add(new LICCollegeDashboardViewModel
                {
                    CollegeName = college.Collegename,
                    CollegeCode = college.Collegecode,
                    FacultyCode = college.Facultycode.ToString(),  // ← saved from college record
                    SenateMember = senate,
                    ACMember = ac,
                    SubjectExpert = subject,
                    TotalCollegeClaim = totalCollegeClaim,
                    ApprovalStatus = approval?.DrApprovalStatus ?? "Pending",
                    Remarks = approval?.DrRemarks,
                    LicApprovalFileName = approval?.LicApprovalFileName
                });
            }

            var dropdownQuery = _context.LicInspectionCollegeDetails.AsQueryable();
            if (sessionFacultyCode != "300")
                dropdownQuery = dropdownQuery.Where(c => c.Facultycode.ToString() == sessionFacultyCode);

            ViewBag.Colleges = await dropdownQuery
                .Select(c => new SelectListItem { Value = c.Collegecode, Text = c.Collegename })
                .ToListAsync();

            ViewBag.SelectedCollege = collegeCode;
            ViewBag.SessionFacultyCode = sessionFacultyCode;  // ← available in View if needed

            return View(result);
        }

        private async Task<LICMemberSummary> BuildMemberSummary(string memberName, string collegeCode)
        {
            if (string.IsNullOrEmpty(memberName)) return null;

            var inspectionDates = await _context.LicinspectionDetails
                .Where(x => x.Name == memberName &&
                            x.SelectedCollegeCode == collegeCode &&
                            x.DateOfInspection.HasValue)
                .Select(x => x.DateOfInspection.Value)
                .ToListAsync();

            var claims = await _context.LicclaimDetails
                .Where(x => x.MemberName == memberName && x.CollegeCode == collegeCode)
                .ToListAsync();

            return new LICMemberSummary
            {
                MemberName = memberName,
                InspectionDates = inspectionDates,
                TotalClaim = claims.Sum(x => x.TotalCost ?? 0),
                DateMismatchFlag = false
            };
        }

        // ── ApproveCollege ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ApproveCollege(string collegeCode, IFormFile licFile, string remarks)
        {
            var sessionFacultyCode = SessionFacultyCode;  // ← fetch from session
            if (string.IsNullOrEmpty(sessionFacultyCode))
                return RedirectToAction("Login", "Account");

            if (licFile != null && licFile.Length > 0)
            {
                if (Path.GetExtension(licFile.FileName).ToLower() != ".pdf")
                    return BadRequest("Only PDF allowed.");
                using var reader = new BinaryReader(licFile.OpenReadStream());
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "%PDF")
                    return BadRequest("Invalid PDF file.");
            }

            var claims = await _context.LicclaimDetails
                .Where(x => x.CollegeCode == collegeCode)
                .ToListAsync();

            // ✅ Approver resolved from session constant, falls back to identity
            var approvedBy = SessionUserName ?? User.Identity?.Name;

            foreach (var claim in claims)
            {
                var approval = new LiccollegeApproval
                {
                    FacultyCode = Convert.ToInt32(sessionFacultyCode),
                    MemberName = claim.MemberName,
                    CollegeCode = claim.CollegeCode,
                    TypeOfMembers = claim.TypeofMember,
                    MobileNo = claim.PhoneNumber,
                    FromPlace = claim.FromPlace,
                    ToPlace = claim.ToPlace,
                    Kilometers = claim.Kilometers,
                    ReturnFromPlace = claim.ReturnFromPlace,
                    ReturnToPlace = claim.ReturnToPlace,
                    ReturnKilometers = claim.ReturnKilometers,
                    TravelCost = claim.TravelCost,
                    Dacost = claim.Dacost,
                    Lcacost = claim.Lcacost,
                    CollegeCost = claim.CollegeCost,
                    AirFair = claim.AirFareCost,
                    AirRoadCost = claim.AirRoadCost,
                    TotalClaimAmount = claim.TotalCost ?? 0,
                    IsBanglore = claim.IsBanglore,
                    NoOfDays = claim.NoofDays,
                    Division = claim.Division,
                    IsLca = claim.IsLca,
                    DrApprovalStatus = "Approved",
                    DrapprovedBy = approvedBy,
                    DrapprovedDate = DateTime.Now,
                    DrRemarks = remarks
                };
                _context.LiccollegeApprovals.Add(approval);
            }

            await _context.SaveChangesAsync();

            if (licFile != null && licFile.Length > 0)
            {
                var firstApproval = await _context.LiccollegeApprovals
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);
                if (firstApproval != null)
                {
                    using var ms = new MemoryStream();
                    await licFile.CopyToAsync(ms);
                    firstApproval.LicApprovalFile = ms.ToArray();
                    firstApproval.LicApprovalFileName = licFile.FileName;
                    firstApproval.LicApprovalUploadedOn = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

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
        public async Task<IActionResult> MemberReport(string memberName, string collegeCode)
        {
            var college = await _context.LicInspectionCollegeDetails
                .FirstOrDefaultAsync(c => c.Collegecode == collegeCode);

            var claims = await _context.LicclaimDetails
                .Where(c => c.MemberName == memberName && c.CollegeCode == collegeCode)
                .ToListAsync();

            var inspectionDates = await _context.LicinspectionDetails
                .Where(i => i.Name == memberName &&
                            i.SelectedCollegeCode == collegeCode &&
                            i.DateOfInspection.HasValue)
                .Select(i => i.DateOfInspection.Value)
                .ToListAsync();

            var total = claims.Sum(c => c.TotalCost ?? 0);
            var firstClaim = claims.FirstOrDefault();

            var approval = await _context.LiccollegeApprovals
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.MemberName == memberName &&
                    x.TypeOfMembers == firstClaim.TypeofMember);

            var model = new LICMemberReportViewModel
            {
                MemberName = memberName,
                CollegeName = college?.Collegename,
                CollegeCode = collegeCode,
                TypeOfMember = firstClaim?.TypeofMember,
                PhoneNumber = firstClaim?.PhoneNumber,
                InspectionDates = inspectionDates,
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
            if (licFile == null || licFile.Length == 0)
                return BadRequest("No file selected.");

            if (Path.GetExtension(licFile.FileName).ToLower() != ".pdf")
                return BadRequest("Only PDF allowed.");

            using (var reader = new BinaryReader(licFile.OpenReadStream()))
            {
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "%PDF")
                    return BadRequest("Invalid PDF file.");
            }

            // Read file bytes once
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await licFile.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Get ALL member rows for this college
            var approvals = await _context.LiccollegeApprovals
                .Where(x => x.CollegeCode == collegeCode)
                .ToListAsync();

            if (approvals.Count == 0)
            {
                // No rows exist yet — create a single placeholder row
                var newApproval = new LiccollegeApproval
                {
                    CollegeCode = collegeCode,
                    DrApprovalStatus = "Pending",
                    LicApprovalFile = fileBytes,
                    LicApprovalFileName = licFile.FileName,
                    LicApprovalUploadedOn = DateTime.Now
                };
                _context.LiccollegeApprovals.Add(newApproval);
            }
            else
            {
                // ✅ Save LIC file data to EVERY member row for this college
                foreach (var approval in approvals)
                {
                    approval.LicApprovalFile = fileBytes;
                    approval.LicApprovalFileName = licFile.FileName;
                    approval.LicApprovalUploadedOn = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { fileName = licFile.FileName, updatedRows = approvals.Count });
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
