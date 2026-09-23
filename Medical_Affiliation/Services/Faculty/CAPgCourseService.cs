using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAPgCourseService : ICAPgCourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAPgCourseService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        // Move all methods from the controller here
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

            var facultyCode = _userContext.FacultyId.ToString();

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
                        DateofRecognitionByNMC = facultyCode == "1"
                            ? existing.DateofRecognitionByNmc
                            : null,

                        DateofRecognitionByDCI = facultyCode == "2"
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

        public async Task<AffiliationPgCourseDisplayVM> GetPgCourseDetailsAsync()
        {
            var collegeCode = _userContext.CollegeCode;

            var degreeCourses = await GetDegreeCourses();
            var diplomaCourses = await GetDiplomaCourses();

            // Overlay course particulars
            var pgParticulars = (await GetPgCoursesParticulars())
                .ToDictionary(x => x.CourseCode);

            var allCourses = degreeCourses
                .Concat(diplomaCourses)
                .Select(c =>
                {
                    pgParticulars.TryGetValue(c.CourseCode, out var p);

                    return new PgCourseParticularsDisplayVM
                    {
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        CourseLevel = c.CourseLevel,
                        CoursePrefix = c.CoursePrefix,
                        CollegeIntake = c.CollegeIntake,
                        RguhsIntake = c.RguhsIntake,

                        DateOfLOP = p?.DateofLOP,
                        DateOfRecognitionByNMC = p?.DateofRecognitionByNMC,
                        DateOfRecognitionByDCI = p?.DateofRecognitionByDCI
                    };
                })
                .ToList();

            var gokCourses = (await GetPgCoursesForGOK())
                .Select(x => new PgCoursesGokDisplayVM
                {
                    CourseCode = x.CourseCode,
                    CourseName = x.CourseName,
                    CourseLevel = x.CourseLevel,
                    CoursePrefix = x.CoursePrefix,
                    CollegeIntake = x.CollegeIntake,
                    RguhsIntake = x.RguhsIntake,
                    DateOfGOK = x.DateofGOK,
                    AcademicYear = x.AcademicYear,
                    HasGOKDocument = x.HasGOKDocument
                })
                .ToList();

            var rguhsCourses = (await GetPgCoursesWithRguhsPermission())
                .Select(x => new PgCoursesRguhsDisplayVM
                {
                    CourseCode = x.CourseCode,
                    CourseName = x.CourseName,
                    CourseLevel = x.CourseLevel,
                    CoursePrefix = x.CoursePrefix,
                    CollegeIntake = x.CollegeIntake,
                    RguhsIntake = x.RguhsIntake,
                    HasRguhsDocument = x.HasRguhsDocument
                })
                .ToList();

            var otherCourses = (await GetOtherDeptCoursesPermittedByNmc())
                .Select(x => new OtherCoursesPermittedByNmcDisplayVM
                {
                    CourseCode = x.CourseCode,
                    CourseName = x.CourseName,
                    CourseLevel = x.CourseLevel,
                    CoursePrefix = x.CoursePrefix,
                    CollegeIntake = x.CollegeIntake,
                    RguhsIntake = x.RguhsIntake,
                    PermissionByNMC = x.PermissionByNMC,
                    HasNMCDocument = x.HasNMCdocument,
                    AdmissionsPerYear = x.AdmissionsPerYear
                })
                .ToList();

            var lic = await GetLicInspectionDetails();

            return new AffiliationPgCourseDisplayVM
            {
                CollegeCode = collegeCode,
                TypeOfAffiliation = _userContext.TypeOfAffiliation,

                PgDegreeCourses = degreeCourses
                    .Select(x => new PgCourseDisplayVM
                    {
                        CourseCode = x.CourseCode,
                        CourseName = x.CourseName,
                        CourseLevel = x.CourseLevel,
                        CoursePrefix = x.CoursePrefix,
                        CollegeIntake = x.CollegeIntake,
                        RguhsIntake = x.RguhsIntake
                    })
                    .ToList(),

                PgDiplomaCourses = diplomaCourses
                    .Select(x => new PgCourseDisplayVM
                    {
                        CourseCode = x.CourseCode,
                        CourseName = x.CourseName,
                        CourseLevel = x.CourseLevel,
                        CoursePrefix = x.CoursePrefix,
                        CollegeIntake = x.CollegeIntake,
                        RguhsIntake = x.RguhsIntake
                    })
                    .ToList(),

                AllCourses = allCourses,
                PgCoursesGOK = gokCourses,
                PgCoursesRguhs = rguhsCourses,
                OtherCoursesPermittedByNMC = otherCourses,

                LicInspection = lic == null ? null : new LicInspectionDisplayVM
                {
                    InspectionDate = lic.PreviousInspectionDate,
                    Remarks = lic.ActionTaken,
                    //HasInspectionReport = lic.HasInspectionReport
                }
            };
        }
    }
}
