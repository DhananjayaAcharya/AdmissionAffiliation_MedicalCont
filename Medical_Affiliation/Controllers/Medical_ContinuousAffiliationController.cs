using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class Medical_ContinuousAffiliationController : BaseController
    {
        private const string EquipmentAcademicYear = "2027-28";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<Medical_ContinuousAffiliationController> _logger;

        public Medical_ContinuousAffiliationController(
            ApplicationDbContext context,
            ILogger<Medical_ContinuousAffiliationController> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // =====================================================================
        // FILE HELPERS
        // =====================================================================
        private async Task<string?> SaveLandFileAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string rootPath = FacultyCode == "2" ? BaseDentalPath : BaseMedicalPath;
            string fullFolder = Path.Combine(rootPath, "LandBuilding", folder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(fullFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }

        private async Task<string?> SaveMeuFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string rootPath = FacultyCode == "2" ? BaseDentalPath : BaseMedicalPath;
            string basePath = Path.Combine(rootPath, "MEUFiles");

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }

        private void TryDeleteFile(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete file {Path}", path);
            }
        }

        // =====================================================================
        // LAND / BUILDING / TEACHING / LABS / MUSEUM / ADMIN
        // =====================================================================
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_LandBuildingdetails()
        {
            var courseLevel = CourseLevel;
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var teaching = await _context.SmallGroupTeachings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                          x.CollegeCode == collegeCode &&
                                          x.CourseLevel == courseLevel);

            var savedIntakeTeaching = teaching ?? await _context.SmallGroupTeachings
                .Where(x => x.FacultyCode == facultyCode &&
                            x.CollegeCode == collegeCode &&
                            x.AnnualMbbsIntake > 0)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            ViewBag.SavedAnnualMbbsIntake = savedIntakeTeaching?.AnnualMbbsIntake;

            var labs = await _context.MedicalStudentPracticalLabs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                          x.CollegeCode == collegeCode &&
                                          x.CourseLevel == courseLevel);

            var museum = await _context.MedicalMuseums.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                          x.CollegeCode == collegeCode &&
                                          x.CourseLevel == courseLevel);

            var admin = await _context.MedicalAdministrativePhysicalFacilities.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                          x.CollegeCode == collegeCode &&
                                          x.CourseLevel == courseLevel);

            // First time: nothing saved in any of the four tables
            if (teaching == null && labs == null && museum == null && admin == null)
            {
                return View(new SmallGroupTeachingViewModel
                {
                    AnnualMbbsIntake = 100,
                    SmallGroupBatchSize = 15
                });
            }

            var vm = new SmallGroupTeachingViewModel
            {
                // --- GENERAL ---
                AnnualMbbsIntake = teaching?.AnnualMbbsIntake
                    ?? savedIntakeTeaching?.AnnualMbbsIntake
                    ?? 100,
                SmallGroupBatchSize = teaching?.SmallGroupBatchSize ?? 15,

                TeachingAreasSharedAllDepts = teaching?.TeachingAreasSharedAllDepts,
                AvInAllTeachingAreas = teaching?.AvInAllTeachingAreas,
                InternetInAllTeachingAreas = teaching?.InternetInAllTeachingAreas,
                DigitalLinkAllTeachingAreas = teaching?.DigitalLinkAllTeachingAreas,

                // --- TEACHING ROOMS ---
                SmallGroupStudents = teaching?.SmallGroupStudents ?? 0,
                RequiredAreaSqm = teaching?.RequiredAreaSqm ?? 0,
                AvailableAreaSqm = teaching?.AvailableAreaSqm ?? 0,
                AreaDeficiencySqm = teaching?.AreaDeficiencySqm ?? 0,

                RoomsSharedByAllDepts = teaching?.RoomsSharedByAllDepts,
                AppropriateAreaEachSpecialty = teaching?.AppropriateAreaEachSpecialty,
                ConnectedToLectureHalls = teaching?.ConnectedToLectureHalls,
                InternetInTeachingRooms = teaching?.InternetInTeachingRooms,

                // --- LABS ---
                HistologyAvailable = labs?.HistologyAvailable ?? false,
                HistologyShared = labs?.HistologyShared ?? false,
                ClinicalPhysiologyAvailable = labs?.ClinicalPhysiologyAvailable ?? false,
                ClinicalPhysiologyShared = labs?.ClinicalPhysiologyShared ?? false,
                BiochemistryAvailable = labs?.BiochemistryAvailable ?? false,
                BiochemistryShared = labs?.BiochemistryShared ?? false,
                HistopathCytopathAvailable = labs?.HistopathCytopathAvailable ?? false,
                HistopathCytopathShared = labs?.HistopathCytopathShared ?? false,
                ClinPathHemeAvailable = labs?.ClinPathHemeAvailable ?? false,
                ClinPathHemeShared = labs?.ClinPathHemeShared ?? false,
                MicrobiologyAvailable = labs?.MicrobiologyAvailable ?? false,
                MicrobiologyShared = labs?.MicrobiologyShared ?? false,
                ClinicalPharmAvailable = labs?.ClinicalPharmAvailable ?? false,
                ClinicalPharmShared = labs?.ClinicalPharmShared ?? false,
                CalPharmAvailable = labs?.CalPharmAvailable ?? false,
                CalPharmShared = labs?.CalPharmShared ?? false,

                AllLabsHaveAV = labs?.AllLabsHaveAv ?? false,
                AllLabsHaveInternet = labs?.AllLabsHaveInternet ?? false,
                TechnicalStaffFacilitiesEnsured = labs?.TechnicalStaffFacilitiesEnsured ?? false,

                // --- MUSEUM ---
                SeparateAnatomyMuseumAvailable = museum?.SeparateAnatomyMuseumAvailable,
                PathologyForensicSharedMuseum = museum?.PathologyForensicSharedMuseum,
                PharmMicroCommSharedMuseum = museum?.PharmMicroCommSharedMuseum,

                SeatingCapacityPerMuseum = museum?.SeatingCapacityPerMuseum ?? 0,
                SeatingAreaAvailableSqm = museum?.SeatingAreaAvailableSqm ?? 0,
                SeatingAreaRequiredSqm = museum?.SeatingAreaRequiredSqm ?? 0,
                SeatingAreaDeficiencySqm = museum?.SeatingAreaDeficiencySqm ?? 0,

                MuseumsHaveAV = museum?.MuseumsHaveAv,
                MuseumsHaveInternet = museum?.MuseumsHaveInternet,
                MuseumsDigitallyLinked = museum?.MuseumsDigitallyLinked,
                MuseumsHaveRacksShelves = museum?.MuseumsHaveRacksShelves,
                MuseumsHaveRadiologyDisplay = museum?.MuseumsHaveRadiologyDisplay,
                TeachingTimeSharingProgrammed = museum?.TeachingTimeSharingProgrammed,

                // --- LAND ---
                IsMinimumLandAvailable = teaching?.IsMinimumLandAvailable,
                LandDetailsIfYes = teaching?.LandDetailsIfYes,
                HasPurchasePlanIfNo = teaching?.HasPurchasePlanIfNo,
                HasBudgetProvisionIfNo = teaching?.HasBudgetProvisionIfNo,
                HasFutureExpansionSpace = teaching?.HasFutureExpansionSpace,
                HasLandRecordsFile = !string.IsNullOrEmpty(teaching?.LandRecordsFilePath),
                HasApprovedBuildingPlanFile = !string.IsNullOrEmpty(teaching?.ApprovedBuildingPlanFilePath),

                // --- BUILDING ---
                IsBuildingAsPerCouncilNorms = teaching?.IsBuildingAsPerCouncilNorms,
                LandOwnershipType = teaching?.LandOwnershipType,
                BuildingOwnershipType = teaching?.BuildingOwnershipType,
                FloorAreaSqFt = teaching?.FloorAreaSqFt ?? 0,
                NumberOfBlocks = teaching?.NumberOfBlocks ?? 0,
                NumberOfFloors = teaching?.NumberOfFloors ?? 0,
                YearOfConstruction = teaching?.YearOfConstruction ?? 0,

                // --- ADMINISTRATIVE ---
                PrincipalChamberAreaSqFt = admin?.PrincipalChamberAreaSqFt,
                OfficeRoomAreaSqFt = admin?.OfficeRoomAreaSqFt,
                StaffRoomsAreaSqFt = admin?.StaffRoomsAreaSqFt,
                LectureHallsAreaSqFt = admin?.LectureHallsAreaSqFt,
                LaboratoriesAreaSqFt = admin?.LaboratoriesAreaSqFt,
                SeminarHallAreaSqFt = admin?.SeminarHallAreaSqFt,
                AuditoriumAreaSqFt = admin?.AuditoriumAreaSqFt,
                MuseumAreaSqFt = admin?.MuseumAreaSqFt,
                CommitteeRoomsAreaSqFt = admin?.CommitteeRoomsAreaSqFt,

                ExaminationHallAvailable = admin?.ExaminationHallAvailable ?? false,
                AnimalHouseAvailable = admin?.AnimalHouseAvailable ?? false,
                CommonRoomMenAvailable = admin?.CommonRoomMenAvailable ?? false,
                CommonRoomWomenAvailable = admin?.CommonRoomWomenAvailable ?? false,
                StudentHostelAvailable = admin?.StudentHostelAvailable ?? false,
                RegisteredUnderAnatomyAct = admin?.RegisteredUnderAnatomyAct ?? false,

                StaffQuartersPrincipal = admin?.StaffQuartersPrincipal ?? false,
                StaffQuartersOtherStaff = admin?.StaffQuartersOtherStaff ?? false,
                StaffQuartersTeachingAncillary = admin?.StaffQuartersTeachingAncillary ?? false,

                WorkshopStaffCount = admin?.WorkshopStaffCount,
                WorkshopEquipmentDetails = admin?.WorkshopEquipmentDetails,
                WorkshopScopeOfWork = admin?.WorkshopScopeOfWork,

                AnimalHouseAreaSqFt = admin?.AnimalHouseAreaSqFt,
                AnimalHouseStaffCount = admin?.AnimalHouseStaffCount,
                AnimalTypes = admin?.AnimalTypes
            };

            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_LandBuildingdetails(SmallGroupTeachingViewModel model)
        {
            var courseLevel = CourseLevel;
            var collegeCode = CollegeCode;
            var facultyCode = FacultyCode;

            if (string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["Error"] = "Session expired. Please select course again.";
                return RedirectToAction("Dashboard", "Collegelogin", new { collegecode = collegeCode });
            }

            // Log binding/validation problems (does not block the save)
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0))
                    foreach (var error in entry.Value!.Errors)
                        _logger.LogWarning("ModelState {Key}: {Error}", entry.Key,
                            string.IsNullOrEmpty(error.ErrorMessage) ? error.Exception?.Message : error.ErrorMessage);
            }

            // Server-side calculations (never trust the browser)
            model.RequiredAreaSqm = model.SmallGroupStudents * 1.2m;
            model.AreaDeficiencySqm = Math.Max(0, model.RequiredAreaSqm - model.AvailableAreaSqm);

            model.SeatingAreaRequiredSqm = Convert.ToDecimal(model.SeatingCapacityPerMuseum) * 1.2m;
            model.SeatingAreaDeficiencySqm = Math.Max(0m,
                model.SeatingAreaRequiredSqm - Convert.ToDecimal(model.SeatingAreaAvailableSqm));

            var filesToDelete = new List<string>();
            var newFilesSaved = new List<string>();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ========================= TEACHING =========================
                var teaching = await _context.SmallGroupTeachings
                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                              x.CollegeCode == collegeCode &&
                                              x.CourseLevel == courseLevel);

                if (teaching == null)
                {
                    teaching = new SmallGroupTeaching
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel
                    };
                    _context.SmallGroupTeachings.Add(teaching);
                }

                teaching.AnnualMbbsIntake = model.AnnualMbbsIntake;
                teaching.SmallGroupBatchSize = model.SmallGroupBatchSize;
                teaching.TeachingAreasSharedAllDepts = model.TeachingAreasSharedAllDepts ?? false;
                teaching.AvInAllTeachingAreas = model.AvInAllTeachingAreas ?? false;
                teaching.InternetInAllTeachingAreas = model.InternetInAllTeachingAreas ?? false;
                teaching.DigitalLinkAllTeachingAreas = model.DigitalLinkAllTeachingAreas ?? false;

                teaching.SmallGroupStudents = model.SmallGroupStudents;
                teaching.RequiredAreaSqm = model.RequiredAreaSqm;
                teaching.AvailableAreaSqm = model.AvailableAreaSqm;
                teaching.AreaDeficiencySqm = model.AreaDeficiencySqm;

                teaching.RoomsSharedByAllDepts = model.RoomsSharedByAllDepts ?? false;
                teaching.AppropriateAreaEachSpecialty = model.AppropriateAreaEachSpecialty ?? false;
                teaching.ConnectedToLectureHalls = model.ConnectedToLectureHalls ?? false;
                teaching.InternetInTeachingRooms = model.InternetInTeachingRooms ?? false;

                // LAND
                teaching.IsMinimumLandAvailable = model.IsMinimumLandAvailable ?? false;
                teaching.LandDetailsIfYes = model.LandDetailsIfYes;
                teaching.HasPurchasePlanIfNo = model.HasPurchasePlanIfNo ?? false;
                teaching.HasBudgetProvisionIfNo = model.HasBudgetProvisionIfNo ?? false;
                teaching.HasFutureExpansionSpace = model.HasFutureExpansionSpace ?? false;

                if (model.LandRecordsDocument is { Length: > 0 })
                {
                    var path = await SaveLandFileAsync(model.LandRecordsDocument, "LandRecords");
                    if (path != null)
                    {
                        newFilesSaved.Add(path);
                        if (!string.IsNullOrEmpty(teaching.LandRecordsFilePath))
                            filesToDelete.Add(teaching.LandRecordsFilePath);
                        teaching.LandRecordsFilePath = path;
                    }
                }

                // BUILDING
                teaching.IsBuildingAsPerCouncilNorms = model.IsBuildingAsPerCouncilNorms ?? false;
                teaching.LandOwnershipType = model.LandOwnershipType;
                teaching.BuildingOwnershipType = model.BuildingOwnershipType;
                teaching.FloorAreaSqFt = model.FloorAreaSqFt;
                teaching.NumberOfBlocks = model.NumberOfBlocks;
                teaching.NumberOfFloors = model.NumberOfFloors;
                teaching.YearOfConstruction = model.YearOfConstruction;

                if (model.ApprovedBuildingPlanDocument is { Length: > 0 })
                {
                    var path = await SaveLandFileAsync(model.ApprovedBuildingPlanDocument, "BuildingPlans");
                    if (path != null)
                    {
                        newFilesSaved.Add(path);
                        if (!string.IsNullOrEmpty(teaching.ApprovedBuildingPlanFilePath))
                            filesToDelete.Add(teaching.ApprovedBuildingPlanFilePath);
                        teaching.ApprovedBuildingPlanFilePath = path;
                    }
                }

                // ========================= LABS =========================
                var labs = await _context.MedicalStudentPracticalLabs
                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                              x.CollegeCode == collegeCode &&
                                              x.CourseLevel == courseLevel);

                if (labs == null)
                {
                    labs = new MedicalStudentPracticalLab
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel
                    };
                    _context.MedicalStudentPracticalLabs.Add(labs);
                }

                labs.HistologyAvailable = model.HistologyAvailable;
                labs.HistologyShared = model.HistologyShared;
                labs.ClinicalPhysiologyAvailable = model.ClinicalPhysiologyAvailable;
                labs.ClinicalPhysiologyShared = model.ClinicalPhysiologyShared;
                labs.BiochemistryAvailable = model.BiochemistryAvailable;
                labs.BiochemistryShared = model.BiochemistryShared;
                labs.HistopathCytopathAvailable = model.HistopathCytopathAvailable;
                labs.HistopathCytopathShared = model.HistopathCytopathShared;
                labs.ClinPathHemeAvailable = model.ClinPathHemeAvailable;
                labs.ClinPathHemeShared = model.ClinPathHemeShared;
                labs.MicrobiologyAvailable = model.MicrobiologyAvailable;
                labs.MicrobiologyShared = model.MicrobiologyShared;
                labs.ClinicalPharmAvailable = model.ClinicalPharmAvailable;
                labs.ClinicalPharmShared = model.ClinicalPharmShared;
                labs.CalPharmAvailable = model.CalPharmAvailable;
                labs.CalPharmShared = model.CalPharmShared;

                labs.AllLabsHaveAv = model.AllLabsHaveAV;
                labs.AllLabsHaveInternet = model.AllLabsHaveInternet;
                labs.TechnicalStaffFacilitiesEnsured = model.TechnicalStaffFacilitiesEnsured;

                // ========================= ADMIN =========================
                var admin = await _context.MedicalAdministrativePhysicalFacilities
                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                              x.CollegeCode == collegeCode &&
                                              x.CourseLevel == courseLevel);

                if (admin == null)
                {
                    admin = new MedicalAdministrativePhysicalFacility
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel,
                        CreatedDate = DateTime.Now
                    };
                    _context.MedicalAdministrativePhysicalFacilities.Add(admin);
                }
                else
                {
                    admin.UpdatedDate = DateTime.Now;
                }

                admin.PrincipalChamberAreaSqFt = model.PrincipalChamberAreaSqFt;
                admin.OfficeRoomAreaSqFt = model.OfficeRoomAreaSqFt;
                admin.StaffRoomsAreaSqFt = model.StaffRoomsAreaSqFt;
                admin.LectureHallsAreaSqFt = model.LectureHallsAreaSqFt;
                admin.LaboratoriesAreaSqFt = model.LaboratoriesAreaSqFt;
                admin.SeminarHallAreaSqFt = model.SeminarHallAreaSqFt;
                admin.AuditoriumAreaSqFt = model.AuditoriumAreaSqFt;
                admin.MuseumAreaSqFt = model.MuseumAreaSqFt;
                admin.CommitteeRoomsAreaSqFt = model.CommitteeRoomsAreaSqFt;

                admin.ExaminationHallAvailable = model.ExaminationHallAvailable;
                admin.AnimalHouseAvailable = model.AnimalHouseAvailable;
                admin.CommonRoomMenAvailable = model.CommonRoomMenAvailable;
                admin.CommonRoomWomenAvailable = model.CommonRoomWomenAvailable;
                admin.StudentHostelAvailable = model.StudentHostelAvailable;
                admin.RegisteredUnderAnatomyAct = model.RegisteredUnderAnatomyAct;

                admin.StaffQuartersPrincipal = model.StaffQuartersPrincipal;
                admin.StaffQuartersOtherStaff = model.StaffQuartersOtherStaff;
                admin.StaffQuartersTeachingAncillary = model.StaffQuartersTeachingAncillary;

                admin.WorkshopStaffCount = model.WorkshopStaffCount;
                admin.WorkshopEquipmentDetails = model.WorkshopEquipmentDetails;
                admin.WorkshopScopeOfWork = model.WorkshopScopeOfWork;

                admin.AnimalHouseAreaSqFt = model.AnimalHouseAreaSqFt;
                admin.AnimalHouseStaffCount = model.AnimalHouseStaffCount;
                admin.AnimalTypes = model.AnimalTypes;

                // ========================= MUSEUM =========================
                var museum = await _context.MedicalMuseums
                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                              x.CollegeCode == collegeCode &&
                                              x.CourseLevel == courseLevel);

                if (museum == null)
                {
                    museum = new MedicalMuseum
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel
                    };
                    _context.MedicalMuseums.Add(museum);
                }

                museum.SeparateAnatomyMuseumAvailable = model.SeparateAnatomyMuseumAvailable ?? false;
                museum.PathologyForensicSharedMuseum = model.PathologyForensicSharedMuseum ?? false;
                museum.PharmMicroCommSharedMuseum = model.PharmMicroCommSharedMuseum ?? false;

                museum.SeatingCapacityPerMuseum = model.SeatingCapacityPerMuseum;
                museum.SeatingAreaAvailableSqm = model.SeatingAreaAvailableSqm;
                museum.SeatingAreaRequiredSqm = model.SeatingAreaRequiredSqm;
                museum.SeatingAreaDeficiencySqm = model.SeatingAreaDeficiencySqm;

                museum.MuseumsHaveAv = model.MuseumsHaveAV ?? false;
                museum.MuseumsHaveInternet = model.MuseumsHaveInternet ?? false;
                museum.MuseumsDigitallyLinked = model.MuseumsDigitallyLinked ?? false;
                museum.MuseumsHaveRacksShelves = model.MuseumsHaveRacksShelves ?? false;
                museum.MuseumsHaveRadiologyDisplay = model.MuseumsHaveRadiologyDisplay ?? false;
                museum.TeachingTimeSharingProgrammed = model.TeachingTimeSharingProgrammed ?? false;

                // ========================= SAVE =========================
                var rows = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Medical_LandBuildingdetails saved {Rows} rows for {College}/{Faculty}/{Level}",
                    rows, collegeCode, facultyCode, courseLevel);

                // Delete replaced files only after a successful commit
                foreach (var old in filesToDelete)
                    TryDeleteFile(old);

                TempData["Success"] = "Land, building & teaching facility details saved successfully.";
                return RedirectToAction("Medical_SkillsLaboratory", "Medical_ContinuousAffiliation");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // DB row was not updated, so drop the files uploaded during this request
                foreach (var f in newFilesSaved)
                    TryDeleteFile(f);

                var root = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Error saving Medical_LandBuildingdetails: {Root}", root);

                // Remove the detail from the message once the cause is found
                TempData["Error"] = "Save failed: " + root;
                return View(model);
            }
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        public async Task<IActionResult> ViewLandRecords()
        {
            var teaching = await _context.SmallGroupTeachings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == FacultyCode &&
                                          x.CollegeCode == CollegeCode &&
                                          x.CourseLevel == CourseLevel);

            if (string.IsNullOrEmpty(teaching?.LandRecordsFilePath) ||
                !System.IO.File.Exists(teaching.LandRecordsFilePath))
                return NotFound();

            return PhysicalFile(teaching.LandRecordsFilePath, "application/pdf");
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        public async Task<IActionResult> ViewBuildingPlan()
        {
            var teaching = await _context.SmallGroupTeachings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == FacultyCode &&
                                          x.CollegeCode == CollegeCode &&
                                          x.CourseLevel == CourseLevel);

            if (string.IsNullOrEmpty(teaching?.ApprovedBuildingPlanFilePath) ||
                !System.IO.File.Exists(teaching.ApprovedBuildingPlanFilePath))
                return NotFound();

            return PhysicalFile(teaching.ApprovedBuildingPlanFilePath, "application/pdf");
        }

        // =====================================================================
        // SKILLS LABORATORY
        // =====================================================================
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_SkillsLaboratory()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("Login", "Account");

            var lab = await _context.MedicalSkillsLaboratories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                          x.CollegeCode == collegeCode);

            if (lab == null)
                return View(new SkillsLabViewModel());

            var vm = new SkillsLabViewModel
            {
                AnnualMbbsIntake = lab.AnnualMbbsIntake,

                TotalAreaAvailableSqm = lab.TotalAreaAvailableSqm,
                TotalAreaRequiredSqm = lab.TotalAreaRequiredSqm,
                TotalAreaDeficiencySqm = lab.TotalAreaDeficiencySqm,

                SixWeeksTrainingCompletedBeforeClinical = lab.SixWeeksTrainingCompletedBeforeClinical,

                NumberOfExaminationRooms = lab.NumberOfExaminationRooms,
                HasMinFourExamRooms = lab.HasMinFourExamRooms,
                HasDemoRoomSmallGroups = lab.HasDemoRoomSmallGroups,
                HasDebriefArea = lab.HasDebriefArea,
                HasFacultyCoordinatorRoom = lab.HasFacultyCoordinatorRoom,
                HasSupportStaffRoom = lab.HasSupportStaffRoom,
                HasStorageForMannequins = lab.HasStorageForMannequins,
                HasVideoRecordingFacility = lab.HasVideoRecordingFacility,

                NumberOfSkillStations = lab.NumberOfSkillStations,
                HasGroupAndIndividualStations = lab.HasGroupAndIndividualStations,
                HasRequiredTrainersAndMannequins = lab.HasRequiredTrainersAndMannequins,
                HasDedicatedTechnicalOfficer = lab.HasDedicatedTechnicalOfficer,
                HasAdequateSupportStaff = lab.HasAdequateSupportStaff,

                TeachingAreasHaveAV = lab.TeachingAreasHaveAv,
                TeachingAreasHaveInternet = lab.TeachingAreasHaveInternet,
                SkillsLabEnabledForELearning = lab.SkillsLabEnabledForElearning
            };

            return View(vm);
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_SkillsLaboratory(SkillsLabViewModel model)
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            // 600 Sq.m up to 150 intake, 800 Sq.m above
            var intake = Convert.ToInt32(model.AnnualMbbsIntake ?? 0);

            model.TotalAreaRequiredSqm = intake <= 150 ? 600m : 800m;
            model.TotalAreaDeficiencySqm =
                Math.Max(0, model.TotalAreaRequiredSqm - model.TotalAreaAvailableSqm);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lab = await _context.MedicalSkillsLaboratories
                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

                if (lab == null)
                {
                    lab = new MedicalSkillsLaboratory
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                    };
                    _context.MedicalSkillsLaboratories.Add(lab);
                }

                lab.AnnualMbbsIntake = model.AnnualMbbsIntake;
                lab.TotalAreaAvailableSqm = model.TotalAreaAvailableSqm;
                lab.TotalAreaRequiredSqm = model.TotalAreaRequiredSqm;
                lab.TotalAreaDeficiencySqm = model.TotalAreaDeficiencySqm;

                lab.SixWeeksTrainingCompletedBeforeClinical =
                    model.SixWeeksTrainingCompletedBeforeClinical ?? false;

                lab.NumberOfExaminationRooms = model.NumberOfExaminationRooms;
                lab.HasMinFourExamRooms = model.HasMinFourExamRooms ?? false;
                lab.HasDemoRoomSmallGroups = model.HasDemoRoomSmallGroups ?? false;
                lab.HasDebriefArea = model.HasDebriefArea ?? false;
                lab.HasFacultyCoordinatorRoom = model.HasFacultyCoordinatorRoom ?? false;
                lab.HasSupportStaffRoom = model.HasSupportStaffRoom ?? false;
                lab.HasStorageForMannequins = model.HasStorageForMannequins ?? false;
                lab.HasVideoRecordingFacility = model.HasVideoRecordingFacility ?? false;

                lab.NumberOfSkillStations = model.NumberOfSkillStations;
                lab.HasGroupAndIndividualStations = model.HasGroupAndIndividualStations ?? false;
                lab.HasRequiredTrainersAndMannequins = model.HasRequiredTrainersAndMannequins ?? false;
                lab.HasDedicatedTechnicalOfficer = model.HasDedicatedTechnicalOfficer ?? false;
                lab.HasAdequateSupportStaff = model.HasAdequateSupportStaff ?? false;

                lab.TeachingAreasHaveAv = model.TeachingAreasHaveAV ?? false;
                lab.TeachingAreasHaveInternet = model.TeachingAreasHaveInternet ?? false;
                lab.SkillsLabEnabledForElearning = model.SkillsLabEnabledForELearning ?? false;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Skills laboratory details saved successfully.";
                return RedirectToAction("Medical_EquimentDetails", "Medical_ContinuousAffiliation");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving Medical_SkillsLaboratory");
                ModelState.AddModelError("", "Error while saving data: " + (ex.InnerException?.Message ?? ex.Message));
                return View(model);
            }
        }

        // =====================================================================
        // DEPARTMENT OFFICES & MEDICAL / DENTAL EDUCATION UNIT
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Medical_DepartmentOfficesAndEducationalUnit()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var entity = await _context.MedicalDepartmentOfficesMeus.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                          x.CollegeCode == collegeCode &&
                                          x.CourseLevel == courseLevel);

            if (entity == null)
                return View(new DepartmentOfficesMeuViewModel { CourseLevel = courseLevel });

            var vm = new DepartmentOfficesMeuViewModel
            {
                CourseLevel = courseLevel,

                HasHodRoomWithOfficeAndRecords = entity.HasHodRoomWithOfficeAndRecords,
                HasRoomsForFacultyAndResidents = entity.HasRoomsForFacultyAndResidents,
                FacultyRoomsHaveCommunicationComputerInternet = entity.FacultyRoomsHaveCommunicationComputerInternet,
                HasRoomsForNonTeachingStaff = entity.HasRoomsForNonTeachingStaff
            };

            if (facultyCode != "2")
            {
                vm.HasMedicalEducationUnit = entity.HasMedicalEducationUnit;
                vm.MedicalEducationUnitAreaSqm = entity.MedicalEducationUnitAreaSqm;
                vm.MedicalEducationUnitHasAudioVisual = entity.MedicalEducationUnitHasAudioVisual;
                vm.MedicalEducationUnitHasInternet = entity.MedicalEducationUnitHasInternet;
                vm.MeuCoordinatorName = entity.MeuCoordinatorName;
                vm.MeuCoordinatorPhone = entity.MeuCoordinatorPhone;
                vm.MeuCoordinatorEmail = entity.MeuCoordinatorEmail;
                vm.MeuCoordinatorDesignationDepartment = entity.MeuCoordinatorDesignationDepartment;
                vm.MeuActivitiesLastAcademicYear = entity.MeuActivitiesLastAcademicYear;
                vm.HasMeuMembersListFile = !string.IsNullOrEmpty(entity.MeuMembersListFilePath);
            }
            else
            {
                vm.HasDentalEducationUnit = entity.HasDentalEducationUnit;
                vm.DentalEducationUnitAreaSqm = entity.DentalEducationUnitAreaSqm;
                vm.DentalEducationUnitHasAudioVisual = entity.DentalEducationUnitHasAudioVisual;
                vm.DentalEducationUnitHasInternet = entity.DentalEducationUnitHasInternet;
                vm.DeuCoordinatorName = entity.DeuCoordinatorName;
                vm.DeuCoordinatorPhone = entity.DeuCoordinatorPhone;
                vm.DeuCoordinatorEmail = entity.DeuCoordinatorEmail;
                vm.DeuCoordinatorDesignationDepartment = entity.DeuCoordinatorDesignationDepartment;
                vm.DeuActivitiesLastAcademicYear = entity.DeuActivitiesLastAcademicYear;
                vm.HasDeuMembersListFile = !string.IsNullOrEmpty(entity.DeuMembersListFilePath);
                vm.DEUYearOfStarting = entity.DeuyearOfStarting;
                vm.NatureOfActivities = entity.NatureOfActivities;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_DepartmentOfficesAndEducationalUnit(DepartmentOfficesMeuViewModel vm)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var isDental = facultyCode == "2";
            string? newFilePath = null;
            string? oldFilePath = null;

            try
            {
                var entity = await _context.MedicalDepartmentOfficesMeus
                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                              x.CollegeCode == collegeCode &&
                                              x.CourseLevel == courseLevel);

                if (entity == null)
                {
                    entity = new MedicalDepartmentOfficesMeu
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel,
                        CreatedOn = DateTime.UtcNow
                    };
                    _context.MedicalDepartmentOfficesMeus.Add(entity);
                }

                // The uploaded file depends on the faculty
                var uploaded = isDental ? vm.DeuMembersListFile : vm.MeuMembersListFile;
                if (uploaded is { Length: > 0 })
                    newFilePath = await SaveMeuFileAsync(uploaded);

                // COMMON FIELDS
                entity.HasHodRoomWithOfficeAndRecords = vm.HasHodRoomWithOfficeAndRecords ?? false;
                entity.HasRoomsForFacultyAndResidents = vm.HasRoomsForFacultyAndResidents ?? false;
                entity.FacultyRoomsHaveCommunicationComputerInternet = vm.FacultyRoomsHaveCommunicationComputerInternet ?? false;
                entity.HasRoomsForNonTeachingStaff = vm.HasRoomsForNonTeachingStaff ?? false;

                if (!isDental)
                {
                    var hasMeu = vm.HasMedicalEducationUnit ?? false;
                    entity.HasMedicalEducationUnit = hasMeu;

                    if (!hasMeu)
                    {
                        entity.MedicalEducationUnitAreaSqm = null;
                        entity.MedicalEducationUnitHasAudioVisual = null;
                        entity.MedicalEducationUnitHasInternet = null;
                        entity.MeuCoordinatorName = null;
                        entity.MeuCoordinatorPhone = null;
                        entity.MeuCoordinatorEmail = null;
                        entity.MeuCoordinatorDesignationDepartment = null;
                        entity.MeuActivitiesLastAcademicYear = null;

                        // A file uploaded while "No" is selected is discarded
                        TryDeleteFile(newFilePath);
                        newFilePath = null;
                    }
                    else
                    {
                        entity.MedicalEducationUnitAreaSqm = vm.MedicalEducationUnitAreaSqm;
                        entity.MedicalEducationUnitHasAudioVisual = vm.MedicalEducationUnitHasAudioVisual ?? false;
                        entity.MedicalEducationUnitHasInternet = vm.MedicalEducationUnitHasInternet ?? false;
                        entity.MeuCoordinatorName = vm.MeuCoordinatorName;
                        entity.MeuCoordinatorPhone = vm.MeuCoordinatorPhone;
                        entity.MeuCoordinatorEmail = vm.MeuCoordinatorEmail;
                        entity.MeuCoordinatorDesignationDepartment = vm.MeuCoordinatorDesignationDepartment;
                        entity.MeuActivitiesLastAcademicYear = vm.MeuActivitiesLastAcademicYear;

                        if (newFilePath != null)
                        {
                            oldFilePath = entity.MeuMembersListFilePath;
                            entity.MeuMembersListFilePath = newFilePath;
                        }
                    }
                }
                else
                {
                    // Saved regardless of Yes/No so that "No" is stored too
                    entity.HasDentalEducationUnit = vm.HasDentalEducationUnit;

                    if (vm.HasDentalEducationUnit == false)
                    {
                        entity.DentalEducationUnitAreaSqm = null;
                        entity.DentalEducationUnitHasAudioVisual = null;
                        entity.DentalEducationUnitHasInternet = null;
                        entity.DeuCoordinatorName = null;
                        entity.DeuCoordinatorPhone = null;
                        entity.DeuCoordinatorEmail = null;
                        entity.DeuCoordinatorDesignationDepartment = null;
                        entity.DeuActivitiesLastAcademicYear = null;
                        entity.NatureOfActivities = null;
                        entity.DeuyearOfStarting = null;

                        TryDeleteFile(newFilePath);
                        newFilePath = null;
                    }
                    else
                    {
                        entity.DentalEducationUnitAreaSqm = vm.DentalEducationUnitAreaSqm;
                        entity.DentalEducationUnitHasAudioVisual = vm.DentalEducationUnitHasAudioVisual ?? false;
                        entity.DentalEducationUnitHasInternet = vm.DentalEducationUnitHasInternet ?? false;
                        entity.DeuCoordinatorName = vm.DeuCoordinatorName;
                        entity.DeuCoordinatorPhone = vm.DeuCoordinatorPhone;
                        entity.DeuCoordinatorEmail = vm.DeuCoordinatorEmail;
                        entity.DeuCoordinatorDesignationDepartment = vm.DeuCoordinatorDesignationDepartment;
                        entity.DeuActivitiesLastAcademicYear = vm.DeuActivitiesLastAcademicYear;
                        entity.NatureOfActivities = vm.NatureOfActivities;
                        entity.DeuyearOfStarting = vm.DEUYearOfStarting;

                        if (newFilePath != null)
                        {
                            oldFilePath = entity.DeuMembersListFilePath;
                            entity.DeuMembersListFilePath = newFilePath;
                        }
                    }
                }

                entity.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Delete the replaced file only after the save succeeded
                TryDeleteFile(oldFilePath);

                TempData["SaveSuccess"] = "Saved successfully.";
                return RedirectToAction("Aff_HostelDetails", "ContinuesAffiliation_Facultybased");
            }
            catch (Exception ex)
            {
                TryDeleteFile(newFilePath);
                _logger.LogError(ex, "Error saving Medical_DepartmentOfficesAndEducationalUnit");
                TempData["Error"] = "Save failed: " + (ex.InnerException?.Message ?? ex.Message);
                return View(vm);
            }
        }

        public async Task<IActionResult> ViewMeuMembersList()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            var entity = await _context.MedicalDepartmentOfficesMeus.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode &&
                                          x.FacultyCode == facultyCode &&
                                          x.CourseLevel == courseLevel);

            // Dental colleges store the file in the DEU column
            var path = facultyCode == "2" ? entity?.DeuMembersListFilePath : entity?.MeuMembersListFilePath;

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return NotFound("File not found");

            Response.Headers["Content-Disposition"] = "inline";
            return PhysicalFile(path, "application/pdf");
        }

        // =====================================================================
        // EQUIPMENT AVAILABILITY
        // =====================================================================
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_EquimentDetails(string departmentCode)
        {
            var collegeCode = CollegeCode;

            if (!int.TryParse(FacultyCode, out var facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var model = new EquipmentAvailabilityViewModel
            {
                FacultyId = facultyCode,
                CollegeCode = collegeCode
            };

            model.Courses = await _context.DepartmentMasters
                .Where(d => d.FacultyCode == facultyCode && d.DepartmentFilter == "Y")
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentCode,
                    Text = d.DepartmentName
                })
                .OrderBy(x => x.Text)
                .ToListAsync();

            if (!string.IsNullOrEmpty(departmentCode))
            {
                model.SelectedDepartmentCode = departmentCode;

                var equipments = await _context.MstLaboratoryEquipmentDetails
                    .AsNoTracking()
                    .Where(e => e.CourseCode == departmentCode && e.FacultyId == facultyCode)
                    .OrderBy(e => e.EquipmentId)
                    .ToListAsync();

                var availabilityList = await _context.TblMedicalEquipmentAvailabilities
                    .AsNoTracking()
                    .Where(a => a.FacultyId == facultyCode &&
                                a.CourseCode == departmentCode &&
                                a.CollegeCode == collegeCode)
                    .ToListAsync();

                model.AcademicYear = availabilityList
                    .Select(a => a.AcademicYear)
                    .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a));

                var availabilityById = availabilityList
                    .GroupBy(a => a.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.First());

                model.Equipments = equipments.Select(e =>
                {
                    availabilityById.TryGetValue(e.EquipmentId, out var existing);

                    return new EquipmentItemViewModel
                    {
                        EquipmentID = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        IsAvailable = existing != null,
                        AvailableQuantity = existing?.AvailableQuantity
                    };
                }).ToList();
            }
            else
            {
                model.Equipments = new List<EquipmentItemViewModel>();
                TempData["Info"] = "Please select a department.";
            }

            return View(model);
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_EquimentDetails(EquipmentAvailabilityViewModel model)
        {
            if (string.IsNullOrWhiteSpace(FacultyCode) || string.IsNullOrWhiteSpace(CollegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(FacultyCode, out int facultyId))
            {
                TempData["Error"] = "Invalid faculty code.";
                return RedirectToAction("Login", "Account");
            }

            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(model.SelectedDepartmentCode) || model.Equipments == null)
            {
                TempData["Error"] = "Invalid data submitted.";
                return RedirectToAction(nameof(Medical_EquimentDetails));
            }

            string departmentCode = model.SelectedDepartmentCode;

            var validDepartment = await _context.DepartmentMasters
                .AnyAsync(d => d.DepartmentCode == departmentCode &&
                               d.FacultyCode == facultyId &&
                               d.DepartmentFilter == "Y");

            if (!validDepartment)
            {
                TempData["Error"] = "Invalid department selected.";
                return RedirectToAction(nameof(Medical_EquimentDetails));
            }

            // Only accept equipment ids that really belong to this department
            var validEquipmentIds = await _context.MstLaboratoryEquipmentDetails
                .Where(e => e.CourseCode == departmentCode && e.FacultyId == facultyId)
                .Select(e => e.EquipmentId)
                .ToHashSetAsync();

            var existingList = await _context.TblMedicalEquipmentAvailabilities
                .Where(x => x.FacultyId == facultyId &&
                            x.CourseCode == departmentCode &&
                            x.CollegeCode == collegeCode)
                .ToListAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in model.Equipments)
                {
                    if (!validEquipmentIds.Contains(item.EquipmentID))
                        continue;

                    int quantity = item.AvailableQuantity ?? 0;
                    var existing = existingList.FirstOrDefault(x => x.EquipmentId == item.EquipmentID);

                    if (quantity > 0)
                    {
                        if (existing == null)
                        {
                            _context.TblMedicalEquipmentAvailabilities.Add(
                                new TblMedicalEquipmentAvailability
                                {
                                    FacultyId = facultyId,
                                    CourseCode = departmentCode,
                                    EquipmentId = item.EquipmentID,
                                    IsAvailable = true,
                                    AvailableQuantity = quantity,
                                    CollegeCode = collegeCode,
                                    AcademicYear = EquipmentAcademicYear,
                                    CreatedOn = DateTime.Now
                                });
                        }
                        else
                        {
                            existing.IsAvailable = true;
                            existing.AvailableQuantity = quantity;
                            existing.AcademicYear = EquipmentAcademicYear;
                        }
                    }
                    else if (existing != null)
                    {
                        _context.TblMedicalEquipmentAvailabilities.Remove(existing);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Equipment availability saved successfully.";
                return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving Medical_EquimentDetails");

                // Redirect (not View(model)) so the department list and equipment grid are reloaded
                TempData["Error"] = "Error while saving data: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
            }
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> DownloadEquipmentTemplate(string departmentCode)
        {
            if (!int.TryParse(FacultyCode, out var facultyId) || string.IsNullOrWhiteSpace(CollegeCode))
                return Unauthorized();

            var validDepartment = await _context.DepartmentMasters
                .AnyAsync(d => d.DepartmentCode == departmentCode && d.FacultyCode == facultyId && d.DepartmentFilter == "Y");

            if (!validDepartment)
                return BadRequest("Invalid department selected.");

            var equipments = await _context.MstLaboratoryEquipmentDetails
                .AsNoTracking()
                .Where(e => e.CourseCode == departmentCode && e.FacultyId == facultyId)
                .OrderBy(e => e.EquipmentId)
                .ToListAsync();

            // Pre-fill the quantities already saved, so the template reflects current data
            var saved = await _context.TblMedicalEquipmentAvailabilities
                .AsNoTracking()
                .Where(x => x.FacultyId == facultyId &&
                            x.CourseCode == departmentCode &&
                            x.CollegeCode == CollegeCode)
                .ToListAsync();

            var savedQty = saved
                .GroupBy(x => x.EquipmentId)
                .ToDictionary(g => g.Key, g => g.First().AvailableQuantity ?? 0);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Equipment Availability");
            var headers = new[]
            {
                "FacultyId", "CourseCode", "CollegeCode", "EquipmentId",
                "EquipmentName", "AvailableQuantity", "AcademicYear"
            };

            for (var index = 0; index < headers.Length; index++)
                sheet.Cell(1, index + 1).Value = headers[index];

            var headerRange = sheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0f2545");
            headerRange.Style.Font.FontColor = XLColor.White;

            for (var rowIndex = 0; rowIndex < equipments.Count; rowIndex++)
            {
                var row = rowIndex + 2;
                var equipment = equipments[rowIndex];
                sheet.Cell(row, 1).Value = facultyId;
                sheet.Cell(row, 2).Value = departmentCode;
                sheet.Cell(row, 3).Value = CollegeCode;
                sheet.Cell(row, 4).Value = equipment.EquipmentId;
                sheet.Cell(row, 5).Value = equipment.EquipmentName ?? string.Empty;
                sheet.Cell(row, 6).Value = savedQty.TryGetValue(equipment.EquipmentId, out var q) ? q : 0;
                sheet.Cell(row, 7).Value = EquipmentAcademicYear;
            }

            sheet.Columns().AdjustToContents();
            sheet.Column(5).Width = Math.Min(Math.Max(sheet.Column(5).Width, 24), 50);
            sheet.Column(6).Style.Protection.Locked = false;
            sheet.Protect();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"EquipmentAvailability_{departmentCode}.xlsx");
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadEquipmentAvailability(
            string departmentCode,
            IFormFile equipmentFile)
        {
            if (!int.TryParse(FacultyCode, out var facultyId) || string.IsNullOrWhiteSpace(CollegeCode))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(departmentCode) || equipmentFile == null || equipmentFile.Length == 0)
            {
                TempData["Error"] = "Select a department and an Excel file.";
                return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
            }

            var validDepartment = await _context.DepartmentMasters
                .AnyAsync(d => d.DepartmentCode == departmentCode && d.FacultyCode == facultyId && d.DepartmentFilter == "Y");

            if (!validDepartment)
            {
                TempData["Error"] = "Invalid department selected.";
                return RedirectToAction(nameof(Medical_EquimentDetails));
            }

            if (!string.Equals(Path.GetExtension(equipmentFile.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only .xlsx files are supported.";
                return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
            }

            var equipmentIds = await _context.MstLaboratoryEquipmentDetails
                .Where(e => e.CourseCode == departmentCode && e.FacultyId == facultyId)
                .Select(e => e.EquipmentId)
                .ToHashSetAsync();

            var rows = new List<(int EquipmentId, int Quantity, string? AcademicYear)>();
            try
            {
                using var stream = equipmentFile.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet == null)
                    throw new InvalidDataException("The workbook has no worksheet.");

                var requiredHeaders = new[]
                {
                    "FacultyId", "CourseCode", "CollegeCode", "EquipmentId",
                    "EquipmentName", "AvailableQuantity", "AcademicYear"
                };

                for (var index = 0; index < requiredHeaders.Length; index++)
                {
                    if (!string.Equals(sheet.Cell(1, index + 1).GetString().Trim(), requiredHeaders[index], StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("The uploaded template columns do not match the downloaded template.");
                }

                foreach (var row in sheet.RowsUsed().Skip(1))
                {
                    if (row.CellsUsed().All(cell => string.IsNullOrWhiteSpace(cell.GetString())))
                        continue;

                    if (row.Cell(1).GetValue<int>() != facultyId ||
                        !string.Equals(row.Cell(2).GetString().Trim(), departmentCode, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(row.Cell(3).GetString().Trim(), CollegeCode, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("FacultyId, CourseCode, and CollegeCode must match the current session and selected department.");

                    var equipmentId = row.Cell(4).GetValue<int>();
                    var quantity = row.Cell(6).GetValue<int>();
                    if (!equipmentIds.Contains(equipmentId) || quantity < 0)
                        throw new InvalidDataException("The workbook contains an invalid equipment or quantity value.");

                    var academicYear = row.Cell(7).GetString().Trim();
                    if (!string.Equals(academicYear, EquipmentAcademicYear, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException($"AcademicYear must be {EquipmentAcademicYear}.");

                    rows.Add((equipmentId, quantity, EquipmentAcademicYear));
                }
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is FormatException || ex is ArgumentException)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
            }

            if (rows.Count == 0)
            {
                TempData["Error"] = "The uploaded workbook contains no equipment rows.";
                return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
            }

            try
            {
                var existingRows = await _context.TblMedicalEquipmentAvailabilities
                    .Where(x => x.FacultyId == facultyId && x.CourseCode == departmentCode && x.CollegeCode == CollegeCode)
                    .ToListAsync();

                foreach (var row in rows)
                {
                    var existing = existingRows.FirstOrDefault(x => x.EquipmentId == row.EquipmentId);

                    if (row.Quantity > 0)
                    {
                        if (existing == null)
                        {
                            _context.TblMedicalEquipmentAvailabilities.Add(new TblMedicalEquipmentAvailability
                            {
                                FacultyId = facultyId,
                                CourseCode = departmentCode,
                                CollegeCode = CollegeCode,
                                EquipmentId = row.EquipmentId,
                                AvailableQuantity = row.Quantity,
                                IsAvailable = true,
                                AcademicYear = row.AcademicYear,
                                CreatedOn = DateTime.Now
                            });
                        }
                        else
                        {
                            existing.AvailableQuantity = row.Quantity;
                            existing.IsAvailable = true;
                            existing.AcademicYear = row.AcademicYear;
                        }
                    }
                    else if (existing != null)
                    {
                        // Same rule as the manual save: quantity 0 means "not available", so remove the row
                        _context.TblMedicalEquipmentAvailabilities.Remove(existing);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Equipment availability uploaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading equipment availability");
                TempData["Error"] = "Error while saving data: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Medical_EquimentDetails), new { departmentCode });
        }

        // =====================================================================
        // SKILLS LAB EQUIPMENT
        // NOTE: TblMedicalSkillsLabEquipments has no CollegeCode/FacultyCode,
        // so all colleges share the same rows. See notes in the reply.
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Medical_SkillsLabEquipment()
        {
            var entities = await _context.TblMedicalSkillsLabEquipments
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            if (!entities.Any())
            {
                entities = SeedSkillsLabEquipment();
                _context.TblMedicalSkillsLabEquipments.AddRange(entities);
                await _context.SaveChangesAsync();
            }

            var vm = new SkillsLabEquipmentViewModel
            {
                Items = entities.Select(e => new SkillsLabEquipmentItemViewModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    IsRequired = e.IsRequired,
                    IsAvailable = e.IsAvailable,
                    Quantity = e.Quantity
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_SkillsLabEquipment(SkillsLabEquipmentViewModel model)
        {
            if (!ModelState.IsValid || model.Items == null)
                return View(model);

            try
            {
                var ids = model.Items.Select(i => i.Id).ToList();

                var entities = await _context.TblMedicalSkillsLabEquipments
                    .Where(e => ids.Contains(e.Id))
                    .ToListAsync();

                foreach (var item in model.Items)
                {
                    var entity = entities.FirstOrDefault(e => e.Id == item.Id);
                    if (entity == null) continue;

                    entity.IsAvailable = item.IsAvailable;
                    entity.Quantity = item.IsAvailable ? item.Quantity : null;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Skills lab equipment saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Medical_SkillsLabEquipment");
                TempData["Error"] = "Error while saving data: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Medical_SkillsLabEquipment));
        }

        private List<TblMedicalSkillsLabEquipment> SeedSkillsLabEquipment()
        {
            return new List<TblMedicalSkillsLabEquipment>
            {
                new() { Name = "First aid, bandaging, splinting trainer",        IsRequired = true, DisplayOrder = 1 },
                new() { Name = "Basic Life Support (BLS), CPR mannequin",        IsRequired = true, DisplayOrder = 2 },
                new() { Name = "Injection trainers (SC / IM / IV)",               IsRequired = true, DisplayOrder = 3 },
                new() { Name = "Urine catheter insertion mannequin",             IsRequired = true, DisplayOrder = 4 },
                new() { Name = "Skin & fascia suturing model",                   IsRequired = true, DisplayOrder = 5 },
                new() { Name = "Breast examination model / mannequin",           IsRequired = true, DisplayOrder = 6 },
                new() { Name = "Gynecological examination model / IUCD trainer", IsRequired = true, DisplayOrder = 7 },
                new() { Name = "Obstetric examination / delivery mannequins",    IsRequired = true, DisplayOrder = 8 },
                new() { Name = "Neonatal & paediatric resuscitation mannequins", IsRequired = true, DisplayOrder = 9 },
                new() { Name = "Whole body mannequin",                           IsRequired = true, DisplayOrder = 10 },
                new() { Name = "Trauma mannequin",                               IsRequired = true, DisplayOrder = 11 }
            };
        }
    }
}