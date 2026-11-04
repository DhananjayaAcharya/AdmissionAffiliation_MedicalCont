using Admission_Affiliation.Models;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services;

//using Medical_Affiliation.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Medical_Affiliation.Controllers
{
    // Attach this to whichever controller hosts the PG-course pages
    // (e.g. Aff_CA_Med_PGCourseController) - shown standalone here for clarity.
    //
    // NOTE: data is NO LONGER filtered by Application Type (TypeOfAffiliation).
    // Whatever was saved for a college + faculty + course shows on every application type
    // (Continuation of Affiliation, Enhancement of Seats / Increase in Intake, ...).
    //
    // The Application Type is read on the server (session first, posted hidden field as fallback)
    // at SAVE time and written on EVERY save - new rows and existing rows alike -
    // so each row always carries the Application Type of the last save.
    // It is still never used to look rows up.
    public class pgcoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PgCourseCompletionService _completion;

        // Fixed row definitions for section i and j - keeps ordering/labels
        // consistent with the printed Form-B regardless of what's in the DB.
        private static readonly (string Name, int Seq)[] UnitDefinitions = new[]
        {
            ("Unit-I", 1), ("Unit-II", 2), ("Unit-III", 3), ("Unit-IV", 4),
            ("Unit-V", 5), ("Unit-VI", 6), ("Unit-VII", 7), ("Unit-VIII", 8)
        };

        private static readonly string[] IcuTypeDefinitions = new[]
        {
            "Surgical ICU (SICU)",
            "Post. op ward / HDU"
        };

        public pgcoursesController(ApplicationDbContext context, PgCourseCompletionService completion)
        {
            _context = context;
            _completion = completion;
        }

        private string? GetSessionTypeOfAffiliation()
        {
            return HttpContext.Session.GetString("TypeOfAffiliation")
                ?? HttpContext.Session.GetString("SelectedTypeOfAffiliation")
                ?? HttpContext.Session.GetString("AffiliationType")
                ?? HttpContext.Session.GetString("AffiliationTypeId");
        }

        // Application Type is resolved on the server at save time (session first);
        // the posted hidden field is only a fallback when the session value is missing.
        private string? ResolveTypeOfAffiliation(string? posted)
        {
            var fromSession = GetSessionTypeOfAffiliation();
            return !string.IsNullOrWhiteSpace(fromSession) ? fromSession : posted;
        }

        private string? GetSessionCourseLevel()
        {
            return HttpContext.Session.GetString("SelectedCourseLevel")?
                   .Trim().ToUpper()
                ?? HttpContext.Session.GetString("CourseLevel")?
                   .Trim().ToUpper();
        }

        private string? GetSessionCourseCode()
        {
            return HttpContext.Session.GetString("CourseCode");
        }

        private void SetSessionCourseCode(string? courseCode)
        {
            if (!string.IsNullOrWhiteSpace(courseCode))
            {
                HttpContext.Session.SetString("CourseCode", courseCode);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SubjectSelection()
        {
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode")?.Trim();
            var typeOfAffiliation = GetSessionTypeOfAffiliation();

            var courseLevel = (GetSessionCourseLevel() ?? "").Trim().ToUpper();

            if (string.IsNullOrEmpty(facultyCodeString) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(courseLevel))
            {
                ModelState.AddModelError(string.Empty, "Course level is not set in session.");
                return View(new PgCourseSubjectSelectionViewModel());
            }

            var subjects = await _context.MstMedicalCollegeCourseIntakes
                .Where(cc => cc.CollCode == collegeCode
                          && cc.UgPg != null
                          && cc.UgPg.Trim().ToUpper() == courseLevel)
                .Select(cc => new PgCourseSubjectSelectionViewModel.PgCourseSubjectItem
                {
                    CourseCode = cc.CourseCode.ToString(),
                    CourseName = cc.Course,
                    SubjectName = cc.Course
                })
                .Distinct()
                .OrderBy(x => x.CourseName)
                .ToListAsync();

            // Completion is read straight from the saved data (faculty + college + course).
            // Application Type is NOT used here, so it cannot break after logout/login.
            var pendingByCourse = await GetPendingSectionsByCourseAsync(facultyCodeString, collegeCode);

            foreach (var s in subjects)
            {
                var key = (s.CourseCode ?? "").Trim();
                s.PendingSections = pendingByCourse.TryGetValue(key, out var pending)
                    ? pending
                    : new List<string>(SectionNames);   // nothing saved yet = everything pending
                s.IsCompleted = s.PendingSections.Count == 0;
            }

            var model = new PgCourseSubjectSelectionViewModel
            {
                FacultyCode = facultyCodeString,
                CollegeCode = collegeCode,
                CourseLevel = courseLevel,
                TypeOfAffiliation = typeOfAffiliation,
                Subjects = subjects
            };

            return View(model);
        }
        // ---------------------------------------------------------------------------------
        // Completion status, calculated from the saved rows.
        // One table per page is checked: each is written in the same SaveChanges as the rest
        // of that page, so its presence means the page was saved for that course.
        // ---------------------------------------------------------------------------------
        private static readonly string[] SectionNames =
        {
    "General Details", "Summary Details", "Infrastructure", "Services & Workload",
    "Staff Details", "Academic Activities"
};

        private async Task<Dictionary<string, List<string>>> GetPendingSectionsByCourseAsync(string facultyCode, string collegeCode)
        {
            static HashSet<string> ToSet(List<string?> codes) =>
                codes.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c!.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var general = ToSet(await _context.TxnPgcourseGeneralDetails.AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .Select(x => (string?)x.CourseCode).Distinct().ToListAsync());

            var summary = ToSet(await _context.TxnPgcourseSummaryDetails.AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .Select(x => (string?)x.CourseCode).Distinct().ToListAsync());

            var infrastructure = ToSet(await _context.OpdDetails.AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .Select(x => (string?)x.CourseCode).Distinct().ToListAsync());

            var services = ToSet(await _context.ClinicalWorkloadDetails.AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .Select(x => (string?)x.CourseCode).Distinct().ToListAsync());

            var staff = ToSet(await _context.EligibleFacultyDetails.AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .Select(x => (string?)x.CourseCode).Distinct().ToListAsync());

            var academic = ToSet(await _context.AcademicSummaryDetails.AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .Select(x => (string?)x.CourseCode).Distinct().ToListAsync());

            var all = general.Union(summary).Union(infrastructure).Union(services).Union(staff).Union(academic);

            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var course in all)
            {
                var pending = new List<string>();
                if (!general.Contains(course)) pending.Add(SectionNames[0]);
                if (!summary.Contains(course)) pending.Add(SectionNames[1]);
                if (!infrastructure.Contains(course)) pending.Add(SectionNames[2]);
                if (!services.Contains(course)) pending.Add(SectionNames[3]);
                if (!staff.Contains(course)) pending.Add(SectionNames[4]);
                if (!academic.Contains(course)) pending.Add(SectionNames[5]);
                result[course] = pending;
            }
            return result;
        }
        private async Task SetSelectedDepartmentAsync(string collegeCode, string? courseCode)
        {
            ViewBag.DepartmentName = await GetDepartmentNameAsync(collegeCode, courseCode);
            ViewBag.CourseLevel = GetSessionCourseLevel();
            ViewBag.CollegeCodeUsed = collegeCode;
            ViewBag.FacultyCodeUsed = HttpContext.Session.GetString("FacultyCode");
        }

        [HttpGet]
        public async Task<IActionResult> GeneralDetails(string courseCode, string typeOfAffiliation)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            typeOfAffiliation ??= GetSessionTypeOfAffiliation();
            courseCode ??= GetSessionCourseCode();
            SetSessionCourseCode(courseCode);

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return RedirectToAction("SubjectSelection", "pgcourses");
            }

            // selected department for the info bar
            await SetSelectedDepartmentAsync(collegeCode, courseCode);

            var entity = await _context.TxnPgcourseGeneralDetails
                .Include(x => x.TxnPgcourseUnitBedDetails)
                .Include(x => x.TxnPgcourseIcudetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == courseCode);

            var model = new PgCourseGeneralViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            if (entity != null)
            {
                model.PgcourseGeneralDetailId = entity.PgcourseGeneralDetailId;
                model.AcademicYear = entity.AcademicYear;
                model.LoPDate = entity.LoPdate?.ToDateTime(TimeOnly.MinValue);
                model.YearsSinceStart = entity.YearsSinceStart;
                model.HodName = entity.Hodname;
                model.ExistingSeats = entity.ExistingSeats;
                model.IncreaseSeatsFrom = entity.IncreaseSeatsFrom;
                model.IncreaseSeatsTo = entity.IncreaseSeatsTo;
                model.TotalUnits = entity.TotalUnits;
                model.DepartmentBeds = entity.DepartmentBeds;
                model.TotalICUHDUBeds = entity.TotalIcuhdubeds;

                foreach (var (name, seq) in UnitDefinitions)
                {
                    var saved = entity.TxnPgcourseUnitBedDetails.FirstOrDefault(u => u.UnitName == name);
                    model.Units.Add(new UnitBedRowViewModel
                    {
                        UnitName = name,
                        UnitSequence = seq,
                        NumberOfBeds = saved?.NumberOfBeds
                    });
                }

                foreach (var icuType in IcuTypeDefinitions)
                {
                    var saved = entity.TxnPgcourseIcudetails.FirstOrDefault(i => i.Icutype == icuType);
                    model.IcuDetails.Add(new IcuRowViewModel
                    {
                        IcuType = icuType,
                        IsAvailable = saved?.IsAvailable ?? false,
                        TotalBeds = saved?.TotalBeds,
                        BedOccupancyOnInspectionDay = saved?.BedOccupancyOnInspectionDay
                    });
                }
            }
            else
            {
                // Fresh form: seed the fixed rows so the view always has 8 units + 2 ICU rows to render
                foreach (var (name, seq) in UnitDefinitions)
                    model.Units.Add(new UnitBedRowViewModel { UnitName = name, UnitSequence = seq });

                foreach (var icuType in IcuTypeDefinitions)
                    model.IcuDetails.Add(new IcuRowViewModel { IcuType = icuType });
            }

            return View(model);
        }


        // POST: /PGCourseGeneral/GeneralDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneralDetails(PgCourseGeneralViewModel model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            // Application Type fetched at save time and stored on every row written below
            var typeOfAffiliation = ResolveTypeOfAffiliation(model.TypeOfAffiliation);
            model.TypeOfAffiliation = typeOfAffiliation;

            // Checkboxes inside a list don't post when unchecked, so ModelState binding
            // for IsAvailable can't be trusted on its own - cross-check against the raw
            // posted form the same way the checkbox-in-loop bugs elsewhere were fixed.
            for (int i = 0; i < model.IcuDetails.Count; i++)
            {
                var formKey = $"IcuDetails[{i}].IsAvailable";
                model.IcuDetails[i].IsAvailable = Request.Form.ContainsKey(formKey) &&
                    (Request.Form[formKey].Contains("true") || Request.Form[formKey].Contains("on"));
            }

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;

                // the info bar must also show when the page is re-displayed after a validation error
                await SetSelectedDepartmentAsync(collegeCode, model.CourseCode);

                return View("GeneralDetails", model);
            }

            var entity = await _context.TxnPgcourseGeneralDetails
                .Include(x => x.TxnPgcourseUnitBedDetails)
                .Include(x => x.TxnPgcourseIcudetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == model.CourseCode);

            var isNew = entity == null;

            if (isNew)
            {
                entity = new TxnPgcourseGeneralDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.TxnPgcourseGeneralDetails.Add(entity);
            }
            else
            {
                entity.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                entity.ModifiedBy = User.Identity?.Name ?? collegeCode;
                entity.ModifiedDate = DateTime.Now;
            }

            entity.AcademicYear = model.AcademicYear;
            entity.LoPdate = model.LoPDate.HasValue ? DateOnly.FromDateTime(model.LoPDate.Value) : null;
            entity.YearsSinceStart = model.YearsSinceStart;
            entity.Hodname = model.HodName;
            entity.ExistingSeats = model.ExistingSeats;
            entity.IncreaseSeatsFrom = model.IncreaseSeatsFrom;
            entity.IncreaseSeatsTo = model.IncreaseSeatsTo;
            entity.TotalUnits = model.TotalUnits;
            entity.DepartmentBeds = model.DepartmentBeds;
            entity.TotalIcuhdubeds = model.TotalICUHDUBeds;

            // Save first so a new record has an Id to hang the child rows off of
            await _context.SaveChangesAsync();

            if (!isNew)
            {
                if (entity.TxnPgcourseUnitBedDetails.Any())
                    _context.TxnPgcourseUnitBedDetails.RemoveRange(entity.TxnPgcourseUnitBedDetails);

                if (entity.TxnPgcourseIcudetails.Any())
                    _context.TxnPgcourseIcudetails.RemoveRange(entity.TxnPgcourseIcudetails);

                await _context.SaveChangesAsync();
            }

            foreach (var unit in model.Units)
            {
                _context.TxnPgcourseUnitBedDetails.Add(new TxnPgcourseUnitBedDetail
                {
                    PgcourseGeneralDetailId = entity.PgcourseGeneralDetailId,
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    UnitName = unit.UnitName,
                    UnitSequence = unit.UnitSequence,
                    NumberOfBeds = unit.NumberOfBeds,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now
                });
            }

            foreach (var icu in model.IcuDetails)
            {
                _context.TxnPgcourseIcudetails.Add(new TxnPgcourseIcudetail
                {
                    PgcourseGeneralDetailId = entity.PgcourseGeneralDetailId,
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    Icutype = icu.IcuType,
                    IsAvailable = icu.IsAvailable,
                    TotalBeds = icu.TotalBeds,
                    BedOccupancyOnInspectionDay = icu.BedOccupancyOnInspectionDay,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            //TempData["SuccessMessage"] = "General details saved successfully.";
            return RedirectToAction(nameof(SummaryDetails), new { courseCode = model.CourseCode, typeOfAffiliation });
        }

        private static readonly (string EntityType, int Seq)[] EntityDefinitions = new[]
        {
            ("Institution/College", 1),
            ("Director/Dean/Principal", 2),
            ("Medical Superintendent", 3)
        };

        private static readonly (string CourseLevel, int Seq)[] CourseLevelDefinitions = new[]
        {
            ("UG", 1),
            ("PG", 2),
            ("SS", 3)
        };

        private static string? GetInstitutionCategoryForView(string? category)
        {
            return category?.Trim().ToUpperInvariant() switch
            {
                "P" => "P",
                "G" => "G",
                "PRIVATE" => "P",
                "GOVERNMENT" => "G",
                _ => string.IsNullOrWhiteSpace(category) ? null : category.Trim()
            };
        }

        // GET: SummaryDetails?courseCode=..&typeOfAffiliation=..
        [HttpGet]
        public async Task<IActionResult> SummaryDetails(string courseCode, string typeOfAffiliation)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            typeOfAffiliation ??= GetSessionTypeOfAffiliation();
            courseCode ??= GetSessionCourseCode();
            SetSessionCourseCode(courseCode);

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account"); // adjust to your actual login route
            }

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return RedirectToAction("SubjectSelection", "pgcourses");
            }

            // selected department for the info bar
            await SetSelectedDepartmentAsync(collegeCode, courseCode);

            var entity = await _context.TxnPgcourseSummaryDetails
                .Include(x => x.TxnPgcourseSummaryContactDetails)
                .Include(x => x.TxnPgcourseSummaryInspectionDetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == courseCode);

            var model = new PgCourseSummaryViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            if (entity != null)
            {
                model.PgcourseSummaryDetailId = entity.PgcourseSummaryDetailId;
                model.DateOfAssessment = entity.DateOfAssessment?.ToDateTime(TimeOnly.MinValue);
                model.AssessorName = entity.AssessorName;
                model.InstitutionName = entity.InstitutionName;
                model.InstitutionCategory = GetInstitutionCategoryForView(entity.InstitutionCategory);
                model.HeadOfInstitutionDesignation = entity.HeadOfInstitutionDesignation;
                model.HeadOfInstitutionName = entity.HeadOfInstitutionName;
                model.HeadOfInstitutionAgeDob = entity.HeadOfInstitutionAgeDob;
                model.HeadOfInstitutionTeachingExp = entity.HeadOfInstitutionTeachingExp;
                model.HeadOfInstitutionPgdegree = entity.HeadOfInstitutionPgdegree;
                model.HeadOfInstitutionPgrecognition = entity.HeadOfInstitutionPgrecognition;
                model.HeadOfInstitutionSubject = entity.HeadOfInstitutionSubject;
                model.DepartmentInspected = entity.DepartmentInspected;
                model.Hodname = entity.Hodname;
                model.HodageDob = entity.HodageDob;
                model.HodteachingExp = entity.HodteachingExp;
                model.HodpgDegree = entity.HodpgDegree;
                model.HodpgRecognition = entity.HodpgRecognition;
                model.NumberOfUgseats = entity.NumberOfUgseats;

                foreach (var (entityType, seq) in EntityDefinitions)
                {
                    var saved = entity.TxnPgcourseSummaryContactDetails.FirstOrDefault(c => c.EntityType == entityType);
                    model.Contacts.Add(new ContactRowViewModel
                    {
                        EntityType = entityType,
                        EntitySequence = seq,
                        Name = saved?.Name,
                        Address = saved?.Address,
                        State = saved?.State,
                        PinCode = saved?.PinCode,
                        PhoneOffice = saved?.PhoneOffice,
                        PhoneResidence = saved?.PhoneResidence,
                        Fax = saved?.Fax,
                        MobileNo = saved?.MobileNo,
                        Email = saved?.Email
                    });
                }

                foreach (var (courseLevel, seq) in CourseLevelDefinitions)
                {
                    var saved = entity.TxnPgcourseSummaryInspectionDetails.FirstOrDefault(i => i.CourseLevel == courseLevel);
                    model.Inspections.Add(new InspectionRowViewModel
                    {
                        CourseLevel = courseLevel,
                        CourseLevelSequence = seq,
                        DateOfLastInspection = saved?.DateOfLastInspection?.ToDateTime(TimeOnly.MinValue),
                        Purpose = saved?.Purpose,
                        Result = saved?.Result
                    });
                }
            }
            else
            {
                // Fresh form: seed the fixed rows so the view always has 3 contact rows + 3 inspection rows to render
                foreach (var (entityType, seq) in EntityDefinitions)
                    model.Contacts.Add(new ContactRowViewModel { EntityType = entityType, EntitySequence = seq });

                foreach (var (courseLevel, seq) in CourseLevelDefinitions)
                    model.Inspections.Add(new InspectionRowViewModel { CourseLevel = courseLevel, CourseLevelSequence = seq });
            }

            // Institution, head of institution and UG seats always come from the college records,
            // whether the form is new or already saved
            await ApplyMasterDataAsync(model, facultyCode, collegeCode);

            return View(model);
        }

        // POST: SummaryDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryDetails(PgCourseSummaryViewModel model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            // Application Type fetched at save time and stored on every row written below
            var typeOfAffiliation = ResolveTypeOfAffiliation(model.TypeOfAffiliation);
            model.TypeOfAffiliation = typeOfAffiliation;

            // Overwrite the fetched fields with the server's own values, so they cannot be changed from the browser
            await ApplyMasterDataAsync(model, facultyCode, collegeCode);

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;

                // the info bar must also show when the page is re-displayed after a validation error
                await SetSelectedDepartmentAsync(collegeCode, model.CourseCode);

                return View("SummaryDetails", model);
            }

            var entity = await _context.TxnPgcourseSummaryDetails
                .Include(x => x.TxnPgcourseSummaryContactDetails)
                .Include(x => x.TxnPgcourseSummaryInspectionDetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == model.CourseCode);

            var isNew = entity == null;

            if (isNew)
            {
                entity = new TxnPgcourseSummaryDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.TxnPgcourseSummaryDetails.Add(entity);
            }
            else
            {
                entity.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                entity.ModifiedBy = User.Identity?.Name ?? collegeCode;
                entity.ModifiedDate = DateTime.Now;
            }

            // Fallbacks for when the college records had no value (kept from the previous version)
            if (string.IsNullOrWhiteSpace(model.InstitutionCategory))
            {
                var savedInstitution = await _context.AffInstitutionsDetails
                    .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                    .OrderByDescending(x => x.InstitutionId)
                    .FirstOrDefaultAsync();

                if (savedInstitution != null && int.TryParse(savedInstitution.TypeOfInstitution, out var institutionTypeId) && institutionTypeId > 0)
                {
                    var orgCategory = await _context.MstInstitutionTypes
                        .Where(x => x.InstitutionTypeId == institutionTypeId)
                        .Select(x => x.OrganizationCategory)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrWhiteSpace(orgCategory))
                    {
                        model.InstitutionCategory = orgCategory.Trim();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(model.InstitutionName))
            {
                model.InstitutionName = await _context.AffInstitutionsDetails
                    .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                    .OrderByDescending(x => x.InstitutionId)
                    .Select(x => x.NameOfInstitution)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            entity.DateOfAssessment = model.DateOfAssessment.HasValue ? DateOnly.FromDateTime(model.DateOfAssessment.Value) : null;
            entity.AssessorName = model.AssessorName;
            entity.InstitutionName = model.InstitutionName ?? string.Empty;
            entity.InstitutionCategory = model.InstitutionCategory ?? string.Empty;
            entity.HeadOfInstitutionDesignation = model.HeadOfInstitutionDesignation;
            entity.HeadOfInstitutionName = model.HeadOfInstitutionName;
            entity.HeadOfInstitutionAgeDob = model.HeadOfInstitutionAgeDob;
            entity.HeadOfInstitutionTeachingExp = model.HeadOfInstitutionTeachingExp;
            entity.HeadOfInstitutionPgdegree = model.HeadOfInstitutionPgdegree;
            entity.HeadOfInstitutionPgrecognition = model.HeadOfInstitutionPgrecognition;
            entity.HeadOfInstitutionSubject = model.HeadOfInstitutionSubject;
            entity.DepartmentInspected = model.DepartmentInspected;
            entity.Hodname = model.Hodname;
            entity.HodageDob = model.HodageDob;
            entity.HodteachingExp = model.HodteachingExp;
            entity.HodpgDegree = model.HodpgDegree;
            entity.HodpgRecognition = model.HodpgRecognition;
            entity.NumberOfUgseats = model.NumberOfUgseats;

            // Save first so a new record has an Id to hang the child rows off of
            await _context.SaveChangesAsync();

            if (!isNew)
            {
                if (entity.TxnPgcourseSummaryContactDetails.Any())
                    _context.TxnPgcourseSummaryContactDetails.RemoveRange(entity.TxnPgcourseSummaryContactDetails);

                if (entity.TxnPgcourseSummaryInspectionDetails.Any())
                    _context.TxnPgcourseSummaryInspectionDetails.RemoveRange(entity.TxnPgcourseSummaryInspectionDetails);

                await _context.SaveChangesAsync();
            }

            foreach (var contact in model.Contacts)
            {
                _context.TxnPgcourseSummaryContactDetails.Add(new TxnPgcourseSummaryContactDetail
                {
                    PgcourseSummaryDetailId = entity.PgcourseSummaryDetailId,
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    EntityType = contact.EntityType,
                    EntitySequence = contact.EntitySequence,
                    Name = contact.Name,
                    Address = contact.Address,
                    State = contact.State,
                    PinCode = contact.PinCode,
                    PhoneOffice = contact.PhoneOffice,
                    PhoneResidence = contact.PhoneResidence,
                    Fax = contact.Fax,
                    MobileNo = contact.MobileNo,
                    Email = contact.Email,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now
                });
            }

            foreach (var inspection in model.Inspections)
            {
                _context.TxnPgcourseSummaryInspectionDetails.Add(new TxnPgcourseSummaryInspectionDetail
                {
                    PgcourseSummaryDetailId = entity.PgcourseSummaryDetailId,
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CourseLevel = inspection.CourseLevel,
                    CourseLevelSequence = inspection.CourseLevelSequence,
                    DateOfLastInspection = inspection.DateOfLastInspection.HasValue
                        ? DateOnly.FromDateTime(inspection.DateOfLastInspection.Value)
                        : null,
                    Purpose = inspection.Purpose,
                    Result = inspection.Result,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Summary details saved successfully.";
            return RedirectToAction(nameof(InfrastructureDetails), new { courseCode = model.CourseCode, typeOfAffiliation });
        }

        // ---------------------------------------------------------------------------------
        // Fetched fields. Each value that is found sets a ViewBag flag so the view can lock the field.
        // ---------------------------------------------------------------------------------
        private async Task ApplyMasterDataAsync(PgCourseSummaryViewModel model, string facultyCode, string collegeCode)
        {
            var level = (GetSessionCourseLevel() ?? "").Trim().ToUpperInvariant();

            // ---- 1. Name of Institution + Private / Government ----
            var intakeRows = _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(i => i.CollCode == collegeCode);

            var collegeInfo = await intakeRows
                .Select(i => new { i.Collegename, i.PvtGovt })
                .FirstOrDefaultAsync();

            // The name comes from the session; the intake table is only a fallback
            var institutionName = HttpContext.Session.GetString("CollegeName");
            if (string.IsNullOrWhiteSpace(institutionName))
                institutionName = collegeInfo?.Collegename;

            if (!string.IsNullOrWhiteSpace(institutionName))
            {
                model.InstitutionName = institutionName.Trim();
                ModelState.Remove(nameof(model.InstitutionName));   // otherwise the posted value is shown instead
                ViewBag.InstitutionNameFetched = true;
            }

            // PVT_GOVT: PVT = Private ("P"), GOVT = Government ("G"); these are the dropdown's option values
            var category = MapInstitutionCategory(collegeInfo?.PvtGovt);
            if (category != null)
            {
                model.InstitutionCategory = category;
                ModelState.Remove(nameof(model.InstitutionCategory));
                ViewBag.InstitutionCategoryFetched = true;
            }

            // ---- 2. Director / Dean / Principal ----
            // Prefer the row for the course level being filled in (UG / PG / SS), otherwise the latest row
            var dean = await _context.AffDeanOrDirectorDetails
                .AsNoTracking()
                .Where(d => d.FacultyCode == facultyCode && d.CollegeCode == collegeCode)
                .OrderBy(d => d.CourseLevel != null && d.CourseLevel.Trim().ToUpper() == level ? 0 : 1)
                .ThenByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (dean != null)
            {
                if (!string.IsNullOrWhiteSpace(dean.DeanOrDirectorName))
                {
                    model.HeadOfInstitutionName = dean.DeanOrDirectorName.Trim();
                    ModelState.Remove(nameof(model.HeadOfInstitutionName));
                    ViewBag.HeadNameFetched = true;
                }

                if (!string.IsNullOrWhiteSpace(dean.DeanQualification))
                {
                    // Recognised status: MCI (medical) or DCI (dental)
                    var recognised = ParseYesNo(facultyCode == "2" ? dean.RecognizedByDci : dean.RecognizedByMci);
                    var status = recognised switch
                    {
                        true => " (Recognized)",
                        false => " (Non-Recognized)",
                        _ => ""
                    };

                    model.HeadOfInstitutionPgdegree = dean.DeanQualification.Trim() + status;
                    ModelState.Remove(nameof(model.HeadOfInstitutionPgdegree));
                    ViewBag.HeadDegreeFetched = true;
                }
            }

            // ---- 3. (a) Number of UG seats = total of Intake_26_27 over the college's UG courses ----
            var ugRows = intakeRows.Where(i => i.UgPg != null && i.UgPg.Trim().ToUpper() == "UG");
            if (await ugRows.AnyAsync())
            {
                model.NumberOfUgseats = await ugRows.SumAsync(i => i.Intake2627 ?? 0);
                ModelState.Remove(nameof(model.NumberOfUgseats));
                ViewBag.UgSeatsFetched = true;
            }

            // ---- A. Particulars of the Institution/College (column: Institution/College) ----
            await ApplyInstitutionContactAsync(model, facultyCode, collegeCode);
        }

        // Fills the "Institution/College" column of section A from AFF_InstitutionsDetails.
        // Only values that exist are filled; those fields are locked in the view (ViewBag.InstFetched).
        private async Task ApplyInstitutionContactAsync(PgCourseSummaryViewModel model, string facultyCode, string collegeCode)
        {
            if (model.Contacts == null || model.Contacts.Count == 0) return;

            // Find the Institution/College column; fall back to the first column
            var idx = model.Contacts.FindIndex(c =>
                (c.EntityType ?? "").Contains("Institution", StringComparison.OrdinalIgnoreCase) ||
                (c.EntityType ?? "").Contains("College", StringComparison.OrdinalIgnoreCase));
            if (idx < 0) idx = 0;

            var inst = await _context.AffInstitutionsDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode)
                .OrderByDescending(x => x.InstitutionId)
                .FirstOrDefaultAsync();
            if (inst == null) return;

            var row = model.Contacts[idx];
            var fetched = new HashSet<string>();

            void Set(string field, string? value, Action<string> assign)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                assign(value.Trim());
                ModelState.Remove($"Contacts[{idx}].{field}");
                fetched.Add(field);
            }

            var address = string.Join(", ", new[] { inst.Address, inst.VillageTownCity, inst.Taluk, inst.District }
                .Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()));

            var std = (inst.StdCode ?? "").Trim();
            var landline = (inst.AltLandlineMobile ?? "").Trim();
            var office = landline.Length == 0 ? "" : (std.Length > 0 ? $"{std}-{landline}" : landline);

            Set("Name", inst.NameOfInstitution, v => row.Name = v);
            Set("Address", address, v => row.Address = v);
            Set("PinCode", inst.PinCode, v => row.PinCode = v);
            Set("PhoneOffice", office, v => row.PhoneOffice = v);
            Set("Fax", inst.Fax, v => row.Fax = v);
            Set("MobileNo", inst.MobileNumber, v => row.MobileNo = v);
            Set("Email", inst.EmailId, v => row.Email = v);

            ViewBag.InstRowIndex = idx;
            ViewBag.InstFetched = fetched;
        }

        private static string? MapInstitutionCategory(string? pvtGovt)
        {
            var value = (pvtGovt ?? "").Trim().ToUpperInvariant();

            if (value.StartsWith("G")) return "G";   // GOVT
            if (value.StartsWith("P")) return "P";   // PVT
            return null;
        }

        // RecognizedByMCI / RecognizedByDCI may be a bit, a number or Y/N text; this reads all of them
        private static bool? ParseYesNo(object? value)
        {
            switch (value)
            {
                case null:
                    return null;
                case bool b:
                    return b;
                case string s:
                    switch (s.Trim().ToUpperInvariant())
                    {
                        case "Y":
                        case "YES":
                        case "TRUE":
                        case "1":
                        case "RECOGNIZED":
                        case "RECOGNISED":
                            return true;
                        case "N":
                        case "NO":
                        case "FALSE":
                        case "0":
                        case "NON-RECOGNIZED":
                        case "NOT RECOGNIZED":
                            return false;
                        default:
                            return null;
                    }
                case IConvertible c:
                    try { return Convert.ToDecimal(c) != 0; }
                    catch { return null; }
                default:
                    return null;
            }
        }

        // ---------------------------------------------------------------------------------
        // Infrastructure details
        // ---------------------------------------------------------------------------------

        private static readonly string[] OpdRoomTypeDefaults = { "Consultation Rooms", "Demonstration room", "Minor OT" };

        private static readonly string[] WardsParameterDefaults =
        {
            "Distance between two cots (in meter)",
            "Infrastructure and facilities",
            "Dressing and procedure room"
        };

        private static readonly string[] EquipmentDefaults =
        {
            "Upper GI Endoscope set",
            "Lower GI Endoscope set",
            "Laparoscopy equipment set (write total no of functioning sets available with the Department)",
            "Ultrasonic Dissector / Coagulator",
            "Vessel Sealing Equipment",
            "Ultrasonography machine with Doppler facility linear, convex and cardiac probe and puncture guide",
            "Laparoscopy Trainers",
            "Any other equipments"
        };

        private static string? ToDisplayYesNo(string? stored) => NormalizeYesNo(stored);

        private static string? ToStoredYesNo(string? posted) => NormalizeYesNo(posted);

        private static readonly char[] _yesNoSeparators = { ' ', '-', '_', '/', ',', '.', '(', ')' };

        private static string? NormalizeYesNo(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;

            var t = v.Trim().ToLowerInvariant();
            var words = t.Split(_yesNoSeparators, StringSplitOptions.RemoveEmptyEntries);
            bool Has(params string[] candidates) => words.Any(w => candidates.Contains(w));

            // negatives first, so "inadequate" / "not permitted" never fall into the Yes branch
            if (t is "n" or "no" or "false" or "0"
                || Has("not", "non", "no", "inadequate", "unavailable", "absent"))
                return "No";

            if (t is "y" or "yes" or "true" or "1"
                || Has("yes", "permitted", "adequate", "available", "present"))
                return "Yes";

            return null;
        }

        private static string? NormalizeInternet(string? v)       // Seminar: "Available" / "Not Available"
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            return Key(v) switch
            {
                "y" or "yes" or "true" or "1" or "available" => "Available",
                "n" or "no" or "false" or "0" or "notavailable" or "unavailable" => "Not Available",
                _ => v.Trim()
            };
        }

        private static string? NormalizeAdequacy(string? v)       // "Adequate" / "Not Adequate"
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            return Key(v) switch
            {
                "y" or "yes" or "true" or "1" or "adequate" => "Adequate",
                "n" or "no" or "false" or "0" or "notadequate" or "inadequate" => "Not Adequate",
                _ => v.Trim()
            };
        }

        private static string? NormalizeAvailability(string? v)   // OPD: "Available" / "Not available"
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            return Key(v) switch
            {
                "y" or "yes" or "true" or "1" or "available" => "Available",
                "n" or "no" or "false" or "0" or "notavailable" or "unavailable" => "Not available",
                _ => v.Trim()
            };
        }

        // ---- Dropdown helpers: Key() reduces text to letters/digits so saved values match loosely ----
        private static string Key(string? v) =>
            new string((v ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();


        // GET: InfrastructureDetails?courseCode=..&typeOfAffiliation=..
        [HttpGet]
        public async Task<IActionResult> InfrastructureDetails(string courseCode, string typeOfAffiliation)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            typeOfAffiliation ??= GetSessionTypeOfAffiliation();
            courseCode ??= GetSessionCourseCode();
            SetSessionCourseCode(courseCode);

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return RedirectToAction("SubjectSelection", "pgcourses");
            }

            // selected department for the info bar
            await SetSelectedDepartmentAsync(collegeCode, courseCode);

            var model = new InfrastructureDetailsViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            // 1. Inspection Committee - always show 3 rows
            var committeeRows = await _context.LocalInspectionCommittees
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            for (int i = 1; i <= 3; i++)
            {
                var saved = committeeRows.FirstOrDefault(x => x.SlNo == i);
                model.InspectionCommittee.Add(new InspectionCommitteeRowVM
                {
                    SlNo = i,
                    NameOfChairmanOrMember = saved?.NameOfChairmanOrMember,
                    CorrespondenceAddress = saved?.CorrespondenceAddress,
                    PhoneOffResMobile = saved?.PhoneOffResMobile,
                    Email = saved?.Email
                });
            }

            // 2. Fee paid details - dynamic, at least 1 blank row
            var feeRows = await _context.FeePaidDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            model.FeePaidDetails = feeRows.Count > 0
                ? feeRows.Select(x => new FeePaidRowVM
                {
                    SlNo = x.SlNo,
                    Particulars = x.Particulars,
                    Amount = x.Amount,
                    TransactionId = x.TransactionId,
                    PaymentDate = x.PaymentDate?.ToDateTime(TimeOnly.MinValue),
                    BankName = x.BankName,
                    BankBranch = x.BankBranch
                }).ToList()
                : new List<FeePaidRowVM> { new() { SlNo = 1 } };

            // k. Other Course/Observership - dynamic, at least 2 blank rows (as printed)
            var otherCourseRows = await _context.OtherCourseObserverships
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            model.OtherCourses = otherCourseRows.Count > 0
                ? otherCourseRows.Select(x => new OtherCourseRowVM
                {
                    NameOfQualificationCourse = x.NameOfQualificationCourse,
                    PermittedByMcinmc = NormalizeYesNo(x.PermittedByMciNmc),
                    NumberOfAdmissionsPerYear = x.NumberOfAdmissionsPerYear
                }).ToList()
                : new List<OtherCourseRowVM> { new(), new() };

            // B.a. OPD header
            var opd = await _context.OpdDetails.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (opd != null)
            {
                model.NoOfRoomsForConsultation = opd.NoOfRoomsForConsultation;
                model.WaitingAreaInSqM = opd.WaitingAreaInSqM;
                model.SpaceAndArrangements = NormalizeAdequacy(opd.SpaceAndArrangements);
                model.IfNotAdequateReasons = opd.IfNotAdequateReasons;
                model.DressingRoomAvailable = NormalizeAvailability(opd.DressingRoomAvailable);
                model.SeparateMinorOtMaleFemale = NormalizeAvailability(opd.SeparateMinorOtMaleFemale);
                model.PerRectalExamRoomAvailable = NormalizeAvailability(opd.PerRectalExamRoomAvailable);
                model.DressingRoom2Available = NormalizeAvailability(opd.DressingRoom2Available);
            }

            // OPD room areas - seed with the 3 fixed room types, keep any extra custom rows saved
            var opdRoomRows = await _context.OpdRoomAreas
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var roomType in OpdRoomTypeDefaults)
            {
                var saved = opdRoomRows.FirstOrDefault(x => x.RoomType == roomType);
                model.OpdRoomAreas.Add(new OpdRoomAreaRowVM { RoomType = roomType, AreaInSqM = saved?.AreaInSqM });
            }
            foreach (var extra in opdRoomRows.Where(x => !OpdRoomTypeDefaults.Contains(x.RoomType)))
            {
                model.OpdRoomAreas.Add(new OpdRoomAreaRowVM { RoomType = extra.RoomType ?? string.Empty, AreaInSqM = extra.AreaInSqM });
            }

            // b. Wards
            var wardsHeader = await _context.WardsHeaders.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (wardsHeader != null)
            {
                model.NoOfWardsMale = wardsHeader.NoOfWardsMale;
                model.NoOfWardsFemale = wardsHeader.NoOfWardsFemale;
            }

            var wardsParamRows = await _context.WardsParameters
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var paramName in WardsParameterDefaults)
            {
                var saved = wardsParamRows.FirstOrDefault(x => x.ParameterName == paramName);
                model.WardsParameters.Add(new WardsParameterRowVM { ParameterName = paramName, Details = saved?.Details });
            }

            // c. OT distribution - dynamic + total, at least 3 blank rows
            var otRows = await _context.OperationTheatreDistributions
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            model.OperationTheatres = otRows.Count > 0
                ? otRows.Select(x => new OtRowVM
                {
                    SlNo = x.SlNo,
                    DepartmentName = x.DepartmentName,
                    MajorOtTables = x.MajorOtTables,
                    MinorOtTables = x.MinorOtTables,
                    IsTotalRow = x.IsTotalRow
                }).ToList()
                : new List<OtRowVM>
                {
                    new() { SlNo = 1 }, new() { SlNo = 2 }, new() { SlNo = 3 }, new() { IsTotalRow = true }
                };

            // e. Seminar Room
            var seminar = await _context.SeminarRooms.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (seminar != null)
            {
                model.SeminarSpaceAndFacility = NormalizeAdequacy(seminar.SpaceAndFacility);
                model.SeminarInternetFacility = NormalizeInternet(seminar.InternetFacility);
                model.SeminarAvdetails = seminar.AudiovisualEquipmentDetails;
            }

            // h. Departmental Museum
            var museum = await _context.DepartmentalMuseums.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (museum != null)
            {
                model.MuseumSpace = museum.Space;
                model.MuseumTotalSpecimens = museum.TotalNumberOfSpecimens;
                model.MuseumTotalChartDiagram = museum.TotalNumberOfChartDiagram;
            }

            // f. Library facility
            var library = await _context.LibraryFacilities.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (library != null)
            {
                model.NumberOfBooksCentral = library.NumberOfBooksCentral;
                model.NumberOfBooksDepartmental = library.NumberOfBooksDepartmental;
                model.BooksPurchasedLast3YrsCentral = library.BooksPurchasedLast3YrsCentral;
                model.BooksPurchasedLast3YrsDept = library.BooksPurchasedLast3YrsDept;
                model.AnnexureAttached = library.AnnexureAttached ?? false;
                model.TotalIndianJournalsCentral = library.TotalIndianJournalsCentral;
                model.TotalIndianJournalsDept = library.TotalIndianJournalsDept;
                model.TotalForeignJournalsCentral = library.TotalForeignJournalsCentral;
                model.TotalForeignJournalsDept = library.TotalForeignJournalsDept;
                model.ComputerWithInternetCentral = library.ComputerWithInternetCentral;
                model.ComputerWithInternetDept = library.ComputerWithInternetDept;
                model.CentralLibraryTiming = library.CentralLibraryTiming;
                model.CentralReadingRoomTiming = library.CentralReadingRoomTiming;
            }

            // g. Departmental Research Lab
            var researchLab = await _context.DepartmentalResearchLabs.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (researchLab != null)
            {
                model.ResearchLabSpace = researchLab.Space;
                model.ResearchLabEquipment = researchLab.Equipment;
                model.ResearchProjectsCompletedPast3Yrs = researchLab.ResearchProjectsCompletedPast3Yrs;
                model.ResearchProjectsInProgress = researchLab.ResearchProjectsInProgress;
            }

            // h. Equipment - 8 fixed rows
            var equipmentRows = await _context.DepartmentEquipments
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var eqName in EquipmentDefaults)
            {
                var saved = equipmentRows.FirstOrDefault(x => x.NameOfEquipment == eqName);
                model.Equipment.Add(new DeptEquipmentRowVM
                {
                    NameOfEquipment = eqName,
                    NumbersAvailable = saved?.NumbersAvailable,
                    FunctionalStatus = saved?.FunctionalStatus,
                    IsAdequate = NormalizeYesNo(saved?.IsAdequate)
                });
            }

            return View(model);
        }

        // POST: InfrastructureDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InfrastructureDetails(InfrastructureDetailsViewModel model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;

                // the info bar must also show when the page is re-displayed after a validation error
                await SetSelectedDepartmentAsync(collegeCode, model.CourseCode);

                return View("InfrastructureDetails", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = ResolveTypeOfAffiliation(model.TypeOfAffiliation);   // fetched at save time, stored on every row
            var createdBy = User.Identity?.Name ?? collegeCode;

            // ---- 1. Inspection Committee: delete & reinsert ----
            var oldCommittee = _context.LocalInspectionCommittees.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.LocalInspectionCommittees.RemoveRange(oldCommittee);

            foreach (var row in model.InspectionCommittee ?? new())
            {
                _context.LocalInspectionCommittees.Add(new LocalInspectionCommittee
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    SlNo = row.SlNo,
                    NameOfChairmanOrMember = row.NameOfChairmanOrMember,
                    CorrespondenceAddress = row.CorrespondenceAddress,
                    PhoneOffResMobile = row.PhoneOffResMobile,
                    Email = row.Email,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- 2. Fee Paid Details: delete & reinsert ----
            var oldFees = _context.FeePaidDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.FeePaidDetails.RemoveRange(oldFees);

            int feeSeq = 1;
            foreach (var row in model.FeePaidDetails ?? new())
            {
                _context.FeePaidDetails.Add(new FeePaidDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    SlNo = feeSeq++,
                    Particulars = row.Particulars,
                    Amount = row.Amount,
                    TransactionId = row.TransactionId,
                    PaymentDate = row.PaymentDate.HasValue ? DateOnly.FromDateTime(row.PaymentDate.Value) : null,
                    BankName = row.BankName,
                    BankBranch = row.BankBranch,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- k. Other Course/Observership: delete & reinsert ----
            var oldOtherCourses = _context.OtherCourseObserverships.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.OtherCourseObserverships.RemoveRange(oldOtherCourses);

            foreach (var row in model.OtherCourses ?? new())
            {
                _context.OtherCourseObserverships.Add(new OtherCourseObservership
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    NameOfQualificationCourse = row.NameOfQualificationCourse,
                    PermittedByMciNmc = NormalizeYesNo(row.PermittedByMcinmc),
                    NumberOfAdmissionsPerYear = row.NumberOfAdmissionsPerYear,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- B.a. OPD header: find-or-create single row ----
            var opd = await _context.OpdDetails.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (opd == null)
            {
                opd = new OpdDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.OpdDetails.Add(opd);
            }
            else
            {
                opd.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                opd.ModifiedBy = createdBy;
                opd.ModifiedDate = DateTime.Now;
            }
            opd.NoOfRoomsForConsultation = model.NoOfRoomsForConsultation;
            opd.WaitingAreaInSqM = model.WaitingAreaInSqM;
            opd.SpaceAndArrangements = NormalizeAdequacy(model.SpaceAndArrangements);
            opd.IfNotAdequateReasons = model.IfNotAdequateReasons;
            opd.DressingRoomAvailable = NormalizeAvailability(model.DressingRoomAvailable);
            opd.SeparateMinorOtMaleFemale = NormalizeAvailability(model.SeparateMinorOtMaleFemale);
            opd.PerRectalExamRoomAvailable = NormalizeAvailability(model.PerRectalExamRoomAvailable);
            opd.DressingRoom2Available = NormalizeAvailability(model.DressingRoom2Available);

            // ---- OPD room areas: delete & reinsert ----
            var oldOpdRooms = _context.OpdRoomAreas.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.OpdRoomAreas.RemoveRange(oldOpdRooms);

            foreach (var row in model.OpdRoomAreas ?? new())
            {
                if (string.IsNullOrWhiteSpace(row.RoomType)) continue;
                _context.OpdRoomAreas.Add(new OpdRoomArea
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    RoomType = row.RoomType,
                    AreaInSqM = row.AreaInSqM,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- b. Wards header: find-or-create ----
            var wardsHeader = await _context.WardsHeaders.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (wardsHeader == null)
            {
                wardsHeader = new WardsHeader
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.WardsHeaders.Add(wardsHeader);
            }
            else
            {
                wardsHeader.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                wardsHeader.ModifiedBy = createdBy;
                wardsHeader.ModifiedDate = DateTime.Now;
            }
            wardsHeader.NoOfWardsMale = model.NoOfWardsMale;
            wardsHeader.NoOfWardsFemale = model.NoOfWardsFemale;

            // ---- Wards parameters: delete & reinsert ----
            var oldWardsParams = _context.WardsParameters.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.WardsParameters.RemoveRange(oldWardsParams);

            foreach (var row in model.WardsParameters ?? new())
            {
                _context.WardsParameters.Add(new WardsParameter
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    ParameterName = row.ParameterName,
                    Details = row.Details,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- c. OT distribution: delete & reinsert ----
            var oldOt = _context.OperationTheatreDistributions.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.OperationTheatreDistributions.RemoveRange(oldOt);

            int otSeq = 1;
            foreach (var row in model.OperationTheatres ?? new())
            {
                _context.OperationTheatreDistributions.Add(new OperationTheatreDistribution
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    SlNo = row.IsTotalRow ? null : otSeq++,
                    DepartmentName = row.DepartmentName,
                    MajorOtTables = row.MajorOtTables,
                    MinorOtTables = row.MinorOtTables,
                    IsTotalRow = row.IsTotalRow,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- e. Seminar Room: find-or-create ----
            var seminar = await _context.SeminarRooms.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (seminar == null)
            {
                seminar = new SeminarRoom
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.SeminarRooms.Add(seminar);
            }
            else
            {
                seminar.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                seminar.ModifiedBy = createdBy;
                seminar.ModifiedDate = DateTime.Now;
            }
            seminar.SpaceAndFacility = NormalizeAdequacy(model.SeminarSpaceAndFacility);
            seminar.InternetFacility = NormalizeInternet(model.SeminarInternetFacility);
            seminar.AudiovisualEquipmentDetails = model.SeminarAvdetails;

            // ---- h. Departmental Museum: find-or-create ----
            var museum = await _context.DepartmentalMuseums.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (museum == null)
            {
                museum = new DepartmentalMuseum
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.DepartmentalMuseums.Add(museum);
            }
            else
            {
                museum.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                museum.ModifiedBy = createdBy;
                museum.ModifiedDate = DateTime.Now;
            }
            museum.Space = model.MuseumSpace;
            museum.TotalNumberOfSpecimens = model.MuseumTotalSpecimens;
            museum.TotalNumberOfChartDiagram = model.MuseumTotalChartDiagram;

            // ---- f. Library facility: find-or-create ----
            var library = await _context.LibraryFacilities.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (library == null)
            {
                library = new LibraryFacility
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.LibraryFacilities.Add(library);
            }
            else
            {
                library.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                library.ModifiedBy = createdBy;
                library.ModifiedDate = DateTime.Now;
            }
            library.NumberOfBooksCentral = model.NumberOfBooksCentral;
            library.NumberOfBooksDepartmental = model.NumberOfBooksDepartmental;
            library.BooksPurchasedLast3YrsCentral = model.BooksPurchasedLast3YrsCentral;
            library.BooksPurchasedLast3YrsDept = model.BooksPurchasedLast3YrsDept;
            library.AnnexureAttached = model.AnnexureAttached;
            library.TotalIndianJournalsCentral = model.TotalIndianJournalsCentral;
            library.TotalIndianJournalsDept = model.TotalIndianJournalsDept;
            library.TotalForeignJournalsCentral = model.TotalForeignJournalsCentral;
            library.TotalForeignJournalsDept = model.TotalForeignJournalsDept;
            library.ComputerWithInternetCentral = model.ComputerWithInternetCentral;
            library.ComputerWithInternetDept = model.ComputerWithInternetDept;
            library.CentralLibraryTiming = model.CentralLibraryTiming;
            library.CentralReadingRoomTiming = model.CentralReadingRoomTiming;

            // ---- g. Departmental Research Lab: find-or-create ----
            var researchLab = await _context.DepartmentalResearchLabs.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (researchLab == null)
            {
                researchLab = new DepartmentalResearchLab
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.DepartmentalResearchLabs.Add(researchLab);
            }
            else
            {
                researchLab.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                researchLab.ModifiedBy = createdBy;
                researchLab.ModifiedDate = DateTime.Now;
            }
            researchLab.Space = model.ResearchLabSpace;
            researchLab.Equipment = model.ResearchLabEquipment;
            researchLab.ResearchProjectsCompletedPast3Yrs = model.ResearchProjectsCompletedPast3Yrs;
            researchLab.ResearchProjectsInProgress = model.ResearchProjectsInProgress;

            // ---- h. Equipment: delete & reinsert (8 fixed rows) ----
            var oldEquipment = _context.DepartmentEquipments.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.DepartmentEquipments.RemoveRange(oldEquipment);

            foreach (var row in model.Equipment ?? new())
            {
                _context.DepartmentEquipments.Add(new DepartmentEquipment
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    NameOfEquipment = row.NameOfEquipment,
                    NumbersAvailable = (int)row.NumbersAvailable,
                    FunctionalStatus = row.FunctionalStatus,
                    IsAdequate = NormalizeYesNo(row.IsAdequate),
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(ServicesWorkload), new { courseCode, typeOfAffiliation });
        }

        // ---------------------------------------------------------------------------------
        // Services & clinical workload
        // ---------------------------------------------------------------------------------

        private static readonly (string ClinicName, int Seq)[] SpecialtyClinicDefaults = new[]
        {
            ("Cardio-Thoracic Vascular Surgery", 1),
            ("Urology", 2),
            ("Plastic Surgery", 3),
            ("Surgical Gastroenterology", 4),
            ("Neurosurgery", 5),
            ("Pediatric Surgery", 6),
            ("Cancer Clinic", 7),
            ("Vascular Surgery", 8),
            ("Any other Clinic", 9)
        };

        private static readonly (string ServiceName, int Seq)[] DepartmentServiceDefaults = new[]
        {
            ("Upper GI Endoscopy", 1),
            ("Colonoscopy", 2),
            ("Any other", 3)
        };

        private static readonly (string ParticularName, int Seq)[] ClinicalWorkloadDefaults = new[]
        {
            ("Number of patients attended OPD", 1),
            ("No. of Admissions (IP)", 2),
            ("No. of Inpatients", 3),
            ("Total no. of beds allotted", 4),
            ("Bed occupancy in percentage", 5),
            ("Number of Major surgeries", 6),
            ("Number of Minor Surgery", 7),
            ("Total X-rays (OP & IP)", 8),
            ("Total Ultrasonography (OP & IP)", 9),
            ("Total CT scans (OP & IP)", 10),
            ("Total MRI scans (OP & IP)", 11),
            ("Total Hematology (OP & IP)", 12),
            ("Total Biochemistry (OP & IP)", 13),
            ("Total Microbiology (OP & IP)", 14),
            ("Total Histopathology (OP & IP)", 15),
            ("Total clinical pathology (OP & IP)", 16),
            ("Total Normal deliveries", 17),
            ("Total cesarean deliveries", 18),
            ("Total deliveries", 19),
            ("Total deaths", 20),
            ("Consumption of blood units with components", 21)
        };

        // GET: ServicesWorkload?courseCode=..&typeOfAffiliation=..
        [HttpGet]
        public async Task<IActionResult> ServicesWorkload(string courseCode, string typeOfAffiliation)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            typeOfAffiliation ??= GetSessionTypeOfAffiliation();
            courseCode ??= GetSessionCourseCode();

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return RedirectToAction("SubjectSelection", "pgcourses");
            }

            SetSessionCourseCode(courseCode);

            // selected department for the info bar
            await SetSelectedDepartmentAsync(collegeCode, courseCode);

            var model = new ServicesWorkloadViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            // ---- C.i. Specialty clinics - fixed rows ----
            var clinicRows = await _context.SpecialtyClinicDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var (clinicName, seq) in SpecialtyClinicDefaults)
            {
                // match on sequence, not name: the "Any other Clinic" row is user-editable
                var saved = clinicRows.FirstOrDefault(x => x.ClinicSequence == seq);
                model.SpecialtyClinics.Add(new SpecialtyClinicRowVM
                {
                    ClinicName = !string.IsNullOrWhiteSpace(saved?.ClinicName) ? saved!.ClinicName : clinicName,
                    ClinicSequence = seq,
                    Weekdays = saved?.Weekdays,
                    Timings = saved?.Timings,
                    NumberOfCasesAvg = saved?.NumberOfCasesAvg,
                    ClinicInchargeName = saved?.ClinicInchargeName
                });
            }

            // ---- C.ii. Department services - fixed rows ----
            var serviceRows = await _context.DepartmentServiceDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var (serviceName, seq) in DepartmentServiceDefaults)
            {
                var saved = serviceRows.FirstOrDefault(x => x.ServiceSequence == seq);
                model.DepartmentServices.Add(new DepartmentServiceRowVM
                {
                    ServiceName = !string.IsNullOrWhiteSpace(saved?.ServiceName) ? saved!.ServiceName : serviceName,
                    ServiceSequence = seq,
                    IsAvailable = ToDisplayYesNo(saved?.IsAvailable),
                    Remarks = saved?.Remarks
                });
            }

            // ---- D. Clinical workload - fixed rows ----
            var workloadRows = await _context.ClinicalWorkloadDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var (particularName, seq) in ClinicalWorkloadDefaults)
            {
                var saved = workloadRows.FirstOrDefault(x => x.ParticularSequence == seq);
                model.ClinicalWorkload.Add(new ClinicalWorkloadRowVM
                {
                    ParticularName = particularName,
                    ParticularSequence = seq,
                    EntireHospital = saved?.EntireHospital,
                    OnDayOfAssessment = saved?.OnDayOfAssessment,
                    Random3Days = saved?.Random3Days,
                    PreviousYear = saved?.PreviousYear
                });
            }

            return View(model);
        }

        // POST: ServicesWorkload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServicesWorkload(ServicesWorkloadViewModel model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;

                // the info bar must also show when the page is re-displayed after a validation error
                await SetSelectedDepartmentAsync(collegeCode, model.CourseCode);

                return View("ServicesWorkload", model);
            }

            // always use session codes, never trust hidden fields for these two
            var courseCode = model.CourseCode;
            var typeOfAffiliation = ResolveTypeOfAffiliation(model.TypeOfAffiliation);   // fetched at save time, stored on every row
            var createdBy = User.Identity?.Name ?? collegeCode;
            var now = DateTime.Now;

            // ---- C.i. Specialty clinics: delete & reinsert ----
            var oldClinics = _context.SpecialtyClinicDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.SpecialtyClinicDetails.RemoveRange(oldClinics);

            foreach (var row in model.SpecialtyClinics)
            {
                _context.SpecialtyClinicDetails.Add(new SpecialtyClinicDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    ClinicName = row.ClinicName,
                    ClinicSequence = row.ClinicSequence,
                    Weekdays = row.Weekdays,
                    Timings = row.Timings,
                    NumberOfCasesAvg = row.NumberOfCasesAvg,
                    ClinicInchargeName = row.ClinicInchargeName,
                    CreatedBy = createdBy,
                    CreatedDate = now
                });
            }

            // ---- C.ii. Department services: delete & reinsert ----
            var oldServices = _context.DepartmentServiceDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.DepartmentServiceDetails.RemoveRange(oldServices);

            foreach (var row in model.DepartmentServices)
            {
                _context.DepartmentServiceDetails.Add(new DepartmentServiceDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    ServiceName = row.ServiceName,
                    ServiceSequence = row.ServiceSequence,
                    IsAvailable = ToStoredYesNo(row.IsAvailable),
                    Remarks = row.Remarks,
                    CreatedBy = createdBy,
                    CreatedDate = now
                });
            }

            // ---- D. Clinical workload: delete & reinsert ----
            var oldWorkload = _context.ClinicalWorkloadDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.ClinicalWorkloadDetails.RemoveRange(oldWorkload);

            foreach (var row in model.ClinicalWorkload)
            {
                _context.ClinicalWorkloadDetails.Add(new ClinicalWorkloadDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    ParticularName = row.ParticularName,
                    ParticularSequence = row.ParticularSequence,
                    EntireHospital = row.EntireHospital,
                    OnDayOfAssessment = row.OnDayOfAssessment,
                    Random3Days = row.Random3Days,
                    PreviousYear = row.PreviousYear,
                    CreatedBy = createdBy,
                    CreatedDate = now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(StaffDetails), new { courseCode, typeOfAffiliation });
        }

        // ---------------------------------------------------------------------------------
        // Staff details
        // ---------------------------------------------------------------------------------

        private static readonly (string Designation, int Seq)[] EligibleFacultyDefaults = new[]
        {
            ("Professor", 1),
            ("Associate Professor", 2),
            ("Assistant Professor", 3),
            ("Senior Resident", 4)
        };

        private static readonly (string YearLabel, int Seq)[] PgStudentsYearDefaults = new[]
        {
            ("1st year", 1),
            ("2nd Year", 2),
            ("3rd year", 3)
        };

        // GET: StaffDetails?courseCode=..&typeOfAffiliation=..
        [HttpGet]
        public async Task<IActionResult> StaffDetails(string courseCode, string typeOfAffiliation)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            typeOfAffiliation ??= GetSessionTypeOfAffiliation();
            courseCode ??= GetSessionCourseCode();

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return RedirectToAction("SubjectSelection", "pgcourses");
            }

            SetSessionCourseCode(courseCode);

            var model = new StaffDetailsViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            // F.i. is a READ-ONLY list of the department's faculty (UG_Faculty_Details),
            // filtered by faculty code + college code + the department of the selected course.
            await LoadDepartmentFacultyAsync(facultyCode, collegeCode, courseCode);

            // F.ii. Eligible faculty - 4 fixed rows
            var eligibleRows = await _context.EligibleFacultyDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var (designation, seq) in EligibleFacultyDefaults)
            {
                // Match by sequence first, then loosely by designation text
                // (tolerates casing / padding differences in the saved designation)
                var saved = eligibleRows.FirstOrDefault(x => x.DesignationSequence == seq)
                            ?? eligibleRows.FirstOrDefault(x => Key(x.Designation) == Key(designation));

                model.EligibleFaculty.Add(new EligibleFacultyRowVM
                {
                    Designation = designation,
                    DesignationSequence = seq,
                    NumberOfFaculty = saved?.NumberOfFaculty,
                    Names = saved?.Names,
                    TotalAdmissionSeats = saved?.TotalAdmissionSeats,
                    // Normalise so "Not adequate", "adequate ", "Y" etc. map to the dropdown's exact option values
                    IsAdequateForAdmission = NormalizeAdequacy(saved?.IsAdequateForAdmission)
                });
            }

            // F.iii. PG students by year - 3 fixed rows
            var yearRows = await _context.PgStudentsYearWiseDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var (yearLabel, seq) in PgStudentsYearDefaults)
            {
                var saved = yearRows.FirstOrDefault(x => x.YearLabel == yearLabel);
                model.PgStudentsByYear.Add(new PgStudentsYearRowVM
                {
                    YearLabel = yearLabel,
                    YearSequence = seq,
                    NumberOfStudents = saved?.NumberOfStudents
                });
            }

            // H. Exam results - dynamic, at least 3 blank rows (as printed)
            var examRows = await _context.StudentExamResultDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            model.ExamResults = examRows.Count > 0
                ? examRows.Select(x => new StudentExamResultRowVM
                {
                    SlNo = x.SlNo,
                    StudentName = x.StudentName,
                    Result = x.Result
                }).ToList()
                : new List<StudentExamResultRowVM> { new() { SlNo = 1 }, new() { SlNo = 2 }, new() { SlNo = 3 } };

            return View(model);
        }

        // POST: StaffDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StaffDetails(StaffDetailsViewModel model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;
                await LoadDepartmentFacultyAsync(facultyCode, collegeCode, model.CourseCode);
                return View("StaffDetails", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = ResolveTypeOfAffiliation(model.TypeOfAffiliation);   // fetched at save time, stored on every row
            var createdBy = User.Identity?.Name ?? collegeCode;
            var now = DateTime.Now;

            // F.i. (faculty list) is read-only and comes from UG_Faculty_Details, so nothing is saved for it.

            // ---- F.ii. Eligible faculty: delete & reinsert ----
            var oldEligible = _context.EligibleFacultyDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.EligibleFacultyDetails.RemoveRange(oldEligible);

            foreach (var row in model.EligibleFaculty)
            {
                _context.EligibleFacultyDetails.Add(new EligibleFacultyDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    Designation = row.Designation,
                    DesignationSequence = row.DesignationSequence,
                    NumberOfFaculty = row.NumberOfFaculty,
                    Names = row.Names,
                    TotalAdmissionSeats = row.TotalAdmissionSeats,
                    // Always store one of the two canonical values: "Adequate" / "Not Adequate"
                    IsAdequateForAdmission = NormalizeAdequacy(row.IsAdequateForAdmission),
                    CreatedBy = createdBy,
                    CreatedDate = now
                });
            }

            // ---- F.iii. PG students by year: delete & reinsert ----
            var oldYears = _context.PgStudentsYearWiseDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.PgStudentsYearWiseDetails.RemoveRange(oldYears);

            foreach (var row in model.PgStudentsByYear)
            {
                _context.PgStudentsYearWiseDetails.Add(new PgStudentsYearWiseDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    YearLabel = row.YearLabel,
                    YearSequence = row.YearSequence,
                    NumberOfStudents = row.NumberOfStudents,
                    CreatedBy = createdBy,
                    CreatedDate = now
                });
            }

            // ---- H. Exam results: delete & reinsert ----
            var oldExamResults = _context.StudentExamResultDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.StudentExamResultDetails.RemoveRange(oldExamResults);

            int examSeq = 1;
            foreach (var row in model.ExamResults)
            {
                _context.StudentExamResultDetails.Add(new StudentExamResultDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    SlNo = examSeq++,
                    StudentName = row.StudentName,
                    Result = row.Result,
                    CreatedBy = createdBy,
                    CreatedDate = now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(AcademicActivities), new { courseCode, typeOfAffiliation });
        }

        // ---------------------------------------------------------------------------------
        // Helpers for StaffDetails
        // Section F.i is a read-only list built from UG_Faculty_Details; nothing in it is saved.
        // ---------------------------------------------------------------------------------

        // ---------------------------------------------------------------------------------
        // Department code resolution
        //
        // SubjectSelection carries the COURSE code (Mst_MedicalCollegeCourseIntake.CourseCode, e.g. 1032),
        // while UG_Faculty_Details.DepartmentCode uses MDxxx codes. DepartmentMaster has both
        // (CourseCode + DepartmentCode), so it is the bridge:
        //   1) match DepartmentMaster.CourseCode to the selected course code
        //   2) otherwise match DepartmentMaster.DepartmentName to the selected course name
        // ---------------------------------------------------------------------------------
        private async Task<string?> ResolveDepartmentCodeAsync(string facultyCode, string? courseCode, string? courseName)
        {
            var rows = await _context.DepartmentMasters
                .AsNoTracking()
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .Select(d => new { d.CourseCode, d.DepartmentCode, d.DepartmentName })
                .ToListAsync();

            // 1) by course code
            var code = (courseCode ?? "").Trim();
            if (code.Length > 0)
            {
                var byCode = rows.FirstOrDefault(r =>
                    string.Equals(Convert.ToString(r.CourseCode)?.Trim(), code, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(Convert.ToString(r.DepartmentCode)));
                if (byCode != null) return Convert.ToString(byCode.DepartmentCode)!.Trim();
            }

            // 2) by name ("M D Anaesthesiology" vs "Anaesthesiology"; spaces/punctuation ignored)
            var key = Key(courseName);
            if (key.Length == 0) return null;

            var exact = rows.FirstOrDefault(r => Key(r.DepartmentName) == key);
            if (exact != null) return Convert.ToString(exact.DepartmentCode)?.Trim();

            var loose = rows.FirstOrDefault(r =>
            {
                var dk = Key(r.DepartmentName);
                return dk.Length > 2 && (key.Contains(dk) || dk.Contains(key));
            });
            return loose == null ? null : Convert.ToString(loose.DepartmentCode)?.Trim();
        }

        // If nothing matches the selected department, list ALL faculty of the college
        // (each row shows its own department) so the page is never empty.
        // Set to false to list only exact department matches.
        private static readonly bool ListAllCollegeFacultyWhenDepartmentUnmapped = true;

        private sealed record DepartmentFacultyResult(
            List<FacultyDisplayRowVM> Faculty,
            bool MatchedDepartment,
            int CollegeFacultyCount,
            string CollegeDepartmentCodes);

        // facultyCode / collegeCode / course level come from the session; the department is the
        // course selected on SubjectSelection, resolved to the faculty master's department code (MDxxx).
        private async Task LoadDepartmentFacultyAsync(string facultyCode, string collegeCode, string courseCode)
        {
            var courseName = await GetDepartmentNameAsync(collegeCode, courseCode);
            var departmentCode = await ResolveDepartmentCodeAsync(facultyCode, courseCode, courseName);

            var result = await GetDepartmentFacultyAsync(facultyCode, collegeCode, departmentCode);

            // Prefer the department name from DepartmentMastersForUG; fall back to the selected course name
            var masterName = result.MatchedDepartment
                ? result.Faculty.Select(f => f.DepartmentName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                : null;

            ViewBag.FacultyCodeUsed = facultyCode;
            ViewBag.CollegeCodeUsed = collegeCode;
            ViewBag.CourseLevel = GetSessionCourseLevel();
            ViewBag.DepartmentCode = departmentCode ?? courseCode;   // shown in the view's warning when unresolved
            ViewBag.DepartmentName = masterName ?? courseName;

            ViewBag.DepartmentFaculty = result.Faculty;
            ViewBag.DepartmentMatched = result.MatchedDepartment;
            ViewBag.CollegeFacultyCount = result.CollegeFacultyCount;
            ViewBag.CollegeDepartmentCodes = result.CollegeDepartmentCodes;
        }

        // Name of the selected department = the course name chosen on SubjectSelection
        private async Task<string?> GetDepartmentNameAsync(string collegeCode, string? courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode)) return null;
            var college = collegeCode.Trim();
            var code = courseCode.Trim();

            var courses = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(c => c.CollCode == college)
                .Select(c => new { c.CourseCode, c.Course })
                .ToListAsync();

            return courses
                .Where(c => string.Equals(Convert.ToString(c.CourseCode)?.Trim(), code,
                                          StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Course?.Trim())
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
        }

        private async Task<DepartmentFacultyResult> GetDepartmentFacultyAsync(
            string facultyCode, string collegeCode, string? departmentCode)
        {
            var college = collegeCode.Trim();

            // College-level fetch in SQL (a few hundred rows at most); department match in memory
            // so it works whether DepartmentCode is stored as text or a number.
            var rows = await _context.UgFacultyDetails
                .AsNoTracking()
                .Where(x => x.FacultyCode.ToString() == facultyCode && x.CollegeCode == college)
                .Select(x => new
                {
                    x.DepartmentCode,
                    x.NameOftheFaculty,
                    x.DesignationCode,
                    x.Dob,
                    x.DateOfAppointment,
                    x.AadhaarNo,
                    x.Panno,
                    x.MobileNo,
                    x.EmailId,
                    x.StateCouncilRegNo,
                    x.AebasattendId,
                    x.ProfessionalQualification,
                    x.NatureOfEmployment,
                    x.TeachingExpInYrs
                })
                .ToListAsync();

            // Designation names (UGDesignationMaster): DesignationID <-> UG_Faculty_Details.DesignationCode
            var designationNames = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var designationOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var designationRows = await _context.UgdesignationMasters      // <- DbSet for UGDesignationMaster
                .AsNoTracking()
                .Select(d => new { d.DesignationId, d.DesignationName, d.Order })
                .ToListAsync();
            foreach (var d in designationRows)
            {
                var id = Convert.ToString(d.DesignationId)?.Trim();
                if (string.IsNullOrEmpty(id)) continue;
                designationNames.TryAdd(id, NullIfBlank(d.DesignationName));
                designationOrder.TryAdd(id, int.TryParse(Convert.ToString(d.Order), out var o) ? o : int.MaxValue);
            }

            // Department names (DepartmentMastersForUG): DepartmentCode (+ FacultyCode)
            var departmentNames = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var departmentRows = await _context.DepartmentMastersForUgs    // <- DbSet for DepartmentMastersForUG
                .AsNoTracking()
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .Select(d => new { d.DepartmentCode, d.DepartmentName })
                .ToListAsync();
            foreach (var d in departmentRows)
            {
                var id = Convert.ToString(d.DepartmentCode)?.Trim();
                if (string.IsNullOrEmpty(id)) continue;
                departmentNames.TryAdd(id, NullIfBlank(d.DepartmentName));
            }

            var all = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.NameOftheFaculty))
                .Select(r =>
                {
                    var rowDept = Convert.ToString(r.DepartmentCode)?.Trim() ?? string.Empty;
                    var desigCode = Convert.ToString(r.DesignationCode)?.Trim() ?? string.Empty;
                    designationNames.TryGetValue(desigCode, out var desigName);
                    departmentNames.TryGetValue(rowDept, out var deptName);

                    return new
                    {
                        Dept = rowDept,
                        Order = designationOrder.TryGetValue(desigCode, out var ord) ? ord : int.MaxValue,
                        Row = new FacultyDisplayRowVM
                        {
                            DepartmentCode = NullIfBlank(rowDept),
                            DepartmentName = deptName,
                            Name = r.NameOftheFaculty!.Trim(),
                            DesignationCode = NullIfBlank(desigCode),
                            Designation = desigName ?? NullIfBlank(desigCode),
                            Dob = FormatDate(r.Dob),
                            DateOfAppointment = FormatDate(r.DateOfAppointment),
                            AadhaarNo = NullIfBlank(Convert.ToString(r.AadhaarNo)),
                            PanNo = NullIfBlank(Convert.ToString(r.Panno)),
                            MobileNo = NullIfBlank(Convert.ToString(r.MobileNo)),
                            EmailId = NullIfBlank(Convert.ToString(r.EmailId)),
                            StateCouncilRegNo = NullIfBlank(Convert.ToString(r.StateCouncilRegNo)),
                            AebasAttendId = NullIfBlank(Convert.ToString(r.AebasattendId)),
                            ProfessionalQualification = NullIfBlank(Convert.ToString(r.ProfessionalQualification)),
                            NatureOfEmployment = NullIfBlank(Convert.ToString(r.NatureOfEmployment)),
                            TeachingExpInYrs = NullIfBlank(Convert.ToString(r.TeachingExpInYrs))
                        }
                    };
                })
                .ToList();

            // e.g. "MD001 (12), MD002 (8)" - shown on the page when nothing matches, to find the right mapping
            var codesSummary = string.Join(", ", all
                .GroupBy(a => a.Dept.Length == 0 ? "(blank)" : a.Dept)
                .OrderBy(g => g.Key)
                .Take(40)
                .Select(g => $"{g.Key} ({g.Count()})"));

            var dept = departmentCode?.Trim() ?? string.Empty;

            var matched = dept.Length == 0
                ? new List<FacultyDisplayRowVM>()
                : all.Where(a => string.Equals(a.Dept, dept, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(a => a.Order).ThenBy(a => a.Row.Name)
                     .Select(a => a.Row)
                     .ToList();

            if (matched.Count > 0)
                return new DepartmentFacultyResult(matched, true, all.Count, codesSummary);

            var fallback = ListAllCollegeFacultyWhenDepartmentUnmapped
                ? all.OrderBy(a => a.Dept).ThenBy(a => a.Order).ThenBy(a => a.Row.Name).Select(a => a.Row).ToList()
                : new List<FacultyDisplayRowVM>();

            return new DepartmentFacultyResult(fallback, false, all.Count, codesSummary);
        }

        private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // Works whether the column maps to DateTime?, DateOnly? or text
        private static DateTime? AsDate(object? value) => value switch
        {
            DateTime dt => dt,
            DateOnly d => d.ToDateTime(TimeOnly.MinValue),
            string str when DateTime.TryParse(str, out var parsed) => parsed,
            _ => null
        };

        // dd-MM-yyyy when it parses as a date, otherwise the stored text as-is
        private static string? FormatDate(object? value)
        {
            var d = AsDate(value);
            return d.HasValue ? d.Value.ToString("dd-MM-yyyy") : NullIfBlank(Convert.ToString(value));
        }

        // ---------------------------------------------------------------------------------
        // Academic activities
        // ---------------------------------------------------------------------------------

        private static readonly (string ActivityDetails, int Seq)[] AcademicActivityDefaults = new[]
        {
            ("Clinico-Pathological conference", 1),
            ("Clinical Seminars", 2),
            ("Journal Clubs", 3),
            ("Case presentations", 4),
            ("Group discussions", 5),
            ("Guest lectures", 6),
            ("Death Audit Meetings", 7)
        };

        // GET: AcademicActivities?courseCode=..&typeOfAffiliation=..
        [HttpGet]
        public async Task<IActionResult> AcademicActivities(string courseCode, string typeOfAffiliation)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            // fall back to the session values so the page also works when opened without query-string values
            typeOfAffiliation ??= GetSessionTypeOfAffiliation();
            courseCode ??= GetSessionCourseCode();

            // selected department for the info bar
            await SetSelectedDepartmentAsync(collegeCode, courseCode);

            var model = new AcademicActivitiesViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            // G. Academic Activities - 7 fixed rows
            var activityRows = await _context.AcademicActivityDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode)
                .ToListAsync();

            foreach (var (activityDetails, seq) in AcademicActivityDefaults)
            {
                var saved = activityRows.FirstOrDefault(x => x.ActivityDetails == activityDetails);
                model.AcademicActivities.Add(new AcademicActivityRowVM
                {
                    ActivityDetails = activityDetails,
                    SlNo = seq,
                    NumberInLastYear = saved?.NumberInLastYear,
                    Remarks = saved?.Remarks
                });
            }

            // Publications / Date / K. LIC Committee Observations - single record
            var summary = await _context.AcademicSummaryDetails.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (summary != null)
            {
                model.PublicationsPast3Years = summary.PublicationsPast3Years;
                model.AssessmentDate = summary.AssessmentDate?.ToDateTime(TimeOnly.MinValue);
                model.LiccommitteeObservations = summary.LiccommitteeObservations;
            }

            return View(model);
        }

        // POST: AcademicActivities
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcademicActivities(AcademicActivitiesViewModel model)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;

                // the info bar must also show when the page is re-displayed after a validation error
                await SetSelectedDepartmentAsync(collegeCode, model.CourseCode);

                return View("AcademicActivities", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = ResolveTypeOfAffiliation(model.TypeOfAffiliation);   // fetched at save time, stored on every row
            var createdBy = User.Identity?.Name ?? collegeCode;

            // ---- G. Academic Activities: delete & reinsert ----
            var oldActivities = _context.AcademicActivityDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);
            _context.AcademicActivityDetails.RemoveRange(oldActivities);

            foreach (var row in model.AcademicActivities)
            {
                _context.AcademicActivityDetails.Add(new AcademicActivityDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    SlNo = row.SlNo,
                    ActivityDetails = row.ActivityDetails,
                    NumberInLastYear = row.NumberInLastYear,
                    Remarks = row.Remarks,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- Publications / Date / K. LIC Committee Observations: find-or-create ----
            var summary = await _context.AcademicSummaryDetails.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode);

            if (summary == null)
            {
                summary = new AcademicSummaryDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };
                _context.AcademicSummaryDetails.Add(summary);
            }
            else
            {
                summary.TypeOfAffiliation = typeOfAffiliation;   // refresh on every save
                summary.ModifiedBy = createdBy;
                summary.ModifiedDate = DateTime.Now;
            }

            summary.PublicationsPast3Years = model.PublicationsPast3Years;
            summary.AssessmentDate = model.AssessmentDate.HasValue ? DateOnly.FromDateTime(model.AssessmentDate.Value) : null;
            summary.LiccommitteeObservations = model.LiccommitteeObservations;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction("Medical_LandBuildingdetails", "Medical_ContinuousAffiliation", new { courseCode, typeOfAffiliation });
        }
    }
}