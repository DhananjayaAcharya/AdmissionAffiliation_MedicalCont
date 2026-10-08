using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Medical_Affiliation.Controllers
{

    [Authorize(AuthenticationSchemes = "CollegeAuth", Roles = "College")]
    public class AffiliationPgCourseController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public AffiliationPgCourseController(ApplicationDbContext context, IUserContext userContext) : base(context)
        {
            _context = context;
            _userContext = userContext;
        }
        public async Task<IActionResult> PgCourses()
        {
            var collegeCode = _userContext.CollegeCode;

            var degreeCourses = await GetDegreeCourses();
            var diplomaCourses = await GetDiplomaCourses();

            // Overlay particulars (existing first)
            var pgParticulars = (await GetPgCoursesParticulars()).ToDictionary(x => x.CourseCode);

            var allCourses = degreeCourses
                .Concat(diplomaCourses)
                .Select(c =>
                {
                    pgParticulars.TryGetValue(c.CourseCode, out var p); // get particulars if exists
                    return new PgCourseParticularsVm
                    {
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        CourseLevel = c.CourseLevel,
                        CoursePrefix = c.CoursePrefix,
                        CollegeIntake = c.CollegeIntake,
                        RguhsIntake = c.RguhsIntake,
                        DateofLOP = p?.DateofLOP,
                        DateofRecognitionByNMC = p?.DateofRecognitionByNMC,
                        DateofRecognitionByDCI = p?.DateofRecognitionByDCI
                    };
                })
                .ToList();

            var gokData = await GetPgCoursesForGOK();

            var rguhsData = await GetPgCoursesWithRguhsPermission();
            var otherDeptData = await GetOtherDeptCoursesPermittedByNmc();
            var licInspectionData = await GetLicInspectionDetails();


            var result = new AffiliationPgCourseViewModel
            {
                CollegeCode = collegeCode,
                PgDegreeCourses = degreeCourses,
                PgDiplomaCourses = diplomaCourses,
                AllCourses = allCourses,
                PgCoursesGOK = gokData,
                TypeOfAffiliation = _userContext.TypeOfAffiliation,
                PgCoursesRguhs = rguhsData,
                OtherCoursesPermittedByNMC = otherDeptData,
                LicInspectionVm = licInspectionData

            };

            return View(result);
        }

        public async Task<List<PgCourseVm>> GetDegreeCourses()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var getDegreeCourses = await (from cc in _context.CollegeCourseIntakeDetails
                                          join ms in _context.MstCourses
                                          on cc.CourseCode equals ms.CourseCode.ToString()
                                          where cc.CollegeCode == collegeCode && ms.CoursePrefix != "Diploma" && ms.CourseLevel != "UG"
                                          select new PgCourseVm
                                          {
                                              CourseCode = cc.CourseCode,
                                              CourseName = ms.CourseName,
                                              CourseLevel = ms.CourseLevel,
                                              CoursePrefix = ms.CoursePrefix,
                                              CollegeIntake = cc.PresentIntake
                                          }
                                          ).ToListAsync();


            // Fallback to AcademicIntake
            if (!getDegreeCourses.Any())
            {
                getDegreeCourses = await (
                    from ai in _context.AcademicIntakes
                    join ms in _context.MstCourses
                        on ai.Courses equals ms.CourseCode.ToString()
                    where ai.CollegeCode == collegeCode
                          && ai.FacultyCode == facultyCode.ToString()
                          && ms.CoursePrefix != "Diploma"
                          && ms.CourseLevel != "UG"
                    select new PgCourseVm
                    {
                        CourseCode = ai.Courses ?? "",
                        CourseName = ms.CourseName,
                        CourseLevel = ms.CourseLevel,
                        CoursePrefix = ms.CoursePrefix,
                        CollegeIntake = ai.Ay2026TotalIntake // choose the appropriate intake field
                    }
                )
                .Distinct()
                .ToListAsync();
            }
            return getDegreeCourses;
        }

        public async Task<List<PgCourseVm>> GetDiplomaCourses()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var getDiplomaCourses = await (from cc in _context.CollegeCourseIntakeDetails
                                           join ms in _context.MstCourses
                                           on cc.CourseCode equals ms.CourseCode.ToString()
                                           where cc.CollegeCode == collegeCode && ms.CoursePrefix == "Diploma"
                                           select new PgCourseVm
                                           {
                                               CourseCode = cc.CourseCode,
                                               CourseName = ms.CourseName,
                                               CourseLevel = ms.CourseLevel,
                                               CoursePrefix = ms.CoursePrefix,
                                               CollegeIntake = cc.PresentIntake
                                           }
                                          ).ToListAsync();

            if (!getDiplomaCourses.Any())
            {
                getDiplomaCourses = await (
                    from ai in _context.AcademicIntakes
                    join ms in _context.MstCourses
                        on ai.Courses equals ms.CourseCode.ToString()
                    where ai.CollegeCode == collegeCode
                          && ai.FacultyCode == facultyCode.ToString()
                          && ms.CoursePrefix == "Diploma"
                    select new PgCourseVm
                    {
                        CourseCode = ai.Courses ?? "",
                        CourseName = ms.CourseName,
                        CourseLevel = ms.CourseLevel,
                        CoursePrefix = ms.CoursePrefix,
                        CollegeIntake = ai.Ay2026TotalIntake // replace if another year is required
                    }
                )
                .Distinct()
                .ToListAsync();
            }

            return getDiplomaCourses;
        }

        public async Task<List<PgCourseParticularsVm>> GetPgCoursesParticulars()
        {
            var collegeCode = _userContext.CollegeCode;

            // 1️⃣ Existing affiliation data (may be empty)
            var existingData = await _context.AffiliationPgSsCourseDetails
                .Where(e => e.CollegeCode == collegeCode)
                .ToDictionaryAsync(e => e.CourseCode);


            // 2️⃣ All PG courses for the college
            var allCourses = await (
                from cc in _context.CollegeCourseIntakeDetails
                join ms in _context.MstCourses
                    on cc.CourseCode equals ms.CourseCode.ToString()
                where cc.CollegeCode == collegeCode
                select new PgCourseVm
                {
                    CourseCode = cc.CourseCode,
                    CourseName = ms.CourseName,
                    CourseLevel = ms.CourseLevel,
                    CoursePrefix = ms.CoursePrefix,
                    CollegeIntake = cc.PresentIntake,
                    RguhsIntake = cc.ExistingIntake,
                }
            ).ToListAsync();

            if (!allCourses.Any())
            {
                var facultyCode = _userContext.FacultyId;

                allCourses = await (
                    from ai in _context.AcademicIntakes
                    join ms in _context.MstCourses
                        on ai.Courses equals ms.CourseCode.ToString()
                    where ai.CollegeCode == collegeCode
                          && ai.FacultyCode == facultyCode.ToString()
                    select new PgCourseVm
                    {
                        CourseCode = ai.Courses ?? "",
                        CourseName = ms.CourseName,
                        CourseLevel = ms.CourseLevel,
                        CoursePrefix = ms.CoursePrefix,
                        CollegeIntake = ai.Ay2026TotalIntake, // use required year
                        RguhsIntake = ai.Ay2026ExistingIntake // use required year
                    }
                )
                .Distinct()
                .ToListAsync();
            }

            var result = new List<PgCourseParticularsVm>();

            // 3️⃣ Overlay existing data (if any)
            foreach (var course in allCourses)
            {
                if (existingData.TryGetValue(course.CourseCode, out var existing))
                {
                    result.Add(new PgCourseParticularsVm
                    {
                        CourseCode = course.CourseCode,
                        DateofLOP = existing.Lopdate,
                        DateofRecognitionByNMC = FacultyCode == "1"
                            ? existing.DateofRecognitionByNmc
                            : null,

                        DateofRecognitionByDCI = FacultyCode == "2"
                            ? existing.DateofRecognitionByDci
                            : null,
                        CourseLevel = course.CourseLevel,
                        CourseName = course.CourseName,
                        CoursePrefix = course.CoursePrefix,
                        CollegeIntake = existing.PresentIntake,
                        RguhsIntake = existing.RguhsIntake
                    });

                }
                else
                {
                    result.Add(new PgCourseParticularsVm
                    {
                        CourseCode = course.CourseCode,
                        CourseLevel = course.CourseLevel,
                        CourseName = course.CourseName,
                        CoursePrefix = course.CoursePrefix,
                        CollegeIntake = course.CollegeIntake,
                        RguhsIntake = course.RguhsIntake
                    });
                }
            }

            // 4️⃣ Existing first (optional ordering)
            return result
                .OrderByDescending(c => c.DateofLOP.HasValue)
                .ToList();
        }

        public async Task<List<PgCoursesGokVM>> GetPgCoursesForGOK()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var pgCoursesQuery =
                from ci in _context.CollegeCourseIntakeDetails
                where ci.CollegeCode == collegeCode

                join cm in _context.MstCourses
                 on ci.CourseCode equals cm.CourseCode.ToString()

                where cm.CourseLevel == "PG"

                join gok in _context.AffiliationPgSsCourseDetailsForGoks
                    .Where(e => e.CollegeCode == collegeCode)
                    on ci.CourseCode equals gok.CourseCode into gokGroup

                from gok in gokGroup.DefaultIfEmpty()

                select new PgCoursesGokVM
                {
                    CollegeCode = collegeCode,
                    CourseCode = gok != null ? gok.CourseCode : ci.CourseCode,
                    CourseName = gok != null ? gok.CourseName : cm.CourseName,
                    CourseLevel = gok != null ? gok.CourseLevel : cm.CourseLevel,
                    CoursePrefix = gok != null ? gok.CoursePrefix : cm.CoursePrefix,
                    CollegeIntake = gok != null ? gok.PresentIntake : ci.PresentIntake,
                    RguhsIntake = gok != null ? gok.PresentIntake : ci.ExistingIntake,
                    HasGOKDocument = gok != null && gok.DocumentofGokpath != null && gok.DocumentofGokpath.Length > 0,
                    AcademicYear = gok != null ? gok.AcademicYear : null,
                    DateofGOK = gok != null ? gok.Gokdate : null,


                };

            var result = await pgCoursesQuery.ToListAsync();
            if (!result.Any() && facultyCode == 2)
            {
                result = await (
                    from ai in _context.AcademicIntakes
                    join cm in _context.MstCourses
                        on ai.Courses equals cm.CourseCode.ToString()

                    join gok in _context.AffiliationPgSsCourseDetailsForGoks
                        .Where(e => e.CollegeCode == collegeCode)
                        on ai.Courses equals gok.CourseCode into gokGroup

                    from gok in gokGroup.DefaultIfEmpty()

                    where ai.CollegeCode == collegeCode
                          && ai.FacultyCode == facultyCode.ToString()
                          && cm.CourseLevel == "PG"

                    select new PgCoursesGokVM
                    {
                        CollegeCode = collegeCode,
                        CourseCode = ai.Courses ?? "",
                        CourseName = gok != null ? gok.CourseName : cm.CourseName,
                        CourseLevel = gok != null ? gok.CourseLevel : cm.CourseLevel,
                        CoursePrefix = gok != null ? gok.CoursePrefix : cm.CoursePrefix,

                        CollegeIntake = gok != null
                            ? gok.PresentIntake
                            : ai.Ay2026TotalIntake,

                        RguhsIntake = gok != null
                            ? gok.PresentIntake
                            : ai.Ay2026ExistingIntake,

                        HasGOKDocument = gok != null
                            && gok.DocumentofGokpath != null
                            && gok.DocumentofGokpath.Length > 0,

                        AcademicYear = gok != null ? gok.AcademicYear : null,
                        DateofGOK = gok != null ? gok.Gokdate : null
                    }
                ).ToListAsync();
            }
            return result;
        }


        public async Task<List<PgCoursesWithRGUHSPermission>> GetPgCoursesWithRguhsPermission()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            var pgCourseswithRguhs =
                from ci in _context.CollegeCourseIntakeDetails
                where ci.CollegeCode == collegeCode

                join mst in _context.MstCourses
                on ci.CourseCode equals mst.CourseCode.ToString()

                where mst.CourseLevel == "PG"

                join rguhsCourses in _context.AffiliationPgSsCourseDetailsRguhs.Where(e => e.CollegeCode == collegeCode)
                on ci.CourseCode equals rguhsCourses.CourseCode into rguhsCoursesGroup

                from rguhsCourses in rguhsCoursesGroup.DefaultIfEmpty()

                select new PgCoursesWithRGUHSPermission
                {
                    CollegeCode = collegeCode,
                    CourseCode = rguhsCourses != null ? rguhsCourses.CourseCode : ci.CourseCode,
                    CourseName = mst.CourseName,
                    CourseLevel = mst.CourseLevel,
                    CoursePrefix = mst.CoursePrefix,
                    RguhsIntake = rguhsCourses != null ? rguhsCourses.RguhsIntake : ci.ExistingIntake,
                    HasRguhsDocument = rguhsCourses != null && rguhsCourses.RguhssupportingDocumentPath != null && rguhsCourses.RguhssupportingDocumentPath.Length > 0,
                };

            var result = await pgCourseswithRguhs.ToListAsync();
            if (!result.Any() && facultyCode == "2")
            {
                result = await (
                    from ai in _context.AcademicIntakes
                    join mst in _context.MstCourses
                        on ai.Courses equals mst.CourseCode.ToString()

                    join rguhsCourses in _context.AffiliationPgSsCourseDetailsRguhs
                        .Where(e => e.CollegeCode == collegeCode)
                        on ai.Courses equals rguhsCourses.CourseCode into rguhsCoursesGroup

                    from rguhsCourses in rguhsCoursesGroup.DefaultIfEmpty()

                    where ai.CollegeCode == collegeCode
                          && ai.FacultyCode == facultyCode
                          && mst.CourseLevel == "PG"

                    select new PgCoursesWithRGUHSPermission
                    {
                        CollegeCode = collegeCode,

                        CourseCode = rguhsCourses != null
                            ? rguhsCourses.CourseCode
                            : ai.Courses!,

                        CourseName = mst.CourseName,
                        CourseLevel = mst.CourseLevel,
                        CoursePrefix = mst.CoursePrefix,

                        RguhsIntake = rguhsCourses != null
                            ? rguhsCourses.RguhsIntake
                            : ai.Ay2026ExistingIntake, // change year if needed

                        HasRguhsDocument =
                            rguhsCourses != null &&
                            rguhsCourses.RguhssupportingDocumentPath != null &&
                            rguhsCourses.RguhssupportingDocumentPath.Length > 0
                    }
                ).ToListAsync();
            }
            return result;
        }

        public async Task<List<OtherCoursesPermittedByNMC>> GetOtherDeptCoursesPermittedByNmc()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var otherCourses = await (
                from mst in _context.MstCourses
                where mst.FacultyCode != facultyCode && mst.CourseLevel == "PG"
                join ot in _context.AffiliationOtherCoursesPermittedByNmcs.Where(e => e.CollegeCode == collegeCode)
                    on mst.CourseCode.ToString() equals ot.CourseCode into otGroup

                from ot in otGroup.DefaultIfEmpty()

                select new OtherCoursesPermittedByNMC
                {
                    CourseLevel = mst.CourseLevel,
                    CourseCode = mst.CourseCode.ToString(),
                    CourseName = mst.CourseName,
                    PermissionByNMC = ot != null && ot.PermissionByNmc == 1,
                    HasNMCdocument = ot.NmcsupportingDocumentPath != null && ot.NmcsupportingDocumentPath.Length > 0,
                    AdmissionsPerYear = ot.NumberOfAdmissionsPerYear ?? 0,
                    FacultyCode = ot != null ? ot.FacultyCode : mst.FacultyCode.ToString()
                }
                ).ToListAsync();

            return otherCourses;
        }

        public async Task<LICinspectionVM> GetLicInspectionDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var vm = new LICinspectionVM();
            var inspectionData = await _context.AffiliationLicinpsections.Where(e => e.CollegeCode == collegeCode && e.FacultyCode == facultyCode.ToString()).FirstOrDefaultAsync();
            if (inspectionData != null)
            {
                vm.ActionTaken = inspectionData.ActionTaken;
                vm.PreviousInspectionDate = inspectionData.PreviousInspectionDate;
            }
            return vm;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePgCourseParticulars(PgCourseParticularsPostVm model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return RedirectToAction(nameof(PgCourses));
            //}

            foreach (var course in model.Courses)
            {
                if (string.IsNullOrWhiteSpace(course.CourseCode))
                    continue;

                // Ignore empty rows (optional safety)
                if ((FacultyCode == "1" &&
                         course.DateofLOP == null &&
                         course.DateofRecognitionByNMC == null)
                     ||
                        (FacultyCode == "2" &&
                         course.DateofLOP == null &&
                         course.DateofRecognitionByDCI == null))
                {
                    continue;
                }

                var existing = await _context.AffiliationPgSsCourseDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == model.CollegeCode &&
                        x.CoursePrefix == course.CourseCode);

                if (existing == null)
                {
                    // INSERT
                    _context.AffiliationPgSsCourseDetails.Add(new AffiliationPgSsCourseDetail
                    {
                        CollegeCode = model.CollegeCode,
                        CourseCode = course.CourseCode,
                        FacultyCode = _userContext.FacultyId.ToString(),
                        TypeOfAffiliation = _userContext.TypeOfAffiliation.ToString(),
                        CourseName = course.CourseName,
                        CoursePrefix = course.CourseCode,
                        CourseLevel = course.CourseLevel,
                        PresentIntake = course.CollegeIntake,
                        RguhsIntake = course.RguhsIntake,
                        Lopdate = course.DateofLOP,
                        DateofRecognitionByNmc = course.DateofRecognitionByNMC,
                        DateofRecognitionByDci = course.DateofRecognitionByDCI
                    });
                }
                else
                {
                    // UPDATE
                    existing.Lopdate = course.DateofLOP;
                    existing.DateofRecognitionByNmc = course.DateofRecognitionByNMC;
                    existing.DateofRecognitionByDci = course.DateofRecognitionByDCI;
                }
            }

            await _context.SaveChangesAsync();

            TempData["pgparticulars"] = "PG Course Particulars saved successfully.";

            return RedirectToAction(nameof(PgCourses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAyurvedaPgCourseParticulars(
    PgCourseParticularsPostVm model,
    string? CourseLevel,
    string? TypeOfAffiliation)
        {
            const int ayurvedaFacultyCode = 4;

            var collegeCode = _userContext.CollegeCode;

            foreach (var course in model.Courses)
            {
                if (string.IsNullOrWhiteSpace(course.CourseCode))
                    continue;

                // For Ayurveda, only Date of LOP is applicable here.
                // Ignore completely empty rows.
                if (course.DateofLOP == null)
                    continue;

                var existing = await _context.AffiliationPgSsCourseDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.CourseCode == course.CourseCode &&
                        x.FacultyCode == ayurvedaFacultyCode.ToString());

                if (existing == null)
                {
                    var entity = new AffiliationPgSsCourseDetail
                    {
                        CollegeCode = collegeCode,

                        CourseCode = course.CourseCode,

                        FacultyCode = ayurvedaFacultyCode.ToString(),

                        TypeOfAffiliation =
                            _userContext.TypeOfAffiliation.ToString(),

                        CourseName = course.CourseName,

                        CoursePrefix = course.CourseCode,

                        CourseLevel = course.CourseLevel,

                        PresentIntake = course.CollegeIntake,

                        RguhsIntake = course.RguhsIntake,

                        Lopdate = course.DateofLOP,

                        DateofRecognitionByNmc = null,

                        DateofRecognitionByDci = null
                    };

                    _context.AffiliationPgSsCourseDetails.Add(entity);
                }
                else
                {
                    existing.Lopdate = course.DateofLOP;
                    existing.PresentIntake = course.CollegeIntake;
                    existing.RguhsIntake = course.RguhsIntake;
                    existing.CourseName = course.CourseName;
                    existing.CourseLevel = course.CourseLevel;
                }
            }

            await _context.SaveChangesAsync();

            // Make sure the PG context remains available.
            HttpContext.Session.SetString(
                "CourseLevel",
                string.IsNullOrWhiteSpace(CourseLevel)
                    ? "PG"
                    : CourseLevel.ToUpperInvariant()
            );

            if (!string.IsNullOrWhiteSpace(TypeOfAffiliation))
            {
                HttpContext.Session.SetString(
                    "TypeOfAffiliation",
                    TypeOfAffiliation
                );
            }

            TempData["pgparticulars"] =
                "Ayurveda PG Course Particulars saved successfully.";

            return RedirectToAction(
                nameof(PgCoursesAyurveda),
                new
                {
                    typeOfAffiliation = TypeOfAffiliation,
                    courseLevel = "PG"
                }
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePgCoursesForGOK(AffiliationPgCourseViewModel model)
        {
            var collegeCode = _userContext.CollegeCode;

            foreach (var course in model.PgCoursesGOK)
            {
                if (string.IsNullOrWhiteSpace(course.CourseCode) || string.IsNullOrWhiteSpace(course.AcademicYear))
                    continue;

                var existingCourse = await _context.AffiliationPgSsCourseDetailsForGoks
                    .FirstOrDefaultAsync(e => e.CollegeCode == course.CollegeCode && e.CourseCode == course.CourseCode);

                var path = await SavePgFileAsync(course.GOKDocumentFile, "GOK");

                if (existingCourse == null)
                {
                    var entity = new AffiliationPgSsCourseDetailsForGok
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,
                        CoursePrefix = course.CoursePrefix,
                        CollegeCode = course.CollegeCode ?? collegeCode,
                        PresentIntake = course.CollegeIntake,
                        SanctionedIntake = course.RguhsIntake,
                        TypeOfAffiliation = _userContext.TypeOfAffiliation.ToString(),
                        Gokdate = course.DateofGOK,
                        FacultyCode = _userContext.FacultyId.ToString(),
                        AcademicYear = course.AcademicYear,
                        DocumentofGokpath = path
                    };

                    _context.AffiliationPgSsCourseDetailsForGoks.Add(entity);
                }
                else
                {
                    existingCourse.SanctionedIntake = course.RguhsIntake;
                    existingCourse.AcademicYear = course.AcademicYear;
                    existingCourse.Gokdate = course.DateofGOK;

                    if (path != null)
                    {
                        if (!string.IsNullOrEmpty(existingCourse.DocumentofGokpath) &&
                            System.IO.File.Exists(existingCourse.DocumentofGokpath))
                        {
                            System.IO.File.Delete(existingCourse.DocumentofGokpath);
                        }

                        existingCourse.DocumentofGokpath = path;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["GokSavemsg"] = "GOK Details saved successfully";
            return RedirectToAction(nameof(PgCourses));
        }
        public async Task<IActionResult> ViewGokDocument(string courseCode, string collegecode)
        {
            var course = await _context.AffiliationPgSsCourseDetailsForGoks
                .FirstOrDefaultAsync(e => e.CollegeCode == collegecode && e.CourseCode == courseCode);

            if (course == null ||
                string.IsNullOrEmpty(course.DocumentofGokpath) ||
                !System.IO.File.Exists(course.DocumentofGokpath))
                return NotFound();

            Response.Headers["Content-Disposition"] = "inline";

            return PhysicalFile(course.DocumentofGokpath, "application/pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePgCoursesRguhs(AffiliationPgCourseViewModel model)
        {
            var collegecode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var afftype = _userContext.TypeOfAffiliation;

            foreach (var course in model.PgCoursesRguhs)
            {
                if (string.IsNullOrWhiteSpace(course.CourseCode))
                    continue;

                if (course.RGUHSDocumentFile == null || course.RGUHSDocumentFile.Length == 0)
                    continue;

                var existing = await _context.AffiliationPgSsCourseDetailsRguhs
                    .FirstOrDefaultAsync(e => e.CourseCode == course.CourseCode && e.CollegeCode == collegecode);

                var path = await SavePgFileAsync(course.RGUHSDocumentFile, "RGUHS");

                if (existing == null)
                {
                    // ✅ INSERT
                    var entity = new AffiliationPgSsCourseDetailsRguh
                    {
                        CollegeCode = collegecode,
                        FacultyCode = facultyCode.ToString(),
                        TypeOfAffiliation = afftype.ToString(),
                        CourseCode = course.CourseCode,
                        CourseLevel = course.CourseLevel,
                        CourseName = course.CourseName,
                        RguhsIntake = course.RguhsIntake,
                        RguhssupportingDocumentPath = path // ✅ save first time
                    };

                    _context.AffiliationPgSsCourseDetailsRguhs.Add(entity);
                }
                else
                {
                    // ✅ UPDATE
                    existing.RguhsIntake = course.RguhsIntake;

                    if (path != null)
                    {
                        // 🔥 DELETE OLD FILE
                        if (!string.IsNullOrEmpty(existing.RguhssupportingDocumentPath) &&
                            System.IO.File.Exists(existing.RguhssupportingDocumentPath))
                        {
                            System.IO.File.Delete(existing.RguhssupportingDocumentPath);
                        }

                        // ✅ UPDATE NEW FILE
                        existing.RguhssupportingDocumentPath = path;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Rguhs"] = "RGUHS Intake details saved successfully";
            return RedirectToAction(nameof(PgCourses));
        }

        public async Task<IActionResult> ViewRguhsDocument(string courseCode)
        {
            var collegecode = _userContext.CollegeCode;

            var course = await _context.AffiliationPgSsCourseDetailsRguhs
                .FirstOrDefaultAsync(e => e.CollegeCode == collegecode && e.CourseCode == courseCode);

            if (course == null ||
                string.IsNullOrEmpty(course.RguhssupportingDocumentPath) ||
                !System.IO.File.Exists(course.RguhssupportingDocumentPath))
                return NotFound("File not found");

            Response.Headers["Content-Disposition"] = "inline";

            return PhysicalFile(course.RguhssupportingDocumentPath, "application/pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherDeptCourses(AffiliationPgCourseViewModel model)
        {
            var collegecode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var afftype = _userContext.TypeOfAffiliation;

            foreach (var course in model.OtherCoursesPermittedByNMC)
            {
                var existingOtherDeptCourse = await _context.AffiliationOtherCoursesPermittedByNmcs
                    .FirstOrDefaultAsync(e => e.CollegeCode == collegecode && e.CourseCode == course.CourseCode);

                var path = await SavePgFileAsync(course.NMCdocumentFile, "NMC");

                if (existingOtherDeptCourse == null)
                {
                    // ✅ INSERT
                    if (course.NMCdocumentFile == null || course.NMCdocumentFile.Length == 0)
                        continue;

                    const long maxSize = 1 * 1024 * 1024; // 1 MB

                    if (course.NMCdocumentFile.Length > maxSize)
                    {
                        ModelState.AddModelError("", "NMC document must be 1 MB or less.");
                        return RedirectToAction(nameof(PgCourses));
                    }

                    var entity = new AffiliationOtherCoursesPermittedByNmc
                    {
                        CollegeCode = collegecode,
                        CourseCode = course.CourseCode,
                        TypeOfAffiliation = afftype.ToString(),
                        FacultyCode = facultyCode.ToString(),
                        CourseLevel = course.CourseLevel,
                        CourseName = course.CourseName,
                        PermissionByNmc = course.PermissionByNMC ? 1 : 0,
                        NumberOfAdmissionsPerYear = course.AdmissionsPerYear,
                        NmcsupportingDocumentPath = path // ✅ first time save
                    };

                    _context.AffiliationOtherCoursesPermittedByNmcs.Add(entity);
                }
                else
                {
                    // ✅ UPDATE
                    existingOtherDeptCourse.PermissionByNmc = course.PermissionByNMC ? 1 : 0;
                    existingOtherDeptCourse.NumberOfAdmissionsPerYear = course.AdmissionsPerYear;

                    if (path != null)
                    {
                        // 🔥 DELETE OLD FILE
                        if (!string.IsNullOrEmpty(existingOtherDeptCourse.NmcsupportingDocumentPath) &&
                            System.IO.File.Exists(existingOtherDeptCourse.NmcsupportingDocumentPath))
                        {
                            System.IO.File.Delete(existingOtherDeptCourse.NmcsupportingDocumentPath);
                        }

                        // ✅ UPDATE NEW FILE
                        existingOtherDeptCourse.NmcsupportingDocumentPath = path;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["others"] = "Other Department Admission details saved successfully";
            return RedirectToAction(nameof(PgCourses));
        }
        public async Task<IActionResult> ViewNMCDocument(string courseCode)
        {
            var collegecode = _userContext.CollegeCode;

            var course = await _context.AffiliationOtherCoursesPermittedByNmcs
                .FirstOrDefaultAsync(e => e.CollegeCode == collegecode && e.CourseCode == courseCode);

            if (course == null ||
                string.IsNullOrEmpty(course.NmcsupportingDocumentPath) ||
                !System.IO.File.Exists(course.NmcsupportingDocumentPath))
                return NotFound("File not found");

            Response.Headers["Content-Disposition"] = "inline";

            return PhysicalFile(course.NmcsupportingDocumentPath, "application/pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLICinspectionData(AffiliationPgCourseViewModel model)
        {
            var collegecode = _userContext.CollegeCode;
            var facultycode = _userContext.FacultyId;
            var typeofAff = _userContext.TypeOfAffiliation;

            var existingData = await _context.AffiliationLicinpsections.Where(e => e.CollegeCode == collegecode && e.FacultyCode == facultycode.ToString()).FirstOrDefaultAsync();

            if (existingData == null)
            {
                var entity = new AffiliationLicinpsection
                {
                    CollegeCode = collegecode,
                    FacultyCode = facultycode.ToString(),
                    PreviousInspectionDate = model.LicInspectionVm.PreviousInspectionDate,
                    ActionTaken = model.LicInspectionVm.ActionTaken,
                    TypeOfAffiliation = typeofAff.ToString()
                };

                _context.AffiliationLicinpsections.Add(entity);
            }
            else
            {
                existingData.PreviousInspectionDate = model.LicInspectionVm.PreviousInspectionDate;
                existingData.ActionTaken = model.LicInspectionVm.ActionTaken;
            }

            await _context.SaveChangesAsync();
            TempData["Lic"] = "Lic Details saved successfully";
            return RedirectToAction(nameof(PgCourses));
        }

        private async Task<string?> SavePgFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return null;

            const long maxSize = 5 * 1024 * 1024; // 5 MB

            var extension = Path.GetExtension(file.FileName)
                                .ToLowerInvariant();

            // PDF only
            if (extension != ".pdf")
                throw new Exception("Only PDF files are allowed.");

            // File size check
            if (file.Length > maxSize)
                throw new Exception("File size cannot exceed 5 MB.");

            var path = BaseMedicalPath;

            if (FacultyCode == "2")
                path = BaseDentalPath;

            string basePath =
                Path.Combine(path, "PgCourseDetails", subFolder);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            string fileName =
                $"{Guid.NewGuid()}.pdf";

            string fullPath =
                Path.Combine(basePath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }

        //Ayurveda Methods are as shown below 

        //private async Task<List<PgCourseVm>> GetAyurvedaDegreeCourses()
        //{
        //    const int ayurvedaFacultyCode = 4;

        //    var collegeCode = _userContext.CollegeCode;

        //    var courses = await (
        //        from ci in _context.MstMedicalCollegeCourseIntakes
        //        join mc in _context.MstCourses
        //            on ci.CourseCode equals mc.CourseCode
        //        where ci.CollCode == collegeCode
        //              && ci.Facultycode == ayurvedaFacultyCode
        //              && ci.UgPg == "PG"  
        //        select new PgCourseVm
        //        {
        //            CourseCode = ci.CourseCode.ToString(),
        //            CourseName = mc.CourseName,
        //            CourseLevel = mc.CourseLevel,
        //            CoursePrefix = mc.CoursePrefix,

        //            // Present Intake
        //            CollegeIntake = ci.Intake2627,

        //            // Existing/RGUHS intake temporarily same as present intake
        //            RguhsIntake = ci.Intake2627
        //        }
        //    )
        //    .ToListAsync();

        //    return courses;
        //}

        private async Task<List<PgCourseVm>> GetAyurvedaDiplomaCourses()
        {
            const int ayurvedaFacultyCode = 4;

            var collegeCode = _userContext.CollegeCode;

            var courses = await (
                from ci in _context.MstMedicalCollegeCourseIntakes
                join mc in _context.MstCourses
                    on ci.CourseCode equals mc.CourseCode
                where ci.CollCode == collegeCode
                      && ci.Facultycode == ayurvedaFacultyCode
                      && ci.UgPg == "PG"
                      && mc.CoursePrefix == "Diploma"
                select new PgCourseVm
                {
                    CourseCode = ci.CourseCode.ToString(),
                    CourseName = mc.CourseName,
                    CourseLevel = mc.CourseLevel,
                    CoursePrefix = mc.CoursePrefix,

                    CollegeIntake = ci.Intake2627,
                    RguhsIntake = ci.Intake2627
                }
            )
            .ToListAsync();

            return courses;
        }

        private async Task<List<PgCourseParticularsVm>> GetAyurvedaPgCoursesParticulars()
        {
            const int ayurvedaFacultyCode = 4;

            var collegeCode = _userContext.CollegeCode;

            var existingData = await _context.AffiliationPgSsCourseDetails
        .Where(e =>
            e.CollegeCode == collegeCode &&
            e.FacultyCode == ayurvedaFacultyCode.ToString())
        .ToDictionaryAsync(e => e.CourseCode);

            var courses = await (
                from ci in _context.MstMedicalCollegeCourseIntakes
                join mc in _context.MstCourses
                    on ci.CourseCode equals mc.CourseCode
                where ci.CollCode == collegeCode
                      && ci.Facultycode == ayurvedaFacultyCode
                      && ci.UgPg == "PG"
                select new PgCourseVm
                {
                    CourseCode = ci.CourseCode.ToString(),
                    CourseName = mc.CourseName,
                    CourseLevel = mc.CourseLevel,
                    CoursePrefix = mc.CoursePrefix,

                    CollegeIntake = ci.Intake2627,
                    RguhsIntake = ci.Intake2627
                }
            )
            .ToListAsync();

            var result = new List<PgCourseParticularsVm>();

            foreach (var course in courses)
            {
                if (existingData.TryGetValue(course.CourseCode, out var existing))
                {
                    result.Add(new PgCourseParticularsVm
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,
                        CoursePrefix = course.CoursePrefix,

                        CollegeIntake = course.CollegeIntake,
                        RguhsIntake = course.RguhsIntake,

                        DateofLOP = existing.Lopdate,

                        // Ayurveda does not use NMC/DCI recognition fields
                        DateofRecognitionByNMC = null,
                        DateofRecognitionByDCI = null
                    });
                }
                else
                {
                    result.Add(new PgCourseParticularsVm
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,
                        CoursePrefix = course.CoursePrefix,

                        CollegeIntake = course.CollegeIntake,
                        RguhsIntake = course.RguhsIntake
                    });
                }
            }

            return result
                .OrderByDescending(x => x.DateofLOP.HasValue)
                .ToList();
        }

        public async Task<IActionResult> PgCoursesAyurveda(
     string? typeOfAffiliation,
     string? courseLevel)
        {
            // Keep the selected course level in session
            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                HttpContext.Session.SetString(
                    "CourseLevel",
                    courseLevel.Trim().ToUpperInvariant()
                );
            }

            // Keep affiliation type in session
            if (!string.IsNullOrWhiteSpace(typeOfAffiliation))
            {
                HttpContext.Session.SetString(
                    "TypeOfAffiliation",
                    typeOfAffiliation
                );
            }

            var collegeCode = _userContext.CollegeCode;

            var degreeCourses = await GetAyurvedaDegreeCourses();
            var diplomaCourses = await GetAyurvedaDiplomaCourses();
            var pgParticularsList = await GetAyurvedaPgCoursesParticulars();

            var pgParticulars = pgParticularsList
                .ToDictionary(x => x.CourseCode);

            var allCourses = pgParticularsList
           .Select(c => new PgCourseParticularsVm
           {
               CourseCode = c.CourseCode,
               CourseName = c.CourseName,
               CourseLevel = c.CourseLevel,
               CoursePrefix = c.CoursePrefix,
               CollegeIntake = c.CollegeIntake,
               RguhsIntake = c.RguhsIntake,
               DateofLOP = c.DateofLOP,
               DateofRecognitionByNMC = null,
               DateofRecognitionByDCI = null
           })
           .ToList();

            // Temporary - these are the existing common methods.
            // We will replace them with Ayurveda-specific methods below.
            var gokData = await GetAyurvedaPgCoursesForGOK();

            var rguhsData =
                await GetAyurvedaPgCoursesWithRguhsPermission();

            var result = new AffiliationPgCourseViewModel
            {
                CollegeCode = collegeCode,
                PgDegreeCourses = degreeCourses,
                PgDiplomaCourses = diplomaCourses,
                AllCourses = allCourses,
                PgCoursesGOK = gokData,
                TypeOfAffiliation = _userContext.TypeOfAffiliation,
                PgCoursesRguhs = rguhsData
            };

            return View(result);
        }

        private async Task<string?> SaveAyurvedaPgFileAsync(
    IFormFile? file,
    string collegeCode,
    string courseCode,
    string documentType)
        {
            if (file == null || file.Length == 0)
                return null;

            const long maxSize = 5 * 1024 * 1024;

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (extension != ".pdf")
                throw new Exception("Only PDF files are allowed.");

            if (file.Length > maxSize)
                throw new Exception("File size cannot exceed 5 MB.");

            string rootPath;

            if (Directory.Exists(@"D:\"))
            {
                rootPath = @"D:\COA";
            }
            else if (Directory.Exists(@"E:\"))
            {
                rootPath = @"E:\COA";
            }
            else
            {
                throw new DirectoryNotFoundException(
                    "Neither D: nor E: drive is available.");
            }

            string folderPath = Path.Combine(
                rootPath,
                "Ayurveda",
                "PG",
                "PGCourseDetails",
                collegeCode,
                courseCode,
                documentType
            );

            Directory.CreateDirectory(folderPath);

            string fileName = $"{Guid.NewGuid()}.pdf";

            string fullPath = Path.Combine(
                folderPath,
                fileName
            );

            await using var stream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            await file.CopyToAsync(stream);

            return fullPath;
        }


        private async Task<List<PgCoursesGokVM>> GetAyurvedaPgCoursesForGOK()
        {
            const int ayurvedaFacultyCode = 4;

            var collegeCode = _userContext.CollegeCode;

            var query =
                from ci in _context.MstMedicalCollegeCourseIntakes

                join cm in _context.MstCourses
                    on ci.CourseCode equals cm.CourseCode

                where ci.CollCode == collegeCode
                      && ci.Facultycode == ayurvedaFacultyCode
                      && ci.UgPg == "PG"

                join gok in _context.AffiliationPgSsCourseDetailsForGoks
                    .Where(e => e.CollegeCode == collegeCode)
                    on ci.CourseCode.ToString() equals gok.CourseCode into gokGroup

                from gok in gokGroup.DefaultIfEmpty()

                select new PgCoursesGokVM
                {
                    CollegeCode = collegeCode,

                    CourseCode = ci.CourseCode.ToString(),

                    CourseName = cm.CourseName,

                    CourseLevel = cm.CourseLevel,

                    CoursePrefix = cm.CoursePrefix,

                    CollegeIntake = gok != null
                        ? gok.PresentIntake
                        : ci.Intake2627,

                    RguhsIntake = gok != null
                        ? gok.PresentIntake
                        : ci.Intake2627,

                    HasGOKDocument =
                        gok != null &&
                        gok.DocumentofGokpath != null &&
                        gok.DocumentofGokpath.Length > 0,

                    AcademicYear = gok != null
                        ? gok.AcademicYear
                        : null,

                    DateofGOK = gok != null
                        ? gok.Gokdate
                        : null
                };

            return await query.ToListAsync();
        }

        private async Task<List<PgCoursesWithRGUHSPermission>>
        GetAyurvedaPgCoursesWithRguhsPermission()
        {
            const int ayurvedaFacultyCode = 4;

            var collegeCode = _userContext.CollegeCode;

            var query =
                from ci in _context.MstMedicalCollegeCourseIntakes

                join mst in _context.MstCourses
                    on ci.CourseCode equals mst.CourseCode

                where ci.CollCode == collegeCode
                      && ci.Facultycode == ayurvedaFacultyCode
                      && ci.UgPg == "PG"

                join rguhs in _context.AffiliationPgSsCourseDetailsRguhs
                    .Where(e => e.CollegeCode == collegeCode)
                    on ci.CourseCode.ToString() equals rguhs.CourseCode
                    into rguhsGroup

                from rguhs in rguhsGroup.DefaultIfEmpty()

                select new PgCoursesWithRGUHSPermission
                {
                    CollegeCode = collegeCode,

                    CourseCode = ci.CourseCode.ToString(),

                    CourseName = mst.CourseName,

                    CourseLevel = mst.CourseLevel,

                    CoursePrefix = mst.CoursePrefix,

                    RguhsIntake = rguhs != null
                        ? rguhs.RguhsIntake
                        : ci.Intake2627,

                    HasRguhsDocument =
                        rguhs != null &&
                        rguhs.RguhssupportingDocumentPath != null &&
                        rguhs.RguhssupportingDocumentPath.Length > 0
                };

            return await query.ToListAsync();
        }

        private async Task<List<PgCourseVm>> GetAyurvedaDegreeCourses()
        {
            const int ayurvedaFacultyCode = 4;

            var collegeCode = _userContext.CollegeCode;

            var courses = await (
                from ci in _context.MstMedicalCollegeCourseIntakes
                join mc in _context.MstCourses
                    on ci.CourseCode equals mc.CourseCode
                where ci.CollCode == collegeCode
                      && ci.Facultycode == ayurvedaFacultyCode
                      && ci.UgPg == "PG"
                      && mc.CoursePrefix != "Diploma"
                select new PgCourseVm
                {
                    CourseCode = ci.CourseCode.ToString(),
                    CourseName = mc.CourseName,
                    CourseLevel = mc.CourseLevel,
                    CoursePrefix = mc.CoursePrefix,
                    CollegeIntake = ci.Intake2627,
                    RguhsIntake = ci.Intake2627
                }
            )
            .ToListAsync();

            return courses;
        }



    }
}
