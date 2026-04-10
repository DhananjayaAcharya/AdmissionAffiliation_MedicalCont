using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;

namespace Medical_Affiliation.Services.Handlers.Medical
{
    public class MedicalHospitalHandler : IFacultyHospitalHandler
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public MedicalHospitalHandler(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        private async Task<List<AffiliatedHospitalDocumentsViewModel>> BuildAffiliatedHospitalDocumentsAsync(string collegeCode)
        {
            var materialsData = await _context.NursingAffiliatedYearwiseMaterialsData
                .Where(n => n.CollegeCode == collegeCode && n.HospitalType != "ParentHospital")
                .ToListAsync();

            var affiliatedDocs = await _context.AffiliatedHospitalDocuments
                .Where(d => d.CollegeCode == collegeCode)
                .ToListAsync();

            var hospitals = await _context.HospitalDetailsForAffiliations
                .Where(h => h.CollegeCode == collegeCode)
                .ToListAsync();

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

            if (affiliatedDocs.Any())
            {
                return affiliatedDocs.Select(doc =>
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
                }).ToList();
            }

            return materialSummary.Select(m => new AffiliatedHospitalDocumentsViewModel
            {
                CollegeCode = collegeCode,
                HospitalType = m.Key,
                HospitalName = m.Value.HospitalName,
                TotalBeds = m.Value.TotalBeds,
                DocumentExists = false
            }).ToList();
        }

        public int FacultyId => _userContext.FacultyId;

        private List<TItem> BuildRequirements<TItem>(List<MstIndoorInfrastructureRequirementsMaster> masters, List<IndoorInfrastructureRequirementsCompliance> existing) where TItem : RequirementItemBaseVM, new()
        {
            return masters.Select(m => new TItem
            {
                RequirementId = m.Id,
                RequirementName = m.RequirementName,
                IsAvailable = existing.Any(e => e.RequirementId == m.Id)
            }).ToList();
        }


        public async Task<HospitalAffiliationCompositeViewModel> GetDetailsAsync(string collegeCode)
        {
            var facultyCode = _userContext.FacultyId;
            var seatSlabId = _userContext.SeatSlabId;
            var typeOfAffiliation = _userContext.TypeOfAffiliation;
            var courseLevel = _userContext.CourseLevel;

            // Sequentially await queries to avoid DbContext concurrency issues
            var hospital = await _context.HospitalDetailsForAffiliations
                .Include(h => h.HospitalFacilities)
                .FirstOrDefaultAsync(h => h.CollegeCode == collegeCode && h.CourseLevel == courseLevel);

            var affiliatedDocs = await _context.AffiliatedHospitalDocuments
                .Where(h => h.CollegeCode == collegeCode && h.CourseLevel == courseLevel)
                .ToListAsync();

            var facilities = await _context.HospitalFacilitiesMasters
                .Where(f => f.AffiliationTypeId == typeOfAffiliation && f.FacultyCode == facultyCode.ToString())
                .ToListAsync();

            var districts = await _context.DistrictMasters
                .Select(d => new DropdownItem { Text = d.DistrictName, Value = d.DistrictId })
                .ToListAsync();

            var taluks = await _context.TalukMasters
                .Select(t => new DropdownItem { Text = t.TalukName, Value = t.TalukId })
                .ToListAsync();

            var hospitalOwnedBy = await _context.MstHospitalOwnedBies
                .Where(f => f.FacultyCode == facultyCode)
                .ToListAsync();

            var existingHealthCenterChp = await _context.HealthCenterChps.Where(f => f.CollegeCode == collegeCode).ToListAsync();


            var filedTypes = await _context.MstFieldTypeChps.Where(f => f.FacultyCode == facultyCode).ToListAsync();
            var planningTypes = await _context.MstFpaAdopAffTypes.Where(f => f.FacultyCode == facultyCode).ToListAsync();
            var administrationTypes = await _context.MstAdministrations.Where(f => f.FacultyCode == facultyCode).ToListAsync();
            var existinHospitalDetails = await _context.HospitalDocumentsToBeUploadeds.Where(f => f.CollegeCode == collegeCode).FirstOrDefaultAsync();
            var mstHospitalDocs = await _context.MstHospitalDocuments.Where(f => f.FacultyCode == facultyCode).ToListAsync();
            var existingHospitalDetailsForAffiliations = await _context.HospitalDetailsForAffiliations.Where(h => h.CollegeCode == collegeCode).FirstOrDefaultAsync();
            var options = await _context.MstHospitalOwnedBies.Where(f => f.FacultyCode == facultyCode).ToListAsync();
            //var hospitalTypesList = await _context.MstHospitalTypes
            // .Where(x => x.FacultyCode == facultyCode)
            // .Select(x => new SelectListItem
            // {
            //     Text = x.HospitalType,
            //     Value = x.Id.ToString()
            // })
            // .ToListAsync();

            //var hospitalOwnedByList = await _context.MstHospitalOwnedBies
            //    .Where(x => x.FacultyCode == facultyCode)
            //    .Select(x => new SelectListItem
            //    {
            //        Text = x.OwnedBy,
            //        Value = x.Id.ToString()
            //    })
            //    .ToListAsync();


            var existingSuperVisionInFpa = await _context.SuperVisionInFieldPracticeAreas.Where(f => f.CollegeCode == collegeCode).ToListAsync();

            SuperVisionInFieldPracticeAreaPostVm supervisionVm;

            if (existingSuperVisionInFpa.Any())
            {
                // Grouping is optional if you only expect a single College-Faculty-Hospital combination
                var group = existingSuperVisionInFpa
                    .GroupBy(x => new
                    {
                        x.CollegeCode,
                        x.FacultyCode,
                        x.AffiliationTypeId,
                        x.HospitalDetailsId
                    })
                    .First(); // pick the first group

                supervisionVm = new SuperVisionInFieldPracticeAreaPostVm
                {
                    CollegeCode = group.Key.CollegeCode,
                    FacultyCode = group.Key.FacultyCode,
                    AffiliationTypeId = group.Key.AffiliationTypeId,
                    HospitalDetailsId = group.Key.HospitalDetailsId,
                    ItemsSuperVision = group.Select(x => new SuperVisionInFieldPracticeAreaItemVM
                    {
                        Id = x.Id,
                        Post = x.Post,
                        Name = x.Name,
                        Qualification = x.Qualification,
                        YearOfQualification = x.YearOfQualification.Year,
                        University = x.University,
                        UgFromDate = x.UgFromDate,
                        UgToDate = x.UgToDate,
                        PgFromDate = x.PgFromDate,
                        PgToDate = x.PgToDate,
                        Responsibilities = x.Responsibilities
                    }).ToList()
                };
            }
            else
            {
                // No data, create empty object for UI
                supervisionVm = new SuperVisionInFieldPracticeAreaPostVm
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    AffiliationTypeId = 2, // default
                    HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                    ItemsSuperVision = new List<SuperVisionInFieldPracticeAreaItemVM>
                    {
                        new SuperVisionInFieldPracticeAreaItemVM()
                    }
                };
            }


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

            // Build Form VM
            var formVM = hospital == null
                ? new ClinicalHospitalFormVM
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode.ToString(),
                    AffiliationTypeId = typeOfAffiliation,
                    AffiliationType = "Continuation",
                    //HospitalTypeList = hospitalTypesList,
                    //HospitalOwnedByList = hospitalOwnedByList

                }
                : new ClinicalHospitalFormVM
                {
                    HospitalDetailsId = hospital.HospitalDetailsId,
                    CollegeCode = hospital.CollegeCode,
                    FacultyCode = hospital.FacultyCode,
                    AffiliationTypeId = hospital.AffiliationTypeId,
                    AffiliationType = "Continuation",
                    HospitalName = hospital.HospitalName,
                    HospitalOwnerName = hospital.HospitalOwnerName,
                    HospitalTypeId = hospital.HospitalType,
                    HospitalOwnedById = hospital.HospitalOwnedBy,

                    //HospitalTypeList = hospitalTypesList,
                    //HospitalOwnedByList = hospitalOwnedByList,
                    HospitalType = hospital.HospitalType,
                    HospitalOwnedBy = hospital.HospitalOwnedBy,
                    HospitalDistrictId = hospital.HospitalDistrictId,
                    HospitalTalukId = hospital.HospitalTalukId,
                    Location = hospital.Location,
                    ParentMedicalCollegeExists = hospital.ParentMedicalCollegeExists,
                    IsParentHospitalForOtherNursingInstitution = hospital.IsParentHospitalForOtherNursingInstitution,
                };

            // Build Hospital Facilities VM
            var hospitalFacilitiesVM = new HospitalFacilitiesViewModel
            {
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                AvailableFacilities = facilities
                    .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
                    .ToList(),
                SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
            };

            var hospitalDocuments = await BuildAffiliatedHospitalDocumentsAsync(collegeCode);


            var hospitalTypes = await _context.MstHospitalTypes.Where(f => f.FacultyCode == facultyCode).ToListAsync();

            // Build Clinical Hospital VM
            var clinicalHospitalVM = new ClinicalHospitalViewModel
            {
                Form = formVM,
                HospitalFacilities = hospitalFacilitiesVM,
                Districts = districts,
                Taluks = taluks.Select(t => new TalukItem { TalukName = t.Text, TalukID = t.Value }).ToList(),
                Locations = new List<string> { "Urban", "Semi-Urban", "Rural" },
                HospitalOwnedByOptions = hospitalOwnedBy
                    .Select(h => new DropdownItem { Text = h.OwnedBy, Value = h.Id.ToString() })
                    .ToList(),
                IsSupportingDocExists = hospital?.HospitalParentSupportingDoc != null,
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

            };

            // Build Affiliated Documents VM
            var affiliatedDocsVM = new AffiliatedHospitalDocumentsPostVM
            {
                CollegeCode = collegeCode,
                Documents = affiliatedDocs.Select(d => new AffiliatedHospitalDocumentItemVM
                {
                    HospitalName = d.HospitalName,
                    HospitalType = d.HospitalType,
                    DocumentExists = true
                }).ToList()
            };

            var uploadDocsVM = mstHospitalDocs.Select(dc =>
            {
                var existing = existingHospitalDocs
                    .FirstOrDefault(e => e.DocumentId == dc.Id);

                return new HospitalDocumentsToBeUploadedViewModel
                {
                    CollegeCode = collegeCode,
                    HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                    DocumentId = dc.Id,
                    DocumentName = dc.DocumentName,
                    CertificateNumber = existing?.CertificateNumber,
                    HospitalName = hospital?.HospitalName,
                    DocumentExists = existing != null
                };
            }).ToList();

            var mastersBySection = await _context.MstIndoorInfrastructureRequirementsMasters
                .Where(m =>
                    m.FacultyCode == facultyCode &&
                    m.AffiliationTypeId == typeOfAffiliation &&
                    m.IsActive)
                .ToListAsync();

            var existingBySection = await _context.IndoorInfrastructureRequirementsCompliances
                .Where(r =>
                    r.CollegeCode == collegeCode &&
                    r.FacultyCode == facultyCode &&
                    r.AffiliationTypeId == typeOfAffiliation)
                .ToListAsync();

            var indoorDeptReqMaster = mastersBySection.Where(r => r.SectionCode == "1").ToList();

            var otReqMaster = mastersBySection.Where(m => m.SectionCode == "2").ToList();

            var casualityReqMaster = mastersBySection.Where(r => r.SectionCode == "3").ToList();

            var CSSDandLaundryReqMaster = mastersBySection.Where(r => r.SectionCode == "4").ToList();

            var RadioDiagnosisReqMaster = mastersBySection.Where(r => r.SectionCode == "5").ToList();

            var AnaesthesiologyReqMaster = mastersBySection.Where(r => r.SectionCode == "6").ToList();

            var CentralLaboratoryReqMaster = mastersBySection.Where(r => r.SectionCode == "7").ToList();

            var BloodBankReqMaster = mastersBySection.Where(r => r.SectionCode == "8").ToList();

            var YogaReqMaster = mastersBySection.Where(r => r.SectionCode == "9").ToList();

            var RadiationOncologyReqMaster = mastersBySection.Where(r => r.SectionCode == "10").ToList();

            var ArtCenterReqMaster = mastersBySection.Where(r => r.SectionCode == "11").ToList();

            var PharmacyReqMaster = mastersBySection.Where(r => r.SectionCode == "12").ToList();

            var UtilitiesReqMaster = mastersBySection.Where(r => r.SectionCode == "13").ToList();

            var IndoorBedsUnitsReqMaster = mastersBySection.Where(r => r.SectionCode == "14").ToList();

            var OutpatientAreaReqMaster = mastersBySection.Where(r => r.SectionCode == "15").ToList();

            var departmentsMaster = await _context.MstIndoorBedsDepartmentMasters
                .Where(d => d.FacultyCode == facultyCode)
                .ToListAsync();

            var seatSlabs = await _context.SeatSlabMasters
                .Where(s => s.FacultyCode == facultyCode)
                .OrderBy(s => s.SeatSlab)
                .ToListAsync();

            var bedMasterRequirements = await _context.MstIndoorBedsOccupancyMasters
                .Where(m =>
                    m.FacultyCode == facultyCode &&
                    m.SeatSlabId == seatSlabId &&
                    m.AffiliationTypeId == typeOfAffiliation)
                .ToListAsync();

            var existingBeds = await _context.IndoorBedsOccupancies
                .Where(o =>
                    o.CollegeCode == collegeCode &&
                    o.FacultyCode == facultyCode &&
                    o.SeatSlabId == seatSlabId &&
                    o.AffiliationTypeId == typeOfAffiliation)
                .ToListAsync();



            var existingIndoorRequirement = existingBySection.Where(r => r.SectionCode == "1").ToList();

            var existingOTRequirement = existingBySection.Where(r => r.SectionCode == "2").ToList();


            var existinCasualityRequirement = existingBySection.Where(r => r.SectionCode == "3").ToList();

            var existingCSSDandLaundryRequirement = existingBySection.Where(r => r.SectionCode == "4").ToList();

            var existingRadioDiagnosisRequirement = existingBySection.Where(r => r.SectionCode == "5").ToList();

            var existingAnaesthesiologyRequirement = existingBySection.Where(r => r.SectionCode == "6").ToList();

            var existingCentralLaboratoryRequirement = existingBySection.Where(r => r.SectionCode == "7").ToList();
            var existingBloodBankRequirement = existingBySection.Where(r => r.SectionCode == "8").ToList();

            var existingYogaRequirement = existingBySection.Where(r => r.SectionCode == "9").ToList();

            var existingRadiationOncologyRequirement = existingBySection.Where(r => r.SectionCode == "10").ToList();

            var existingArtCenterRequirement = existingBySection.Where(r => r.SectionCode == "11").ToList();

            var existingPharmacyRequirement = existingBySection.Where(r => r.SectionCode == "12").ToList();

            var existingUtilitiesRequirement = existingBySection.Where(r => r.SectionCode == "13").ToList();

            var existingIndoorBedsUnitsRequirement = existingBySection.Where(r => r.SectionCode == "14").ToList();

            var existingOutpatientAreaRequirement = existingBySection.Where(r => r.SectionCode == "15").ToList();

            var indoorDeptVM = new IndoorDepartmentRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,

                Requirements = BuildRequirements<IndoorDepartmentRequirementItemVM>(
                    indoorDeptReqMaster,
                    existingIndoorRequirement
                )
            };

            var masterLookup = bedMasterRequirements.ToDictionary(m => (m.DepartmentCode, m.SeatSlabId));

            var savedLookup = existingBeds.ToDictionary(o => (o.DepartmentId, o.SeatSlabId));
            var filteredSeatSlabs = seatSlabs.Where(s => s.SeatSlabId == seatSlabId);



            var items =
                (
                    from d in departmentsMaster
                    from s in filteredSeatSlabs
                    let master = masterLookup.GetValueOrDefault((d.DeptId, s.SeatSlabId))
                    let saved = savedLookup.GetValueOrDefault((d.DeptId, s.SeatSlabId))
                    select new IndoorBedsOccupancyItemVM
                    {
                        DepartmentId = d.DeptId,
                        DepartmentName = d.DepartmentName,
                        SeatSlabId = s.SeatSlabId,
                        SeatSlab = s.SeatSlab,

                        RGUHSintake =
                            saved?.Rguhsintake > 0
                                ? saved.Rguhsintake
                                : master?.RequiredBeds ?? 0,

                        CollegeIntake = saved?.CollegeIntake ?? 0
                    }
                ).ToList();


            var clinicalCapacityVM = new ClinicalCapacityViewModel
            {
                Form = hospital == null
                    ? new ClinicalCapacityFormVM
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode.ToString(),
                        AffiliationTypeId = typeOfAffiliation
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
                        AffiliationTypeId = typeOfAffiliation
                    },

                IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
                {
                    new() { Text = "Yes", Value = "true" },
                    new() { Text = "No", Value = "false" }
                }
            };

            // Build composite VM
            var compositeVM = new HospitalAffiliationCompositeViewModel
            {
                ClinicalHospitalDetails = clinicalHospitalVM,
                AffiliatedDocumentsPostVM = affiliatedDocsVM,
                ClinicalCapacity = clinicalCapacityVM,
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                IndoorDepartment = indoorDeptVM,
                SuperVisionInFieldPracticeArea = supervisionVm
            };


            compositeVM.IndoorBedsOccupancy = new IndoorBedsOccupancyPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Items = items,
            };

            //compositeVM.ClinicalHospitalDetails.HospitalFacilities = new HospitalFacilitiesViewModel
            //{
            //    HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
            //    AvailableFacilities = facilities
            //        .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
            //        .ToList(),
            //    SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>(),
            //};

            compositeVM.IndoorBedsUnitsRequirements = new IndoorBedsUnitsRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<IndoorBedsUnitsItemVM>(
                    IndoorBedsUnitsReqMaster,
                    existingIndoorBedsUnitsRequirement
                )
            };

            compositeVM.OTRequirements = new OTRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,

                Requirements = BuildRequirements<OTRequirementItemVM>(
                    otReqMaster,
                    existingOTRequirement
                )
            };


            compositeVM.CasualityRequirements = new CasualityRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<CasualityRequirementItemVM>(
                    casualityReqMaster,
                    existinCasualityRequirement
                )

            };

            compositeVM.CSSDandLaundryRequirements = new CSSDandLaundryRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<CSSDandLaundryItemVM>(
                    CSSDandLaundryReqMaster,
                    existingCSSDandLaundryRequirement
                )
            };


            compositeVM.RadioDiagnosisRequirements = new RadioDiagnosisRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<RadioDiagnosisItemVM>(
                    RadioDiagnosisReqMaster,
                    existingRadioDiagnosisRequirement
                )
            };


            compositeVM.AnaesthesiologyRequirements = new AnaesthesiologyRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<AnaesthesiologyItemVM>(
                    AnaesthesiologyReqMaster,
                    existingAnaesthesiologyRequirement
                )
            };

            compositeVM.CentralLaboratoryRequirements = new CentralLaboratoryRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<CentralLaboratoryItemVM>(
                    CentralLaboratoryReqMaster,
                    existingCentralLaboratoryRequirement
                )
            };

            compositeVM.BloodBankRequirements = new BloodBankRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<BloodBankItemVM>(
                    BloodBankReqMaster,
                    existingBloodBankRequirement
                )
            };

            compositeVM.OutPatientRequirements = new OutPatientRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<OutPatientAreaItemVM>(
                    OutpatientAreaReqMaster,
                    existingOutpatientAreaRequirement
                )
            };

            compositeVM.YogaRequirements = new YogaRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<YogaItemVM>(
                    YogaReqMaster,
                    existingYogaRequirement
                )
            };

            compositeVM.RadiationOncologyRequirements = new RadiationOncologyRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<RadiationOncologyItemVM>(
                    RadiationOncologyReqMaster,
                    existingRadiationOncologyRequirement
                )
            };

            compositeVM.ArtCenterRequirements = new ArtCenterRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<ArtCenterItemVM>(
                    ArtCenterReqMaster,
                    existingArtCenterRequirement
                )
            };

            compositeVM.PharmacyRequirements = new PharmacyRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<PharmacyItemVM>(
                    PharmacyReqMaster,
                    existingPharmacyRequirement
                )
            };

            compositeVM.UtilitiesRequirements = new UtilitiesRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospital?.HospitalDetailsId ?? 0,
                Requirements = BuildRequirements<UtilitiesItemVM>(
                    UtilitiesReqMaster,
                    existingUtilitiesRequirement
                )
            };

            compositeVM.AffiliatedDocumentsPostVM = new AffiliatedHospitalDocumentsPostVM
            {
                CollegeCode = collegeCode,
                Documents = hospitalDocuments.Select(d => new AffiliatedHospitalDocumentItemVM
                {
                    DocumentId = d.DocumentId,
                    HospitalType = d.HospitalType,
                    HospitalName = d.HospitalName,
                    TotalBeds = d.TotalBeds,
                    DocumentExists = d.DocumentExists
                }).ToList()
            };


            // Add dynamic sections
            compositeVM.Sections.Add(new SectionViewModel
            {
                Name = "ClinicalSection",
                SubSections = new List<string>
                {
                    "InstructionSet",
                    "HospitalDetails",
                    "ClinicalCapacity",
                    "AffiliatedDocuments",
                    "CasualityRequirements",
                    "CSSDandLaundryRequirements",
                    "RadioDiagnosis",
                    "Anaesthesiology",
                    "CentralLaboratory",
                    "BloodBank",
                    "Yoga",
                    "RadiationOncology",
                    "ArtCenter",
                    "Pharmacy",
                    "SuperVisionInFieldPracticeArea",
                    "OutPatientArea",
                    //"FieldPracticeArea",
                    //"Facilities",
                    "IndoorBedsOccupancy",
                    "IndoorBedsUnits",
                    "IndoorDepartment",
                    "Utilities",
                    "OT",
                    //"HospitalDocumentsToBeUploaded"
                }
            });
            return compositeVM;
        }



    }
}
