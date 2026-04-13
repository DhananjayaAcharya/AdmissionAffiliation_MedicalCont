using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAHospitalAffiliationService : ICAHospitalAffiliationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAHospitalAffiliationService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }


        public async Task<HospitalAffiliationCompositeDisplayVM> GetHospitalAffiliationAsync()
        {
            string collegeCode = _userContext.CollegeCode;
            int facultyId = _userContext.FacultyId;
            // 1️⃣ Fetch hospital details
            var hospitals = await _context.HospitalDetailsForAffiliations.AsNoTracking().Where(h => h.CollegeCode == collegeCode).ToListAsync();

            var firstHospital = hospitals.FirstOrDefault();

            // ✅ IMPORTANT NULL CHECK
            if (firstHospital == null)
            {
                return new HospitalAffiliationCompositeDisplayVM
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyId,

                    // ✅ Keep empty object instead of null
                    ClinicalHospitalDetails = new ClinicalHospitalDisplayViewModel(),

                    // ✅ Correct types
                    AffiliatedHospitalDocuments = new List<AffiliatedHospitalDocumentsDisplayViewModel>(),

                    Sections = new List<DepartmentRequirementsSectionDisplayVM>(),

                    IndoorBedsOccupancy = new List<IndoorBedsOccupancyItemVM>(),

                    SuperVisionInFPa = new List<SuperVisionInFieldPracticeAreaDisplayVM>(),

                    HospitalDocumentsToBeUploadedList = new List<HospitalDocumentsToBeUploadedDisplayViewModel>()
                };
            }

            var locData = await (
                from taluk in _context.TalukMasters
                join district in _context.DistrictMasters
                    on taluk.DistrictId equals district.DistrictId
                where taluk.TalukId == firstHospital.HospitalTalukId
                select new
                {
                    district.DistrictName,
                    taluk.TalukName
                }
            ).AsNoTracking().FirstOrDefaultAsync();



            var firstHospitalType = await _context.MstHospitalTypes
                        .AsNoTracking()
                        .Where(x => x.Id == firstHospital.AffiliationTypeId)
                        .Select(x => x.HospitalType)
                        .FirstOrDefaultAsync();

            int ownedById = firstHospital.HospitalOwnedBy != null
                    ? Convert.ToInt32(firstHospital.HospitalOwnedBy)
                    : 0;

            var firstHospitalOwnedBy = await _context.MstHospitalOwnedBies
                        .AsNoTracking().Where(x => x.Id == ownedById)
                        .Select(x => x.OwnedBy)
                        .FirstOrDefaultAsync();


            // 3️⃣ Fetch affiliated documents


            var hospitalsByType = hospitals.GroupBy(h => h.HospitalType).ToDictionary(g => g.Key, g => g.ToList());
            //var AffdocsByHospitalType = await _context.AffiliatedHospitalDocuments.AsNoTracking().Where(d => d.CollegeCode == collegeCode).GroupBy(d => d.HospitalType).ToDictionaryAsync(g => g.Key, g => g.ToList());


            var AffiliatedHospitalDocuments = await _context.AffiliatedHospitalDocuments
                .AsNoTracking()
                .Where(d => d.CollegeCode == collegeCode)
                .Select(d => new AffiliatedHospitalDocumentsDisplayViewModel
                {
                    HospitalType = d.HospitalType ?? "Unknown Hospital Type",
                    HospitalName = d.HospitalName ?? string.Empty,
                    TotalBeds = d.TotalBeds ?? 0,
                    DocumentName = d.DocumentName,
                    DocumentId = d.DocumentId,
                    DocumentExists = d.DocumentFile != null
                })
                .ToListAsync();



            var indoorBedsOccupancy = await BuildIndoorBedsOccupancyDisplayAsync(collegeCode, facultyId, firstHospital.HospitalDetailsId);

            var vm = new HospitalAffiliationCompositeDisplayVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyId,

                ClinicalHospitalDetails = new ClinicalHospitalDisplayViewModel
                {
                    HospitalDetailsId = firstHospital.HospitalDetailsId,
                    HospitalType = firstHospitalType,
                    HospitalName = firstHospital.HospitalName,
                    HospitalOwnedBy = firstHospitalOwnedBy,
                    OwnerName = firstHospital.HospitalOwnerName,
                    DistrictName = locData?.DistrictName,
                    TalukName = locData?.TalukName,
                    Location = firstHospital.Location,
                    TotalBeds = firstHospital.TotalBeds ?? 0,
                    OpdPerDay = firstHospital.OpdperDay ?? 0,
                    IpdOccupancyPercent = firstHospital.IpdbedOccupancyPercent ?? 0,
                    IsOwnerAmemberOfTrust = firstHospital.IsOwnerAmemberOfTrust ?? false,
                    IsSupportingDocExists = firstHospital.HospitalDocumentsToBeUploadeds != null && firstHospital.HospitalDocumentsToBeUploadeds.Any()
                },

                //HospitalDocumentsToBeUploadedList = hospitalDocuments,
                AffiliatedHospitalDocuments = AffiliatedHospitalDocuments,

                Sections = await BuildAllDepartmentSectionsAsync(collegeCode, facultyId, firstHospital.HospitalDetailsId),

                IndoorBedsOccupancy = indoorBedsOccupancy.Items,

                SuperVisionInFPa = new List<SuperVisionInFieldPracticeAreaDisplayVM>
                {
                    await BuildSupervisionInFieldPracticeAreaDisplayAsync(
                        collegeCode, facultyId, firstHospital.HospitalDetailsId)
                }
            };

            return vm;

        }


        private async Task<List<DepartmentRequirementsSectionDisplayVM>> BuildAllDepartmentSectionsAsync(string collegeCode, int facultyCode, int hospitalId)
        {
            var data = await (
                from comp in _context.IndoorInfrastructureRequirementsCompliances
                join master in _context.MstIndoorInfrastructureRequirementsMasters
                    on comp.RequirementId equals master.Id
                where comp.CollegeCode == collegeCode
                      && comp.HospitalDetailsId == hospitalId
                      && master.FacultyCode == facultyCode
                      && master.IsActive
                select new
                {
                    comp.SectionCode,
                    master.SectionName,
                    master.Id,
                    master.RequirementName,
                    comp.IsCompliant,
                    comp.Remarks,
                    comp.AffiliationTypeId
                })
                .AsNoTracking()
                .ToListAsync();

            return data
                .GroupBy(x => x.SectionCode)
                .Select(g => new DepartmentRequirementsSectionDisplayVM
                {
                    SectionCode = int.Parse(g.Key!),
                    SectionName = g.First().SectionName,

                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    HospitalDetailsId = hospitalId,
                    AffiliationTypeId = g.First().AffiliationTypeId,

                    Items = g.Select(x => new DepartmentRequirementBaseDisplayVM
                    {
                        RequirementId = x.Id,
                        RequirementName = x.RequirementName,
                        SectionName = x.SectionName,
                        IsCompliant = x.IsCompliant,
                        Remarks = x.Remarks ?? string.Empty
                    }).ToList()
                })
                .OrderBy(s => s.SectionCode)
                .ToList();
        }


        private async Task<IndoorBedsUnitsRequirementDisplayVM> BuildIndoorBedsOccupancyDisplayAsync(string collegeCode, int facultyCode, int hospitalId)
        {
            var occupancyData = await (from o in _context.IndoorBedsOccupancies.AsNoTracking()
                                       join p in _context.MstIndoorBedsDepartmentMasters.AsNoTracking()
                                       on o.DepartmentId equals p.DeptId
                                       where o.CollegeCode == collegeCode && o.FacultyCode == facultyCode
                                       select new
                                       {
                                           o.DepartmentId,
                                           DepartmentName = p.DepartmentName,
                                           o.SeatSlabId,
                                           o.Rguhsintake,
                                           o.CollegeIntake,
                                           o.AffiliationTypeId
                                       }).ToListAsync();

            var vm = new IndoorBedsUnitsRequirementDisplayVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                HospitalDetailsId = hospitalId,
                AffiliationTypeId = occupancyData.FirstOrDefault()?.AffiliationTypeId ?? 0,
                Items = occupancyData.Select(x => new IndoorBedsOccupancyItemVM
                {
                    DepartmentId = x.DepartmentId,
                    DepartmentName = x.DepartmentName,
                    SeatSlabId = x.SeatSlabId,
                    RGUHSintake = x.Rguhsintake,
                    CollegeIntake = x.CollegeIntake
                }).ToList()
            };

            return vm;  // ✅ Correctly returns VM
        }

        private async Task<SuperVisionInFieldPracticeAreaDisplayVM> BuildSupervisionInFieldPracticeAreaDisplayAsync(string collegeCode, int facultyCode, int hospitalDetailsId)
        {
            var entities = await _context.SuperVisionInFieldPracticeAreas.AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.HospitalDetailsId == hospitalDetailsId)
                .ToListAsync();

            return new SuperVisionInFieldPracticeAreaDisplayVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                HospitalDetailsId = hospitalDetailsId,
                AffiliationTypeId = entities.FirstOrDefault()?.AffiliationTypeId ?? 0,

                Items = entities.Select(x => new SuperVisionInFieldPracticeAreaItemDisplayVM
                {
                    Id = x.Id,
                    Post = x.Post,
                    Name = x.Name,
                    Qualification = x.Qualification ?? "",
                    YearOfQualification = x.YearOfQualification,
                    University = x.University ?? "",
                    UgFromDate = x.UgFromDate,
                    UgToDate = x.UgToDate,
                    PgFromDate = x.PgFromDate,
                    PgToDate = x.PgToDate,
                    Responsibilities = x.Responsibilities ?? ""
                }).ToList()
            };
        }

    }

}
