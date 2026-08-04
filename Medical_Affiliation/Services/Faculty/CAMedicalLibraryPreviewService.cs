using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAMedicalLibraryPreviewService
        : CAPreviewServiceBase, ICAMedicalLibraryPreviewService
    {
        public CAMedicalLibraryPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
            : base(context, userContext)
        {
        }

        public async Task<MedicalLibraryPreviewVM> GetMedicalLibraryPreviewAsync()
        {
            var vm = new MedicalLibraryPreviewVM();

            

            // ============================================================
            // USAGE REPORT
            // ============================================================

            vm.HasUsageReport = await _context.CaMedicalLibraryUsageReports
                .AnyAsync(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) ||
                     x.CourseLevel == CourseLevel));

            vm.UsageReportViewController = "CollegeAssessment";
            vm.UsageReportViewAction = "ViewLibraryUsageReport";

            // ============================================================
            // LIBRARY STAFF
            // ============================================================

            vm.LibraryStaff = await _context.CaMedicalLibraryStaffs
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) ||
                     x.CourseLevel == CourseLevel))

                .OrderBy(x => x.StaffName)

                .Select(x => new LibraryStaffPreviewVM
                {
                    StaffName = x.StaffName,
                    Designation = x.Designation,
                    Qualification = x.Qualification,
                    Experience = x.Experience,
                    Category = x.Category
                })

                .ToListAsync();

            // ============================================================
            // DEPARTMENT LIBRARIES
            // ============================================================

            vm.DepartmentLibraries = await (
                from dept in _context.DepartmentMasters

                join saved in _context.CaMedicalDepartmentLibraries
                    .Where(x =>
                        x.CollegeCode == CollegeCode &&
                        x.FacultyCode == FacultyCode &&
                        (string.IsNullOrEmpty(x.CourseLevel) ||
                         x.CourseLevel == CourseLevel))

                on dept.DepartmentCode equals saved.DepartmentCode into grp

                from saved in grp.DefaultIfEmpty()

                where dept.FacultyCode == FacultyCode

                orderby dept.DepartmentCode

                select new DepartmentLibraryPreviewVM
                {
                    DepartmentCode = dept.DepartmentCode,
                    DepartmentName = dept.DepartmentName,

                    TotalBooks = saved != null ? saved.TotalBooks : null,
                    BooksAddedInYear = saved != null ? saved.BooksAddedInYear : null,
                    CurrentJournals = saved != null ? saved.CurrentJournals : null,

                    LibraryStaff = saved != null
                        ? saved.LibraryStaff
                        : null,

                    Titles = saved != null ? saved.Titles : null,
                    InternationalJournals = saved != null ? saved.InternationalJournals : null,
                    BackVolumes = saved != null ? saved.BackVolumes : null,
                    PrintJournalPercentage = saved != null ? saved.PrintJournalPercentage : null
                }

            ).ToListAsync();

            // ============================================================
            // OTHER DETAILS
            // ============================================================

            var other = await _context.CaMedicalLibraryOtherDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) ||
                     x.CourseLevel == CourseLevel));

            if (other != null)
            {
                vm.OtherDetails = new MedicalLibraryOtherPreviewVM
                {
                    HasDigitalValuationCentre = other.HasDigitalValuationCentre,
                    NoOfSystems = other.NoOfSystems,
                    HasStableInternet = other.HasStableInternet,
                    HasCccameraSystem = other.HasCccameraSystem,

                    HasSpecialFeatures =
                        !string.IsNullOrWhiteSpace(other.SpecialFeaturesAchievementsPdfPath),

                    ViewController = "CollegeAssessment",
                    ViewAction = "ViewSpecialFeaturesPdf"
                };
            }

            // ============================================================
            // DENTAL LIBRARY RECORDS
            // ============================================================

            if (FacultyCode == 2)
            {
                vm.DentalLibraryRecords = await (

                    from master in _context.CaMstDentalLibraryRecords

                    join uploaded in _context.CaDentalLibraryRecords
                        .Where(x =>
                            x.CollegeCode == CollegeCode &&
                            x.FacultyCode == FacultyCode)

                    on master.RecordId equals uploaded.RecordId into grp

                    from uploaded in grp.DefaultIfEmpty()

                    orderby master.DisplayOrder

                    select new DentalLibraryRecordPreviewVM
                    {
                        RecordId = master.RecordId,
                        RecordName = master.RecordName,

                        HasDocument =
                            uploaded != null &&
                            !string.IsNullOrWhiteSpace(uploaded.FileName),

                        ViewController = "CollegeAssessment",
                        ViewAction = "ViewDentalLibraryRecord"
                    }

                ).ToListAsync();
            }

            // ============================================================
            // RESEARCH PUBLICATIONS
            // ============================================================

            var research = new ResearchPublicationsPreviewVM();

            var publication = await _context.CaMedResearchPublicationsDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString() &&
                    (x.CourseLevel == "ALL" ||
                     string.IsNullOrEmpty(x.CourseLevel)));

            if (publication != null)
            {
                research.PublicationsNo = publication.PublicationsNo ?? 0;

                research.HasPublicationsPdf =
                    !string.IsNullOrWhiteSpace(publication.PublicationsPdfPath);

                research.HasClinicalTrialsPdf =
                    !string.IsNullOrWhiteSpace(publication.ClinicalTrialsPdfPath);

                research.StudentsRGUHSFunded =
                    publication.StudentsRguhsfunded;

                research.StudentsExternalBodyFunding =
                    publication.StudentsExternalBodyFunding;

                research.HasStudentProjectsPdf =
                    !string.IsNullOrWhiteSpace(publication.StudentsProjectsPdfPath);

                research.FacultyRGUHSFunded =
                    publication.FacultyRguhsfunded;

                research.FacultyExternalBodyFunding =
                    publication.FacultyExternalBodyFunding;

                research.HasFacultyProjectsPdf =
                    !string.IsNullOrWhiteSpace(publication.FacultyProjectsPdfPath);
            }

            research.OtherActivities = await
            (
                from activity in _context.CaMedLibOtherAcademicActivities

                join master in _context.CaMstMedOtherAcademicActivities
                    on activity.ActivityId equals master.Id

                join dept in _context.DepartmentMasters
                    on activity.DepartmentCode equals dept.DepartmentCode
                    into deptGroup

                from dept in deptGroup.DefaultIfEmpty()

                where activity.CollegeCode == CollegeCode &&
                        activity.FacultyCode == FacultyCode.ToString()

                orderby master.ActivityName

                select new OtherAcademicActivityPreviewVM
                {
                    ActivityId = activity.ActivityId,
                    ActivityName = master.ActivityName,
                    DepartmentCode = activity.DepartmentCode,
                    DepartmentName = dept != null ? dept.DepartmentName : "",
                    DepartmentWise = activity.DepartmentWise,
                    HasDocument =
                        !string.IsNullOrWhiteSpace(activity.ActivityPdfPath)
                }

            ).ToListAsync();

            research.Committees = await
            (
                from master in _context.CaMstMedCommitteeNames

                join committee in _context.CaMedLibCommittees
                    .Where(x =>
                        x.CollegeCode == CollegeCode &&
                        x.FacultyCode == FacultyCode.ToString())

                on master.Id equals committee.CommitteeId into grp

                from committee in grp.DefaultIfEmpty()

                where master.FacultyCode == FacultyCode.ToString()

                orderby master.CommitteeName

                select new ResearchCommitteePreviewVM
                {
                    CommitteeId = master.Id,
                    CommitteeName = master.CommitteeName,
                    IsPresent = committee != null
                        ? committee.IsPresent
                        : null,

                    HasDocument =
                        committee != null &&
                        !string.IsNullOrWhiteSpace(committee.CommitteePdfPath)
                }

            ).ToListAsync();

            research.DepartmentPublications = await
                (
                    from dept in _context.DepartmentMasters

                    join deptPublication in _context.DeptWisePublications
                        .Where(x =>
                            x.CollegeCode == CollegeCode &&
                            x.FacultyCode == FacultyCode)

                    on dept.DepartmentCode equals deptPublication.DeptCode
                        into grp

                    from deptPublication in grp.DefaultIfEmpty()

                    where dept.FacultyCode == FacultyCode

                    orderby dept.DepartmentName

                    select new DepartmentPublicationPreviewVM
                    {

                        Id = deptPublication.Id,

                        DepartmentCode = dept.DepartmentCode,
                        DepartmentName = dept.DepartmentName,

                        PublicationsCount =
                            deptPublication != null
                                ? deptPublication.PublicationsCount
                                : 0,

                        HasDocument =
                            deptPublication != null &&
                            !string.IsNullOrWhiteSpace(deptPublication.PublicationPath)
                    }

                ).ToListAsync();

            vm.ResearchPublications = research;

            // ============================================================
            // LIBRARY INFORMATION
            // ============================================================

            var libraryInfo = new LibraryInformationPreviewVM();

            //
            // GENERAL
            //
            var general = await _context.CaMedLibraryGenerals
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.CourseLevel)
                .FirstOrDefaultAsync();

            if (general != null)
            {
                libraryInfo.General = new LibraryGeneralPreviewVM
                {
                    LibraryEmailId = general.LibraryEmailId,
                    DigitalLibrary = general.DigitalLibrary,
                    HelinetServices = general.HelinetServices,
                    DepartmentWiseLibrary = general.DepartmentWiseLibrary
                };
            }

            //
            // LIBRARY ITEMS
            //
            var itemMasters = await _context.CaMstMedLibraryItems
                .Where(x => x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedItems = await _context.CaMedLibraryItems
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString())
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            libraryInfo.Items = itemMasters
                .Select(master =>
                {
                    var saved = savedItems.FirstOrDefault(x => x.SlNo == master.SlNo);

                    return new LibraryItemPreviewVM
                    {
                        SlNo = master.SlNo,
                        ItemName = master.ItemName,

                        CurrentForeign = saved?.CurrentForeign ?? 0,
                        CurrentIndian = saved?.CurrentIndian ?? 0,
                        PreviousForeign = saved?.PreviousForeign ?? 0,
                        PreviousIndian = saved?.PreviousIndian ?? 0
                    };
                })
                .ToList();

            //
            // BUILDING
            //
            var building = await _context.CaMedLibraryBuildings
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString());

            if (building != null)
            {
                libraryInfo.Building = new LibraryBuildingPreviewVM
                {
                    IsIndependent = building.IsIndependent,
                    AreaSqMtrs = building.AreaSqMtrs
                };
            }

            //
            // TECHNICAL PROCESS
            //
            var techMasters = await _context.CaMstMedLibTechnicalProcesses
                .Where(x => x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedTech = await _context.CaMedLibTechnicalProcesses
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString())
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            libraryInfo.TechnicalProcesses = techMasters
                .Select(master =>
                {
                    var saved = savedTech.FirstOrDefault(x => x.SlNo == master.SlNo);

                    return new LibraryTechnicalProcessPreviewVM
                    {
                        SlNo = master.SlNo,
                        ProcessName = master.ProcessName,
                        Value = saved?.Value
                    };
                })
                .ToList();

            //
            // EQUIPMENTS
            //
            var equipmentMasters = await _context.CaMstMedLibraryEquipments
                .Where(x => x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedEquipments = await _context.CaMedLibraryEquipments
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString())
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            libraryInfo.Equipments = equipmentMasters
                .Select(master =>
                {
                    var saved = savedEquipments.FirstOrDefault(x => x.SlNo == master.SlNo);

                    return new LibraryEquipmentPreviewVM
                    {
                        SlNo = master.SlNo,
                        EquipmentName = master.EquipmentName,
                        HasEquipment = saved?.HasEquipment
                    };
                })
                .ToList();

            libraryInfo.BinderyValue = savedEquipments
                .FirstOrDefault(x => x.EquipmentName == "Bindery")
                ?.HasEquipment;

            //
            // FINANCE
            //
            var finance = await _context.CaMedLibraryFinances
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.CourseLevel)
                .FirstOrDefaultAsync();

            if (finance != null)
            {
                libraryInfo.Finance = new LibraryFinancePreviewVM
                {
                    TotalBudgetLakhs = finance.TotalBudgetLakhs,
                    ExpenditureBooksLakhs = finance.ExpenditureBooksLakhs
                };
            }

            vm.LibraryInformation = libraryInfo;

            return vm;
        }
    }
}