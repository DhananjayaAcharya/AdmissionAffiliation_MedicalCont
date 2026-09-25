
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalLibraryService : ICADentalLibraryService
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADentalLibraryService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<DentalLibraryPreviewVM> GetLibraryAsync()
        {
            string collegeCode = _userContext.CollegeCode;
            int facultyCode = _userContext.FacultyId;
            int affiliationType = _userContext.TypeOfAffiliation;
            string? courseLevel = _userContext.CourseLevel;

            var model = new DentalLibraryPreviewVM
            {
                facultyCode = facultyCode
            };

            var general = await _context.CaMedLibraryGenerals
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode.ToString())
                .OrderBy(x => x.CourseLevel)
                .FirstOrDefaultAsync();

            var itemsMaster = await _context.CaMstMedLibraryItems
                .Where(x => x.FacultyCode == facultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedItems = await _context.CaMedLibraryItems
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode.ToString())
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            var building = await _context.CaMedLibraryBuildings
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString());

            var techMaster = await _context.CaMstMedLibTechnicalProcesses
                .Where(x => x.FacultyCode == facultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedTech = await _context.CaMedLibTechnicalProcesses
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode.ToString())
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            var equipMaster = await _context.CaMstMedLibraryEquipments
                .Where(x => x.FacultyCode == facultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedEquip = await _context.CaMedLibraryEquipments
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode.ToString())
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            var finance = await _context.CaMedLibraryFinances
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode.ToString())
                .OrderBy(x => x.CourseLevel)
                .FirstOrDefaultAsync();

            var bindery = savedEquip
                .FirstOrDefault(x => x.EquipmentName == "Bindery");

            model.LibraryInformation = new LibraryInformationPreviewVM
            {
                General = new LibraryGeneralPreviewVM
                {
                    LibraryEmailId = general?.LibraryEmailId,
                    DigitalLibrary = general?.DigitalLibrary,
                    HelinetServices = general?.HelinetServices,
                    DepartmentWiseLibrary = general?.DepartmentWiseLibrary
                },

                Items = itemsMaster.Select(m =>
                {
                    var saved = savedItems.FirstOrDefault(x => x.SlNo == m.SlNo);

                    return new LibraryItemPreviewVM
                    {
                        SlNo = m.SlNo,
                        ItemName = m.ItemName,
                        CurrentForeign = saved?.CurrentForeign ?? 0,
                        CurrentIndian = saved?.CurrentIndian ?? 0,
                        PreviousForeign = saved?.PreviousForeign ?? 0,
                        PreviousIndian = saved?.PreviousIndian ?? 0
                    };
                }).ToList(),

                Building = new LibraryBuildingPreviewVM
                {
                    IsIndependent = building?.IsIndependent,
                    AreaSqMtrs = building?.AreaSqMtrs
                },

                TechnicalProcesses = techMaster.Select(m =>
                {
                    var saved = savedTech.FirstOrDefault(x => x.SlNo == m.SlNo);

                    return new LibraryTechnicalProcessPreviewVM
                    {
                        SlNo = m.SlNo,
                        ProcessName = m.ProcessName,
                        Value = saved?.Value
                    };
                }).ToList(),

                Equipments = equipMaster.Select(m =>
                {
                    var saved = savedEquip.FirstOrDefault(x => x.SlNo == m.SlNo);

                    return new LibraryEquipmentPreviewVM
                    {
                        SlNo = m.SlNo,
                        EquipmentName = m.EquipmentName,
                        HasEquipment = saved?.HasEquipment
                    };
                }).ToList(),

                Finance = new LibraryFinancePreviewVM
                {
                    TotalBudgetLakhs = finance?.TotalBudgetLakhs,
                    ExpenditureBooksLakhs = finance?.ExpenditureBooksLakhs
                },

                BinderyValue = bindery?.HasEquipment
            };

            // ============================================================
            // 1. USAGE REPORT
            // ============================================================

            //var usageReport = await _context.CaMedicalLibraryUsageReports
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(x =>
            //        x.CollegeCode == collegeCode &&
            //        x.CourseLevel == courseLevel &&
            //        x.FacultyCode == facultyCode &&
            //        x.AffiliationType == affiliationType);

            //if (usageReport != null &&
            //    !string.IsNullOrWhiteSpace(usageReport.UploadedFileName))
            //{
            //    model.HasUsageReport = true;

            //    // Use the controller/action which is already handling
            //    // the medical library usage report.
            //    model.UsageReportViewController = "CA_Aff_MedicalLibrary";
            //    model.UsageReportViewAction = "ViewUsageReport";
            //}


            // ============================================================
            // 2. DEPARTMENT LIBRARIES
            // ============================================================

            var savedDepartmentList = await (
                from cmdl in _context.CaMedicalDepartmentLibraries
                join deptMaster in _context.DepartmentMasters
                    on cmdl.DepartmentCode equals deptMaster.DepartmentCode
                where cmdl.CollegeCode == collegeCode
                      && cmdl.FacultyCode == facultyCode
                      && cmdl.CourseLevel == courseLevel
                      && cmdl.AffiliationType == affiliationType
                select new
                {
                    cmdl,
                    deptMaster
                })
                .AsNoTracking()
                .ToListAsync();

            model.DepartmentLibraries = savedDepartmentList
                .Select(s =>
                {
                    var staffParts = (s.cmdl.LibraryStaff ?? string.Empty)
                        .Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray();

                    return new DepartmentLibraryPreviewVM
                    {
                        DepartmentCode = s.cmdl.DepartmentCode,
                        DepartmentName = s.deptMaster.DepartmentName,

                        TotalBooks = s.cmdl.TotalBooks,
                        BooksAddedInYear = s.cmdl.BooksAddedInYear,
                        CurrentJournals = s.cmdl.CurrentJournals,

                        LibraryStaff1 = staffParts.Length > 0
                            ? staffParts[0]
                            : null,

                        LibraryStaff2 = staffParts.Length > 1
                            ? staffParts[1]
                            : null,

                        Titles = s.cmdl.Titles,
                        InternationalJournals = s.cmdl.InternationalJournals,
                        BackVolumes = s.cmdl.BackVolumes,
                        PrintJournalPercentage = s.cmdl.PrintJournalPercentage
                    };
                })
                .ToList();



            // ============================================================
            // 3. DENTAL LIBRARY RECORDS
            // ============================================================

            model.DentalLibraryRecords = await (
                from record in _context.CaDentalLibraryRecords
                join master in _context.CaMstDentalLibraryRecords
                    on record.RecordId equals master.RecordId
                where record.CollegeCode == collegeCode
                      && record.FacultyCode == facultyCode
                      && record.CourseLevel == courseLevel
                      && record.AffiliationType == affiliationType
                orderby master.DisplayOrder
                select new DentalLibraryRecordPreviewVM
                {
                    RecordId = record.RecordId,

                    // Name comes from master table
                    RecordName = master.RecordName,

                    // FilePath is the actual uploaded document path
                    HasDocument = !string.IsNullOrEmpty(record.FilePath),

                    ViewController = "CA_Aff_MedicalLibrary",
                    ViewAction = "ViewDentalLibraryRecord"
                })
                .AsNoTracking()
                .ToListAsync();


            var mainDataList = await _context.CaMedResearchPublicationsDetails
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString())
                .ToListAsync();

            var commonData = mainDataList
                .FirstOrDefault(x =>
                    x.CourseLevel != null &&
                    x.CourseLevel.Trim().ToUpper() == "ALL")
                ?? mainDataList.FirstOrDefault();


            var savedDeptPublications = await _context.DeptWisePublications
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .ToListAsync();

            var savedDeptResearchProjects = await _context.DepartmentWiseResearchProjects
                .Where(x => x.CollegeCode == collegeCode &&
                            x.CourseLevel == courseLevel &&
                            x.TypeId == affiliationType &&
                            x.FacultyCode == facultyCode)
                .ToListAsync();


            var departments = await _context.DepartmentMasters
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            model.ResearchPublications = commonData == null
                ? null
                : new ResearchPublicationsPreviewVM
                {
                    // ================= Publications =================
                    PublicationsNo = commonData.PublicationsNo ?? 0,

                    HasPublicationsPdf =
                        !string.IsNullOrWhiteSpace(commonData.PublicationsPdfPath),

                    // ================= Clinical Trials =================
                    HasClinicalTrialsPdf =
                        !string.IsNullOrWhiteSpace(commonData.ClinicalTrialsPdfPath),

                    // ================= Student Projects =================
                    StudentsRGUHSFunded =
                        commonData.StudentsRguhsfunded,

                    StudentsExternalBodyFunding =
                        commonData.StudentsExternalBodyFunding,

                    HasStudentProjectsPdf =
                        !string.IsNullOrWhiteSpace(commonData.StudentsProjectsPdfPath),

                    // ================= Faculty Projects =================
                    FacultyRGUHSFunded =
                        commonData.FacultyRguhsfunded,

                    FacultyExternalBodyFunding =
                        commonData.FacultyExternalBodyFunding,

                    HasFacultyProjectsPdf =
                        !string.IsNullOrWhiteSpace(commonData.FacultyProjectsPdfPath),

                    // ================= Department-wise Publications =================
                    DepartmentPublications = departments
                        .Select(d =>
                        {
                            var saved = savedDeptPublications
                                .FirstOrDefault(x =>
                                    x.DeptCode == d.DepartmentCode);

                            return new DepartmentPublicationPreviewVM
                            {
                                Id = saved?.Id ?? 0,

                                DepartmentCode = d.DepartmentCode,

                                DepartmentName = d.DepartmentName,

                                PublicationsCount =
                                    saved?.PublicationsCount ?? 0,

                                HasDocument =
                                    !string.IsNullOrWhiteSpace(
                                        saved?.PublicationPath)
                            };
                        })
                        .ToList(),

                    DepartmentWiseResearchProjects = departments
                    .Select(d =>
                    {
                        var saved = savedDeptResearchProjects
                            .FirstOrDefault(x =>
                                x.DepartmentCode == d.DepartmentCode);

                        return new DepartmentWiseResearchProjectPreviewVM
                        {
                            Id = saved?.Id ?? 0,

                            DepartmentCode = d.DepartmentCode,

                            DepartmentName = d.DepartmentName,

                            NoOfResearchProjectsLast3Years =
                                saved?.NoOfResearchProjectsLast3Years ?? 0,

                            PdfFilePath = saved?.PdfFilePath,

                            HasDocument =
                                !string.IsNullOrWhiteSpace(saved?.PdfFilePath)
                        };
                    })
                    .ToList(),
                };

            var masterExpenditures = await _context.MstLibraryExpenditures
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            x.FacultyId == facultyCode &&
                            x.TypeId == affiliationType)
                .OrderBy(x => x.ItemName)
                .ToListAsync();

            var savedExpenditures = await _context.LibraryExpenditures
                .AsNoTracking()
                .Where(x => x.CollegeCode == collegeCode &&
                            x.CourseLevel == courseLevel &&
                            x.IsActive)
                .ToDictionaryAsync(x => x.ItemId, x => x.ExpenditureProposed);

            var masterServices = await _context.MstDentalLibraryServices
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            x.FacultyId == facultyCode &&
                            x.TypeId == affiliationType)
                .OrderBy(x => x.ServiceName)
                .ToListAsync();

            var savedServices = await _context.DentalLibraryServices
                .AsNoTracking()
                .Where(x => x.CollegeCode == collegeCode &&
                            x.CourseLevel == courseLevel &&
                            x.IsActive)
                .ToDictionaryAsync(x => x.ServiceId, x => x.IsAvailable);


            //var otherActivities = await _context.CaMedLibOtherAcademicActivities
            //            .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString() && x.CourseLevel == courseLevel)
            //            .Join(
            //                _context.CaMstMedOtherAcademicActivities,
            //                saved => saved.ActivityId,
            //                master => master.Id,
            //                (saved, master) => new CA_Med_Lib_OtherAcademicActivitiesVM
            //                {
            //                    Id = saved.Id,
            //                    ActivityId = saved.ActivityId,
            //                    ActivityName = master.ActivityName,
            //                    DepartmentCode = saved.DepartmentCode,
            //                    DepartmentWise = saved.DepartmentWise,
            //                    ActivityPdfName = saved.ActivityPdfName
            //                })
            //            .ToListAsync();
            model.Expenditures = masterExpenditures
                .Select(item => new DentalLibraryExpenditurePreviewVM
                {
                    ItemId = item.LibraryExpenditureId,

                    ItemName = item.ItemName,

                    ExpenditureProposed =
                        savedExpenditures.GetValueOrDefault(
                            item.LibraryExpenditureId)
                })
                .ToList();

            model.Services = masterServices
                .Select(service => new DentalLibraryServicePreviewVM
                {
                    ServiceId = service.DentalLibraryServiceId,

                    ServiceName = service.ServiceName,

                    IsAvailable =
                        savedServices.GetValueOrDefault(
                            service.DentalLibraryServiceId)
                })
                .ToList();

            var libraryStaff = await _context.LibraryStaffDetails
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyCode &&
                    x.TypeId == affiliationType &&
                    x.IsActive)
                .ToListAsync();


            model.LibraryStaff = libraryStaff
                .Select(x => new DentalLibraryStaffPreviewVM
                {
                    LibraryStaffId = x.LibraryStaffId,

                    Name = x.Name,

                    Designation = x.Designation,

                    Qualification = x.Qualification,

                    ExperienceFrom = x.ExperienceFrom,

                    ExperienceTo = x.ExperienceTo,

                    PayScale = x.PayScale,

                    Category = x.Category,

                    // ExperienceTo is null => currently working
                    CurrentlyWorking = !x.ExperienceTo.HasValue
                })
                .ToList();


            var existingUsers = await _context.UserDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyCode &&
                    x.TypeId == affiliationType &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive);

            model.LibraryUsers = new DentalLibraryUserPreviewVM
            {
                NoOfTeachingStaff = existingUsers.NoOfTeachingStaff ?? 0,

                NoOfResearchScholarsAssistants = existingUsers.NoOfResearchScholarsAssistants ?? 0,

                NoOfPostGraduateStudents = existingUsers.NoOfPostGraduateStudents ?? 0,

                NoOfUnderGraduateStudents = existingUsers.NoOfUnderGraduateStudents ?? 0,

                NoOfAdministrativeStaff = existingUsers.NoOfAdministrativeStaff ?? 0,

                NoOfParaMedicalStaff = existingUsers.NoOfParaMedicalStaff ?? 0,

                NoOfOutsiders = existingUsers.NoOfOutsiders ?? 0,

                ProvideUserEducationProgrammes = existingUsers.ProvideUserEducationProgrammes.HasValue
            };


            return model;
        }

        public async Task<CaMedLibCommitteeListDisplayViewModel> GetLibCommittee()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var libCommitteList = await (from det in _context.CaMedLibCommittees
                                         join cmst in _context.CaMstMedCommitteeNames
                                         on det.CommitteeId equals cmst.Id
                                         where det.CollegeCode == collegeCode && det.FacultyCode == facultyCode.ToString()
                                            && det.CourseLevel == courseLevel
                                         orderby cmst.CommitteeName
                                         select new CaMedLibCommitteeDisplayViewModel
                                         {
                                             Id = det.Id,
                                             CommitteeId = det.CommitteeId,
                                             CommitteeName = cmst.CommitteeName,
                                             IsPresent = det.IsPresent == "Y",
                                             HasCommitteePdf = det.CommitteePdfPath != null && det.CommitteePdfPath.Length > 0,
                                             CommitteePdfName = det.CommitteePdfName,

                                         }
                                         ).ToListAsync();
            return new CaMedLibCommitteeListDisplayViewModel { Committees = libCommitteList };
        }

        public async Task<CaMedLibraryGeneralDisplayViewModel> GetLibraryGeneral()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var libGen = await _context.CaMedLibraryGenerals
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString() && e.CourseLevel == courseLevel)
                .Select(e => new CaMedLibraryGeneralDisplayViewModel
                {
                    SlNo = e.SlNo,
                    CollegeCode = e.CollegeCode,
                    FacultyCode = e.FacultyCode,
                    LibraryEmailId = e.LibraryEmailId,
                    HasDigitalLibrary = e.DigitalLibrary == "Y",
                    HasDepartmentWiseLibrary = e.DepartmentWiseLibrary == "Y",
                    HasHelinetServices = e.HelinetServices == "Y",
                })
                .FirstOrDefaultAsync();

            return libGen;

        }

        public async Task<CaMedLibraryItemListDisplayViewModel> GetLibraryItems()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;
            var libItemList = await (from det in _context.CaMedLibraryItems
                                     join mst in _context.CaMstMedLibraryItems
                                    on det.ItemName equals mst.ItemName
                                     where det.CollegeCode == collegeCode && det.FacultyCode == facultyCode.ToString() && det.CourseLevel == courseLevel
                                     select new CaMedLibraryItemDisplayViewModel
                                     {
                                         SlNo = det.SlNo,
                                         ItemName = mst.ItemName,
                                         CurrentForeign = det.CurrentForeign ?? 0,
                                         CurrentIndian = det.CurrentIndian ?? 0,
                                         PreviousForeign = det.PreviousForeign ?? 0,
                                         PreviousIndian = det.PreviousIndian ?? 0,
                                         HasIndianForeignSplit = mst.SlNo == 2
                                     }
                                     ).ToListAsync();

            return new CaMedLibraryItemListDisplayViewModel { Items = libItemList };
        }

        public async Task<CaMedLibraryBuildingDisplayViewModel> GetLibraryBuilding()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var libBuildingDetails = await _context.CaMedLibraryBuildings
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString() && e.CourseLevel == courseLevel)
                .Select(e => new CaMedLibraryBuildingDisplayViewModel
                {
                    SlNo = e.SlNo,
                    IsIndependent = e.IsIndependent == "Y",
                    AreaSqMtrs = e.AreaSqMtrs
                }).FirstOrDefaultAsync();

            return libBuildingDetails;
        }

        public async Task<CaMedLibTechnicalProcessListDisplayViewModel> GetLibraryTechnicalProcess()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var techProcessList = await _context.CaMedLibTechnicalProcesses
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString() && e.CourseLevel == courseLevel)
                .Select(e => new CaMedLibTechnicalProcessDisplayViewModel
                {
                    SlNo = e.SlNo,
                    ProcessName = e.ProcessName,
                    Value = e.Value,

                }).ToListAsync();

            return new CaMedLibTechnicalProcessListDisplayViewModel { Processes = techProcessList };
        }

        public async Task<CaMedLibraryFinanceDisplayViewModel> GetLibraryFinance()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var LibraryFinanceDetails = await _context.CaMedLibraryFinances
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString() && e.CourseLevel == courseLevel)
                .Select(e => new CaMedLibraryFinanceDisplayViewModel
                {
                    TotalBudgetLakhs = e.TotalBudgetLakhs,
                    ExpenditureBooksLakhs = e.ExpenditureBooksLakhs
                }).FirstOrDefaultAsync();

            return LibraryFinanceDetails;

        }

        public async Task<CaMedResearchPublicationsDisplayViewModel> GetResearchPublications()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var data = await _context.CaMedResearchPublicationsDetails
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString() && e.CourseLevel == courseLevel)
                .Select(e => new CaMedResearchPublicationsDisplayViewModel
                {
                    PublicationsNo = e.PublicationsNo,
                    Pi = e.Pi,

                    RguhsFunded = e.Rguhsfunded,
                    ExternalBodyFunding = e.ExternalBodyFunding,

                    HasPublicationsPdf = e.PublicationsPdfPath != null,
                    HasProjectsPdf = e.ProjectsPdfPath != null,
                    HasClinicalTrialsPdf = e.ClinicalTrialsPdfPath != null,

                    StudentsRguhsFunded = e.StudentsRguhsfunded,
                    StudentsExternalFunding = e.StudentsExternalBodyFunding,
                    HasStudentsProjectsPdf = e.StudentsProjectsPdfPath != null,

                    FacultyRguhsFunded = e.FacultyRguhsfunded,
                    FacultyExternalFunding = e.FacultyExternalBodyFunding,
                    HasFacultyProjectsPdf = e.FacultyProjectsPdfPath != null
                })
                .FirstOrDefaultAsync();

            return data;
        }

        public async Task<CaMedLibraryEquipmentListDisplayViewModel> GetLibraryEquipment()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var equipmentList = await _context.CaMedLibraryEquipments
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString() && e.CourseLevel == courseLevel)
                .Select(e => new CaMedLibraryEquipmentDisplayViewModel
                {
                    SlNo = e.SlNo,
                    CollegeCode = e.CollegeCode,
                    FacultyCode = e.FacultyCode,
                    SubFacultyCode = e.SubFacultyCode,
                    RegistrationNo = e.RegistrationNo,
                    EquipmentName = e.EquipmentName,
                    HasEquipment = e.HasEquipment == "Y"
                })
                .ToListAsync();

            return new CaMedLibraryEquipmentListDisplayViewModel
            {
                CollegeCode = collegeCode,
                Items = equipmentList
            };
        }


    }
}
