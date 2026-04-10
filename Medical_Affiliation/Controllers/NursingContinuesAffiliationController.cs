//using Medical_Affiliation.DATA;
//using Medical_Affiliation.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Globalization;

//namespace Medical_Affiliation.Controllers
//{
//    public class NursingContinuesAffiliationController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public NursingContinuesAffiliationController(ApplicationDbContext context)
//        {
//            _context = context;

//        }
//        public class AffiliationViewModel
//        {
//            public int SelectedAffiliationId { get; set; }
//            public IEnumerable<SelectListItem> TypeOfAffiliations { get; set; }
//        }

//        public IActionResult Index()
//        {
//            var model = new SidebarViewModel
//            {
//                TypeOfAffiliationList = _context.TypeOfAffiliations
//                    .Select(t => new SelectListItem
//                    {
//                        Value = t.TypeId.ToString(),
//                        Text = t.TypeDescription
//                    })
//                    .ToList()
//            };

//            return View(model);
//        }

//        [HttpGet]
//        public IActionResult NursingAffiliation()
//        {
//            var model = new NursingAffiliationViewModel
//            {
//                AffSanIntake = _context.AffSanctionedIntakeForCourses.FirstOrDefault() == null
//                    ? new SanctionedIntakeCreateVm()
//                    : new SanctionedIntakeCreateVm { /* map from entity */ },

//                AffCourseDetails = _context.AffCourseDetails.FirstOrDefault() == null
//                    ? new AffCourseDetailsListVM()
//                    : new AffCourseDetailsListVM { /* map */ },

//                InstituteDetails = _context.InstitutionDetails.FirstOrDefault() == null
//                    ? new InstitutionDetailsViewModel()
//                    : new InstitutionDetailsViewModel { /* map */ },

//                NursingInstituteDetails = _context.NursingInstituteDetails.FirstOrDefault() == null
//                    ? new InstitutionBasicDetailsViewModel()
//                    : new InstitutionBasicDetailsViewModel { /* map */ },

//                TrustMemberDetails = _context.TrustMemberDetails.FirstOrDefault() == null
//                    ? new TrustMemberDetailsListVM()
//                    : new TrustMemberDetailsListVM { /* map */ }
//            };

//            return View("NursingAffiliation", model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult OnAffiliationTypeChanged(SidebarViewModel model)
//        {
//            // Adjust the value check to match your actual ID/text
//            if (model.SelectedAffiliationId == 2) // Continuation of Affiliation
//            {
//                return RedirectToAction(
//                    actionName: "Nursing_Institute_Details1",
//                    controllerName: "NursingContinuesAffiliation");
//            }

//            // default: stay on same page or redirect somewhere else
//            return RedirectToAction("Dashboard", "Collegelogin");
//        }

//        // GET: /Institution/Create
//        [HttpGet]
//        public async Task<IActionResult> Nursing_Institute_Details()
//        {
//            // Build empty VM
//            var vm = new InstitutionBasicDetailsViewModel();

//            // Get codes from session/claims
//            var collegeName = User.FindFirst("CollegeName")?.Value;
//            var collegeCode = User.FindFirst("CollegeCode")?.Value;
//            var facultyCode = User.FindFirst("FacultyCode")?.Value;

//            if (string.IsNullOrEmpty(collegeName))
//                collegeName = HttpContext.Session.GetString("CollegeName");
//            if (string.IsNullOrEmpty(collegeCode))
//                collegeCode = HttpContext.Session.GetString("CollegeCode");
//            if (string.IsNullOrEmpty(facultyCode))
//                facultyCode = HttpContext.Session.GetString("FacultyCode");

//            // Try load existing entity
//            InstitutionBasicDetail existing = null;
//            if (!string.IsNullOrWhiteSpace(collegeCode) && !string.IsNullOrWhiteSpace(facultyCode))
//            {
//                existing = await _context.InstitutionBasicDetails
//                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);
//            }

//            if (existing != null)
//            {
//                // Map entity -> VM (EDIT mode)
//                vm.InstitutionId = existing.InstitutionId;
//                vm.CollegeCode = existing.CollegeCode;
//                vm.FacultyCode = existing.FacultyCode;
//                vm.NameOfInstitution = existing.NameOfInstitution;
//                vm.AddressOfInstitution = existing.AddressOfInstitution;
//                vm.VillageTownCity = existing.VillageTownCity;
//                vm.Taluk = existing.Taluk;
//                vm.District = existing.District;
//                vm.PinCode = existing.PinCode;
//                vm.MobileNumber = existing.MobileNumber;
//                vm.StdCode = existing.StdCode;
//                vm.Fax = existing.Fax;
//                vm.Website = existing.Website;
//                vm.EmailId = existing.EmailId;
//                vm.AltLandlineOrMobile = existing.AltLandlineOrMobile;
//                vm.AltEmailId = existing.AltEmailId;
//                vm.AcademicYearStarted = existing.AcademicYearStarted;
//                vm.IsRuralInstitution = (bool)existing.IsRuralInstitution;
//                vm.IsMinorityInstitution = (bool)existing.IsMinorityInstitution;
//                vm.TrustName = existing.TrustName;
//                vm.PresidentName = existing.PresidentName;
//                vm.AadhaarNumber = existing.AadhaarNumber;
//                vm.PANNumber = existing.Pannumber;
//                vm.RegistrationNumber = existing.RegistrationNumber;
//                vm.RegistrationDate = existing.RegistrationDate?.ToDateTime(TimeOnly.MinValue);
//                vm.Amendments = (bool)existing.Amendments;
//                vm.ExistingTrustName = existing.ExistingTrustName;
//                vm.GOKObtainedTrustName = existing.GokobtainedTrustName;
//                vm.ChangesInTrustName = (bool)existing.ChangesInTrustName;
//                vm.OtherNursingCollegeInCity = (bool)existing.OtherNursingCollegeInCity;
//                vm.CategoryOfOrganisation = existing.CategoryOfOrganisation;
//                vm.ContactPersonName = existing.ContactPersonName;
//                vm.ContactPersonRelation = existing.ContactPersonRelation;
//                vm.ContactPersonMobile = existing.ContactPersonMobile;
//                vm.OtherPhysiotherapyCollegeInCity = (bool)existing.OtherPhysiotherapyCollegeInCity;
//                vm.CoursesAppliedText = existing.CoursesAppliedText;
//                vm.HeadOfInstitutionName = existing.HeadOfInstitutionName;
//                vm.HeadOfInstitutionAddress = existing.HeadOfInstitutionAddress;
//                vm.FinancingAuthorityName = existing.FinancingAuthorityName;
//                vm.CollegeStatus = existing.CollegeStatus;
//                vm.GovAutonomousCertNumber = existing.GovAutonomousCertNumber;
//                vm.KncCertificateNumber = existing.KncCertificateNumber;
//                vm.TypeOfInstitution = existing.TypeOfInstitution;
//                //vm.TypeOfOrganization = existing.TypeOfOrganization;
//            }
//            else
//            {
//                // CREATE mode default values
//                vm.NameOfInstitution = collegeName ?? string.Empty;
//                vm.CollegeCode = collegeCode;
//                vm.FacultyCode = facultyCode;
//            }

//            // default OtherPhysiotherapyCollegeInCity
//            if (vm.OtherPhysiotherapyCollegeInCity == null)
//                vm.OtherPhysiotherapyCollegeInCity = false;

//            // Dropdown
//            vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
//                .Where(e => e.FacultyId.ToString() == facultyCode)
//                .OrderBy(t => t.InstitutionType)
//                .Select(t => new SelectListItem
//                {
//                    Value = t.InstitutionTypeId.ToString(),
//                    Text = t.InstitutionType
//                })
//                .ToListAsync();

//            //vm.TypeOfOrganization = await _context.TypeOfOrganizationMasters
//            //    //.Where(e => e.faculty)
//            //    .OrderBy(t => t.TypeName)
//            //    .Select(t => new SelectListItem
//            //    {
//            //        Value = t.TypeId.ToString(),
//            //        Text = t.TypeName
//            //    })
//            //    .ToListAsync();

//            return View(vm);
//        }

//        // POST: /Institution/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [RequestSizeLimit(104857600)]  // 100MB
//        [RequestFormLimits(ValueCountLimit = 100000, ValueLengthLimit = int.MaxValue)]
//        public async Task<IActionResult> Nursing_Institute_Details(InstitutionBasicDetailsViewModel vm)
//        {
//            // Get session/claims FIRST
//            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? User?.FindFirst("CollegeCode")?.Value;
//            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? User?.FindFirst("FacultyCode")?.Value;

//            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login", "Account");
//            }

//            // Server-authoritative
//            vm.CollegeCode = collegeCode;
//            vm.FacultyCode = facultyCode;
//            vm.NameOfInstitution ??= User?.FindFirst("CollegeName")?.Value
//                ?? HttpContext.Session.GetString("CollegeName") ?? "";

//            // Remove client-supplied model state for server fields
//            ModelState.Remove(nameof(vm.CollegeCode));
//            ModelState.Remove(nameof(vm.FacultyCode));
//            ModelState.Remove(nameof(vm.NameOfInstitution));

//            if (!ModelState.IsValid)
//            {
//                vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
//                    .OrderBy(t => t.InstitutionType)
//                    .Select(t => new SelectListItem
//                    {
//                        Value = t.InstitutionTypeId.ToString(),
//                        Text = t.InstitutionType
//                    }).ToListAsync();

//                TempData["ModelStateErrors"] = string.Join(", ",
//                    ModelState.Values.Where(v => v.Errors.Count > 0)
//                                     .SelectMany(v => v.Errors)
//                                     .Select(e => e.ErrorMessage));
//                return View(vm);
//            }

//            // Decide: UPDATE or INSERT
//            InstitutionBasicDetail entity;

//            if (vm.InstitutionId > 0)
//            {
//                // UPDATE existing
//                entity = await _context.InstitutionBasicDetails
//                    .FirstOrDefaultAsync(x => x.InstitutionId == vm.InstitutionId
//                                           && x.CollegeCode == collegeCode
//                                           && x.FacultyCode == facultyCode);

//                if (entity == null)
//                {
//                    TempData["Error"] = "Institution record not found for update.";
//                    return RedirectToAction(nameof(Nursing_Institute_Details));
//                }
//            }
//            else
//            {
//                // INSERT new (ensure no duplicate by Faculty+College)
//                var duplicate = await _context.InstitutionBasicDetails
//                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

//                if (duplicate != null)
//                {
//                    TempData["Error"] = "Institution details already exist for this college. Use Edit.";
//                    return RedirectToAction(nameof(TrustMemberDetails), new { id = duplicate.InstitutionId });
//                }

//                entity = new InstitutionBasicDetail
//                {
//                    FacultyCode = facultyCode,
//                    CollegeCode = collegeCode,
//                    CreatedOn = DateTime.UtcNow
//                };
//                _context.InstitutionBasicDetails.Add(entity);
//            }

//            // Map VM -> entity (common for both create/update)
//            //entity.TypeOfOrganization = vm.TypeOfOrganization ?? "";
//            entity.TypeOfInstitution = vm.TypeOfInstitution;
//            entity.NameOfInstitution = vm.NameOfInstitution ?? "";
//            entity.AddressOfInstitution = vm.AddressOfInstitution ?? "";
//            entity.VillageTownCity = vm.VillageTownCity ?? "";
//            entity.Taluk = vm.Taluk ?? "";
//            entity.District = vm.District ?? "";
//            entity.PinCode = vm.PinCode;
//            entity.MobileNumber = vm.MobileNumber;
//            entity.StdCode = vm.StdCode;
//            entity.Fax = vm.Fax;
//            entity.Website = vm.Website;
//            entity.EmailId = vm.EmailId;
//            entity.AltLandlineOrMobile = vm.AltLandlineOrMobile;
//            entity.AltEmailId = vm.AltEmailId;
//            entity.AcademicYearStarted = vm.AcademicYearStarted;
//            entity.IsRuralInstitution = vm.IsRuralInstitution;
//            entity.IsMinorityInstitution = vm.IsMinorityInstitution;
//            entity.TrustName = vm.TrustName ?? "";
//            entity.PresidentName = vm.PresidentName ?? "";
//            entity.AadhaarNumber = vm.AadhaarNumber;
//            entity.Pannumber = vm.PANNumber;
//            entity.RegistrationNumber = vm.RegistrationNumber;
//            entity.RegistrationDate = vm.RegistrationDate.HasValue
//                ? DateOnly.FromDateTime(vm.RegistrationDate.Value)
//                : null;
//            entity.Amendments = vm.Amendments;
//            entity.ExistingTrustName = vm.ExistingTrustName ?? "";
//            entity.GokobtainedTrustName = vm.GOKObtainedTrustName ?? "";
//            entity.ChangesInTrustName = vm.ChangesInTrustName;
//            entity.OtherNursingCollegeInCity = vm.OtherNursingCollegeInCity;
//            entity.CategoryOfOrganisation = vm.CategoryOfOrganisation ?? "";
//            entity.ContactPersonName = vm.ContactPersonName ?? "";
//            entity.ContactPersonRelation = vm.ContactPersonRelation ?? "";
//            entity.ContactPersonMobile = vm.ContactPersonMobile;
//            entity.OtherPhysiotherapyCollegeInCity = vm.OtherPhysiotherapyCollegeInCity;
//            entity.CoursesAppliedText = vm.CoursesAppliedText ?? "";
//            entity.HeadOfInstitutionName = vm.HeadOfInstitutionName ?? "";
//            entity.HeadOfInstitutionAddress = vm.HeadOfInstitutionAddress ?? "";
//            entity.FinancingAuthorityName = vm.FinancingAuthorityName ?? "";
//            entity.CollegeStatus = vm.CollegeStatus;
//            entity.GovAutonomousCertNumber = vm.GovAutonomousCertNumber;
//            entity.KncCertificateNumber = vm.KncCertificateNumber;

//            // Files (null-safe)
//            entity.GovAutonomousCertFile = await SafeToBytes(vm.GovAutonomousCertFile);
//            entity.GovCouncilMembershipFile = await SafeToBytes(vm.GovCouncilMembershipFile);
//            entity.GokOrderExistingCoursesFile = await SafeToBytes(vm.GokOrderExistingCoursesFile);
//            entity.FirstAffiliationNotifFile = await SafeToBytes(vm.FirstAffiliationNotifFile);
//            entity.ContinuationAffiliationFile = await SafeToBytes(vm.ContinuationAffiliationFile);
//            entity.KncCertificateFile = await SafeToBytes(vm.KncCertificateFile);
//            entity.AmendedDoc = await SafeToBytes(vm.AmendedDoc);
//            entity.AadhaarFile = await SafeToBytes(vm.AadhaarFile);
//            entity.Panfile = await SafeToBytes(vm.PANFile);
//            entity.BankStatementFile = await SafeToBytes(vm.BankStatementFile);
//            entity.RegistrationCertificateFile = await SafeToBytes(vm.RegistrationCertificateFile);
//            entity.RegisteredTrustMemberDetails = await SafeToBytes(vm.RegisteredTrustMemberDetails);
//            entity.AuditStatementFile = await SafeToBytes(vm.AuditStatementFile);

//            try
//            {
//                var savedCount = await _context.SaveChangesAsync();

//                if (savedCount > 0)
//                {
//                    TempData["Success"] = vm.InstitutionId > 0
//                        ? "Institution details updated successfully!"
//                        : "Institution details saved successfully!";

//                    return RedirectToAction(nameof(TrustMemberDetails), new { id = entity.InstitutionId });
//                }
//                else
//                {
//                    TempData["Error"] = "No changes were saved. Check required fields.";
//                    vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
//                        .OrderBy(t => t.InstitutionType)
//                        .Select(t => new SelectListItem
//                        {
//                            Value = t.InstitutionTypeId.ToString(),
//                            Text = t.InstitutionType
//                        }).ToListAsync();
//                    return View(vm);
//                }
//            }
//            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("duplicate") == true ||
//                                               ex.InnerException?.Message?.Contains("PRIMARY KEY") == true)
//            {
//                TempData["Error"] = "Duplicate record exists for this Faculty/College combination.";
//                return View(vm);
//            }
//            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("NULL") == true)
//            {
//                TempData["Error"] = $"Required field cannot be null: {ex.InnerException.Message}";
//                return View(vm);
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Save failed: {ex.Message}";
//                Console.WriteLine($"Institution save error: {ex}");
//                return View(vm);
//            }
//        }
//        public async Task<IActionResult> DownloadGovAutonomousCert(int id)
//        {
//            var entity = await _context.InstitutionBasicDetails
//                .FirstOrDefaultAsync(x => x.InstitutionId == id);

//            if (entity == null || entity.GovAutonomousCertFile == null || entity.GovAutonomousCertFile.Length == 0)
//                return NotFound();

//            // Adjust contentType and fileName if you store them
//            var contentType = "application/pdf";
//            var fileName = "GovAutonomousCertificate.pdf";

//            return File(entity.GovAutonomousCertFile, contentType, fileName);
//        }

//        // **REQUIRED HELPER METHOD** - Replace your ToBytes
//        private async Task<byte[]?> SafeToBytes(IFormFile? file)
//        {
//            if (file == null || file.Length == 0)
//                return null;

//            try
//            {
//                using var stream = file.OpenReadStream();
//                using var memoryStream = new MemoryStream();
//                await stream.CopyToAsync(memoryStream);
//                return memoryStream.ToArray();
//            }
//            catch
//            {
//                return null; // Fail-safe: return null instead of throwing
//            }
//        }

//        private static async Task<byte[]> ToBytes(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return null;

//            using var ms = new MemoryStream();
//            await file.CopyToAsync(ms);
//            return ms.ToArray();
//        }

//        // Optional: GET /Institution/Details/5
//        [HttpGet]
//        public async Task<IActionResult> Details(int id)
//        {
//            var entity = await _context.InstitutionBasicDetails
//                .AsNoTracking()
//                .FirstOrDefaultAsync(x => x.InstitutionId == id);

//            if (entity == null) return NotFound();

//            // map to a read-only VM or reuse the same VM as needed
//            return View(entity);
//        }

//        // Update the TrustMemberDetails GET to materialize entities and map DateOnly? -> DateTime?
//        // GET: TrustMemberDetails
//        // Replace the TrustMemberDetails GET and POST with the following.
//        // - Populates designation dropdown filtered by facultyCode (falls back to MstDesignations)
//        // - Binds designation values as strings (DesignationCode) so they map to your ContinuationTrustMemberDetail.DesignationId

//        [HttpGet]
//        public async Task<IActionResult> TrustMemberDetails()
//        {
//            var model = new TrustMemberDetailsListVM();
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");
//            model.FacultyCode = facultyCode;
//            model.CollegeCode = collegeCode;

//            // Populate designation dropdown filtered by facultyCode
//            if (!string.IsNullOrEmpty(facultyCode))
//            {
//                // Primary: DesignationMasters (uses DesignationCode, FacultyCode)
//                var fromDesignationMasters = await _context.DesignationMasters
//                    .Where(d => d.FacultyCode.ToString() == facultyCode)
//                    .OrderBy(d => d.DesignationName)
//                    .Select(d => new SelectListItem
//                    {
//                        Value = d.DesignationCode,
//                        Text = d.DesignationName ?? string.Empty
//                    })
//                    .ToListAsync();

//                if (fromDesignationMasters.Any())
//                {
//                    model.DesignationList = fromDesignationMasters;
//                }
//                else
//                {
//                    // Fallback: MstDesignations (uses DesignationId int, FacultyId)
//                    if (int.TryParse(facultyCode, out var facultyId))
//                    {
//                        model.DesignationList = await _context.MstDesignations
//                            .Where(d => d.FacultyId.HasValue && d.FacultyId.Value == facultyId)
//                            .OrderBy(d => d.DesignationName)
//                            .Select(d => new SelectListItem
//                            {
//                                Value = d.DesignationId.ToString(),
//                                Text = d.DesignationName ?? string.Empty
//                            })
//                            .ToListAsync();
//                    }
//                    else
//                    {
//                        model.DesignationList = new List<SelectListItem>();
//                    }
//                }
//            }
//            else
//            {
//                // No faculty code: empty list
//                model.DesignationList = new List<SelectListItem>();
//            }

//            if (!string.IsNullOrEmpty(collegeCode) && !string.IsNullOrEmpty(facultyCode))
//            {
//                var existingEntities = await _context.ContinuationTrustMemberDetails
//                    .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
//                    .OrderBy(x => x.TrustMemberName)
//                    .ToListAsync();

//                model.Rows = existingEntities.Select(x => new TrustMemberDetailsRowVM
//                {
//                    SlNo = x.SlNo,
//                    FacultyCode = x.FacultyCode,
//                    CollegeCode = x.CollegeCode,
//                    TrustMemberName = x.TrustMemberName,
//                    Qualification = x.Qualification,
//                    MobileNumber = x.MobileNumber,
//                    Age = x.Age,
//                    JoiningDateString = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("yyyy-MM-dd") : null,
//                    DesignationCode = x.DesignationId // store DB string id/code directly
//                }).ToList();
//            }

//            if (!model.Rows.Any())
//                model.Rows.Add(new TrustMemberDetailsRowVM { FacultyCode = facultyCode, CollegeCode = collegeCode });

//            return View(model);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> TrustMemberDetails(TrustMemberDetailsListVM model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))

//                return RedirectToAction("InstituteDetails");

//            // Remove server-controlled fields from validation
//            ModelState.Remove(nameof(TrustMemberDetailsListVM.CollegeCode));
//            ModelState.Remove(nameof(TrustMemberDetailsListVM.FacultyCode));
//            //ModelState.Remove(nameof(TrustMemberDetailsRowVM.Designation));

//            // Re-populate designation list so view can re-render on validation error
//            if (!string.IsNullOrEmpty(facultyCode))
//            {
//                var fromDesignationMasters = await _context.DesignationMasters
//                    .Where(d => d.FacultyCode.ToString() == facultyCode)
//                    .OrderBy(d => d.DesignationName)
//                    .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? string.Empty })
//                    .ToListAsync();

//                if (fromDesignationMasters.Any())
//                    model.DesignationList = fromDesignationMasters;
//                else if (int.TryParse(facultyCode, out var facultyId))
//                    model.DesignationList = await _context.MstDesignations
//                        .Where(d => d.FacultyId.HasValue && d.FacultyId.Value == facultyId)
//                        .OrderBy(d => d.DesignationName)
//                        .Select(d => new SelectListItem { Value = d.DesignationId.ToString(), Text = d.DesignationName ?? string.Empty })
//                        .ToListAsync();
//            }

//            if (!ModelState.IsValid)
//                return View(model);

//            // Remove existing records
//            var existingRecords = _context.ContinuationTrustMemberDetails
//                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);
//            _context.ContinuationTrustMemberDetails.RemoveRange(existingRecords);

//            foreach (var row in model.Rows)
//            {
//                if (string.IsNullOrWhiteSpace(row.TrustMemberName))
//                    continue;

//                // Parse HTML date string -> DateOnly
//                DateOnly? joiningDate = null;
//                if (!string.IsNullOrWhiteSpace(row.JoiningDateString) &&
//                    DateOnly.TryParseExact(row.JoiningDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
//                {
//                    joiningDate = d;
//                }

//                // DesignationCode is stored as string in DB.DesignationId
//                var designationCode = string.IsNullOrWhiteSpace(row.DesignationCode) ? null : row.DesignationCode.Trim();

//                // Optional: fetch designation text if you want to persist the human-readable name
//                string? designationName = null;
//                if (!string.IsNullOrWhiteSpace(designationCode))
//                {
//                    // Try DesignationMasters first
//                    var des = await _context.DesignationMasters.FirstOrDefaultAsync(dm => dm.DesignationCode == designationCode);
//                    if (des != null)
//                        designationName = des.DesignationName;
//                    else if (int.TryParse(designationCode, out var did))
//                    {
//                        // fallback to MstDesignations numeric id
//                        var md = await _context.MstDesignations.FindAsync(did);
//                        designationName = md?.DesignationName;
//                    }
//                }

//                var entity = new ContinuationTrustMemberDetail
//                {
//                    FacultyCode = facultyCode,
//                    CollegeCode = collegeCode,
//                    TrustMemberName = row.TrustMemberName,
//                    Designation = designationName,
//                    Qualification = row.Qualification,
//                    MobileNumber = row.MobileNumber,
//                    Age = row.Age,
//                    JoiningDate = joiningDate,
//                    DesignationId = designationCode
//                };

//                _context.ContinuationTrustMemberDetails.Add(entity);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction("InstituteDetails");
//        }


//        // Replace the GET portion that populates the InstituteDetails VM with this excerpt
//        [HttpGet]
//        public async Task<IActionResult> InstituteDetails()
//        {
//            var vm = new InstitutionDetailsViewModel();
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            vm.CollegeCode = collegeCode;
//            vm.FacultyCode = facultyCode;

//            if (!string.IsNullOrEmpty(collegeCode) && !string.IsNullOrEmpty(facultyCode))
//            {
//                var existingInstitution = await _context.AffInstitutionsDetails
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

//                if (existingInstitution != null)
//                {
//                    vm.TypeOfInstitution = existingInstitution.TypeOfInstitution;
//                    // Prefer database value; fall back to session/claim if DB value missing
//                    vm.NameOfInstitution = !string.IsNullOrWhiteSpace(existingInstitution.NameOfInstitution)
//                        ? existingInstitution.NameOfInstitution
//                        : (HttpContext.Session.GetString("CollegeName") ?? User?.FindFirst("CollegeName")?.Value ?? string.Empty);

//                    vm.Address = existingInstitution.Address;
//                    vm.VillageTownCity = existingInstitution.VillageTownCity;
//                    vm.Taluk = existingInstitution.Taluk;
//                    vm.District = existingInstitution.District;
//                    vm.PinCode = existingInstitution.PinCode;
//                    vm.MobileNumber = existingInstitution.MobileNumber;
//                    vm.StdCode = existingInstitution.StdCode;
//                    vm.Fax = existingInstitution.Fax;
//                    vm.Website = existingInstitution.Website;
//                    vm.SurveyNoPidNo = existingInstitution.SurveyNoPidNo;
//                    vm.MinorityInstitute = existingInstitution.MinorityInstitute;
//                    vm.AttachedToMedicalClg = existingInstitution.AttachedToMedicalClg;
//                    vm.RuralInstitute = existingInstitution.RuralInstitute;

//                    // YearOfEstablishment in DB may be stored as string (yyyy or yyyy-MM-dd). Preserve as-is.
//                    vm.YearOfEstablishment = existingInstitution.YearOfEstablishment ?? string.Empty;

//                    vm.EmailId = existingInstitution.EmailId;
//                    vm.AltLandlineMobile = existingInstitution.AltLandlineMobile;
//                    vm.AltEmailId = existingInstitution.AltEmailId;
//                    vm.HeadOfInstitution = existingInstitution.HeadOfInstitution;
//                    vm.HeadAddress = existingInstitution.HeadAddress;
//                    vm.FinancingAuthority = existingInstitution.FinancingAuthority;
//                    vm.StatusOfCollege = existingInstitution.StatusOfCollege;
//                    vm.CourseApplied = existingInstitution.CourseApplied;
//                }
//                else
//                {
//                    // No DB record — use session/claim name if available
//                    vm.NameOfInstitution = HttpContext.Session.GetString("CollegeName") ?? User?.FindFirst("CollegeName")?.Value ?? string.Empty;
//                    vm.YearOfEstablishment = string.Empty;
//                }
//            }
//            else
//            {
//                // Session missing — still try to populate name
//                vm.NameOfInstitution = HttpContext.Session.GetString("CollegeName") ?? User?.FindFirst("CollegeName")?.Value ?? string.Empty;
//                vm.YearOfEstablishment = string.Empty;
//            }

//            return View(vm);
//        }

//        // Replace the POST portion handling YearOfEstablishment validation and mapping with this excerpt
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> InstituteDetails(InstitutionDetailsViewModel vm)
//        {
//            // Remove server-side controlled properties from ModelState
//            ModelState.Remove(nameof(vm.CollegeCode));
//            ModelState.Remove(nameof(vm.FacultyCode));
//            ModelState.Remove(nameof(vm.DocumentFile));

//            // Enforce codes from session
//            vm.CollegeCode = HttpContext.Session.GetString("CollegeCode");
//            vm.FacultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(vm.CollegeCode) || string.IsNullOrEmpty(vm.FacultyCode))
//            {
//                ModelState.AddModelError(string.Empty, "College / Faculty code not found in session.");
//                return View(vm);
//            }

//            // YearOfEstablishment is now a string; require a non-empty value
//            if (string.IsNullOrWhiteSpace(vm.YearOfEstablishment))
//            {
//                ModelState.AddModelError(nameof(vm.YearOfEstablishment), "Year of Establishment is required.");
//                return View(vm);
//            }

//            if (!ModelState.IsValid)
//            {
//                return View(vm);
//            }

//            // Fetch existing record (if any)
//            var existingInstitution = await _context.AffInstitutionsDetails
//                .FirstOrDefaultAsync(x => x.CollegeCode == vm.CollegeCode &&
//                                          x.FacultyCode == vm.FacultyCode);

//            // Handle document upload
//            byte[] docBytes = null;
//            string docName = null;
//            string docContentType = null;

//            if (vm.DocumentFile != null && vm.DocumentFile.Length > 0)
//            {
//                using var ms = new MemoryStream();
//                await vm.DocumentFile.CopyToAsync(ms);
//                docBytes = ms.ToArray();
//                docName = vm.DocumentFile.FileName;
//                docContentType = vm.DocumentFile.ContentType;
//            }

//            // Use the string provided by the user (trim). If you want to normalize to yyyy, do additional parsing here.
//            var yearOfEstablishment = vm.YearOfEstablishment?.Trim();

//            if (existingInstitution != null)
//            {
//                // UPDATE
//                existingInstitution.CollegeCode = vm.CollegeCode;
//                existingInstitution.FacultyCode = vm.FacultyCode;
//                existingInstitution.TypeOfInstitution = vm.TypeOfInstitution;
//                existingInstitution.NameOfInstitution = vm.NameOfInstitution;
//                existingInstitution.Address = vm.Address;
//                existingInstitution.VillageTownCity = vm.VillageTownCity;
//                existingInstitution.Taluk = vm.Taluk;
//                existingInstitution.District = vm.District;
//                existingInstitution.PinCode = vm.PinCode;
//                existingInstitution.MobileNumber = vm.MobileNumber;
//                existingInstitution.StdCode = vm.StdCode;
//                existingInstitution.Fax = vm.Fax;
//                existingInstitution.Website = vm.Website;
//                existingInstitution.SurveyNoPidNo = vm.SurveyNoPidNo;
//                existingInstitution.MinorityInstitute = vm.MinorityInstitute;
//                existingInstitution.AttachedToMedicalClg = vm.AttachedToMedicalClg;
//                existingInstitution.RuralInstitute = vm.RuralInstitute;
//                existingInstitution.YearOfEstablishment = yearOfEstablishment;
//                existingInstitution.EmailId = vm.EmailId;
//                existingInstitution.AltLandlineMobile = vm.AltLandlineMobile;
//                existingInstitution.AltEmailId = vm.AltEmailId;
//                existingInstitution.HeadOfInstitution = vm.HeadOfInstitution;
//                existingInstitution.HeadAddress = vm.HeadAddress;
//                existingInstitution.FinancingAuthority = vm.FinancingAuthority;
//                existingInstitution.StatusOfCollege = vm.StatusOfCollege;
//                existingInstitution.CourseApplied = vm.CourseApplied;

//                // Only overwrite document fields if a new file was uploaded
//                if (docBytes != null)
//                {
//                    existingInstitution.DocumentName = docName;
//                    existingInstitution.DocumentContentType = docContentType;
//                    existingInstitution.DocumentData = docBytes;
//                }

//                await _context.SaveChangesAsync();
//                var id = existingInstitution.InstitutionId;
//                return RedirectToAction(nameof(AffCourseDetails), new { id });
//            }
//            else
//            {
//                // CREATE
//                var entity = new AffInstitutionsDetail
//                {
//                    CollegeCode = vm.CollegeCode,
//                    FacultyCode = vm.FacultyCode,
//                    TypeOfInstitution = vm.TypeOfInstitution,
//                    NameOfInstitution = vm.NameOfInstitution,
//                    Address = vm.Address,
//                    VillageTownCity = vm.VillageTownCity,
//                    Taluk = vm.Taluk,
//                    District = vm.District,
//                    PinCode = vm.PinCode,
//                    MobileNumber = vm.MobileNumber,
//                    StdCode = vm.StdCode,
//                    Fax = vm.Fax,
//                    Website = vm.Website,
//                    SurveyNoPidNo = vm.SurveyNoPidNo,
//                    MinorityInstitute = vm.MinorityInstitute,
//                    AttachedToMedicalClg = vm.AttachedToMedicalClg,
//                    RuralInstitute = vm.RuralInstitute,
//                    YearOfEstablishment = yearOfEstablishment,
//                    EmailId = vm.EmailId,
//                    AltLandlineMobile = vm.AltLandlineMobile,
//                    AltEmailId = vm.AltEmailId,
//                    HeadOfInstitution = vm.HeadOfInstitution,
//                    HeadAddress = vm.HeadAddress,
//                    FinancingAuthority = vm.FinancingAuthority,
//                    StatusOfCollege = vm.StatusOfCollege,
//                    CourseApplied = vm.CourseApplied,
//                    DocumentName = docName,
//                    DocumentContentType = docContentType,
//                    DocumentData = docBytes
//                };

//                _context.AffInstitutionsDetails.Add(entity);
//                await _context.SaveChangesAsync();

//                // Use newly created Id
//                var newId = entity.InstitutionId;
//                return RedirectToAction(nameof(AffCourseDetails), new { id = newId });
//            }
//        }




//        [HttpGet]
//        public async Task<IActionResult> DownloadDocument(string collegeCode, string facultyCode)
//        {
//            var entity = await _context.AffInstitutionsDetails
//                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

//            if (entity == null || entity.DocumentData == null || string.IsNullOrEmpty(entity.DocumentName))
//            {
//                return NotFound();
//            }

//            var contentType = string.IsNullOrEmpty(entity.DocumentContentType)
//                ? "application/octet-stream"
//                : entity.DocumentContentType;

//            return File(entity.DocumentData, contentType, entity.DocumentName);
//        }

//        //[HttpGet]
//        //public async Task<IActionResult> DownloadDocument(int id)
//        //{
//        //    var institution = await _context.AffInstitutionsDetails
//        //        .FirstOrDefaultAsync(x => x.InstitutionId == id);

//        //    if (institution?.DocumentData == null || institution.DocumentName == null)
//        //    {
//        //        return NotFound();
//        //    }

//        //    return File(institution.DocumentData,
//        //                institution.DocumentContentType ?? "application/octet-stream",
//        //                institution.DocumentName);
//        //}


//        // Example Details action
//        public async Task<IActionResult> Details1(int id)
//        {
//            var entity = await _context.AffInstitutionsDetails.FindAsync(id);
//            if (entity == null)
//                return NotFound();

//            return View(entity);
//        }


//        [HttpGet]
//        public async Task<IActionResult> AffCourseDetails()
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");

//            // 1) get all courses for this faculty from Mst_Course
//            var mstCourses = await _context.MstCourses
//                .Where(c => c.FacultyCode.ToString() == facultyCode)
//                .OrderBy(c => c.CourseName)
//                .ToListAsync();

//            // 2) get existing AFF_CourseDetails rows
//            var affRows = await _context.AffCourseDetails
//                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
//                .ToListAsync();

//            var vm = new AffCourseDetailsListVM();

//            foreach (var c in mstCourses)
//            {
//                var existing = affRows.FirstOrDefault(x => x.CourseId == c.CourseCode.ToString());

//                vm.Courses.Add(new AffCourseDetailRowVM
//                {
//                    Id = existing?.Id ?? 0,
//                    CollegeCode = collegeCode ?? string.Empty,
//                    FacultyCode = facultyCode ?? string.Empty,
//                    CourseId = c.CourseCode.ToString(),
//                    CourseName = c.CourseName ?? string.Empty,

//                    IsRecognized = existing?.IsRecognized ?? false,
//                    RguhsNotificationNo = existing?.RguhsNotificationNo ?? string.Empty,
//                    ExistingFileUrl = existing != null && existing.DocumentData != null
//                        ? Url.Action("DownloadAffCourseFile", "Course", new { id = existing.Id })
//                        : null,
//                    CreatedOn = existing?.CreatedOn ?? DateTime.UtcNow,
//                    CreatedBy = existing?.CreatedBy ?? string.Empty
//                });
//            }

//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> AffCourseDetails(AffCourseDetailsListVM vm)
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");

//            // Clear existing model state errors first
//            ModelState.Clear();

//            // Custom validation for conditional fields using correct index
//            for (int i = 0; i < vm.Courses.Count; i++)
//            {
//                var row = vm.Courses[i];
//                if (row.IsRecognized && string.IsNullOrWhiteSpace(row.RguhsNotificationNo))
//                {
//                    ModelState.AddModelError($"Courses[{i}].RguhsNotificationNo",
//                        "RGUHS Notification No is required when course is recognized.");
//                }
//            }

//            if (!ModelState.IsValid)
//                return View(vm);

//            foreach (var row in vm.Courses)
//            {
//                AffCourseDetail entity;

//                if (row.Id > 0)
//                {
//                    entity = await _context.AffCourseDetails.FindAsync(row.Id);
//                    if (entity == null) continue;
//                }
//                else
//                {
//                    entity = new AffCourseDetail
//                    {
//                        CollegeCode = collegeCode ?? string.Empty,
//                        FacultyCode = facultyCode ?? string.Empty,
//                        CourseId = row.CourseId,
//                        CourseName = row.CourseName,
//                        CreatedOn = DateTime.UtcNow,
//                        CreatedBy = User.Identity?.Name ?? string.Empty
//                    };
//                    _context.AffCourseDetails.Add(entity);
//                }

//                // Update core fields
//                entity.CourseName = row.CourseName;
//                entity.IsRecognized = row.IsRecognized;
//                entity.RguhsNotificationNo = row.RguhsNotificationNo ?? string.Empty;

//                // Handle file upload
//                if (row.SupportingFile != null && row.SupportingFile.Length > 0)
//                {
//                    using var ms = new MemoryStream();
//                    await row.SupportingFile.CopyToAsync(ms);
//                    entity.DocumentData = ms.ToArray();
//                }

//                // Update audit fields for existing records
//                if (row.Id > 0)
//                {
//                    entity.CreatedBy = row.CreatedBy ?? string.Empty;
//                }
//            }

//            await _context.SaveChangesAsync();
//            TempData["Message"] = "Course details saved successfully.";
//            return RedirectToAction(nameof(AFF_SanIntake));
//        }

//        [HttpGet]
//        public async Task<IActionResult> DownloadAffCourseFile(int id)
//        {
//            var courseDetail = await _context.AffCourseDetails
//                .Where(x => x.Id == id)
//                .FirstOrDefaultAsync();

//            if (courseDetail == null || courseDetail.DocumentData == null || courseDetail.DocumentData.Length == 0)
//            {
//                TempData["Error"] = "Document not found.";
//                return NotFound();
//            }

//            return File(courseDetail.DocumentData,
//                        "application/octet-stream",  // Generic MIME type
//                        $"RGUHS_Course_{courseDetail.CourseId}_{courseDetail.RguhsNotificationNo ?? "Doc"}.pdf");
//        }

//        // Update: set ExistingIntakeId in the VM so view shows the "View" button when a saved row has a file
//        [HttpGet]
//        public async Task<IActionResult> AFF_SanIntake()
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode")
//                              ?? User?.FindFirst("FacultyCode")?.Value;

//            var collegeCode = HttpContext.Session.GetString("CollegeCode")
//                              ?? User?.FindFirst("CollegeCode")?.Value;

//            if (string.IsNullOrWhiteSpace(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login", "Account");
//            }

//            // All courses for this faculty
//            var courses = await _context.MstCourses
//                .Where(c => c.FacultyCode.ToString() == facultyCode)
//                .OrderBy(c => c.CourseName)
//                .ToListAsync();

//            // Existing sanctioned intake per course for this college+faculty
//            var existing = await _context.AffSanctionedIntakeForCourses
//                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
//                .ToListAsync();

//            var vm = new SanctionedIntakeCreateVm
//            {
//                Courses = courses.Select(c =>
//                {
//                    var existingRow = existing
//                        // prefer match by CourseName if that's what DB stores; adjust to CourseCode if available
//                        .FirstOrDefault(e => e.CourseName == c.CourseName);

//                    return new SanctionedIntakeRowVm
//                    {
//                        CourseCode = c.CourseCode.ToString(),
//                        CourseName = c.CourseName ?? string.Empty,
//                        SanctionedIntake = existingRow?.SanctionedIntake,
//                        EligibleSeatSlab = existingRow?.EligibleSeatSlab,
//                        ExistingIntakeId = existingRow?.Id   // <- important: set this so view can render "View" link
//                    };
//                }).ToList()
//            };

//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> AFF_SanIntake(SanctionedIntakeCreateVm model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode")
//                              ?? User?.FindFirst("CollegeCode")?.Value;
//            var facultyCode = HttpContext.Session.GetString("FacultyCode")
//                              ?? User?.FindFirst("FacultyCode")?.Value;

//            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login", "Account");
//            }

//            // basic validation: at least one row has intake filled
//            if (model.Courses == null || !model.Courses.Any(r => !string.IsNullOrWhiteSpace(r.SanctionedIntake)))
//            {
//                ModelState.AddModelError(string.Empty, "Enter sanctioned intake for at least one course.");
//            }

//            if (!ModelState.IsValid)
//            {
//                // re-load course names in case they were not posted
//                var courses = await _context.MstCourses
//                    .Where(c => c.FacultyCode.ToString() == facultyCode)
//                    .OrderBy(c => c.CourseName)
//                    .ToListAsync();

//                foreach (var row in model.Courses)
//                {
//                    var course = courses.FirstOrDefault(c => c.CourseCode.ToString() == row.CourseCode);
//                    if (course != null)
//                        row.CourseName = course.CourseName ?? row.CourseName;
//                }

//                return View(model);
//            }

//            try
//            {
//                foreach (var row in model.Courses.Where(r => !string.IsNullOrWhiteSpace(r.SanctionedIntake)))
//                {
//                    var course = await _context.MstCourses.FirstOrDefaultAsync(c =>
//                        c.CourseCode.ToString() == row.CourseCode &&
//                        c.FacultyCode.ToString() == facultyCode);

//                    if (course == null)
//                        continue;

//                    byte[]? documentBytes = null;
//                    if (row.DocumentFile != null && row.DocumentFile.Length > 0)
//                    {
//                        using var ms = new MemoryStream();
//                        await row.DocumentFile.CopyToAsync(ms);
//                        documentBytes = ms.ToArray();
//                    }

//                    var entity = new AffSanctionedIntakeForCourse
//                    {
//                        CollegeCode = collegeCode,
//                        FacultyCode = facultyCode,
//                        CourseName = course.CourseName ?? row.CourseName,
//                        SanctionedIntake = row.SanctionedIntake ?? string.Empty,
//                        EligibleSeatSlab = row.EligibleSeatSlab,
//                        DocumentData = documentBytes,
//                        CreatedOn = DateTime.UtcNow,
//                        CreatedBy = HttpContext.Session.GetString("UserName") ?? User?.Identity?.Name
//                    };

//                    _context.AffSanctionedIntakeForCourses.Add(entity);
//                }

//                await _context.SaveChangesAsync();
//                TempData["Success"] = "Sanctioned intake saved for selected courses.";
//                return RedirectToAction(nameof(Nursing_LandAndBuildingDetails));
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"AFF_SanIntake save error: {ex}");
//                TempData["Error"] = "Unable to save sanctioned intake. " + ex.Message;

//                var courses = await _context.MstCourses
//                    .Where(c => c.FacultyCode.ToString() == facultyCode)
//                    .OrderBy(c => c.CourseName)
//                    .ToListAsync();

//                foreach (var row in model.Courses)
//                {
//                    var course = courses.FirstOrDefault(c => c.CourseCode.ToString() == row.CourseCode);
//                    if (course != null)
//                        row.CourseName = course.CourseName ?? row.CourseName;
//                }

//                return View(model);
//            }
//        }


//        // New: download saved sanctioned intake document by PK (ExistingIntakeId from the view)
//        [HttpGet]
//        public async Task<IActionResult> DownloadAffSanIntakeFile(int id)
//        {
//            var row = await _context.AffSanctionedIntakeForCourses
//                .AsNoTracking()
//                .FirstOrDefaultAsync(x => x.Id == id);

//            if (row == null || row.DocumentData == null || row.DocumentData.Length == 0)
//            {
//                TempData["Error"] = "Document not found.";
//                return NotFound();
//            }

//            // Use stored content type if you saved it; fall back to octet-stream
//            var contentType = "application/octet-stream";
//            var safeName = string.IsNullOrWhiteSpace(row.CourseName) ? $"SanctionedIntake_{id}" : row.CourseName;
//            var fileName = $"{safeName}.pdf"; // adjust extension if you store original filename

//            return File(row.DocumentData, contentType, fileName);
//        }

//        [HttpGet]
//        public async Task<IActionResult> AdminTeachingBlock()
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? "";
//            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";

//            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login", "Account");
//            }

//            // Load DISTINCT facilities (your deduping is correct)
//            var mstFacilities = await _context.AdministrativeFacilityTypes
//                .OrderBy(f => f.FacilityId)
//                .GroupBy(f => f.FacilityId)  // Dedupe by FacilityId
//                .Select(g => g.First())
//                .ToListAsync();

//            // Load existing data
//            var existing = await _context.AffAdminTeachingBlocks
//                .Where(a => a.FacultyCode == facultyCode && a.CollegeCode == collegeCode)
//                .ToListAsync();

//            var vm = new AdminTeachingBlockListVM();
//            foreach (var f in mstFacilities)
//            {
//                var row = existing.FirstOrDefault(x => x.FacilityId == f.FacilityId.ToString());
//                vm.Rows.Add(new AdminTeachingBlockRowVM
//                {
//                    Id = row?.Id ?? 0,
//                    CollegeCode = collegeCode,
//                    FacultyCode = facultyCode,
//                    FacilityId = f.FacilityId,
//                    Facilities = f.FacilityName,
//                    SizeSqFtAsPerNorms = f.MinAreaSqM.ToString(),
//                    IsAvailable = row?.IsAvailable == "Yes",
//                    NoOfRooms = row?.NoOfRooms ?? "",
//                    SizeSqFtAvailablePerRoom = row?.SizeSqFtAvailablePerRoom ?? ""
//                });
//            }

//            return View(vm);
//        }

//        //csharp Medical_Affiliation\Controllers\NursingContinuesAffiliationController.cs
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [RequestFormLimits(ValueCountLimit = 100000)]
//        public async Task<IActionResult> AdminTeachingBlock(AdminTeachingBlockListVM vm)
//        {
//            if (vm == null || vm.Rows == null || !vm.Rows.Any())
//            {
//                TempData["Error"] = "No rows received. Please check the form.";
//                return RedirectToAction(nameof(AdminTeachingBlock));
//            }

//            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? string.Empty;
//            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? string.Empty;

//            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
//            {
//                TempData["Error"] = "Session expired.";
//                return RedirectToAction("Login", "Account");
//            }

//            TempData["Debug"] = $"Rows count: {vm.Rows.Count}\nSession: College={collegeCode}, Faculty={facultyCode}";

//            var changesCount = 0;

//            try
//            {
//                // --- inside AdminTeachingBlock POST, replace the foreach loop with this ---
//                foreach (var row in vm.Rows)
//                {
//                    // Convert empty strings to null explicitly
//                    var noOfRooms = string.IsNullOrWhiteSpace(row.NoOfRooms?.ToString()) ? (string)null : row.NoOfRooms;
//                    var sizeSqFt = string.IsNullOrWhiteSpace(row.SizeSqFtAvailablePerRoom?.ToString()) ? (string)null : row.SizeSqFtAvailablePerRoom;

//                    if (row.FacilityId <= 0) continue;

//                    var entity = await _context.AffAdminTeachingBlocks
//                        .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode &&
//                                                 x.FacultyCode == facultyCode &&
//                                                 x.FacilityId == row.FacilityId.ToString());

//                    if (entity == null)
//                    {
//                        entity = new AffAdminTeachingBlock
//                        {
//                            CollegeCode = collegeCode,
//                            FacultyCode = facultyCode,
//                            FacilityId = row.FacilityId.ToString(),
//                            Facilities = row.Facilities?.Trim() ?? null,  // NULL if empty
//                            SizeSqFtAsPerNorms = row.SizeSqFtAsPerNorms?.Trim() ?? null,
//                            IsAvailable = row.IsAvailable ? "Yes" : "No",
//                            NoOfRooms = noOfRooms,  // Explicit NULL handling
//                            SizeSqFtAvailablePerRoom = sizeSqFt,  // Explicit NULL handling
//                            CreatedOn = DateTime.UtcNow,
//                            CreatedBy = User.Identity?.Name ?? "Unknown"
//                        };
//                        _context.AffAdminTeachingBlocks.Add(entity);
//                    }
//                    else
//                    {
//                        // Update with null-safe assignments
//                        entity.Facilities = row.Facilities?.Trim() ?? null;
//                        entity.SizeSqFtAsPerNorms = row.SizeSqFtAsPerNorms?.Trim() ?? null;
//                        entity.IsAvailable = row.IsAvailable ? "Yes" : "No";
//                        entity.NoOfRooms = noOfRooms;
//                        entity.SizeSqFtAvailablePerRoom = sizeSqFt;
//                        _context.Entry(entity).State = EntityState.Modified;
//                    }
//                    changesCount++;
//                }


//                var saved = await _context.SaveChangesAsync();
//                TempData["Message"] = $"Saved {saved} records successfully. Changes: {changesCount}";
//                TempData["Debug"] += $"\nEF SaveChanges returned: {saved}";
//            }
//            catch (DbUpdateException ex)
//            {
//                TempData["Error"] = $"DB Error: {ex.InnerException?.Message ?? ex.Message}";
//                TempData["Debug"] += $"\nDB Error: {ex}";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Error: {ex.Message}";
//                TempData["Debug"] += $"\nError: {ex}";
//            }

//            return RedirectToAction(nameof(AdminTeachingBlock));
//        }


//        [HttpGet]
//        public IActionResult Aff_HostelDetails()
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            var vm = new AffHostelDetailsCreateVm
//            {
//                HostelTypes = _context.MstHosteltypes
//                    .Select(x => new SelectListItem { Value = x.HospitalType, Text = x.HospitalType })
//                    .ToList()
//            };

//            if (!string.IsNullOrEmpty(collegeCode) && !string.IsNullOrEmpty(facultyCode))
//            {
//                vm.Hostel = _context.AffHostelDetails
//                    .FirstOrDefault(h => h.CollegeCode == collegeCode && h.FacultyCode == facultyCode);

//                vm.Hostel ??= new AffHostelDetail { CollegeCode = collegeCode, FacultyCode = facultyCode };
//            }
//            else
//            {
//                vm.Hostel = new AffHostelDetail();
//            }

//            return View(vm);
//        }
//        [HttpPost]
//        public async Task<IActionResult> Aff_HostelDetails(AffHostelDetailsCreateVm vm, IFormFile possessionFile)
//        {
//            vm.Hostel.CollegeCode = HttpContext.Session.GetString("CollegeCode") ?? string.Empty;
//            vm.Hostel.FacultyCode = HttpContext.Session.GetString("FacultyCode") ?? string.Empty;

//            ModelState.Remove("Hostel.CollegeCode");
//            ModelState.Remove("Hostel.FacultyCode");

//            if (possessionFile != null && possessionFile.Length > 0)
//            {
//                using var ms = new MemoryStream();
//                await possessionFile.CopyToAsync(ms);
//                vm.Hostel.PossessionProofPath = ms.ToArray();
//            }

//            if (!ModelState.IsValid)
//            {
//                vm.HostelTypes = _context.MstHosteltypes
//                    .Select(x => new SelectListItem
//                    {
//                        Value = x.HospitalType,
//                        Text = x.HospitalType
//                    })
//                    .ToList();
//                return View(vm);
//            }

//            var existingHostel = await _context.AffHostelDetails
//                .FirstOrDefaultAsync(h => h.CollegeCode == vm.Hostel.CollegeCode &&
//                                          h.FacultyCode == vm.Hostel.FacultyCode);

//            if (existingHostel != null)
//            {
//                // DO NOT touch HostelDetailsId (key)
//                existingHostel.HostelType = vm.Hostel.HostelType;
//                existingHostel.BuiltUpAreaSqFt = vm.Hostel.BuiltUpAreaSqFt;
//                existingHostel.HasSeparateHostel = vm.Hostel.HasSeparateHostel;
//                existingHostel.SeparateProvisionMaleFemale = vm.Hostel.SeparateProvisionMaleFemale;
//                existingHostel.TotalFemaleStudents = vm.Hostel.TotalFemaleStudents;
//                existingHostel.TotalFemaleRooms = vm.Hostel.TotalFemaleRooms;
//                existingHostel.TotalMaleStudents = vm.Hostel.TotalMaleStudents;
//                existingHostel.TotalMaleRooms = vm.Hostel.TotalMaleRooms;

//                if (vm.Hostel.PossessionProofPath != null)
//                    existingHostel.PossessionProofPath = vm.Hostel.PossessionProofPath;
//            }
//            else
//            {
//                // New record: HostelDetailsId stays default (identity), EF will set it
//                _context.AffHostelDetails.Add(vm.Hostel);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction("HospitalDetailsForAffiliation", "ClinicalFacilitiesForAffiliation");
//        }

//        [HttpGet]
//        public async Task<IActionResult> DownloadPossessionProof(string collegeCode, string facultyCode)
//        {
//            var hostel = await _context.AffHostelDetails
//                .FirstOrDefaultAsync(h => h.CollegeCode == collegeCode && h.FacultyCode == facultyCode);

//            if (hostel?.PossessionProofPath == null) return NotFound();

//            return File(hostel.PossessionProofPath, "application/pdf", "PossessionProof.pdf");
//        }


//        // Fixes to AFF_HostelDetailswithfacilities GET mapping and POST save diagnostics.
//        // - GET: correctly match detail rows by FacilityId (not FacultyCode) and also by college/faculty.
//        // - POST: added lightweight validation/logging to show received rows and IsAvailable values,
//        //   and use SaveChangesAsync for consistency.

//        [HttpGet]
//        public IActionResult AFF_HostelDetailswithfacilities()
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");

//            // master: all hostel facilities
//            var facilities = _context.MstHostelFacilities
//                .Where(e => e.FacultyId.ToString() == facultyCode)
//                .OrderBy(f => f.HostelFacilityId)
//                .ToList();

//            // detail: existing availability for this faculty/college
//            var affRows = _context.AffHostelFacilityDetails
//                .Where(a => a.FacultyCode == facultyCode && a.CollegeCode == collegeCode)
//                .ToList();

//            var vm = new HostelFacilityListVm
//            {
//                FacultyCode = facultyCode,
//                CollegeCode = collegeCode,
//                Rows = facilities
//                    .Select(f =>
//                    {
//                        // CORRECT MATCH: match by FacilityId (int) — previous code compared FacultyCode to HostelFacilityId which is wrong
//                        var aff = affRows.FirstOrDefault(a =>
//                            a.FacilityId == f.HostelFacilityId &&
//                            a.CollegeCode == collegeCode &&
//                            a.FacultyCode == facultyCode);

//                        return new HostelFacilityRowVm
//                        {
//                            HostelFacilityId = f.HostelFacilityId,
//                            HostelFacilityName = f.HostelFacilityName,
//                            FacultyId = f.FacultyId,
//                            AffId = aff?.Id,                     // PK from Aff_ table
//                            FacultyCode = facultyCode,
//                            CollegeCode = collegeCode,
//                            IsAvailable = aff?.IsAvailable ?? false
//                        };
//                    })
//                    .ToList()
//            };

//            return View(vm);
//        }

//        // Replace the POST action for AFF_HostelDetailswithfacilities with this diagnostic-friendly version
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> AFF_HostelDetailswithfacilities(HostelFacilityListVm model)
//        {
//            var form = Request.Form;

//            // Optional debug
//            TempData["Debug_RowsCount_Posted"] = model?.Rows?.Count ?? 0;
//            TempData["Debug_CheckedCount_Posted"] = model?.Rows?.Count(r => r.IsAvailable) ?? 0;

//            // Server-controlled codes from session
//            var facultyCodeSession = HttpContext.Session.GetString("FacultyCode");
//            var collegeCodeSession = HttpContext.Session.GetString("CollegeCode");

//            // Remove server-only fields from validation if needed
//            ModelState.Remove(nameof(model.FacultyCode));
//            ModelState.Remove(nameof(model.CollegeCode));

//            if (!ModelState.IsValid)
//            {
//                // Reload master facilities so view can be rebuilt
//                var facilities = _context.MstHostelFacilities
//                    .Where(e => e.FacultyId.ToString() == facultyCodeSession)
//                    .OrderBy(f => f.HostelFacilityId)
//                    .ToList();

//                model.Rows = facilities.Select(f =>
//                {
//                    var postedRow = model.Rows?.FirstOrDefault(r => r.HostelFacilityId == f.HostelFacilityId);
//                    return new HostelFacilityRowVm
//                    {
//                        HostelFacilityId = f.HostelFacilityId,
//                        HostelFacilityName = f.HostelFacilityName,
//                        FacultyId = f.FacultyId,
//                        FacultyCode = facultyCodeSession,
//                        CollegeCode = collegeCodeSession,
//                        AffId = postedRow?.AffId,
//                        IsAvailable = postedRow?.IsAvailable ?? false
//                    };
//                }).ToList();

//                return View(model);
//            }

//            try
//            {
//                // Defensive: if list not bound, rebuild from Request.Form
//                if ((model.Rows == null || !model.Rows.Any()) && form.Keys.Any(k => k.StartsWith("Rows[")))
//                {
//                    var built = new List<HostelFacilityRowVm>();

//                    var indices = form.Keys
//                        .Where(k => k.StartsWith("Rows["))
//                        .Select(k =>
//                        {
//                            var idxPart = k.Substring("Rows[".Length);
//                            var idx = idxPart.Split(']').FirstOrDefault();
//                            return idx;
//                        })
//                        .Where(s => int.TryParse(s, out _))
//                        .Select(int.Parse)
//                        .Distinct()
//                        .OrderBy(i => i)
//                        .ToList();

//                    foreach (var i in indices)
//                    {
//                        var prefix = $"Rows[{i}]";

//                        var idStr = form[$"{prefix}.HostelFacilityId"].FirstOrDefault();
//                        if (!int.TryParse(idStr, out var hid)) continue;

//                        // "true,false" when checked, "false" when not checked [web:35][web:61]
//                        var rawIsAvailable = form[$"{prefix}.IsAvailable"].FirstOrDefault() ?? string.Empty;
//                        var isAvailable = rawIsAvailable
//                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
//                            .Any(v => v.Trim().Equals("true", StringComparison.OrdinalIgnoreCase));

//                        built.Add(new HostelFacilityRowVm
//                        {
//                            HostelFacilityId = hid,
//                            HostelFacilityName = form[$"{prefix}.HostelFacilityName"].FirstOrDefault() ?? string.Empty,
//                            FacultyCode = facultyCodeSession ?? form[$"{prefix}.FacultyCode"].FirstOrDefault(),
//                            CollegeCode = collegeCodeSession ?? form[$"{prefix}.CollegeCode"].FirstOrDefault(),
//                            AffId = int.TryParse(form[$"{prefix}.AffId"].FirstOrDefault(), out var aid) ? aid : (int?)null,
//                            IsAvailable = isAvailable
//                        });
//                    }

//                    model.Rows = built;
//                    TempData["Debug_RowsCount_Rebuilt"] = model.Rows.Count;
//                    TempData["Debug_CheckedCount_Rebuilt"] = model.Rows.Count(r => r.IsAvailable);
//                }

//                if (model.Rows == null || !model.Rows.Any())
//                {
//                    TempData["Error"] = "No facility rows were received.";
//                    return RedirectToAction(nameof(AFF_HostelDetailswithfacilities));
//                }

//                foreach (var row in model.Rows)
//                {
//                    AffHostelFacilityDetail entity = null;

//                    if (row.AffId.HasValue && row.AffId.Value > 0)
//                    {
//                        entity = await _context.AffHostelFacilityDetails
//                            .FirstOrDefaultAsync(a => a.Id == row.AffId.Value);
//                    }

//                    if (entity == null)
//                    {
//                        entity = await _context.AffHostelFacilityDetails
//                            .FirstOrDefaultAsync(a =>
//                                a.FacilityId == row.HostelFacilityId &&
//                                a.CollegeCode == collegeCodeSession &&
//                                a.FacultyCode == facultyCodeSession);
//                    }

//                    if (entity == null)
//                    {
//                        entity = new AffHostelFacilityDetail
//                        {
//                            FacilityId = row.HostelFacilityId,
//                            FacultyCode = facultyCodeSession ?? row.FacultyCode ?? string.Empty,
//                            CollegeCode = collegeCodeSession ?? row.CollegeCode ?? string.Empty,
//                            FacilityName = row.HostelFacilityName ?? string.Empty,
//                            IsAvailable = row.IsAvailable   // bool → BIT 1/0 [web:53][web:67]
//                        };
//                        _context.AffHostelFacilityDetails.Add(entity);
//                    }

//                    // Update availability
//                    entity.IsAvailable = row.IsAvailable;
//                }

//                await _context.SaveChangesAsync();
//                TempData["Success"] = "Hostel facilities updated successfully.";
//            }
//            catch (DbUpdateException ex)
//            {
//                TempData["Error"] = $"Database error: {ex.InnerException?.Message ?? ex.Message}";
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Unexpected error: {ex.Message}";
//            }

//            return RedirectToAction(nameof(AFF_HostelDetailswithfacilities));
//        }

//        public IActionResult Nursing_LandDetails()
//        {
//            return View();
//        }

//        public IActionResult Nursing_LandAndBuildingDetails()
//        {
//            var facultyCode = HttpContext.Session.GetString("FacultyCode"); // or however you store it

//            var req = _context.MstBuildingDetailRequireds
//                              .FirstOrDefault(x => x.FacultyCode == facultyCode);




//            // Only one record exists, so fetch the first one
//            var data = _context.LandBuildingDetails.FirstOrDefault();

//            LandBuildingDetailsVM vm = new LandBuildingDetailsVM();

//            if (data != null)
//            {
//                vm.AgreeTerms = data.AgreeTerms ?? false;

//                vm.RequiredBuildingArea = req?.BuildingAreaRequiredSqFt ?? 0;
//                vm.LandAcres = data.LandAcres ?? 0;
//                vm.BuildingArea = data.BuildingArea ?? 0;
//                vm.BuildingType = data.BuildingType;
//                vm.OwnerName = data.OwnerName;

//                vm.CourtCase = data.CourtCase;
//                vm.CoursesInBuilding = data.CoursesInBuilding;

//                vm.Classrooms = data.Classrooms ?? 0;
//                vm.Labs = data.Labs ?? 0;

//                vm.Survey = data.Survey;
//                vm.RR = data.Rr;

//                vm.WaterSupply = data.WaterSupply;
//                vm.Auditorium = data.Auditorium;
//                vm.OfficeFacilities = data.OfficeFacilities;
//                vm.Seating = data.Seating;
//                vm.Electricity = data.Electricity;

//                vm.BlueprintCertNo = data.BlueprintCertNo;
//                vm.ApprovalCertNo = data.ApprovalCertNo;
//                vm.TaxCertNo = data.TaxCertNo;
//                vm.RTCCertNo = data.RtccertNo;
//                vm.OccupancyCertNo = data.OccupancyCertNo;
//                vm.SaleDeedCertNo = data.SaleDeedCertNo;
//                vm.BlueprintPdf = data.BlueprintDoc;
//                vm.ApprovalCertPdf = data.ApprovalCert;
//                vm.TaxReceiptPdf = data.TaxReceipt;
//                vm.RTCPdf = data.Rtc;
//                vm.OccupancyCertPdf = data.OccupancyCert;
//                vm.SaleDeedPdf = data.SaleDeed;

//            }
//            ViewBag.FormSaved = true;
//            return View(vm);
//        }

//        [HttpPost]
//        public IActionResult SaveLandBuilding(LandBuildingDetailsVM model)
//        {
//            // Fetch existing record (only one allowed)
//            var entity = _context.LandBuildingDetails.FirstOrDefault();

//            // Create new if not found
//            if (entity == null)
//            {
//                entity = new LandBuildingDetail
//                {
//                    CreatedDate = DateTime.Now
//                };

//                _context.LandBuildingDetails.Add(entity);
//            }

//            // Update all fields ALWAYS (insert or update)
//            entity.AgreeTerms = model.AgreeTerms;
//            entity.LandAcres = model.LandAcres;
//            entity.BuildingArea = model.BuildingArea;
//            entity.BuildingType = model.BuildingType;
//            entity.OwnerName = model.OwnerName;

//            entity.CourtCase = model.CourtCase;
//            entity.CoursesInBuilding = model.CoursesInBuilding;

//            entity.Classrooms = model.Classrooms;
//            entity.Labs = model.Labs;

//            entity.Survey = model.Survey;
//            entity.Rr = model.RR;

//            entity.WaterSupply = model.WaterSupply;
//            entity.Auditorium = model.Auditorium;
//            entity.OfficeFacilities = model.OfficeFacilities;
//            entity.Seating = model.Seating;
//            entity.Electricity = model.Electricity;

//            // Certificate numbers
//            entity.BlueprintCertNo = model.BlueprintCertNo;
//            entity.ApprovalCertNo = model.ApprovalCertNo;
//            entity.TaxCertNo = model.TaxCertNo;
//            entity.RtccertNo = model.RTCCertNo;
//            entity.OccupancyCertNo = model.OccupancyCertNo;
//            entity.SaleDeedCertNo = model.SaleDeedCertNo;

//            entity.UpdatedDate = DateTime.Now;

//            // PDF upload handling
//            if (model.BlueprintDoc != null)
//                entity.BlueprintDoc = ConvertToBytes(model.BlueprintDoc);

//            if (model.ApprovalCert != null)
//                entity.ApprovalCert = ConvertToBytes(model.ApprovalCert);

//            if (model.TaxReceipt != null)
//                entity.TaxReceipt = ConvertToBytes(model.TaxReceipt);

//            if (model.RTC != null)
//                entity.Rtc = ConvertToBytes(model.RTC);

//            if (model.OccupancyCert != null)
//                entity.OccupancyCert = ConvertToBytes(model.OccupancyCert);

//            if (model.SaleDeed != null)
//                entity.SaleDeed = ConvertToBytes(model.SaleDeed);

//            _context.SaveChanges();

//            TempData["Success"] = "Building details saved successfully!";

//            //return RedirectToAction("ClassroomAndLaboratoryDetails");
//            return RedirectToAction("Nursing_LandAndBuildingDetails");
//        }

//        // 🔥 Helper method to convert PDF to byte[]
//        private byte[] ConvertToBytes(IFormFile file)
//        {
//            using (var ms = new MemoryStream())
//            {
//                file.CopyTo(ms);
//                return ms.ToArray();
//            }
//        }


//        public IActionResult ViewPdf(string type)
//        {
//            var data = _context.LandBuildingDetails.FirstOrDefault();
//            if (data == null) return NotFound();

//            byte[] fileBytes = null;

//            switch (type)
//            {
//                case "Blueprint":
//                    fileBytes = data.BlueprintDoc;
//                    break;

//                case "Approval":
//                    fileBytes = data.ApprovalCert;
//                    break;

//                case "Tax":
//                    fileBytes = data.TaxReceipt;
//                    break;

//                case "RTC":
//                    fileBytes = data.Rtc;
//                    break;

//                case "Occupancy":
//                    fileBytes = data.OccupancyCert;
//                    break;

//                case "SaleDeed":
//                    fileBytes = data.SaleDeed;
//                    break;
//            }

//            if (fileBytes == null) return NotFound("PDF not uploaded");

//            return File(fileBytes, "application/pdf");
//        }

//        public async Task<IActionResult> ClassroomAndLaboratoryDetails()
//        {
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(facultyCode))
//                return RedirectToAction("Login", "Account");

//            // ⭐ GET AGREE TERMS FROM Tbl_ClassroomAvailability
//            bool agreeTerms = await _context.TblClassroomAvailabilities
//                                    .Where(x => x.FacultyCode == facultyCode)
//                                    .Select(x => x.AgreeTerms ?? false)
//                                    .FirstOrDefaultAsync();

//            // 1️⃣ Get required data from master table + course names
//            var masterData =
//                await (from cls in _context.MstClassroomDetails
//                       join crs in _context.MstCourses
//                           on new
//                           {
//                               CC = cls.CourseCode,
//                               FC = cls.FacultyCode
//                           }
//                           equals new
//                           {
//                               CC = crs.CourseCode.ToString(),
//                               FC = crs.FacultyCode.HasValue ? crs.FacultyCode.Value.ToString() : ""
//                           }
//                       where cls.FacultyCode == facultyCode
//                       select new
//                       {

//                           cls.CourseCode,
//                           crs.CourseName,
//                           cls.NoOfClassrooms,
//                           cls.SizeOfClassrooms
//                       }).ToListAsync();


//            // 2️⃣ Get saved availability from Tbl_ClassroomAvailability
//            var savedData = await _context.TblClassroomAvailabilities
//                                .Where(a => a.FacultyCode == facultyCode)
//                                .ToListAsync();


//            // 3️⃣ Merge master + saved values
//            var classroomData = masterData.Select(m => new ClassroomDetailVM
//            {

//                CourseCode = m.CourseCode,
//                CourseName = m.CourseName,

//                RequiredClassrooms = m.NoOfClassrooms,
//                RequiredSize = string.IsNullOrEmpty(m.SizeOfClassrooms)
//                                ? 0
//                                : Convert.ToInt32(m.SizeOfClassrooms),

//                AvailableClassrooms = savedData
//                    .FirstOrDefault(s => s.CourseCode == m.CourseCode)?.NoOfClassroomsAvailable,

//                AvailableSize = savedData
//                    .FirstOrDefault(s => s.CourseCode == m.CourseCode)?.SizeAvailableSqFt

//            }).ToList();


//            // ------------------ LABORATORY DATA ------------------
//            var labData =
//                await _context.MstLaboratories
//                    .Where(x => x.Facultyid.ToString() == facultyCode && x.NoOfLabs == 1)
//                    .Select(x => new LaboratoryDetailVM
//                    {
//                        LabID = x.LabId,
//                        LaboratoryName = x.Laboratories,
//                        RequiredLabs = x.NoOfLabs ?? 0,
//                        RequiredSize = Convert.ToInt32(x.SizeofLab),

//                        //AvailableSize = null,
//                        //AvailableMannequin = null

//                        AvailableSize = (
//                                            from a in _context.TblLaboratoryAvailabilities
//                                            where a.FacultyCode == facultyCode && a.LabId == x.LabId
//                                            select a.SizeAvailableSqFt
//                                        ).FirstOrDefault(),
//                        AvailableMannequin = (
//                                                from a in _context.TblLaboratoryAvailabilities
//                                                where a.FacultyCode == facultyCode && a.LabId == x.LabId
//                                                select a.MannequinAvailable
//                                            ).FirstOrDefault(),

//                    })
//                    .Distinct()
//                    .ToListAsync();

//            // 4️⃣ Send to ViewModel
//            var vm = new ClassroomPageVM
//            {
//                FacultyCode = facultyCode,
//                ClassroomDetails = classroomData,
//                LaboratoryDetails = labData,
//                //AgreeTerms = agreeTerms,  // ⭐ POPULATE HERE
//            };

//            ViewBag.FormSaved = TempData["FormSaved"] ?? "false";
//            ViewBag.Success = TempData["Success"] ?? null;

//            return View(vm);
//        }

//        [HttpPost]
//        public async Task<IActionResult> ClassroomAndLaboratoryDetails(ClassroomPageVM model)
//        {
//            if (model == null || model.ClassroomDetails == null)
//                return BadRequest("Invalid data.");

//            string facultyCode = HttpContext.Session.GetString("FacultyCode");
//            if (string.IsNullOrEmpty(facultyCode))
//                return RedirectToAction("Login", "Account");

//            foreach (var item in model.ClassroomDetails)
//            {
//                // Find if already exists
//                var existing = await _context.TblClassroomAvailabilities
//                    .FirstOrDefaultAsync(x =>
//                        x.FacultyCode == facultyCode &&
//                        x.CourseCode == item.CourseCode);

//                if (existing == null)
//                {
//                    // Insert new
//                    var entity = new TblClassroomAvailability
//                    {
//                        FacultyCode = facultyCode,
//                        CourseCode = item.CourseCode,

//                        NoOfClassroomsAvailable = item.AvailableClassrooms,
//                        SizeAvailableSqFt = item.AvailableSize
//                    };

//                    _context.TblClassroomAvailabilities.Add(entity);
//                }
//                else
//                {
//                    // Update existing
//                    existing.NoOfClassroomsAvailable = item.AvailableClassrooms;
//                    existing.SizeAvailableSqFt = item.AvailableSize;

//                    _context.TblClassroomAvailabilities.Update(existing);
//                }
//            }

//            // ------------- SAVE LABORATORY AVAILABILITY ----------------
//            foreach (var lab in model.LaboratoryDetails)
//            {
//                var existing = await _context.TblLaboratoryAvailabilities
//                    .FirstOrDefaultAsync(x =>
//                        x.LabId == lab.LabID &&
//                        x.FacultyCode == facultyCode);   // FIXED

//                if (existing == null)
//                {
//                    // INSERT
//                    var newRow = new TblLaboratoryAvailability
//                    {
//                        FacultyCode = facultyCode,
//                        LabId = lab.LabID,
//                        SizeAvailableSqFt = lab.AvailableSize ?? 0,
//                        MannequinAvailable = lab.AvailableMannequin ?? 0,
//                        CreatedDate = DateTime.Now
//                    };

//                    _context.TblLaboratoryAvailabilities.Add(newRow);
//                }
//                else
//                {
//                    // UPDATE
//                    existing.SizeAvailableSqFt = lab.AvailableSize ?? 0;
//                    existing.MannequinAvailable = lab.AvailableMannequin ?? 0;
//                    existing.UpdatedDate = DateTime.Now;
//                }
//            }

//            await _context.SaveChangesAsync();

//            TempData["Success"] = "Classroom details saved successfully!";
//            // Used to let client-side know the form was saved
//            TempData["FormSaved"] = "true";

//            return RedirectToAction("ClassroomAndLaboratoryDetails"); // Replace with your next step
//        }


//        public async Task<IActionResult> Nursing_EquipmentDetails()
//        {
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(facultyCode))
//                return RedirectToAction("Login", "Account");

//            var equipments = await _context.TblEquipmentDetails
//                                           .Where(e => e.FacultyCode == facultyCode)
//                                           .Select(e => new EquipmentVM
//                                           {
//                                               ID = e.Id,
//                                               EquipmentName = e.EquipmentName,
//                                               Make = e.Make,
//                                               Model = e.Model
//                                           })
//                                           .ToListAsync();

//            return View(equipments);
//        }


//        [HttpPost]
//        public async Task<IActionResult> SaveEquipment(string EquipmentName, string Make, string Model)
//        {
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(facultyCode))
//                return Json(new { success = false });

//            var entity = new TblEquipmentDetail
//            {
//                FacultyCode = facultyCode,
//                EquipmentName = EquipmentName,
//                Make = Make,
//                Model = Model,
//                CreatedDate = DateTime.Now
//            };

//            _context.TblEquipmentDetails.Add(entity);
//            await _context.SaveChangesAsync();

//            return Json(new { success = true, id = entity.Id });
//        }


//        [HttpPost]
//        public async Task<IActionResult> DeleteEquipment(int id)
//        {
//            var item = await _context.TblEquipmentDetails.FindAsync(id);

//            if (item == null)
//                return Json(new { success = false });

//            _context.TblEquipmentDetails.Remove(item);
//            await _context.SaveChangesAsync();

//            return Json(new { success = true });
//        }

//        public IActionResult Declaration()
//        {
//            var model = new DeclarationViewModel
//            {
//                IsPenaltyEnabled = true,
//                SubmittedOn = DateTime.UtcNow,          // or from DB
//                UniversityName = "ABC University",      // from DB/config
//                DownloadUrl = Url.Action("DownloadApp") // your action
//            };
//            return View(model);
//        }

//        [HttpGet]
//        public IActionResult AFF_TeachingFacultyDetails()
//        {
//            string collegeCode = HttpContext.Session.GetString("CollegeCode");
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please log in again.";
//                return RedirectToAction("Login", "Account");
//            }

//            // Dropdown lists
//            var subjectsList = _context.MstCourses
//                .Where(c => c.FacultyCode.ToString() == facultyCode)
//                .Select(c => new SelectListItem
//                {
//                    Value = c.CourseCode.ToString(),
//                    Text = c.CourseName ?? ""
//                })
//                .Distinct()
//                .ToList();

//            var designationsList = _context.DesignationMasters
//                .Where(c => c.FacultyCode.ToString() == facultyCode)
//                .Select(d => new SelectListItem
//                {
//                    Value = d.DesignationCode,
//                    Text = d.DesignationName ?? ""
//                })
//                .ToList();

//            var departmentsList = _context.MstCourses
//                .Where(e => e.FacultyCode.ToString() == facultyCode)
//                .Select(d => new SelectListItem
//                {
//                    Value = d.CourseCode.ToString(),
//                    Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
//                })
//                .ToList();

//            // ✅ Get ALL faculty records from new table
//            var facultyDetails = _context.AffTeachingFacultyAllDetails
//                .Where(f => f.CollegeCode == collegeCode
//                            && f.FacultyCode == facultyCode
//                            && f.IsRemoved != true)
//                .Take(1000)
//                .ToList();

//            List<Aff_FacultyDetailsViewModel> vmList = new List<Aff_FacultyDetailsViewModel>();

//            if (!facultyDetails.Any())
//            {
//                TempData["Info"] = "No faculty records found for this faculty.";
//                vmList.Add(new Aff_FacultyDetailsViewModel
//                {
//                    Subjects = subjectsList,
//                    Designations = designationsList,
//                    DepartmentDetailsList = departmentsList
//                });
//                return View(vmList);
//            }

//            // ✅ Complete mapping of ALL fields
//            vmList = facultyDetails.Select(f => new Aff_FacultyDetailsViewModel
//            {
//                FacultyDetailId = f.TeachingFacultyId,
//                NameOfFaculty = f.TeachingFacultyName,
//                Subject = f.Subject,
//                DateOfBirth = f.DateOfBirth == default ? (DateTime?)null : f.DateOfBirth.ToDateTime(TimeOnly.MinValue),
//                Mobile = f.Mobile,
//                Email = f.Email,
//                Aadhaar = f.AadhaarNumber,
//                PAN = f.Pannumber,
//                RN_RMNumber = f.RnRmnumber,
//                Department = f.Department,
//                DepartmentDetails = f.DepartmentDetails,
//                SelectedDepartment = f.DepartmentDetails,
//                Designation = f.Designation,
//                Qualification = f.Qualification,
//                UGInstituteName = f.UginstituteName,
//                UGYearOfPassing = f.UgyearOfPassing,
//                PGInstituteName = f.PginstituteName,
//                PGYearOfPassing = f.PgyearOfPassing,
//                PGPassingSpecialization = f.PgpassingSpecialization,
//                TeachingExperienceAfterUGYears = f.TeachingExperienceAfterUgyears,
//                TeachingExperienceAfterPGYears = f.TeachingExperienceAfterPgyears,
//                DateOfJoining = f.DateOfJoining == default ? (DateTime?)null : f.DateOfJoining.ToDateTime(TimeOnly.MinValue),
//                RecognizedPgTeacher = (bool)f.RecognizedPgTeacher,
//                RecognizedPhDTeacher = (bool)f.RecognizedPhDteacher,
//                IsRecognizedPGGuide = (bool)f.IsRecognizedPgguide,
//                IsExaminer = (bool)f.IsExaminer,
//                ExaminerFor = f.ExaminerFor,
//                ExaminerForList = !string.IsNullOrEmpty(f.ExaminerFor)
//                    ? f.ExaminerFor.Split(',').ToList()
//                    : new List<string>(),
//                LitigationPending = (bool)f.LitigationPending,
//                NRTSNumber = f.Nrtsnumber,
//                RGUHSTIN = f.Rguhstin,
//                RemoveRemarks = f.RemoveRemarks,
//                IsRemoved = f.IsRemoved,

//                Subjects = subjectsList,
//                Designations = designationsList,
//                DepartmentDetailsList = departmentsList
//            }).ToList();

//            return View(vmList);
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult AFF_TeachingFacultyDetails(IList<Aff_FacultyDetailsViewModel> model)
//        {
//            string collegeCode = HttpContext.Session.GetString("CollegeCode");
//            string facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please log in again.";
//                return RedirectToAction("Login", "Account");
//            }

//            if (model == null || !model.Any())
//            {
//                TempData["Error"] = "No data to save.";
//                // Repopulate dropdowns...
//                return View(model);
//            }

//            using (var transaction = _context.Database.BeginTransaction())
//            {
//                try
//                {
//                    var incomingIds = model
//                        .Where(m => m.FacultyDetailId > 0)
//                        .Select(m => m.FacultyDetailId)
//                        .ToHashSet();

//                    var existingFaculty = _context.AffTeachingFacultyAllDetails
//                        .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
//                        .ToList();

//                    foreach (var m in model)
//                    {
//                        // ✅ Convert file uploads
//                        byte[] aadhaarDoc = m.AadhaarDocument != null ? ConvertFileToBytes(m.AadhaarDocument) : null;
//                        byte[] panDoc = m.PANDocument != null ? ConvertFileToBytes(m.PANDocument) : null;
//                        byte[] rnRmDoc = m.RN_RMDocument != null ? ConvertFileToBytes(m.RN_RMDocument) : null;
//                        byte[] form16Doc = m.Form16OrLast6MonthsStatement != null ? ConvertFileToBytes(m.Form16OrLast6MonthsStatement) : null;
//                        byte[] appointmentDoc = m.AppointmentOrderDocument != null ? ConvertFileToBytes(m.AppointmentOrderDocument) : null;
//                        byte[] guideDoc = m.GuideRecognitionDoc != null ? ConvertFileToBytes(m.GuideRecognitionDoc) : null;
//                        byte[] phdDoc = m.PhDRecognitionDoc != null ? ConvertFileToBytes(m.PhDRecognitionDoc) : null;
//                        byte[] litigationDoc = m.LitigationDoc != null ? ConvertFileToBytes(m.LitigationDoc) : null;
//                        byte[] OnlineTeachersDatabase = m.OnlineTeachersDatabase != null ? ConvertFileToBytes(m.OnlineTeachersDatabase) : null;
//                        byte[] madetorecruit = m.madetorecruit != null ? ConvertFileToBytes(m.madetorecruit) : null;

//                        var existing = existingFaculty.FirstOrDefault(f => f.TeachingFacultyId == m.FacultyDetailId);

//                        if (existing != null)
//                        {
//                            // ✅ UPDATE all fields
//                            existing.TeachingFacultyName = m.NameOfFaculty?.Trim() ?? "N/A";
//                            existing.Subject = m.Subject?.Trim() ?? "N/A";
//                            // Replace this line in the POST AFF_TeachingFacultyDetails action (inside the foreach loop for updating existing records):
//                            // existing.DateOfBirth = m.DateOfBirth;

//                            // With the following code to properly convert nullable DateTime? to DateOnly:
//                            existing.DateOfBirth = m.DateOfBirth.HasValue ? DateOnly.FromDateTime(m.DateOfBirth.Value) : default;
//                            //existing.DateOfBirth = m.DateOfBirth;
//                            existing.Mobile = m.Mobile?.Trim() ?? "N/A";
//                            existing.Email = m.Email?.Trim() ?? "N/A";
//                            existing.AadhaarNumber = m.Aadhaar?.Trim() ?? "N/A";
//                            existing.Pannumber = m.PAN?.Trim() ?? "N/A";
//                            existing.RnRmnumber = m.RN_RMNumber?.Trim() ?? "N/A";
//                            existing.Department = m.Department?.Trim() ?? "N/A";
//                            existing.DepartmentDetails = m.DepartmentDetails?.Trim() ?? "N/A";
//                            existing.Designation = m.Designation?.Trim() ?? "N/A";
//                            existing.Qualification = m.Qualification?.Trim() ?? "N/A";
//                            existing.UginstituteName = m.UGInstituteName?.Trim() ?? "N/A";
//                            existing.UgyearOfPassing = (int)m.UGYearOfPassing;
//                            existing.PginstituteName = m.PGInstituteName?.Trim() ?? "N/A";
//                            existing.PgyearOfPassing = (int)m.PGYearOfPassing;
//                            existing.PgpassingSpecialization = m.PGPassingSpecialization?.Trim() ?? "N/A";
//                            existing.TeachingExperienceAfterUgyears = m.TeachingExperienceAfterUGYears;
//                            existing.TeachingExperienceAfterPgyears = m.TeachingExperienceAfterPGYears;
//                            existing.DateOfJoining = m.DateOfJoining.HasValue ? DateOnly.FromDateTime(m.DateOfJoining.Value) : default;
//                            existing.RecognizedPgTeacher = m.RecognizedPgTeacher;
//                            existing.RecognizedPhDteacher = m.RecognizedPhDTeacher;
//                            existing.IsRecognizedPgguide = m.IsRecognizedPGGuide;
//                            existing.IsExaminer = m.IsExaminer;
//                            existing.ExaminerFor = m.ExaminerForList != null ? string.Join(",", m.ExaminerForList) : null;
//                            existing.LitigationPending = m.LitigationPending;
//                            existing.Nrtsnumber = m.NRTSNumber?.Trim() ?? "N/A";
//                            existing.Rguhstin = m.RGUHSTIN?.Trim() ?? "N/A";
//                            existing.RemoveRemarks = m.RemoveRemarks;
//                            existing.IsRemoved = !string.IsNullOrWhiteSpace(m.RemoveRemarks);

//                            // Update documents if new files uploaded
//                            if (aadhaarDoc != null) existing.AadhaarDocument = aadhaarDoc;
//                            if (panDoc != null) existing.Pandocument = panDoc;
//                            if (rnRmDoc != null) existing.RnRmdocument = rnRmDoc;
//                            if (form16Doc != null) existing.Form16OrLast6MonthsStatement = form16Doc;
//                            if (appointmentDoc != null) existing.AppointmentOrderDocument = appointmentDoc;
//                            if (guideDoc != null) existing.GuideRecognitionDoc = guideDoc;
//                            if (phdDoc != null) existing.PhDrecognitionDoc = phdDoc;
//                            if (litigationDoc != null) existing.LitigationDoc = litigationDoc;
//                            if (OnlineTeachersDatabase != null) existing.OnlineTeachersDatabase = OnlineTeachersDatabase;
//                            if (madetorecruit != null) existing.Madetorecruit = madetorecruit;

//                            _context.AffTeachingFacultyAllDetails.Update(existing);
//                        }
//                        else
//                        {
//                            // ✅ INSERT new record with ALL fields
//                            var faculty = new AffTeachingFacultyAllDetail
//                            {
//                                FacultyCode = facultyCode,
//                                CollegeCode = collegeCode,
//                                TeachingFacultyName = m.NameOfFaculty?.Trim() ?? "N/A",
//                                Subject = m.Subject?.Trim() ?? "N/A",
//                                DateOfBirth = m.DateOfBirth.HasValue ? DateOnly.FromDateTime(m.DateOfBirth.Value) : default,
//                                Mobile = m.Mobile?.Trim() ?? "N/A",
//                                Email = m.Email?.Trim() ?? "N/A",
//                                AadhaarNumber = m.Aadhaar?.Trim() ?? "N/A",
//                                Pannumber = m.PAN?.Trim() ?? "N/A",
//                                RnRmnumber = m.RN_RMNumber?.Trim() ?? "N/A",
//                                Department = m.Department?.Trim() ?? "N/A",
//                                DepartmentDetails = m.DepartmentDetails?.Trim() ?? "N/A",
//                                Designation = m.Designation?.Trim() ?? "N/A",
//                                Qualification = m.Qualification?.Trim() ?? "N/A",
//                                UginstituteName = m.UGInstituteName?.Trim() ?? "N/A",
//                                UgyearOfPassing = (int)m.UGYearOfPassing,
//                                PginstituteName = m.PGInstituteName?.Trim() ?? "N/A",
//                                PgyearOfPassing = (int)m.PGYearOfPassing,
//                                PgpassingSpecialization = m.PGPassingSpecialization?.Trim() ?? "N/A",
//                                TeachingExperienceAfterUgyears = m.TeachingExperienceAfterUGYears,
//                                TeachingExperienceAfterPgyears = m.TeachingExperienceAfterPGYears,
//                                DateOfJoining = m.DateOfJoining.HasValue ? DateOnly.FromDateTime(m.DateOfJoining.Value) : default,
//                                RecognizedPgTeacher = m.RecognizedPgTeacher,
//                                RecognizedPhDteacher = m.RecognizedPhDTeacher,
//                                IsRecognizedPgguide = m.IsRecognizedPGGuide,
//                                IsExaminer = m.IsExaminer,
//                                ExaminerFor = m.ExaminerForList != null ? string.Join(",", m.ExaminerForList) : null,
//                                LitigationPending = m.LitigationPending,
//                                Nrtsnumber = m.NRTSNumber?.Trim() ?? "N/A",
//                                Rguhstin = m.RGUHSTIN?.Trim() ?? "N/A",
//                                RemoveRemarks = m.RemoveRemarks,
//                                IsRemoved = !string.IsNullOrWhiteSpace(m.RemoveRemarks),

//                                // Documents
//                                AadhaarDocument = aadhaarDoc,
//                                Pandocument = panDoc,
//                                RnRmdocument = rnRmDoc,
//                                Form16OrLast6MonthsStatement = form16Doc,
//                                AppointmentOrderDocument = appointmentDoc,
//                                GuideRecognitionDoc = guideDoc,
//                                PhDrecognitionDoc = phdDoc,
//                                LitigationDoc = litigationDoc,
//                                OnlineTeachersDatabase = OnlineTeachersDatabase,
//                                Madetorecruit = madetorecruit
//                            };

//                            _context.AffTeachingFacultyAllDetails.Add(faculty);
//                        }
//                    }

//                    _context.SaveChanges();
//                    transaction.Commit();

//                    TempData["Success"] = "Faculty records saved successfully.";
//                    return RedirectToAction("AFF_TeachingFacultyDetails");
//                }
//                catch (Exception ex)
//                {
//                    transaction.Rollback();
//                    TempData["Error"] = "Error saving faculty records: " + ex.Message;
//                    return View(model);
//                }
//            }
//        }


//        private byte[] ConvertFileToBytes(IFormFile formFile)
//        {
//            using (var ms = new MemoryStream())
//            {
//                formFile.CopyTo(ms);
//                return ms.ToArray();
//            }
//        }
//        public IActionResult ViewFacultyDocument(int id, string type, string mode = "view")
//        {
//            var faculty = _context.FacultyDetails.FirstOrDefault(f => f.Id == id);
//            if (faculty == null)
//                return NotFound();

//            byte[] fileBytes = null;
//            string fileName = $"{type}_document.pdf";

//            switch (type.ToLower())
//            {
//                case "pg":
//                    fileBytes = faculty.GuideRecognitionDoc;
//                    break;

//                case "phd":
//                    fileBytes = faculty.PhDrecognitionDoc;
//                    break;

//                case "litig":
//                    fileBytes = faculty.LitigationDoc;
//                    break;

//                default:
//                    return BadRequest("Invalid document type.");
//            }

//            if (fileBytes == null)
//                return NotFound("Document not uploaded.");

//            if (mode == "download")
//            {
//                // 📥 FORCE DOWNLOAD
//                return File(fileBytes, "application/octet-stream", fileName);
//            }

//            // 👀 VIEW IN BROWSER
//            return File(fileBytes, "application/pdf");
//        }



//        private IEnumerable<SelectListItem> GetDesignations(string facultyCode)
//        {
//            return _context.DesignationMasters
//                .Where(d => d.FacultyCode.ToString() == facultyCode)      // if you need filter
//                .OrderBy(d => d.DesignationOrder)
//                .Select(d => new SelectListItem
//                {
//                    Value = d.DesignationCode,                  // or Id
//                    Text = d.DesignationName
//                })
//                .ToList();
//        }

//        private IEnumerable<SelectListItem> GetQualifications(string facultyCode)
//        {
//            return _context.MstCourses
//                .Where(c => c.FacultyCode.ToString() == facultyCode)      // optional filter
//                .Select(c => new SelectListItem
//                {
//                    Value = c.CourseCode.ToString(),                      // or ID
//                    Text = c.CourseName
//                })
//                .ToList();
//        }
//        [HttpGet]
//        public IActionResult Aff_NonFacultyDeatils()
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            var rows = _context.AffNonTeachingFaculties
//                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
//                .Select(x => new NonTeachingFacultyRow
//                {
//                    Id = x.Id,
//                    FacultyName = x.FacultyName,
//                    Designation = x.Designation,
//                    Qualification = x.Qualification,
//                    TotalExperience = x.TotalExperience,
//                    Remarks = x.Remarks
//                }).ToList();

//            if (!rows.Any())
//                rows.Add(new NonTeachingFacultyRow()); // first blank row

//            var model = new NonTeachingFacultyViewModel
//            {
//                CollegeCode = collegeCode,
//                FacultyCode = facultyCode,
//                Rows = rows
//            };

//            ViewBag.Designations = GetDesignations(facultyCode);
//            ViewBag.Qualifications = GetQualifications(facultyCode);

//            return View(model);
//        }


//        [HttpPost]
//        public IActionResult Aff_NonFacultyDeatils(NonTeachingFacultyViewModel model)
//        {
//            var collegeCode = HttpContext.Session.GetString("CollegeCode");
//            var facultyCode = HttpContext.Session.GetString("FacultyCode");

//            if (!ModelState.IsValid)
//            {
//                ViewBag.Designations = GetDesignations(facultyCode);
//                ViewBag.Qualifications = GetQualifications(facultyCode);
//                return View(model);
//            }

//            foreach (var row in model.Rows)
//            {
//                if (string.IsNullOrWhiteSpace(row.FacultyName))
//                    continue;

//                AffNonTeachingFaculty entity;
//                if (row.Id > 0)
//                {
//                    entity = _context.AffNonTeachingFaculties
//                        .FirstOrDefault(x => x.Id == row.Id);

//                    if (entity == null)
//                    {
//                        // safety: treat as new
//                        entity = new AffNonTeachingFaculty();
//                        _context.AffNonTeachingFaculties.Add(entity);
//                    }
//                }
//                else
//                {
//                    entity = new AffNonTeachingFaculty();
//                    _context.AffNonTeachingFaculties.Add(entity);
//                }

//                entity.CollegeCode = collegeCode;
//                entity.FacultyCode = facultyCode;
//                entity.FacultyName = row.FacultyName;
//                entity.Designation = row.Designation;
//                entity.Qualification = row.Qualification;
//                entity.TotalExperience = row.TotalExperience;
//                entity.Remarks = row.Remarks;
//            }

//            _context.SaveChanges();
//            return RedirectToAction("Aff_NonFacultyDeatils");
//        }

//        ///////////////////////////////////////////////////////////////////////////////////////////////////
//        ///


//        [HttpGet]
//        public async Task<IActionResult> Nursing_Institute_Details1()
//        {
//            // Build empty VM
//            var vm = new InstitutionBasicDetailsViewModel1();

//            // Get codes from session/claims
//            var collegeName = User.FindFirst("CollegeName")?.Value;
//            var collegeCode = User.FindFirst("CollegeCode")?.Value;
//            var facultyCode = User.FindFirst("FacultyCode")?.Value;

//            if (string.IsNullOrEmpty(collegeName))
//                collegeName = HttpContext.Session.GetString("CollegeName");
//            if (string.IsNullOrEmpty(collegeCode))
//                collegeCode = HttpContext.Session.GetString("CollegeCode");
//            if (string.IsNullOrEmpty(facultyCode))
//                facultyCode = HttpContext.Session.GetString("FacultyCode");

//            // =======================
//            // 1. InstitutionBasicDetail (existing)
//            // =======================
//            InstitutionBasicDetail existingBasic = null;
//            if (!string.IsNullOrWhiteSpace(collegeCode) && !string.IsNullOrWhiteSpace(facultyCode))
//            {
//                existingBasic = await _context.InstitutionBasicDetails
//                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);
//            }

//            if (existingBasic != null)
//            {
//                // Map entity -> VM (EDIT mode)
//                vm.InstitutionId = existingBasic.InstitutionId;
//                vm.CollegeCode = existingBasic.CollegeCode;
//                vm.FacultyCode = existingBasic.FacultyCode;
//                vm.NameOfInstitution = existingBasic.NameOfInstitution;
//                vm.AddressOfInstitution = existingBasic.AddressOfInstitution;
//                vm.VillageTownCity = existingBasic.VillageTownCity;
//                vm.Taluk = existingBasic.Taluk;
//                vm.District = existingBasic.District;
//                vm.PinCode = existingBasic.PinCode;
//                vm.MobileNumber = existingBasic.MobileNumber;
//                vm.StdCode = existingBasic.StdCode;
//                vm.Fax = existingBasic.Fax;
//                vm.Website = existingBasic.Website;
//                vm.EmailId = existingBasic.EmailId;
//                vm.AltLandlineOrMobile = existingBasic.AltLandlineOrMobile;
//                vm.AltEmailId = existingBasic.AltEmailId;
//                vm.AcademicYearStarted = existingBasic.AcademicYearStarted;
//                vm.IsRuralInstitution = (bool)existingBasic.IsRuralInstitution;
//                vm.IsMinorityInstitution = (bool)existingBasic.IsMinorityInstitution;
//                vm.TrustName = existingBasic.TrustName;
//                vm.PresidentName = existingBasic.PresidentName;
//                vm.AadhaarNumber = existingBasic.AadhaarNumber;
//                vm.PANNumber = existingBasic.Pannumber;
//                vm.RegistrationNumber = existingBasic.RegistrationNumber;
//                vm.RegistrationDate = existingBasic.RegistrationDate?.ToDateTime(TimeOnly.MinValue);
//                vm.Amendments = (bool)existingBasic.Amendments;
//                vm.ExistingTrustName = existingBasic.ExistingTrustName;
//                vm.GOKObtainedTrustName = existingBasic.GokobtainedTrustName;
//                vm.ChangesInTrustName = (bool)existingBasic.ChangesInTrustName;
//                vm.OtherNursingCollegeInCity = (bool)existingBasic.OtherNursingCollegeInCity;
//                vm.CategoryOfOrganisation = existingBasic.CategoryOfOrganisation;
//                vm.ContactPersonName = existingBasic.ContactPersonName;
//                vm.ContactPersonRelation = existingBasic.ContactPersonRelation;
//                vm.ContactPersonMobile = existingBasic.ContactPersonMobile;
//                vm.OtherPhysiotherapyCollegeInCity = (bool)existingBasic.OtherPhysiotherapyCollegeInCity;
//                vm.CoursesAppliedText = existingBasic.CoursesAppliedText;
//                vm.HeadOfInstitutionName = existingBasic.HeadOfInstitutionName;
//                vm.HeadOfInstitutionAddress = existingBasic.HeadOfInstitutionAddress;
//                vm.FinancingAuthorityName = existingBasic.FinancingAuthorityName;
//                vm.CollegeStatus = existingBasic.CollegeStatus;
//                vm.GovAutonomousCertNumber = existingBasic.GovAutonomousCertNumber;
//                vm.KncCertificateNumber = existingBasic.KncCertificateNumber;
//                vm.TypeOfInstitution = existingBasic.TypeOfInstitution;
//                // vm.TypeOfOrganization = existingBasic.TypeOfOrganization;
//            }
//            else
//            {
//                // CREATE mode default values
//                vm.NameOfInstitution = collegeName ?? string.Empty;
//                vm.CollegeCode = collegeCode;
//                vm.FacultyCode = facultyCode;
//            }

//            // default OtherPhysiotherapyCollegeInCity
//            if (vm.OtherPhysiotherapyCollegeInCity == null)
//                vm.OtherPhysiotherapyCollegeInCity = false;

//            // =======================
//            // 2. AffInstitutionsDetail (InstituteDetails merged)
//            // =======================
//            if (!string.IsNullOrEmpty(collegeCode) && !string.IsNullOrEmpty(facultyCode))
//            {
//                var existingInstitution = await _context.AffInstitutionsDetails
//                    .AsNoTracking()
//                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

//                if (existingInstitution != null)
//                {
//                    vm.AffInstitutionId = existingInstitution.InstitutionId;

//                    // Prefer DB value; fall back to session/claim if DB value missing
//                    vm.TypeOfInstitution = existingInstitution.TypeOfInstitution;
//                    vm.NameOfInstitution = !string.IsNullOrWhiteSpace(existingInstitution.NameOfInstitution)
//                        ? existingInstitution.NameOfInstitution
//                        : (collegeName ?? vm.NameOfInstitution ?? string.Empty);

//                    vm.Address = existingInstitution.Address;
//                    vm.VillageTownCity = existingInstitution.VillageTownCity;
//                    vm.Taluk = existingInstitution.Taluk;
//                    vm.District = existingInstitution.District;
//                    vm.PinCode = existingInstitution.PinCode;
//                    vm.MobileNumber = existingInstitution.MobileNumber;
//                    vm.StdCode = existingInstitution.StdCode;
//                    vm.Fax = existingInstitution.Fax;
//                    vm.Website = existingInstitution.Website;
//                    vm.SurveyNoPidNo = existingInstitution.SurveyNoPidNo;
//                    vm.MinorityInstitute = existingInstitution.MinorityInstitute;
//                    vm.AttachedToMedicalClg = existingInstitution.AttachedToMedicalClg;
//                    vm.RuralInstitute = existingInstitution.RuralInstitute;

//                    // YearOfEstablishment in DB may be stored as string. Preserve as-is.
//                    vm.YearOfEstablishment = existingInstitution.YearOfEstablishment ?? string.Empty;

//                    vm.EmailId = existingInstitution.EmailId;
//                    vm.AltLandlineMobile = existingInstitution.AltLandlineMobile;
//                    vm.AltEmailId = existingInstitution.AltEmailId;
//                    vm.HeadOfInstitution = existingInstitution.HeadOfInstitution;
//                    vm.HeadAddress = existingInstitution.HeadAddress;
//                    vm.FinancingAuthority = existingInstitution.FinancingAuthority;
//                    vm.StatusOfCollege = existingInstitution.StatusOfCollege;
//                    vm.CourseApplied = existingInstitution.CourseApplied;
//                }
//                else
//                {
//                    // No DB record — use session/claim name if available
//                    vm.NameOfInstitution ??= collegeName ?? string.Empty;
//                    vm.YearOfEstablishment = string.Empty;
//                }
//            }
//            else
//            {
//                // Session missing — still try to populate name
//                vm.NameOfInstitution ??= collegeName ?? string.Empty;
//                vm.YearOfEstablishment = string.Empty;
//            }

//            // Dropdown
//            vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
//                .Where(e => e.FacultyId.ToString() == facultyCode)
//                .OrderBy(t => t.InstitutionType)
//                .Select(t => new SelectListItem
//                {
//                    Value = t.InstitutionTypeId.ToString(),
//                    Text = t.InstitutionType
//                })
//                .ToListAsync();

//            //vm.TypeOfOrganization = await _context.TypeOfOrganizationMasters
//            //    .OrderBy(t => t.TypeName)
//            //    .Select(t => new SelectListItem
//            //    {
//            //        Value = t.TypeId.ToString(),
//            //        Text = t.TypeName
//            //    })
//            //    .ToListAsync();

//            return View(vm);
//        }













//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [RequestSizeLimit(104857600)]  // 100MB
//        [RequestFormLimits(ValueCountLimit = 100000, ValueLengthLimit = int.MaxValue)]
//        public async Task<IActionResult> Nursing_Institute_Details1(InstitutionBasicDetailsViewModel1 vm)
//        {
//            // Get session/claims FIRST
//            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? User?.FindFirst("CollegeCode")?.Value;
//            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? User?.FindFirst("FacultyCode")?.Value;

//            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(facultyCode))
//            {
//                TempData["Error"] = "Session expired. Please login again.";
//                return RedirectToAction("Login", "Account");
//            }

//            // Server-authoritative
//            vm.CollegeCode = collegeCode;
//            vm.FacultyCode = facultyCode;
//            vm.NameOfInstitution ??= User?.FindFirst("CollegeName")?.Value
//                ?? HttpContext.Session.GetString("CollegeName") ?? "";

//            // Remove client-supplied model state for server fields
//            ModelState.Remove(nameof(vm.CollegeCode));
//            ModelState.Remove(nameof(vm.FacultyCode));
//            ModelState.Remove(nameof(vm.NameOfInstitution));

//            // From InstituteDetails: YearOfEstablishment is required
//            if (string.IsNullOrWhiteSpace(vm.YearOfEstablishment))
//            {
//                ModelState.AddModelError(nameof(vm.YearOfEstablishment), "Year of Establishment is required.");
//            }

//            if (!ModelState.IsValid)
//            {
//                vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
//                    .Where(e => e.FacultyId.ToString() == facultyCode)
//                    .OrderBy(t => t.InstitutionType)
//                    .Select(t => new SelectListItem
//                    {
//                        Value = t.InstitutionTypeId.ToString(),
//                        Text = t.InstitutionType
//                    }).ToListAsync();

//                TempData["ModelStateErrors"] = string.Join(", ",
//                    ModelState.Values.Where(v => v.Errors.Count > 0)
//                                     .SelectMany(v => v.Errors)
//                                     .Select(e => e.ErrorMessage));
//                return View(vm);
//            }

//            // =======================
//            // 1. InstitutionBasicDetail: UPDATE or INSERT
//            // =======================
//            InstitutionBasicDetail entity;

//            if (vm.InstitutionId > 0)
//            {
//                // UPDATE existing
//                entity = await _context.InstitutionBasicDetails
//                    .FirstOrDefaultAsync(x => x.InstitutionId == vm.InstitutionId
//                                           && x.CollegeCode == collegeCode
//                                           && x.FacultyCode == facultyCode);

//                if (entity == null)
//                {
//                    TempData["Error"] = "Institution record not found for update.";
//                    return RedirectToAction(nameof(Nursing_Institute_Details));
//                }
//            }
//            else
//            {
//                // INSERT new (ensure no duplicate by Faculty+College)
//                var duplicate = await _context.InstitutionBasicDetails
//                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

//                if (duplicate != null)
//                {
//                    TempData["Error"] = "Institution details already exist for this college. Use Edit.";
//                    return RedirectToAction(nameof(TrustMemberDetails), new { id = duplicate.InstitutionId });
//                }

//                entity = new InstitutionBasicDetail
//                {
//                    FacultyCode = facultyCode,
//                    CollegeCode = collegeCode,
//                    CreatedOn = DateTime.UtcNow
//                };
//                _context.InstitutionBasicDetails.Add(entity);
//            }

//            // Map VM -> InstitutionBasicDetail
//            // entity.TypeOfOrganization = vm.TypeOfOrganization ?? "";
//            entity.TypeOfInstitution = vm.TypeOfInstitution;
//            entity.NameOfInstitution = vm.NameOfInstitution ?? "";
//            entity.AddressOfInstitution = vm.AddressOfInstitution ?? "";
//            entity.VillageTownCity = vm.VillageTownCity ?? "";
//            entity.Taluk = vm.Taluk ?? "";
//            entity.District = vm.District ?? "";
//            entity.PinCode = vm.PinCode;
//            entity.MobileNumber = vm.MobileNumber;
//            entity.StdCode = vm.StdCode;
//            entity.Fax = vm.Fax;
//            entity.Website = vm.Website;
//            entity.EmailId = vm.EmailId;
//            entity.AltLandlineOrMobile = vm.AltLandlineOrMobile;
//            entity.AltEmailId = vm.AltEmailId;
//            entity.AcademicYearStarted = vm.AcademicYearStarted;
//            entity.IsRuralInstitution = vm.IsRuralInstitution;
//            entity.IsMinorityInstitution = vm.IsMinorityInstitution;
//            entity.TrustName = vm.TrustName ?? "";
//            entity.PresidentName = vm.PresidentName ?? "";
//            entity.AadhaarNumber = vm.AadhaarNumber;
//            entity.Pannumber = vm.PANNumber;
//            entity.RegistrationNumber = vm.RegistrationNumber;
//            entity.RegistrationDate = vm.RegistrationDate.HasValue
//                ? DateOnly.FromDateTime(vm.RegistrationDate.Value)
//                : null;
//            entity.Amendments = vm.Amendments;
//            entity.ExistingTrustName = vm.ExistingTrustName ?? "";
//            entity.GokobtainedTrustName = vm.GOKObtainedTrustName ?? "";
//            entity.ChangesInTrustName = vm.ChangesInTrustName;
//            entity.OtherNursingCollegeInCity = vm.OtherNursingCollegeInCity;
//            entity.CategoryOfOrganisation = vm.CategoryOfOrganisation ?? "";
//            entity.ContactPersonName = vm.ContactPersonName ?? "";
//            entity.ContactPersonRelation = vm.ContactPersonRelation ?? "";
//            entity.ContactPersonMobile = vm.ContactPersonMobile;
//            entity.OtherPhysiotherapyCollegeInCity = vm.OtherPhysiotherapyCollegeInCity;
//            entity.CoursesAppliedText = vm.CoursesAppliedText ?? "";
//            entity.HeadOfInstitutionName = vm.HeadOfInstitutionName ?? "";
//            entity.HeadOfInstitutionAddress = vm.HeadOfInstitutionAddress ?? "";
//            entity.FinancingAuthorityName = vm.FinancingAuthorityName ?? "";
//            entity.CollegeStatus = vm.CollegeStatus;
//            entity.GovAutonomousCertNumber = vm.GovAutonomousCertNumber;
//            entity.KncCertificateNumber = vm.KncCertificateNumber;

//            // Files (null-safe)
//            entity.GovAutonomousCertFile = await SafeToBytes(vm.GovAutonomousCertFile);
//            entity.GovCouncilMembershipFile = await SafeToBytes(vm.GovCouncilMembershipFile);
//            entity.GokOrderExistingCoursesFile = await SafeToBytes(vm.GokOrderExistingCoursesFile);
//            entity.FirstAffiliationNotifFile = await SafeToBytes(vm.FirstAffiliationNotifFile);
//            entity.ContinuationAffiliationFile = await SafeToBytes(vm.ContinuationAffiliationFile);
//            entity.KncCertificateFile = await SafeToBytes(vm.KncCertificateFile);
//            entity.AmendedDoc = await SafeToBytes(vm.AmendedDoc);
//            entity.AadhaarFile = await SafeToBytes(vm.AadhaarFile);
//            entity.Panfile = await SafeToBytes(vm.PANFile);
//            entity.BankStatementFile = await SafeToBytes(vm.BankStatementFile);
//            entity.RegistrationCertificateFile = await SafeToBytes(vm.RegistrationCertificateFile);
//            entity.RegisteredTrustMemberDetails = await SafeToBytes(vm.RegisteredTrustMemberDetails);
//            entity.AuditStatementFile = await SafeToBytes(vm.AuditStatementFile);

//            // =======================
//            // 2. AffInstitutionsDetail: UPDATE or INSERT (merged InstituteDetails logic)
//            // =======================

//            var existingInstitution = await _context.AffInstitutionsDetails
//                .FirstOrDefaultAsync(x => x.CollegeCode == vm.CollegeCode &&
//                                          x.FacultyCode == vm.FacultyCode);

//            // Handle document upload
//            byte[] docBytes = null;
//            string docName = null;
//            string docContentType = null;

//            if (vm.DocumentFile != null && vm.DocumentFile.Length > 0)
//            {
//                using var ms = new MemoryStream();
//                await vm.DocumentFile.CopyToAsync(ms);
//                docBytes = ms.ToArray();
//                docName = vm.DocumentFile.FileName;
//                docContentType = vm.DocumentFile.ContentType;
//            }

//            // Use the string provided by the user (trim). If you want to normalize to yyyy, do additional parsing here.
//            var yearOfEstablishment = vm.YearOfEstablishment?.Trim();

//            if (existingInstitution != null)
//            {
//                // UPDATE
//                existingInstitution.CollegeCode = vm.CollegeCode;
//                existingInstitution.FacultyCode = vm.FacultyCode;
//                existingInstitution.TypeOfInstitution = vm.TypeOfInstitution;
//                existingInstitution.NameOfInstitution = vm.NameOfInstitution;
//                existingInstitution.Address = vm.Address;
//                existingInstitution.VillageTownCity = vm.VillageTownCity;
//                existingInstitution.Taluk = vm.Taluk;
//                existingInstitution.District = vm.District;
//                existingInstitution.PinCode = vm.PinCode;
//                existingInstitution.MobileNumber = vm.MobileNumber;
//                existingInstitution.StdCode = vm.StdCode;
//                existingInstitution.Fax = vm.Fax;
//                existingInstitution.Website = vm.Website;
//                existingInstitution.SurveyNoPidNo = vm.SurveyNoPidNo;
//                existingInstitution.MinorityInstitute = (bool)vm.MinorityInstitute;
//                existingInstitution.AttachedToMedicalClg = (bool)vm.AttachedToMedicalClg;
//                existingInstitution.RuralInstitute = (bool)vm.RuralInstitute;
//                existingInstitution.YearOfEstablishment = yearOfEstablishment;
//                existingInstitution.EmailId = vm.EmailId;
//                existingInstitution.AltLandlineMobile = vm.AltLandlineMobile;
//                existingInstitution.AltEmailId = vm.AltEmailId;
//                existingInstitution.HeadOfInstitution = vm.HeadOfInstitution;
//                existingInstitution.HeadAddress = vm.HeadAddress;
//                existingInstitution.FinancingAuthority = vm.FinancingAuthority;
//                existingInstitution.StatusOfCollege = vm.StatusOfCollege;
//                existingInstitution.CourseApplied = vm.CourseApplied;

//                // Only overwrite document fields if a new file was uploaded
//                if (docBytes != null)
//                {
//                    existingInstitution.DocumentName = docName;
//                    existingInstitution.DocumentContentType = docContentType;
//                    existingInstitution.DocumentData = docBytes;
//                }
//            }
//            else
//            {
//                // CREATE
//                var affEntity = new AffInstitutionsDetail
//                {
//                    CollegeCode = vm.CollegeCode,
//                    FacultyCode = vm.FacultyCode,
//                    TypeOfInstitution = vm.TypeOfInstitution,
//                    NameOfInstitution = vm.NameOfInstitution,
//                    Address = vm.Address,
//                    VillageTownCity = vm.VillageTownCity,
//                    Taluk = vm.Taluk,
//                    District = vm.District,
//                    PinCode = vm.PinCode,
//                    MobileNumber = vm.MobileNumber,
//                    StdCode = vm.StdCode,
//                    Fax = vm.Fax,
//                    Website = vm.Website,
//                    SurveyNoPidNo = vm.SurveyNoPidNo,
//                    MinorityInstitute = (bool)vm.MinorityInstitute,
//                    AttachedToMedicalClg = (bool)vm.AttachedToMedicalClg,
//                    RuralInstitute = (bool)vm.RuralInstitute,
//                    YearOfEstablishment = yearOfEstablishment,
//                    EmailId = vm.EmailId,
//                    AltLandlineMobile = vm.AltLandlineMobile,
//                    AltEmailId = vm.AltEmailId,
//                    HeadOfInstitution = vm.HeadOfInstitution,
//                    HeadAddress = vm.HeadAddress,
//                    FinancingAuthority = vm.FinancingAuthority,
//                    StatusOfCollege = vm.StatusOfCollege,
//                    CourseApplied = vm.CourseApplied,
//                    DocumentName = docName,
//                    DocumentContentType = docContentType,
//                    DocumentData = docBytes
//                };

//                _context.AffInstitutionsDetails.Add(affEntity);
//            }

//            // =======================
//            // 3. Save and redirect
//            // =======================
//            try
//            {
//                var savedCount = await _context.SaveChangesAsync();

//                if (savedCount > 0)
//                {
//                    TempData["Success"] = vm.InstitutionId > 0
//                        ? "Institution details updated successfully!"
//                        : "Institution details saved successfully!";

//                    // If you now want to go to course details after this combined save:
//                    // var affId = existingInstitution?.InstitutionId ?? affEntity.InstitutionId;
//                    // return RedirectToAction(nameof(AffCourseDetails), new { id = affId });

//                    return RedirectToAction(nameof(TrustMemberDetails), new { id = entity.InstitutionId });
//                }
//                else
//                {
//                    TempData["Error"] = "No changes were saved. Check required fields.";
//                    vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
//                        .Where(e => e.FacultyId.ToString() == facultyCode)
//                        .OrderBy(t => t.InstitutionType)
//                        .Select(t => new SelectListItem
//                        {
//                            Value = t.InstitutionTypeId.ToString(),
//                            Text = t.InstitutionType
//                        }).ToListAsync();
//                    return View(vm);
//                }
//            }
//            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("duplicate") == true ||
//                                               ex.InnerException?.Message?.Contains("PRIMARY KEY") == true)
//            {
//                TempData["Error"] = "Duplicate record exists for this Faculty/College combination.";
//                return View(vm);
//            }
//            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("NULL") == true)
//            {
//                TempData["Error"] = $"Required field cannot be null: {ex.InnerException.Message}";
//                return View(vm);
//            }
//            catch (Exception ex)
//            {
//                TempData["Error"] = $"Save failed: {ex.Message}";
//                Console.WriteLine($"Institution save error: {ex}");
//                return View(vm);
//            }
//        }



//    }
//}

