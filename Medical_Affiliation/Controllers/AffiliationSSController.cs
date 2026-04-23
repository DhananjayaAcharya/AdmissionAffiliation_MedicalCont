using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Medical_Affiliation.Controllers
{
    public class AffiliationSSController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public AffiliationSSController(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<IActionResult> CoursesOffered()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var AllssDetails = new AffiliationSSViewModel();

            AllssDetails.AllCourses = await GetAllCourses();

            return View(AllssDetails);
        }

        public async Task<List<SScourseVM>> GetAllCourses()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();
            var courselevel = HttpContext.Session.GetString("CourseLevel");

            // 1️⃣ Load data (only what we need)
            var offeredCourses = await _context.CoursesOffereds
                .Where(co => co.CollegeCode == collegeCode && co.FacultyCode == facultyCode)
                .Select(co => new
                {
                    co.CourseCode,
                    co.CourseName,
                    co.CourseLevel,
                    co.SanctionedAdmissions,
                    co.AdmittedAdmissions,
                    co.YearOfStarting,
                    co.Remarks
                })
                .ToListAsync();

            var intakeCourses = await _context.CollegeCourseIntakeDetails
                .Where(ci => ci.CollegeCode == collegeCode)
                .Select(ci => new
                {
                    ci.CourseCode,
                    ci.CourseName,
                    ci.ExistingIntake
                })
                .ToListAsync();

            var masterCourses = await _context.MstCourses
                .Select(m => new
                {
                    Code = m.CourseCode.ToString(),
                    m.CourseName,
                    m.CourseLevel
                })
                .ToListAsync();

            // 2️⃣ Convert to dictionaries (O(1) lookups)
            var offeredDict = offeredCourses
                .ToDictionary(x => x.CourseCode);

            var intakeDict = intakeCourses
                .ToDictionary(x => x.CourseCode);

            var masterDict = masterCourses
                .ToDictionary(x => x.Code);

            //code updated by ram on 09-04-2026

            // 3️⃣ Union of course codes
            //var allCourseCodes = offeredDict.Keys
            //    .Union(intakeDict.Keys)
            //    .ToList();

            var allCourseCodes = offeredDict.Keys
            .Union(intakeDict.Keys)
            .Where(code =>
                (offeredDict.ContainsKey(code) && offeredDict[code].CourseLevel == "SS") ||
                (masterDict.ContainsKey(code) && masterDict[code].CourseLevel == "SS")
            )
            .ToList();



            // 4️⃣ Build result
            var result = new List<SScourseVM>(allCourseCodes.Count);

            foreach (var code in allCourseCodes)
            {
                offeredDict.TryGetValue(code, out var offered);
                intakeDict.TryGetValue(code, out var intake);
                masterDict.TryGetValue(code, out var master);

                result.Add(new SScourseVM
                {
                    CourseCode = code,

                    CourseName =
                        offered?.CourseName ??
                        intake?.CourseName ??
                        master?.CourseName,

                    CourseLevel =
                        offered?.CourseLevel ??
                        master?.CourseLevel,

                    Sanctioned =
                        offered != null
                            ? offered.SanctionedAdmissions
                            : intake?.ExistingIntake,

                    Admitted = offered?.AdmittedAdmissions,
                    YearOfStarting = offered?.YearOfStarting,

                    Remarks = offered?.Remarks
                });
            }

            return result;
        }


        //[HttpPost]
        //public async Task<IActionResult> SaveCourses(AffiliationSSViewModel vm)
        //{
        //    var collegeCode = _userContext.CollegeCode;
        //    var facultyCode = _userContext.FacultyId;
        //    var affType = _userContext.TypeOfAffiliation;

        //    foreach (var course in vm.AllCourses)
        //    {
        //        //code updated by ram on 09-04-2026

        //        //if (course.Admitted == null) continue;

        //        // ✅ Skip only completely empty rows
        //        if (course.Admitted == null &&
        //            string.IsNullOrWhiteSpace(course.YearOfStarting) &&
        //            string.IsNullOrWhiteSpace(course.Remarks))
        //        {
        //            continue;
        //        }


        //        var existing = await _context.CoursesOffereds.Where(e => e.CollegeCode == collegeCode && e.CourseCode == course.CourseCode && e.FacultyCode == facultyCode.ToString()).FirstOrDefaultAsync();
        //        if (existing == null)
        //        {
        //            var entity = new CoursesOffered()
        //            {
        //                CollegeCode = collegeCode,
        //                FacultyCode = facultyCode.ToString(),
        //                CourseCode = course.CourseCode,
        //                CourseLevel = course.CourseLevel,
        //                CourseName = course.CourseName,
        //                TypeOfAffiliation = affType,
        //                YearOfStarting = course.YearOfStarting,
        //                SanctionedAdmissions = course.Sanctioned,
        //                AdmittedAdmissions = course.Admitted,
        //                Remarks = course.Remarks,
        //            };
        //            _context.CoursesOffereds.Add(entity);
        //        }
        //        else
        //        {
        //            existing.AdmittedAdmissions = course.Admitted;
        //            existing.YearOfStarting = course.YearOfStarting;
        //            existing.Remarks = course.Remarks;
        //        }
        //    }

        //    await _context.SaveChangesAsync();
        //    TempData["SScourseOffered"] = "Course Offered saved succesfully";
        //    //return RedirectToAction(nameof(Index));
        //    return RedirectToAction("AssociatedInstitutions");


        //}

        [HttpPost]
        public async Task<IActionResult> SaveCourses(AffiliationSSViewModel vm)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("SessionExpired");

            ModelState.Clear();

            foreach (var course in vm.AllCourses)
            {
                var existing = await _context.CoursesOffereds
                    .FirstOrDefaultAsync(x =>
                        x.CourseCode == course.CourseCode &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode);

                bool isEmpty =
                    course.Admitted == null &&
                    string.IsNullOrWhiteSpace(course.YearOfStarting) &&
                    string.IsNullOrWhiteSpace(course.Remarks);

                // ✅ CASE 1: Row cleared → DELETE from DB
                if (isEmpty)
                {
                    if (existing != null)
                    {
                        _context.CoursesOffereds.Remove(existing);
                    }
                    continue;
                }

                // ✅ CASE 2: UPDATE
                if (existing != null)
                {
                    existing.AdmittedAdmissions = course.Admitted;
                    existing.SanctionedAdmissions = course.Sanctioned;
                    existing.YearOfStarting = course.YearOfStarting;
                    existing.Remarks = course.Remarks;
                }
                else
                {
                    // ✅ INSERT
                    var newCourse = new CoursesOffered
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        AdmittedAdmissions = course.Admitted,
                        SanctionedAdmissions = course.Sanctioned,
                        YearOfStarting = course.YearOfStarting,
                        Remarks = course.Remarks,
                        CreatedOn = DateTime.Now,
                        TypeOfAffiliation = vm.TypeOfAffiliation
                    };

                    _context.CoursesOffereds.Add(newCourse);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Medical_LandBuildingdetails", "Medical_ContinuousAffiliation");
        }


        public async Task<IActionResult> AssociatedInstitutions()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var typeOfAffiliation = _userContext.TypeOfAffiliation;
            var vm = new AffiliationSSViewModel
            {
                CollegeCode = collegeCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            vm.AssociatedInstitutions = await (
                        from ai in _context.AssociatedInstitutions
                        where ai.CollegeCode == collegeCode && ai.FacultyCode == facultyCode.ToString()

                        join acm in _context.AffiliationCollegeMasters
                            on ai.CollegeCode equals acm.CollegeCode
                            into collegejoin

                        from acm in collegejoin.DefaultIfEmpty()

                        join f in _context.Faculties
                             on ai.FacultyCode equals f.FacultyId.ToString()
                             into facultyjoin

                        from f in facultyjoin.DefaultIfEmpty()

                        join com in _context.MstCourses
                            on ai.CourseCode equals com.CourseCode.ToString()
                            into coursesjoin

                        from com in coursesjoin.DefaultIfEmpty()

                        select new SSAssociatedInstitutions
                        {
                            Id = ai.Id,

                            CourseCode = ai.CourseCode,
                            CourseName = com.CourseName,
                            CourseLevel = ai.CourseLevel,

                            FacultyCode = ai.FacultyCode,

                            AssociatedCollegeCode = ai.AssociatedCollegeCode,
                            AssociatedCollegeName = acm != null ? acm.CollegeName : null,

                            AssociatedFacultyCode = ai.AssociatedFacultyCode,
                            AssociatedFacultyName = f != null ? f.FacultyName : null,

                            AnnualIntake = ai.AnnualIntake
                        }
                ).ToListAsync();


            vm.Faculties = await _context.Faculties
                .AsNoTracking()
                .Where(e => e.FacultyId != 1)
                .Select(c => new FacultyOptionVM
                {
                    FacultyCode = c.FacultyId.ToString(),
                    FacultyName = c.FacultyName,
                }).ToListAsync();


            vm.CourseLevelList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UG", Value = "UG" },
                new SelectListItem { Text = "PG", Value = "PG" },
                new SelectListItem { Text = "SS", Value = "SS" },
            };
            return View("AssociatedInstitutions", vm);

        }

        [HttpGet]
        public async Task<IActionResult> GetAssociatedColleges(string facultyCode)
        {
            var colleges = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(c => c.FacultyCode == facultyCode)
                .Select(c => new CollegeOptionVM
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName
                })
                .OrderBy(e => e.CollegeName)
                .ToListAsync();

            return Json(colleges);
        }

        [HttpGet]
        public async Task<IActionResult> GetCoursesByCollege(string collegeCode, string facultyCode, string courseLevel)
        {
            var courses = await _context.CollegeCourseIntakeDetails
                .AsNoTracking()
                .Where(c =>
                    c.CollegeCode == collegeCode &&
                    c.FacultyCode.ToString() == facultyCode
                )
                .Join(
                    _context.MstCourses,
                    ci => ci.CourseCode,
                    mc => mc.CourseCode.ToString(),
                    (ci, mc) => new { ci, mc }
                )
                .Where(x => x.mc.CourseLevel == courseLevel)
                .Select(x => new CourseOptionVM
                {
                    CourseCode = x.mc.CourseCode.ToString(),
                    CourseName = x.mc.CourseName,
                    CourseLevel = x.mc.CourseLevel
                })
                .Distinct()
                .OrderBy(e => e.CourseName)
                .ToListAsync();

            return Json(courses);
        }

        [HttpPost]
        public async Task<IActionResult> AssociatedInstitutions([FromBody] SSAssociatedInstitutionPostVM model)
        {
            if (string.IsNullOrWhiteSpace(model.CourseLevel) ||
                string.IsNullOrWhiteSpace(model.AssociatedFacultyCode) ||
                string.IsNullOrWhiteSpace(model.AssociatedCollegeCode) ||
                string.IsNullOrWhiteSpace(model.CourseCode) ||
                model.AnnualIntake <= 0)
            {
                return BadRequest("Invalid input");
            }

            var collegeCode = _userContext.CollegeCode;
            var typeOfAffiliation = _userContext.TypeOfAffiliation;
            var facultyCode = _userContext.FacultyId.ToString();

            var exists = await _context.AssociatedInstitutions.AnyAsync(e =>
                e.CollegeCode == collegeCode &&
                e.AssociatedCollegeCode == model.AssociatedCollegeCode &&
                e.AssociatedFacultyCode == model.AssociatedFacultyCode &&
                e.CourseCode == model.CourseCode &&
                e.TypeOfAffiliation == typeOfAffiliation);

            if (exists)
            {
                return Conflict(new
                {
                    message = "This association already exists"
                });
            }

            var entity = new AssociatedInstitution
            {
                CollegeCode = collegeCode,
                TypeOfAffiliation = typeOfAffiliation,
                FacultyCode = facultyCode,
                AssociatedFacultyCode = model.AssociatedFacultyCode,
                AssociatedCollegeCode = model.AssociatedCollegeCode,
                CourseCode = model.CourseCode,
                CourseLevel = model.CourseLevel,
                AnnualIntake = model.AnnualIntake
            };

            _context.AssociatedInstitutions.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }


    }
}
