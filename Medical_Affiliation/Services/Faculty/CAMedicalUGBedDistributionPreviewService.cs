using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAMedicalUGBedDistributionPreviewService
        : CAPreviewServiceBase, ICAMedicalUGBedDistributionPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAMedicalUGBedDistributionPreviewService(
            ApplicationDbContext context,
            IUserContext userContext) : base(context, userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<MedicalUGBedDistributionVM> GetMedicalUGBedDistributionPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var courseLevel = _userContext.CourseLevel;

            var vm = new MedicalUGBedDistributionVM();

            var existing = await _context.MedicalUgbedDistributions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString() );

            // =========================================
            // MEDICAL
            // =========================================
            if (facultyCode == 1 && existing != null)
            {
                vm.Medical = new MedicalBedDistributionVM
                {
                    GenMedicine = existing.GenMedicine,
                    Paediatrics = existing.Paediatrics,
                    SkinVD = existing.SkinVd,
                    Psychiatry = existing.Psychiatry,

                    GenSurgery = existing.GenSurgery,
                    Orthopaedics = existing.Orthopaedics,
                    Ophthalmology = existing.Ophthalmology,
                    ENT = existing.Ent,

                    ObstetricsANC = existing.ObstetricsAnc,
                    Gynaecology = existing.Gynaecology,
                    Postpartum = existing.Postpartum,

                    MajorOT = existing.MajorOt,
                    MinorOT = existing.MinorOt,

                    ICCU = existing.Iccu,
                    ICU = existing.Icu,
                    PICU_NICU = existing.PicuNicu,
                    SICU = existing.Sicu,

                    TotalICUBeds = existing.TotalIcubeds,
                    CasualtyBeds = existing.CasualtyBeds
                };
            }

            // =========================================
            // DENTAL
            // =========================================
            if (facultyCode == 2)
            {
                var academicIntake = await _context.AcademicIntakes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode.ToString());

                int seatIntake = academicIntake?.Ay2026TotalIntake ?? 0;

                int seatSlab = seatIntake switch
                {
                    <= 50 => 50,
                    <= 100 => 100,
                    <= 150 => 150,
                    _ => 0
                };

                vm.Dental = new DentalBedDistributionPreviewVM
                {
                    OralMaxillofacialSurgery = existing?.OralMaxillofacialSurgery,

                    DentalWards = await (
                        from master in _context.MstDentalBedDistributions

                        join saved in _context.DentalWardBedDistributions
                            .Where(x =>
                                x.CollegeCode == collegeCode &&
                                x.FacultyCode == facultyCode)
                        on master.Id equals saved.WardId into savedGroup

                        from saved in savedGroup.DefaultIfEmpty()

                        where master.FacultyCode == facultyCode
                              && master.SeatSlab == seatSlab

                        orderby master.WardName

                        select new DentalWardBedDistributionVm
                        {
                            WardId = master.Id,
                            WardName = master.WardName,
                            SeatSlab = master.SeatSlab,
                            BedsRequired = master.BedRequirement,
                            BedsPresent = saved != null ? saved.BedsPresent : null
                        }).ToListAsync()
                };
            }

            return vm;
        }
    }
}