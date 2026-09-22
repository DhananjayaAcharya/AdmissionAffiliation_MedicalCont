using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using GeoPhotoModule.Services;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAPreviewService : ICAPreviewService
    {
        private readonly ICAAcademicService _academicService;
        private readonly ICAHospitalAffiliationService _hospitalService;
        private readonly ICALandClassEquipmentService _landClassEqService;
        private readonly ICAInstitutionBasicDetails _institutionBasicDetailsService;
        private readonly ICALibraryService _libraryService;
        private readonly ICAVehicleService _vehicleService;
        private readonly ICAFinanceService _financeService;
        private readonly ICAAdminTeachAndHostel _adminTeachAndHostelService;
        private readonly ICAFacultyDesigNonTeaching _facultyDesigNonTeachingService;
        private readonly IUserContext _userContext;
        private readonly ICAPaymentService _capaymentService;
        private readonly ICADeclarationService _cADeclarationService;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeoPhotoRepository _geoPhotoRepository;


        public CAPreviewService(
            ICAAcademicService academicService,
            ICAHospitalAffiliationService hospitalService,
            ICALandClassEquipmentService landClassEqService,
            ICAInstitutionBasicDetails institutionBasicDetailsService,
            ICALibraryService libraryService,
            ICAVehicleService vehicleService,
            ICAFinanceService financeService,
            ICAAdminTeachAndHostel adminTeachAndHostelService,
            ICAFacultyDesigNonTeaching facultyDesigNonTeachingService,
            ICAPaymentService paymentService,
            ICADeclarationService declarationService,
            IUserContext userContext,
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IGeoPhotoRepository geoPhotoRepository)
        {
            _academicService = academicService;
            _hospitalService = hospitalService;
            _landClassEqService = landClassEqService;
            _institutionBasicDetailsService = institutionBasicDetailsService;
            _libraryService = libraryService;
            _vehicleService = vehicleService;
            _financeService = financeService;
            _adminTeachAndHostelService = adminTeachAndHostelService;
            _facultyDesigNonTeachingService = facultyDesigNonTeachingService;
            _userContext = userContext;
            _cADeclarationService = declarationService;
            _capaymentService = paymentService;
            _context = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _geoPhotoRepository = geoPhotoRepository;
        }

        public async Task<CApreviewViewModel> GetPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var collegeName = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).Select(e => e.CollegeName).FirstOrDefaultAsync();

            var facultyCode = _userContext.FacultyId;
            var courseLevel = (_httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                               ?? _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel")
                               ?? _userContext.CourseLevel)
                ?.Trim()
                .ToUpperInvariant();
            var facultyName = await _context.Faculties.Where(e => e.FacultyId == facultyCode).Select(e => e.FacultyName).FirstOrDefaultAsync();
            var applicationType = _httpContextAccessor.HttpContext?.Session.GetString("TypeOfAffiliation");
            if (string.IsNullOrWhiteSpace(applicationType))
            {
                applicationType = await _context.TypeOfAffiliations
                    .Where(e => e.TypeId == _userContext.TypeOfAffiliation)
                    .Select(e => e.TypeDescription)
                    .FirstOrDefaultAsync();
            }
            var institutionEntity = await _context.AffInstitutionsDetails
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
                .OrderByDescending(e => e.InstitutionId)
                .FirstOrDefaultAsync();
            var affiliatedInstituteEntity = await _context.InstitutionBasicDetails
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
                .OrderByDescending(e => e.InstitutionId)
                .FirstOrDefaultAsync();
            var institutionTypeNames = await _context.MstInstitutionTypes
                .AsNoTracking()
                .GroupBy(e => e.InstitutionTypeId)
                .Select(group => new
                {
                    Id = group.Key,
                    Name = group.Select(e => e.InstitutionType)
                        .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty
                })
                .ToDictionaryAsync(e => e.Id.ToString(), e => e.Name);
            var districtNames = await _context.DistrictMasters
                .AsNoTracking()
                .ToDictionaryAsync(e => e.DistrictId, e => e.DistrictName);
            var talukNames = await _context.TalukMasters
                .AsNoTracking()
                .ToDictionaryAsync(e => e.TalukId, e => e.TalukName);
            var institutionStatusNames = await _context.AffInstitutionStatusMasters
                .AsNoTracking()
                .ToDictionaryAsync(e => e.InstitutionStatusId.ToString(), e => e.StatusName);

            static string ResolveName(string? value, IReadOnlyDictionary<string, string> names)
            {
                return !string.IsNullOrWhiteSpace(value) && names.TryGetValue(value.Trim(), out var name)
                    ? name
                    : value ?? string.Empty;
            }

            var trustInstitutionType = ResolveName(
                string.IsNullOrWhiteSpace(affiliatedInstituteEntity?.TypeOfInstitution)
                    ? institutionEntity?.TypeOfInstitution
                    : affiliatedInstituteEntity.TypeOfInstitution,
                institutionTypeNames);

            MedicalVm? affInstituteDetails = null;

            if (affiliatedInstituteEntity != null)
            {
                affInstituteDetails = new MedicalVm
                {
                    InstitutionId = affiliatedInstituteEntity.InstitutionId,
                    FacultyCode = affiliatedInstituteEntity.FacultyCode,
                    CollegeCode = affiliatedInstituteEntity.CollegeCode,
                    TypeOfInstitution = trustInstitutionType,
                    NameOfInstitution = affiliatedInstituteEntity.NameOfInstitution ?? string.Empty,
                    Address = affiliatedInstituteEntity.AddressOfInstitution ?? string.Empty,
                    PinCode = affiliatedInstituteEntity.PinCode ?? string.Empty,
                    MobileNumber = affiliatedInstituteEntity.MobileNumber ?? string.Empty,
                    StdCode = affiliatedInstituteEntity.StdCode ?? string.Empty,
                    Fax = affiliatedInstituteEntity.Fax ?? string.Empty,
                    Website = affiliatedInstituteEntity.Website ?? string.Empty,
                    EmailId = affiliatedInstituteEntity.EmailId ?? string.Empty,
                    AltLandlineOrMobile = affiliatedInstituteEntity.AltLandlineOrMobile ?? string.Empty,
                    AltEmailId = affiliatedInstituteEntity.AltEmailId ?? string.Empty,
                    AcademicYearStarted = affiliatedInstituteEntity.AcademicYearStarted ?? string.Empty,
                    IsRuralInstitution = affiliatedInstituteEntity.IsRuralInstitution ?? false,
                    IsMinorityInstitution = affiliatedInstituteEntity.IsMinorityInstitution ?? false,
                    TrustName = affiliatedInstituteEntity.TrustName ?? string.Empty,
                    PresidentName = affiliatedInstituteEntity.PresidentName ?? string.Empty,
                    PANNumber = affiliatedInstituteEntity.Pannumber ?? string.Empty,
                    RegistrationNumber = affiliatedInstituteEntity.RegistrationNumber ?? string.Empty,
                    RegistrationDate = affiliatedInstituteEntity.RegistrationDate,
                    Amendments = affiliatedInstituteEntity.Amendments ?? false,
                    GOKObtainedTrustName = affiliatedInstituteEntity.GokobtainedTrustName ?? string.Empty,
                    ChangesInTrustName = affiliatedInstituteEntity.ChangesInTrustName,
                    OtherNursingCollegeInCity = affiliatedInstituteEntity.OtherNursingCollegeInCity,
                    OtherDentalCollegeInCity = affiliatedInstituteEntity.OtherDentalCollegeInCity,
                    CategoryOfOrganisation = affiliatedInstituteEntity.CategoryOfOrganisation ?? string.Empty,
                    ContactPersonName = affiliatedInstituteEntity.ContactPersonName ?? string.Empty,
                    ContactPersonRelation = affiliatedInstituteEntity.ContactPersonRelation ?? string.Empty,
                    ContactPersonMobile = affiliatedInstituteEntity.ContactPersonMobile ?? string.Empty,
                    OtherPhysiotherapyCollegeInCity = affiliatedInstituteEntity.OtherPhysiotherapyCollegeInCity,
                    CoursesAppliedText = affiliatedInstituteEntity.CoursesAppliedText ?? string.Empty,
                    HeadOfInstitutionName = affiliatedInstituteEntity.HeadOfInstitutionName ?? string.Empty,
                    HeadOfInstitutionAddress = affiliatedInstituteEntity.HeadOfInstitutionAddress ?? string.Empty,
                    FinancingAuthorityName = affiliatedInstituteEntity.FinancingAuthorityName ?? string.Empty,
                    CollegeStatus = affiliatedInstituteEntity.CollegeStatus ?? string.Empty,
                    GovAutonomousCertNumber = affiliatedInstituteEntity.GovAutonomousCertNumber,
                    hasGovAutoCertFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.GovAutonomousCertFilePath),
                    hasGovCouncilMembershipFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.GovCouncilMembershipFilePath),
                    hasGokOrderExistingCoursesFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.GokOrderExistingCoursesFilePath),
                    hasFirstAffiliationNotifFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.FirstAffiliationNotifFilePath),
                    hasContinuationAffiliationFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.ContinuationAffiliationFilePath),
                    hasAmendedDoc = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.AmendedDocPath),
                    hasPANFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.PanfilePath),
                    hasBankStatementFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.BankStatementFilePath),
                    hasRegistrationCertificateFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.RegistrationCertificateFilePath),
                    hasRegisteredTrustMemberDetails = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.RegisteredTrustMemberDetailsPath),
                    hasAuditStatementFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.AuditStatementFilePath),
                    hasDCIfile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.DcicertificateFilePath),
                    hasKSDCcertificateFile = !string.IsNullOrWhiteSpace(affiliatedInstituteEntity.KsdccertificateFilePath)
                };
            }
            else if (institutionEntity != null)
            {
                affInstituteDetails = new MedicalVm
                {
                    InstitutionId = institutionEntity.InstitutionId,
                    FacultyCode = institutionEntity.FacultyCode,
                    CollegeCode = institutionEntity.CollegeCode,
                    TypeOfInstitution = trustInstitutionType,
                    NameOfInstitution = institutionEntity.NameOfInstitution ?? string.Empty,
                    Address = institutionEntity.Address ?? string.Empty,
                    PinCode = institutionEntity.PinCode ?? string.Empty,
                    MobileNumber = institutionEntity.MobileNumber ?? string.Empty,
                    StdCode = institutionEntity.StdCode ?? string.Empty,
                    Fax = institutionEntity.Fax ?? string.Empty,
                    Website = institutionEntity.Website ?? string.Empty,
                    EmailId = institutionEntity.EmailId ?? string.Empty,
                    AltLandlineOrMobile = institutionEntity.AltLandlineMobile ?? string.Empty,
                    AltEmailId = institutionEntity.AltEmailId ?? string.Empty,
                    AcademicYearStarted = institutionEntity.YearOfEstablishment ?? string.Empty,
                    IsRuralInstitution = institutionEntity.RuralInstitute,
                    IsMinorityInstitution = institutionEntity.MinorityInstitute,
                    TrustName = institutionEntity.TrustName ?? string.Empty,
                    PresidentName = institutionEntity.TrustPresidentName ?? string.Empty,
                    GovAutonomousCertNumber = institutionEntity.GovAutonomousCertNumber,
                    hasGovAutoCertFile = !string.IsNullOrWhiteSpace(institutionEntity.GovAutonomousCertPath)
                };
            }

            return new CApreviewViewModel
            {
                CollegeCode = _userContext.CollegeCode,
                FacultyCode = _userContext.FacultyId.ToString(),
                CollegeName = collegeName,
                FacultyName = facultyName,
                ApplicationType = applicationType ?? "—",
                ApplyingCourseLevel = courseLevel,
                CompletionPercentage = await GetCompletionPercentageAsync(collegeCode, facultyCode, courseLevel, applicationType),
                AffInstituteDetails = affInstituteDetails,
                InstitutionDetails = institutionEntity == null ? null : new InstitutionViewModel
                {
                    InstitutionId = institutionEntity.InstitutionId,
                    CollegeCode = institutionEntity.CollegeCode,
                    FacultyCode = institutionEntity.FacultyCode,
                    InstitutionTypeId = int.TryParse(institutionEntity.TypeOfInstitution, out var institutionTypeId) ? institutionTypeId : null,
                    CourseLevel = institutionEntity.CourseLevel,
                    TypeOfInstitution = ResolveName(institutionEntity.TypeOfInstitution, institutionTypeNames),
                    NameOfInstitution = institutionEntity.NameOfInstitution,
                    Address = institutionEntity.Address,
                    VillageTownCity = institutionEntity.VillageTownCity,
                    Taluk = ResolveName(institutionEntity.Taluk, talukNames),
                    District = ResolveName(institutionEntity.District, districtNames),
                    PinCode = institutionEntity.PinCode,
                    MobileNumber = institutionEntity.MobileNumber,
                    StdCode = institutionEntity.StdCode,
                    Fax = institutionEntity.Fax,
                    Website = institutionEntity.Website,
                    SurveyNoPidNo = institutionEntity.SurveyNoPidNo,
                    MinorityInstitute = institutionEntity.MinorityInstitute,
                    AttachedToMedicalClg = institutionEntity.AttachedToMedicalClg,
                    RuralInstitute = institutionEntity.RuralInstitute,
                    YearOfEstablishment = institutionEntity.YearOfEstablishment,
                    EmailId = institutionEntity.EmailId,
                    AltLandlineMobile = institutionEntity.AltLandlineMobile,
                    AltEmailId = institutionEntity.AltEmailId,
                    HeadOfInstitution = institutionEntity.HeadOfInstitution,
                    HeadAddress = institutionEntity.HeadAddress,
                    FinancingAuthority = institutionEntity.FinancingAuthority,
                    StatusOfCollege = ResolveName(institutionEntity.StatusOfCollege, institutionStatusNames),
                    CourseApplied = institutionEntity.CourseApplied,
                    DocumentName = institutionEntity.DocumentName,
                    DocumentContentType = institutionEntity.DocumentContentType,
                    NodalOfficer_Name = institutionEntity.NodalOfficerName,
                    NodalOfficer_Mob_Number = institutionEntity.NodalOfficerMobNumber,
                    NodalOfficer_Email = institutionEntity.NodalOfficerEmail,
                    Principal_Name = institutionEntity.PrincipalName,
                    Principal_Mob_No = institutionEntity.PrincipalMobNo,
                    Principal_Email = institutionEntity.PrincipalEmail,
                    HeadOfInstitution_Mob_NO = institutionEntity.HeadOfInstitutionMobNo,
                    HeadOfInstitution_Email = institutionEntity.HeadOfInstitutionEmail,
                    College_URL = institutionEntity.CollegeUrl,
                    TrustName = institutionEntity.TrustName,
                    TrustAddress = institutionEntity.TrustAddress,
                    TrustEstablishmentDate = institutionEntity.TrustEstablishmentDate,
                    TrustPresidentName = institutionEntity.TrustPresidentName,
                    TrustPresidentContactNo = institutionEntity.TrustPresidentContactNo,
                    DeanName = institutionEntity.DeanName,
                    DeanMobileNumber = institutionEntity.DeanMobileNumber,
                    DeanEmailId = institutionEntity.DeanEmailId,
                    PrincipalMobileNumber = institutionEntity.PrincipalMobileNumber,
                    PrincipalEmailId = institutionEntity.PrincipalEmailId,
                    MinorityCategory = institutionEntity.MinorityCategory,
                    RunningCourse = institutionEntity.RunningCourse,
                    GovAutonomousCertNumber = institutionEntity.GovAutonomousCertNumber,
                    hasGovAutoCertFile = !string.IsNullOrWhiteSpace(institutionEntity.GovAutonomousCertPath)
                },
                InstitutionBasicVM = await _institutionBasicDetailsService.GetAllDetails(),
                CourseIntakeList = await GetCourseIntakeListAsync(collegeCode, facultyCode, courseLevel),
                BedDistribution = courseLevel == "UG"
                    ? await GetBedDistributionAsync(collegeCode, facultyCode, _userContext.TypeOfAffiliation, courseLevel)
                    : null,
                CAacademicMattersVM = await _academicService.GetAcademicMattersAsync(),
                CAHospitalAFfiliationCompVM = await _hospitalService.GetHospitalAffiliationAsync(),
                PhysicalFacilities = await _landClassEqService.GetLandClassEquipmentService(),
                LibraryDisplay = await _libraryService.GetLibraryAsync(),
                VehicleDetailsVM = await _vehicleService.GetVehicleDetailsAsync(),
                FinanceVm = await _financeService.GetFinanceDetails(),
                AdminTeachAndHostelVM = await _adminTeachAndHostelService.GetAdminTeachAndHostelDetails(),
                FacultyDesigNonTeachDisplayVM = await _facultyDesigNonTeachingService.GetFacultyDesigNonTeachingAsync(),
                PaymentVM = await _capaymentService.GetPaymentDetails(),
                GeoPhotos = await _geoPhotoRepository.GetPageAsync(
                    collegeCode,
                    facultyCode.ToString()),
                DeclarationVM = await _cADeclarationService.GetDeclarationDetails()

            };
        }

        private async Task<int> GetCompletionPercentageAsync(
            string collegeCode,
            int facultyCode,
            string? courseLevel,
            string? applicationType)
        {
            var levels = await (
                from intake in _context.AcademicIntakes
                join course in _context.MstCourses on intake.Courses equals course.CourseCode.ToString()
                where intake.CollegeCode == collegeCode && intake.FacultyCode == facultyCode.ToString()
                select course.CourseLevel
            ).Distinct().ToListAsync();

            levels = levels
                .Where(level => !string.IsNullOrWhiteSpace(level))
                .Select(level => level.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                levels = new List<string> { courseLevel.Trim().ToUpperInvariant() };
            }

            if (levels.Count == 0 && !string.IsNullOrWhiteSpace(courseLevel))
            {
                levels.Add(courseLevel.Trim().ToUpperInvariant());
            }

            var requiredSteps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Institution", "DeanDetails", "PrincipalDetails", "ClinicalFacilities",
                "DepartmentUnits", "Hostel", "Finance", "StaffDetails", "LibraryServices",
                "Research", "Library", "FacultyDetails", "NonTeachingStaff", "LandBuilding",
                "EquipmentDetails", "Vehicle", "SkillsLab", "IntakeDetails",
                "PaymentCalculation", "GeoPhoto"
            };

            if (facultyCode == 2)
            {
                requiredSteps.Remove("EquipmentDetails");
                requiredSteps.Remove("SkillsLab");
                requiredSteps.Add("DentalEquipmentDetails");
                requiredSteps.Add("DentalSkillsLab");
                requiredSteps.Add("DentalFacultyDetails");
                requiredSteps.Add("ChairDistribution");
            }

            var institutionTypeId = await _context.AffInstitutionsDetails
                .AsNoTracking()
                .Where(x => x.CollegeCode == collegeCode
                    && x.FacultyCode == facultyCode.ToString()
                    && (string.IsNullOrWhiteSpace(courseLevel) || x.CourseLevel == courseLevel))
                .OrderByDescending(x => x.InstitutionId)
                .Select(x => x.TypeOfInstitution)
                .FirstOrDefaultAsync();
            var organizationCategory = int.TryParse(institutionTypeId, out var parsedInstitutionTypeId)
                ? await _context.MstInstitutionTypes
                    .AsNoTracking()
                    .Where(x => x.InstitutionTypeId == parsedInstitutionTypeId)
                    .Select(x => x.OrganizationCategory)
                    .FirstOrDefaultAsync()
                : null;
            if (string.Equals(organizationCategory, "P", StringComparison.OrdinalIgnoreCase))
            {
                requiredSteps.Add("TrustDetails");
                requiredSteps.Add("TrustMemberDetails");
            }

            if (levels.Contains("UG", StringComparer.OrdinalIgnoreCase))
            {
                requiredSteps.Add("BedDistribution");
                requiredSteps.Add("AcademicMatters");
                requiredSteps.Add(facultyCode == 2 ? "BDSDetails" : "MBBSDetails");
            }

            if (levels.Contains("PG", StringComparer.OrdinalIgnoreCase))
            {
                requiredSteps.Add("PgCourses");
                requiredSteps.Add("PGAcademicMatters");

                if (!string.IsNullOrWhiteSpace(_httpContextAccessor.HttpContext?.Session.GetString("CourseCode")))
                {
                    requiredSteps.UnionWith(new[]
                    {
                        "CourseSubjectSelection", "CourseGeneralDetails", "CourseInfrastructureDetails",
                        "CourseSummaryDetails", "CourseAcademicActivities", "CourseServicesWorkload",
                        "CourseStaffDetails"
                    });
                }
            }

            if (levels.Contains("SS", StringComparer.OrdinalIgnoreCase))
            {
                requiredSteps.Add("SsCoursesApplied");
            }

            if (!string.IsNullOrWhiteSpace(applicationType)
                && applicationType.Contains("Additional", StringComparison.OrdinalIgnoreCase))
            {
                requiredSteps.Add("AdditionalCourses");
            }

            var completedSteps = await _context.CaProgresses
                .AsNoTracking()
                .Where(x => x.CollegeCode == collegeCode
                    && x.CourseLevel != null
                    && levels.Contains(x.CourseLevel.Trim().ToUpper())
                    && x.IsCompleted == true
                    && requiredSteps.Contains(x.StepKey))
                .Select(x => x.StepKey)
                .Distinct()
                .ToListAsync();

            // Faculty details are optional in continuous affiliation. Keep
            // the preview completion calculation consistent with the sidebar,
            // which treats this step as complete even when no record exists.
            completedSteps.Add("FacultyDetails");

            return requiredSteps.Count == 0
                ? 0
                : (int)Math.Round((double)completedSteps.Count / requiredSteps.Count * 100);
        }

        private async Task<List<PreviewCourseIntakeItemVM>> GetCourseIntakeListAsync(
            string collegeCode,
            int facultyCode,
            string? courseLevel)
        {
            var rows = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(x => x.CollCode == collegeCode &&
                            x.Facultycode == facultyCode)
                .OrderBy(x => x.Slno)
                .ToListAsync();

            var normalizedLevel = courseLevel?.Trim().ToUpperInvariant();
            var levelRows = rows
                .Where(x => string.Equals(x.UgPg?.Trim(), normalizedLevel, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (levelRows.Count > 0)
            {
                rows = levelRows;
            }

            var applicationType = _httpContextAccessor.HttpContext?.Session.GetString("TypeOfAffiliation") ?? string.Empty;
            var applyingCourseLevel = courseLevel
                ?? _httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                ?? _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel")
                ?? string.Empty;

            return rows.Select(x =>
            {
                var presentIntake = x.Intake2627 ?? 0;
                var additionalSeatsRequested = 0;

                if (!string.IsNullOrWhiteSpace(x.IncreasedIntake)
                    && int.TryParse(x.IncreasedIntake, out var parsedIncreasedIntake))
                {
                    additionalSeatsRequested = parsedIncreasedIntake;
                }

                return new PreviewCourseIntakeItemVM
                {
                    Slno = x.Slno,
                    CourseName = x.Course ?? string.Empty,
                    CourseLevel = x.UgPg ?? string.Empty,
                    Intake = x.Intake2627,
                    CourseCode = x.CourseCode,
                    ApplicationType = applicationType,
                    ApplyingCourseLevel = applyingCourseLevel,
                    AdditionalSeatsRequested = additionalSeatsRequested,
                    TotalSeats = presentIntake + additionalSeatsRequested,
                    //MatchNote = x.MatchNote ?? string.Empty
                };
            }).ToList();
        }

        private async Task<MedicalUGBedDistributionVm?> GetBedDistributionAsync(
            string collegeCode,
            int facultyCode,
            int affiliationTypeId,
            string? courseLevel)
        {
            var selectedCourseLevel = _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel");
            var requestedLevels = new[] { courseLevel, selectedCourseLevel }
                .Where(level => !string.IsNullOrWhiteSpace(level))
                .Select(level => level!.Trim().ToUpperInvariant())
                .Distinct()
                .ToHashSet();

            var savedRows = await _context.MedicalUgbedDistributions
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString() &&
                    x.AffiliationTypeId == affiliationTypeId)
                .ToListAsync();

            if (savedRows.Count == 0)
            {
                savedRows = await _context.MedicalUgbedDistributions
                    .AsNoTracking()
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode.ToString())
                    .ToListAsync();
            }

            var entity = savedRows
                .Where(x => requestedLevels.Contains((x.CourseLevel ?? string.Empty).Trim().ToUpperInvariant()))
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            entity ??= savedRows
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            if (entity == null)
            {
                return null;
            }

            return new MedicalUGBedDistributionVm
            {
                OralMaxillofacialSurgery = entity.OralMaxillofacialSurgery,
                GenMedicine = entity.GenMedicine,
                Paediatrics = entity.Paediatrics,
                SkinVD = entity.SkinVd,
                Psychiatry = entity.Psychiatry,
                GenSurgery = entity.GenSurgery,
                Orthopaedics = entity.Orthopaedics,
                Ophthalmology = entity.Ophthalmology,
                ENT = entity.Ent,
                ObstetricsANC = entity.ObstetricsAnc,
                Gynaecology = entity.Gynaecology,
                Postpartum = entity.Postpartum,
                MajorOT = entity.MajorOt,
                MinorOT = entity.MinorOt,
                ICCU = entity.Iccu,
                ICU = entity.Icu,
                PICU_NICU = entity.PicuNicu,
                SICU = entity.Sicu,
                TotalICUBeds = entity.TotalIcubeds,
                CasualtyBeds = entity.CasualtyBeds
            };
        }


    }

}
