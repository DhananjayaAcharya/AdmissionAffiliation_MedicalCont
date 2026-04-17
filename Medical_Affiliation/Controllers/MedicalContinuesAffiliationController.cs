using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

namespace Medical_Affiliation.Controllers
{
    public class MedicalContinuesAffiliationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicalContinuesAffiliationController(ApplicationDbContext context)
        {
            _context = context;

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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult OnAffiliationTypeChanged(SidebarViewModel model)
        //{
        //    // Adjust the value check to match your actual ID/text
        //    if (model.SelectedAffiliationId == 2) // Continuation of Affiliation
        //    {
        //        return RedirectToAction(
        //            actionName: "Medical_InstituteDetails",
        //            controllerName: "MedicalContinuesAffiliation");
        //    }

        //    // default: stay on same page or redirect somewhere else
        //    return RedirectToAction("Dashboard", "Collegelogin");
        //}
        //csharp Medical_Affiliation\Controllers\MedicalContinuesAffiliationController.cs
        [HttpGet]
        public async Task<IActionResult> Medical_InstituteDetails()
        {
            var vm = new MedicalVm();

            // Get from claims/session
            var collegeName = User.FindFirst("CollegeName")?.Value;
            if (string.IsNullOrEmpty(collegeName))
                collegeName = HttpContext.Session.GetString("CollegeName");

            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? User.FindFirst("CollegeCode")?.Value;
            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? User.FindFirst("FacultyCode")?.Value;

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "College/Faculty codes not found. Please login again.";
                return RedirectToAction("Index");
            }

            // Populate VM from session/claims
            vm.FacultyCode = facultyCode;
            vm.CollegeCode = collegeCode;
            vm.NameOfInstitution = collegeName ?? string.Empty;

            // Load existing institution data
            var existingInstitution = await _context.InstitutionBasicDetails
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

            if (existingInstitution != null)
            {
                vm.InstitutionId = existingInstitution.InstitutionId;
                // NOTE: use vm.TypeOfInstitution to match View's asp-for
                vm.TypeOfInstitution = existingInstitution.TypeOfInstitution;
                vm.AddressOfInstitution = existingInstitution.AddressOfInstitution;
                vm.VillageTownCity = existingInstitution.VillageTownCity;
                vm.Taluk = existingInstitution.Taluk;
                vm.District = existingInstitution.District;
                vm.PinCode = existingInstitution.PinCode;
                vm.MobileNumber = existingInstitution.MobileNumber;
                vm.StdCode = existingInstitution.StdCode;
                vm.Fax = existingInstitution.Fax;
                vm.Website = existingInstitution.Website;
                vm.EmailId = existingInstitution.EmailId;
                vm.AltLandlineOrMobile = existingInstitution.AltLandlineOrMobile;
                vm.AltEmailId = existingInstitution.AltEmailId;
                vm.AcademicYearStarted = existingInstitution.AcademicYearStarted;
                vm.IsRuralInstitution = existingInstitution.IsRuralInstitution ?? false;
                vm.IsMinorityInstitution = existingInstitution.IsMinorityInstitution ?? false;
                vm.TrustName = existingInstitution.TrustName;
                vm.PresidentName = existingInstitution.PresidentName;
                vm.AadhaarNumber = existingInstitution.AadhaarNumber;
                vm.PANNumber = existingInstitution.Pannumber;
                vm.RegistrationNumber = existingInstitution.RegistrationNumber;
                vm.RegistrationDate = existingInstitution.RegistrationDate;
                vm.Amendments = existingInstitution.Amendments ?? false;
                vm.ExistingTrustName = existingInstitution.ExistingTrustName;
                vm.GOKObtainedTrustName = existingInstitution.GokobtainedTrustName;
                vm.ChangesInTrustName = existingInstitution.ChangesInTrustName ?? false;
                vm.OtherNursingCollegeInCity = existingInstitution.OtherNursingCollegeInCity ?? false;
                vm.CategoryOfOrganisation = existingInstitution.CategoryOfOrganisation;
                vm.ContactPersonName = existingInstitution.ContactPersonName;
                vm.ContactPersonRelation = existingInstitution.ContactPersonRelation;
                vm.ContactPersonMobile = existingInstitution.ContactPersonMobile;
                vm.OtherPhysiotherapyCollegeInCity = existingInstitution.OtherPhysiotherapyCollegeInCity ?? false;
                vm.CoursesAppliedText = existingInstitution.CoursesAppliedText;
                vm.HeadOfInstitutionName = existingInstitution.HeadOfInstitutionName;
                vm.HeadOfInstitutionAddress = existingInstitution.HeadOfInstitutionAddress;
                vm.FinancingAuthorityName = existingInstitution.FinancingAuthorityName;
                vm.CollegeStatus = existingInstitution.CollegeStatus;
                vm.GovAutonomousCertNumber = existingInstitution.GovAutonomousCertNumber;
                vm.KncCertificateNumber = existingInstitution.KncCertificateNumber;
            }

            // Dropdown: institution type (filtered by faculty)
            if (int.TryParse(facultyCode, out var facultyId))
            {
                vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
                      .Where(e => e.FacultyId == facultyId)
                      .OrderBy(t => t.InstitutionType)
                      .Select(t => new SelectListItem
                      {
                          Value = t.InstitutionTypeId.ToString(),
                          Text = t.InstitutionType
                      })
                      .ToListAsync();
            }
            else
            {
                // fallback to previous string comparison if FacultyId stored as string
                vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
                      .Where(e => e.FacultyId.ToString() == facultyCode)
                      .OrderBy(t => t.InstitutionType)
                      .Select(t => new SelectListItem
                      {
                          Value = t.InstitutionTypeId.ToString(),
                          Text = t.InstitutionType
                      })
                      .ToListAsync();
            }

            return View(vm);
        }

        //csharp Medical_Affiliation\Controllers\MedicalContinuesAffiliationController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(ValueCountLimit = 100000, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Medical_InstituteDetails(MedicalVm vm)
        {
            var collegeName = User.FindFirst("CollegeName")?.Value
                              ?? HttpContext.Session.GetString("CollegeName");

            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? User.FindFirst("CollegeCode")?.Value;
            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? User.FindFirst("FacultyCode")?.Value;

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Index");
            }

            vm.FacultyCode = facultyCode;
            vm.CollegeCode = collegeCode;
            vm.NameOfInstitution = collegeName ?? string.Empty;

            ModelState.Remove(nameof(vm.TypeOfInstitutionList));
            ModelState.Remove(nameof(vm.FacultyCode));
            ModelState.Remove(nameof(vm.CollegeCode));
            ModelState.Remove(nameof(vm.NameOfInstitution));

            if (!ModelState.IsValid)
            {
                vm.TypeOfInstitutionList = await _context.MstInstitutionTypes
                    .OrderBy(t => t.InstitutionType)
                    .Select(t => new SelectListItem
                    {
                        Value = t.InstitutionTypeId.ToString(),
                        Text = t.InstitutionType
                    }).ToListAsync();

                return View(vm);
            }

            var existingInstitution = await _context.InstitutionBasicDetails
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (existingInstitution == null)
                {
                    existingInstitution = new InstitutionBasicDetail
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CreatedOn = DateTime.Now
                    };
                    _context.InstitutionBasicDetails.Add(existingInstitution);
                    await _context.SaveChangesAsync();
                }

                // 🔥 COMMON FILE SAVE FUNCTION
                async Task<string> SaveFile(IFormFile file, string folder)
                {
                    if (file == null || file.Length == 0)
                        return null;

                    string basePath = @"D:\Affiliation_Medical\InstitutionDetails";
                    string fullFolder = Path.Combine(basePath, folder);

                    if (!Directory.Exists(fullFolder))
                        Directory.CreateDirectory(fullFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string fullPath = Path.Combine(fullFolder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return fullPath;
                }

                // 🔥 SAVE FILES (REPLACED byte[])
                var govAutoPath = await SaveFile(vm.GovAutonomousCertFile, "GovAutonomous");
                var councilPath = await SaveFile(vm.GovCouncilMembershipFile, "Council");
                var gokPath = await SaveFile(vm.GokOrderExistingCoursesFile, "GOK");
                var firstPath = await SaveFile(vm.FirstAffiliationNotifFile, "FirstAffiliation");
                var contPath = await SaveFile(vm.ContinuationAffiliationFile, "Continuation");
                var kncPath = await SaveFile(vm.KncCertificateFile, "KNC");
                var amendPath = await SaveFile(vm.AmendedDoc, "Amendments");
                var aadhaarPath = await SaveFile(vm.AadhaarFile, "Aadhaar");
                var panPath = await SaveFile(vm.PANFile, "PAN");
                var bankPath = await SaveFile(vm.BankStatementFile, "Bank");
                var regPath = await SaveFile(vm.RegistrationCertificateFile, "Registration");
                var trustPath = await SaveFile(vm.RegisteredTrustMemberDetails, "TrustMembers");
                var auditPath = await SaveFile(vm.AuditStatementFile, "Audit");

                // 🔥 MAP FILE PATHS
                if (govAutoPath != null) existingInstitution.GovAutonomousCertFilePath = govAutoPath;
                if (councilPath != null) existingInstitution.GovCouncilMembershipFilePath = councilPath;
                if (gokPath != null) existingInstitution.GokOrderExistingCoursesFilePath = gokPath;
                if (firstPath != null) existingInstitution.FirstAffiliationNotifFilePath = firstPath;
                if (contPath != null) existingInstitution.ContinuationAffiliationFilePath = contPath;
                if (kncPath != null) existingInstitution.KncCertificateFilePath = kncPath;
                if (amendPath != null) existingInstitution.AmendedDocPath = amendPath;
                if (aadhaarPath != null) existingInstitution.AadhaarFilePath = aadhaarPath;
                if (panPath != null) existingInstitution.PanfilePath = panPath;
                if (bankPath != null) existingInstitution.BankStatementFilePath = bankPath;
                if (regPath != null) existingInstitution.RegistrationCertificateFilePath = regPath;
                if (trustPath != null) existingInstitution.RegisteredTrustMemberDetailsPath = trustPath;
                if (auditPath != null) existingInstitution.AuditStatementFilePath = auditPath;

                // 🔥 SAVE
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Institution details saved successfully!";
                return RedirectToAction(nameof(Medical_InstituteDetails));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Error: {ex.Message}";
                return View(vm);
            }
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


        [HttpGet]
        public async Task<IActionResult> Medical_TrustMemberDetails()
        {
            var model = new Medical_TrustMemberDetailsListVM();
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            model.FacultyCode = facultyCode;
            model.CollegeCode = collegeCode;

            // Populate designation dropdown filtered by facultyCode
            if (!string.IsNullOrEmpty(facultyCode))
            {
                // Primary: DesignationMasters (uses DesignationCode, FacultyCode)
                var fromDesignationMasters = await _context.DesignationMasters
                    .Where(d => d.FacultyCode.ToString() == facultyCode)
                    .OrderBy(d => d.DesignationName)
                    .Select(d => new SelectListItem
                    {
                        Value = d.DesignationCode,
                        Text = d.DesignationName ?? string.Empty
                    })
                    .ToListAsync();

                if (fromDesignationMasters.Any())
                {
                    model.DesignationList = fromDesignationMasters;
                }
                else
                {
                    // Fallback: MstDesignations (uses DesignationId int, FacultyId)
                    if (int.TryParse(facultyCode, out var facultyId))
                    {
                        model.DesignationList = await _context.MstDesignations
                            .Where(d => d.FacultyId.HasValue && d.FacultyId.Value == facultyId)
                            .OrderBy(d => d.DesignationName)
                            .Select(d => new SelectListItem
                            {
                                Value = d.DesignationId.ToString(),
                                Text = d.DesignationName ?? string.Empty
                            })
                            .ToListAsync();
                    }
                    else
                    {
                        model.DesignationList = new List<SelectListItem>();
                    }
                }
            }
            else
            {
                // No faculty code: empty list
                model.DesignationList = new List<SelectListItem>();
            }

            if (!string.IsNullOrEmpty(collegeCode) && !string.IsNullOrEmpty(facultyCode))
            {
                var existingEntities = await _context.ContinuationTrustMemberDetails
                    .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                    .OrderBy(x => x.TrustMemberName)
                    .ToListAsync();

                model.Rows = existingEntities.Select(x => new TrustMemberDetailsRowVM
                {
                    SlNo = x.SlNo,
                    FacultyCode = x.FacultyCode,
                    CollegeCode = x.CollegeCode,
                    TrustMemberName = x.TrustMemberName,
                    Qualification = x.Qualification,
                    MobileNumber = x.MobileNumber,
                    Age = x.Age,
                    JoiningDateString = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("yyyy-MM-dd") : null,
                    DesignationCode = x.DesignationId // store DB string id/code directly
                }).ToList();
            }

            if (!model.Rows.Any())
                model.Rows.Add(new TrustMemberDetailsRowVM { FacultyCode = facultyCode, CollegeCode = collegeCode });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_TrustMemberDetails(Medical_TrustMemberDetailsListVM model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))

                return RedirectToAction("InstituteDetails");

            // Remove server-controlled fields from validation
            ModelState.Remove(nameof(Medical_TrustMemberDetailsListVM.CollegeCode));
            ModelState.Remove(nameof(Medical_TrustMemberDetailsListVM.FacultyCode));
            //ModelState.Remove(nameof(TrustMemberDetailsRowVM.Designation));

            // Re-populate designation list so view can re-render on validation error
            if (!string.IsNullOrEmpty(facultyCode))
            {
                var fromDesignationMasters = await _context.DesignationMasters
                    .Where(d => d.FacultyCode.ToString() == facultyCode)
                    .OrderBy(d => d.DesignationName)
                    .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? string.Empty })
                    .ToListAsync();

                if (fromDesignationMasters.Any())
                    model.DesignationList = fromDesignationMasters;
                else if (int.TryParse(facultyCode, out var facultyId))
                    model.DesignationList = await _context.MstDesignations
                        .Where(d => d.FacultyId.HasValue && d.FacultyId.Value == facultyId)
                        .OrderBy(d => d.DesignationName)
                        .Select(d => new SelectListItem { Value = d.DesignationId.ToString(), Text = d.DesignationName ?? string.Empty })
                        .ToListAsync();
            }

            if (!ModelState.IsValid)
                return View(model);

            // Remove existing records
            var existingRecords = _context.ContinuationTrustMemberDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);
            _context.ContinuationTrustMemberDetails.RemoveRange(existingRecords);

            foreach (var row in model.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.TrustMemberName))
                    continue;

                // Parse HTML date string -> DateOnly
                DateOnly? joiningDate = null;
                if (!string.IsNullOrWhiteSpace(row.JoiningDateString) &&
                    DateOnly.TryParseExact(row.JoiningDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                {
                    joiningDate = d;
                }

                // DesignationCode is stored as string in DB.DesignationId
                var designationCode = string.IsNullOrWhiteSpace(row.DesignationCode) ? null : row.DesignationCode.Trim();

                // Optional: fetch designation text if you want to persist the human-readable name
                string? designationName = null;
                if (!string.IsNullOrWhiteSpace(designationCode))
                {
                    // Try DesignationMasters first
                    var des = await _context.DesignationMasters.FirstOrDefaultAsync(dm => dm.DesignationCode == designationCode);
                    if (des != null)
                        designationName = des.DesignationName;
                    else if (int.TryParse(designationCode, out var did))
                    {
                        // fallback to MstDesignations numeric id
                        var md = await _context.MstDesignations.FindAsync(did);
                        designationName = md?.DesignationName;
                    }
                }

                var entity = new ContinuationTrustMemberDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    TrustMemberName = row.TrustMemberName,
                    Designation = designationName,
                    Qualification = row.Qualification,
                    MobileNumber = row.MobileNumber,
                    Age = row.Age,
                    JoiningDate = joiningDate,
                    DesignationId = designationCode
                };

                _context.ContinuationTrustMemberDetails.Add(entity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("InstituteDetails");
        }

        [HttpGet]
        public async Task<IActionResult> Medical_AdminTeachingBlock()
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
        public async Task<IActionResult> Medical_AdminTeachingBlock(AdminTeachingBlockListVM vm)
        {
            if (vm == null || vm.Rows == null || !vm.Rows.Any())
            {
                TempData["Error"] = "No rows received. Please check the form.";
                return RedirectToAction(nameof(Medical_AdminTeachingBlock));
            }

            var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? string.Empty;
            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? string.Empty;

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                TempData["Error"] = "Session expired.";
                return RedirectToAction("Login", "Account");
            }

            TempData["Debug"] = $"Rows count: {vm.Rows.Count}\nSession: College={collegeCode}, Faculty={facultyCode}";

            var changesCount = 0;

            try
            {
                // --- inside AdminTeachingBlock POST, replace the foreach loop with this ---
                foreach (var row in vm.Rows)
                {
                    // Convert empty strings to null explicitly
                    var noOfRooms = string.IsNullOrWhiteSpace(row.NoOfRooms?.ToString()) ? (string)null : row.NoOfRooms;
                    var sizeSqFt = string.IsNullOrWhiteSpace(row.SizeSqFtAvailablePerRoom?.ToString()) ? (string)null : row.SizeSqFtAvailablePerRoom;

                    if (row.FacilityId <= 0) continue;

                    var entity = await _context.AffAdminTeachingBlocks
                        .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode &&
                                                 x.FacultyCode == facultyCode &&
                                                 x.FacilityId == row.FacilityId.ToString());

                    if (entity == null)
                    {
                        entity = new AffAdminTeachingBlock
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            FacilityId = row.FacilityId.ToString(),
                            Facilities = row.Facilities?.Trim() ?? null,  // NULL if empty
                            SizeSqFtAsPerNorms = row.SizeSqFtAsPerNorms?.Trim() ?? null,
                            IsAvailable = row.IsAvailable ? "Yes" : "No",
                            NoOfRooms = noOfRooms,  // Explicit NULL handling
                            SizeSqFtAvailablePerRoom = sizeSqFt,  // Explicit NULL handling
                            CreatedOn = DateTime.UtcNow,
                            CreatedBy = User.Identity?.Name ?? "Unknown"
                        };
                        _context.AffAdminTeachingBlocks.Add(entity);
                    }
                    else
                    {
                        // Update with null-safe assignments
                        entity.Facilities = row.Facilities?.Trim() ?? null;
                        entity.SizeSqFtAsPerNorms = row.SizeSqFtAsPerNorms?.Trim() ?? null;
                        entity.IsAvailable = row.IsAvailable ? "Yes" : "No";
                        entity.NoOfRooms = noOfRooms;
                        entity.SizeSqFtAvailablePerRoom = sizeSqFt;
                        _context.Entry(entity).State = EntityState.Modified;
                    }
                    changesCount++;
                }


                var saved = await _context.SaveChangesAsync();
                TempData["Message"] = $"Saved {saved} records successfully. Changes: {changesCount}";
                TempData["Debug"] += $"\nEF SaveChanges returned: {saved}";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = $"DB Error: {ex.InnerException?.Message ?? ex.Message}";
                TempData["Debug"] += $"\nDB Error: {ex}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                TempData["Debug"] += $"\nError: {ex}";
            }

            return RedirectToAction(nameof(Medical_AdminTeachingBlock));
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
                return RedirectToAction(nameof(AFF_SanIntake));
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

    }
}
