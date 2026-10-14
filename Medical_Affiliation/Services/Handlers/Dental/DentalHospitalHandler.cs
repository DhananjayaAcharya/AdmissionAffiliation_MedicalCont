using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Handlers.Medical
{
    public class DentalHospitalHandler : IFacultyHospitalHandler
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DentalHospitalHandler(ApplicationDbContext context, IUserContext userContext, IHttpContextAccessor accessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = accessor;
        }

        public int FacultyId => 2;

        // ------------------------------------------------------------------
        // Affiliated hospital documents
        // ------------------------------------------------------------------
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

        // ------------------------------------------------------------------
        // Requirement builders
        // ------------------------------------------------------------------
        private List<TItem> BuildRequirements<TItem>(
            List<MstIndoorInfrastructureRequirementsMaster> masters,
            List<IndoorInfrastructureRequirementsCompliance> existing)
            where TItem : RequirementItemBaseVM, new()
        {
            return masters.Select(m => new TItem
            {
                RequirementId = m.Id,
                RequirementName = m.RequirementName,
                IsAvailable = existing.FirstOrDefault(e => e.RequirementId == m.Id)?.IsCompliant ?? false
            }).ToList();
        }

        private List<TItem> BuildDentalRequirements<TItem>(
            List<MstDentalService> masters,
            List<DentalService> existing,
            int hospitalDetailsId)
            where TItem : RequirementDentalItemBaseVM, new()
        {
            return masters.Select(m =>
            {
                var existingItem = existing.FirstOrDefault(e =>
                    e.RequirementId == m.Id && e.AvailabilityStatus == true);

                return new TItem
                {
                    RequirementId = m.Id,
                    RequirementName = m.RequirementName,
                    IsAvailable = existingItem?.AvailabilityStatus,
                    SectionCode = m.SectionCode,
                    HospitalDetailsId = hospitalDetailsId
                };
            }).ToList();
        }

        // ------------------------------------------------------------------
        // Main entry point
        // ------------------------------------------------------------------
        public async Task<HospitalAffiliationCompositeViewModel> GetDetailsAsync(string collegeCode, string courseLevel)
        {
            var facultyCode = _userContext.FacultyId;
            var seatSlabId = _userContext.SeatSlabId;
            var typeOfAffiliation = _userContext.TypeOfAffiliation;

            // Fallback if the caller passed nothing
            if (string.IsNullOrWhiteSpace(courseLevel))
            {
                courseLevel = _userContext.CourseLevel
                    ?? _httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                    ?? string.Empty;
            }

            // Queries are awaited sequentially to avoid DbContext concurrency issues

            // ---------------- Hospital ----------------
            var hospital = await _context.HospitalDetailsForAffiliations
                .Include(h => h.HospitalFacilities)
                .FirstOrDefaultAsync(h => h.CollegeCode == collegeCode && h.CourseLevel == courseLevel);

            var hospitalDetailsId = hospital?.HospitalDetailsId ?? 0;
            var dentalDocuments = await _context.InstitutionBasicDetails
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString()).FirstOrDefaultAsync();


            // ---------------- Tie-ups ----------------
            var hospitalTieUps = new List<HospitalTieUpDetailVM>();

            if (hospital != null)
            {
                hospitalTieUps = await _context.HospitalTieUpDetails
                    .AsNoTracking()
                    .Where(x =>
                        x.HospitalDetailsId == hospital.HospitalDetailsId &&
                        x.CollegeCode == hospital.CollegeCode &&
                        x.CourseLevel == courseLevel &&
                        x.FacultyCode == facultyCode &&
                        !x.IsDeleted)
                    .Select(x => new HospitalTieUpDetailVM
                    {
                        Id = x.Id,
                        HospitalDetailsId = x.HospitalDetailsId,
                        CollegeCode = x.CollegeCode,
                        FacultyCode = x.FacultyCode,
                        CourseLevel = x.CourseLevel,
                        TieUpType = x.TieUpType,
                        HospitalName = x.HospitalName,
                        HospitalAddress = x.HospitalAddress,
                        TieUpDetails = x.TieUpDetails,
                        SupportingDocumentPath = x.SupportingDocumentPath,
                        SupportingDocumentName = x.SupportingDocumentName
                    })
                    .ToListAsync();
            }

            // ---------------- Form VM ----------------
            var formVM = hospital == null
                ? new ClinicalHospitalFormVM
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode.ToString(),
                    AffiliationTypeId = typeOfAffiliation,
                    AffiliationType = "Continuation",
                    HasHospitalTieUp = null,
                    HospitalTieUps = new List<HospitalTieUpDetailVM>()
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
                    HospitalType = hospital.HospitalType,
                    HospitalOwnedBy = hospital.HospitalOwnedBy,
                    HospitalDistrictId = hospital.HospitalDistrictId,
                    HospitalTalukId = hospital.HospitalTalukId,
                    Location = hospital.Location,
                    ParentMedicalCollegeExists = hospital.ParentMedicalCollegeExists,
                    IsParentHospitalForOtherNursingInstitution = hospital.IsParentHospitalForOtherNursingInstitution,
                    DrugFreeCampusCertificationPath = hospital.DrugFreeCampusCertificationPdfPath,
                    KPMECertificatePath = hospital.KpmecertificatePdfPath,
                    ProposedPlansForFutureDevelopmentsPath = hospital.ProposedPlansForFutureDevelopmentsPdfPath,
                    PollutionControlBoardCertificatePath = hospital.PollutionControlBoardCertificatePdfPath,
                    BioMedicalCertificatePath = hospital.BioMedicalCertificatePdfPath,

                    DciCertificationPath = dentalDocuments.DcicertificateFilePath,
                    ContinuousAffiliationPath = dentalDocuments.ContinuationAffiliationFilePath,
                    KsdcCertificationPath = dentalDocuments.KsdccertificateFilePath,

                    HasAnatomyActRegistration = hospital.HasAnatomyActRegistration,
                    AnatomyActRegistrationDetails = hospital.AnatomyActRegistrationDetails,
                    AnatomyActRegistrationPdfPath = hospital.AnatomyActRegistrationPdfPath,
                    HasHospitalTieUp = hospital.HasHospitalTieUp,
                    HospitalTieUps = hospitalTieUps
                };

            // ---------------- Dropdown / master data ----------------
            var facilities = await _context.HospitalFacilitiesMasters
                .AsNoTracking()
                .Where(f => f.AffiliationTypeId == typeOfAffiliation && f.FacultyCode == facultyCode.ToString())
                .OrderBy(e => e.FacilityName)
                .ToListAsync();

            var districts = await _context.DistrictMasters
                .AsNoTracking()
                .Select(d => new DropdownItem { Text = d.DistrictName, Value = d.DistrictId })
                .ToListAsync();

            var taluks = await _context.TalukMasters
                .AsNoTracking()
                .Select(t => new DropdownItem { Text = t.TalukName, Value = t.TalukId })
                .ToListAsync();

            var hospitalOwnedBy = await _context.MstHospitalOwnedBies
                .AsNoTracking()
                .Where(f => f.FacultyCode == facultyCode)
                .ToListAsync();

            var hospitalTypes = await _context.MstHospitalTypes
                .AsNoTracking()
                .Where(f => f.FacultyCode == facultyCode)
                .ToListAsync();

            // ---------------- Supervision in field practice area ----------------
            var existingSuperVisionInFpa = await _context.SuperVisionInFieldPracticeAreas
                .AsNoTracking()
                .Where(f => f.CollegeCode == collegeCode && f.CourseLevel == courseLevel)
                .ToListAsync();

            SuperVisionInFieldPracticeAreaPostVm supervisionVm;

            if (existingSuperVisionInFpa.Any())
            {
                var group = existingSuperVisionInFpa
                    .GroupBy(x => new
                    {
                        x.CollegeCode,
                        x.FacultyCode,
                        x.AffiliationTypeId,
                        x.HospitalDetailsId
                    })
                    .First();

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
                supervisionVm = new SuperVisionInFieldPracticeAreaPostVm
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    AffiliationTypeId = 2,
                    HospitalDetailsId = hospitalDetailsId,
                    ItemsSuperVision = new List<SuperVisionInFieldPracticeAreaItemVM>
                    {
                        new SuperVisionInFieldPracticeAreaItemVM()
                    }
                };
            }

            // ---------------- Facilities VM ----------------
            var hospitalFacilitiesVM = new HospitalFacilitiesViewModel
            {
                HospitalDetailsId = hospitalDetailsId,
                AvailableFacilities = facilities
                    .Select(f => new DropdownItem { Text = f.FacilityName, Value = f.FacilityId.ToString() })
                    .ToList(),
                SelectedFacilityIds = hospital?.HospitalFacilities.Select(f => f.FacilityId).ToList() ?? new List<int>()
            };

            var hospitalDocuments = await BuildAffiliatedHospitalDocumentsAsync(collegeCode);

            // ---------------- Clinical hospital VM ----------------
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
                }
            };

            // ---------------- Requirement masters ----------------
            var mastersBySection = await _context.MstIndoorInfrastructureRequirementsMasters
                .AsNoTracking()
                .Where(m =>
                    m.FacultyCode == facultyCode &&
                    m.AffiliationTypeId == typeOfAffiliation &&
                    m.IsActive)
                .ToListAsync();

            var mastersDentalBySection = await _context.MstDentalServices
                .AsNoTracking()
                .Where(m => m.FacultyCode == facultyCode && m.IsActive)
                .ToListAsync();

            var mstDentalAffHospitalWardBedDistribution = await _context.MstDentalBedDistributions
                .AsNoTracking()
                .Where(e => e.FacultyCode == facultyCode)
                .ToListAsync();

            // ---------------- Saved requirement data ----------------
            var existingBySection = await _context.IndoorInfrastructureRequirementsCompliances
                .AsNoTracking()
                .Where(r =>
                    r.CollegeCode == collegeCode &&
                    r.FacultyCode == facultyCode &&
                    r.AffiliationTypeId == typeOfAffiliation &&
                    r.CourseLevel == courseLevel)
                .ToListAsync();

            var existingDentalBySection = await _context.DentalServices
                .AsNoTracking()
                .Where(r =>
                    r.CollegeCode == collegeCode &&
                    r.FacultyCode == facultyCode &&
                    r.AffiliationTypeId == typeOfAffiliation)
                .ToListAsync();

            // ---------------- Masters by section ----------------
            var indoorDeptReqMaster = mastersBySection.Where(r => r.SectionCode == "1").ToList();
            var otReqMaster = mastersBySection.Where(r => r.SectionCode == "2").ToList();
            var casualityReqMaster = mastersBySection.Where(r => r.SectionCode == "3").ToList();
            var cssdAndLaundryReqMaster = mastersBySection.Where(r => r.SectionCode == "4").ToList();
            var radioDiagnosisReqMaster = mastersBySection.Where(r => r.SectionCode == "5").ToList();
            var anaesthesiologyReqMaster = mastersBySection.Where(r => r.SectionCode == "6").ToList();
            var centralLaboratoryReqMaster = mastersBySection.Where(r => r.SectionCode == "7").ToList();
            var bloodBankReqMaster = mastersBySection.Where(r => r.SectionCode == "8").ToList();
            var yogaReqMaster = mastersBySection.Where(r => r.SectionCode == "9").ToList();
            var radiationOncologyReqMaster = mastersBySection.Where(r => r.SectionCode == "10").ToList();
            var artCenterReqMaster = mastersBySection.Where(r => r.SectionCode == "11").ToList();
            var pharmacyReqMaster = mastersBySection.Where(r => r.SectionCode == "12").ToList();
            var utilitiesReqMaster = mastersBySection.Where(r => r.SectionCode == "13").ToList();
            var indoorBedsUnitsReqMaster = mastersBySection.Where(r => r.SectionCode == "14").ToList();
            var outpatientAreaReqMaster = mastersBySection.Where(r => r.SectionCode == "15").ToList();

            var nptaReqMaster = mastersDentalBySection.Where(r => r.SectionCode == 1).OrderBy(e => e.RequirementName).ToList();
            var engAlliedReqMaster = mastersDentalBySection.Where(r => r.SectionCode == 2).OrderBy(e => e.RequirementName).ToList();
            var admAncReqMaster = mastersDentalBySection.Where(r => r.SectionCode == 3).OrderBy(e => e.RequirementName).ToList();

            // ---------------- Indoor beds data ----------------
            var departmentsMaster = await _context.MstIndoorBedsDepartmentMasters
                .AsNoTracking()
                .Where(d => d.FacultyCode == facultyCode)
                .ToListAsync();

            var seatSlabs = await _context.SeatSlabMasters
                .AsNoTracking()
                .Where(s => s.FacultyCode == facultyCode)
                .OrderBy(s => s.SeatSlab)
                .ToListAsync();

            var seatSlab = seatSlabs
                .Where(e => e.SeatSlabId == seatSlabId)
                .Select(e => e.SeatSlab)
                .FirstOrDefault();

            var bedMasterRequirements = await _context.MstIndoorBedsOccupancyMasters
                .AsNoTracking()
                .Where(m =>
                    m.FacultyCode == facultyCode &&
                    m.SeatSlabId == seatSlabId &&
                    m.AffiliationTypeId == typeOfAffiliation)
                .ToListAsync();

            var disciplineMst = await _context.MstMedicalAlliedDisciplines
                .AsNoTracking()
                .Where(e => e.FacultyCode == facultyCode)
                .OrderBy(e => e.DisciplineName)
                .ToListAsync();

            var existingBeds = await _context.IndoorBedsOccupancies
                .AsNoTracking()
                .Where(o =>
                    o.CollegeCode == collegeCode &&
                    o.FacultyCode == facultyCode &&
                    o.CourseLevel == courseLevel &&
                    o.SeatSlabId == seatSlabId &&
                    o.AffiliationTypeId == typeOfAffiliation)
                .ToListAsync();

            // ---------------- Saved rows by section ----------------
            var existingIndoorRequirement = existingBySection.Where(r => r.SectionCode == "1").ToList();
            var existingOTRequirement = existingBySection.Where(r => r.SectionCode == "2").ToList();
            var existingCasualityRequirement = existingBySection.Where(r => r.SectionCode == "3").ToList();
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

            // NOTE: no OrderBy on e.Requirement.* here - the navigation is not loaded,
            // and BuildDentalRequirements already iterates the (ordered) masters.
            var existingNPTAServices = existingDentalBySection.Where(r => r.SectionCode == 1).ToList();
            var existingEngAlliedServices = existingDentalBySection.Where(r => r.SectionCode == 2).ToList();
            var existingAdmAncServices = existingDentalBySection.Where(r => r.SectionCode == 3).ToList();

            var existingDiscipline = await _context.MedicalAlliedDisciplineDetails
                .AsNoTracking()
                .Where(e => e.FacultyCode == facultyCode && e.CollegeCode == collegeCode)
                .ToListAsync();

            // ---------------- Indoor department VM ----------------
            var indoorDeptVM = new IndoorDepartmentRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<IndoorDepartmentRequirementItemVM>(
                    indoorDeptReqMaster,
                    existingIndoorRequirement)
            };

            // ---------------- Indoor beds occupancy (duplicate-safe lookups) ----------------
            var masterLookup = bedMasterRequirements
                .GroupBy(m => (m.DepartmentCode, m.SeatSlabId))
                .ToDictionary(g => g.Key, g => g.First());

            var savedLookup = existingBeds
                .GroupBy(o => (o.DepartmentId, o.SeatSlabId))
                .ToDictionary(g => g.Key, g => g.First());

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

            var existingDentalAffHospitalWardBedDistribution = await _context.DentalWardBedDistributions
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode)
                .ToListAsync();

            // ---------------- Clinical capacity VM ----------------
            var clinicalCapacityVM = new ClinicalCapacityViewModel
            {
                hospitalDetailsId = hospitalDetailsId,
                SeatSlab = seatSlab,
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
                        DistanceBetweenCollegeAndHospitalKm = hospital.DistanceBetweenCollegeAndHospitalKm,
                        IsOwnerAmemberOfTrust = hospital.IsOwnerAmemberOfTrust,
                        CollegeCode = hospital.CollegeCode,
                        FacultyCode = hospital.FacultyCode,
                        AffiliationTypeId = typeOfAffiliation,
                        DentalChairsCount = hospital.DentalChairsCount,
                        Has24HourEmergency = hospital.Has24HourEmergency,
                        HasCriticalCareServices = hospital.HasCriticalCareServices
                    },
                IsOwnerAmemberOfTrustOptions = new List<DropdownItem>
                {
                    new() { Text = "Yes", Value = "true" },
                    new() { Text = "No", Value = "false" }
                }
            };

            // ---------------- Disciplines ----------------
            var disciplines = disciplineMst.Select(e =>
            {
                var existing = existingDiscipline.FirstOrDefault(x => x.DisciplineCode == e.DisciplineCode);

                return new DisciplineVm
                {
                    DisciplineCode = e.DisciplineCode,
                    DisciplineName = e.DisciplineName,
                    SeatIntake = existing?.Intake ?? 0,
                    SeatSlab = existing?.SeatSlab ?? seatSlab,
                    IsSelected = existing?.IsActive ?? false,
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode.ToString(),
                    AffiliationTypeId = typeOfAffiliation,
                    HospitalDetailsId = existing?.HospitalDetailsId ?? 0
                };
            }).ToList();

            var disciplineVm = new DisciplinePostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode.ToString(),
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Disciplines = disciplines,
                SeatSlab = seatSlab
            };

            // ---------------- Composite VM ----------------
            var compositeVM = new HospitalAffiliationCompositeViewModel
            {
                ClinicalHospitalDetails = clinicalHospitalVM,
                ClinicalCapacity = clinicalCapacityVM,
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                IndoorDepartment = indoorDeptVM,
                SuperVisionInFieldPracticeArea = supervisionVm,
                DisciplineVm = disciplineVm
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

            // ---------------- Dental-specific sections ----------------
            compositeVM.NptaRequirementPostvm = new NPTARequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                SeatSlab = seatSlab,
                Requirements = BuildDentalRequirements<NPTAServicesItemVM>(
                    nptaReqMaster, existingNPTAServices, hospitalDetailsId)
            };

            compositeVM.EngAlliedRequirementPostvm = new EngAlliedRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                SeatSlab = seatSlab,
                Requirements = BuildDentalRequirements<EngAlliedServicesItemVM>(
                    engAlliedReqMaster, existingEngAlliedServices, hospitalDetailsId)
            };

            compositeVM.AdmAncRequirementPostvm = new AdmAncRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildDentalRequirements<AdmAncServicesItemVM>(
                    admAncReqMaster, existingAdmAncServices, hospitalDetailsId)
            };

            compositeVM.DentalWardBedDistribution = mstDentalAffHospitalWardBedDistribution
                .Select(mst =>
                {
                    var existing = existingDentalAffHospitalWardBedDistribution
                        .FirstOrDefault(x => x.WardId == mst.Id);

                    return new DentalWardBedDistributionVm
                    {
                        WardId = mst.Id,
                        WardName = mst.WardName,
                        SeatSlab = mst.SeatSlab,
                        BedsRequired = mst.BedRequirement,
                        BedsPresent = existing?.BedsPresent ?? 0,
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        HospitalDetailsId = hospitalDetailsId
                    };
                })
                .ToList();

            // ---------------- Indoor beds + shared requirement sections ----------------
            compositeVM.IndoorBedsOccupancy = new IndoorBedsOccupancyPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Items = items
            };

            compositeVM.IndoorBedsUnitsRequirements = new IndoorBedsUnitsRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<IndoorBedsUnitsItemVM>(
                    indoorBedsUnitsReqMaster, existingIndoorBedsUnitsRequirement)
            };

            compositeVM.OTRequirements = new OTRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<OTRequirementItemVM>(
                    otReqMaster, existingOTRequirement)
            };

            compositeVM.CasualityRequirements = new CasualityRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<CasualityRequirementItemVM>(
                    casualityReqMaster, existingCasualityRequirement)
            };

            compositeVM.CSSDandLaundryRequirements = new CSSDandLaundryRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<CSSDandLaundryItemVM>(
                    cssdAndLaundryReqMaster, existingCSSDandLaundryRequirement)
            };

            compositeVM.RadioDiagnosisRequirements = new RadioDiagnosisRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<RadioDiagnosisItemVM>(
                    radioDiagnosisReqMaster, existingRadioDiagnosisRequirement)
            };

            compositeVM.AnaesthesiologyRequirements = new AnaesthesiologyRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<AnaesthesiologyItemVM>(
                    anaesthesiologyReqMaster, existingAnaesthesiologyRequirement)
            };

            compositeVM.CentralLaboratoryRequirements = new CentralLaboratoryRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<CentralLaboratoryItemVM>(
                    centralLaboratoryReqMaster, existingCentralLaboratoryRequirement)
            };

            compositeVM.BloodBankRequirements = new BloodBankRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<BloodBankItemVM>(
                    bloodBankReqMaster, existingBloodBankRequirement)
            };

            compositeVM.OutPatientRequirements = new OutPatientRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<OutPatientAreaItemVM>(
                    outpatientAreaReqMaster, existingOutpatientAreaRequirement)
            };

            compositeVM.YogaRequirements = new YogaRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<YogaItemVM>(
                    yogaReqMaster, existingYogaRequirement)
            };

            compositeVM.RadiationOncologyRequirements = new RadiationOncologyRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<RadiationOncologyItemVM>(
                    radiationOncologyReqMaster, existingRadiationOncologyRequirement)
            };

            compositeVM.ArtCenterRequirements = new ArtCenterRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<ArtCenterItemVM>(
                    artCenterReqMaster, existingArtCenterRequirement)
            };

            compositeVM.PharmacyRequirements = new PharmacyRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<PharmacyItemVM>(
                    pharmacyReqMaster, existingPharmacyRequirement)
            };

            compositeVM.UtilitiesRequirements = new UtilitiesRequirementsPostVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                AffiliationTypeId = typeOfAffiliation,
                HospitalDetailsId = hospitalDetailsId,
                Requirements = BuildRequirements<UtilitiesItemVM>(
                    utilitiesReqMaster, existingUtilitiesRequirement)
            };

            // ---------------- Sections shown in the UI ----------------
            compositeVM.Sections.Add(new SectionViewModel
            {
                Name = "ClinicalSection",
                SubSections = new List<string>
                {
                    "InstructionSet",
                    "HospitalDetails",
                    "ClinicalCapacity",
                    "Discipline",
                    "NPTAservices",
                    "EngAlliedservices",
                    "WardWiseBedDistribution",
                    "Facilities"
                }
            });

            return compositeVM;
        }
    }
}