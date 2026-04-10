using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Medical_Affiliation.Controllers
{


    public class Medical_ContinuousAffiliationController : BaseController
    {

        private readonly ApplicationDbContext _context;

        public Medical_ContinuousAffiliationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }



        // GET: /SmallGroupTeaching/Edit
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_LandBuildingdetails()
        {
            var courseLevel = CourseLevel;

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;


            if (string.IsNullOrEmpty(facultyCode))
            {
                return RedirectToAction("Login", "Account");
            }

            // ============================
            // 🔹 TABLE 1: SmallGroupTeachings
            // ============================
            var teaching = await _context.SmallGroupTeachings
                                         .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                             x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);

            // ============================
            // 🔹 TABLE 2: Medical_StudentPracticalLabs
            // ============================
            var labs = await _context.MedicalStudentPracticalLabs
                                     .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                             x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);

            var museum = await _context.MedicalMuseums
                               .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                             x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);


            // ============================
            var admin = await _context.MedicalAdministrativePhysicalFacilities
                                      .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                             x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);

            // ============================
            // 🔹 FIRST TIME DEFAULTS
            // ============================
            if (teaching == null && labs == null)
            {
                return View(new SmallGroupTeachingViewModel
                {
                    AnnualMbbsIntake = 100,
                    SmallGroupBatchSize = 15
                });
            }

            // ============================
            // 🔹 MAP TO VIEWMODEL
            // ============================
            var vm = new SmallGroupTeachingViewModel
            {
                // --- GENERAL ---
                AnnualMbbsIntake = teaching?.AnnualMbbsIntake ?? 100,
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


                // Types
                SeparateAnatomyMuseumAvailable = museum?.SeparateAnatomyMuseumAvailable,
                PathologyForensicSharedMuseum = museum?.PathologyForensicSharedMuseum,
                PharmMicroCommSharedMuseum = museum?.PharmMicroCommSharedMuseum,

                // Seating & area
                // Seating & area
                SeatingCapacityPerMuseum = museum?.SeatingCapacityPerMuseum ?? 0,
                SeatingAreaAvailableSqm = museum?.SeatingAreaAvailableSqm ?? 0,
                SeatingAreaRequiredSqm = museum?.SeatingAreaRequiredSqm ?? 0,
                SeatingAreaDeficiencySqm = museum?.SeatingAreaDeficiencySqm ?? 0,


                // Facilities
                MuseumsHaveAV = museum?.MuseumsHaveAv,
                MuseumsHaveInternet = museum?.MuseumsHaveInternet,
                MuseumsDigitallyLinked = museum?.MuseumsDigitallyLinked,
                MuseumsHaveRacksShelves = museum?.MuseumsHaveRacksShelves,
                MuseumsHaveRadiologyDisplay = museum?.MuseumsHaveRadiologyDisplay,
                TeachingTimeSharingProgrammed = museum?.TeachingTimeSharingProgrammed,

                // ====================================================
                // 🆕 LAND DETAILS (NEWLY ADDED)
                // ====================================================
                IsMinimumLandAvailable = teaching?.IsMinimumLandAvailable,
                LandDetailsIfYes = teaching?.LandDetailsIfYes,
                HasPurchasePlanIfNo = teaching?.HasPurchasePlanIfNo,
                HasBudgetProvisionIfNo = teaching?.HasBudgetProvisionIfNo,
                HasFutureExpansionSpace = teaching?.HasFutureExpansionSpace,
                HasLandRecordsFile = teaching?.LandRecordsFile != null,
                HasApprovedBuildingPlanFile = teaching?.ApprovedBuildingPlanFile != null,


                // ====================================================
                // 🆕 BUILDING DETAILS (NEWLY ADDED)
                // ====================================================
                IsBuildingAsPerCouncilNorms = teaching?.IsBuildingAsPerCouncilNorms,
                LandOwnershipType = teaching?.LandOwnershipType,
                BuildingOwnershipType = teaching?.BuildingOwnershipType,
                FloorAreaSqFt = teaching?.FloorAreaSqFt ?? 0,
                NumberOfBlocks = teaching?.NumberOfBlocks ?? 0,
                NumberOfFloors = teaching?.NumberOfFloors ?? 0,
                YearOfConstruction = teaching?.YearOfConstruction ?? 0,


                // ---------- ADMINISTRATIVE ----------
                PrincipalChamberAreaSqFt = admin?.PrincipalChamberAreaSqFt,
                OfficeRoomAreaSqFt = admin?.OfficeRoomAreaSqFt,
                StaffRoomsAreaSqFt = admin?.StaffRoomsAreaSqFt,
                LectureHallsAreaSqFt = admin?.LectureHallsAreaSqFt,
                LaboratoriesAreaSqFt = admin?.LaboratoriesAreaSqFt,
                SeminarHallAreaSqFt = admin?.SeminarHallAreaSqFt,
                AuditoriumAreaSqFt = admin?.AuditoriumAreaSqFt,
                MuseumAreaSqFt = admin?.MuseumAreaSqFt,

                ExaminationHallAvailable = admin?.ExaminationHallAvailable,
                AnimalHouseAvailable = admin?.AnimalHouseAvailable,   // ⭐ FIXED

                WorkshopStaffCount = admin?.WorkshopStaffCount,
                WorkshopEquipmentDetails = admin?.WorkshopEquipmentDetails,
                WorkshopScopeOfWork = admin?.WorkshopScopeOfWork,

                AnimalHouseAreaSqFt = admin?.AnimalHouseAreaSqFt,
                AnimalHouseStaffCount = admin?.AnimalHouseStaffCount,
                AnimalTypes = admin?.AnimalTypes,

                CommitteeRoomsAreaSqFt = admin?.CommitteeRoomsAreaSqFt,
                CommonRoomMenAvailable = admin?.CommonRoomMenAvailable,
                CommonRoomWomenAvailable = admin?.CommonRoomWomenAvailable,

                StudentHostelAvailable = admin?.StudentHostelAvailable,
                StaffQuartersPrincipal = admin?.StaffQuartersPrincipal,
                StaffQuartersOtherStaff = admin?.StaffQuartersOtherStaff,
                StaffQuartersTeachingAncillary = admin?.StaffQuartersTeachingAncillary,

                RegisteredUnderAnatomyAct = admin?.RegisteredUnderAnatomyAct

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

            // ✅ SESSION SAFE CHECK
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

            // ✅ LOG ERRORS BUT DO NOT BLOCK SAVE
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Console.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }
            }

            // ✅ CALCULATIONS
            model.RequiredAreaSqm = model.SmallGroupStudents * 1.2m;
            model.AreaDeficiencySqm = Math.Max(0, model.RequiredAreaSqm - model.AvailableAreaSqm);

            using var transaction = await _context.Database.BeginTransactionAsync();

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

                if (model.LandRecordsDocument != null && model.LandRecordsDocument.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.LandRecordsDocument.CopyToAsync(ms);
                    teaching.LandRecordsFile = ms.ToArray();
                }

                // BUILDING
                teaching.IsBuildingAsPerCouncilNorms = model.IsBuildingAsPerCouncilNorms ?? false;
                teaching.LandOwnershipType = model.LandOwnershipType;
                teaching.BuildingOwnershipType = model.BuildingOwnershipType;
                teaching.FloorAreaSqFt = model.FloorAreaSqFt;
                teaching.NumberOfBlocks = model.NumberOfBlocks;
                teaching.NumberOfFloors = model.NumberOfFloors;
                teaching.YearOfConstruction = model.YearOfConstruction;

                if (model.ApprovedBuildingPlanDocument != null && model.ApprovedBuildingPlanDocument.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.ApprovedBuildingPlanDocument.CopyToAsync(ms);
                    teaching.ApprovedBuildingPlanFile = ms.ToArray();
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

                admin.ExaminationHallAvailable = model.ExaminationHallAvailable;
                admin.AnimalHouseAvailable = model.AnimalHouseAvailable;

                admin.WorkshopStaffCount = model.WorkshopStaffCount;
                admin.WorkshopEquipmentDetails = model.WorkshopEquipmentDetails;
                admin.WorkshopScopeOfWork = model.WorkshopScopeOfWork;

                admin.AnimalHouseAreaSqFt = model.AnimalHouseAreaSqFt;
                admin.AnimalHouseStaffCount = model.AnimalHouseStaffCount;
                admin.AnimalTypes = model.AnimalTypes;

                admin.CommitteeRoomsAreaSqFt = model.CommitteeRoomsAreaSqFt;
                admin.CommonRoomMenAvailable = model.CommonRoomMenAvailable;
                admin.CommonRoomWomenAvailable = model.CommonRoomWomenAvailable;

                admin.StudentHostelAvailable = model.StudentHostelAvailable;
                admin.StaffQuartersPrincipal = model.StaffQuartersPrincipal;
                admin.StaffQuartersOtherStaff = model.StaffQuartersOtherStaff;
                admin.StaffQuartersTeachingAncillary = model.StaffQuartersTeachingAncillary;

                admin.RegisteredUnderAnatomyAct = model.RegisteredUnderAnatomyAct;

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
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Data saved successfully!";
                return RedirectToAction("Medical_LandBuildingdetails");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Something went wrong while saving data.";
                return View(model);
            }
        }


        public async Task<IActionResult> ViewLandRecords()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var teaching = await _context.SmallGroupTeachings
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode &&
                                             x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);

            if (teaching?.LandRecordsFile == null)
                return NotFound();

            return File(teaching.LandRecordsFile, "application/pdf");
        }


        public async Task<IActionResult> ViewBuildingPlan()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var teaching = await _context.SmallGroupTeachings
                .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode && x.CourseLevel == courseLevel);

            if (teaching?.ApprovedBuildingPlanFile == null)
                return NotFound();

            return File(teaching.ApprovedBuildingPlanFile, "application/pdf");
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_SkillsLaboratory()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var lab = await _context.MedicalSkillsLaboratories
                                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode);

            if (lab == null)
            {
                return View(new SkillsLabViewModel());
            }

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

            // ================================
            // SERVER-SIDE CALCULATION
            // ================================
            model.TotalAreaRequiredSqm = model.AnnualMbbsIntake * 1.2m;
            model.TotalAreaDeficiencySqm =
                Math.Max(0, model.TotalAreaRequiredSqm - model.TotalAreaAvailableSqm);

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

            // ================================
            // UPDATE FIELDS
            // ================================
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

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Saved successfully!";
                return RedirectToAction(nameof(Medical_SkillsLaboratory));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error while saving data");
                return View(model);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Medical_DepartmentOfficesAndEducationalUnit()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var entity = await _context.MedicalDepartmentOfficesMeus
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseLevel == courseLevel);

            if (entity == null)
            {
                return View(new DepartmentOfficesMeuViewModel
                {
                    CourseLevel = courseLevel
                });
            }

            var vm = new DepartmentOfficesMeuViewModel
            {
                CourseLevel = courseLevel,

                HasHodRoomWithOfficeAndRecords = entity.HasHodRoomWithOfficeAndRecords,
                HasRoomsForFacultyAndResidents = entity.HasRoomsForFacultyAndResidents,
                FacultyRoomsHaveCommunicationComputerInternet = entity.FacultyRoomsHaveCommunicationComputerInternet,
                HasRoomsForNonTeachingStaff = entity.HasRoomsForNonTeachingStaff,

                HasMedicalEducationUnit = entity.HasMedicalEducationUnit,
                MedicalEducationUnitAreaSqm = entity.MedicalEducationUnitAreaSqm,
                MedicalEducationUnitHasAudioVisual = entity.MedicalEducationUnitHasAudioVisual,
                MedicalEducationUnitHasInternet = entity.MedicalEducationUnitHasInternet,
                MeuCoordinatorName = entity.MeuCoordinatorName,
                MeuCoordinatorPhone = entity.MeuCoordinatorPhone,
                MeuCoordinatorEmail = entity.MeuCoordinatorEmail,
                MeuCoordinatorDesignationDepartment = entity.MeuCoordinatorDesignationDepartment,
                MeuActivitiesLastAcademicYear = entity.MeuActivitiesLastAcademicYear,
                HasMeuMembersListFile = entity.MeuMembersListFile != null
            };

            return View(vm);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_DepartmentOfficesAndEducationalUnit(DepartmentOfficesMeuViewModel vm)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (!ModelState.IsValid)
                return View(vm);

            var entity = await _context.MedicalDepartmentOfficesMeus
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
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

            entity.HasHodRoomWithOfficeAndRecords = vm.HasHodRoomWithOfficeAndRecords ?? false;
            entity.HasRoomsForFacultyAndResidents = vm.HasRoomsForFacultyAndResidents ?? false;
            entity.FacultyRoomsHaveCommunicationComputerInternet = vm.FacultyRoomsHaveCommunicationComputerInternet ?? false;
            entity.HasRoomsForNonTeachingStaff = vm.HasRoomsForNonTeachingStaff ?? false;
            entity.HasMedicalEducationUnit = vm.HasMedicalEducationUnit ?? false;

            if (vm.HasMedicalEducationUnit == false)
            {
                entity.MedicalEducationUnitAreaSqm = null;
                entity.MedicalEducationUnitHasAudioVisual = null;
                entity.MedicalEducationUnitHasInternet = null;
                entity.MeuCoordinatorName = null;
                entity.MeuCoordinatorPhone = null;
                entity.MeuCoordinatorEmail = null;
                entity.MeuCoordinatorDesignationDepartment = null;
                entity.MeuActivitiesLastAcademicYear = null;
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

                if (vm.MeuMembersListFile != null && vm.MeuMembersListFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await vm.MeuMembersListFile.CopyToAsync(ms);
                    entity.MeuMembersListFile = ms.ToArray();
                }
            }

            entity.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SaveSuccess"] = "Saved successfully.";
            return RedirectToAction(nameof(Medical_DepartmentOfficesAndEducationalUnit));
        }

        public async Task<IActionResult> ViewMeuMembersList()
        {
            var entity = await _context.MedicalDepartmentOfficesMeus.FirstOrDefaultAsync();
            if (entity == null || entity.MeuMembersListFile == null)
                return NotFound();

            var contentType = "application/pdf"; // or detect if you store type
            Response.Headers["Content-Disposition"] = "inline";

            return File(entity.MeuMembersListFile, contentType);
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_EquimentDetails(string departmentCode)
        {
            // var facultyCodeStr = HttpContext.Session.GetString("FacultyCode") ?? "1";
            var collegeCode = CollegeCode;
            var facultyCodeStr = FacultyCode;
            int facultyCode = Convert.ToInt32(facultyCodeStr);

            var model = new EquipmentAvailabilityViewModel();

            // 1️⃣ Load Department dropdown (Faculty-wise)
            model.Courses = await _context.DepartmentMasters
                .Where(d => d.FacultyCode == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentCode,    // MD001
                    Text = d.DepartmentName      // Anatomy
                })
                .OrderBy(x => x.Text)
                .ToListAsync();


            // 2️⃣ Load equipment if department selected
            if (!string.IsNullOrEmpty(departmentCode))
            {

                model.SelectedDepartmentCode = departmentCode;

                // ✅ Load equipment list
                var equipments = await _context.MstLaboratoryEquipmentDetails
                    .Where(e =>
                        e.CourseCode == departmentCode &&
                        e.FacultyId == facultyCode)
                    .OrderBy(e => e.EquipmentId)
                    .ToListAsync();

                // ✅ Load availability ONCE (Fix N+1)
                var availabilityList = await _context.TblMedicalEquipmentAvailabilities
                    .Where(a =>
                        a.FacultyId == facultyCode &&
                        a.CourseCode == departmentCode &&
                        a.CollegeCode == collegeCode)
                    .ToListAsync();

                model.Equipments = equipments.Select(e =>
                {
                    var existing = availabilityList
                        .FirstOrDefault(a => a.EquipmentId == e.EquipmentId);

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
            //var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? "1";
            //int facultyId = Convert.ToInt32(facultyCode);



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

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(model.SelectedDepartmentCode) || model.Equipments == null)
            {
                TempData["Error"] = "Invalid data submitted.";
                return RedirectToAction(nameof(Medical_EquimentDetails));
            }

            string departmentCode = model.SelectedDepartmentCode; // MD001

            var existingList = await _context.TblMedicalEquipmentAvailabilities
                .Where(x =>
                    x.FacultyId == facultyId &&
                    x.CourseCode == departmentCode &&
                    x.CollegeCode == collegeCode)
                .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                foreach (var item in model.Equipments)
                {
                    int quantity = item.AvailableQuantity ?? 0;

                    var existing = existingList.FirstOrDefault(x => x.EquipmentId == item.EquipmentID);

                    if (quantity > 0)
                    {
                        // AVAILABLE
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
                                });
                        }
                        else
                        {
                            existing.IsAvailable = true;
                            existing.AvailableQuantity = quantity;
                        }
                    }
                    else
                    {
                        // NOT AVAILABLE → Remove record
                        if (existing != null)
                        {
                            _context.TblMedicalEquipmentAvailabilities.Remove(existing);
                        }
                    }

                }
                await transaction.CommitAsync();

                TempData["Success"] = "Equipment availability saved successfully.";

                return RedirectToAction(nameof(Medical_EquimentDetails),
                    new { departmentCode = departmentCode });

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] = "Error while saving data.";
                return View(model);
            }

        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Medical_EquimentDetails(EquipmentAvailabilityViewModel model)
        //{
        //    var facultyCode = HttpContext.Session.GetString("FacultyCode");
        //    int facultyId = Convert.ToInt32(facultyCode);

        //    if (string.IsNullOrEmpty(model.SelectedDepartmentCode) || model.Equipments == null)
        //    {
        //        TempData["Error"] = "Invalid data submitted.";
        //        return RedirectToAction(nameof(Medical_EquimentDetails));
        //    }

        //    string departmentCode = model.SelectedDepartmentCode; // ✅ STRING (MD001)

        //    foreach (var item in model.Equipments)
        //    {
        //        var existing = await _context.TblMedicalEquipmentAvailabilities
        //            .FirstOrDefaultAsync(x =>
        //                x.FacultyId == facultyId &&
        //                x.CourseCode == departmentCode &&      // ✅ string == string
        //                x.EquipmentId == item.EquipmentID);

        //        if (item.IsAvailable)
        //        {
        //            if (existing == null)
        //            {
        //                _context.TblMedicalEquipmentAvailabilities.Add(
        //                    new TblMedicalEquipmentAvailability
        //                    {
        //                        FacultyId = facultyId,
        //                        CourseCode = departmentCode,    // ✅ STRING
        //                        EquipmentId = item.EquipmentID,
        //                        IsAvailable = true,
        //                        AvailableQuantity = item.AvailableQuantity ?? 0
        //                    });
        //            }
        //            else
        //            {
        //                existing.IsAvailable = true;
        //                existing.AvailableQuantity = item.AvailableQuantity ?? 0;
        //            }
        //        }
        //        else if (existing != null)
        //        {
        //            _context.TblMedicalEquipmentAvailabilities.Remove(existing);
        //        }
        //    }




        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Equipment availability saved successfully.";

        //    return RedirectToAction(nameof(Medical_EquimentDetails),
        //        new { departmentCode = departmentCode });
        //}


        [HttpGet]
        public async Task<IActionResult> Medical_SkillsLabEquipment()
        {
            // Load from DB; if empty, seed from NMC list once
            var entities = await _context.TblMedicalSkillsLabEquipments
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            if (!entities.Any())
            {
                entities = SeedSkillsLabEquipment();     // you create this method
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ids = model.Items.Select(i => i.Id).ToList();

            var entities = await _context.TblMedicalSkillsLabEquipments
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            foreach (var item in model.Items)
            {
                var entity = entities.First(e => e.Id == item.Id);
                entity.IsAvailable = item.IsAvailable;
                entity.Quantity = item.IsAvailable ? item.Quantity : null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Medical_SkillsLabEquipment));
        }


        private List<TblMedicalSkillsLabEquipment> SeedSkillsLabEquipment()
        {
            return new List<TblMedicalSkillsLabEquipment>
                {
                    new() { Name = "First aid, bandaging, splinting trainer",        IsRequired = true,  DisplayOrder = 1 },
                    new() { Name = "Basic Life Support (BLS), CPR mannequin",        IsRequired = true,  DisplayOrder = 2 },
                    new() { Name = "Injection trainers (SC / IM / IV)",               IsRequired = true,  DisplayOrder = 3 },
                    new() { Name = "Urine catheter insertion mannequin",             IsRequired = true,  DisplayOrder = 4 },
                    new() { Name = "Skin & fascia suturing model",                   IsRequired = true,  DisplayOrder = 5 },
                    new() { Name = "Breast examination model / mannequin",           IsRequired = true,  DisplayOrder = 6 },
                    new() { Name = "Gynecological examination model / IUCD trainer", IsRequired = true,  DisplayOrder = 7 },
                    new() { Name = "Obstetric examination / delivery mannequins",    IsRequired = true,  DisplayOrder = 8 },
                    new() { Name = "Neonatal & paediatric resuscitation mannequins", IsRequired = true,  DisplayOrder = 9 },
                    new() { Name = "Whole body mannequin",                           IsRequired = true,  DisplayOrder = 10 },
                    new() { Name = "Trauma mannequin",                               IsRequired = true,  DisplayOrder = 11 }
                };
        }



    }
}
