using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{

    //[Authorize(AuthenticationSchemes = "CollegeAuth", Roles = "College")]
    public class AffiliationPgCourseController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public AffiliationPgCourseController(ApplicationDbContext context, IUserContext userContext)
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
                        DateofRecognitionByNMC = p?.DateofRecognitionByNMC
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
                        DateofRecognitionByNMC = existing.DateofRecognitionByNmc,
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
                    HasGOKDocument = gok != null && gok.DocumentofGok != null && gok.DocumentofGok.Length > 0,
                    AcademicYear = gok != null ? gok.AcademicYear : null,
                    DateofGOK = gok != null ? gok.Gokdate : null,


                };

            var result = await pgCoursesQuery.ToListAsync();
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
                    HasRguhsDocument = rguhsCourses != null && rguhsCourses.RguhssupportingDocument != null && rguhsCourses.RguhssupportingDocument.Length > 0,
                };

            var result = await pgCourseswithRguhs.ToListAsync();
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
                    HasNMCdocument = ot.NmcsupportingDocument != null && ot.NmcsupportingDocument.Length > 0,
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
                if (course.DateofLOP == null && course.DateofRecognitionByNMC == null)
                    continue;

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
                        DateofRecognitionByNmc = course.DateofRecognitionByNMC
                    });
                }
                else
                {
                    // UPDATE
                    existing.Lopdate = course.DateofLOP;
                    existing.DateofRecognitionByNmc = course.DateofRecognitionByNMC;
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
                {
                    continue;
                }

                var existingCourse = await _context.AffiliationPgSsCourseDetailsForGoks.Where(e => e.CollegeCode == course.CollegeCode && e.CourseCode == course.CourseCode).FirstOrDefaultAsync();
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

                    };

                    if (course.GOKDocumentFile != null && course.GOKDocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await course.GOKDocumentFile.CopyToAsync(ms);
                        entity.DocumentofGok = ms.ToArray();
                    }

                    _context.AffiliationPgSsCourseDetailsForGoks.Add(entity);
                }
                else
                {
                    existingCourse.SanctionedIntake = course.RguhsIntake;
                    existingCourse.AcademicYear = course.AcademicYear;

                    if (course.GOKDocumentFile != null && course.GOKDocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await course.GOKDocumentFile.CopyToAsync(ms);
                        existingCourse.DocumentofGok = ms.ToArray();
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["GokSavemsg"] = "Details saved successfully";
            return RedirectToAction(nameof(PgCourses));
        }

        public async Task<IActionResult> ViewGokDocument(string courseCode, string collegecode)
        {
            var course = await _context.AffiliationPgSsCourseDetailsForGoks
                .FirstOrDefaultAsync(e => e.CollegeCode == collegecode && e.CourseCode == courseCode);

            if (course == null || course.DocumentofGok == null)
                return NotFound();

            return File(course.DocumentofGok, "application/pdf");
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
                {
                    continue;
                }
                if (course.RGUHSDocumentFile == null || course.RGUHSDocumentFile.Length == 0)
                    continue;

                const long maxSize = 1 * 1024 * 1024; // 1 MB

                if (course.RGUHSDocumentFile != null &&
                    course.RGUHSDocumentFile.Length > maxSize)
                {
                    ModelState.AddModelError("", "RGUHS document must be 1 MB or less.");
                    return RedirectToAction(nameof(PgCourses));
                }

                var existing = await _context.AffiliationPgSsCourseDetailsRguhs.Where(e => e.CourseCode == course.CourseCode && e.CollegeCode == collegecode).FirstOrDefaultAsync();
                if (existing == null)
                {
                    var entity = new AffiliationPgSsCourseDetailsRguh
                    {
                        CollegeCode = collegecode,
                        FacultyCode = facultyCode.ToString(),
                        TypeOfAffiliation = afftype.ToString(),
                        CourseCode = course.CourseCode,
                        CourseLevel = course.CourseLevel,
                        CourseName = course.CourseName,
                        RguhsIntake = course.RguhsIntake,
                    };

                    if (course.RGUHSDocumentFile != null && course.RGUHSDocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await course.RGUHSDocumentFile.CopyToAsync(ms);
                        entity.RguhssupportingDocument = ms.ToArray();
                    }
                    _context.AffiliationPgSsCourseDetailsRguhs.Add(entity);
                }
                else
                {
                    existing.RguhsIntake = course.RguhsIntake;
                    if (course.RGUHSDocumentFile != null && course.RGUHSDocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await course.RGUHSDocumentFile.CopyToAsync(ms);
                        existing.RguhssupportingDocument = ms.ToArray();
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

            if (course == null || course.RguhssupportingDocument == null)
                return NotFound();

            return File(course.RguhssupportingDocument, "application/pdf");
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
                var existingOtherDeptCourse = await _context.AffiliationOtherCoursesPermittedByNmcs.Where(e => e.CollegeCode == collegecode && e.CourseCode == course.CourseCode).FirstOrDefaultAsync();
                if (existingOtherDeptCourse == null)
                {
                    if (course.NMCdocumentFile == null || course.NMCdocumentFile.Length == 0)
                        continue;

                    const long maxSize = 1 * 1024 * 1024; // 1 MB

                    if (course.NMCdocumentFile != null &&
                        course.NMCdocumentFile.Length > maxSize)
                    {
                        ModelState.AddModelError("", "RGUHS document must be 1 MB or less.");
                        return RedirectToAction(nameof(PgCourses));
                    }
                    var entity = new AffiliationOtherCoursesPermittedByNmc
                    {
                        CollegeCode = collegecode,
                        CourseCode = course.CourseCode,
                        TypeOfAffiliation = afftype.ToString(),
                        FacultyCode = course.FacultyCode,
                        CourseLevel = course.CourseLevel,
                        CourseName = course.CourseName,
                        PermissionByNmc = course.PermissionByNMC ? 1 : 0,
                        NumberOfAdmissionsPerYear = course.AdmissionsPerYear,

                    };
                    if (course.NMCdocumentFile != null && course.NMCdocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await course.NMCdocumentFile.CopyToAsync(ms);
                        entity.NmcsupportingDocument = ms.ToArray();
                    }
                    _context.AffiliationOtherCoursesPermittedByNmcs.Add(entity);

                }
                else
                {
                    existingOtherDeptCourse.PermissionByNmc = course.PermissionByNMC ? 1 : 0;
                    existingOtherDeptCourse.NumberOfAdmissionsPerYear = course.AdmissionsPerYear;
                    if (course.NMCdocumentFile != null && course.NMCdocumentFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await course.NMCdocumentFile.CopyToAsync(ms);
                        existingOtherDeptCourse.NmcsupportingDocument = ms.ToArray();
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

            if (course == null || course.NmcsupportingDocument == null)
                return NotFound();

            return File(course.NmcsupportingDocument, "application/pdf");
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

    }
}
