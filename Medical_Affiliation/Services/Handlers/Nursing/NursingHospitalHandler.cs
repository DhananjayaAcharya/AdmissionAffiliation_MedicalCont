//using Medical_Affiliation.DATA;
//using Medical_Affiliation.Models;
//using Medical_Affiliation.Services.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.VisualBasic.FileIO;

//namespace Medical_Affiliation.Services.Handlers.Nursing
//{
//    public class NursingHospitalHandler : IFacultyHospitalHandler
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IUserContext _userContext;

//        public NursingHospitalHandler(ApplicationDbContext context, IUserContext userContext)
//        {
//            _context = context;
//            _userContext = userContext;
//        }
//        public int FacultyId => 3;

//        private async Task<List<AffiliatedHospitalDocumentsViewModel>>BuildAffiliatedHospitalDocumentsAsync(string collegeCode)
//        {
//            var materialsData = await _context.NursingAffiliatedYearwiseMaterialsData
//                .Where(n => n.CollegeCode == collegeCode && n.HospitalType != "ParentHospital")
//                .ToListAsync();

//            var affiliatedDocs = await _context.AffiliatedHospitalDocuments
//                .Where(d => d.CollegeCode == collegeCode)
//                .ToListAsync();

//            var hospitals = await _context.HospitalDetailsForAffiliations
//                .Where(h => h.CollegeCode == collegeCode)
//                .ToListAsync();

//            var materialSummary = materialsData
//                .GroupBy(m => m.HospitalType)
//                .ToDictionary(
//                    g => g.Key!,
//                    g => new
//                    {
//                        TotalBeds = g.Sum(m =>
//                        {
//                            int kpme = int.TryParse(m.Kpmebeds, out var k) ? k : 0;
//                            int post = int.TryParse(m.PostBasicBeds, out var p) ? p : 0;
//                            return kpme + post;
//                        }),
//                        HospitalName =
//                            hospitals.FirstOrDefault(h => h.HospitalType == g.Key)?.HospitalName
//                            ?? g.FirstOrDefault()?.ParentHospitalName
//                            ?? string.Empty
//                    });

//            if (affiliatedDocs.Any())
//            {
//                return affiliatedDocs.Select(doc =>
//                {
//                    materialSummary.TryGetValue(doc.HospitalType!, out var material);

//                    return new AffiliatedHospitalDocumentsViewModel
//                    {
//                        CollegeCode = collegeCode,
//                        HospitalType = doc.HospitalType!,
//                        HospitalName = doc.HospitalName ?? material?.HospitalName ?? string.Empty,
//                        TotalBeds = doc.TotalBeds ?? material?.TotalBeds ?? 0,
//                        DocumentExists = true,
//                        DocumentId = doc.DocumentId,
//                        DocumentName = doc.DocumentName
//                    };
//                }).ToList();
//            }

//            return materialSummary.Select(m => new AffiliatedHospitalDocumentsViewModel
//            {
//                CollegeCode = collegeCode,
//                HospitalType = m.Key,
//                HospitalName = m.Value.HospitalName,
//                TotalBeds = m.Value.TotalBeds,
//                DocumentExists = false
//            }).ToList();
//        }

//        public async Task<HospitalAffiliationCompositeViewModel> GetDetailsAsync(string collegeCode)
//        {
//            var facultyCode = _userContext.FacultyId;

//            // Sequentially await queries to avoid DbContext concurrency issues
//            var hospital = await _context.HospitalDetailsForAffiliations
//                .Include(h => h.HospitalFacilities)
//                .FirstOrDefaultAsync(h => h.CollegeCode == collegeCode);

//            var affiliatedDocs = await _context.AffiliatedHospitalDocuments
//                .Where(h => h.CollegeCode == collegeCode)
//                .ToListAsync();

//            var facilities = await _context.HospitalFacilitiesMasters
//                .Where(f => f.AffiliationTypeId == 2 && f.FacultyCode == facultyCode.ToString())
//                .ToListAsync();

//            var districts = await _context.DistrictMasters
//                .Select(d => new DropdownItem { Text = d.DistrictName, Value = d.DistrictId })
//                .ToListAsync();

//            var taluks = await _context.TalukMasters
//                .Select(t => new DropdownItem { Text = t.TalukName, Value = t.TalukId })
//                .ToListAsync();

//            var hospitalOwnedBy = await _context.MstHospitalOwnedBies
//                .Where(f => f.FacultyCode == facultyCode)
//                .ToListAsync();

//            var existingHealthCenterChp = await _context.HealthCenterChps.Where(f => f.CollegeCode == collegeCode).ToListAsync();


//            var filedTypes = await _context.MstFieldTypeChps.Where(f => f.FacultyCode == facultyCode).ToListAsync();
//            var planningTypes = await _context.MstFpaAdopAffTypes.Where(f => f.FacultyCode == facultyCode).ToListAsync();
//            var administrationTypes = await _context.MstAdministrations.Where(f => f.FacultyCode == facultyCode).ToListAsync();
//            var existinHospitalDetails = await _context.HospitalDocumentsToBeUploadeds.Where(f => f.CollegeCode == collegeCode).FirstOrDefaultAsync();
//            var mstHospitalDocs = await _context.MstHospitalDocuments.Where(f => f.FacultyCode == facultyCode).ToListAsync();
//            var existingHospitalDetailsForAffiliations = await _context.HospitalDetailsForAffiliations.Where(h => h.CollegeCode == collegeCode).FirstOrDefaultAsync();

//            List<HospitalDocumentsToBeUploaded> existingHospitalDocs;

//            if (existinHospitalDetails != null)
//            {
//                existingHospitalDocs = await _context.HospitalDocumentsToBeUploadeds
//                    .Where(f => f.CollegeCode == collegeCode
//                             && f.HospitalDetailsId == existinHospitalDetails.HospitalDetailsId)
//                    .ToListAsync();
//            }
//            else
//            {
//                existingHospitalDocs = new List<HospitalDocumentsToBeUploaded>();
//            }

//            var hospitalDocuments = await BuildAffiliatedHospitalDocumentsAsync(collegeCode);



//            // Build Form VM
//            var formVM = hospital == null
//                ? new ClinicalHospitalFormVM
//                {
//                    CollegeCode = collegeCode,
//                    FacultyCode = facultyCode.ToString(),
//                    AffiliationTypeId = 2,
//                    AffiliationType = "Continuation"
//                }
//                : new ClinicalHospitalFormVM
//                {
//                    HospitalDetailsId = hospital.HospitalDetailsId,
//                    CollegeCode = hospital.CollegeCode,
//                    FacultyCode = hospital.FacultyCode,
//                    AffiliationTypeId = hospital.AffiliationTypeId,
//                    AffiliationType = "Nursing",
//                    HospitalName = hospital.HospitalName,
//                    HospitalOwnerName = hospital.HospitalOwnerName,
//                    HospitalType = hospital.HospitalType,
//                    HospitalOwnedBy = hospital.HospitalOwnedBy,
//                    HospitalDistrictId = hospital.HospitalDistrictId,
//                    HospitalTalukId = hospital.HospitalTalukId,
//                    Location = hospital.Location,
//                    ParentMedicalCollegeExists = hospital.ParentMedicalCollegeExists,
//                    IsParentHospitalForOtherNursingInstitution = hospital.IsParentHospitalForOtherNursingInstitution,
//                };

//            // Build Hospital Facilities VM
//            var hospitalFacilitiesVM = new HospitalFacilitiesViewModel
//            {
//                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
//                AvailableFacilities = facilities
//                    .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
//                    .ToList(),
//                SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
//            };

//            // Build Clinical Hospital VM
//            var clinicalHospitalVM = new ClinicalHospitalViewModel
//            {
//                Form = formVM,
//                HospitalFacilities = hospitalFacilitiesVM,
//                Districts = districts,
//                Taluks = taluks.Select(t => new TalukItem { TalukName = t.Text, TalukID = t.Value }).ToList(),
//                Locations = new List<string> { "Urban", "Semi-Urban", "Rural" },
//                HospitalOwnedByOptions = hospitalOwnedBy
//                    .Select(h => new DropdownItem { Text = h.OwnedBy, Value = h.OwnedBy.ToString() })
//                    .ToList(),
//                IsSupportingDocExists = hospital?.HospitalParentSupportingDoc != null
//            };

//            // Build Affiliated Documents VM
//            var affiliatedDocsVM = new AffiliatedHospitalDocumentsPostVM
//            {
//                CollegeCode = collegeCode,
//                Documents = affiliatedDocs.Select(d => new AffiliatedHospitalDocumentItemVM
//                {
//                    HospitalName = d.HospitalName,
//                    HospitalType = d.HospitalType,
//                    DocumentExists = true
//                }).ToList()
//            };

//            var uploadDocsVM = mstHospitalDocs.Select(dc =>
//            {
//                var existing = existingHospitalDocs
//                    .FirstOrDefault(e => e.DocumentId == dc.Id);

//                return new HospitalDocumentsToBeUploadedViewModel
//                {
//                    CollegeCode = collegeCode,
//                    HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
//                    DocumentId = dc.Id,
//                    DocumentName = dc.DocumentName,
//                    CertificateNumber = existing?.CertificateNumber,
//                    HospitalName = hospital?.HospitalName,
//                    DocumentExists = existing != null
//                };
//            }).ToList();

//            // Build composite VM
//            var compositeVM = new HospitalAffiliationCompositeViewModel
//            {
//                ClinicalHospitalDetails = clinicalHospitalVM,
//                AffiliatedDocumentsPostVM = affiliatedDocsVM,
//                FacultyCode = facultyCode,
//                CollegeCode = collegeCode
//            };

//            // Add dynamic sections
//            compositeVM.Sections.Add(new SectionViewModel
//            {
//                Name = "ClinicalSection",
//                SubSections = new List<string>
//                {
//                    "InstructionSet",
//                    "HospitalDetails",
//                    "ClinicalCapacity",
//                    "AffiliatedDocuments",
//                    "FieldPracticeArea",
//                    "Facilities",
//                    "HospitalDocumentsToBeUploaded"
//                }
//            });

//            compositeVM.AffiliatedDocumentsPostVM = new AffiliatedHospitalDocumentsPostVM
//            {
//                CollegeCode = collegeCode,
//                Documents = hospitalDocuments.Select(d => new AffiliatedHospitalDocumentItemVM
//                {
//                    DocumentId = d.DocumentId,
//                    HospitalType = d.HospitalType,
//                    HospitalName = d.HospitalName,
//                    TotalBeds = d.TotalBeds,
//                    DocumentExists = d.DocumentExists
//                }).ToList()
//            };
//            compositeVM.FieldPracticeAreaPostVM = new FieldPracticeAreasPostViewModel
//            {
//                FieldPracticeAreas = filedTypes.Select(ft =>
//                {
//                    var existing = existingHealthCenterChp.FirstOrDefault(e => e.FieldTypeId == ft.Id);

//                    return new FieldPracticeAreaViewModel
//                    {
//                        SelectedFieldTypeId = ft.Id,
//                        FieldType = ft.FieldType,
//                        CollegeCode = collegeCode,
//                        FacultyCode = facultyCode,

//                        SelectedPlanningTypeId = existing?.PlanningId,
//                        AdopAffType = existing?.PlanningType,

//                        SelectedAdministrationTypeId = existing?.AdministrationId,
//                        AdminType = existing?.AdministrationType,

//                        NameOfCHC = existing?.NameofHealthCenter ?? string.Empty,
//                        ServicesRendered = existing?.ServicesRendered ?? string.Empty,
//                        DistanceFromNursingInstitution =
//                            existing?.DistanceFromNursingInstitution.ToString() ?? string.Empty,

//                        FieldTypeOptions = filedTypes.Select(f => new DropdownItem
//                        {
//                            Text = f.FieldType,
//                            Value = f.Id.ToString()
//                        }).ToList(),

//                        AdopAffTypeOptions = planningTypes.Select(f => new DropdownItem
//                        {
//                            Text = f.FpaType,
//                            Value = f.Id.ToString()
//                        }).ToList(),

//                        AdministrationTypeOptions = administrationTypes.Select(e => new DropdownItem
//                        {
//                            Text = e.AdministrationType,
//                            Value = e.Id.ToString()
//                        }).ToList()
//                    };
//                }).ToList()
//            };

//            compositeVM.ClinicalCapacity = new ClinicalCapacityViewModel
//            {
//                Form = hospital == null
//                    ? new ClinicalCapacityFormVM
//                    {
//                        CollegeCode = collegeCode,
//                        FacultyCode = facultyCode.ToString(),
//                        AffiliationTypeId = 2,
//                    }
//                    : new ClinicalCapacityFormVM
//                    {
//                        HospitalDetailsId = hospital.HospitalDetailsId,
//                        TotalBeds = hospital.TotalBeds,
//                        OpdperDay = hospital.OpdperDay,
//                        IpdbedOccupancyPercent = hospital.IpdbedOccupancyPercent,
//                        AnnualOpdprevYear = hospital.AnnualOpdprevYear,
//                        AnnualIpdprevYear = hospital.AnnualIpdprevYear,
//                        DistanceBetweenCollegeAndHospitalKm = hospital.DistanceBetweenCollegeAndHospitalKm,
//                        IsOwnerAmemberOfTrust = hospital.IsOwnerAmemberOfTrust,
//                        CollegeCode = hospital.CollegeCode,
//                        FacultyCode = hospital.FacultyCode,
//                        AffiliationTypeId = 2,
//                    },
//                IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
//                {
//                    new() { Text = "Yes", Value = "true" },
//                    new() { Text = "No", Value = "false" }
//                }
//            };

//            compositeVM.ClinicalHospitalDetails.HospitalFacilities = new HospitalFacilitiesViewModel
//            {
//                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
//                AvailableFacilities = facilities
//                    .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
//                    .ToList(),
//                SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
//            };

//            compositeVM.HospitalDocumentsToBeUploadedList = mstHospitalDocs.Select(dc =>
//            {
//                var existing = existingHospitalDocs.FirstOrDefault(e => e.DocumentId == dc.Id);
//                return new HospitalDocumentsToBeUploadedViewModel
//                {
//                    CollegeCode = collegeCode,
//                    HospitalDetailsId = existingHospitalDetailsForAffiliations?.HospitalDetailsId ?? 0,
//                    DocumentId = dc.Id,
//                    DocumentName = dc.DocumentName,
//                    CertificateNumber = existing?.CertificateNumber,
//                    HospitalName = existingHospitalDetailsForAffiliations?.HospitalName,
//                    DocumentExists = existing != null
//                };
//            }).ToList();

//            return compositeVM;
//        }



//    }
//}
