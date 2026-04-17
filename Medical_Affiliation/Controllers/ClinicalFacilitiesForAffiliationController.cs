using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Medical_Affiliation.Controllers
{
    public class ClinicalFacilitiesForAffiliationController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ClinicalFacilitiesForAffiliationController(ApplicationDbContext context)
        {
            _context = context;
        }

        private (string collegeCode, int facultyId) GetSessionCollegeAndFaculty()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "N465";
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode))
                throw new UnauthorizedAccessException("College code not found in session");

            int facultyId = int.TryParse(facultyCode, out int f) ? f : 1;

            return (collegeCode, facultyId);
        }

        public async Task<IActionResult> HospitalDetailsForAffiliation()
        {
            var (collegeCode, facultyId) = GetSessionCollegeAndFaculty();

            switch (facultyId)
            {
                case 1: // Medical
                    var medModel = await Med_HospitalDetailsForAffiliation();
                    return View("HospitalDetailsForAffiliation", medModel);

                case 3: // Nursing
                    var nursingModel = await NursingHospitalDetailsForAffiliation();
                    return View("InstructionSet/_Nursing", nursingModel);

                default:
                    return NotFound();
            }
        }


        public async Task<IActionResult> NursingHospitalDetailsForAffiliation()
        {
            var (collegeCode, facultyId) = GetSessionCollegeAndFaculty();


            // Fetch total beds from NursingAffiliatedYearwiseMaterialsData
            var materialsData = await _context.NursingAffiliatedYearwiseMaterialsData
                .Where(n => n.CollegeCode == collegeCode && n.HospitalType != "ParentHospital")
                .ToListAsync();

            var affiliatedDocs = await _context.AffiliatedHospitalDocuments
                .Where(d => d.CollegeCode == collegeCode)
                .ToListAsync();

            // Optional: existing hospital details (may be empty initially)
            var hospitals = await _context.HospitalDetailsForAffiliations
                .Include(h => h.HospitalFacilities)
                .Where(h => h.CollegeCode == collegeCode)
                .ToListAsync();

            var existingHospitalDetailsForAffiliations = await _context.HospitalDetailsForAffiliations.Where(h => h.CollegeCode == collegeCode).FirstOrDefaultAsync();
            var options = await _context.MstHospitalOwnedBies.Where(f => f.FacultyCode == facultyId).ToListAsync();
            var hospitalTypes = await _context.MstHospitalTypes.Where(f => f.FacultyCode == facultyId).ToListAsync();

            var materialSummary = materialsData
                .GroupBy(m => m.HospitalType)
                .ToDictionary(
                    g => g.Key!,
                    g => new
                    {
                        TotalBeds = g.Sum(m =>
                        {
                            int kpme = int.TryParse(m.Kpmebeds, out var k) ? k : 0;
                            int post = int.TryParse(m.PostBasicBeds, out var p) ? p : 0;
                            return kpme + post;
                        }),
                        HospitalName =
                            hospitals.FirstOrDefault(h => h.HospitalType == g.Key)?.HospitalName
                            ?? g.FirstOrDefault()?.ParentHospitalName
                            ?? string.Empty
                    });

            List<AffiliatedHospitalDocumentsViewModel> hospitalDocuments;

            if (affiliatedDocs.Any())
            {
                // ✅ DOCUMENTS DRIVE THE UI
                hospitalDocuments = affiliatedDocs
                    .Select(doc =>
                    {
                        materialSummary.TryGetValue(doc.HospitalType!, out var material);

                        return new AffiliatedHospitalDocumentsViewModel
                        {
                            CollegeCode = collegeCode,
                            HospitalType = doc.HospitalType!,
                            HospitalName = doc.HospitalName ?? material?.HospitalName ?? string.Empty,
                            TotalBeds = doc.TotalBeds ?? material?.TotalBeds ?? 0,

                            DocumentExists = true,
                            DocumentId = doc.DocumentId,
                            DocumentName = doc.DocumentName
                        };
                    })
                    .ToList();
            }
            else
            {
                // ✅ MATERIALS DRIVE THE UI (NO documents yet)
                hospitalDocuments = materialSummary
                    .Select(m => new AffiliatedHospitalDocumentsViewModel
                    {
                        CollegeCode = collegeCode,
                        HospitalType = m.Key,
                        HospitalName = m.Value.HospitalName,
                        TotalBeds = m.Value.TotalBeds,

                        DocumentExists = false
                    })
                    .ToList();
            }




            var mstHospitalDocs = await _context.MstHospitalDocuments.Where(f => f.FacultyCode == facultyId).ToListAsync();

            var filedTypes = await _context.MstFieldTypeChps.Where(f => f.FacultyCode == facultyId).ToListAsync();
            var planningTypes = await _context.MstFpaAdopAffTypes.Where(f => f.FacultyCode == facultyId).ToListAsync();
            var administrationTypes = await _context.MstAdministrations.Where(f => f.FacultyCode == facultyId).ToListAsync();
            var existinHospitalDetails = await _context.HospitalDocumentsToBeUploadeds.Where(f => f.CollegeCode == collegeCode).FirstOrDefaultAsync();

            List<HospitalDocumentsToBeUploaded> existingHospitalDocs;

            if (existinHospitalDetails != null)
            {
                existingHospitalDocs = await _context.HospitalDocumentsToBeUploadeds
                    .Where(f => f.CollegeCode == collegeCode
                             && f.HospitalDetailsId == existinHospitalDetails.HospitalDetailsId)
                    .ToListAsync();
            }
            else
            {
                existingHospitalDocs = new List<HospitalDocumentsToBeUploaded>();
            }

            var existingHealthCenterChp = await _context.HealthCenterChps.Where(f => f.CollegeCode == collegeCode).ToListAsync();

            var hospital = hospitals.FirstOrDefault();

            var defAffTypes = await _context.TypeOfAffiliations.ToListAsync();

            // Build the composite view model
            var vm = new HospitalAffiliationCompositeViewModel
            {
                ClinicalHospitalDetails = new ClinicalHospitalViewModel
                {
                    Form = hospital == null
                    ? new ClinicalHospitalFormVM
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyId.ToString(),
                        AffiliationTypeId = 2,
                        AffiliationType = defAffTypes.Where(f => f.TypeId == 2).Select(e => e.TypeDescription).FirstOrDefault()
                    }
                    : new ClinicalHospitalFormVM
                    {
                        HospitalDetailsId = hospital.HospitalDetailsId,
                        ParentMedicalCollegeExists = hospital.ParentMedicalCollegeExists,
                        HospitalType = hospital.HospitalType,
                        HospitalOwnedBy = hospital.HospitalOwnedBy,
                        HospitalName = hospital.HospitalName,
                        HospitalOwnerName = hospital.HospitalOwnerName,
                        HospitalDistrictId = hospital.HospitalDistrictId,
                        HospitalTalukId = hospital.HospitalTalukId,
                        Location = hospital.Location,
                        IsParentHospitalForOtherNursingInstitution = hospital.IsParentHospitalForOtherNursingInstitution,
                        CollegeCode = hospital.CollegeCode,
                        FacultyCode = hospital.FacultyCode,
                        AffiliationTypeId = hospital.AffiliationTypeId,
                        AffiliationType = defAffTypes.Where(f => f.TypeId == 2).Select(e => e.TypeDescription).FirstOrDefault()
                    },

                    SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
                    Districts = await _context.DistrictMasters
                        .Select(d => new DropdownItem { Text = d.DistrictName, Value = d.DistrictId })
                        .ToListAsync(),
                    Taluks = await _context.TalukMasters
                        .Select(t => new TalukItem
                        {
                            TalukID = t.TalukId,
                            TalukName = t.TalukName,
                            DistrictID = t.DistrictId
                        })
                        .ToListAsync(),
                    Locations = new List<string> { "Urban", "Semi-Urban", "Rural" },
                    ParentMedicalCollegeExistsOptions = new List<DropdownItem>
                    {
                        new() { Text = "Yes", Value = "true" },
                        new() { Text = "No", Value = "false" }
                    },
                    HospitalTypes = hospitalTypes.Select(f => new DropdownItem
                    {
                        Text = f.HospitalType,
                        Value = f.Id.ToString()
                    }).ToList(),

                    HospitalOwnedByOptions = options.Select(f => new DropdownItem
                    {
                        Text = f.OwnedBy,
                        Value = f.Id.ToString(),
                    }).OrderBy(e => e.Text).ToList(),


                    IsParentHospitalForOtherNursingInstitutionOptions = new List<DropdownItem>
                    {
                        new() { Text = "Yes", Value = "true" },
                        new() { Text = "No", Value = "false" }
                    },
                    IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
                    {
                        new() { Text = "Yes", Value = "true" },
                        new() { Text = "No", Value = "false" }
                    },

                    AvailableFacilities = await _context.HospitalFacilitiesMasters
                        .Where(f => f.AffiliationTypeId == 2 && f.FacultyCode == facultyId.ToString())
                        .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
                        .ToListAsync(),
                    IsSupportingDocExists = await GetSupportDocument(collegeCode),
                    HospitalFacilities = new HospitalFacilitiesViewModel
                    {
                        SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
                        AvailableFacilities = await _context.HospitalFacilitiesMasters
                            .Where(f => f.AffiliationTypeId == 2 && f.FacultyCode == facultyId.ToString())
                            .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
                            .ToListAsync(),
                        HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                    }
                },
                //HospitalDocuments = hospitalDocuments,
                AffiliatedDocumentsPostVM = new AffiliatedHospitalDocumentsPostVM
                {
                    CollegeCode = collegeCode,
                    Documents = hospitalDocuments.Select(d => new AffiliatedHospitalDocumentItemVM
                    {
                        DocumentId = d.DocumentId,
                        HospitalType = d.HospitalType,
                        HospitalName = d.HospitalName,
                        TotalBeds = d.TotalBeds,
                        DocumentExists = d.DocumentFile != null
                        // DocumentFile intentionally null on GET
                    }).ToList()
                },
                FieldPracticeAreaPostVM = new FieldPracticeAreasPostViewModel
                {
                    FieldPracticeAreas = filedTypes.Select(ft =>
                    {
                        var existing = existingHealthCenterChp.FirstOrDefault(e => e.FieldTypeId == ft.Id);

                        return new FieldPracticeAreaViewModel
                        {

                            SelectedFieldTypeId = ft.Id,
                            FieldType = ft.FieldType,
                            CollegeCode = collegeCode,
                            FacultyCode = facultyId,

                            SelectedPlanningTypeId = existing?.PlanningId,
                            AdopAffType = existing?.PlanningType,

                            SelectedAdministrationTypeId = existing?.AdministrationId,
                            AdminType = existing?.AdministrationType,

                            NameOfCHC = existing?.NameofHealthCenter ?? string.Empty,
                            ServicesRendered = existing?.ServicesRendered ?? string.Empty,
                            DistanceFromNursingInstitution =
                                existing?.DistanceFromNursingInstitution != null
                                    ? existing.DistanceFromNursingInstitution.ToString()
                                    : string.Empty,

                            FieldTypeOptions = filedTypes.Select(f => new DropdownItem
                            {
                                Text = f.FieldType,
                                Value = f.Id.ToString()
                            }).ToList(),
                            AdopAffTypeOptions = planningTypes.Select(f => new DropdownItem
                            {
                                Text = f.FpaType,
                                Value = f.Id.ToString()
                            }).ToList(),

                            AdministrationTypeOptions = administrationTypes.Select(e => new DropdownItem
                            {
                                Text = e.AdministrationType,
                                Value = e.Id.ToString()
                            }).ToList()
                        };
                    }).ToList(),
                },
                HospitalDocumentsToBeUploadedList = mstHospitalDocs.Select(dc =>
                {
                    var existing = existingHospitalDocs.FirstOrDefault(e => e.DocumentId == dc.Id);
                    return new HospitalDocumentsToBeUploadedViewModel
                    {
                        CollegeCode = collegeCode,
                        HospitalDetailsId = existingHospitalDetailsForAffiliations?.HospitalDetailsId ?? 0,
                        DocumentId = dc.Id,
                        DocumentName = dc.DocumentName,
                        CertificateNumber = existing?.CertificateNumber,
                        HospitalName = existingHospitalDetailsForAffiliations?.HospitalName,
                        DocumentExists = existing != null,


                    };
                }).ToList(),
                FacultyCode = facultyId,
                CollegeCode = collegeCode,
                ClinicalCapacity = new ClinicalCapacityViewModel
                {
                    Form = hospital == null
                    ? new ClinicalCapacityFormVM
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyId.ToString(),
                        AffiliationTypeId = 2,
                        //AffiliationType = defAffTypes.Where(f => f.TypeId == 2).Select(e => e.TypeDescription).FirstOrDefault()
                    }
                    : new ClinicalCapacityFormVM
                    {
                        HospitalDetailsId = hospital.HospitalDetailsId,
                        TotalBeds = hospital.TotalBeds,
                        OpdperDay = hospital.OpdperDay,
                        IpdbedOccupancyPercent = hospital.IpdbedOccupancyPercent,
                        AnnualOpdprevYear = hospital.AnnualOpdprevYear,
                        AnnualIpdprevYear = hospital.AnnualIpdprevYear,
                        DistanceBetweenCollegeAndHospitalKm =
                            hospital.DistanceBetweenCollegeAndHospitalKm,
                        IsOwnerAmemberOfTrust = hospital.IsOwnerAmemberOfTrust,
                        CollegeCode = hospital.CollegeCode,
                        FacultyCode = hospital.FacultyCode,
                        AffiliationTypeId = 2,
                        //AffiliationType = defAffTypes.Where(f => f.TypeId == 2).Select(e => e.TypeDescription).FirstOrDefault()
                    },

                    IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
                        {
                            new() { Text = "Yes", Value = "true" },
                            new() { Text = "No", Value = "false" }
                        }
                },
            };

            return View("Nursing/HospitalDetailsForAFfiliation", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClinicalHospitalDetails(ClinicalHospitalDetailsPostVM vm)
        {
            try
            {
                var (collegeCode, facultyId) = GetSessionCollegeAndFaculty();

                vm.Form.CollegeCode = collegeCode;
                vm.Form.FacultyCode = facultyId.ToString();
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
                        .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode && e.HospitalDetailsId == vm.Form.HospitalDetailsId);

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

            if (vm.Form.IsParentHospitalForOtherNursingInstitution == true)
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
                hospital = new HospitalDetailsForAffiliation();
                _context.HospitalDetailsForAffiliations.Add(hospital);
            }

            hospital.TotalBeds = vm.Form.TotalBeds;
            hospital.OpdperDay = vm.Form.OpdperDay;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHospitalFacilities(HospitalFacilitiesPostVM vm)
        {
            try
            {
                var (collegeCode, facultyId) = GetSessionCollegeAndFaculty();

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
                    .FirstOrDefaultAsync(h => h.HospitalDetailsId == vm.HospitalDetailsId && h.CollegeCode == collegeCode);

                if (hospital == null)
                    return NotFound();

                hospital.HospitalFacilities.Clear();

                foreach (var facilityId in vm.SelectedFacilityIds)
                {
                    hospital.HospitalFacilities.Add(new HospitalFacility
                    {
                        FacilityId = facilityId,
                        FacultyCode = facultyId,
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

        private async Task<string?> SaveAffiliatedHospitalFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = @"D:\Affiliation_Medical\AffiliatedHospitalDocs";

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
                var (collegeCode, _) = GetSessionCollegeAndFaculty();
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

                // 1️⃣ Get existing docs
                var existingDocs = await _context.AffiliatedHospitalDocuments
                    .Where(d => d.CollegeCode == collegeCode)
                    .ToListAsync();

                // 2️⃣ Submitted IDs
                var submittedDocIds = model.Documents
                    .Where(d => d.DocumentId.HasValue)
                    .Select(d => d.DocumentId!.Value)
                    .ToList();

                // 3️⃣ DELETE DB + FILE 🔥
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
                        // 🔹 UPDATE
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
                                // 🔥 Delete old file
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
                        // 🔹 ADD
                        if (doc.DocumentFile == null || doc.DocumentFile.Length == 0)
                            continue;

                        var filePath = await SaveAffiliatedHospitalFileAsync(doc.DocumentFile);

                        _context.AffiliatedHospitalDocuments.Add(
                            new AffiliatedHospitalDocument
                            {
                                CollegeCode = collegeCode,
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
        private static async Task<byte[]> ConvertToBytesAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentsToBeUploaded(HospitalDocumentsToBeUploadedPostModel model)
        {
            try
            {
                var (collegeCode, _) = GetSessionCollegeAndFaculty();
                //model.CollegeCode = collegeCode;

                ValidateDocumentsToBeUploadedDocuments(model);

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

                //if (model.Documents == null || !model.Documents.Any())
                //{
                //    ModelState.AddModelError("Documents", "No documents submitted.");
                //    return BadRequest(ModelState);
                //}

                int hospitalDetailsId = model.Documents.First().HospitalDetailsId; // safe after the check
                var existingDocs = await _context.HospitalDocumentsToBeUploadeds
                    .Where(d => d.CollegeCode == collegeCode && d.HospitalDetailsId == hospitalDetailsId)
                    .ToListAsync();


                // 2️⃣ Extract submitted document IDs
                var submittedDocIds = model.Documents
                    .Where(d => d.DocumentId.HasValue)
                    .Select(d => d.DocumentId!.Value)
                    .ToList();

                if (submittedDocIds.Any())
                {
                    var toDelete = existingDocs
                        .Where(db => !submittedDocIds.Contains(db.DocumentId))
                        .ToList();

                    if (toDelete.Any())
                    {
                        _context.HospitalDocumentsToBeUploadeds.RemoveRange(toDelete);
                    }
                }


                // 4️⃣ ADD / UPDATE
                foreach (var doc in model.Documents)
                {
                    var existing = existingDocs
                        .FirstOrDefault(d => d.DocumentId == doc.DocumentId);

                    if (existing != null)
                    {
                        // UPDATE
                        existing.CertificateNumber = doc.CertificateNumber;
                        existing.HospitalName = doc.HospitalName;

                        if (doc.DocumentFile != null && doc.DocumentFile.Length > 0)
                        {
                            existing.HospitalDocumentFile =
                                await ConvertToBytesAsync(doc.DocumentFile);
                        }
                    }
                    else
                    {
                        // ADD
                        if (doc.DocumentFile == null || doc.DocumentFile.Length == 0)
                            continue; // no file → no insert

                        _context.HospitalDocumentsToBeUploadeds.Add(
                            new HospitalDocumentsToBeUploaded
                            {
                                CollegeCode = collegeCode,
                                HospitalDetailsId = hospitalDetailsId,
                                DocumentId = doc.DocumentId!.Value,
                                DocumentName = doc.DocumentName,
                                CertificateNumber = doc.CertificateNumber,
                                HospitalDocumentFile =
                                    await ConvertToBytesAsync(doc.DocumentFile),
                                HospitalName = doc.HospitalName,

                            });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Documents saved successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }


        private void ValidateDocumentsToBeUploadedDocuments(HospitalDocumentsToBeUploadedPostModel model)
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

                if (string.IsNullOrWhiteSpace(doc.CertificateNumber))
                    ModelState.AddModelError($"Documents[{i}].CertificateNumber", "Certificate Number is required.");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFieldPracticeAreas(FieldPracticeAreasPostViewModel model)
        {
            try
            {
                var (collegeCode, facultyId) = GetSessionCollegeAndFaculty();

                var first = model.FieldPracticeAreas.FirstOrDefault();
                if (first != null)
                {
                    first.CollegeCode = collegeCode;
                    first.FacultyCode = facultyId;
                }


                ValidateFieldPracticeAreas(model);

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

                var existingCenters = await _context.HealthCenterChps
                    .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyId)
                    .ToListAsync();

                foreach (var fpa in model.FieldPracticeAreas)
                {
                    if (!IsTouched(fpa)) continue;

                    int distance = int.Parse(fpa.DistanceFromNursingInstitution);

                    var existing = existingCenters.FirstOrDefault(x => x.FieldTypeId == fpa.SelectedFieldTypeId);

                    if (existing != null)
                    {
                        existing.NameofHealthCenter = fpa.NameOfCHC;
                        existing.DistanceFromNursingInstitution = distance;
                        existing.PlanningId = fpa.SelectedPlanningTypeId.Value;
                        existing.AdministrationId = fpa.SelectedAdministrationTypeId.Value;
                        existing.ServicesRendered = fpa.ServicesRendered;
                        existing.AdministrationType = fpa.AdminType;
                        existing.PlanningType = fpa.AdopAffType;
                        existing.FieldType = fpa.FieldType;
                    }
                    else
                    {
                        _context.HealthCenterChps.Add(new HealthCenterChp
                        {
                            CollegeCode = fpa.CollegeCode ?? collegeCode,
                            FacultyCode = fpa.FacultyCode != 0 ? fpa.FacultyCode : facultyId,
                            NameofHealthCenter = fpa.NameOfCHC,
                            PlanningId = fpa.SelectedPlanningTypeId.Value,
                            AdministrationId = fpa.SelectedAdministrationTypeId.Value,
                            FieldTypeId = fpa.SelectedFieldTypeId.Value,
                            DistanceFromNursingInstitution = distance,
                            ServicesRendered = fpa.ServicesRendered,
                            AdministrationType = fpa.AdminType,
                            PlanningType = fpa.AdopAffType,
                            FieldType = fpa.FieldType,
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Field Practice Areas saved successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        private void ValidateFieldPracticeAreas(FieldPracticeAreasPostViewModel model)
        {
            if (model.FieldPracticeAreas == null || !model.FieldPracticeAreas.Any())
            {
                ModelState.AddModelError("Areas", "No Field Practice Areas submitted.");
                return;
            }

            for (int i = 0; i < model.FieldPracticeAreas.Count; i++)
            {
                var fpa = model.FieldPracticeAreas[i];

                if (!IsTouched(fpa)) continue;

                //if (!fpa.SelectedFieldTypeId.HasValue)
                //    ModelState.AddModelError($"Areas[{i}].SelectedFieldTypeId", "Field Type is required.");

                //if (!fpa.SelectedPlanningTypeId.HasValue)
                //    ModelState.AddModelError($"Areas[{i}].SelectedPlanningTypeId", "Planning Type is required.");

                //if (!fpa.SelectedAdministrationTypeId.HasValue)
                //    ModelState.AddModelError($"Areas[{i}].SelectedAdministrationTypeId", "Administration Type is required.");

                //if (string.IsNullOrWhiteSpace(fpa.NameOfCHC))
                //    ModelState.AddModelError($"Areas[{i}].NameOfCHC", "Name of CHC is required.");

                //else if (fpa.NameOfCHC.Length < 2 || fpa.NameOfCHC.Length > 200)
                //    ModelState.AddModelError($"FieldPracticeAreas[{i}].NameOfCHC", "Name of CHC must be between 2 and 200 characters.");

                //if (string.IsNullOrWhiteSpace(fpa.DistanceFromNursingInstitution))
                //{
                //    ModelState.AddModelError($"Areas[{i}].DistanceFromNursingInstitution", "Distance is required.");
                //}
                //else if (!int.TryParse(fpa.DistanceFromNursingInstitution, out _))
                //{
                //    ModelState.AddModelError($"Areas[{i}].DistanceFromNursingInstitution", "Distance must be numeric.");
                //}

                //if (!string.IsNullOrWhiteSpace(fpa.ServicesRendered) && (fpa.ServicesRendered.Length < 2 || fpa.ServicesRendered.Length > 500))
                //{
                //    ModelState.AddModelError($"FieldPracticeAreas[{i}].ServicesRendered", "Services must be between 2 and 500 characters.");
                //}
            }
        }

        private static bool IsTouched(FieldPracticeAreaViewModel fpa)
        {
            return fpa.SelectedPlanningTypeId.HasValue ||
                   fpa.SelectedAdministrationTypeId.HasValue ||
                   !string.IsNullOrWhiteSpace(fpa.NameOfCHC) ||
                   !string.IsNullOrWhiteSpace(fpa.DistanceFromNursingInstitution);
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


        //private async Task PopulateDropdowns(ClinicalFacilitesViewModel model)
        //{
        //    model.Districts = await _context.DistrictMasters
        //        .Select(d => new DropdownItem
        //        {
        //            Text = d.DistrictName,
        //            Value = d.DistrictId
        //        }).ToListAsync();

        //    model.Taluks = await _context.TalukMasters
        //        .Select(t => new TalukItem
        //        {
        //            TalukID = t.TalukId,
        //            TalukName = t.TalukName,
        //            DistrictID = t.DistrictId
        //        }).ToListAsync();

        //    model.Locations = new List<string> { "Urban", "Semi-Urban", "Rural" };

        //    model.AvailableFacilities = await _context.HospitalFacilitiesMasters
        //        .Where(f => f.AffiliationTypeId == 2 && f.FacultyCode == "3")
        //        .Select(f => new DropdownItem
        //        {
        //            Text = f.FacilityName,
        //            Value = f.FacilityId.ToString()
        //        })
        //        .ToListAsync();
        //}

        public async Task<IActionResult> ViewHospitalDocument(int documentId)
        {
            var doc = await _context.AffiliatedHospitalDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (doc == null) return NotFound();

            // ✅ View inline (no download)
            return File(doc.DocumentFilePth, "application/pdf");
        }

        public async Task<IActionResult> ViewSupportingHospitalDocument(string code)
        {
            var doc = await _context.HospitalDetailsForAffiliations
                .FirstOrDefaultAsync(d => d.CollegeCode == code);

            if (doc.HospitalParentSupportingDoc == null)
                return NotFound();

            // ✅ View inline (no download)
            return File(
                doc.HospitalParentSupportingDoc,
                "application/pdf" // or detect MIME
            );
        }

        public async Task<IActionResult> ViewHospitalDoc(string code, int docId)
        {
            var doc = await _context.HospitalDocumentsToBeUploadeds.FirstOrDefaultAsync(e => e.CollegeCode == code && e.DocumentId == docId);
            if (doc == null) return NotFound();

            return File(doc.HospitalDocumentFile, "application/pdf");
        }

        public async Task<bool> GetSupportDocument(string code)
        {
            var doc = await _context.HospitalDetailsForAffiliations.FirstOrDefaultAsync(d => d.CollegeCode == code);
            return doc?.HospitalParentSupportingDoc != null;
        }

        //    private async Task RebuildDropdowns(HospitalAffiliationCompositeViewModel model)
        //    {
        //        // -----------------------------
        //        // Clinical Facilities dropdowns
        //        // -----------------------------
        //        model.ClinicalFacilities.Districts = await _context.DistrictMasters
        //            .Select(d => new DropdownItem
        //            {
        //                Text = d.DistrictName,
        //                Value = d.DistrictId.ToString()
        //            })
        //            .ToListAsync();

        //        model.ClinicalFacilities.Taluks = await _context.TalukMasters
        //            .Select(t => new TalukItem
        //            {
        //                TalukID = t.TalukId.ToString(),
        //                TalukName = t.TalukName,
        //                DistrictID = t.DistrictId.ToString()
        //            })
        //            .ToListAsync();

        //        //model.ClinicalFacilities.Locations = await _context.Locations
        //        //    .Select(l => l.LocationName)
        //        //    .ToListAsync();

        //        model.ClinicalFacilities.ParentMedicalCollegeExistsOptions = new List<DropdownItem>
        //{
        //    new DropdownItem { Text = "Yes", Value = "true" },
        //    new DropdownItem { Text = "No",  Value = "false" }
        //};

        //        model.ClinicalFacilities.HospitalTypes = await _context.MstHospitalTypes
        //            .Select(ht => new DropdownItem
        //            {
        //                Text = ht.HospitalType,
        //                Value = ht.Id.ToString()
        //            })
        //            .ToListAsync();

        //        model.ClinicalFacilities.HospitalOwnedByOptions = await _context.MstHospitalOwnedBies
        //            .Select(h => new DropdownItem
        //            {
        //                Text = h.OwnedBy,
        //                Value = h.Id.ToString()
        //            })
        //            .ToListAsync();

        //        model.ClinicalFacilities.IsParentHospitalForOtherNursingInstitutionOptions = new List<DropdownItem>
        //{
        //    new DropdownItem { Text = "Yes", Value = "true" },
        //    new DropdownItem { Text = "No",  Value = "false" }
        //};

        //        model.ClinicalFacilities.IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
        //{
        //    new DropdownItem { Text = "Yes", Value = "true" },
        //    new DropdownItem { Text = "No",  Value = "false" }
        //};

        //        model.ClinicalFacilities.AvailableFacilities = await _context.HospitalFacilitiesMasters
        //            .Select(f => new DropdownItem
        //            {
        //                Text = f.FacilityName,
        //                Value = f.FacilityId.ToString()
        //            })
        //            .ToListAsync();


        //        // -------------------------------------
        //        // Field Practice Area dropdowns per row
        //        // -------------------------------------
        //        model.ClinicalFacilities.Locations = new List<string> { "Urban", "Semi-Urban", "Rural" };

        //        if (model.FieldPracticeAreas != null)
        //        {
        //            foreach (var fpa in model.FieldPracticeAreas)
        //            {
        //                if (fpa == null) continue;

        //                fpa.FieldTypeOptions = await _context.MstFieldTypeChps
        //                    .Select(ft => new DropdownItem
        //                    {
        //                        Text = ft.FieldType,
        //                        Value = ft.Id.ToString()
        //                    })
        //                    .ToListAsync();

        //                fpa.AdopAffTypeOptions = await _context.MstFpaAdopAffTypes
        //                    .Select(a => new DropdownItem
        //                    {
        //                        Text = a.FpaType,
        //                        Value = a.Id.ToString()
        //                    })
        //                    .ToListAsync();

        //                fpa.AdministrationTypeOptions = await _context.MstAdministrations
        //                    .Select(a => new DropdownItem
        //                    {
        //                        Text = a.AdministrationType,
        //                        Value = a.Id.ToString()
        //                    })
        //                    .ToListAsync();

        //            }
        //        }

        //        // ----------------------------------------------------
        //        // Hospital Documents To Be Uploaded dropdowns per item
        //        // ----------------------------------------------------

        //        if (model.HospitalDocumentsToBeUploadedList != null)
        //        {
        //            foreach (var doc in model.HospitalDocumentsToBeUploadedList)
        //            {
        //                if (doc == null) continue;

        //                // If you want document types here, uncomment:
        //                //doc.DocumentTypeOptions = await _context.DocumentTypes
        //                //    .Select(d => new DropdownItem
        //                //    {
        //                //        Text = d.DocumentName,
        //                //        Value = d.DocumentId.ToString()
        //                //    })
        //                //    .ToListAsync();
        //            }
        //        }
        //    }


        /*
         * Medical Affiliation
         */
        public async Task<IActionResult> Med_HospitalDetailsForAffiliation()
        {
            var (collegeCode, facultyId) = GetSessionCollegeAndFaculty();


            // Fetch total beds from NursingAffiliatedYearwiseMaterialsData
            var materialsData = await _context.NursingAffiliatedYearwiseMaterialsData
                .Where(n => n.CollegeCode == collegeCode && n.HospitalType != "ParentHospital")
                .ToListAsync();

            var affiliatedDocs = await _context.AffiliatedHospitalDocuments
                .Where(d => d.CollegeCode == collegeCode)
                .ToListAsync();

            // Optional: existing hospital details (may be empty initially)
            var hospitals = await _context.HospitalDetailsForAffiliations
                .Include(h => h.HospitalFacilities)
                .Where(h => h.CollegeCode == collegeCode)
                .ToListAsync();

            var existingHospitalDetailsForAffiliations = await _context.HospitalDetailsForAffiliations.Where(h => h.CollegeCode == collegeCode).FirstOrDefaultAsync();
            var options = await _context.MstHospitalOwnedBies.Where(f => f.FacultyCode == facultyId).ToListAsync();
            var hospitalTypes = await _context.MstHospitalTypes.Where(f => f.FacultyCode == facultyId).ToListAsync();

            var hospital = hospitals.FirstOrDefault();

            var defAffTypes = await _context.TypeOfAffiliations.ToListAsync();
            var vm = new HospitalAffiliationCompositeViewModel
            {
                ClinicalHospitalDetails = new ClinicalHospitalViewModel
                {
                    Form = hospital == null
                    ? new ClinicalHospitalFormVM
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyId.ToString(),
                        AffiliationTypeId = 2,
                        AffiliationType = defAffTypes.Where(f => f.TypeId == 2).Select(e => e.TypeDescription).FirstOrDefault()
                    }
                    : new ClinicalHospitalFormVM
                    {
                        HospitalDetailsId = hospital.HospitalDetailsId,
                        ParentMedicalCollegeExists = hospital.ParentMedicalCollegeExists,
                        HospitalType = hospital.HospitalType,
                        HospitalOwnedBy = hospital.HospitalOwnedBy,
                        HospitalName = hospital.HospitalName,
                        HospitalOwnerName = hospital.HospitalOwnerName,
                        HospitalDistrictId = hospital.HospitalDistrictId,
                        HospitalTalukId = hospital.HospitalTalukId,
                        Location = hospital.Location,
                        IsParentHospitalForOtherNursingInstitution = hospital.IsParentHospitalForOtherNursingInstitution,
                        CollegeCode = hospital.CollegeCode,
                        FacultyCode = hospital.FacultyCode,
                        AffiliationTypeId = hospital.AffiliationTypeId,
                        AffiliationType = defAffTypes.Where(f => f.TypeId == 2).Select(e => e.TypeDescription).FirstOrDefault()
                    },

                    SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
                    Districts = await _context.DistrictMasters
                        .Select(d => new DropdownItem { Text = d.DistrictName, Value = d.DistrictId })
                        .ToListAsync(),
                    Taluks = await _context.TalukMasters
                        .Select(t => new TalukItem
                        {
                            TalukID = t.TalukId,
                            TalukName = t.TalukName,
                            DistrictID = t.DistrictId
                        })
                        .ToListAsync(),
                    Locations = new List<string> { "Urban", "Semi-Urban", "Rural" },
                    ParentMedicalCollegeExistsOptions = new List<DropdownItem>
                    {
                        new() { Text = "Yes", Value = "true" },
                        new() { Text = "No", Value = "false" }
                    },
                    HospitalTypes = hospitalTypes.Select(f => new DropdownItem
                    {
                        Text = f.HospitalType,
                        Value = f.Id.ToString()
                    }).ToList(),

                    HospitalOwnedByOptions = options.Select(f => new DropdownItem
                    {
                        Text = f.OwnedBy,
                        Value = f.Id.ToString(),
                    }).OrderBy(e => e.Text).ToList(),


                    IsParentHospitalForOtherNursingInstitutionOptions = new List<DropdownItem>
                    {
                        new() { Text = "Yes", Value = "true" },
                        new() { Text = "No", Value = "false" }
                    },
                    IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
                    {
                        new() { Text = "Yes", Value = "true" },
                        new() { Text = "No", Value = "false" }
                    },

                    AvailableFacilities = await _context.HospitalFacilitiesMasters
                        .Where(f => f.AffiliationTypeId == 2 && f.FacultyCode == facultyId.ToString())
                        .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
                        .ToListAsync(),
                    IsSupportingDocExists = await GetSupportDocument(collegeCode),
                    HospitalFacilities = new HospitalFacilitiesViewModel
                    {
                        SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
                        AvailableFacilities = await _context.HospitalFacilitiesMasters
                            .Where(f => f.AffiliationTypeId == 2 && f.FacultyCode == facultyId.ToString())
                            .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
                            .ToListAsync(),
                        HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                    }
                },
            };
            return View(vm);
        }

    }
}
