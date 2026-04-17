
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CALibraryService : ICALibraryService
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CALibraryService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<MedicalLibraryDisplayViewModel> GetLibraryAsync()
        {
            string collegeCode = _userContext.CollegeCode;
            int facultyCode = _userContext.FacultyId;
            int affiliationType = _userContext.TypeOfAffiliation;

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
                .Where(x => x.CollegeCode == "M404" &&
                            x.FacultyCode == facultyCode &&
                            x.AffiliationType == affiliationType)
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
                    LibraryServiceId = saved.LibraryServiceId

                };
            }).ToList();

            // ===================== 2. USAGE REPORT =====================
            var usage = _context.CaMedicalLibraryUsageReports
                .FirstOrDefault(x => x.CollegeCode == "M404" &&
                                     x.FacultyCode == facultyCode &&
                                     x.AffiliationType == affiliationType);

            if (usage != null)
            {
                model.caAffMedicalLibraryvm.ExistingUsageReportFileName = usage.UploadedFileName;
                model.caAffMedicalLibraryvm.UsageReportId = usage.UsageReportId;
            }

            // ===================== 3. LIBRARY STAFF =====================
            var savedStaff = _context.CaMedicalLibraryStaffs
                .Where(x => x.CollegeCode == "M404" &&
                            x.FacultyCode == facultyCode &&
                            x.AffiliationType == affiliationType)
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


            var savedDepartmentList = (from cmdl in _context.CaMedicalDepartmentLibraries
                                       join deptMaster in _context.DepartmentMasters
                                       on cmdl.DepartmentCode equals deptMaster.DepartmentCode
                                       where cmdl.CollegeCode == "M404" &&
                                           cmdl.FacultyCode == facultyCode &&
                                           cmdl.AffiliationType == affiliationType
                                       select new { cmdl, deptMaster })
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
                .FirstOrDefault(x => x.CollegeCode == "M404" &&
                                     x.FacultyCode == facultyCode &&
                                     x.AffiliationType == affiliationType);

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
                    HasSpecialFeatures = otherDetails.SpecialFeaturesQuestion == "Yes",
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

        public async Task<CaMedLibCommitteeListDisplayViewModel> GetLibCommittee()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var libCommitteList = await (from det in _context.CaMedLibCommittees
                                         join cmst in _context.CaMstMedCommitteeNames
                                         on det.CommitteeId equals cmst.Id
                                         where det.CollegeCode == collegeCode && det.FacultyCode == facultyCode.ToString()
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

            var libGen = await _context.CaMedLibraryGenerals
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
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
            var libItemList = await (from det in _context.CaMedLibraryItems
                                     join mst in _context.CaMstMedLibraryItems
                                    on det.ItemName equals mst.ItemName
                                     where det.CollegeCode == collegeCode && det.FacultyCode == facultyCode.ToString()
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

            var libBuildingDetails = await _context.CaMedLibraryBuildings
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
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

            var techProcessList = await _context.CaMedLibTechnicalProcesses
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
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
            var LibraryFinanceDetails = await _context.CaMedLibraryFinances
                .Where(e => e.CollegeCode == collegeCode && facultyCode == facultyCode)
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

            var data = await _context.CaMedResearchPublicationsDetails
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
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

            var equipmentList = await _context.CaMedLibraryEquipments
                .Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString())
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
