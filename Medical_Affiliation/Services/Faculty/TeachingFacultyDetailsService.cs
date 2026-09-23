using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class TeachingFacultyDetailsService : ITeachingFacultyDetailsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public TeachingFacultyDetailsService( ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<TeachingFacultyDetailsPreviewViewModel?> GetTeachingFacultyDetailsAsync()
        {
            // =====================================================
            // College / Faculty
            // =====================================================

            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode))
            {
                return null;
            }

            // =====================================================
            // Academic Intake
            // =====================================================

            var academicIntake = await _context.AcademicIntakes
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (academicIntake == null)
                return null;

            // =====================================================
            // Calculate Seat Slab
            // =====================================================

            int totalIntake = academicIntake.Ay2026TotalIntake;

            int slabValue = ((totalIntake + 49) / 50) * 50;

            // =====================================================
            // Get Seat Slab
            // =====================================================

            var seatSlabId = await _context.SeatSlabMasters
                .AsNoTracking()
                .Where(x =>
                    x.FacultyCode.ToString() == facultyCode &&
                    x.SeatSlab == slabValue)
                .Select(x => x.SeatSlabId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(seatSlabId))
                return null;

            // =====================================================
            // Departments
            // =====================================================

            var departments = await _context.DepartmentMasters
                .AsNoTracking()
                .Where(x =>
                    x.FacultyCode.ToString() == facultyCode)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            // =====================================================
            // Top 3 Designations
            // =====================================================

            var designations = await _context.DesignationMasters
                .AsNoTracking()
                .Where(x =>
                    x.FacultyCode.ToString() == facultyCode)
                .OrderBy(x => x.DesignationOrder)
                .Take(3)
                .ToListAsync();

            // =====================================================
            // Faculty Requirement Master
            // =====================================================

            var facultyRequirements =
                await _context.DepartmentWiseFacultyMasters
                    .AsNoTracking()
                    .Where(x =>
                        x.FacultyCode.ToString() == facultyCode &&
                        x.SeatSlabId == seatSlabId)
                    .ToListAsync();

            // =====================================================
            // Saved College Faculty Details
            // =====================================================

            var existingRecords =
                await _context.CollegeDesignationDetails
                    .AsNoTracking()
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode)
                    .ToListAsync();

            // =====================================================
            // Build Preview Records
            // =====================================================

            var result = new List<TeachingFacultyPreviewViewModel>();

            foreach (var dept in departments)
            {
                foreach (var desig in designations)
                {
                    // ---------------------------------------------
                    // Requirement from master
                    // ---------------------------------------------

                    var requirement = facultyRequirements
                        .FirstOrDefault(x =>
                            x.DepartmentCode == dept.DepartmentCode &&
                            x.DesignationCode == desig.DesignationCode);

                    int requiredSeats =
                        requirement?.Seats ?? 0;

                    // ---------------------------------------------
                    // Saved college data
                    // ---------------------------------------------

                    var existing = existingRecords
                        .FirstOrDefault(x =>
                            x.DepartmentCode == dept.DepartmentCode &&
                            x.DesignationCode == desig.DesignationCode &&
                            x.SeatSlabId == seatSlabId);

                    // ---------------------------------------------
                    // Add preview row
                    // ---------------------------------------------

                    result.Add(new TeachingFacultyPreviewViewModel
                    {
                        FacultyCode = facultyCode,

                        CollegeCode = collegeCode,

                        DepartmentCode =
                            dept.DepartmentCode,

                        DepartmentName =
                            dept.DepartmentName,

                        DesignationCode =
                            desig.DesignationCode,

                        DesignationName =
                            desig.DesignationName,

                        SeatSlabId =
                            seatSlabId,

                        RequiredFaculty =
                            existing?.RequiredIntake
                            ?? requiredSeats.ToString(),

                        AvailableFaculty =
                            existing?.AvailableIntake
                            ?? "0"
                    });
                }
            }

            // =====================================================
            // Return Preview ViewModel
            // =====================================================

            return new TeachingFacultyDetailsPreviewViewModel
            {
                FacultyDetails = result
            };
        }
    }
}