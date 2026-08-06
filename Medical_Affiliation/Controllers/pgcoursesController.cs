using Admission_Affiliation.Models;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
//using Medical_Affiliation.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_Affiliation.Controllers
{
    // Attach this to whichever controller hosts the PG-course pages
    // (e.g. Aff_CA_Med_PGCourseController) - shown standalone here for clarity.
    public class pgcoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

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

        public pgcoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetSessionTypeOfAffiliation()
        {
            return HttpContext.Session.GetString("TypeOfAffiliation")
                ?? HttpContext.Session.GetString("SelectedTypeOfAffiliation")
                ?? HttpContext.Session.GetString("AffiliationType")
                ?? HttpContext.Session.GetString("AffiliationTypeId");
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
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var typeOfAffiliation = GetSessionTypeOfAffiliation();
            var courseLevel = GetSessionCourseLevel();

            if (string.IsNullOrEmpty(facultyCodeString) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(courseLevel))
            {
                ModelState.AddModelError(string.Empty, "Course level is not set in session.");
                return View(new PgCourseSubjectSelectionViewModel());
            }

            if (!int.TryParse(facultyCodeString, out var facultyCode))
            {
                ModelState.AddModelError(string.Empty, "Faculty code is invalid.");
                return View(new PgCourseSubjectSelectionViewModel());
            }

            var subjects = await (
                from c in _context.MstCourses
                join cc in _context.CollegeCourseIntakeDetails
                    on c.CourseCode.ToString() equals cc.CourseCode
                where c.FacultyCode == facultyCode
                      && cc.CollegeCode == collegeCode
                      && c.CourseLevel.Trim().ToUpper() == courseLevel
                select new PgCourseSubjectSelectionViewModel.PgCourseSubjectItem
                {
                    CourseCode = c.CourseCode.ToString(),
                    CourseName = c.CourseName,
                    SubjectName = c.SubjectName
                })
                .Distinct()
                .OrderBy(x => x.SubjectName)
                .ToListAsync();

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

            var entity = await _context.TxnPgcourseGeneralDetails
                .Include(x => x.TxnPgcourseUnitBedDetails)
                .Include(x => x.TxnPgcourseIcudetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == courseCode &&
                    x.TypeOfAffiliation == typeOfAffiliation);

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
                return View("GeneralDetails", model);
            }

            var entity = await _context.TxnPgcourseGeneralDetails
                .Include(x => x.TxnPgcourseUnitBedDetails)
                .Include(x => x.TxnPgcourseIcudetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == model.CourseCode &&
                    x.TypeOfAffiliation == model.TypeOfAffiliation);

            var isNew = entity == null;

            if (isNew)
            {
                entity = new TxnPgcourseGeneralDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = model.TypeOfAffiliation,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.TxnPgcourseGeneralDetails.Add(entity);
            }
            else
            {
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
                    TypeOfAffiliation = model.TypeOfAffiliation,
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
                    TypeOfAffiliation = model.TypeOfAffiliation,
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
            return RedirectToAction(nameof(SummaryDetails), new { courseCode = model.CourseCode, typeOfAffiliation = model.TypeOfAffiliation });
        }

        // ---- Add these two static fields inside your existing controller class ----

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

        // ---- Add these two action methods inside your existing controller class ----

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

            var entity = await _context.TxnPgcourseSummaryDetails
                .Include(x => x.TxnPgcourseSummaryContactDetails)
                .Include(x => x.TxnPgcourseSummaryInspectionDetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == courseCode &&
                    x.TypeOfAffiliation == typeOfAffiliation);

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
                model.InstitutionCategory = entity.InstitutionCategory;
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

            return View(model);
        }

        //POST: POSSIBLE ALL DATA STORES 

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

            if (!ModelState.IsValid)
            {
                model.FacultyCode = facultyCode;
                model.CollegeCode = collegeCode;
                return View("SummaryDetails", model);
            }

            var entity = await _context.TxnPgcourseSummaryDetails
                .Include(x => x.TxnPgcourseSummaryContactDetails)
                .Include(x => x.TxnPgcourseSummaryInspectionDetails)
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.CollegeCode == collegeCode &&
                    x.CourseCode == model.CourseCode &&
                    x.TypeOfAffiliation == model.TypeOfAffiliation);

            var isNew = entity == null;

            if (isNew)
            {
                entity = new TxnPgcourseSummaryDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = model.CourseCode,
                    TypeOfAffiliation = model.TypeOfAffiliation,
                    CreatedBy = User.Identity?.Name ?? collegeCode,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _context.TxnPgcourseSummaryDetails.Add(entity);
            }
            else
            {
                entity.ModifiedBy = User.Identity?.Name ?? collegeCode;
                entity.ModifiedDate = DateTime.Now;
            }

            entity.DateOfAssessment = model.DateOfAssessment.HasValue ? DateOnly.FromDateTime(model.DateOfAssessment.Value) : null;
            entity.AssessorName = model.AssessorName;
            entity.InstitutionName = model.InstitutionName;
            entity.InstitutionCategory = model.InstitutionCategory;
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
                    TypeOfAffiliation = model.TypeOfAffiliation,
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
                    TypeOfAffiliation = model.TypeOfAffiliation,
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
            return RedirectToAction(nameof(InfrastructureDetails), new { courseCode = model.CourseCode, typeOfAffiliation = model.TypeOfAffiliation });
        }


        // ---- Add these static fields inside your existing controller class ----

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

        // ---- Add these two action methods inside your existing controller class ----

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
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            model.OtherCourses = otherCourseRows.Count > 0
                ? otherCourseRows.Select(x => new OtherCourseRowVM
                {
                    NameOfQualificationCourse = x.NameOfQualificationCourse,
                    PermittedByMcinmc = x.PermittedByMciNmc,
                    NumberOfAdmissionsPerYear = x.NumberOfAdmissionsPerYear
                }).ToList()
                : new List<OtherCourseRowVM> { new(), new() };

            // B.a. OPD header
            var opd = await _context.OpdDetails.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

            if (opd != null)
            {
                model.NoOfRoomsForConsultation = opd.NoOfRoomsForConsultation;
                model.WaitingAreaInSqM = opd.WaitingAreaInSqM;
                model.SpaceAndArrangements = opd.SpaceAndArrangements;
                model.IfNotAdequateReasons = opd.IfNotAdequateReasons;
                model.DressingRoomAvailable = opd.DressingRoomAvailable;
                model.SeparateMinorOtMaleFemale = opd.SeparateMinorOtMaleFemale;
                model.PerRectalExamRoomAvailable = opd.PerRectalExamRoomAvailable;
                model.DressingRoom2Available = opd.DressingRoom2Available;
            }

            // OPD room areas - seed with the 3 fixed room types, keep any extra custom rows saved
            var opdRoomRows = await _context.OpdRoomAreas
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

            if (wardsHeader != null)
            {
                model.NoOfWardsMale = wardsHeader.NoOfWardsMale;
                model.NoOfWardsFemale = wardsHeader.NoOfWardsFemale;
            }

            var wardsParamRows = await _context.WardsParameters
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            foreach (var paramName in WardsParameterDefaults)
            {
                var saved = wardsParamRows.FirstOrDefault(x => x.ParameterName == paramName);
                model.WardsParameters.Add(new WardsParameterRowVM { ParameterName = paramName, Details = saved?.Details });
            }

            // c. OT distribution - dynamic + total, at least 3 blank rows
            var otRows = await _context.OperationTheatreDistributions
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

            if (seminar != null)
            {
                model.SeminarSpaceAndFacility = seminar.SpaceAndFacility;
                model.SeminarInternetFacility = seminar.InternetFacility;
                model.SeminarAvdetails = seminar.AudiovisualEquipmentDetails;
            }

            // h. Departmental Museum
            var museum = await _context.DepartmentalMuseums.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

            if (museum != null)
            {
                model.MuseumSpace = museum.Space;
                model.MuseumTotalSpecimens = museum.TotalNumberOfSpecimens;
                model.MuseumTotalChartDiagram = museum.TotalNumberOfChartDiagram;
            }

            // f. Library facility
            var library = await _context.LibraryFacilities.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            foreach (var eqName in EquipmentDefaults)
            {
                var saved = equipmentRows.FirstOrDefault(x => x.NameOfEquipment == eqName);
                model.Equipment.Add(new DeptEquipmentRowVM
                {
                    NameOfEquipment = eqName,
                    NumbersAvailable = saved?.NumbersAvailable,
                    FunctionalStatus = saved?.FunctionalStatus,
                    IsAdequate = saved?.IsAdequate
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
                return View("InfrastructureDetails", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = model.TypeOfAffiliation;
            var createdBy = User.Identity?.Name ?? collegeCode;

            // ---- 1. Inspection Committee: delete & reinsert ----
            var oldCommittee = _context.LocalInspectionCommittees.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.LocalInspectionCommittees.RemoveRange(oldCommittee);

            foreach (var row in model.InspectionCommittee)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.FeePaidDetails.RemoveRange(oldFees);

            int feeSeq = 1; 
            foreach (var row in model.FeePaidDetails)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.OtherCourseObserverships.RemoveRange(oldOtherCourses);

            foreach (var row in model.OtherCourses)
            {
                _context.OtherCourseObserverships.Add(new OtherCourseObservership
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    NameOfQualificationCourse = row.NameOfQualificationCourse,
                    PermittedByMciNmc = row.PermittedByMcinmc,
                    NumberOfAdmissionsPerYear = row.NumberOfAdmissionsPerYear,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- B.a. OPD header: find-or-create single row ----
            var opd = await _context.OpdDetails.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                opd.ModifiedBy = createdBy;
                opd.ModifiedDate = DateTime.Now;
            }
            opd.NoOfRoomsForConsultation = model.NoOfRoomsForConsultation;
            opd.WaitingAreaInSqM = model.WaitingAreaInSqM;
            opd.SpaceAndArrangements = model.SpaceAndArrangements;
            opd.IfNotAdequateReasons = model.IfNotAdequateReasons;
            opd.DressingRoomAvailable = model.DressingRoomAvailable;
            opd.SeparateMinorOtMaleFemale = model.SeparateMinorOtMaleFemale;
            opd.PerRectalExamRoomAvailable = model.PerRectalExamRoomAvailable;
            opd.DressingRoom2Available = model.DressingRoom2Available;

            // ---- OPD room areas: delete & reinsert ----
            var oldOpdRooms = _context.OpdRoomAreas.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.OpdRoomAreas.RemoveRange(oldOpdRooms);

            foreach (var row in model.OpdRoomAreas)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                wardsHeader.ModifiedBy = createdBy;
                wardsHeader.ModifiedDate = DateTime.Now;
            }
            wardsHeader.NoOfWardsMale = model.NoOfWardsMale;
            wardsHeader.NoOfWardsFemale = model.NoOfWardsFemale;

            // ---- Wards parameters: delete & reinsert ----
            var oldWardsParams = _context.WardsParameters.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.WardsParameters.RemoveRange(oldWardsParams);

            foreach (var row in model.WardsParameters)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.OperationTheatreDistributions.RemoveRange(oldOt);

            int otSeq = 1;
            foreach (var row in model.OperationTheatres)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                seminar.ModifiedBy = createdBy;
                seminar.ModifiedDate = DateTime.Now;
            }
            seminar.SpaceAndFacility = model.SeminarSpaceAndFacility;
            seminar.InternetFacility = model.SeminarInternetFacility;
            seminar.AudiovisualEquipmentDetails = model.SeminarAvdetails;

            // ---- h. Departmental Museum: find-or-create ----
            var museum = await _context.DepartmentalMuseums.FirstOrDefaultAsync(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                museum.ModifiedBy = createdBy;
                museum.ModifiedDate = DateTime.Now;
            }
            museum.Space = model.MuseumSpace;
            museum.TotalNumberOfSpecimens = model.MuseumTotalSpecimens;
            museum.TotalNumberOfChartDiagram = model.MuseumTotalChartDiagram;

            // ---- f. Library facility: find-or-create ----
            var library = await _context.LibraryFacilities.FirstOrDefaultAsync(x => 
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
            _context.DepartmentEquipments.RemoveRange(oldEquipment);

            foreach (var row in model.Equipment)
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
                    IsAdequate = row.IsAdequate,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(ServicesWorkload), new { courseCode, typeOfAffiliation });
        }

        // ---- Add these static fields inside your existing controller class ----

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

        // ---- Add these two action methods inside your existing controller class ----

        // GET: ServicesWorkload?courseCode=..&typeOfAffiliation=..
        [HttpGet]
        public async Task<IActionResult> ServicesWorkload(string courseCode, string typeOfAffiliation)
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

            var model = new ServicesWorkloadViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation
            };

            // C.i. Specialty clinics - 9 fixed rows
            var clinicRows = await _context.SpecialtyClinicDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            foreach (var (clinicName, seq) in SpecialtyClinicDefaults)
            {
                var saved = clinicRows.FirstOrDefault(x => x.ClinicName == clinicName);
                model.SpecialtyClinics.Add(new SpecialtyClinicRowVM
                {
                    ClinicName = clinicName,
                    ClinicSequence = seq,
                    Weekdays = saved?.Weekdays,
                    Timings = saved?.Timings,
                    NumberOfCasesAvg = saved?.NumberOfCasesAvg,
                    ClinicInchargeName = saved?.ClinicInchargeName
                });
            }

            // C.ii. Department services - 3 fixed rows
            var serviceRows = await _context.DepartmentServiceDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            foreach (var (serviceName, seq) in DepartmentServiceDefaults)
            {
                var saved = serviceRows.FirstOrDefault(x => x.ServiceName == serviceName);
                model.DepartmentServices.Add(new DepartmentServiceRowVM
                {
                    ServiceName = serviceName,
                    ServiceSequence = seq,
                    IsAvailable = saved?.IsAvailable,
                    Remarks = saved?.Remarks
                });
            }

            // D. Clinical workload - 21 fixed rows
            var workloadRows = await _context.ClinicalWorkloadDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            foreach (var (particularName, seq) in ClinicalWorkloadDefaults)
            {
                var saved = workloadRows.FirstOrDefault(x => x.ParticularName == particularName);
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
                return View("ServicesWorkload", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = model.TypeOfAffiliation;
            var createdBy = User.Identity?.Name ?? collegeCode;

            // ---- C.i. Specialty clinics: delete & reinsert ----
            var oldClinics = _context.SpecialtyClinicDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                    CreatedDate = DateTime.Now
                });
            }

            // ---- C.ii. Department services: delete & reinsert ----
            var oldServices = _context.DepartmentServiceDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                    IsAvailable = row.IsAvailable,
                    Remarks = row.Remarks,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- D. Clinical workload: delete & reinsert ----
            var oldWorkload = _context.ClinicalWorkloadDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(StaffDetails), new { courseCode, typeOfAffiliation });
        }

        // ---- Add these static fields inside your existing controller class ----

        // ---- Add these static fields inside your existing controller class ----

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

        // ---- Add these two action methods inside your existing controller class ----

        // GET: StaffDetails?courseCode=..&typeOfAffiliation=..&unitNo=..
        [HttpGet]
        public async Task<IActionResult> StaffDetails(string courseCode, string typeOfAffiliation, string? unitNo = null)
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new StaffDetailsViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                CourseCode = courseCode,
                TypeOfAffiliation = typeOfAffiliation,
                UnitNo = unitNo
            };

            // F.i. Unit-wise staff - dynamic, at least 1 blank row, scoped to the given unit
            var staffQuery = _context.StaffUnitWiseDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

            if (!string.IsNullOrEmpty(unitNo))
            {
                staffQuery = staffQuery.Where(x => x.UnitNo == unitNo);
            }

            var staffRows = await staffQuery.OrderBy(x => x.SrNo).ToListAsync();

            model.StaffMembers = staffRows.Count > 0
                ? staffRows.Select(x => new StaffRowVM
                {
                    SrNo = x.SrNo,
                    Designation = x.Designation,
                    Name = x.Name,
                    JoiningDate = x.JoiningDate?.ToDateTime(TimeOnly.MinValue),
                    RelievedRetiredWorking = x.RelievedRetiredWorking,
                    RelievingRetirementDate = x.RelievingRetirementDate?.ToDateTime(TimeOnly.MinValue),
                    AttendanceDaysForYear = x.AttendanceDaysForYear,
                    AttendancePercentage = x.AttendancePercentage,
                    PhoneNo = x.PhoneNo,
                    Email = x.Email
                }).ToList()
                : new List<StaffRowVM> { new() { SrNo = 1 } };

            // F.ii. Eligible faculty - 4 fixed rows
            var eligibleRows = await _context.EligibleFacultyDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
                .ToListAsync();

            foreach (var (designation, seq) in EligibleFacultyDefaults)
            {
                var saved = eligibleRows.FirstOrDefault(x => x.Designation == designation);
                model.EligibleFaculty.Add(new EligibleFacultyRowVM
                {
                    Designation = designation,
                    DesignationSequence = seq,
                    NumberOfFaculty = saved?.NumberOfFaculty,
                    Names = saved?.Names,
                    TotalAdmissionSeats = saved?.TotalAdmissionSeats,
                    IsAdequateForAdmission = saved?.IsAdequateForAdmission
                });
            }

            // F.iii. PG students by year - 3 fixed rows
            var yearRows = await _context.PgStudentsYearWiseDetails
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                .Where(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                return View("StaffDetails", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = model.TypeOfAffiliation;
            var createdBy = User.Identity?.Name ?? collegeCode;

            // ---- F.i. Unit-wise staff: delete & reinsert (scoped to this unit only) ----
            var oldStaff = _context.StaffUnitWiseDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation &&
                x.UnitNo == model.UnitNo);
            _context.StaffUnitWiseDetails.RemoveRange(oldStaff);

            int staffSeq = 1;
            foreach (var row in model.StaffMembers)
            {
                _context.StaffUnitWiseDetails.Add(new StaffUnitWiseDetail
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                    CourseCode = courseCode,
                    TypeOfAffiliation = typeOfAffiliation,
                    UnitNo = model.UnitNo,
                    SrNo = staffSeq++,
                    Designation = row.Designation,
                    Name = row.Name,
                    JoiningDate = row.JoiningDate.HasValue ? DateOnly.FromDateTime(row.JoiningDate.Value) : null,
                    RelievedRetiredWorking = row.RelievedRetiredWorking,
                    RelievingRetirementDate = row.RelievingRetirementDate.HasValue ? DateOnly.FromDateTime(row.RelievingRetirementDate.Value) : null,
                    AttendanceDaysForYear = row.AttendanceDaysForYear,
                    AttendancePercentage = row.AttendancePercentage,
                    PhoneNo = row.PhoneNo,
                    Email = row.Email,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- F.ii. Eligible faculty: delete & reinsert ----
            var oldEligible = _context.EligibleFacultyDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                    IsAdequateForAdmission = row.IsAdequateForAdmission,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                });
            }

            // ---- F.iii. PG students by year: delete & reinsert ----
            var oldYears = _context.PgStudentsYearWiseDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                    CreatedDate = DateTime.Now
                });
            }

            // ---- H. Exam results: delete & reinsert ----
            var oldExamResults = _context.StudentExamResultDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(AcademicActivities), new { courseCode, typeOfAffiliation, unitNo = model.UnitNo });
        }
        // ---- Add this static field inside your existing controller class ----

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

        // ---- Add these two action methods inside your existing controller class ----

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
                            x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation)
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                return View("AcademicActivities", model);
            }

            var courseCode = model.CourseCode;
            var typeOfAffiliation = model.TypeOfAffiliation;
            var createdBy = User.Identity?.Name ?? collegeCode;

            // ---- G. Academic Activities: delete & reinsert ----
            var oldActivities = _context.AcademicActivityDetails.Where(x =>
                x.FacultyCode == facultyCode && x.CollegeCode == collegeCode &&
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);
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
                x.CourseCode == courseCode && x.TypeOfAffiliation == typeOfAffiliation);

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
                summary.ModifiedBy = createdBy;
                summary.ModifiedDate = DateTime.Now;
            }

            summary.PublicationsPast3Years = model.PublicationsPast3Years;
            summary.AssessmentDate = model.AssessmentDate.HasValue ? DateOnly.FromDateTime(model.AssessmentDate.Value) : null;
            summary.LiccommitteeObservations = model.LiccommitteeObservations;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Details saved successfully.";
            return RedirectToAction(nameof(AcademicActivities), new { courseCode, typeOfAffiliation });
        }
    }
}