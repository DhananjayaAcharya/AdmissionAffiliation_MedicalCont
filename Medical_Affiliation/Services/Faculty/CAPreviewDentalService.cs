using System.Data;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAPreviewDentalService : ICADentalPreviewService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICAInstitutionBasicDetails _basicDetailsService;
        private readonly ICATrustDetailsService _cATrustDetailsService;

        private readonly ITeachingFacultyDetailsService _teachingFacultyDetailsPreviewService;
        private readonly IUGPgIntakeDetailsService _ugPgIntakeDetailsService;
        private readonly ICATrustMemberDetailsPreviewService _trustMembersDetailsService;
        private readonly ICADentalChairDistributionPreviewService _chairPreviewService;
        private readonly IInstitutionPreviewService _institutionPreviewService;
        private readonly IHumanResourcesPreviewService _humanResourcesPreviewService;
        private readonly ICAAcademicService _academicService;
        private readonly ICADentalHospitalAffiliationService _hospitalService;
        private readonly ICALandClassEquipmentService _landClassEqService;
        private readonly ICADentalLandBuildingPreviewService _cADentalLandBuildingPreviewService;

        private readonly ICADentalBedDistributionService _cADentalBedDistributionService;

        private readonly ICAAcademicIntakeService _academicIntakeService;
        private readonly IUserContext _userContext;
        private readonly ICAHostelPreviewService _hostelPreviewService;

        private readonly IWorkShopDetailsService _workshopService;
        private readonly IAnimalHouseService _animalHouseService;

        private readonly ICADentalLibraryService _dentalLibraryService;


        private readonly ICADentalPaymentService _caDentalpaymentService;
        private readonly ICAVehiclePreviewService _vehiclePreviewService;
        private readonly ICADeclarationService _cADeclarationService;
        private readonly ICADepartmentOfficesMeuService _cADepartmentOfficesMeuService;
        private readonly ICAEquipmentPreviewService _cAEquipmentPreviewService;
        private readonly ICADentalFieldPracticeAreaService _caDentalFieldPracticeAreaService;
        private readonly ICADentalStaffDetailsPreviewService _cADentalStaffDetailsPreviewService;
        private readonly ICAFinanceService _cAFinanceService;
        private readonly IActionTakenDeficiencyReportService _actionTakenDeficiencyReportService;
        private readonly ApplicationDbContext _context;


        public CAPreviewDentalService(
            ICAAcademicService academicService,
            IInstitutionPreviewService institutionPreviewService,
            ICAInstitutionBasicDetails basicDetailsService,
            ICADentalHospitalAffiliationService hospitalService,
            ICAAcademicIntakeService academicIntakeService,
            ICADentalLandBuildingPreviewService cADentalLandBuildingPreviewService,
            ICADentalChairDistributionPreviewService chairPreviewService,
            ICALandClassEquipmentService landClassEqService,
            ICAHostelPreviewService hostelPreviewService,
            ICADentalPaymentService dentalPaymentService,
            ITeachingFacultyDetailsService teachingFacultyDetailsPreviewService,
            ICADeclarationService declarationService,
            ICAVehiclePreviewService vehiclePreviewService,
            IHumanResourcesPreviewService humanResourcesPreviewService,
            ICATrustDetailsService cATrustDetailsService,
            IWorkShopDetailsService workShopDetailsService,
            ICADentalLibraryService dentalLibraryService,
            IAnimalHouseService animalHouseService,
            IUGPgIntakeDetailsService ugPgIntakeDetailsService,
            ICATrustMemberDetailsPreviewService cATrustMemberDetailsPreviewService,
            IUserContext userContext,
            ICADentalBedDistributionService cADentalBedDistributionService,
            ICADepartmentOfficesMeuService cADepartmentOfficesMeuService,
            ICAEquipmentPreviewService cAEquipmentPreviewService,
            ICADentalFieldPracticeAreaService cADentalFieldPracticeAreaService,
            ICAFinanceService cAFinanceService,
            ICADentalStaffDetailsPreviewService cADentalStaffDetailsPreviewService,
            IActionTakenDeficiencyReportService actionTakenDeficiencyReportService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext dbContext)
        {
            _basicDetailsService = basicDetailsService;
            _academicIntakeService = academicIntakeService;
            _trustMembersDetailsService = cATrustMemberDetailsPreviewService;
            _cATrustDetailsService = cATrustDetailsService;
            _cADentalLandBuildingPreviewService = cADentalLandBuildingPreviewService;
            _academicService = academicService;
            _hospitalService = hospitalService;
            _hostelPreviewService = hostelPreviewService;
            _vehiclePreviewService = vehiclePreviewService;
            _institutionPreviewService = institutionPreviewService;
            _landClassEqService = landClassEqService;
            _chairPreviewService = chairPreviewService;
            _teachingFacultyDetailsPreviewService = teachingFacultyDetailsPreviewService;
            _userContext = userContext;
            _ugPgIntakeDetailsService = ugPgIntakeDetailsService;
            _humanResourcesPreviewService = humanResourcesPreviewService;
            _cADeclarationService = declarationService;
            _caDentalpaymentService = dentalPaymentService;
            _workshopService = workShopDetailsService;
            _dentalLibraryService = dentalLibraryService;
            _animalHouseService = animalHouseService;
            _cADentalBedDistributionService = cADentalBedDistributionService;
            _cADepartmentOfficesMeuService = cADepartmentOfficesMeuService;
            _cAEquipmentPreviewService = cAEquipmentPreviewService;
            _caDentalFieldPracticeAreaService = cADentalFieldPracticeAreaService;
            _cADentalStaffDetailsPreviewService = cADentalStaffDetailsPreviewService;
            _cAFinanceService = cAFinanceService;
            _actionTakenDeficiencyReportService = actionTakenDeficiencyReportService;
            _context = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CADentalpreviewViewModel> GetDentalPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var collegeName = await _context.AffiliationCollegeMasters.Where(e => e.CollegeCode == collegeCode).Select(e => e.CollegeName).FirstOrDefaultAsync();

            var facultyCode = _userContext.FacultyId;
            var facultyName = await _context.Faculties.Where(e => e.FacultyId == facultyCode).Select(e => e.FacultyName).FirstOrDefaultAsync();

            var affTypeId = _httpContextAccessor.HttpContext.Session.GetString("TypeOfAffiliationId");
            var courseLevel = _httpContextAccessor.HttpContext.Session.GetString("CourseLevel");

            var vm = new CADentalpreviewViewModel
            {
                CollegeCode = _userContext.CollegeCode,
                FacultyCode = _userContext.FacultyId.ToString(),
                CollegeName = collegeName,
                FacultyName = facultyName,
                InstitutionBasicVM = await _basicDetailsService.GetAllDentalDetails(),
                AcademicIntakeVM = await _academicIntakeService.GetAcademicIntakePreviewAsync(),
                TrustDetailsVM = await _cATrustDetailsService.GetTrustDetailsAsync(),
                TeachingFacultyDetailsVM = await _teachingFacultyDetailsPreviewService.GetTeachingFacultyDetailsAsync(),
                TrustMemberDetailsVM = await _trustMembersDetailsService.GetTrustMemberDetailsAsync(),
                DentalChairDistribution = await _chairPreviewService.GetDentalChairDistributionPreviewAsync(),
                DentalLandBuildingPreview = await _cADentalLandBuildingPreviewService.GetDentalLandBuildingPreviewAsync(),
                UGPgIntakeDetailsVM = await _ugPgIntakeDetailsService.GetUgCourseDetailsAsync(),
                HostelPreviewVM = await _hostelPreviewService.GetHostelPreviewAsync(),
                InstitutionPreviewVM = await _institutionPreviewService.GetInstitutionPreviewAsync(),
                VehiclePreviewVM = await _vehiclePreviewService.GetVehiclePreviewAsync(),
                CAacademicMattersVM = await _academicService.GetAcademicMattersAsync(),
                CAHospitalAFfiliationCompVM = await _hospitalService.GetHospitalAffiliationAsync(),
                PhysicalFacilities = await _landClassEqService.GetLandClassEquipmentService(),
                DentalPaymentVM = await _caDentalpaymentService.GetDentalPaymentDetails(),
                DeclarationVM = await _cADeclarationService.GetDeclarationDetails(),
                HumanResourcesVM = await _humanResourcesPreviewService.GetHumanResourcesPreviewAsync(),
                WorkshopDetails = await _workshopService.GetWorkshopDetailsAsync(),
                AnimalHouseDetails = await _animalHouseService.GetAnimalHouseDetails(),
                DentalLibraryDisplay = await _dentalLibraryService.GetLibraryAsync(),
                DentalBedDistributionVM = await _cADentalBedDistributionService.GetDentalBedDistributionAsync(),
                DepartmentOfficesMeuVM = await _cADepartmentOfficesMeuService.GetDepartmentOfficesMeuAsync(),
                EquipmentPreviewVM = await _cAEquipmentPreviewService.GetEquipmentPreviewAsync(),

                FiedPracticeArea = await _caDentalFieldPracticeAreaService.GetFieldPracticeAreaAsync(),
                DentalStaffDetailsVM = await _cADentalStaffDetailsPreviewService.GetDentalStaffDetailsPreviewAsync(),
                FinanceVm = await _cAFinanceService.GetFinanceDetails(),
                ActionTakenDeficiencyReports = await _actionTakenDeficiencyReportService.GetPreviewAsync()
            };

            var institution = await _context.AffInstitutionsDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString());

            string academicYear = "2026-27";

            int affiliationTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetInt32("AffiliationType"));

            var finalSubmissionStatus = await _context.FinalDentalSubmissions
                .FirstOrDefaultAsync(e =>
                    e.CollegeCode == collegeCode &&
                    e.FacultyCode == 2 &&
                    e.AffiliationTypeId == affiliationTypeId &&
                    e.CourseLevel == courseLevel &&
                    e.AcademicYear == academicYear);

            string? typeOfAffiliation = _httpContextAccessor.HttpContext?.Session.GetString("TypeOfAffiliation");


            vm.FinalSubmissionVM = new DentalFinalSubmissionViewModel
            {
                CollegeCode = collegeCode,
                CollegeName = institution?.NameOfInstitution ?? string.Empty,
                PrincipalName = institution?.PrincipalName ?? string.Empty,
                PrincipalConsent = finalSubmissionStatus?.PrincipalConsent ?? false,
                ApplicationNumber = finalSubmissionStatus?.ApplicationNumber ?? string.Empty,
                AffiliationTypeDescription = typeOfAffiliation,
                FacultyCode = 2,
                AffiliationTypeId = Convert.ToInt32(affTypeId),
                CourseLevel = courseLevel,
                AcademicYear = academicYear,
                IsSubmitted = finalSubmissionStatus != null,
                IsActive = finalSubmissionStatus?.IsActive ?? true,
                CreatedDate = finalSubmissionStatus?.CreatedDate ?? DateTime.MinValue,
                ModifiedDate = finalSubmissionStatus?.ModifiedDate,
            };

            return vm;
        }



        public async Task<string> FinalSubmitAsync(DentalFinalSubmissionViewModel model)
        {
            // 1. Get authenticated college and faculty from session.

            var httpContext = _httpContextAccessor.HttpContext;
            string? collegeCode = httpContext?.Session.GetString("CollegeCode");

            string? facultyCode =  httpContext?.Session.GetString("FacultyCode");

            if (string.IsNullOrWhiteSpace(collegeCode) || facultyCode != "2")
            {
                throw new InvalidOperationException(
                    "Invalid college session. Please log in again.");
            }

            // 2. Validate principal consent.
            if (!model.PrincipalConsent)
            {
                throw new InvalidOperationException( "Principal consent is required.");
            }

            // 3. Validate the application context.
            // These values must be checked against your server-side
            // affiliation/application context before accepting submission.
            int affiliationTypeId = model.AffiliationTypeId;
            string courseLevel = model.CourseLevel?.Trim() ?? "";
            string academicYear = model.AcademicYear?.Trim() ?? "";

            if (affiliationTypeId <= 0 || string.IsNullOrWhiteSpace(courseLevel) || string.IsNullOrWhiteSpace(academicYear))
            {
                throw new InvalidOperationException(
                    "Invalid affiliation application details.");
            }

            if (academicYear.Length != 6 || !academicYear.All(char.IsDigit))
            {
                throw new InvalidOperationException("Invalid academic year.");
            }


            DateTime currentDate = DateTime.Now;

            // 4. Start a serializable transaction to protect serial generation.
            await using var transaction = await _context.Database.BeginTransactionAsync( IsolationLevel.Serializable);

            try
            {
                // 5. Verify the institution and principal details.
                var institution = await _context.AffInstitutionsDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode);

                if (institution == null ||
                    string.IsNullOrWhiteSpace(institution.PrincipalName))
                {
                    throw new InvalidOperationException(
                        "Principal details are missing. " +
                        "Please update institution details before submission.");
                }

                var submission = await _context.FinalDentalSubmissions
                   .FirstOrDefaultAsync(x =>
                       x.CollegeCode == collegeCode &&
                       x.FacultyCode == 2 &&
                       x.AffiliationTypeId == affiliationTypeId &&
                       x.CourseLevel == courseLevel &&
                       x.AcademicYear == academicYear);

                if (submission != null && submission.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Final submission has already been completed. " +
                        $"Application Number: {submission.ApplicationNumber}. " +
                        "The application is locked. Please contact the authorized " +
                        "administrator if further changes are required.");
                }

                bool isNewSubmission = submission == null;

                if (isNewSubmission)
                {
                    // 7. Generate an application number only for new records.
                    string applicationNumber =
                        await GenerateDentalApplicationNumberAsync(
                            2,
                            affiliationTypeId,
                            academicYear);

                    submission = new FinalDentalSubmission
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = 2,
                        CourseLevel = courseLevel,
                        AffiliationTypeId = affiliationTypeId,
                        AcademicYear = academicYear,

                        ApplicationNumber = applicationNumber,

                        PrincipalConsent = true,
                        IsActive = true,

                        CreatedDate = currentDate,
                        CreatedBy = institution.PrincipalName,

                        SubmittedDate = currentDate
                    };

                    _context.FinalDentalSubmissions.Add(submission);
                }
                else
                {
                    // 8. Update the existing submission.
                    // Preserve its ID, application number and creation audit fields.

                    submission!.PrincipalConsent = true;
                    submission.IsActive = true;

                    submission.ModifiedDate = currentDate;
                    submission.ModifiedBy = institution.PrincipalName;

                    // Refresh the submission timestamp on successful resubmission.
                    //submission.SubmittedDate = currentDate;
                }

                // 9. Save all changes.
                await _context.SaveChangesAsync();

                // 9. Commit only after the record is saved successfully.
                await transaction.CommitAsync();

                return submission!.ApplicationNumber;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        private async Task<string> GenerateDentalApplicationNumberAsync(
            int facultyCode,
            int affiliationTypeId,
            string academicYear)
        {
            // Example prefix: 2026270202
            string prefix =
                $"{academicYear}{facultyCode:D2}{affiliationTypeId:D2}";

            // Must execute inside the Serializable transaction
            // opened by FinalSubmitAsync.
            var lastApplicationNumber =
                await _context.FinalDentalSubmissions
                    .Where(x => x.ApplicationNumber.StartsWith(prefix))
                    .OrderByDescending(x => x.ApplicationNumber)
                    .Select(x => x.ApplicationNumber)
                    .FirstOrDefaultAsync();

            int nextSerial = 1;

            if (!string.IsNullOrWhiteSpace(lastApplicationNumber))
            {
                string serialPart =
                    lastApplicationNumber.Substring(prefix.Length);

                if (serialPart.Length != 4 ||
                    !int.TryParse(serialPart, out int lastSerial))
                {
                    throw new InvalidOperationException(
                        "The existing application number has an invalid format.");
                }

                nextSerial = checked(lastSerial + 1);
            }

            if (nextSerial > 9999)
            {
                throw new InvalidOperationException(
                    "The application number serial limit has been reached.");
            }

            return $"{prefix}{nextSerial:D4}";
        }



    }

}