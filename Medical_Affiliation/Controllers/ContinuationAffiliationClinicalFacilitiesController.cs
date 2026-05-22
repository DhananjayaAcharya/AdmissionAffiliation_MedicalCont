using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
    public class ContinuationAffiliationClinicalFacilitiesController : BaseController
    {
        private readonly IHospitalService _hospitalService;
        private readonly IUserContext _userContext;
        private readonly ApplicationDbContext _context;


        public ContinuationAffiliationClinicalFacilitiesController(IHospitalService hospitalService, IUserContext userContext, ApplicationDbContext context)
        {
            _hospitalService = hospitalService;
            _userContext = userContext;
            _context = context;
        }
        private async Task<string?> SaveHospitalFileAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = Path.Combine("D:\\Affiliation_Medical", "HospitalDetails");
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
        public async Task<IActionResult> ClinicalFacilities()
        {
            var model = await _hospitalService.GetHospitalDetailsAsync(
                _userContext.CollegeCode,
                _userContext.FacultyId,
                _userContext.CourseLevel
            );

            if (model == null)
            {
                throw new Exception("Composite VM is NULL");
            }

            if (model.ClinicalHospitalDetails == null)
            {
                throw new Exception("ClinicalHospitalDetails is NULL");
            }

            if (model.NptaRequirementPostvm == null)
            {
                throw new Exception("NptaRequirementPostvm is NULL");
            }

            if (model.EngAlliedRequirementPostvm == null)
            {
                throw new Exception("EngAlliedRequirementPostvm is NULL");
            }

            if (model.DisciplineVm == null)
            {
                throw new Exception("DisciplineVm is NULL");
            }

            return View("HospitalDetailsForAffiliation", model);
        }

        private async Task<bool> HospitalExists(int hospitalId, string collegeCode, string facultyCode)
        {
            return await _context.HospitalDetailsForAffiliations
                .AnyAsync(h => h.HospitalDetailsId == hospitalId
                          && h.CollegeCode == collegeCode
                          && h.FacultyCode == facultyCode);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClinicalHospitalDetails(ClinicalHospitalDetailsPostVM vm)
        {
            try
            {
                var collegeCode = _userContext.CollegeCode;
                var facultyId = _userContext.FacultyId;
                var courseLevel = _userContext.CourseLevel;
                vm.Form.CollegeCode = collegeCode;
                vm.Form.FacultyCode = facultyId.ToString();
                vm.Form.CourseLevel = courseLevel;
                vm.Form.AffiliationTypeId = 2;

                ValidateClinicalHospitalDetails(vm);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .Select(ms => new
                        {
                            Field = ms.Key,
                            Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        }).ToArray();

                    return BadRequest(new { errors });
                }



                HospitalDetailsForAffiliation hospital;

                if (vm.Form.HospitalDetailsId > 0)
                {
                    hospital = await _context.HospitalDetailsForAffiliations
                        .Include(h => h.HospitalFacilities)
                        .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode && e.HospitalDetailsId == vm.Form.HospitalDetailsId && e.CourseLevel == courseLevel);

                    if (hospital == null) return NotFound();

                    _context.Entry(hospital).CurrentValues.SetValues(vm.Form);
                }
                else
                {
                    hospital = new HospitalDetailsForAffiliation();
                    _context.HospitalDetailsForAffiliations.Add(hospital);
                }

                hospital.CollegeCode = vm.Form.CollegeCode;
                hospital.FacultyCode = vm.Form.FacultyCode.ToString();
                hospital.CourseLevel = vm.Form.CourseLevel;
                hospital.AffiliationTypeId = vm.Form.AffiliationTypeId;

                hospital.ParentMedicalCollegeExists = vm.Form.ParentMedicalCollegeExists;
                hospital.HospitalType = vm.Form.HospitalType.ToString();
                hospital.HospitalOwnedBy = vm.Form.HospitalOwnedBy.ToString();
                hospital.HospitalName = vm.Form.HospitalName;
                hospital.HospitalOwnerName = vm.Form.HospitalOwnerName;
                hospital.HospitalDistrictId = vm.Form.HospitalDistrictId.ToString();
                hospital.HospitalTalukId = vm.Form.HospitalTalukId.ToString();
                hospital.Location = vm.Form.Location;
                hospital.IsParentHospitalForOtherNursingInstitution =
                    vm.Form.IsParentHospitalForOtherNursingInstitution;

                if (hospital.IsParentHospitalForOtherNursingInstitution == true)
                {
                    if (vm.SupportingDoc != null)
                    {
                        hospital.HospitalParentSupportingDoc =
                            await ConvertToBytesAsync(vm.SupportingDoc);
                    }
                    // else: keep existing document
                }
                else
                {
                    hospital.HospitalParentSupportingDoc = null;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Basic Hospital details and clinical facilities saved successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClinicalCapacity(ClinicalCapacityPostVM vm)
        {
            ValidateClinicalCapacity(vm);

            if (!ModelState.IsValid)
            {
                // Return structured JSON for AJAX
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .Select(ms => new
                    {
                        Field = ms.Key,
                        Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    })
                    .ToArray();

                return BadRequest(new { errors });
            }

            var hospital = await _context.HospitalDetailsForAffiliations
                .FirstOrDefaultAsync(h => h.HospitalDetailsId == vm.Form.HospitalDetailsId);

            if (hospital == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please fill Basic Hospital Details (first section)."
                });

            }

            hospital.TotalBeds = vm.Form.TotalBeds;
            hospital.OpdperDay = vm.Form.OpdperDay;
            hospital.DentalChairsCount = vm.Form?.DentalChairsCount;
            hospital.Has24HourEmergency = vm.Form?.Has24HourEmergency;
            hospital.HasCriticalCareServices = vm.Form?.HasCriticalCareServices;
            hospital.IpdbedOccupancyPercent = vm.Form.IpdbedOccupancyPercent;
            hospital.AnnualOpdprevYear = vm.Form.AnnualOpdprevYear;
            hospital.AnnualIpdprevYear = vm.Form.AnnualIpdprevYear;
            hospital.DistanceBetweenCollegeAndHospitalKm = vm.Form.DistanceBetweenCollegeAndHospitalKm;
            hospital.IsOwnerAmemberOfTrust = vm.Form.IsOwnerAmemberOfTrust;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Capacity & Statistics saved successfully" });
        }

        private void ValidateClinicalCapacity(ClinicalCapacityPostVM vm)
        {
            if (vm.Form == null)
            {
                ModelState.AddModelError("Form", "Clinical capacity form is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(vm.Form.CollegeCode))
                ModelState.AddModelError("Form.CollegeCode", "College code is required.");

            if (string.IsNullOrWhiteSpace(vm.Form.FacultyCode))
                ModelState.AddModelError("Form.FacultyCode", "Faculty code is required.");

            if (!vm.Form.TotalBeds.HasValue || vm.Form.TotalBeds <= 0)
                ModelState.AddModelError("Form.TotalBeds", "Total beds must be greater than 0.");

            if (!vm.Form.OpdperDay.HasValue || vm.Form.OpdperDay < 0)
                ModelState.AddModelError("Form.OpdperDay", "OPD per day cannot be negative.");

            if (!vm.Form.IpdbedOccupancyPercent.HasValue
                || vm.Form.IpdbedOccupancyPercent < 0
                || vm.Form.IpdbedOccupancyPercent > 100)
                ModelState.AddModelError("Form.IpdbedOccupancyPercent", "IPD bed occupancy must be between 0 and 100.");

            if (!vm.Form.AnnualOpdprevYear.HasValue || vm.Form.AnnualOpdprevYear < 0)
                ModelState.AddModelError("Form.AnnualOpdprevYear", "Annual OPD (previous year) cannot be negative.");

            if (!vm.Form.AnnualIpdprevYear.HasValue || vm.Form.AnnualIpdprevYear < 0)
                ModelState.AddModelError("Form.AnnualIpdprevYear", "Annual IPD (previous year) cannot be negative.");

            if (!vm.Form.DistanceBetweenCollegeAndHospitalKm.HasValue || vm.Form.DistanceBetweenCollegeAndHospitalKm < 0)
                ModelState.AddModelError("Form.DistanceBetweenCollegeAndHospitalKm", "Distance cannot be negative.");

            if (!vm.Form.IsOwnerAmemberOfTrust.HasValue)
                ModelState.AddModelError("Form.IsOwnerAmemberOfTrust", "Owner trust membership status is required.");
        }


        private static async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
        private async Task<string?> SaveAffiliatedHospitalFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = Path.Combine("D:\\Affiliation_Medical", "AffiliatedHospitalDocs");
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAffiliatedHospitalDocuments(AffiliatedHospitalDocumentsPostVM model)
        {
            try
            {
                var collegeCode = _userContext.CollegeCode;
                var courseLevel = _userContext.CourseLevel;
                model.CollegeCode = collegeCode;

                ValidateAffiliatedHospitalDocuments(model);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .Select(ms => new
                        {
                            Field = ms.Key,
                            Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToArray();

                    return BadRequest(new { errors });
                }

                // 1️⃣ Existing records
                var existingDocs = await _context.AffiliatedHospitalDocuments
                    .Where(d => d.CollegeCode == collegeCode && d.CourseLevel == courseLevel)
                    .ToListAsync();

                // 2️⃣ Submitted IDs
                var submittedDocIds = model.Documents
                    .Where(d => d.DocumentId.HasValue)
                    .Select(d => d.DocumentId!.Value)
                    .ToList();

                // 3️⃣ DELETE missing records + FILES 🔥
                var toDelete = existingDocs
                    .Where(db => !submittedDocIds.Contains(db.DocumentId))
                    .ToList();

                if (toDelete.Any())
                {
                    foreach (var item in toDelete)
                    {
                        if (!string.IsNullOrEmpty(item.DocumentFilePth) &&
                            System.IO.File.Exists(item.DocumentFilePth))
                        {
                            System.IO.File.Delete(item.DocumentFilePth);
                        }
                    }

                    _context.AffiliatedHospitalDocuments.RemoveRange(toDelete);
                }

                // 4️⃣ ADD / UPDATE
                foreach (var doc in model.Documents)
                {
                    if (doc.DocumentId.HasValue)
                    {
                        // UPDATE
                        var existing = existingDocs
                            .First(x => x.DocumentId == doc.DocumentId.Value);

                        existing.HospitalType = doc.HospitalType;
                        existing.HospitalName = doc.HospitalName;
                        existing.TotalBeds = doc.TotalBeds;

                        if (doc.DocumentFile != null && doc.DocumentFile.Length > 0)
                        {
                            var filePath = await SaveAffiliatedHospitalFileAsync(doc.DocumentFile);

                            if (filePath != null)
                            {
                                // 🔹 Delete old file
                                if (!string.IsNullOrEmpty(existing.DocumentFilePth) &&
                                    System.IO.File.Exists(existing.DocumentFilePth))
                                {
                                    System.IO.File.Delete(existing.DocumentFilePth);
                                }

                                existing.DocumentFilePth = filePath;
                                existing.DocumentName = doc.DocumentFile.FileName;
                            }
                        }
                    }
                    else
                    {
                        // ADD
                        if (doc.DocumentFile == null || doc.DocumentFile.Length == 0)
                            continue;

                        var filePath = await SaveAffiliatedHospitalFileAsync(doc.DocumentFile);

                        _context.AffiliatedHospitalDocuments.Add(
                            new AffiliatedHospitalDocument
                            {
                                CollegeCode = collegeCode,
                                CourseLevel = courseLevel,
                                HospitalType = doc.HospitalType,
                                HospitalName = doc.HospitalName,
                                TotalBeds = doc.TotalBeds,
                                DocumentName = doc.DocumentFile.FileName,
                                DocumentFilePth = filePath, // ✅ FIXED
                                CreatedDate = DateTime.UtcNow
                            });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Affiliated Hospital Documents saved successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDentalWardDistribution( [FromBody] List<DentalWardBedDistributionVm> dentalWards)
        {
            try
            {
                var facultyCode = FacultyCode;
                var collegeCode = CollegeCode;

                if (dentalWards == null || !dentalWards.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No Dental Ward Details Found."
                    });
                }

                var hospital = await _context.HospitalDetailsForAffiliations
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

                if (hospital == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Hospital Details Not Found."
                    });
                }

                foreach (var item in dentalWards)
                {
                    var existing = await _context.DentalWardBedDistributions
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == Convert.ToInt32(facultyCode) &&
                            x.HospitalDetailsId == hospital.HospitalDetailsId &&
                            x.AffiliationTypeId == hospital.AffiliationTypeId &&
                            x.WardId == item.WardId);

                    if (existing != null)
                    {
                        // UPDATE

                        existing.WardName = item.WardName;
                        existing.SeatSlab = item.SeatSlab;
                        existing.BedsRequired = item.BedsRequired;
                        existing.BedsPresent = item.BedsPresent ?? 0;
                    }
                    else
                    {
                        // INSERT

                        var entity = new DentalWardBedDistribution
                        {
                            FacultyCode = Convert.ToInt32(facultyCode),
                            CollegeCode = collegeCode,
                            HospitalDetailsId = hospital.HospitalDetailsId,
                            AffiliationTypeId = hospital.AffiliationTypeId,

                            WardId = item.WardId,
                            WardName = item.WardName,
                            SeatSlab = item.SeatSlab,
                            BedsRequired = item.BedsRequired,
                            BedsPresent = item.BedsPresent ?? 0
                        };

                        await _context.DentalWardBedDistributions.AddAsync(entity);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Dental Ward Distribution Saved Successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEngAlliedServices( [FromBody] EngAlliedRequirementsPostVM model)
        {
            try
            {
                if (model == null || model.Requirements == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid request."
                    });
                }

                // Existing saved data
                var existingRecords = await _context.DentalServices
                    .Where(x =>
                        x.CollegeCode == model.CollegeCode &&
                        x.FacultyCode == model.FacultyCode &&
                        x.AffiliationTypeId == model.AffiliationTypeId &&
                        x.HospitalDetailsId == model.HospitalDetailsId &&
                        x.SectionCode == 2)
                    .ToListAsync();

                foreach (var item in model.Requirements)
                {
                    var existing = existingRecords
                        .FirstOrDefault(x => x.RequirementId == item.RequirementId);

                    // =========================
                    // UPDATE
                    // =========================

                    if (existing != null)
                    {
                        existing.AvailabilityStatus = item.IsAvailable;

                        existing.SectionCode = item.SectionCode;

                        existing.UpdatedOn = DateTime.Now;
                    }

                    // =========================
                    // INSERT
                    // =========================

                    else
                    {
                        var entity = new DentalService
                        {
                            CollegeCode = model.CollegeCode,
                            FacultyCode = model.FacultyCode,
                            AffiliationTypeId = model.AffiliationTypeId,
                            HospitalDetailsId = model.HospitalDetailsId,

                            SectionCode = item.SectionCode,

                            RequirementId = item.RequirementId,

                            AvailabilityStatus = item.IsAvailable,

                            CreatedOn = DateTime.Now
                        };

                        await _context.DentalServices.AddAsync(entity);
                    }
                }
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Engineering & Allied Services Saved Successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNPTAServices( [FromBody] NPTARequirementsPostVM model)
        {
            try
            {
                if (model == null || model.Requirements == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid request."
                    });
                }

                // Existing saved data
                var existingRecords = await _context.DentalServices
                    .Where(x =>
                        x.CollegeCode == model.CollegeCode &&
                        x.FacultyCode == model.FacultyCode &&
                        x.AffiliationTypeId == model.AffiliationTypeId &&
                        x.HospitalDetailsId == model.HospitalDetailsId &&
                        x.SectionCode == 1)
                    .ToListAsync();

                foreach (var item in model.Requirements)
                {
                    var existing = existingRecords
                        .FirstOrDefault(x => x.RequirementId == item.RequirementId);

                    // =========================
                    // UPDATE
                    // =========================

                    if (existing != null)
                    {
                        existing.AvailabilityStatus = item.IsAvailable;

                        existing.SectionCode = item.SectionCode;

                        existing.UpdatedOn = DateTime.Now;
                    }

                    // =========================
                    // INSERT
                    // =========================

                    else
                    {
                        var entity = new DentalService
                        {
                            CollegeCode = model.CollegeCode,
                            FacultyCode = model.FacultyCode,
                            AffiliationTypeId = model.AffiliationTypeId,
                            HospitalDetailsId = model.HospitalDetailsId,

                            SectionCode = item.SectionCode,

                            RequirementId = item.RequirementId,

                            AvailabilityStatus = item.IsAvailable,

                            CreatedOn = DateTime.Now
                        };

                        await _context.DentalServices.AddAsync(entity);
                    }
                }
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "NPTA Services Saved Successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAdmAncServices( [FromBody] AdmAncRequirementsPostVM model)
        {
            try
            {
                if (model == null || model.Requirements == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid request."
                    });
                }

                // Existing saved data
                var existingRecords = await _context.DentalServices
                    .Where(x =>
                        x.CollegeCode == model.CollegeCode &&
                        x.FacultyCode == model.FacultyCode &&
                        x.AffiliationTypeId == model.AffiliationTypeId &&
                        x.HospitalDetailsId == model.HospitalDetailsId &&
                        x.SectionCode == 3)
                    .ToListAsync();

                foreach (var item in model.Requirements)
                {
                    var existing = existingRecords
                        .FirstOrDefault(x => x.RequirementId == item.RequirementId);

                    // =========================
                    // UPDATE
                    // =========================

                    if (existing != null)
                    {
                        existing.AvailabilityStatus = item.IsAvailable;

                        existing.SectionCode = item.SectionCode;

                        existing.UpdatedOn = DateTime.Now;
                    }

                    // =========================
                    // INSERT
                    // =========================

                    else
                    {
                        var entity = new DentalService
                        {
                            CollegeCode = model.CollegeCode,
                            FacultyCode = model.FacultyCode,
                            AffiliationTypeId = model.AffiliationTypeId,
                            HospitalDetailsId = model.HospitalDetailsId,

                            SectionCode = item.SectionCode,

                            RequirementId = item.RequirementId,

                            AvailabilityStatus = item.IsAvailable,

                            CreatedOn = DateTime.Now
                        };

                        await _context.DentalServices.AddAsync(entity);
                    }
                }
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Administration & Ancillary Services Saved Successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }



        private void ValidateAffiliatedHospitalDocuments(AffiliatedHospitalDocumentsPostVM model)
        {
            if (model.Documents == null || !model.Documents.Any())
            {
                ModelState.AddModelError("Documents", "No documents uploaded.");
                return;
            }

            for (int i = 0; i < model.Documents.Count; i++)
            {
                var doc = model.Documents[i];

                if (doc.DocumentFile == null || doc.DocumentFile.Length == 0)
                    continue;
                ValidateFileSize(doc.DocumentFile, $"Documents[{i}].DocumentFile");
                //if (string.IsNullOrWhiteSpace(doc.CertificateNumber))
                //    ModelState.AddModelError($"Documents[{i}].CertificateNumber", "Certificate Number is required.");
            }
        }
        public async Task<IActionResult> ViewHospitalDocument(int documentId, string mode = "view")
        {
            var doc = await _context.AffiliatedHospitalDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DocumentId == documentId);

            if (doc == null ||
                string.IsNullOrEmpty(doc.DocumentFilePth) ||
                !System.IO.File.Exists(doc.DocumentFilePth))
                return NotFound("File not found");

            var fileName = Path.GetFileName(doc.DocumentFilePth);

            // 🔥 Detect content type dynamically
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(doc.DocumentFilePth, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // 📥 Download mode
            if (mode == "download")
            {
                return PhysicalFile(doc.DocumentFilePth, contentType, fileName);
            }

            // 👀 Preview mode (inline)
            return PhysicalFile(doc.DocumentFilePth, contentType);
        }
        private void ValidateClinicalHospitalDetails(ClinicalHospitalDetailsPostVM vm)
        {
            if (vm.Form == null)
            {
                ModelState.AddModelError("Form", "Clinical facility form is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(vm.Form.CollegeCode))
                ModelState.AddModelError("Form.CollegeCode", "College Code is required.");

            if (string.IsNullOrWhiteSpace(vm.Form.FacultyCode))
                ModelState.AddModelError("Form.FacultyCode", "Faculty Code is required.");

            //if (vm.Form.HospitalFacilities == null || !vm.Form.HospitalFacilities.Any())
            //    ModelState.AddModelError("SelectedFacilityIds", "Select at least one facility.");

            if (vm?.Form?.IsParentHospitalForOtherNursingInstitution == true)
            {
                if (vm.SupportingDoc == null)
                {
                    ModelState.AddModelError("SupportingDoc", "Supporting document is required for parent hospitals.");
                }
                else
                {
                    ValidateFileSize(vm.SupportingDoc, "SupportingDoc");
                }
            }
        }

        private void ValidateFileSize(IFormFile file, string fieldName, double maxMb = 2)
        {
            if (file == null)
                return; // No file uploaded, skip validation

            if (file.Length <= 0)
            {
                ModelState.AddModelError(fieldName, "File cannot be empty.");
                return;
            }

            var sizeInMb = file.Length / (1024.0 * 1024.0);
            if (sizeInMb > maxMb)
            {
                ModelState.AddModelError(fieldName, $"File size must not exceed {maxMb} MB.");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHospitalFacilities(HospitalFacilitiesPostVM vm)
        {
            try
            {

                var collegeCode = _userContext.CollegeCode;
                var facultyId = _userContext.FacultyId;
                var courseLevel = _userContext.CourseLevel;

                ValidateHospitalFacilities(vm);

                if (!ModelState.IsValid)
                {
                    // Return structured JSON for AJAX
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .Select(ms => new
                        {
                            Field = ms.Key,
                            Messages = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToArray();

                    return BadRequest(new { errors });
                }

                var hospital = await _context.HospitalDetailsForAffiliations
                    .Include(h => h.HospitalFacilities)
                    .FirstOrDefaultAsync(h => h.HospitalDetailsId == vm.HospitalDetailsId && h.CollegeCode == collegeCode && h.CourseLevel == courseLevel);

                if (hospital == null)
                    return NotFound();

                hospital.HospitalFacilities.Clear();

                foreach (var facilityId in vm.SelectedFacilityIds)
                {
                    hospital.HospitalFacilities.Add(new HospitalFacility
                    {
                        FacilityId = facilityId,
                        FacultyCode = facultyId,
                        CourseLevel = courseLevel,
                        HospitalDetailsId = hospital.HospitalDetailsId,

                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Hospital facilities saved successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }


        private void ValidateHospitalFacilities(HospitalFacilitiesPostVM vm)
        {
            if (vm.HospitalDetailsId <= 0)
                ModelState.AddModelError("HospitalDetailsId", "Hospital reference is missing.");

            //if (vm.SelectedFacilityIds == null || !vm.SelectedFacilityIds.Any())
            //    ModelState.AddModelError("SelectedFacilityIds", "Select at least one facility.");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveIndoorRequirements([FromForm] IndoorDepartmentRequirementsPostVM model)
        {
            var courseLevel = _userContext.CourseLevel;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                await SaveRequirementsAsync(
                    collegeCode: model.CollegeCode,
                    facultyCode: model.FacultyCode,
                    //courseLevel :model.CourseLevel,
                    affiliationTypeId: model.AffiliationTypeId,
                    hospitalDetailsId: model.HospitalDetailsId,
                    sectionCode: "1",                 // 👈 Indoor section code
                    requirements: model.Requirements
                );

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOperationTheatreRequirements([FromBody] OTRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel:model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "2",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCasualtyRequirements([FromBody] CasualityRequirementsPostVM model)
        {


            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "3",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCSSDandLaundryRequirements([FromBody] CSSDandLaundryRequirementsPostVM model)
        {

            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel : model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "4",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRadioDiagnosisRequirements([FromBody] RadioDiagnosisRequirementsPostVM model)
        {

            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "5",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAnaesthesiologyRequirements([FromBody] AnaesthesiologyRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel:model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "6",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCentralLaboratoryRequirements([FromBody] CentralLaboratoryRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "7",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBloodBankRequirements([FromBody] BloodBankRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "8",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveYogaRequirements([FromBody] YogaRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "9",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRadiationOncologyRequirements([FromBody] RadiationOncologyRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "10",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveArtCenterRequirements([FromBody] ArtCenterRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "11",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePharmacyRequirements([FromBody] PharmacyRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "12",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUtilitiesRequirements([FromBody] UtilitiesRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            if (!await HospitalExists(model.HospitalDetailsId, model.CollegeCode, model.FacultyCode.ToString()))
            {
                // Return a 200 OK but with success=false so the .then block catches it, 
                // OR return BadRequest with a JSON object.
                return Json(new { success = false, message = "Hospital record not found. Please verify and submit Basic Details (first section)." });
            }

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "13",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOutPatientAreaRequirements([FromBody] OutPatientRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            if (!await HospitalExists(model.HospitalDetailsId, model.CollegeCode, model.FacultyCode.ToString()))
            {
                // Return a 200 OK but with success=false so the .then block catches it, 
                // OR return BadRequest with a JSON object.
                return Json(new { success = false, message = "Hospital record not found. Please verify and submit Basic Details (first section)." });
            }

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "15",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveIndoorBedsUnitsRequirements([FromBody] IndoorBedsUnitsRequirementsPostVM model)
        {
            if (model == null || model.Requirements == null)
                return BadRequest("Invalid data");

            if (!await HospitalExists(model.HospitalDetailsId, model.CollegeCode, model.FacultyCode.ToString()))
            {
                // Return a 200 OK but with success=false so the .then block catches it, 
                // OR return BadRequest with a JSON object.
                return Json(new { success = false, message = "Hospital record not found. Please verify and submit Basic Details (first section)." });
            }

            await SaveRequirementsAsync(
                collegeCode: model.CollegeCode,
                facultyCode: model.FacultyCode,
                //  courseLevel: model.CourseLevel,
                affiliationTypeId: model.AffiliationTypeId,
                hospitalDetailsId: model.HospitalDetailsId,
                sectionCode: "14",
                requirements: model.Requirements
            );

            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }

        private async Task SaveRequirementsAsync(string collegeCode, int facultyCode, int affiliationTypeId, int hospitalDetailsId,
            string sectionCode, IEnumerable<RequirementItemBaseVM> requirements)
        {

            var courselevel = _userContext.CourseLevel;
            // Fetch existing records for the section
            var existing = await _context.IndoorInfrastructureRequirementsCompliances
                .Where(r =>
                    r.CollegeCode == collegeCode &&
                    r.FacultyCode == facultyCode &&
                    r.AffiliationTypeId == affiliationTypeId &&
                    r.HospitalDetailsId == hospitalDetailsId &&
                    r.CourseLevel == courselevel &&
                    r.SectionCode == sectionCode)
                .ToListAsync();

            foreach (var req in requirements)
            {
                var match = existing.FirstOrDefault(e => e.RequirementId == req.RequirementId);

                if (match == null)
                {
                    _context.IndoorInfrastructureRequirementsCompliances.Add(
                        new IndoorInfrastructureRequirementsCompliance
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = courselevel,
                            AffiliationTypeId = affiliationTypeId,
                            HospitalDetailsId = hospitalDetailsId,
                            RequirementId = req.RequirementId,
                            SectionCode = sectionCode,
                            IsCompliant = req.IsAvailable,
                            InspectedOn = DateTime.Now,

                        });
                }
                else
                {
                    match.IsCompliant = req.IsAvailable;
                    match.InspectedOn = DateTime.Now;
                    _context.IndoorInfrastructureRequirementsCompliances.Update(match);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveIndoorBedsOccupancy([FromForm] IndoorBedsOccupancyPostVM model)
        {

            var courseLevel = _userContext.CourseLevel;
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                var existing = await _context.IndoorBedsOccupancies
                    .Where(o =>
                        o.CollegeCode == model.CollegeCode &&
                        o.FacultyCode == model.FacultyCode &&
                        o.CourseLevel == courseLevel &&
                        o.AffiliationTypeId == model.AffiliationTypeId)
                    .ToListAsync();


                foreach (var item in model.Items)
                {
                    if (item.CollegeIntake < 0)
                        return BadRequest("College intake must be >= 0");

                    //if (item.CollegeIntake > item.RGUHSintake)
                    //    return BadRequest("College intake cannot exceed RGUHS intake");

                    var match = existing.FirstOrDefault(o => o.DepartmentId == item.DepartmentId &&
                                                           o.SeatSlabId == item.SeatSlabId);

                    if (match == null)
                    {

                        _context.IndoorBedsOccupancies.Add(
                            new IndoorBedsOccupancy
                            {
                                CollegeCode = model.CollegeCode,
                                FacultyCode = model.FacultyCode,
                                CourseLevel = courseLevel,
                                AffiliationTypeId = model.AffiliationTypeId,
                                DepartmentId = item.DepartmentId,
                                SeatSlabId = item.SeatSlabId,
                                Rguhsintake = item.RGUHSintake,
                                CollegeIntake = item.CollegeIntake
                            });
                    }
                    else
                    {
                        match.CollegeIntake = item.CollegeIntake;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSuperVisionInFieldPracticeArea([FromForm] SuperVisionInFieldPracticeAreaPostVm model)
        {
            var courselevel = _userContext.CourseLevel;

            if (model == null || model.ItemsSuperVision == null || !model.ItemsSuperVision.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No supervision data received."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            try
            {
                var existingRecords = await _context.SuperVisionInFieldPracticeAreas
                    .Where(e =>
                        e.CollegeCode == model.CollegeCode &&
                        e.FacultyCode == model.FacultyCode &&
                        e.AffiliationTypeId == model.AffiliationTypeId &&
                        e.CourseLevel == courselevel &&
                        e.HospitalDetailsId == model.HospitalDetailsId)
                    .ToListAsync();

                foreach (var item in model.ItemsSuperVision)
                {
                    SuperVisionInFieldPracticeArea entity;

                    // UPDATE if ID exists
                    if (item.Id > 0)
                    {
                        entity = existingRecords.FirstOrDefault(x => x.Id == item.Id);

                        if (entity == null)
                            continue; // safety
                    }
                    else
                    {
                        // INSERT new row
                        entity = new SuperVisionInFieldPracticeArea
                        {
                            CollegeCode = model.CollegeCode,
                            FacultyCode = model.FacultyCode,
                            CourseLevel = courselevel,
                            AffiliationTypeId = model.AffiliationTypeId,
                            HospitalDetailsId = model.HospitalDetailsId
                        };

                        _context.SuperVisionInFieldPracticeAreas.Add(entity);
                    }

                    // Common fields (update or insert)
                    entity.Post = item.Post;
                    entity.Name = item.Name;
                    entity.Qualification = item.Qualification;
                    entity.YearOfQualification = new DateOnly(item.YearOfQualification, 1, 1);
                    entity.University = item.University;

                    entity.UgFromDate = item.UgFromDate ?? throw new Exception("UG From Date is required");
                    entity.UgToDate = item.UgToDate ?? throw new Exception("UG To Date is required");
                    entity.PgFromDate = item.PgFromDate ?? DateOnly.MinValue;
                    entity.PgToDate = item.PgToDate ?? DateOnly.MinValue;


                    entity.Responsibilities = item.Responsibilities;
                }

                // DELETE removed rows
                var incomingIds = model.ItemsSuperVision
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToList();

                var toDelete = existingRecords
                    .Where(x => !incomingIds.Contains(x.Id))
                    .ToList();

                if (toDelete.Any())
                    _context.SuperVisionInFieldPracticeAreas.RemoveRange(toDelete);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Supervision details saved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error while saving supervision data.",
                    error = ex.Message
                });
            }
        }

    }
}
