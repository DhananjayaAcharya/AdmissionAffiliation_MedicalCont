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

        // ug_pg values used in Mst_MedicalCollegeCourseIntake
        private const string LevelUG = "UG";
        private const string LevelPG = "PG";

        // Mst_MedicalCollegeCourseIntake has no course-prefix column, so the prefix is derived
        // from the course name (see IsDiplomaCourse). Change that one method if the rule changes.
        private const string DiplomaPrefix = "Diploma";
        private const string DegreePrefix = "Degree";

        // Per-request cache: every section of the page reads the same intake rows
        private List<PgCourseVm>? _collegeCourses;

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

        // ---------------------------------------------------------------
        // Single source of course data: Mst_MedicalCollegeCourseIntake
        // (coll_code + Facultycode of the logged-in college; ug_pg = course level)
        // ---------------------------------------------------------------
        // Course level chosen in the sidebar (stored in session by SetCourseLevel): "PG" or "SS".
        // This page is never used for UG, so UG or an empty value falls back to PG.
        private string CurrentCourseLevel()
        {
            var level = (HttpContext.Session.GetString("CourseLevel")
                         ?? HttpContext.Session.GetString("SelectedCourseLevel")
                         ?? "").Trim().ToUpperInvariant();

            return string.IsNullOrEmpty(level) || level == LevelUG ? LevelPG : level;
        }

        private static bool IsDiplomaCourse(string? courseName) =>
            !string.IsNullOrWhiteSpace(courseName) &&
            courseName.Contains("Diploma", StringComparison.OrdinalIgnoreCase);

        private async Task<List<PgCourseVm>> GetCollegeCoursesAsync()
        {
            if (_collegeCourses != null)
                return _collegeCourses;

            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var level = CurrentCourseLevel();

            var rows = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(i => i.CollCode == collegeCode
                         && i.Facultycode == facultyCode
                         && i.CourseCode != null
                         && i.UgPg != null
                         && i.UgPg.Trim().ToUpper() == level)
                .Select(i => new
                {
                    i.CourseCode,
                    i.Course,
                    i.UgPg,
                    i.Intake2627,
                    i.Intake2025
                })
                .ToListAsync();

            _collegeCourses = rows
                .GroupBy(r => r.CourseCode)          // one row per course code
                .Select(g => g.First())
                .Select(r => new PgCourseVm
                {
                    CourseCode = r.CourseCode?.ToString() ?? "",
                    CourseName = r.Course,
                    CourseLevel = (r.UgPg ?? "").Trim().ToUpperInvariant(),
                    CoursePrefix = IsDiplomaCourse(r.Course) ? DiplomaPrefix : DegreePrefix,
                    CollegeIntake = r.Intake2627 ?? 0,   // intake applied for 2026-27
                    RguhsIntake = r.Intake2025 ?? 0      // existing (sanctioned) intake
                })
                .OrderBy(c => c.CourseName)
                .ToList();

            return _collegeCourses;
        }

        public async Task<List<PgCourseVm>> GetDegreeCourses()
        {
            var courses = await GetCollegeCoursesAsync();

            return courses
                .Where(c => c.CoursePrefix != DiplomaPrefix)
                .ToList();
        }

        public async Task<List<PgCourseVm>> GetDiplomaCourses()
        {
            var courses = await GetCollegeCoursesAsync();

            return courses
                .Where(c => c.CoursePrefix == DiplomaPrefix)
                .ToList();
        }

        public async Task<List<PgCourseParticularsVm>> GetPgCoursesParticulars()
        {
            var collegeCode = _userContext.CollegeCode;

            // 1️⃣ Existing affiliation data (may be empty)
            var existingData = await _context.AffiliationPgSsCourseDetails
                .Where(e => e.CollegeCode == collegeCode)
                .ToDictionaryAsync(e => e.CourseCode);

            // 2️⃣ All courses of the college (from the intake table)
            var allCourses = await GetCollegeCoursesAsync();

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

            // Courses of the college at the selected level, from the intake table
            var pgCourses = (await GetCollegeCoursesAsync()).ToList();

            // Saved GOK rows for this college, keyed by course code
            var gokRows = (await _context.AffiliationPgSsCourseDetailsForGoks
                    .AsNoTracking()
                    .Where(e => e.CollegeCode == collegeCode)
                    .ToListAsync())
                .Where(g => g.CourseCode != null)
                .GroupBy(g => g.CourseCode)
                .ToDictionary(g => g.Key!, g => g.First());

            var result = pgCourses
                .Select(ci =>
                {
                    gokRows.TryGetValue(ci.CourseCode, out var gok);

                    return new PgCoursesGokVM
                    {
                        CollegeCode = collegeCode,
                        CourseCode = gok != null ? gok.CourseCode : ci.CourseCode,
                        CourseName = gok != null ? gok.CourseName : ci.CourseName,
                        CourseLevel = gok != null ? gok.CourseLevel : ci.CourseLevel,
                        CoursePrefix = gok != null ? gok.CoursePrefix : ci.CoursePrefix,
                        CollegeIntake = gok != null ? gok.PresentIntake : ci.CollegeIntake,
                        RguhsIntake = gok != null ? gok.PresentIntake : ci.RguhsIntake,
                        HasGOKDocument = gok != null && gok.DocumentofGokpath != null && gok.DocumentofGokpath.Length > 0,
                        AcademicYear = gok != null ? gok.AcademicYear : null,
                        DateofGOK = gok != null ? gok.Gokdate : null
                    };
                })
                .ToList();

            return result;
        }


        public async Task<List<PgCoursesWithRGUHSPermission>> GetPgCoursesWithRguhsPermission()
        {
            var collegeCode = _userContext.CollegeCode;

            // Courses of the college at the selected level, from the intake table
            var pgCourses = (await GetCollegeCoursesAsync()).ToList();

            // Saved RGUHS rows for this college, keyed by course code
            var rguhsRows = (await _context.AffiliationPgSsCourseDetailsRguhs
                    .AsNoTracking()
                    .Where(e => e.CollegeCode == collegeCode)
                    .ToListAsync())
                .Where(r => r.CourseCode != null)
                .GroupBy(r => r.CourseCode)
                .ToDictionary(g => g.Key!, g => g.First());

            var result = pgCourses
                .Select(ci =>
                {
                    rguhsRows.TryGetValue(ci.CourseCode, out var rguhsCourses);

                    return new PgCoursesWithRGUHSPermission
                    {
                        CollegeCode = collegeCode,
                        CourseCode = rguhsCourses != null ? rguhsCourses.CourseCode : ci.CourseCode,
                        CourseName = ci.CourseName,
                        CourseLevel = ci.CourseLevel,
                        CoursePrefix = ci.CoursePrefix,
                        RguhsIntake = rguhsCourses != null ? rguhsCourses.RguhsIntake : ci.RguhsIntake,
                        HasRguhsDocument = rguhsCourses != null && rguhsCourses.RguhssupportingDocumentPath != null && rguhsCourses.RguhssupportingDocumentPath.Length > 0,
                    };
                })
                .ToList();

            return result;
        }

        public async Task<List<OtherCoursesPermittedByNMC>> GetOtherDeptCoursesPermittedByNmc()
        {
            var collegeCode = _userContext.CollegeCode;

            // 1) Every PG course in the intake table (any college, any faculty) is a candidate
            var catalogue = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(i => i.CourseCode != null
                         && i.UgPg != null
                         && i.UgPg.Trim().ToUpper() == LevelPG)
                .Select(i => new { i.CourseCode, i.Course, i.Facultycode })
                .Distinct()
                .ToListAsync();

            // 2) Courses this college already runs (any level) are not "other" courses
            var ownCodes = (await _context.MstMedicalCollegeCourseIntakes
                    .AsNoTracking()
                    .Where(i => i.CollCode == collegeCode && i.CourseCode != null)
                    .Select(i => i.CourseCode)
                    .ToListAsync())
                .Select(c => c?.ToString() ?? "")
                .ToHashSet();

            // 3) Rows this college has already saved, keyed by course code
            var nmcRows = (await _context.AffiliationOtherCoursesPermittedByNmcs
                    .AsNoTracking()
                    .Where(e => e.CollegeCode == collegeCode)
                    .ToListAsync())
                .Where(o => o.CourseCode != null)
                .GroupBy(o => o.CourseCode)
                .ToDictionary(g => g.Key!, g => g.First());

            static OtherCoursesPermittedByNMC Build(
                string code, string? name, string facultyCode, AffiliationOtherCoursesPermittedByNmc? ot) =>
                new OtherCoursesPermittedByNMC
                {
                    CourseLevel = LevelPG,
                    CourseCode = code,
                    CourseName = ot?.CourseName ?? name ?? "",
                    PermissionByNMC = ot != null && ot.PermissionByNmc == 1,
                    HasNMCdocument = ot != null && !string.IsNullOrEmpty(ot.NmcsupportingDocumentPath),
                    AdmissionsPerYear = ot?.NumberOfAdmissionsPerYear ?? 0,
                    FacultyCode = ot?.FacultyCode ?? facultyCode
                };

            var result = new Dictionary<string, OtherCoursesPermittedByNMC>();

            foreach (var r in catalogue.GroupBy(x => x.CourseCode).Select(g => g.First()))
            {
                var code = r.CourseCode?.ToString() ?? "";
                nmcRows.TryGetValue(code, out var ot);

                // Skip the college's own courses, unless a row was already saved for them
                if (ownCodes.Contains(code) && ot == null)
                    continue;

                result[code] = Build(code, r.Course, r.Facultycode?.ToString() ?? "", ot);
            }

            // Saved rows must never disappear, even if their course is no longer in the catalogue
            foreach (var kv in nmcRows)
            {
                if (!result.ContainsKey(kv.Key))
                    result[kv.Key] = Build(kv.Key, kv.Value.CourseName, kv.Value.FacultyCode ?? "", kv.Value);
            }

            return result.Values
                .OrderBy(c => c.CourseName)
                .ToList();
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

            // Nothing posted (for example, no row was added): nothing to save
            if (model.OtherCoursesPermittedByNMC == null)
                return RedirectToAction(nameof(PgCourses));

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

    }
}