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
        private readonly ICAPgCourseService _pgCourseService;

        public AffiliationPgCourseController(ApplicationDbContext context, ICAPgCourseService pgCourseService, IUserContext userContext) : base(context)
        {
            _context = context;
            _userContext = userContext;
            _pgCourseService = pgCourseService;
        }
        public async Task<IActionResult> PgCourses()
        {
            var collegeCode = _userContext.CollegeCode;

            var degreeCourses = await _pgCourseService.GetDegreeCourses();
            var diplomaCourses = await _pgCourseService.GetDiplomaCourses();

            // Overlay particulars (existing first)
            var pgParticulars = (await _pgCourseService.GetPgCoursesParticulars()).ToDictionary(x => x.CourseCode);

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

            var gokData = await _pgCourseService.GetPgCoursesForGOK();

            var rguhsData = await _pgCourseService.GetPgCoursesWithRguhsPermission();
            var otherDeptData = await _pgCourseService.GetOtherDeptCoursesPermittedByNmc();
            var licInspectionData = await _pgCourseService.GetLicInspectionDetails();


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
