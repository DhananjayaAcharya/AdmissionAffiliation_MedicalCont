using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class PhysicalInfrastructureController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public PhysicalInfrastructureController(ApplicationDbContext context, IUserContext userContext)
        {
            this._context = context;
            this._userContext = userContext;
        }

        public async Task<IActionResult> ChairDistribution()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            // =========================================================
            // GET ALL ACADEMIC INTAKES
            // =========================================================

            var academicIntakes = await _context.AcademicIntakes
                .Where(x =>
                    x.FacultyCode == facultyCode.ToString()
                    && x.CollegeCode == collegeCode)
                .ToListAsync();

            // =========================================================
            // CHECK COURSE SOURCE
            // =========================================================

            var savedDentalChairs = await _context.DentalChairs
                                    .Where(x =>
                                        x.CollegeCode == collegeCode &&
                                        x.FacultyCode == facultyCode)
                                    .ToListAsync();

            var collegeCourseExists = await _context.CollegeCourseIntakeDetails
                .AnyAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            List<DentalChairVm> model = new();

            // =========================================================
            // SOURCE 1 → CollegeCourseIntakeDetails
            // =========================================================

            if (collegeCourseExists)
            {
                var courses = await (
                    from mc in _context.MstCourses

                    join cc in _context.CollegeCourseIntakeDetails
                        on mc.CourseCode.ToString() equals cc.CourseCode

                    where mc.FacultyCode == facultyCode
                          && cc.CollegeCode == collegeCode
                          && cc.FacultyCode == facultyCode

                    select mc
                ).Distinct().ToListAsync();

                foreach (var course in courses)
                {
                    // =================================================
                    // FIND COURSE INTAKE
                    // =================================================

                    var intake = academicIntakes.FirstOrDefault(x =>
                        !string.IsNullOrEmpty(x.Courses) &&
                        x.Courses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .Contains(course.CourseCode.ToString()));

                    int seatCount = intake?.Ay2025TotalIntake ?? 0;
                    if (seatCount <= 0)
                    {
                        continue;
                    }

                    // =================================================
                    // CALCULATE SEAT SLAB
                    // =================================================

                    int seatSlab = seatCount > 0
                        ? ((seatCount - 1) / 50 + 1) * 50
                        : 0;

                    // =================================================
                    // GET SLAB MASTER
                    // =================================================

                    var seatSlabData = await _context.SeatSlabMasters
                        .FirstOrDefaultAsync(x =>
                            x.FacultyCode == facultyCode &&
                            x.SeatSlab == seatSlab);

                    // =================================================
                    // ADD TO MODEL
                    // =================================================
                    var existingChairData = savedDentalChairs.FirstOrDefault(x => x.CourseCode == course.CourseCode);


                    model.Add(new DentalChairVm
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,

                        SeatSlab = seatSlab,

                        SeatSlabId = seatSlabData != null
                            ? seatSlabData.SeatSlabId
                            : string.Empty,

                        ChairsRequired = seatSlab,

                        ChairsExisting = existingChairData?.ChairsExisting ?? 0
                    });
                }
            }

            // =========================================================
            // FALLBACK → AcademicIntake.Courses
            // =========================================================

            else
            {
                var courseCodes = academicIntakes
                    .Where(x => !string.IsNullOrEmpty(x.Courses))
                    .SelectMany(x => x.Courses!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                var courses = await _context.MstCourses
                    .Where(x =>
                        x.FacultyCode == facultyCode &&
                        courseCodes.Contains(x.CourseCode.ToString()))
                    .ToListAsync();

                foreach (var course in courses)
                {
                    // =================================================
                    // FIND COURSE INTAKE
                    // =================================================

                    var intake = academicIntakes.FirstOrDefault(x =>
                        !string.IsNullOrEmpty(x.Courses) &&
                        x.Courses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .Contains(course.CourseCode.ToString()));

                    int seatCount = intake?.Ay2025TotalIntake ?? 0;

                    // =================================================
                    // CALCULATE SEAT SLAB
                    // =================================================

                    if (seatCount <= 0)
                    {
                        continue;
                    }
                    int seatSlab = seatCount > 0
                        ? ((seatCount - 1) / 50 + 1) * 50
                        : 0;

                    // =================================================
                    // GET SLAB MASTER
                    // =================================================

                    var seatSlabData = await _context.SeatSlabMasters
                        .FirstOrDefaultAsync(x =>
                            x.FacultyCode == facultyCode &&
                            x.SeatSlab == seatSlab);

                    // =================================================
                    // ADD TO MODEL
                    // =================================================
                    var existingChairData = savedDentalChairs.FirstOrDefault(x => x.CourseCode == course.CourseCode);


                    model.Add(new DentalChairVm
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,

                        SeatSlab = seatSlab,

                        SeatSlabId = seatSlabData != null
                            ? seatSlabData.SeatSlabId
                            : string.Empty,

                        ChairsRequired = seatSlab,

                        ChairsExisting = existingChairData?.ChairsExisting ?? 0
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChairDistribution(List<DentalChairVm> model)
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            foreach (var item in model)
            {
                // Check existing record
                var existingChair = await _context.DentalChairs
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseCode == item.CourseCode);

                // ============================================
                // UPDATE
                // ============================================

                if (existingChair != null)
                {
                    existingChair.ChairsExisting = item.ChairsExisting;
                    existingChair.ChairsRequired = item.ChairsRequired;
                    existingChair.SeatSlab = item.SeatSlab;
                    existingChair.SeatSlabId = item.SeatSlabId;
                }

                // ============================================
                // INSERT
                // ============================================

                else
                {
                    _context.DentalChairs.Add(new DentalChair
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,

                        CourseCode = item.CourseCode,
                        CourseName = item.CourseName,
                        CourseLevel = item.CourseLevel,

                        SeatSlab = item.SeatSlab,
                        SeatSlabId = item.SeatSlabId,

                        ChairsRequired = item.ChairsRequired,
                        ChairsExisting = item.ChairsExisting
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Chair details saved successfully.";

            return RedirectToAction(nameof(ChairDistribution));
        }
    }
}
