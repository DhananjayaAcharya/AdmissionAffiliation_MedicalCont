
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CALibraryService : ICALibraryService
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CALibraryService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<MedicalLibraryDisplayViewModel> GetLibraryAsync()
        {
            string collegeCode = _userContext.CollegeCode;
            int facultyCode = _userContext.FacultyId;
            int affiliationType = await ResolveLibraryAffiliationTypeAsync();
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();

            var model = new MedicalLibraryDisplayViewModel
            {
                CollegeCode = collegeCode,
                //FacultyCode = facultyCode,
                //AffiliationType = affiliationType
                caAffMedicalLibraryvm = new CA_Aff_MedicalLibraryViewModel1(),
                librarayCommitteeVM = new CaMedLibCommitteeListDisplayViewModel()
            };

            // ===================== 1. LIBRARY SERVICES =====================
            var savedServices = _context.CaMedicalLibraryServices
                .AsNoTracking()
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(x => x.LibraryServiceId)
                .ToList();

            var masterServices = _context.CaMstMediLibraryServices
                .OrderBy(s => s.ServiceId)
                .ToList();

            model.caAffMedicalLibraryvm.LibraryServices = masterServices.Select(m =>
            {
                var saved = savedServices.FirstOrDefault(s => s.ServiceId == m.ServiceId);
                return new LibraryServiceRowViewModel1
                {
                    ServiceId = m.ServiceId,
                    IsAvailable = saved?.IsAvailable,
                    ServiceName = m.ServiceName,

                    ExistingFileName = saved?.UploadedFileName,
                    HasPdf = saved?.UploadedPdfPath != null,
                    LibraryServiceId = saved?.LibraryServiceId ?? 0

                };
            }).ToList();

            // ===================== 2. USAGE REPORT =====================
            var usage = _context.CaMedicalLibraryUsageReports
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(x => x.UsageReportId)
                .FirstOrDefault();

            if (usage != null)
            {
                model.caAffMedicalLibraryvm.ExistingUsageReportFileName = usage.UploadedFileName;
                model.caAffMedicalLibraryvm.UsageReportId = usage.UsageReportId;
            }

            // ===================== 3. LIBRARY STAFF =====================
            var savedStaff = _context.CaMedicalLibraryStaffs
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(x => x.Id)
                .ToList();

            model.caAffMedicalLibraryvm.LibraryStaff = savedStaff.Select(s => new LibraryStaffViewModel1
            {
                Id = s.Id,
                StaffName = s.StaffName,
                Designation = s.Designation,
                Qualification = s.Qualification,
                Experience = s.Experience,
                Category = s.Category
            }).ToList();

            // ===================== 4. DEPARTMENTAL LIBRARY (FIXED) =====================
            //var savedDepartments = _context.CaMedicalDepartmentLibraries
            //    .Where(x => x.CollegeCode == "M404" &&
            //                x.FacultyCode == facultyCode &&
            //                x.AffiliationType == affiliationType)
            //    .ToList();


            var savedDepartmentCandidates = (from cmdl in _context.CaMedicalDepartmentLibraries.AsNoTracking()
                                             join deptMaster in _context.DepartmentMasters.AsNoTracking()
                                                 on new { cmdl.DepartmentCode, FacultyCode = cmdl.FacultyCode }
                                                 equals new { deptMaster.DepartmentCode, deptMaster.FacultyCode }
                                             where cmdl.CollegeCode == collegeCode &&
                                                 cmdl.FacultyCode == facultyCode &&
                                                 (string.IsNullOrEmpty(cmdl.CourseLevel) || cmdl.CourseLevel.Trim().ToUpper() == courseLevel) &&
                                                 cmdl.AffiliationType == affiliationType
                                             orderby cmdl.CourseLevel == courseLevel descending, cmdl.DepartmentalLibraryId descending
                                             select new { cmdl, deptMaster })
                .ToList();
            var savedDepartmentList = savedDepartmentCandidates
                .GroupBy(x => x.cmdl.DepartmentCode)
                .Select(group => group.First())
                .ToList();

            // If data exists → load only saved rows
            if (savedDepartmentList.Any())
            {
                model.caAffMedicalLibraryvm.DepartmentLibraries = savedDepartmentList.Select(s =>
                {
                    string staff1 = "";
                    string staff2 = "";

                    if (!string.IsNullOrWhiteSpace(s.cmdl.LibraryStaff))
                    {
                        var parts = s.cmdl.LibraryStaff.Split('|', StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length > 0)
                            staff1 = parts[0].Trim();

                        if (parts.Length > 1)
                            staff2 = parts[1].Trim();
                    }

                    return new DepartmentLibraryViewModel1
                    {
                        DepartmentCode = s.cmdl.DepartmentCode,
                        TotalBooks = s.cmdl.TotalBooks,
                        BooksAddedInYear = s.cmdl.BooksAddedInYear,
                        CurrentJournals = s.cmdl.CurrentJournals,
                        LibraryStaff1 = staff1,
                        LibraryStaff2 = staff2,
                        DepartmentName = s.deptMaster.DepartmentName
                    };
                }).ToList();

            }


            // ===================== 5. OTHER DETAILS =====================
            var otherDetails = _context.CaMedicalLibraryOtherDetails
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            (string.IsNullOrEmpty(x.CourseLevel) || x.CourseLevel.Trim().ToUpper() == courseLevel) &&
                            x.AffiliationType == affiliationType)
                .OrderByDescending(x => x.CourseLevel != null && x.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(x => x.DigitalValuationId)
                .FirstOrDefault();

            if (otherDetails != null)
            {
                model.caAffMedicalLibraryvm.OtherDetails = new MedicalLibraryOtherDetailsViewModel1
                {
                    DigitalValuationId = otherDetails.DigitalValuationId,
                    HasDigitalValuationCentre = otherDetails.HasDigitalValuationCentre,
                    NoOfSystems = otherDetails.NoOfSystems,
                    HasStableInternet = otherDetails.HasStableInternet,
                    HasCccameraSystem = otherDetails.HasCccameraSystem,
                    UploadedFileName = otherDetails.UploadedFileName,
                    SpecialFeaturesQuestion = otherDetails.SpecialFeaturesAchievementsPdfPath != null ? "Yes" : "No",
                    HasSpecialFeaturesPdf = otherDetails.SpecialFeaturesAchievementsPdfPath != null,
                    CreatedDate = otherDetails.CreatedDate,
                    HasSpecialFeatures = !string.IsNullOrWhiteSpace(otherDetails.SpecialFeaturesAchievementsPdfPath)
                };

            }


            bool hasLibraryServicePdf = model.caAffMedicalLibraryvm.LibraryServices.Any(s => !string.IsNullOrEmpty(s.ExistingFileName));

            bool hasUsageReportPdf =
                !string.IsNullOrEmpty(model.caAffMedicalLibraryvm.ExistingUsageReportFileName);

            bool hasSpecialFeaturesPdf =
                model.caAffMedicalLibraryvm.OtherDetails?.HasSpecialFeaturesPdf == true;

            model.caAffMedicalLibraryvm.IsFirstLogin = !(hasLibraryServicePdf || hasUsageReportPdf || hasSpecialFeaturesPdf);
            model.librarayCommitteeVM = await GetLibCommittee();
            model.LibraryGeneralVM = await GetLibraryGeneral();
            model.LibraryItemListVM = await GetLibraryItems();
            model.LibraryBuildingVM = await GetLibraryBuilding();
            model.LibraryTechListVM = await GetLibraryTechnicalProcess();
            model.LibraryFinancVM = await GetLibraryFinance();
            model.LibraryEquipmentListVM = await GetLibraryEquipment();
            model.ResearchPublicationsDisplayViewModel = await GetResearchPublications();
            return model;
        }

        private async Task<int> ResolveLibraryAffiliationTypeAsync()
        {
            var affiliationName = _httpContextAccessor.HttpContext?.Session.GetString("TypeOfAffiliation");
            if (string.IsNullOrWhiteSpace(affiliationName))
                return _userContext.TypeOfAffiliation;

            var normalizedName = affiliationName.Trim().ToUpperInvariant();
            var libraryTypeId = await _context.TypeOfAffiliations
                .AsNoTracking()
                .Where(type => type.TypeDescription.Trim().ToUpper() == normalizedName)
                .OrderBy(type => type.TypeId)
                .Select(type => (int?)type.TypeId)
                .FirstOrDefaultAsync();

            return libraryTypeId ?? _userContext.TypeOfAffiliation;
        }

        public async Task<CaMedLibCommitteeListDisplayViewModel> GetLibCommittee()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();
            var libCommitteList = await (from det in _context.CaMedLibCommittees
                                         join cmst in _context.CaMstMedCommitteeNames
                                         on det.CommitteeId equals cmst.Id
                                         where det.CollegeCode == collegeCode &&
                                               det.FacultyCode == facultyCode.ToString() &&
                                               (string.IsNullOrEmpty(det.CourseLevel) || det.CourseLevel.Trim().ToUpper() == courseLevel)
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
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();

            var libGen = await _context.CaMedLibraryGenerals
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode &&
                            e.FacultyCode == facultyCode.ToString() &&
                            e.CourseLevel == courseLevel)
                .OrderByDescending(e => e.SlNo)
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
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();
            var libItemList = await (from det in _context.CaMedLibraryItems
                                     join mst in _context.CaMstMedLibraryItems
                                    on det.ItemName equals mst.ItemName
                                     where det.CollegeCode == collegeCode &&
                                           det.FacultyCode == facultyCode.ToString() &&
                                           det.CourseLevel == courseLevel
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
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();

            var libBuildingDetails = await _context.CaMedLibraryBuildings
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode &&
                            e.FacultyCode == facultyCode.ToString() &&
                            (string.IsNullOrEmpty(e.CourseLevel) || e.CourseLevel.Trim().ToUpper() == courseLevel))
                .OrderByDescending(e => e.CourseLevel != null && e.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenByDescending(e => e.SlNo)
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
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();

            var techProcessList = await _context.CaMedLibTechnicalProcesses
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode &&
                            e.FacultyCode == facultyCode.ToString() &&
                            e.CourseLevel == courseLevel)
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
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();
            var libraryFinanceDetails = await _context.CaMedLibraryFinances
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode &&
                            e.FacultyCode == facultyCode.ToString() &&
                            e.CourseLevel == courseLevel)
                .OrderByDescending(e => e.SlNo)
                .Select(e => new CaMedLibraryFinanceDisplayViewModel
                {
                    CollegeCode = e.CollegeCode,
                    FacultyCode = e.FacultyCode,
                    TotalBudgetLakhs = e.TotalBudgetLakhs,
                    ExpenditureBooksLakhs = e.ExpenditureBooksLakhs
                }).FirstOrDefaultAsync();

            return libraryFinanceDetails;

        }

        public async Task<CaMedResearchPublicationsDisplayViewModel> GetResearchPublications()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var data = await _context.CaMedResearchPublicationsDetails
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
                .OrderByDescending(e => e.CourseLevel != null && e.CourseLevel.Trim().ToUpper() == "ALL")
                .ThenByDescending(e => e.SlNo)
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
            var courseLevel = _userContext.CourseLevel.Trim().ToUpperInvariant();

            var savedEquipment = await _context.CaMedLibraryEquipments
                .AsNoTracking()
                .Where(e => e.CollegeCode == collegeCode &&
                            e.FacultyCode == facultyCode.ToString())
                .OrderByDescending(e => e.CourseLevel != null &&
                                        e.CourseLevel.Trim().ToUpper() == courseLevel)
                .ThenBy(e => e.SlNo)
                .ToListAsync();

            var equipmentList = savedEquipment
                .GroupBy(e => e.SlNo)
                .Select(group => group.First())
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
                .ToList();

            return new CaMedLibraryEquipmentListDisplayViewModel
            {
                CollegeCode = collegeCode,
                Items = equipmentList
            };
        }


    }
}
