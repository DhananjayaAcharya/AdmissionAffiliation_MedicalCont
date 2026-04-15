using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Medical_Affiliation.Controllers
{
    public class ContinuesAffiliation_FacultybasedController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly SessionUserContext _userContext;

        public ContinuesAffiliation_FacultybasedController(ApplicationDbContext context)
        {
            _context = context;

        }
        //private (string? FacultyCode, string? CollegeCode) GetSessionCodes()
        //{
        //    var facultyCode = User.FindFirst("FacultyCode")?.Value;
        //    var collegeCode = User.FindFirst("CollegeCode")?.Value;
        //    return (facultyCode, collegeCode);
        //}

        private IActionResult SessionError()
        {
            TempData["Error"] = "Session expired. Please login again.";
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Index()
        {
            var model = new SidebarViewModel
            {
                TypeOfAffiliationList = _context.TypeOfAffiliations
                    .Select(t => new SelectListItem
                    {
                        Value = t.TypeId.ToString(),
                        Text = t.TypeDescription
                    })
                    .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> ViewGOKOrder()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            var entity = await _context.AffiliationCourseDetails
                .FirstOrDefaultAsync(x =>
                    x.Facultycode == facultyCode &&
                    x.Collegecode == collegeCode &&
                    x.CourseId == "MBBS");

            if (entity?.Gokorder == null)
                return NotFound("File not found");

            return File(entity.Gokorder, "application/pdf");
        }

        // ✅ VIEW LAST AFFILIATION PDF
        public async Task<IActionResult> ViewLastAffiliation()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            var entity = await _context.AffiliationCourseDetails
                .FirstOrDefaultAsync(x =>
                    x.Facultycode == facultyCode &&
                    x.Collegecode == collegeCode &&
                    x.CourseId == "MBBS");

            if (entity?.LastAffiliationRguhsfile == null)
                return NotFound("File not found");

            return File(entity.LastAffiliationRguhsfile, "application/pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OnAffiliationTypeChanged(SidebarViewModel model)
        {
            // Adjust the value check to match your actual ID/text
            if (model.SelectedAffiliationId == 2) // Continuation of Affiliation
            {
                return RedirectToAction(
                    actionName: "Institution_Details",
                    controllerName: "ContinuesAffiliation_Facultybased");
            }

            // default: stay on same page or redirect somewhere else
            return RedirectToAction("Dashboard", "Collegelogin");
        }
        //csharp Medical_Affiliation\Controllers\MedicalContinuesAffiliationController.cs
        // GET: /Institution/Create


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Aff_InstituteDetails()
        {
            //var (facultyCode, collegeCode) = GetSessionCodes();

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            var vm = new MedicalVm
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode
            };

            var entity = await _context.InstitutionBasicDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode.Trim() == collegeCode.Trim() &&
                    x.FacultyCode.Trim() == facultyCode.Trim());

            if (entity != null)
            {
                vm.InstitutionId = entity.InstitutionId;
                vm.NameOfInstitution = entity.NameOfInstitution ?? "";
                vm.AddressOfInstitution = entity.AddressOfInstitution ?? "";
                vm.VillageTownCity = entity.VillageTownCity ?? "";
                vm.Taluk = entity.Taluk ?? "";
                vm.District = entity.District ?? "";
                vm.PinCode = entity.PinCode;
                vm.MobileNumber = entity.MobileNumber;
                vm.StdCode = entity.StdCode;
                vm.Fax = entity.Fax;
                vm.Website = entity.Website;
                vm.EmailId = entity.EmailId;
                vm.AltLandlineOrMobile = entity.AltLandlineOrMobile;
                vm.AltEmailId = entity.AltEmailId;
                vm.AcademicYearStarted = entity.AcademicYearStarted;
                vm.IsRuralInstitution = entity.IsRuralInstitution ?? false;
                vm.IsMinorityInstitution = entity.IsMinorityInstitution ?? false;
                vm.TrustName = entity.TrustName ?? "";
                vm.PresidentName = entity.PresidentName ?? "";
                vm.AadhaarNumber = entity.AadhaarNumber;
                vm.PANNumber = entity.Pannumber;
                vm.RegistrationNumber = entity.RegistrationNumber;
                vm.RegistrationDate = entity.RegistrationDate;
                vm.Amendments = entity.Amendments ?? false;
                vm.ExistingTrustName = entity.ExistingTrustName ?? "";
                vm.GOKObtainedTrustName = entity.GokobtainedTrustName ?? "";
                vm.ChangesInTrustName = entity.ChangesInTrustName ?? false;
                vm.OtherNursingCollegeInCity = entity.OtherNursingCollegeInCity ?? false;
                vm.CategoryOfOrganisation = entity.CategoryOfOrganisation ?? "";
                vm.ContactPersonName = entity.ContactPersonName ?? "";
                vm.ContactPersonRelation = entity.ContactPersonRelation ?? "";
                vm.ContactPersonMobile = entity.ContactPersonMobile;
                vm.OtherPhysiotherapyCollegeInCity = entity.OtherPhysiotherapyCollegeInCity ?? false;
                vm.CoursesAppliedText = entity.CoursesAppliedText ?? "";
                vm.HeadOfInstitutionName = entity.HeadOfInstitutionName ?? "";
                vm.HeadOfInstitutionAddress = entity.HeadOfInstitutionAddress ?? "";
                vm.FinancingAuthorityName = entity.FinancingAuthorityName ?? "";
                vm.CollegeStatus = entity.CollegeStatus;
                vm.GovAutonomousCertNumber = entity.GovAutonomousCertNumber;
                vm.KncCertificateNumber = entity.KncCertificateNumber;

                // ✓ Convert int? → string? so it matches SelectListItem.Value format
                vm.TypeOfInstitution = entity.TypeOfInstitution?.ToString();
            }
            else
            {
                var collegeName = await _context.AffiliationCollegeMasters
                    .Where(e => e.CollegeCode.Trim() == collegeCode.Trim())
                    .Select(e => e.CollegeName)
                    .FirstOrDefaultAsync();

                vm.NameOfInstitution = collegeName ?? User.Identity?.Name ?? "";
            }

            // Build dropdown — int == int comparison, returns Value as string
            vm.TypeOfInstitutionList = await LoadInstitutionTypeList(facultyCode);

            return View(vm);
        }


        // ── COMPLETE FIXED POST (copy-paste ready) ───────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(ValueCountLimit = 100000, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Aff_InstituteDetails(
                                MedicalVm vm,
                                IFormFile? GovAutonomousCertFile,
                                IFormFile? GovCouncilMembershipFile,
                                IFormFile? GokOrderExistingCoursesFile,
                                IFormFile? FirstAffiliationNotifFile,
                                IFormFile? ContinuationAffiliationFile,
                                IFormFile? KncCertificateFile,
                                IFormFile? AmendedDoc,
                                IFormFile? AadhaarFile,
                                IFormFile? PANFile,
                                IFormFile? BankStatementFile,
                                IFormFile? RegistrationCertificateFile,
                                IFormFile? RegisteredTrustMemberDetails,
                                IFormFile? AuditStatementFile)
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            vm.FacultyCode = FacultyCode;
            vm.CollegeCode = CollegeCode;

            ModelState.Remove(nameof(vm.FacultyCode));
            ModelState.Remove(nameof(vm.CollegeCode));
            ModelState.Remove(nameof(vm.TypeOfInstitutionList));
            ModelState.Remove(nameof(vm.InstitutionId));
            ModelState.Remove(nameof(vm.AmendedDoc));
            ModelState.Remove(nameof(vm.AadhaarFile));
            ModelState.Remove(nameof(vm.GovAutonomousCertFile));
            ModelState.Remove(nameof(vm.GovCouncilMembershipFile));
            ModelState.Remove(nameof(vm.GokOrderExistingCoursesFile));
            ModelState.Remove(nameof(vm.FirstAffiliationNotifFile));
            ModelState.Remove(nameof(vm.ContinuationAffiliationFile));
            ModelState.Remove(nameof(vm.KncCertificateFile));
            ModelState.Remove(nameof(vm.PANFile));
            ModelState.Remove(nameof(vm.BankStatementFile));
            ModelState.Remove(nameof(vm.RegistrationCertificateFile));
            ModelState.Remove(nameof(vm.RegisteredTrustMemberDetails));
            ModelState.Remove(nameof(vm.AuditStatementFile));


            if (!ModelState.IsValid)
            {
                vm.TypeOfInstitutionList = await LoadInstitutionTypeList(facultyCode);
                return View(vm);
            }

            // ★ Load entity first so we can read back InstitutionId for the view
            var entity = await _context.InstitutionBasicDetails
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode.Trim() == facultyCode.Trim() &&
                    x.CollegeCode.Trim() == collegeCode.Trim());

            bool isNew = entity == null;

            if (isNew)
            {
                entity = new InstitutionBasicDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CreatedOn = DateTime.UtcNow
                };
                _context.InstitutionBasicDetails.Add(entity);
            }
            else
            {
                // ★ Carry InstitutionId into vm so view buttons work after a failed save
                vm.InstitutionId = entity.InstitutionId;
            }

            // Map scalar fields
            entity.TypeOfInstitution = vm.TypeOfInstitution;
            entity.NameOfInstitution = vm.NameOfInstitution ?? "";
            entity.AddressOfInstitution = vm.AddressOfInstitution ?? "";
            entity.VillageTownCity = vm.VillageTownCity ?? "";
            entity.Taluk = vm.Taluk ?? "";
            entity.District = vm.District ?? "";
            entity.PinCode = vm.PinCode;
            entity.MobileNumber = vm.MobileNumber;
            entity.StdCode = vm.StdCode;
            entity.Fax = vm.Fax;
            entity.Website = vm.Website;
            entity.EmailId = vm.EmailId;
            entity.AltLandlineOrMobile = vm.AltLandlineOrMobile;
            entity.AltEmailId = vm.AltEmailId;
            entity.AcademicYearStarted = vm.AcademicYearStarted;
            entity.IsRuralInstitution = vm.IsRuralInstitution;
            entity.IsMinorityInstitution = vm.IsMinorityInstitution;
            entity.TrustName = vm.TrustName ?? "";
            entity.PresidentName = vm.PresidentName ?? "";
            entity.AadhaarNumber = vm.AadhaarNumber;
            entity.Pannumber = vm.PANNumber;
            entity.RegistrationNumber = vm.RegistrationNumber;
            entity.RegistrationDate = vm.RegistrationDate;
            entity.Amendments = vm.Amendments;
            entity.ExistingTrustName = vm.ExistingTrustName ?? "";
            entity.GokobtainedTrustName = vm.GOKObtainedTrustName ?? "";
            entity.ChangesInTrustName = vm.ChangesInTrustName;
            entity.OtherNursingCollegeInCity = vm.OtherNursingCollegeInCity;
            entity.CategoryOfOrganisation = vm.CategoryOfOrganisation ?? "";
            entity.ContactPersonName = vm.ContactPersonName ?? "";
            entity.ContactPersonRelation = vm.ContactPersonRelation ?? "";
            entity.ContactPersonMobile = vm.ContactPersonMobile;
            entity.OtherPhysiotherapyCollegeInCity = vm.OtherPhysiotherapyCollegeInCity;
            entity.CoursesAppliedText = vm.CoursesAppliedText ?? "";
            entity.HeadOfInstitutionName = vm.HeadOfInstitutionName ?? "";
            entity.HeadOfInstitutionAddress = vm.HeadOfInstitutionAddress ?? "";
            entity.FinancingAuthorityName = vm.FinancingAuthorityName ?? "";
            entity.CollegeStatus = vm.CollegeStatus;
            entity.GovAutonomousCertNumber = vm.GovAutonomousCertNumber;
            entity.KncCertificateNumber = vm.KncCertificateNumber;

            // ★ AssignFileIfProvided skips null/empty uploads — existing DB bytes are untouched
            await AssignFileIfProvided(GovAutonomousCertFile, b => entity.GovAutonomousCertFile = b);
            await AssignFileIfProvided(GovCouncilMembershipFile, b => entity.GovCouncilMembershipFile = b);
            await AssignFileIfProvided(GokOrderExistingCoursesFile, b => entity.GokOrderExistingCoursesFile = b);
            await AssignFileIfProvided(FirstAffiliationNotifFile, b => entity.FirstAffiliationNotifFile = b);
            await AssignFileIfProvided(ContinuationAffiliationFile, b => entity.ContinuationAffiliationFile = b);
            await AssignFileIfProvided(KncCertificateFile, b => entity.KncCertificateFile = b);
            await AssignFileIfProvided(AmendedDoc, b => entity.AmendedDoc = b);
            await AssignFileIfProvided(AadhaarFile, b => entity.AadhaarFile = b);
            await AssignFileIfProvided(PANFile, b => entity.Panfile = b);
            await AssignFileIfProvided(BankStatementFile, b => entity.BankStatementFile = b);
            await AssignFileIfProvided(RegistrationCertificateFile, b => entity.RegistrationCertificateFile = b);
            await AssignFileIfProvided(RegisteredTrustMemberDetails, b => entity.RegisteredTrustMemberDetails = b);
            await AssignFileIfProvided(AuditStatementFile, b => entity.AuditStatementFile = b);

            try
            {
                await _context.SaveChangesAsync();
                ContinuousAffiliationController.MarkDone(HttpContext, "Aff_InstituteDetails");

                // ★ After insert, push the new PK back so a page reload shows View buttons
                vm.InstitutionId = entity.InstitutionId;

                TempData["Success"] = isNew ? "Saved successfully!" : "Updated successfully!";
                return RedirectToAction(nameof(Aff_TrustMemberDetails));
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = inner.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                    ? "A record already exists for this college."
                    : $"Database error: {inner}";
                vm.TypeOfInstitutionList = await LoadInstitutionTypeList(facultyCode);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                vm.TypeOfInstitutionList = await LoadInstitutionTypeList(facultyCode);
                return View(vm);
            }
        }


        // ── HELPERS ──────────────────────────────────────────────────────────────────


        private async Task<IActionResult> ServeFile(int id, Func<InstitutionBasicDetail, byte[]?> selector, string fileName)
        {
            var entity = await _context.InstitutionBasicDetails.AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.InstitutionId == id);

            if (entity == null) return NotFound("Record not found.");

            var bytes = selector(entity);
            if (bytes == null || bytes.Length == 0)
                return NotFound("No file has been uploaded for this field yet.");

            // Detect PDF vs image (simple magic-byte check)
            string mime = "application/octet-stream";
            if (bytes.Length >= 4)
            {
                if (bytes[0] == 0x25 && bytes[1] == 0x50) mime = "application/pdf";          // %PDF
                else if (bytes[0] == 0xFF && bytes[1] == 0xD8) mime = "image/jpeg";          // JPEG
                else if (bytes[0] == 0x89 && bytes[1] == 0x50) mime = "image/png";           // PNG
            }

            // Open inline in browser (not force-download)
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
            return File(bytes, mime);
        }

        // ── Individual download endpoints ────────────────────────────────────────────
        [HttpGet]
        public Task<IActionResult> DownloadGovAutonomousCert(int id)
            => ServeFile(id, e => e.GovAutonomousCertFile, "GovAutonomousCert.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadGovCouncilMembership(int id)
            => ServeFile(id, e => e.GovCouncilMembershipFile, "GovCouncilMembership.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadGokOrderExistingCourses(int id)
            => ServeFile(id, e => e.GokOrderExistingCoursesFile, "GokOrderExistingCourses.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadFirstAffiliationNotif(int id)
            => ServeFile(id, e => e.FirstAffiliationNotifFile, "FirstAffiliationNotif.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadContinuationAffiliation(int id)
            => ServeFile(id, e => e.ContinuationAffiliationFile, "ContinuationAffiliation.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadKncCertificate(int id)
            => ServeFile(id, e => e.KncCertificateFile, "KncCertificate.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadAmendedDoc(int id)
            => ServeFile(id, e => e.AmendedDoc, "AmendedDoc.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadAadhaarFile(int id)
            => ServeFile(id, e => e.AadhaarFile, "Aadhaar.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadPANFile(int id)
            => ServeFile(id, e => e.Panfile, "PAN.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadBankStatement(int id)
            => ServeFile(id, e => e.BankStatementFile, "BankStatement.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadRegistrationCertificate(int id)
            => ServeFile(id, e => e.RegistrationCertificateFile, "RegistrationCertificate.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadRegisteredTrustMemberDetails(int id)
            => ServeFile(id, e => e.RegisteredTrustMemberDetails, "RegisteredTrustMemberDetails.pdf");

        [HttpGet]
        public Task<IActionResult> DownloadAuditStatement(int id)
            => ServeFile(id, e => e.AuditStatementFile, "AuditStatement.pdf");

        private async Task<List<SelectListItem>> LoadInstitutionTypeList(string facultyCode)
        {
            // Trim and parse safely
            var trimmed = facultyCode?.Trim() ?? "";

            if (!int.TryParse(trimmed, out int facultyId))
                return new List<SelectListItem>(); // facultyCode is non-numeric — log this!

            return await _context.MstInstitutionTypes
                .OrderBy(t => t.InstitutionType)
                .Select(t => new SelectListItem
                {
                    Value = t.InstitutionTypeId.ToString(),
                    Text = t.InstitutionType
                })
                .ToListAsync();
        }

        private static async Task AssignFileIfProvided(IFormFile? file, Action<byte[]> setter)
        {
            if (file == null || file.Length == 0) return;   // ← preserve existing DB bytes
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            setter(ms.ToArray());
        }


        private async Task<byte[]?> SafeToBytes(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch
            {
                return null; // Fail-safe: return null instead of throwing
            }
        }

        private static async Task<byte[]> ToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        // ✅ REQUIRED HELPER METHOD - Convert IFormFile to byte[]
        private async Task<byte[]?> ToBytesAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            return stream.ToArray();
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Aff_TrustMemberDetails()
        {
            //var (facultyCode, collegeCode) = GetSessionCodes();

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            var vm = new Medical_TrustMemberDetailsListVM
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                Rows = new List<TrustMemberDetailsRowVM>()
            };

            vm.DesignationList = await GetDesignationListAsync(facultyCode);

            var existing = await _context.ContinuationTrustMemberDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .OrderBy(x => x.TrustMemberName)
                .ToListAsync();

            if (existing.Any())
            {
                vm.Rows = existing.Select(x => new TrustMemberDetailsRowVM
                {
                    SlNo = x.SlNo,
                    FacultyCode = x.FacultyCode,
                    CollegeCode = x.CollegeCode,
                    TrustMemberName = x.TrustMemberName,
                    Qualification = x.Qualification,
                    MobileNumber = x.MobileNumber,
                    Age = x.Age,
                    JoiningDateString = x.JoiningDate?.ToString("yyyy-MM-dd"),
                    DesignationCode = x.DesignationId
                }).ToList();
            }
            else
            {
                vm.Rows.Add(new TrustMemberDetailsRowVM
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode
                });
            }

            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aff_TrustMemberDetails(Medical_TrustMemberDetailsListVM vm)
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            vm.FacultyCode = facultyCode;
            vm.CollegeCode = collegeCode;

            ModelState.Remove(nameof(vm.FacultyCode));
            ModelState.Remove(nameof(vm.CollegeCode));

            vm.DesignationList = await GetDesignationListAsync(facultyCode);

            if (!ModelState.IsValid)
                return View(vm);

            // ── Remove existing rows ──
            var existing = _context.ContinuationTrustMemberDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);
            _context.ContinuationTrustMemberDetails.RemoveRange(existing);

            // ── Load designations ──
            var allDesignations = await _context.DesignationMasters
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .ToListAsync();

            foreach (var row in vm.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.TrustMemberName))
                    continue;

                DateOnly? joiningDate = null;

                if (!string.IsNullOrWhiteSpace(row.JoiningDateString) &&
                    DateOnly.TryParseExact(
                        row.JoiningDateString,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsed))
                {
                    joiningDate = parsed;
                }

                var matchedDesignation = allDesignations
                    .FirstOrDefault(d => d.DesignationCode == row.DesignationCode);

                var entity = new ContinuationTrustMemberDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    TrustMemberName = row.TrustMemberName.Trim(),
                    Qualification = row.Qualification,
                    MobileNumber = row.MobileNumber,
                    Age = row.Age,
                    JoiningDate = joiningDate,

                    DesignationId = row.DesignationCode,
                    Designation = matchedDesignation?.DesignationName ?? row.DesignationCode
                };

                _context.ContinuationTrustMemberDetails.Add(entity);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Trust member details saved successfully.";

            // 🔽 REDIRECTION BASED ON SESSION (NO DB REQUIRED)
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (courseLevel == "UG")
            {
                return RedirectToAction("Details_Of_MBBS", "ContinuesAffiliation_Facultybased");
            }
            else if (courseLevel == "PG")
            {
                return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased");
            }
            else if (courseLevel == "SS")
            {
                return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased");
            }

            // ❗ REQUIRED to avoid compiler error
            throw new Exception("Invalid CourseLevel in session");
        }

        private async Task<List<SelectListItem>> GetDesignationListAsync(string facultyCode)
        {
            if (string.IsNullOrEmpty(facultyCode)) return new List<SelectListItem>();

            var query = _context.DesignationMasters
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .OrderBy(d => d.DesignationName)
                .Select(d => new SelectListItem
                {
                    Value = d.DesignationCode,
                    Text = d.DesignationName ?? ""
                });

            var items = await query.ToListAsync();

            if (items.Any()) return items;

            // Fallback - numeric faculty code
            if (int.TryParse(facultyCode, out int fid))
            {
                items = await _context.DesignationMasters
                    .Where(d => d.FacultyCode == fid)
                    .OrderBy(d => d.DesignationName)
                    .Select(d => new SelectListItem
                    {
                        Value = d.DesignationCode,
                        Text = d.DesignationName ?? ""
                    })
                    .ToListAsync();
            }

            return items;
        }

        [HttpGet]
        public async Task<IActionResult> AFF_SanIntake()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode")
                              ?? User?.FindFirst("FacultyCode")?.Value;

            var collegeCode = HttpContext.Session.GetString("CollegeCode")
                              ?? User?.FindFirst("CollegeCode")?.Value;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // All courses for this faculty
            var courses = await _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            // Existing sanctioned intake per course for this college+faculty
            var existing = await _context.AffSanctionedIntakeForCourses
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .ToListAsync();

            var vm = new SanctionedIntakeCreateVm
            {
                Courses = courses.Select(c =>
                {
                    var existingRow = existing
                        // prefer match by CourseName if that's what DB stores; adjust to CourseCode if available
                        .FirstOrDefault(e => e.CourseName == c.CourseName);

                    return new SanctionedIntakeRowVm
                    {
                        CourseCode = c.CourseCode.ToString(),
                        CourseName = c.CourseName ?? string.Empty,
                        SanctionedIntake = existingRow?.SanctionedIntake,
                        EligibleSeatSlab = existingRow?.EligibleSeatSlab,
                        ExistingIntakeId = existingRow?.Id   // <- important: set this so view can render "View" link
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AFF_SanIntake(SanctionedIntakeCreateVm model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode")
                              ?? User?.FindFirst("CollegeCode")?.Value;
            var facultyCode = HttpContext.Session.GetString("FacultyCode")
                              ?? User?.FindFirst("FacultyCode")?.Value;

            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // basic validation: at least one row has intake filled
            if (model.Courses == null || !model.Courses.Any(r => !string.IsNullOrWhiteSpace(r.SanctionedIntake)))
            {
                ModelState.AddModelError(string.Empty, "Enter sanctioned intake for at least one course.");
            }

            if (!ModelState.IsValid)
            {
                // re-load course names in case they were not posted
                var courses = await _context.MstCourses
                    .Where(c => c.FacultyCode.ToString() == facultyCode)
                    .OrderBy(c => c.CourseName)
                    .ToListAsync();

                foreach (var row in model.Courses)
                {
                    var course = courses.FirstOrDefault(c => c.CourseCode.ToString() == row.CourseCode);
                    if (course != null)
                        row.CourseName = course.CourseName ?? row.CourseName;
                }

                return View(model);
            }

            try
            {
                foreach (var row in model.Courses.Where(r => !string.IsNullOrWhiteSpace(r.SanctionedIntake)))
                {
                    var course = await _context.MstCourses.FirstOrDefaultAsync(c =>
                        c.CourseCode.ToString() == row.CourseCode &&
                        c.FacultyCode.ToString() == facultyCode);

                    if (course == null)
                        continue;

                    byte[]? documentBytes = null;
                    if (row.DocumentFile != null && row.DocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await row.DocumentFile.CopyToAsync(ms);
                        documentBytes = ms.ToArray();
                    }

                    var entity = new AffSanctionedIntakeForCourse
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseName = course.CourseName ?? row.CourseName,
                        SanctionedIntake = row.SanctionedIntake ?? string.Empty,
                        EligibleSeatSlab = row.EligibleSeatSlab,
                        DocumentData = documentBytes,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = HttpContext.Session.GetString("UserName") ?? User?.Identity?.Name
                    };

                    _context.AffSanctionedIntakeForCourses.Add(entity);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Sanctioned intake saved for selected courses.";
                return RedirectToAction(nameof(AffCourseDetails));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AFF_SanIntake save error: {ex}");
                TempData["Error"] = "Unable to save sanctioned intake. " + ex.Message;

                var courses = await _context.MstCourses
                    .Where(c => c.FacultyCode.ToString() == facultyCode)
                    .OrderBy(c => c.CourseName)
                    .ToListAsync();

                foreach (var row in model.Courses)
                {
                    var course = courses.FirstOrDefault(c => c.CourseCode.ToString() == row.CourseCode);
                    if (course != null)
                        row.CourseName = course.CourseName ?? row.CourseName;
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Administrative_teachingBlock()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? "";
            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // Load DISTINCT facilities (your deduping is correct)
            var mstFacilities = await _context.AdministrativeFacilityTypes
                .OrderBy(f => f.FacilityId)
                .GroupBy(f => f.FacilityId)  // Dedupe by FacilityId
                .Select(g => g.First())
                .ToListAsync();

            // Load existing data
            var existing = await _context.AffAdminTeachingBlocks
                .Where(a => a.FacultyCode == facultyCode && a.CollegeCode == collegeCode)
                .ToListAsync();

            var vm = new AdminTeachingBlockListVM();
            foreach (var f in mstFacilities)
            {
                var row = existing.FirstOrDefault(x => x.FacilityId == f.FacilityId.ToString());
                vm.Rows.Add(new AdminTeachingBlockRowVM
                {
                    Id = row?.Id ?? 0,
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    FacilityId = f.FacilityId,
                    Facilities = f.FacilityName,
                    SizeSqFtAsPerNorms = f.MinAreaSqM.ToString(),
                    IsAvailable = row?.IsAvailable == "Yes",
                    NoOfRooms = row?.NoOfRooms ?? "",
                    SizeSqFtAvailablePerRoom = row?.SizeSqFtAvailablePerRoom ?? ""
                });
            }

            return View(vm);
        }

        //csharp Medical_Affiliation\Controllers\NursingContinuesAffiliationController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public async Task<IActionResult> Administrative_teachingBlock(AdminTeachingBlockListVM vm)
        {
            // 🔴 BASIC NULL CHECK
            if (vm?.Rows == null || !vm.Rows.Any())
            {
                TempData["Error"] = "No rows submitted.";
                return RedirectToAction(nameof(Administrative_teachingBlock));
            }

            // 🔐 SESSION VALUES
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            // ❗ REMOVE SESSION BASED VALIDATION
            ModelState.Remove("FacultyCode");
            ModelState.Remove("CollegeCode");

            int processed = 0;
            int skipped = 0;

            try
            {
                foreach (var row in vm.Rows)
                {
                    // 🚫 Ignore rows without FacilityId
                    if (row.FacilityId <= 0)
                    {
                        skipped++;
                        continue;
                    }

                    // 🚫 Ignore completely empty rows
                    bool hasAnyValue =
                        row.IsAvailable ||
                        !string.IsNullOrWhiteSpace(row.NoOfRooms) ||
                        !string.IsNullOrWhiteSpace(row.SizeSqFtAvailablePerRoom);

                    if (!hasAnyValue)
                    {
                        skipped++;
                        continue;
                    }

                    // ✔ NULL SAFE VALUES
                    string noOfRooms = string.IsNullOrWhiteSpace(row.NoOfRooms) ? null : row.NoOfRooms.Trim();
                    string sizeSqFt = string.IsNullOrWhiteSpace(row.SizeSqFtAvailablePerRoom) ? null : row.SizeSqFtAvailablePerRoom.Trim();

                    var entity = await _context.AffAdminTeachingBlocks.FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.FacilityId == row.FacilityId.ToString());

                    if (entity == null)
                    {
                        entity = new AffAdminTeachingBlock
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            FacilityId = row.FacilityId.ToString(),
                            Facilities = row.Facilities?.Trim(),
                            SizeSqFtAsPerNorms = row.SizeSqFtAsPerNorms?.Trim(),
                            IsAvailable = row.IsAvailable ? "Yes" : "No",
                            NoOfRooms = noOfRooms,
                            SizeSqFtAvailablePerRoom = sizeSqFt,
                            CreatedOn = DateTime.Now,
                            CreatedBy = User.Identity?.Name ?? "System"
                        };

                        _context.AffAdminTeachingBlocks.Add(entity);
                    }
                    else
                    {
                        entity.IsAvailable = row.IsAvailable ? "Yes" : "No";
                        entity.NoOfRooms = noOfRooms;
                        entity.SizeSqFtAvailablePerRoom = sizeSqFt;
                        entity.Facilities = row.Facilities?.Trim();
                        entity.SizeSqFtAsPerNorms = row.SizeSqFtAsPerNorms?.Trim();

                        _context.AffAdminTeachingBlocks.Update(entity);
                    }

                    processed++;
                }

                if (processed > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"Saved {processed} row(s). Skipped {skipped} empty row(s).";
                }
                else
                {
                    TempData["Warning"] = "No valid rows to save.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while saving data.";
                TempData["Debug"] = ex.Message;
            }

            return RedirectToAction(nameof(aff_FacultyDetails));
        }


        [HttpGet]
        public async Task<IActionResult> DownloadAffSanIntakeFile(int id)
        {
            var row = await _context.AffSanctionedIntakeForCourses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (row == null || row.DocumentData == null || row.DocumentData.Length == 0)
            {
                TempData["Error"] = "Document not found.";
                return NotFound();
            }

            // Use stored content type if you saved it; fall back to octet-stream
            var contentType = "application/octet-stream";
            var safeName = string.IsNullOrWhiteSpace(row.CourseName) ? $"SanctionedIntake_{id}" : row.CourseName;
            var fileName = $"{safeName}.pdf"; // adjust extension if you store original filename

            return File(row.DocumentData, contentType, fileName);
        }

        [HttpGet]
        public IActionResult Aff_HostelDetails()
        {
            var courseLevel = CourseLevel;
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var vm = new AffHostelDetailsCreateVm
            {
                HostelTypes = _context.MstHosteltypes
                    .Select(x => new SelectListItem
                    {
                        Value = x.HospitalType,
                        Text = x.HospitalType
                    })
                    .ToList()
            };

            // Load existing hostel
            vm.Hostel = _context.AffHostelDetails
                .FirstOrDefault(h => h.CollegeCode == collegeCode && h.FacultyCode == facultyCode && h.CourseLevel == courseLevel)
                ?? new AffHostelDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = courseLevel
                };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aff_HostelDetails(
                                                 AffHostelDetailsCreateVm vm,
                                                 IFormFile? possessionFile)
        {
            var courseLevel = CourseLevel;
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            vm.Hostel.CollegeCode = collegeCode;
            vm.Hostel.FacultyCode = facultyCode;
            vm.Hostel.CourseLevel = courseLevel;

            // Remove session-bound validation
            ModelState.Remove("Hostel.CollegeCode");
            ModelState.Remove("Hostel.FacultyCode");
            ModelState.Remove("Hostel.CourseLevel");

            // 📄 File upload
            if (possessionFile != null && possessionFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await possessionFile.CopyToAsync(ms);
                vm.Hostel.PossessionProofPath = ms.ToArray();
            }

            // ❌ Validation fail → reload dropdown
            if (!ModelState.IsValid)
            {
                vm.HostelTypes = _context.MstHosteltypes
                    .Select(x => new SelectListItem
                    {
                        Value = x.HospitalType,
                        Text = x.HospitalType
                    })
                    .ToList();

                return View(vm);
            }

            // 🔍 Check existing
            var existingHostel = await _context.AffHostelDetails
                .FirstOrDefaultAsync(h =>
                    h.CollegeCode == collegeCode &&
                    h.FacultyCode == facultyCode && h.CourseLevel == courseLevel);

            if (existingHostel != null)
            {

                // ================= BASIC DETAILS =================
                existingHostel.HostelType = vm.Hostel.HostelType;
                existingHostel.BuiltUpAreaSqFt = vm.Hostel.BuiltUpAreaSqFt;
                existingHostel.HasSeparateHostel = vm.Hostel.HasSeparateHostel;
                existingHostel.SeparateProvisionMaleFemale = vm.Hostel.SeparateProvisionMaleFemale;
                existingHostel.TotalFemaleStudents = vm.Hostel.TotalFemaleStudents;
                existingHostel.TotalFemaleRooms = vm.Hostel.TotalFemaleRooms;
                existingHostel.TotalMaleStudents = vm.Hostel.TotalMaleStudents;
                existingHostel.TotalMaleRooms = vm.Hostel.TotalMaleRooms;

                // ================= STUDENT AMENITIES =================

                // In college
                existingHostel.CommonRoomMen = vm.Hostel.CommonRoomMen;
                existingHostel.CommonRoomWomen = vm.Hostel.CommonRoomWomen;
                existingHostel.AnyOtherFacility = vm.Hostel.AnyOtherFacility;

                // Hostel info
                existingHostel.HostelFacilityDetails = vm.Hostel.HostelFacilityDetails;
                existingHostel.HostelMenCount = vm.Hostel.HostelMenCount;
                existingHostel.HostelWomenCount = vm.Hostel.HostelWomenCount;
                existingHostel.OwnOrRented = vm.Hostel.OwnOrRented;
                existingHostel.SpacePerStudent = vm.Hostel.SpacePerStudent;

                // Yes/No facilities
                existingHostel.SleepingFurniture = vm.Hostel.SleepingFurniture;
                existingHostel.SanitaryBathing = vm.Hostel.SanitaryBathing;
                existingHostel.DiningHall = vm.Hostel.DiningHall;
                existingHostel.HostelCommonRoom = vm.Hostel.HostelCommonRoom;
                existingHostel.VisitorsRoom = vm.Hostel.VisitorsRoom;
                existingHostel.KitchenPantry = vm.Hostel.KitchenPantry;
                existingHostel.WardenOffice = vm.Hostel.WardenOffice;
                existingHostel.ReceptionCounter = vm.Hostel.ReceptionCounter;
                existingHostel.GamesRecreation = vm.Hostel.GamesRecreation;
                existingHostel.MedicalFacilities = vm.Hostel.MedicalFacilities;

                // File update only if new uploaded
                if (vm.Hostel.PossessionProofPath != null)
                    existingHostel.PossessionProofPath = vm.Hostel.PossessionProofPath;
            }
            else
            {
                // ➕ New record
                _context.AffHostelDetails.Add(vm.Hostel);
            }

            await _context.SaveChangesAsync();

            //return RedirectToAction("TeachingStaffDepartmentWise");

            return RedirectToAction("IncreaseIntake", "ContinuousAffiliationIncreaseintake");
        }



        [HttpGet]
        public async Task<IActionResult> DownloadPossessionProof(string collegeCode, string facultyCode)
        {
            var hostel = await _context.AffHostelDetails
                .FirstOrDefaultAsync(h => h.CollegeCode == collegeCode && h.FacultyCode == facultyCode);

            if (hostel?.PossessionProofPath == null) return NotFound();

            return File(hostel.PossessionProofPath, "application/pdf", "PossessionProof.pdf");
        }


        [HttpGet]
        public IActionResult AFF_HostelDetailswithfacilities()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // master: all hostel facilities
            var facilities = _context.MstHostelFacilities
                .Where(e => e.FacultyId.ToString() == facultyCode)
                .OrderBy(f => f.HostelFacilityId)
                .ToList();

            // detail: existing availability for this faculty/college
            var affRows = _context.AffHostelFacilityDetails
                .Where(a => a.FacultyCode == facultyCode && a.CollegeCode == collegeCode)
                .ToList();

            var vm = new HostelFacilityListVm
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                Rows = facilities
                    .Select(f =>
                    {
                        // CORRECT MATCH: match by FacilityId (int) — previous code compared FacultyCode to HostelFacilityId which is wrong
                        var aff = affRows.FirstOrDefault(a =>
                            a.FacilityId == f.HostelFacilityId &&
                            a.CollegeCode == collegeCode &&
                            a.FacultyCode == facultyCode);

                        return new HostelFacilityRowVm
                        {
                            HostelFacilityId = f.HostelFacilityId,
                            HostelFacilityName = f.HostelFacilityName,
                            FacultyId = f.FacultyId,
                            AffId = aff?.Id,                     // PK from Aff_ table
                            FacultyCode = facultyCode,
                            CollegeCode = collegeCode,
                            IsAvailable = aff?.IsAvailable ?? false
                        };
                    })
                    .ToList()
            };

            return View(vm);
        }

        // Replace the POST action for AFF_HostelDetailswithfacilities with this diagnostic-friendly version
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AFF_HostelDetailswithfacilities(HostelFacilityListVm model)
        {
            var form = Request.Form;

            // 🔐 Server controlled values
            var facultyCodeSession = HttpContext.Session.GetString("FacultyCode");
            var collegeCodeSession = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCodeSession) || string.IsNullOrEmpty(collegeCodeSession))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var rows = new List<HostelFacilityRowVm>();

                // ✅ ALWAYS rebuild rows from Request.Form (NO model binding dependency)
                var indices = form.Keys
                    .Where(k => k.StartsWith("Rows["))
                    .Select(k =>
                    {
                        var part = k.Substring("Rows[".Length);
                        return part.Split(']').FirstOrDefault();
                    })
                    .Where(i => int.TryParse(i, out _))
                    .Select(int.Parse)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();

                foreach (var i in indices)
                {
                    var prefix = $"Rows[{i}]";

                    if (!int.TryParse(form[$"{prefix}.HostelFacilityId"], out int facilityId))
                        continue;

                    var rawIsAvailable = form[$"{prefix}.IsAvailable"].ToString();

                    // ✅ checkbox value resolver (true,false OR false)
                    bool isAvailable = rawIsAvailable
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Any(v => v.Equals("true", StringComparison.OrdinalIgnoreCase));

                    rows.Add(new HostelFacilityRowVm
                    {
                        HostelFacilityId = facilityId,
                        HostelFacilityName = form[$"{prefix}.HostelFacilityName"],
                        FacultyCode = facultyCodeSession,
                        CollegeCode = collegeCodeSession,
                        AffId = int.TryParse(form[$"{prefix}.AffId"], out int aid) ? aid : (int?)null,
                        IsAvailable = isAvailable
                    });
                }

                if (!rows.Any())
                {
                    TempData["Error"] = "No hostel facility data received.";
                    return RedirectToAction(nameof(AFF_HostelDetailswithfacilities));
                }

                // 🔄 SAVE / UPDATE
                foreach (var row in rows)
                {
                    AffHostelFacilityDetail entity = null;

                    if (row.AffId.HasValue && row.AffId > 0)
                    {
                        entity = await _context.AffHostelFacilityDetails
                            .FirstOrDefaultAsync(x => x.Id == row.AffId.Value);
                    }

                    if (entity == null)
                    {
                        entity = await _context.AffHostelFacilityDetails
                            .FirstOrDefaultAsync(x =>
                                x.FacilityId == row.HostelFacilityId &&
                                x.FacultyCode == facultyCodeSession &&
                                x.CollegeCode == collegeCodeSession);
                    }

                    if (entity == null)
                    {
                        entity = new AffHostelFacilityDetail
                        {
                            FacilityId = row.HostelFacilityId,
                            FacilityName = row.HostelFacilityName,
                            FacultyCode = facultyCodeSession,
                            CollegeCode = collegeCodeSession,
                            IsAvailable = row.IsAvailable
                        };

                        _context.AffHostelFacilityDetails.Add(entity);
                    }
                    else
                    {
                        // ✅ UPDATE
                        entity.IsAvailable = row.IsAvailable;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Hostel facilities saved successfully.";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Database error: " + (ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unexpected error: " + ex.Message;
            }

            return RedirectToAction("administrative_teachingBlock");
        }




        //[HttpGet]
        //public IActionResult AFF_TeachingFacultyDetails()
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //    {
        //        TempData["Error"] = "Session expired. Please log in again.";
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // Dropdown lists
        //    var subjectsList = _context.MstCourses
        //        .Where(c => c.FacultyCode.ToString() == facultyCode)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CourseCode.ToString(),
        //            Text = c.CourseName ?? ""
        //        })
        //        .Distinct()
        //        .ToList();

        //    var designationsList = _context.DesignationMasters
        //        .Where(c => c.FacultyCode.ToString() == facultyCode)
        //        .Select(d => new SelectListItem
        //        {
        //            Value = d.DesignationCode,
        //            Text = d.DesignationName ?? ""
        //        })
        //        .ToList();

        //    var departmentsList = _context.MstCourses
        //        .Where(e => e.FacultyCode.ToString() == facultyCode)
        //        .Select(d => new SelectListItem
        //        {
        //            Value = d.CourseCode.ToString(),
        //            Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //        })
        //        .ToList();

        //    // ✅ Get ALL faculty records from new table
        //    var facultyDetails = _context.AffTeachingFacultyAllDetails
        //        .Where(f => f.CollegeCode == collegeCode
        //                    && f.FacultyCode == facultyCode
        //                    && f.IsRemoved != true)
        //        .Take(1000)
        //        .ToList();

        //    List<Aff_FacultyDetailsViewModel> vmList = new List<Aff_FacultyDetailsViewModel>();

        //    if (!facultyDetails.Any())
        //    {
        //        TempData["Info"] = "No faculty records found for this faculty.";
        //        vmList.Add(new Aff_FacultyDetailsViewModel
        //        {
        //            Subjects = subjectsList,
        //            Designations = designationsList,
        //            DepartmentDetailsList = departmentsList
        //        });
        //        return View(vmList);
        //    }

        //    // ✅ Complete mapping of ALL fields
        //    vmList = facultyDetails.Select(f => new Aff_FacultyDetailsViewModel
        //    {
        //        FacultyDetailId = f.TeachingFacultyId,
        //        NameOfFaculty = f.TeachingFacultyName,
        //        Subject = f.Subject,
        //        DateOfBirth = f.DateOfBirth == default ? (DateTime?)null : f.DateOfBirth.ToDateTime(TimeOnly.MinValue),
        //        Mobile = f.Mobile,
        //        Email = f.Email,
        //        Aadhaar = f.AadhaarNumber,
        //        PAN = f.Pannumber,
        //        RN_RMNumber = f.RnRmnumber,
        //        Department = f.Department,
        //        DepartmentDetails = f.DepartmentDetails,
        //        SelectedDepartment = f.DepartmentDetails,
        //        Designation = f.Designation,
        //        Qualification = f.Qualification,
        //        UGInstituteName = f.UginstituteName,
        //        UGYearOfPassing = f.UgyearOfPassing,
        //        PGInstituteName = f.PginstituteName,
        //        PGYearOfPassing = f.PgyearOfPassing,
        //        PGPassingSpecialization = f.PgpassingSpecialization,
        //        TeachingExperienceAfterUGYears = f.TeachingExperienceAfterUgyears,
        //        TeachingExperienceAfterPGYears = f.TeachingExperienceAfterPgyears,
        //        DateOfJoining = f.DateOfJoining == default ? (DateTime?)null : f.DateOfJoining.ToDateTime(TimeOnly.MinValue),
        //        RecognizedPgTeacher = (bool)f.RecognizedPgTeacher,
        //        RecognizedPhDTeacher = (bool)f.RecognizedPhDteacher,
        //        IsRecognizedPGGuide = (bool)f.IsRecognizedPgguide,
        //        IsExaminer = (bool)f.IsExaminer,
        //        ExaminerFor = f.ExaminerFor,
        //        ExaminerForList = !string.IsNullOrEmpty(f.ExaminerFor)
        //            ? f.ExaminerFor.Split(',').ToList()
        //            : new List<string>(),
        //        LitigationPending = (bool)f.LitigationPending,
        //        NRTSNumber = f.Nrtsnumber,
        //        RGUHSTIN = f.Rguhstin,
        //        RemoveRemarks = f.RemoveRemarks,
        //        IsRemoved = f.IsRemoved,

        //        Subjects = subjectsList,
        //        Designations = designationsList,
        //        DepartmentDetailsList = departmentsList
        //    }).ToList();

        //    return View(vmList);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult AFF_TeachingFacultyDetails(IList<Aff_FacultyDetailsViewModel> model)
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //    {
        //        TempData["Error"] = "Session expired. Please log in again.";
        //        return RedirectToAction("Login", "Account");
        //    }

        //    if (model == null || !model.Any())
        //    {
        //        TempData["Error"] = "No data to save.";
        //        // Repopulate dropdowns...
        //        return View(model);
        //    }

        //    using (var transaction = _context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var incomingIds = model
        //                .Where(m => m.FacultyDetailId > 0)
        //                .Select(m => m.FacultyDetailId)
        //                .ToHashSet();

        //            var existingFaculty = _context.AffTeachingFacultyAllDetails
        //                .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
        //                .ToList();

        //            foreach (var m in model)
        //            {
        //                // ✅ Convert file uploads
        //                byte[] aadhaarDoc = m.AadhaarDocument != null ? ConvertFileToBytes(m.AadhaarDocument) : null;
        //                byte[] panDoc = m.PANDocument != null ? ConvertFileToBytes(m.PANDocument) : null;
        //                byte[] rnRmDoc = m.RN_RMDocument != null ? ConvertFileToBytes(m.RN_RMDocument) : null;
        //                byte[] form16Doc = m.Form16OrLast6MonthsStatement != null ? ConvertFileToBytes(m.Form16OrLast6MonthsStatement) : null;
        //                byte[] appointmentDoc = m.AppointmentOrderDocument != null ? ConvertFileToBytes(m.AppointmentOrderDocument) : null;
        //                byte[] guideDoc = m.GuideRecognitionDoc != null ? ConvertFileToBytes(m.GuideRecognitionDoc) : null;
        //                byte[] phdDoc = m.PhDRecognitionDoc != null ? ConvertFileToBytes(m.PhDRecognitionDoc) : null;
        //                byte[] litigationDoc = m.LitigationDoc != null ? ConvertFileToBytes(m.LitigationDoc) : null;
        //                byte[] OnlineTeachersDatabase = m.OnlineTeachersDatabase != null ? ConvertFileToBytes(m.OnlineTeachersDatabase) : null;
        //                byte[] madetorecruit = m.madetorecruit != null ? ConvertFileToBytes(m.madetorecruit) : null;

        //                var existing = existingFaculty.FirstOrDefault(f => f.TeachingFacultyId == m.FacultyDetailId);

        //                if (existing != null)
        //                {
        //                    // ✅ UPDATE all fields
        //                    existing.TeachingFacultyName = m.NameOfFaculty?.Trim() ?? "N/A";
        //                    existing.Subject = m.Subject?.Trim() ?? "N/A";
        //                    // Replace this line in the POST AFF_TeachingFacultyDetails action (inside the foreach loop for updating existing records):
        //                    // existing.DateOfBirth = m.DateOfBirth;

        //                    // With the following code to properly convert nullable DateTime? to DateOnly:
        //                    existing.DateOfBirth = m.DateOfBirth.HasValue ? DateOnly.FromDateTime(m.DateOfBirth.Value) : default;
        //                    //existing.DateOfBirth = m.DateOfBirth;
        //                    existing.Mobile = m.Mobile?.Trim() ?? "N/A";
        //                    existing.Email = m.Email?.Trim() ?? "N/A";
        //                    existing.AadhaarNumber = m.Aadhaar?.Trim() ?? "N/A";
        //                    existing.Pannumber = m.PAN?.Trim() ?? "N/A";
        //                    existing.RnRmnumber = m.RN_RMNumber?.Trim() ?? "N/A";
        //                    existing.Department = m.Department?.Trim() ?? "N/A";
        //                    existing.DepartmentDetails = m.DepartmentDetails?.Trim() ?? "N/A";
        //                    existing.Designation = m.Designation?.Trim() ?? "N/A";
        //                    existing.Qualification = m.Qualification?.Trim() ?? "N/A";
        //                    existing.UginstituteName = m.UGInstituteName?.Trim() ?? "N/A";
        //                    existing.UgyearOfPassing = (int)m.UGYearOfPassing;
        //                    existing.PginstituteName = m.PGInstituteName?.Trim() ?? "N/A";
        //                    existing.PgyearOfPassing = (int)m.PGYearOfPassing;
        //                    existing.PgpassingSpecialization = m.PGPassingSpecialization?.Trim() ?? "N/A";
        //                    existing.TeachingExperienceAfterUgyears = m.TeachingExperienceAfterUGYears;
        //                    existing.TeachingExperienceAfterPgyears = m.TeachingExperienceAfterPGYears;
        //                    existing.DateOfJoining = m.DateOfJoining.HasValue ? DateOnly.FromDateTime(m.DateOfJoining.Value) : default;
        //                    existing.RecognizedPgTeacher = m.RecognizedPgTeacher;
        //                    existing.RecognizedPhDteacher = m.RecognizedPhDTeacher;
        //                    existing.IsRecognizedPgguide = m.IsRecognizedPGGuide;
        //                    existing.IsExaminer = m.IsExaminer;
        //                    existing.ExaminerFor = m.ExaminerForList != null ? string.Join(",", m.ExaminerForList) : null;
        //                    existing.LitigationPending = m.LitigationPending;
        //                    existing.Nrtsnumber = m.NRTSNumber?.Trim() ?? "N/A";
        //                    existing.Rguhstin = m.RGUHSTIN?.Trim() ?? "N/A";
        //                    existing.RemoveRemarks = m.RemoveRemarks;
        //                    existing.IsRemoved = !string.IsNullOrWhiteSpace(m.RemoveRemarks);

        //                    // Update documents if new files uploaded
        //                    if (aadhaarDoc != null) existing.AadhaarDocument = aadhaarDoc;
        //                    if (panDoc != null) existing.Pandocument = panDoc;
        //                    if (rnRmDoc != null) existing.RnRmdocument = rnRmDoc;
        //                    if (form16Doc != null) existing.Form16OrLast6MonthsStatement = form16Doc;
        //                    if (appointmentDoc != null) existing.AppointmentOrderDocument = appointmentDoc;
        //                    if (guideDoc != null) existing.GuideRecognitionDoc = guideDoc;
        //                    if (phdDoc != null) existing.PhDrecognitionDoc = phdDoc;
        //                    if (litigationDoc != null) existing.LitigationDoc = litigationDoc;
        //                    if (OnlineTeachersDatabase != null) existing.OnlineTeachersDatabase = OnlineTeachersDatabase;
        //                    if (madetorecruit != null) existing.Madetorecruit = madetorecruit;

        //                    _context.AffTeachingFacultyAllDetails.Update(existing);
        //                }
        //                else
        //                {
        //                    // ✅ INSERT new record with ALL fields
        //                    var faculty = new AffTeachingFacultyAllDetail
        //                    {
        //                        FacultyCode = facultyCode,
        //                        CollegeCode = collegeCode,
        //                        TeachingFacultyName = m.NameOfFaculty?.Trim() ?? "N/A",
        //                        Subject = m.Subject?.Trim() ?? "N/A",
        //                        DateOfBirth = m.DateOfBirth.HasValue ? DateOnly.FromDateTime(m.DateOfBirth.Value) : default,
        //                        Mobile = m.Mobile?.Trim() ?? "N/A",
        //                        Email = m.Email?.Trim() ?? "N/A",
        //                        AadhaarNumber = m.Aadhaar?.Trim() ?? "N/A",
        //                        Pannumber = m.PAN?.Trim() ?? "N/A",
        //                        RnRmnumber = m.RN_RMNumber?.Trim() ?? "N/A",
        //                        Department = m.Department?.Trim() ?? "N/A",
        //                        DepartmentDetails = m.DepartmentDetails?.Trim() ?? "N/A",
        //                        Designation = m.Designation?.Trim() ?? "N/A",
        //                        Qualification = m.Qualification?.Trim() ?? "N/A",
        //                        UginstituteName = m.UGInstituteName?.Trim() ?? "N/A",
        //                        UgyearOfPassing = (int)m.UGYearOfPassing,
        //                        PginstituteName = m.PGInstituteName?.Trim() ?? "N/A",
        //                        PgyearOfPassing = (int)m.PGYearOfPassing,
        //                        PgpassingSpecialization = m.PGPassingSpecialization?.Trim() ?? "N/A",
        //                        TeachingExperienceAfterUgyears = m.TeachingExperienceAfterUGYears,
        //                        TeachingExperienceAfterPgyears = m.TeachingExperienceAfterPGYears,
        //                        DateOfJoining = m.DateOfJoining.HasValue ? DateOnly.FromDateTime(m.DateOfJoining.Value) : default,
        //                        RecognizedPgTeacher = m.RecognizedPgTeacher,
        //                        RecognizedPhDteacher = m.RecognizedPhDTeacher,
        //                        IsRecognizedPgguide = m.IsRecognizedPGGuide,
        //                        IsExaminer = m.IsExaminer,
        //                        ExaminerFor = m.ExaminerForList != null ? string.Join(",", m.ExaminerForList) : null,
        //                        LitigationPending = m.LitigationPending,
        //                        Nrtsnumber = m.NRTSNumber?.Trim() ?? "N/A",
        //                        Rguhstin = m.RGUHSTIN?.Trim() ?? "N/A",
        //                        RemoveRemarks = m.RemoveRemarks,
        //                        IsRemoved = !string.IsNullOrWhiteSpace(m.RemoveRemarks),

        //                        // Documents
        //                        AadhaarDocument = aadhaarDoc,
        //                        Pandocument = panDoc,
        //                        RnRmdocument = rnRmDoc,
        //                        Form16OrLast6MonthsStatement = form16Doc,
        //                        AppointmentOrderDocument = appointmentDoc,
        //                        GuideRecognitionDoc = guideDoc,
        //                        PhDrecognitionDoc = phdDoc,
        //                        LitigationDoc = litigationDoc,
        //                        OnlineTeachersDatabase = OnlineTeachersDatabase,
        //                        Madetorecruit = madetorecruit
        //                    };

        //                    _context.AffTeachingFacultyAllDetails.Add(faculty);
        //                }
        //            }

        //            _context.SaveChanges();
        //            transaction.Commit();

        //            TempData["Success"] = "Faculty records saved successfully.";
        //            return RedirectToAction("AFF_TeachingFacultyDetails");
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            TempData["Error"] = "Error saving faculty records: " + ex.Message;
        //            return View(model);
        //        }
        //    }
        //}


        private byte[] ConvertFileToBytes(IFormFile formFile)
        {
            using (var ms = new MemoryStream())
            {
                formFile.CopyTo(ms);
                return ms.ToArray();
            }
        }
        //public IActionResult ViewFacultyDocument(int id, string type, string mode = "view")
        //{
        //    var faculty = _context.FacultyDetails.FirstOrDefault(f => f.Id == id);
        //    if (faculty == null)
        //        return NotFound();

        //    byte[] fileBytes = null;
        //    string fileName = $"{type}_document.pdf";

        //    switch (type.ToLower())
        //    {
        //        case "pg":
        //            fileBytes = faculty.GuideRecognitionDoc;
        //            break;

        //        case "phd":
        //            fileBytes = faculty.PhDrecognitionDoc;
        //            break;

        //        case "litig":
        //            fileBytes = faculty.LitigationDoc;
        //            break;

        //        default:
        //            return BadRequest("Invalid document type.");
        //    }

        //    if (fileBytes == null)
        //        return NotFound("Document not uploaded.");

        //    if (mode == "download")
        //    {
        //        // 📥 FORCE DOWNLOAD
        //        return File(fileBytes, "application/octet-stream", fileName);
        //    }

        //    // 👀 VIEW IN BROWSER
        //    return File(fileBytes, "application/pdf");
        //}

        [HttpGet]
        public async Task<IActionResult> Aff_GovInstituteDetails()
        {
            var vm = new InstitutionDetailsViewModel();
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            vm.CollegeCode = collegeCode;
            vm.FacultyCode = facultyCode;

            if (!string.IsNullOrEmpty(collegeCode) && !string.IsNullOrEmpty(facultyCode))
            {
                var existingInstitution = await _context.AffInstitutionsDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

                if (existingInstitution != null)
                {
                    vm.TypeOfInstitution = existingInstitution.TypeOfInstitution;
                    // Prefer database value; fall back to session/claim if DB value missing
                    vm.NameOfInstitution = !string.IsNullOrWhiteSpace(existingInstitution.NameOfInstitution)
                        ? existingInstitution.NameOfInstitution
                        : (HttpContext.Session.GetString("CollegeName") ?? User?.FindFirst("CollegeName")?.Value ?? string.Empty);

                    vm.Address = existingInstitution.Address;
                    vm.VillageTownCity = existingInstitution.VillageTownCity;
                    vm.Taluk = existingInstitution.Taluk;
                    vm.District = existingInstitution.District;
                    vm.PinCode = existingInstitution.PinCode;
                    vm.MobileNumber = existingInstitution.MobileNumber;
                    vm.StdCode = existingInstitution.StdCode;
                    vm.Fax = existingInstitution.Fax;
                    vm.Website = existingInstitution.Website;
                    vm.SurveyNoPidNo = existingInstitution.SurveyNoPidNo;
                    vm.MinorityInstitute = existingInstitution.MinorityInstitute;
                    vm.AttachedToMedicalClg = existingInstitution.AttachedToMedicalClg;
                    vm.RuralInstitute = existingInstitution.RuralInstitute;

                    // YearOfEstablishment in DB may be stored as string (yyyy or yyyy-MM-dd). Preserve as-is.
                    vm.YearOfEstablishment = existingInstitution.YearOfEstablishment ?? string.Empty;

                    vm.EmailId = existingInstitution.EmailId;
                    vm.AltLandlineMobile = existingInstitution.AltLandlineMobile;
                    vm.AltEmailId = existingInstitution.AltEmailId;
                    vm.HeadOfInstitution = existingInstitution.HeadOfInstitution;
                    vm.HeadAddress = existingInstitution.HeadAddress;
                    vm.FinancingAuthority = existingInstitution.FinancingAuthority;
                    vm.StatusOfCollege = existingInstitution.StatusOfCollege;
                    vm.CourseApplied = existingInstitution.CourseApplied;
                }
                else
                {
                    // No DB record — use session/claim name if available
                    vm.NameOfInstitution = HttpContext.Session.GetString("CollegeName") ?? User?.FindFirst("CollegeName")?.Value ?? string.Empty;
                    vm.YearOfEstablishment = string.Empty;
                }
            }
            else
            {
                // Session missing — still try to populate name
                vm.NameOfInstitution = HttpContext.Session.GetString("CollegeName") ?? User?.FindFirst("CollegeName")?.Value ?? string.Empty;
                vm.YearOfEstablishment = string.Empty;
            }

            return View(vm);
        }

        // Replace the POST portion handling YearOfEstablishment validation and mapping with this excerpt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aff_GovInstituteDetails(InstitutionDetailsViewModel vm)
        {
            // Remove server-side controlled properties from ModelState
            ModelState.Remove(nameof(vm.CollegeCode));
            ModelState.Remove(nameof(vm.FacultyCode));
            ModelState.Remove(nameof(vm.DocumentFile));

            // Enforce codes from session
            vm.CollegeCode = HttpContext.Session.GetString("CollegeCode");
            vm.FacultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(vm.CollegeCode) || string.IsNullOrEmpty(vm.FacultyCode))
            {
                ModelState.AddModelError(string.Empty, "College / Faculty code not found in session.");
                return View(vm);
            }

            // YearOfEstablishment is now a string; require a non-empty value
            if (string.IsNullOrWhiteSpace(vm.YearOfEstablishment))
            {
                ModelState.AddModelError(nameof(vm.YearOfEstablishment), "Year of Establishment is required.");
                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Fetch existing record (if any)
            var existingInstitution = await _context.AffInstitutionsDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == vm.CollegeCode &&
                                          x.FacultyCode == vm.FacultyCode);

            // Handle document upload
            byte[] docBytes = null;
            string docName = null;
            string docContentType = null;

            if (vm.DocumentFile != null && vm.DocumentFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await vm.DocumentFile.CopyToAsync(ms);
                docBytes = ms.ToArray();
                docName = vm.DocumentFile.FileName;
                docContentType = vm.DocumentFile.ContentType;
            }

            // Use the string provided by the user (trim). If you want to normalize to yyyy, do additional parsing here.
            var yearOfEstablishment = vm.YearOfEstablishment?.Trim();

            if (existingInstitution != null)
            {
                // UPDATE
                existingInstitution.CollegeCode = vm.CollegeCode;
                existingInstitution.FacultyCode = vm.FacultyCode;
                existingInstitution.TypeOfInstitution = vm.TypeOfInstitution;
                existingInstitution.NameOfInstitution = vm.NameOfInstitution;
                existingInstitution.Address = vm.Address;
                existingInstitution.VillageTownCity = vm.VillageTownCity;
                existingInstitution.Taluk = vm.Taluk;
                existingInstitution.District = vm.District;
                existingInstitution.PinCode = vm.PinCode;
                existingInstitution.MobileNumber = vm.MobileNumber;
                existingInstitution.StdCode = vm.StdCode;
                existingInstitution.Fax = vm.Fax;
                existingInstitution.Website = vm.Website;
                existingInstitution.SurveyNoPidNo = vm.SurveyNoPidNo;
                existingInstitution.MinorityInstitute = vm.MinorityInstitute;
                existingInstitution.AttachedToMedicalClg = vm.AttachedToMedicalClg;
                existingInstitution.RuralInstitute = vm.RuralInstitute;
                existingInstitution.YearOfEstablishment = yearOfEstablishment;
                existingInstitution.EmailId = vm.EmailId;
                existingInstitution.AltLandlineMobile = vm.AltLandlineMobile;
                existingInstitution.AltEmailId = vm.AltEmailId;
                existingInstitution.HeadOfInstitution = vm.HeadOfInstitution;
                existingInstitution.HeadAddress = vm.HeadAddress;
                existingInstitution.FinancingAuthority = vm.FinancingAuthority;
                existingInstitution.StatusOfCollege = vm.StatusOfCollege;
                existingInstitution.CourseApplied = vm.CourseApplied;

                // Only overwrite document fields if a new file was uploaded
                if (docBytes != null)
                {
                    existingInstitution.DocumentName = docName;
                    existingInstitution.DocumentContentType = docContentType;
                    existingInstitution.DocumentData = docBytes;
                }

                await _context.SaveChangesAsync();
                var id = existingInstitution.InstitutionId;
                return RedirectToAction(nameof(AffCourseDetails), new { id });
            }
            else
            {
                // CREATE
                var entity = new AffInstitutionsDetail
                {
                    CollegeCode = vm.CollegeCode,
                    FacultyCode = vm.FacultyCode,
                    TypeOfInstitution = vm.TypeOfInstitution,
                    NameOfInstitution = vm.NameOfInstitution,
                    Address = vm.Address,
                    VillageTownCity = vm.VillageTownCity,
                    Taluk = vm.Taluk,
                    District = vm.District,
                    PinCode = vm.PinCode,
                    MobileNumber = vm.MobileNumber,
                    StdCode = vm.StdCode,
                    Fax = vm.Fax,
                    Website = vm.Website,
                    SurveyNoPidNo = vm.SurveyNoPidNo,
                    MinorityInstitute = vm.MinorityInstitute,
                    AttachedToMedicalClg = vm.AttachedToMedicalClg,
                    RuralInstitute = vm.RuralInstitute,
                    YearOfEstablishment = yearOfEstablishment,
                    EmailId = vm.EmailId,
                    AltLandlineMobile = vm.AltLandlineMobile,
                    AltEmailId = vm.AltEmailId,
                    HeadOfInstitution = vm.HeadOfInstitution,
                    HeadAddress = vm.HeadAddress,
                    FinancingAuthority = vm.FinancingAuthority,
                    StatusOfCollege = vm.StatusOfCollege,
                    CourseApplied = vm.CourseApplied,
                    DocumentName = docName,
                    DocumentContentType = docContentType,
                    DocumentData = docBytes
                };

                _context.AffInstitutionsDetails.Add(entity);
                await _context.SaveChangesAsync();

                // Use newly created Id
                var newId = entity.InstitutionId;
                return RedirectToAction(nameof(AffCourseDetails), new { id = newId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AffCourseDetails()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // 1) get all courses for this faculty from Mst_Course
            var mstCourses = await _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            // 2) get existing AFF_CourseDetails rows
            var affRows = await _context.AffCourseDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            var vm = new AffCourseDetailsListVM();

            foreach (var c in mstCourses)
            {
                var existing = affRows.FirstOrDefault(x => x.CourseId == c.CourseCode.ToString());

                vm.Courses.Add(new AffCourseDetailRowVM
                {
                    Id = existing?.Id ?? 0,
                    CollegeCode = collegeCode ?? string.Empty,
                    FacultyCode = facultyCode ?? string.Empty,
                    CourseId = c.CourseCode.ToString(),
                    CourseName = c.CourseName ?? string.Empty,

                    IsRecognized = existing?.IsRecognized ?? false,
                    RguhsNotificationNo = existing?.RguhsNotificationNo ?? string.Empty,
                    ExistingFileUrl = existing != null && existing.DocumentData != null
                        ? Url.Action("DownloadAffCourseFile", "Course", new { id = existing.Id })
                        : null,
                    CreatedOn = existing?.CreatedOn ?? DateTime.UtcNow,
                    CreatedBy = existing?.CreatedBy ?? string.Empty
                });
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AffCourseDetails(AffCourseDetailsListVM vm)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // Clear existing model state errors first
            ModelState.Clear();

            // Custom validation for conditional fields using correct index
            for (int i = 0; i < vm.Courses.Count; i++)
            {
                var row = vm.Courses[i];
                if (row.IsRecognized && string.IsNullOrWhiteSpace(row.RguhsNotificationNo))
                {
                    ModelState.AddModelError($"Courses[{i}].RguhsNotificationNo",
                        "RGUHS Notification No is required when course is recognized.");
                }
            }

            if (!ModelState.IsValid)
                return View(vm);

            foreach (var row in vm.Courses)
            {
                AffCourseDetail entity;

                if (row.Id > 0)
                {
                    entity = await _context.AffCourseDetails.FindAsync(row.Id);
                    if (entity == null) continue;
                }
                else
                {
                    entity = new AffCourseDetail
                    {
                        CollegeCode = collegeCode ?? string.Empty,
                        FacultyCode = facultyCode ?? string.Empty,
                        CourseId = row.CourseId,
                        CourseName = row.CourseName,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = User.Identity?.Name ?? string.Empty
                    };
                    _context.AffCourseDetails.Add(entity);
                }

                // Update core fields
                entity.CourseName = row.CourseName;
                entity.IsRecognized = row.IsRecognized;
                entity.RguhsNotificationNo = row.RguhsNotificationNo ?? string.Empty;

                // Handle file upload
                if (row.SupportingFile != null && row.SupportingFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await row.SupportingFile.CopyToAsync(ms);
                    entity.DocumentData = ms.ToArray();
                }

                // Update audit fields for existing records
                if (row.Id > 0)
                {
                    entity.CreatedBy = row.CreatedBy ?? string.Empty;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Course details saved successfully.";
            return RedirectToAction(nameof(Details_Of_MBBS));
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAffCourseFile(int id)
        {
            var courseDetail = await _context.AffCourseDetails
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (courseDetail == null || courseDetail.DocumentData == null || courseDetail.DocumentData.Length == 0)
            {
                TempData["Error"] = "Document not found.";
                return NotFound();
            }

            return File(courseDetail.DocumentData,
                        "application/octet-stream",  // Generic MIME type
                        $"RGUHS_Course_{courseDetail.CourseId}_{courseDetail.RguhsNotificationNo ?? "Doc"}.pdf");
        }

        public async Task<IActionResult> Details1(int id)
        {
            var entity = await _context.AffInstitutionsDetails.FindAsync(id);
            if (entity == null)
                return NotFound();

            return View(entity);
        }

        private void FillDropDowns(InstitutionViewModel vm)
        {
            vm.TalukList = _context.TalukMasters
                .OrderBy(t => t.TalukName)
                .Select(t => new SelectListItem
                {
                    Value = t.TalukId,
                    Text = t.TalukName
                }).ToList();

            vm.DistrictList = _context.DistrictMasters
                .OrderBy(d => d.DistrictName)
                .Select(d => new SelectListItem
                {
                    Value = d.DistrictName,
                    Text = d.DistrictName
                }).ToList();

            vm.CourseList = _context.MstMedicalCourseTypes
                .OrderBy(c => c.CourseTypeName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseTypeName,
                    Text = c.CourseTypeName
                }).ToList();

            vm.institutetypelist = _context.MstInstitutionTypes
               //.Where(e => e.FacultyId == 1)
               .OrderBy(c => c.OrganizationCategory)
               .Select(c => new SelectListItem
               {
                   Value = c.InstitutionTypeId.ToString(),
                   Text = c.InstitutionType
               }).ToList();

            vm.Institutestatuslist = _context.AffInstitutionStatusMasters
               .OrderBy(c => c.InstitutionStatusId)
               .Select(c => new SelectListItem
               {
                   Value = c.InstitutionStatusId.ToString(),
                   Text = c.StatusName
               }).ToList();

        }


        // ─────────────────────────────────────────────────────────────────────────────
        // GET: Institution/Institution_Details
        // ─────────────────────────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public IActionResult Institution_Details()
        {
            // 1. Resolve session values
            var courseLevel = CourseLevel;
            var facultyCode = User.FindFirst("FacultyCode")?.Value;
            var collegeCode = User.FindFirst("CollegeCode")?.Value;

            if (string.IsNullOrWhiteSpace(courseLevel))
                return RedirectToAction("Dashboard", "Collegelogin");

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return BadRequest("Session expired. FacultyCode / CollegeCode not found. Please login again.");

            // 2. Try to load existing record
            var entity = _context.AffInstitutionsDetails.FirstOrDefault(x => x.FacultyCode.Trim() == facultyCode.Trim() &&
                         x.CollegeCode.Trim() == collegeCode.Trim() && x.CourseLevel == courseLevel.Trim());

            // 3. Build view model
            InstitutionViewModel vm;

            if (entity == null)
            {
                // No record yet – show blank form
                vm = new InstitutionViewModel
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseLevel = courseLevel,
                };
            }
            else
            {
                vm = MapEntityToViewModel(entity);
            }

            FillDropDowns(vm);
            return View(vm);
        }


        // ─────────────────────────────────────────────────────────────────────────────
        // POST: Institution/Institution_Details
        // ─────────────────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 100000)]
        public IActionResult Institution_Details(InstitutionViewModel vm, IFormFile? documentFile)
        {
            // 1. Re-apply session codes (never trust hidden fields for security)
            var courseLevel = CourseLevel;
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please login again.");
                FillDropDowns(vm);
                return View(vm);
            }

            vm.FacultyCode = facultyCode;
            vm.CollegeCode = collegeCode;
            vm.CourseLevel = courseLevel;

            // 2. Remove non-user-input fields from validation
            ModelState.Remove(nameof(vm.FacultyCode));
            ModelState.Remove(nameof(vm.CollegeCode));
            ModelState.Remove(nameof(vm.TalukList));
            ModelState.Remove(nameof(vm.CourseList));
            ModelState.Remove(nameof(vm.DistrictList));
            ModelState.Remove(nameof(vm.institutetypelist));
            ModelState.Remove(nameof(vm.Institutestatuslist));
            ModelState.Remove(nameof(vm.DocumentName));
            ModelState.Remove(nameof(vm.DocumentContentType));  // ← fixes silent validation failure
                                                                // Uncomment the line below if DocumentData is a property on your VM:
                                                                // ModelState.Remove(nameof(vm.DocumentData));

            // 3. Return view with errors if validation fails
            if (!ModelState.IsValid)
            {
                vm.CourseLevel = courseLevel;
                FillDropDowns(vm);
                return View(vm);
            }

            // 4. Load existing entity or create new one
            var entity = _context.AffInstitutionsDetails
                .FirstOrDefault(x =>
                    x.FacultyCode == vm.FacultyCode &&
                    x.CollegeCode == vm.CollegeCode && x.CourseLevel == courseLevel);

            bool isNew = entity == null;

            if (isNew)
            {
                entity = new AffInstitutionsDetail
                {
                    FacultyCode = vm.FacultyCode,
                    CollegeCode = vm.CollegeCode,
                    CourseLevel = courseLevel,
                };
                _context.AffInstitutionsDetails.Add(entity);
            }

            // 5. Handle document upload
            //    Only overwrite the stored document when a new file is actually submitted.
            if (documentFile != null && documentFile.Length > 0)
            {
                using var ms = new MemoryStream();
                documentFile.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();

                if (fileBytes.Length > 0)
                {
                    entity.DocumentData = fileBytes;
                    entity.DocumentName = Path.GetFileName(documentFile.FileName); // strip path for safety
                    entity.DocumentContentType = documentFile.ContentType;
                }
            }
            // If no new file was uploaded, leave entity.DocumentData / DocumentName / DocumentContentType unchanged.

            // 6. Map all other ViewModel fields → Entity
            MapViewModelToEntity(vm, entity);

            // 7. Save with error handling
            try
            {
                _context.SaveChanges();
                // After saving Institution Details:
                ContinuousAffiliationController.MarkDone(HttpContext, "BasicDetails");
            }
            catch (DbUpdateException ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty, $"Database error: {innerMsg}");
                FillDropDowns(vm);
                return View(vm);
            }

            // 8. Success – redirect
            TempData["SuccessMessage"] = "Institution details saved successfully.";
            return RedirectToAction("aff_institutedetails");
        }


        private static InstitutionViewModel MapEntityToViewModel(AffInstitutionsDetail e)
        {
            return new InstitutionViewModel
            {
                CollegeCode = e.CollegeCode,
                FacultyCode = e.FacultyCode,
                CourseLevel = e.CourseLevel,
                TypeOfInstitution = e.TypeOfInstitution,
                NameOfInstitution = e.NameOfInstitution,
                Address = e.Address,
                VillageTownCity = e.VillageTownCity,
                Taluk = e.Taluk,
                District = e.District,
                PinCode = e.PinCode,
                MobileNumber = e.MobileNumber,
                StdCode = e.StdCode,
                Fax = e.Fax,
                Website = e.Website,
                SurveyNoPidNo = e.SurveyNoPidNo,
                MinorityInstitute = e.MinorityInstitute,
                AttachedToMedicalClg = e.AttachedToMedicalClg,
                RuralInstitute = e.RuralInstitute,
                YearOfEstablishment = e.YearOfEstablishment,
                EmailId = e.EmailId,
                AltLandlineMobile = e.AltLandlineMobile,
                AltEmailId = e.AltEmailId,
                HeadOfInstitution = e.HeadOfInstitution,
                HeadAddress = e.HeadAddress,
                FinancingAuthority = e.FinancingAuthority,
                StatusOfCollege = e.StatusOfCollege,
                CourseApplied = e.CourseApplied,
                DocumentName = e.DocumentName,
                DocumentContentType = e.DocumentContentType,
                NodalOfficer_Name = e.NodalOfficerName,
                NodalOfficer_Mob_Number = e.NodalOfficerMobNumber,
                NodalOfficer_Email = e.NodalOfficerEmail,
                Principal_Name = e.PrincipalName,
                Principal_Mob_No = e.PrincipalMobNo,
                Principal_Email = e.PrincipalEmail,
                HeadOfInstitution_Mob_NO = e.HeadOfInstitutionMobNo,
                HeadOfInstitution_Email = e.HeadOfInstitutionEmail,
                College_URL = e.CollegeUrl,
                TrustName = e.TrustName,
                TrustAddress = e.TrustAddress,
                TrustEstablishmentDate = e.TrustEstablishmentDate,
                TrustPresidentName = e.TrustPresidentName,
                TrustPresidentContactNo = e.TrustPresidentContactNo,
                DeanName = e.DeanName,
                DeanMobileNumber = e.DeanMobileNumber,
                DeanEmailId = e.DeanEmailId,
                PrincipalMobileNumber = e.PrincipalMobileNumber,
                PrincipalEmailId = e.PrincipalEmailId,
                MinorityCategory = e.MinorityCategory,
                RunningCourse = e.RunningCourse
            };
        }

        private static void MapViewModelToEntity(InstitutionViewModel vm, AffInstitutionsDetail e)
        {
            e.TypeOfInstitution = vm.TypeOfInstitution;
            e.NameOfInstitution = vm.NameOfInstitution;
            e.Address = vm.Address;
            e.VillageTownCity = vm.VillageTownCity;
            e.Taluk = vm.Taluk;
            e.District = vm.District;
            e.PinCode = vm.PinCode;
            e.MobileNumber = vm.MobileNumber;
            e.StdCode = vm.StdCode;
            e.Fax = vm.Fax;
            e.Website = vm.Website;
            e.SurveyNoPidNo = vm.SurveyNoPidNo;
            e.MinorityInstitute = vm.MinorityInstitute;
            e.AttachedToMedicalClg = vm.AttachedToMedicalClg;
            e.RuralInstitute = vm.RuralInstitute;
            e.YearOfEstablishment = vm.YearOfEstablishment;
            e.EmailId = vm.EmailId;
            e.AltLandlineMobile = vm.AltLandlineMobile;
            e.AltEmailId = vm.AltEmailId;
            e.HeadOfInstitution = vm.HeadOfInstitution;
            e.HeadAddress = vm.HeadAddress;
            e.FinancingAuthority = vm.FinancingAuthority;
            e.StatusOfCollege = vm.StatusOfCollege;
            e.CourseApplied = vm.CourseApplied;
            e.NodalOfficerName = vm.NodalOfficer_Name;
            e.NodalOfficerMobNumber = vm.NodalOfficer_Mob_Number;
            e.NodalOfficerEmail = vm.NodalOfficer_Email;
            e.PrincipalName = vm.Principal_Name;
            e.PrincipalMobNo = vm.Principal_Mob_No;
            e.PrincipalEmail = vm.Principal_Email;
            e.HeadOfInstitutionMobNo = vm.HeadOfInstitution_Mob_NO;
            e.HeadOfInstitutionEmail = vm.HeadOfInstitution_Email;
            e.CollegeUrl = vm.College_URL;
            e.TrustName = vm.TrustName;
            e.TrustAddress = vm.TrustAddress;
            e.TrustEstablishmentDate = vm.TrustEstablishmentDate;
            e.TrustPresidentName = vm.TrustPresidentName;
            e.TrustPresidentContactNo = vm.TrustPresidentContactNo;
            e.DeanName = vm.DeanName;
            e.DeanMobileNumber = vm.DeanMobileNumber;
            e.DeanEmailId = vm.DeanEmailId;
            e.PrincipalMobileNumber = vm.PrincipalMobileNumber;
            e.PrincipalEmailId = vm.PrincipalEmailId;
            e.MinorityCategory = vm.MinorityCategory;
            e.RunningCourse = vm.RunningCourse;
        }


        // GET: Load existing data
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        public async Task<IActionResult> Details_Of_MBBS()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            var entity = await _context.AffiliationCourseDetails
                .FirstOrDefaultAsync(x => x.Facultycode == facultyCode &&
                                          x.Collegecode == collegeCode &&
                                          x.CourseId == "MBBS");

            var model = new AffiliationCourseDetailsViewModel
            {
                Facultycode = facultyCode,
                Collegecode = collegeCode,
                CourseId = "MBBS",
                courseName = "MBBS",

                IntakeDuring_2025_26 = entity?.IntakeDuring202526 ?? "",
                Intake_slab = entity?.IntakeSlab ?? "",
                Typeofpermission = entity?.Typeofpermission ?? "",
                yearofLOP = entity?.YearofLop,
                dateofrecognition = entity?.Dateofrecognition ?? "",
                yearofObtainingECandFC = entity?.YearofObtainingEcandFc,
                sannctionedIntake_EC_FC = entity?.SannctionedIntakeEcFc ?? "",

                // New fields
                SanctionedIntake_Permission = entity?.SanctionedIntakePermission ?? "",
                DateOfLOP_Renewal_GOIMCI = entity?.DateOfLoprenewalGoimci ?? "",
                YearOfLastAffiliation_RGUHS = entity?.YearOfLastAffiliationRguhs ?? "",
                SanctionedIntake_LastAffiliation = entity?.SanctionedIntakeLastAffiliation ?? "",
                DateOfPreviousLICInspection = entity?.DateOfPreviousLicinspection,
                ActionTakenOnDeficiencies = entity?.ActionTakenOnDeficiencies ?? "",

                HasGOKOrder = entity?.Gokorder != null && entity.Gokorder.Length > 0,
                HasLastAffiliationFile = entity?.LastAffiliationRguhsfile != null && entity.LastAffiliationRguhsfile.Length > 0
            };

            return View(model);
        }

        // POST: Save / Update
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details_Of_MBBS(AffiliationCourseDetailsViewModel model)
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            if (ModelState.IsValid)
            {

                model.Facultycode = facultyCode;
                model.Collegecode = collegeCode;

                var existingEntity = await _context.AffiliationCourseDetails
                    .FirstOrDefaultAsync(x => x.Facultycode == model.Facultycode &&
                                              x.Collegecode == model.Collegecode &&
                                              x.CourseId == model.CourseId);

                var entity = existingEntity ?? new AffiliationCourseDetail();

                // Map fields
                entity.Facultycode = model.Facultycode;
                entity.Collegecode = model.Collegecode;
                entity.CourseId = model.CourseId;
                entity.CourseName = model.courseName;
                entity.IntakeDuring202526 = model.IntakeDuring_2025_26;
                entity.IntakeSlab = model.Intake_slab;
                entity.Typeofpermission = model.Typeofpermission;
                entity.YearofLop = model.yearofLOP;
                entity.Dateofrecognition = model.dateofrecognition;
                entity.YearofObtainingEcandFc = model.yearofObtainingECandFC;
                entity.SannctionedIntakeEcFc = model.sannctionedIntake_EC_FC;

                // New fields
                entity.SanctionedIntakePermission = model.SanctionedIntake_Permission;
                entity.DateOfLoprenewalGoimci = model.DateOfLOP_Renewal_GOIMCI;
                entity.YearOfLastAffiliationRguhs = model.YearOfLastAffiliation_RGUHS;
                entity.SanctionedIntakeLastAffiliation = model.SanctionedIntake_LastAffiliation;
                entity.DateOfPreviousLicinspection = model.DateOfPreviousLICInspection;
                entity.ActionTakenOnDeficiencies = model.ActionTakenOnDeficiencies;

                // File: GOK Order (EC & FC)
                if (model.GOKorder != null && model.GOKorder.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.GOKorder.CopyToAsync(ms);
                    entity.Gokorder = ms.ToArray();
                }

                // File: Last Affiliation by RGUHS
                if (model.LastAffiliationRGUHSFile != null && model.LastAffiliationRGUHSFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.LastAffiliationRGUHSFile.CopyToAsync(ms);
                    entity.LastAffiliationRguhsfile = ms.ToArray();
                }

                if (existingEntity == null)
                {
                    _context.AffiliationCourseDetails.Add(entity);
                }

                await _context.SaveChangesAsync();

                //return RedirectToAction("Med_CA_AccountAndFeeDetails", "Aff_CA_Med_FinanceDetails");
                return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased");

                var courseLevel = HttpContext.Session.GetString("CourseLevel");
                if (courseLevel == "UG")
                {
                    return RedirectToAction("Details_Of_MBBS", "ContinuesAffiliation_Facultybased");
                }
                else if (courseLevel == "PG")
                {
                    return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased");
                }
                else if (courseLevel == "SS")
                {
                    return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased_SS");
                }
            }

            // Repopulate on validation error
            model.Facultycode = facultyCode;
            model.Collegecode = collegeCode;
            return View(model);
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public IActionResult Dean_DirectorDetails()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            var courseLevel = CourseLevel;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            if (string.IsNullOrWhiteSpace(courseLevel))
                return RedirectToAction("Dashboard", "Collegelogin", new { collegecode = collegeCode });



            var vm = new DeanDetailsViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseLevel = courseLevel,
            };

            // 🔽 Qualifications dropdown
            ViewBag.Qualifications = new SelectList(
                _context.MstCourses
                    .Where(c => !string.IsNullOrEmpty(c.CourseName)
                             && c.FacultyCode.ToString() == facultyCode)
                    .OrderBy(c => c.CourseName)
                    .Select(c => new { c.Id, c.CourseName }),
                "Id",
                "CourseName"
            );

            // 🔍 Fetch existing dean/director record
            var dean = _context.AffDeanOrDirectorDetails
                .FirstOrDefault(d => d.FacultyCode == facultyCode
                                  && d.CollegeCode == collegeCode && d.CourseLevel == courseLevel);

            if (dean != null)
            {
                // 🧾 Populate main fields
                vm.DeanOrDirectorName = dean.DeanOrDirectorName;
                vm.DeanQualification = dean.DeanQualification;
                vm.DeanQualificationDate = dean.DeanQualificationDate;
                vm.DeanUniversity = dean.DeanUniversity;
                vm.DeanStateCouncilNumber = dean.DeanStateCouncilNumber;
                vm.RecognizedByMCI = (bool)(dean.RecognizedByMci);

                // 📘 Teaching Experience
                vm.TeachingExperiences = _context.AffDeanTeachingExperiences
                    .Where(t => t.DeanId == dean.Id)
                    .Select(t => new TeachingExperienceRow
                    {
                        Designation = t.Designation,
                        UGFrom = t.Ugfrom,
                        UGTo = t.Ugto,
                        PGFrom = t.Pgfrom,
                        PGTo = t.Pgto,
                        TotalExperienceYears = t.TotalExperienceYears
                    })
                    .ToList();

                // 🏛 Administrative Experience
                vm.AdministrativeExperiences = _context.AffDeanAdministrativeExperiences
                    .Where(a => a.DeanId == dean.Id)
                    .Select(a => new AdministrativeExperienceRow
                    {
                        PostHeld = a.PostHeld,
                        FromDate = a.FromDate,
                        ToDate = a.ToDate,
                        TotalExperienceYears = a.TotalExperienceYears
                    })
                    .ToList();
            }

            // 🧱 If no teaching rows → add defaults
            if (!vm.TeachingExperiences.Any())
            {
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "JR (If Applicable)" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "SR (If Applicable)" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "Assistant Professor" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "Associate Professor" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "Professor" });
            }

            // 🧱 If no admin rows → add one empty row
            if (!vm.AdministrativeExperiences.Any())
            {
                vm.AdministrativeExperiences.Add(new AdministrativeExperienceRow());
            }

            return View(vm);
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Dean_DirectorDetails(DeanDetailsViewModel model)
        {
            // 🔐 Always fetch from session
            var courseLevel = CourseLevel;
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            if (string.IsNullOrWhiteSpace(courseLevel))
                return RedirectToAction("Dashboard", "Collegelogin", new { collegecode = collegeCode });

            model.FacultyCode = facultyCode;
            model.CollegeCode = collegeCode;
            model.CourseLevel = courseLevel;

            // Remove unused validation
            ModelState.Remove(nameof(model.UGYears));
            ModelState.Remove("AdministrativeExperiences[0].FacultyCode");
            ModelState.Remove("AdministrativeExperiences[0].CollegeCode");
            ModelState.Remove("TeachingExperiences[0].FacultyCode");
            ModelState.Remove("TeachingExperiences[0].CollegeCode");


            if (!ModelState.IsValid)
            {
                ViewBag.Qualifications = new SelectList(
                    _context.MstCourses
                        .Where(c => !string.IsNullOrEmpty(c.CourseName)
                            && c.FacultyCode.ToString().Trim() == facultyCode.Trim()
                        )
                        .OrderBy(c => c.CourseName)
                        .Select(c => new { c.Id, c.CourseName }),
                    "Id",
                    "CourseName",
                    model.DeanQualification
                );

                return View(model);
            }

            try
            {
                // 🔍 Check existing dean/director
                var existingDean = _context.AffDeanOrDirectorDetails
                    .FirstOrDefault(d => d.FacultyCode == facultyCode &&
                                         d.CollegeCode == collegeCode && d.CourseLevel == courseLevel);

                if (existingDean == null)
                {
                    // ➕ Insert
                    existingDean = new AffDeanOrDirectorDetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel,
                        DeanOrDirectorName = model.DeanOrDirectorName,
                        DeanQualification = model.DeanQualification,
                        DeanQualificationDate = model.DeanQualificationDate,
                        DeanUniversity = model.DeanUniversity,
                        DeanStateCouncilNumber = model.DeanStateCouncilNumber,
                        RecognizedByMci = model.RecognizedByMCI
                    };

                    _context.AffDeanOrDirectorDetails.Add(existingDean);
                    _context.SaveChanges(); // 🔥 REQUIRED to generate Id
                }
                else
                {
                    // ✏ Update
                    existingDean.DeanOrDirectorName = model.DeanOrDirectorName;
                    existingDean.DeanQualification = model.DeanQualification;
                    existingDean.DeanQualificationDate = model.DeanQualificationDate;
                    existingDean.DeanUniversity = model.DeanUniversity;
                    existingDean.DeanStateCouncilNumber = model.DeanStateCouncilNumber;
                    existingDean.RecognizedByMci = model.RecognizedByMCI;
                }

                // 🧹 Remove old child records
                _context.AffDeanTeachingExperiences.RemoveRange(
                    _context.AffDeanTeachingExperiences
                        .Where(t => t.DeanId == existingDean.Id));

                _context.AffDeanAdministrativeExperiences.RemoveRange(
                    _context.AffDeanAdministrativeExperiences
                        .Where(a => a.DeanId == existingDean.Id));

                // 📘 Teaching Experience
                if (model.TeachingExperiences != null)
                {
                    foreach (var t in model.TeachingExperiences)
                    {
                        if (string.IsNullOrWhiteSpace(t.Designation) &&
                            t.UGFrom == null && t.UGTo == null &&
                            t.PGFrom == null && t.PGTo == null &&
                            t.TotalExperienceYears == null)
                            continue;

                        _context.AffDeanTeachingExperiences.Add(
                            new AffDeanTeachingExperience
                            {
                                DeanId = existingDean.Id,
                                Facultycode = facultyCode,
                                Collegecode = collegeCode,
                                CourseLevel = courseLevel,
                                Designation = t.Designation?.Trim(),
                                Ugfrom = t.UGFrom,
                                Ugto = t.UGTo,
                                Pgfrom = t.PGFrom,
                                Pgto = t.PGTo,
                                TotalExperienceYears = t.TotalExperienceYears ?? 0
                            });
                    }
                }

                // 🏛 Administrative Experience
                if (model.AdministrativeExperiences != null)
                {
                    foreach (var a in model.AdministrativeExperiences)
                    {
                        if (string.IsNullOrWhiteSpace(a.PostHeld) &&
                            a.FromDate == null && a.ToDate == null &&
                            a.TotalExperienceYears == null)
                            continue;

                        _context.AffDeanAdministrativeExperiences.Add(
                            new AffDeanAdministrativeExperience
                            {
                                DeanId = existingDean.Id,
                                Facultycode = facultyCode,
                                Collegecode = collegeCode,
                                CourseLevel = courseLevel,
                                PostHeld = a.PostHeld?.Trim(),
                                FromDate = a.FromDate,
                                ToDate = a.ToDate,
                                TotalExperienceYears = a.TotalExperienceYears ?? 0
                            });
                    }
                }

                // 💾 Final commit
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Dean / Director details saved successfully.";
                return RedirectToAction("Aff_PrincipalDetails");
            }
            catch (Exception ex)
            {
                // TODO: log ex (ILogger)
                ModelState.AddModelError("", "An error occurred while saving data.");

                ViewBag.Qualifications = new SelectList(
                    _context.MstCourses
                        .Where(c => !string.IsNullOrEmpty(c.CourseName))
                        .OrderBy(c => c.CourseName)
                        .Select(c => new { c.Id, c.CourseName }),
                    "Id",
                    "CourseName",
                    model.DeanQualification
                );

                return View(model);
            }
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public IActionResult Aff_PrincipalDetails()
        {
            var courseLevel = CourseLevel;

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            //if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
            //    return RedirectToAction("ClgLogin");

            if (string.IsNullOrWhiteSpace(courseLevel))
                return RedirectToAction("Dashboard", "Collegelogin", new { collegecode = collegeCode });

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(courseLevel))
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = new DeanDetailsViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseLevel = courseLevel,
            };

            // 🔽 Qualifications dropdown
            ViewBag.Qualifications = new SelectList(
                _context.MstCourses
                    .Where(c => !string.IsNullOrEmpty(c.CourseName)
                             && c.FacultyCode.ToString() == facultyCode)
                    .OrderBy(c => c.CourseName)
                    .Select(c => new { c.Id, c.CourseName }),
                "Id",
                "CourseName"
            );

            // 🔍 Fetch existing Dean/Principal
            var dean = _context.AffPrincipalDetails
                .FirstOrDefault(x => x.FacultyCode == facultyCode
                                  && x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);

            if (dean != null)
            {
                // ✅ Populate main fields
                vm.DeanOrDirectorName = dean.DeanOrDirectorName;
                vm.DeanQualification = dean.DeanQualification;
                vm.DeanQualificationDate = dean.DeanQualificationDate;
                vm.DeanUniversity = dean.DeanUniversity;
                vm.DeanStateCouncilNumber = dean.DeanStateCouncilNumber;
                vm.RecognizedByMCI = (bool)(dean.RecognizedByMci);

                // 📘 Teaching Experience
                var teachingList = _context.AffPrincipalTeachingExperiences
                    .Where(t => t.DeanId == dean.Id)
                    .OrderBy(t => t.Id)
                    .ToList();

                foreach (var t in teachingList)
                {
                    vm.TeachingExperiences.Add(new TeachingExperienceRow
                    {
                        Designation = t.Designation,
                        UGFrom = t.Ugfrom,
                        UGTo = t.Ugto,
                        PGFrom = t.Pgfrom,
                        PGTo = t.Pgto,
                        TotalExperienceYears = t.TotalExperienceYears
                    });
                }

                // 🏛 Administrative Experience
                var adminList = _context.AffPrincipalAdministrativeExperiences
                    .Where(a => a.DeanId == dean.Id)
                    .OrderBy(a => a.Id)
                    .ToList();

                foreach (var a in adminList)
                {
                    vm.AdministrativeExperiences.Add(new AdministrativeExperienceRow
                    {
                        PostHeld = a.PostHeld,
                        FromDate = a.FromDate,
                        ToDate = a.ToDate,
                        TotalExperienceYears = a.TotalExperienceYears
                    });
                }
            }

            // ➕ Default Teaching rows if no data exists
            if (!vm.TeachingExperiences.Any())
            {
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "JR (If Applicable)" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "SR (If Applicable)" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "Assistant Professor" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "Associate Professor" });
                vm.TeachingExperiences.Add(new TeachingExperienceRow { Designation = "Professor" });
            }

            // ➕ Default Admin row if none exists
            if (!vm.AdministrativeExperiences.Any())
            {
                vm.AdministrativeExperiences.Add(new AdministrativeExperienceRow());
            }

            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Aff_PrincipalDetails(DeanDetailsViewModel model)
        {
            var courseLevel = CourseLevel;
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("ClgLogin");

            if (string.IsNullOrWhiteSpace(courseLevel))
                return RedirectToAction("Dashboard", "Collegelogin", new { collegecode = collegeCode });

            model.FacultyCode = facultyCode;
            model.CollegeCode = collegeCode;
            model.CourseLevel = courseLevel;

            ModelState.Remove(nameof(model.UGYears));
            ModelState.Remove("AdministrativeExperiences");
            ModelState.Remove("TeachingExperiences");
            ModelState.Remove("AdministrativeExperiences[0].FacultyCode");
            ModelState.Remove("AdministrativeExperiences[0].CollegeCode");
            ModelState.Remove("TeachingExperiences[0].FacultyCode");
            ModelState.Remove("TeachingExperiences[0].CollegeCode");

            if (!ModelState.IsValid)
            {
                LoadQualifications(model.DeanQualification);
                return View(model);
            }

            try
            {
                var existingDean = _context.AffPrincipalDetails
                    .FirstOrDefault(d =>
                        d.FacultyCode == facultyCode &&
                        d.CollegeCode == collegeCode &&
                        d.CourseLevel == courseLevel);

                if (existingDean == null)
                {
                    existingDean = new AffPrincipalDetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel
                    };

                    _context.AffPrincipalDetails.Add(existingDean);
                }

                existingDean.DeanOrDirectorName = model.DeanOrDirectorName;
                existingDean.DeanQualification = model.DeanQualification;
                existingDean.DeanQualificationDate = model.DeanQualificationDate;
                existingDean.DeanUniversity = model.DeanUniversity;
                existingDean.DeanStateCouncilNumber = model.DeanStateCouncilNumber;
                existingDean.RecognizedByMci = model.RecognizedByMCI;

                _context.SaveChanges();

                _context.AffPrincipalTeachingExperiences.RemoveRange(
                    _context.AffPrincipalTeachingExperiences.Where(t => t.DeanId == existingDean.Id));

                _context.AffPrincipalAdministrativeExperiences.RemoveRange(
                    _context.AffPrincipalAdministrativeExperiences.Where(a => a.DeanId == existingDean.Id));

                if (model.TeachingExperiences != null)
                {
                    foreach (var t in model.TeachingExperiences)
                    {
                        if (string.IsNullOrWhiteSpace(t.Designation) &&
                            t.UGFrom == null && t.UGTo == null &&
                            t.PGFrom == null && t.PGTo == null &&
                            t.TotalExperienceYears == null)
                            continue;

                        _context.AffPrincipalTeachingExperiences.Add(new AffPrincipalTeachingExperience
                        {
                            DeanId = existingDean.Id,
                            Facultycode = facultyCode,
                            Collegecode = collegeCode,
                            CourseLevel = courseLevel,
                            Designation = t.Designation?.Trim(),
                            Ugfrom = t.UGFrom,
                            Ugto = t.UGTo,
                            Pgfrom = t.PGFrom,
                            Pgto = t.PGTo,
                            TotalExperienceYears = t.TotalExperienceYears ?? 0
                        });
                    }
                }

                if (model.AdministrativeExperiences != null)
                {
                    foreach (var a in model.AdministrativeExperiences)
                    {
                        if (string.IsNullOrWhiteSpace(a.PostHeld) &&
                            a.FromDate == null && a.ToDate == null &&
                            a.TotalExperienceYears == null)
                            continue;

                        _context.AffPrincipalAdministrativeExperiences.Add(new AffPrincipalAdministrativeExperience
                        {
                            DeanId = existingDean.Id,
                            Facultycode = facultyCode,
                            Collegecode = collegeCode,
                            CourseLevel = courseLevel,
                            PostHeld = a.PostHeld?.Trim(),
                            FromDate = a.FromDate,
                            ToDate = a.ToDate,
                            TotalExperienceYears = a.TotalExperienceYears ?? 0
                        });
                    }
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "principal details saved successfully.";

                // 🔥 UPDATED REDIRECT LOGIC
                if (courseLevel == "UG")
                {
                    return RedirectToAction("Medical_LandBuildingdetails", "Medical_ContinuousAffiliation");
                }
                else if (courseLevel == "PG")
                {
                    return RedirectToAction("PgCourses", "AffiliationPgCourse");
                }
                else if (courseLevel == "SS")
                {
                    return RedirectToAction("CoursesOffered", "AffiliationSS");
                }

                throw new Exception("Invalid CourseLevel");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving data.");
                LoadQualifications(model.DeanQualification);
                return View(model);
            }
        }

        private void LoadQualifications(object selectedValue = null)
        {
            ViewBag.Qualifications = new SelectList(
                _context.MstCourses
                    .Where(c => !string.IsNullOrEmpty(c.CourseName))
                    .OrderBy(c => c.CourseName)
                    .Select(c => new { c.Id, c.CourseName }),
                "Id",
                "CourseName",
                selectedValue
            );
        }


        [HttpGet]
        public async Task<IActionResult> TeachingFacultyDetails()
        {
            var courseLevel = CourseLevel;

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

            return RedirectToAction("NonTeachingStaff");

        }


        [HttpGet]
        public IActionResult NonTeachingStaff()
        {
            var model = new NonTeachingStaffVm();

            // Get session values
            model.CollegeCode = HttpContext.Session.GetString("CollegeCode");
            model.FacultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(model.CollegeCode) || string.IsNullOrEmpty(model.FacultyCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login");
            }

            // Load Designation dropdown
            model.DesignationList = _context.DesignationMasters
                .Select(d => new SelectListItem
                {
                    Value = d.DesignationCode,
                    Text = d.DesignationName
                })
                .ToList();

            // Load existing staff
            model.StaffList = _context.AffNonTeachingStaffs
                .Where(s => s.CollegeCode == model.CollegeCode
                         && s.FacultyCode == model.FacultyCode)
                .Select(s => new NonTeachingStaffItemVm
                {
                    Name = s.StaffName,
                    Designation = s.Designation,
                    MobileNumber = s.MobileNumber,
                    SalaryPaid = s.SalaryPaid,
                    PfProvided = s.PfProvided,
                    EsiProvided = s.EsiProvided,
                    ServiceRegisterMaintained = s.ServiceRegisterMaintained,
                    SalaryAcquaintanceRegister = s.SalaryAcquaintanceRegister
                })
                .ToList();

            // Ensure at least one row
            if (!model.StaffList.Any())
            {
                model.StaffList.Add(new NonTeachingStaffItemVm());
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NonTeachingStaff(NonTeachingStaffVm model)
        {
            // Force session values
            model.CollegeCode = HttpContext.Session.GetString("CollegeCode");
            model.FacultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(model.CollegeCode) || string.IsNullOrEmpty(model.FacultyCode))
            {
                ModelState.AddModelError("", "Session expired. Please login again.");
            }

            if (!ModelState.IsValid)
            {
                // Reload dropdown
                model.DesignationList = _context.DesignationMasters
                    .Select(d => new SelectListItem
                    {
                        Value = d.DesignationCode,
                        Text = d.DesignationName
                    }).ToList();

                return View(model);
            }

            // Remove existing staff for this college + faculty
            var existingRecords = _context.AffNonTeachingStaffs
                .Where(s => s.CollegeCode == model.CollegeCode
                         && s.FacultyCode == model.FacultyCode);

            _context.AffNonTeachingStaffs.RemoveRange(existingRecords);

            // Insert fresh records
            foreach (var staff in model.StaffList)
            {
                // Skip empty rows
                if (string.IsNullOrWhiteSpace(staff.Name))
                    continue;

                var entity = new AffNonTeachingStaff
                {
                    CollegeCode = model.CollegeCode,
                    FacultyCode = model.FacultyCode,
                    StaffName = staff.Name,
                    Designation = staff.Designation,
                    MobileNumber = staff.MobileNumber,
                    SalaryPaid = (decimal)(staff.SalaryPaid),
                    PfProvided = staff.PfProvided ?? false,
                    EsiProvided = staff.EsiProvided ?? false,
                    ServiceRegisterMaintained = staff.ServiceRegisterMaintained ?? false,
                    SalaryAcquaintanceRegister = staff.SalaryAcquaintanceRegister ?? false
                };

                _context.AffNonTeachingStaffs.Add(entity);
            }

            _context.SaveChanges();

            TempData["Success"] = "Non-Teaching staff details saved successfully.";

            //return RedirectToAction("nonteachingstaff");
            return RedirectToAction("Medical_LandBuildingdetails", "Medical_ContinuousAffiliation");
        }

        [HttpGet]
        public IActionResult aff_FacultyDetails()
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
                .ToList();

            var facultyDetails = _context.FacultyDetails
                                 .Where(f => f.CollegeCode == collegeCode
                                             && f.FacultyCode == facultyCode
                                             && f.IsRemoved != true)
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
                          //Subject = f1.Subject,
                          RecognizedPGTeacher = f1.RecognizedPgTeacher,
                          Mobile = f1.Mobile,
                          Email = f1.Email,
                          Subjects = subjectsList,
                          Designations = designationsList,
                          DepartmentDetails = departmentsList,
                          RecognizedPhDTeacher = f1.RecognizedPhDteacher,
                          LitigationPending = f1.LitigationPending,
                          PhDRecognitionDocData = f1.PhDrecognitionDoc,
                          LitigationDocData = f1.LitigationDoc,
                          PGRecognitionDocData = f1.GuideRecognitionDoc,
                          IsExaminer = f1.IsExaminer,
                          ExaminerFor = f1.ExaminerFor,
                          ExaminerForList = !string.IsNullOrEmpty(f1.ExaminerFor)
                                              ? f1.ExaminerFor.Split(',').ToList()
                                              : new List<string>(),
                          // ⭐ ADD THIS
                          RemoveRemarks = f1.RemoveRemarks

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult aff_FacultyDetails(IList<FacultyDetailsViewModel> model)
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
                    //var toDelete = existingFaculty
                    //    .Where(f => !incomingIds.Contains(f.Id))
                    //    .ToList();

                    //if (toDelete.Any())
                    //{
                    //    _context.FacultyDetails.RemoveRange(toDelete);
                    //}

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
                        string recognizedPhD = m.RecognizedPhDTeacher?.Trim() ?? "N/A";
                        string litigation = m.LitigationPending?.Trim() ?? "N/A";

                        byte[] guideDocBytes = null;
                        byte[] phdDocBytes = null;
                        byte[] litigDocBytes = null;

                        if (m.GuideRecognitionDoc != null)
                            guideDocBytes = ConvertFileToBytes(m.GuideRecognitionDoc);

                        if (m.PhDRecognitionDoc != null)
                            phdDocBytes = ConvertFileToBytes(m.PhDRecognitionDoc);

                        if (m.LitigationDoc != null)
                            litigDocBytes = ConvertFileToBytes(m.LitigationDoc);


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
                            //existing.Subject = subject;
                            existing.RecognizedPhDteacher = recognizedPhD;
                            existing.LitigationPending = litigation;

                            // Only replace files if new uploads exist
                            if (guideDocBytes != null)
                                existing.GuideRecognitionDoc = guideDocBytes;

                            if (phdDocBytes != null)
                                existing.PhDrecognitionDoc = phdDocBytes;

                            if (litigDocBytes != null)
                                existing.LitigationDoc = litigDocBytes;

                            existing.IsExaminer = m.IsExaminer;
                            existing.ExaminerFor = m.ExaminerForList != null
                                                    ? string.Join(",", m.ExaminerForList)
                                                    : null;
                            if (!string.IsNullOrWhiteSpace(m.RemoveRemarks))
                            {
                                existing.IsRemoved = true;
                                existing.RemoveRemarks = m.RemoveRemarks;
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
                                //Subject = subject,
                                Designation = designation,
                                RecognizedPgTeacher = recognizedPG,
                                RecognizedPhDteacher = recognizedPhD,
                                LitigationPending = litigation,
                                Mobile = mobile,
                                Email = email,
                                Pan = pan,
                                Aadhaar = aadhaar,
                                DepartmentDetails = dept,
                                GuideRecognitionDoc = guideDocBytes,
                                PhDrecognitionDoc = phdDocBytes,
                                LitigationDoc = litigDocBytes,
                                IsExaminer = m.IsExaminer,
                                ExaminerFor = m.ExaminerForList != null
                                            ? string.Join(",", m.ExaminerForList)
                                            : null,
                                RemoveRemarks = m.RemoveRemarks,
                            };

                            _context.FacultyDetails.Add(faculty);
                        }
                    }

                    // 🔹 3. Save all changes once
                    _context.SaveChanges();

                    transaction.Commit();

                    //TempData["Success"] = "Faculty records saved successfully.";
                    return RedirectToAction("TeachingFacultyDetails");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

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


        public IActionResult ViewFacultyDocument(int id, string type, string mode = "view")
        {
            var faculty = _context.FacultyDetails.FirstOrDefault(f => f.Id == id);
            if (faculty == null)
                return NotFound();

            byte[] fileBytes = null;
            string fileName = $"{type}_document.pdf";

            switch (type.ToLower())
            {
                case "pg":
                    fileBytes = faculty.GuideRecognitionDoc;
                    break;

                case "phd":
                    fileBytes = faculty.PhDrecognitionDoc;
                    break;

                case "litig":
                    fileBytes = faculty.LitigationDoc;
                    break;

                default:
                    return BadRequest("Invalid document type.");
            }

            if (fileBytes == null)
                return NotFound("Document not uploaded.");

            if (mode == "download")
            {
                // 📥 FORCE DOWNLOAD
                return File(fileBytes, "application/octet-stream", fileName);
            }

            // 👀 VIEW IN BROWSER
            return File(fileBytes, "application/pdf");
        }


        //code added by DP 06022026


        [HttpGet]
        public async Task<IActionResult> TeachingStaffDepartmentWise()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = CourseLevel;

            int.TryParse(facultyCode, out int facultyCodeInt);

            var vm = new TeachingStaffDepartmentWiseVm
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                CourseLevel = courseLevel
            };

            // 🔽 All departments of faculty
            var departments = await _context.DepartmentMasters
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // 🔽 All designations of faculty
            var designations = await _context.DesignationMasters
                .Where(d => d.FacultyCode == facultyCodeInt)
                .OrderBy(d => d.DesignationOrder)
                .ToListAsync();

            // 🔽 Existing saved data
            var saved = await _context.TeachingStaffDepartmentWiseDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel)
                .ToListAsync();

            foreach (var dept in departments)
            {
                var deptVm = new DepartmentTeachingStaffVm
                {
                    DepartmentCode = dept.DepartmentCode,
                    DepartmentName = dept.DepartmentName
                };

                foreach (var des in designations)
                {
                    var existing = saved.FirstOrDefault(x =>
                        x.DepartmentCode == dept.DepartmentCode &&
                        x.DesignationCode == des.DesignationCode);

                    deptVm.Rows.Add(new TeachingStaffDepartmentWiseRow
                    {
                        Id = existing?.Id ?? 0,
                        DesignationCode = des.DesignationCode,
                        DesignationName = des.DesignationName,
                        UGFrom = existing?.Ugfrom?.ToDateTime(TimeOnly.MinValue),
                        UGTo = existing?.Ugto?.ToDateTime(TimeOnly.MinValue),
                        PGFrom = existing?.Pgfrom?.ToDateTime(TimeOnly.MinValue),
                        PGTo = existing?.Pgto?.ToDateTime(TimeOnly.MinValue),

                        TotalExperience = existing?.TotalExperience
                    });
                }

                vm.Departments.Add(deptVm);
            }

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 500000)]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> TeachingStaffDepartmentWise(TeachingStaffDepartmentWiseVm vm)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = CourseLevel;


            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            // 🔴 NULL CHECK (VERY IMPORTANT)
            if (vm?.Departments == null || !vm.Departments.Any())
                return RedirectToAction("TeachingStaffDepartmentWise");

            foreach (var dept in vm.Departments)
            {
                if (dept?.Rows == null) continue;

                foreach (var row in dept.Rows)
                {
                    bool hasUG = row.UGFrom.HasValue && row.UGTo.HasValue;
                    bool hasPG = row.PGFrom.HasValue && row.PGTo.HasValue;

                    if (!hasUG && !hasPG)
                        continue; // ⛔ skip empty rows

                    var existing = await _context.TeachingStaffDepartmentWiseDetails
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel &&
                            x.DepartmentCode == dept.DepartmentCode &&
                            x.DesignationCode == row.DesignationCode);

                    if (existing != null)
                    {
                        existing.Ugfrom = hasUG ? DateOnly.FromDateTime(row.UGFrom!.Value) : null;
                        existing.Ugto = hasUG ? DateOnly.FromDateTime(row.UGTo!.Value) : null;
                        existing.Pgfrom = hasPG ? DateOnly.FromDateTime(row.PGFrom!.Value) : null;
                        existing.Pgto = hasPG ? DateOnly.FromDateTime(row.PGTo!.Value) : null;
                        existing.TotalExperience = row.TotalExperience;
                    }
                    else
                    {
                        _context.TeachingStaffDepartmentWiseDetails.Add(new TeachingStaffDepartmentWiseDetail
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = courseLevel,
                            DepartmentCode = dept.DepartmentCode,
                            DesignationCode = row.DesignationCode,
                            DesignationName = row.DesignationName,
                            Ugfrom = hasUG ? DateOnly.FromDateTime(row.UGFrom!.Value) : null,
                            Ugto = hasUG ? DateOnly.FromDateTime(row.UGTo!.Value) : null,
                            Pgfrom = hasPG ? DateOnly.FromDateTime(row.PGFrom!.Value) : null,
                            Pgto = hasPG ? DateOnly.FromDateTime(row.PGTo!.Value) : null,
                            TotalExperience = row.TotalExperience
                        });
                    }
                }

            }


            await _context.SaveChangesAsync();

            return RedirectToAction("NonTeachingStaffDepartmentwise");
        }


        [HttpGet]
        public async Task<IActionResult> NonTeachingStaffDepartmentwise()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = CourseLevel;

            var vm = new TeachingStaffDepartmentWiseVm
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                CourseLevel = courseLevel
            };

            vm.NonTeachingStaff = await _context.NonTeachingStaffDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseLevel == courseLevel)
                .Select(x => new NonTeachingStaffRow
                {
                    Id = x.Id,
                    StaffName = x.StaffName,
                    Designation = x.Designation,
                    MobileNumber = x.MobileNumber,
                    SalaryPaid = x.SalaryPaid
                })
                .ToListAsync();

            // If empty → show 2 blank rows
            if (!vm.NonTeachingStaff.Any())
            {
                vm.NonTeachingStaff.Add(new NonTeachingStaffRow());
                vm.NonTeachingStaff.Add(new NonTeachingStaffRow());
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NonTeachingStaffDepartmentwise(TeachingStaffDepartmentWiseVm vm)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = CourseLevel;

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (vm?.NonTeachingStaff == null || !vm.NonTeachingStaff.Any())
                return RedirectToAction("NonTeachingStaffDepartmentwise");

            foreach (var row in vm.NonTeachingStaff)
            {
                // 🔴 Skip empty rows
                bool isEmpty =
                    string.IsNullOrWhiteSpace(row.StaffName) &&
                    string.IsNullOrWhiteSpace(row.Designation) &&
                    string.IsNullOrWhiteSpace(row.MobileNumber) &&
                    string.IsNullOrWhiteSpace(row.SalaryPaid);

                if (isEmpty)
                    continue;

                var existing = await _context.NonTeachingStaffDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == courseLevel &&
                        x.Id == row.Id);

                if (existing != null)
                {
                    // 🔄 UPDATE
                    existing.StaffName = row.StaffName;
                    existing.Designation = row.Designation;
                    existing.MobileNumber = row.MobileNumber;
                    existing.SalaryPaid = row.SalaryPaid;
                }
                else
                {
                    // ➕ INSERT
                    _context.NonTeachingStaffDetails.Add(new NonTeachingStaffDetail
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel,
                        StaffName = row.StaffName,
                        Designation = row.Designation,
                        MobileNumber = row.MobileNumber,
                        SalaryPaid = row.SalaryPaid
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Repo_FacultyDetails", "FacultyDetails");
        }


        [HttpGet]
        public async Task<IActionResult> MedicalUGBedDistribution()
        {


            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = CourseLevel;

            var existing = await _context.MedicalUgbedDistributions.FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == courseLevel);


            if (existing == null)
                return View(new MedicalUGBedDistributionVm());

            var vm = new MedicalUGBedDistributionVm
            {
                Id = existing.Id,
                GenMedicine = existing.GenMedicine,
                Paediatrics = existing.Paediatrics,
                SkinVD = existing.SkinVd,
                Psychiatry = existing.Psychiatry,

                GenSurgery = existing.GenSurgery,
                Orthopaedics = existing.Orthopaedics,
                Ophthalmology = existing.Ophthalmology,
                ENT = existing.Ent,

                ObstetricsANC = existing.ObstetricsAnc,
                Gynaecology = existing.Gynaecology,
                Postpartum = existing.Postpartum,

                MajorOT = existing.MajorOt,
                MinorOT = existing.MinorOt,

                ICCU = existing.Iccu,
                ICU = existing.Icu,
                PICU_NICU = existing.PicuNicu,
                SICU = existing.Sicu,
                TotalICUBeds = existing.TotalIcubeds,
                CasualtyBeds = existing.CasualtyBeds
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicalUGBedDistribution(MedicalUGBedDistributionVm vm)
        {

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;
            var courseLevel = CourseLevel;

            if (!ModelState.IsValid)
                return View(vm);



            var entity = await _context.MedicalUgbedDistributions
                         .FirstOrDefaultAsync(x =>
                             x.CollegeCode == collegeCode &&
                             x.FacultyCode == facultyCode &&
                             x.CourseLevel == courseLevel);

            if (entity == null)
            {
                entity = new MedicalUgbedDistribution
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    CourseLevel = courseLevel,
                    CreatedDate = DateTime.Now
                };

                _context.MedicalUgbedDistributions.Add(entity);
            }

            // Update fields
            entity.GenMedicine = vm.GenMedicine;
            entity.Paediatrics = vm.Paediatrics;
            entity.SkinVd = vm.SkinVD;
            entity.Psychiatry = vm.Psychiatry;

            entity.GenSurgery = vm.GenSurgery;
            entity.Orthopaedics = vm.Orthopaedics;
            entity.Ophthalmology = vm.Ophthalmology;
            entity.Ent = vm.ENT;

            entity.ObstetricsAnc = vm.ObstetricsANC;
            entity.Gynaecology = vm.Gynaecology;
            entity.Postpartum = vm.Postpartum;

            entity.MajorOt = vm.MajorOT;
            entity.MinorOt = vm.MinorOT;

            entity.Iccu = vm.ICCU;
            entity.Icu = vm.ICU;
            entity.PicuNicu = vm.PICU_NICU;
            entity.Sicu = vm.SICU;
            entity.TotalIcubeds = vm.TotalICUBeds;
            entity.CasualtyBeds = vm.CasualtyBeds;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bed distribution saved successfully!";
            return RedirectToAction("MedicalUGBedDistribution");
        }

    }
}
