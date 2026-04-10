using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Services.UserContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace Medical_Affiliation.Controllers
{
    public class CollegeBasicDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SessionUserContext _userContext;

        public CollegeBasicDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private (string? FacultyCode, string? CollegeCode) GetSessionCodes()
        {
            return (
                HttpContext.Session.GetString("FacultyCode")?.Trim(),
                HttpContext.Session.GetString("CollegeCode")?.Trim()
            );
        }

        private IActionResult SessionError()
        {
            TempData["Error"] = "Session expired. Please login again.";
            return RedirectToAction("Login", "Account");
        }

        private void FillDropDowns(InstitutionViewModel vm)
        {

            vm.TalukList = _context.TalukMasters
                .OrderBy(t => t.TalukName)
                .Select(t => new SelectListItem
                {
                    Value = t.TalukId.ToString(),  // Use .ToString() for consistency
                    Text = t.TalukName
                }).ToList();

            vm.DistrictList = _context.DistrictMasters
                .OrderBy(d => d.DistrictName)
                .Select(d => new SelectListItem
                {
                    Value = d.DistrictId.ToString(),  // Fixed: Use DistrictId, not DistrictName
                    Text = d.DistrictName
                }).ToList();

            vm.CourseList = _context.MstMedicalCourseTypes
                .OrderBy(c => c.CourseTypeName)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseTypeId.ToString(),  // Assume ID exists; adjust if needed
                    Text = c.CourseTypeName
                }).ToList();

            vm.institutetypelist = _context.MstInstitutionTypes
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
        [HttpGet]
        public IActionResult Institution_Details()
        {
            // 1. Resolve session values
            var facultyCode = HttpContext.Session.GetString("FacultyCode")
                              ?? _userContext.FacultyId.ToString();
            var collegeCode = HttpContext.Session.GetString("CollegeCode")
                              ?? _userContext.CollegeCode;

            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return BadRequest("Session expired. FacultyCode / CollegeCode not found. Please login again.");

            // 2. Try to load existing record
            var entity = _context.AffInstitutionsDetails
                .AsNoTracking()
                .FirstOrDefault(x =>
                    x.FacultyCode.Trim() == facultyCode.Trim() &&
                    x.CollegeCode.Trim() == collegeCode.Trim());

            // 3. Build view model
            InstitutionViewModel vm;

            if (entity == null)
            {
                // No record yet – show blank form
                vm = new InstitutionViewModel
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode
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
                FillDropDowns(vm);
                return View(vm);
            }

            // 4. Load existing entity or create new one
            var entity = _context.AffInstitutionsDetails
                .FirstOrDefault(x =>
                    x.FacultyCode == vm.FacultyCode &&
                    x.CollegeCode == vm.CollegeCode);

            bool isNew = entity == null;

            if (isNew)
            {
                entity = new AffInstitutionsDetail
                {
                    FacultyCode = vm.FacultyCode,
                    CollegeCode = vm.CollegeCode
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




        /// <summary>Maps an AffInstitutionsDetail entity to a view model.</summary>
        private static InstitutionViewModel MapEntityToViewModel(AffInstitutionsDetail e)
        {
            return new InstitutionViewModel
            {
                CollegeCode = e.CollegeCode,
                FacultyCode = e.FacultyCode,
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

        private void FillInstitutionDropDowns(InstitutionViewModel1 vm)
        {
            vm.TalukList = _context.TalukMasters
                .OrderBy(t => t.TalukName)
                .Select(t => new SelectListItem { Value = t.TalukId, Text = t.TalukName })
                .ToList();

            vm.DistrictList = _context.DistrictMasters
                .OrderBy(d => d.DistrictName)
                .Select(d => new SelectListItem { Value = d.DistrictName, Text = d.DistrictName })
                .ToList();

            vm.CourseList = _context.MstMedicalCourseTypes
                .OrderBy(c => c.CourseTypeName)
                .Select(c => new SelectListItem { Value = c.CourseTypeName, Text = c.CourseTypeName })
                .ToList();

            vm.institutetypelist = _context.MstInstitutionTypes
                .Where(e => e.FacultyId == 1)
                .OrderBy(c => c.OrganizationCategory)
                .Select(c => new SelectListItem { Value = c.InstitutionTypeId.ToString(), Text = c.InstitutionType })
                .ToList();

            vm.Institutestatuslist = _context.AffInstitutionStatusMasters
                .OrderBy(c => c.InstitutionStatusId)
                .Select(c => new SelectListItem { Value = c.InstitutionStatusId.ToString(), Text = c.StatusName })
                .ToList();
        }


        [HttpGet]
    public async Task<IActionResult> Aff_InstituteDetails()
        {
            var (facultyCode, collegeCode) = GetSessionCodes();
            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return SessionError();

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

                vm.NameOfInstitution = collegeName
                    ?? HttpContext.Session.GetString("CollegeName")
                    ?? "";
            }

            // Build dropdown — int == int comparison, returns Value as string
            vm.TypeOfInstitutionList = await LoadInstitutionTypeList(facultyCode);

            return View(vm);
        }


        // ── COMPLETE FIXED POST (copy-paste ready) ───────────────────────────────────
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
            var (facultyCode, collegeCode) = GetSessionCodes();
            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return SessionError();

            vm.FacultyCode = facultyCode;
            vm.CollegeCode = collegeCode;

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

        private static async Task<byte[]?> FileToBytesOrNull(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }


        [HttpGet]
        public async Task<IActionResult> Aff_TrustMemberDetails()
        {
            var (facultyCode, collegeCode) = GetSessionCodes();
            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return SessionError();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aff_TrustMemberDetails(Medical_TrustMemberDetailsListVM vm)
        {
            var (facultyCode, collegeCode) = GetSessionCodes();
            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return SessionError();

            vm.FacultyCode = facultyCode;
            vm.CollegeCode = collegeCode;

            ModelState.Remove(nameof(vm.FacultyCode));
            ModelState.Remove(nameof(vm.CollegeCode));

            vm.DesignationList = await GetDesignationListAsync(facultyCode);

            if (!ModelState.IsValid)
                return View(vm);

            // ── Remove existing rows for this college ────────────────────────────────
            var existing = _context.ContinuationTrustMemberDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);
            _context.ContinuationTrustMemberDetails.RemoveRange(existing);

            // ── Pre-load all designations in ONE query (avoid N+1) ──────────────────
            var allDesignations = await _context.DesignationMasters
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .ToListAsync();

            foreach (var row in vm.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.TrustMemberName))
                    continue;

                // ── Parse joining date ───────────────────────────────────────────────
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

                // ── Resolve designation name from the pre-loaded list ────────────────
                // row.DesignationCode holds whatever value the <select> posted.
                // We match against DesignationCode in the master table.
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

                    // ★ Save both the code (FK) and the resolved name
                    DesignationId = row.DesignationCode,                      // the posted value
                    Designation = matchedDesignation?.DesignationName ?? row.DesignationCode  // fallback to code if name not found
                };

                _context.ContinuationTrustMemberDetails.Add(entity);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Trust member details saved successfully.";
            return RedirectToAction(nameof(Aff_TrustMemberDetails));
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
    }
}