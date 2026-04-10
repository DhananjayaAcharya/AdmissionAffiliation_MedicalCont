//using Medical_Affiliation.DATA;
//using Medical_Affiliation.Models;
//using Medical_Affiliation.Services.Interfaces;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Medical_Affiliation.Services.Faculty
//{
//    public class CALandClassEquipmentService :  ICALandClassEquipmentService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IUserContext _userContext;

//        public CALandClassEquipmentService( ApplicationDbContext context, IUserContext userContext)
//        {
//            _context = context;
//            _userContext = userContext;
//        }
//        public async Task<SkillsLabDisplayViewModel> GetSkillsLabService()
//        {
//            var facultyId = _userContext.FacultyId;
//            var collegeCode = _userContext.CollegeCode;
//            var lab = await _context.MedicalSkillsLaboratories
//                            .AsNoTracking()
//                            .Where(x => x.FacultyCode == facultyId.ToString())
//                            .Select(x => new SkillsLabDisplayViewModel
//                            {
//                                AnnualMbbsIntake = x.AnnualMbbsIntake,
//                                TotalAreaAvailableSqm = x.TotalAreaAvailableSqm,
//                                TotalAreaRequiredSqm = x.TotalAreaRequiredSqm,
//                                TotalAreaDeficiencySqm = x.TotalAreaDeficiencySqm,
//                                SixWeeksTrainingCompletedBeforeClinical = x.SixWeeksTrainingCompletedBeforeClinical,
//                                NumberOfExaminationRooms = x.NumberOfExaminationRooms,
//                                HasMinFourExamRooms = x.HasMinFourExamRooms,
//                                HasDemoRoomSmallGroups = x.HasDemoRoomSmallGroups,
//                                HasDebriefArea = x.HasDebriefArea,
//                                HasFacultyCoordinatorRoom = x.HasFacultyCoordinatorRoom,
//                                HasSupportStaffRoom = x.HasSupportStaffRoom,
//                                HasStorageForMannequins = x.HasStorageForMannequins,
//                                HasVideoRecordingFacility = x.HasVideoRecordingFacility,
//                                NumberOfSkillStations = x.NumberOfSkillStations,
//                                HasGroupAndIndividualStations = x.HasGroupAndIndividualStations,
//                                HasRequiredTrainersAndMannequins = x.HasRequiredTrainersAndMannequins,
//                                HasDedicatedTechnicalOfficer = x.HasDedicatedTechnicalOfficer,
//                                HasAdequateSupportStaff = x.HasAdequateSupportStaff,
//                                TeachingAreasHaveAV = x.TeachingAreasHaveAv,
//                                TeachingAreasHaveInternet = x.TeachingAreasHaveInternet,
//                                SkillsLabEnabledForELearning = x.SkillsLabEnabledForElearning,
//                            }).FirstOrDefaultAsync();




//            return lab;
//        }


//        public async Task<DepartmentOfficesMeuDisplayViewModel> GetDepartmentOfficesMeu()
//        {
//            var facultyId = _userContext.FacultyId;
//            var collegeCode = _userContext.CollegeCode;
//            var entity = await _context.MedicalDepartmentOfficesMeus.FirstOrDefaultAsync();
//            var vm = new DepartmentOfficesMeuDisplayViewModel
//            {
//                HasHodRoomWithOfficeAndRecords = entity.HasHodRoomWithOfficeAndRecords,
//                HasRoomsForFacultyAndResidents = entity.HasRoomsForFacultyAndResidents,
//                FacultyRoomsHaveCommunicationComputerInternet = entity.FacultyRoomsHaveCommunicationComputerInternet,
//                HasRoomsForNonTeachingStaff = entity.HasRoomsForNonTeachingStaff,

//                HasMedicalEducationUnit = entity.HasMedicalEducationUnit,
//                MedicalEducationUnitAreaSqm = entity.MedicalEducationUnitAreaSqm,
//                MedicalEducationUnitHasAudioVisual = entity.MedicalEducationUnitHasAudioVisual,
//                MedicalEducationUnitHasInternet = entity.MedicalEducationUnitHasInternet,
//                MeuCoordinatorName = entity.MeuCoordinatorName,
//                MeuCoordinatorPhone = entity.MeuCoordinatorPhone,
//                MeuCoordinatorEmail = entity.MeuCoordinatorEmail,
//                MeuCoordinatorDesignationDepartment = entity.MeuCoordinatorDesignationDepartment,
//                MeuMembersListDescription = entity.MeuMembersListDescription,
//                MeuActivitiesLastAcademicYear = entity.MeuActivitiesLastAcademicYear,
//                HasMeuMembersListFile = entity.MeuMembersListFile != null && entity.MeuMembersListFile.Length > 0
//            };

//            return vm;
//        }


//        public async Task<LaboratoryEquipmentDisplayViewModel> GetLaboratoryEquipmentService()
//        {
//            var facultyId = _userContext.FacultyId;

//            var data = await (
//                 from eq in _context.MstLaboratoryEquipmentDetails.AsNoTracking()
//                 join av in _context.TblMedicalEquipmentAvailabilities.AsNoTracking()
//                     on eq.EquipmentId equals av.EquipmentId 
//                 where eq.FacultyId == facultyId
//                       && !string.IsNullOrWhiteSpace(eq.CourseCode)
//                 select new LaboratoryEquipmentDisplayItemVM
//                 {
//                     EquipmentId = eq.EquipmentId,
//                     EquipmentName = eq.EquipmentName,
//                     RequiredAsPerNorm = eq.RequiredAsPerNorm,
//                     //CourseCode = eq.CourseCode,          // ✅ REQUIRED
//                     Subject = eq.Subjects ?? "General",
//                     IsAvailable = av != null && av.IsAvailable,
//                     AvailableQuantity = av.AvailableQuantity
//                 }
//             ).ToListAsync();

//            var vm = new LaboratoryEquipmentDisplayViewModel
//            {
//                Courses = data
//                .GroupBy(x => x.CourseCode)
//                .Select(courseGroup => new LaboratoryEquipmentCourseGroupVM
//                {
//                    CourseCode = courseGroup.Key,
//                    Subjects = courseGroup
//                        .GroupBy(x => x.Subject)
//                        .Select(subjectGroup => new LaboratoryEquipmentSubjectGroupVM
//                        {
//                            Subject = subjectGroup.Key,
//                            Equipments = subjectGroup.ToList()
//                        })
//                        .ToList(),
//                })
//                .ToList()
//            };

//            return vm;
//        }

//        public async Task<SkillsLabEquipmentViewModel> GetSkillsLabEquipmentService()
//        {
//            // Load from DB; if empty, seed from NMC list once
//            var entities = await _context.TblMedicalSkillsLabEquipments
//                .OrderBy(e => e.DisplayOrder)
//                .ToListAsync();


//            var vm = new SkillsLabEquipmentViewModel
//            {
//                Items = entities.Select(e => new SkillsLabEquipmentItemViewModel
//                {
//                    Id = e.Id,
//                    Name = e.Name,
//                    IsRequired = e.IsRequired,
//                    IsAvailable = e.IsAvailable,
//                    Quantity = e.Quantity
//                }).ToList()
//            };

//            return vm;
//        }

//        public async Task<PhysicalFacilitiesDisplayViewModel> GetLandClassEquipmentService()
//        {
//            var facultyId = _userContext.FacultyId;
//            var collegeCode = _userContext.CollegeCode;
//            var smg = await _context.SmallGroupTeachings
//                .AsNoTracking()
//                .Where(x => x.FacultyCode == facultyId.ToString())
//                .ToListAsync();

//            var teaching = smg.Select(x => new SmallGroupTeachingDisplayViewModel
//                {
//                    AnnualMbbsIntake = x.AnnualMbbsIntake,
//                    SmallGroupBatchSize = x.SmallGroupBatchSize,
//                    TeachingAreasSharedAllDepts = x.TeachingAreasSharedAllDepts,
//                    AvInAllTeachingAreas = x.AvInAllTeachingAreas,
//                    InternetInAllTeachingAreas = x.InternetInAllTeachingAreas,
//                    DigitalLinkAllTeachingAreas = x.DigitalLinkAllTeachingAreas,
//                    SmallGroupStudents = x.SmallGroupStudents,
//                    RequiredAreaSqm = x.RequiredAreaSqm,
//                    AvailableAreaSqm = x.AvailableAreaSqm,
//                    AreaDeficiencySqm = x.AreaDeficiencySqm,
//                    RoomsSharedByAllDepts = x.RoomsSharedByAllDepts,
//                    AppropriateAreaEachSpecialty = x.AppropriateAreaEachSpecialty,
//                    ConnectedToLectureHalls = x.ConnectedToLectureHalls,
//                    InternetInTeachingRooms = x.InternetInTeachingRooms,

//                }).FirstOrDefault();

//            var studentLabs = await _context.MedicalStudentPracticalLabs
//                .Select(x => new SmallGroupStudentLabsDisplayViewModel
//                {
//                    HistologyAvailable = x.HistologyAvailable,
//                    HistologyShared = x.HistologyShared,
//                    ClinicalPhysiologyAvailable = x.ClinicalPhysiologyAvailable,
//                    ClinicalPhysiologyShared = x.ClinicalPhysiologyShared,
//                    BiochemistryAvailable = x.BiochemistryAvailable,
//                    BiochemistryShared = x.BiochemistryShared,
//                    HistopathCytopathAvailable = x.HistopathCytopathAvailable,
//                    HistopathCytopathShared = x.HistopathCytopathShared,
//                    ClinPathHemeAvailable = x.ClinPathHemeAvailable,
//                    ClinPathHemeShared = x.ClinPathHemeShared,
//                    MicrobiologyAvailable = x.MicrobiologyAvailable,
//                    MicrobiologyShared = x.MicrobiologyShared,
//                    ClinicalPharmAvailable = x.ClinicalPharmAvailable,
//                    ClinicalPharmShared = x.ClinicalPharmShared,
//                    CalPharmAvailable = x.CalPharmAvailable,
//                    CalPharmShared = x.CalPharmShared,
//                    AllLabsHaveAV = x.AllLabsHaveAv,
//                    AllLabsHaveInternet = x.AllLabsHaveInternet,
//                    TechnicalStaffFacilitiesEnsured = x.TechnicalStaffFacilitiesEnsured,
//                }).FirstOrDefaultAsync();

//            var museums = await _context.MedicalMuseums
//                .AsNoTracking()
//                .Where(x => x.FacultyCode == facultyId.ToString())
//                .Select(x => new SmallGroupMuseumsDisplayViewModel
//                {
//                    SeparateAnatomyMuseumAvailable = x.SeparateAnatomyMuseumAvailable,
//                    PathologyForensicSharedMuseum = x.PathologyForensicSharedMuseum,
//                    PharmMicroCommSharedMuseum = x.PharmMicroCommSharedMuseum,
//                    SeatingCapacityPerMuseum = x.SeatingCapacityPerMuseum,
//                    SeatingAreaAvailableSqm = x.SeatingAreaAvailableSqm,
//                    SeatingAreaRequiredSqm = x.SeatingAreaRequiredSqm,
//                    SeatingAreaDeficiencySqm = x.SeatingAreaDeficiencySqm,
//                    MuseumsHaveAV = x.MuseumsHaveAv,
//                    MuseumsHaveInternet = x.MuseumsHaveInternet,
//                    MuseumsDigitallyLinked = x.MuseumsDigitallyLinked,
//                    MuseumsHaveRacksShelves = x.MuseumsHaveRacksShelves,
//                    MuseumsHaveRadiologyDisplay = x.MuseumsHaveRadiologyDisplay,
//                    TeachingTimeSharingProgrammed = x.TeachingTimeSharingProgrammed,
//                }).FirstOrDefaultAsync();

//            var skillslab = await GetSkillsLabService();
//            var deptsMeu = await GetDepartmentOfficesMeu();
//            var EquipmentDetails = await GetLaboratoryEquipmentService();
//            var SkillsLabEquipment = await GetSkillsLabEquipmentService();


//            return new PhysicalFacilitiesDisplayViewModel
//            {
//                CollegeCode = collegeCode,
//                FacultyCode = facultyId,
//                SmallGroupTeaching = teaching,
//                SmallGroupStudentLabs = studentLabs,
//                SmallGroupMuseums = museums,
//                SkillsLab = skillslab,
//                DeptOfficeMeu = deptsMeu,
//                LaboratoryEquipment = EquipmentDetails,
//                SkillsLabEquipment = SkillsLabEquipment,
//            };
//        }


//    }

//}
