using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    [Authorize(Policy = "CollegeOnly")]
    public class AdditionalInformationInAcademicActivitiesController : BaseController
    {
        public AdditionalInformationInAcademicActivitiesController(ApplicationDbContext context) : base(context)
        {
        }

        // ============================================================
        // GET: AdditionalInformationInAcademicActivities/Index
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrWhiteSpace(CollegeCode) ||
                string.IsNullOrWhiteSpace(FacultyCode))
            {
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            int facultyCode = Convert.ToInt32(FacultyCode);

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var typeId = HttpContext.Session.GetString("TypeOfAffiliationId");

            var entity = await _context
                .AdditionalInformationInAcademicActivities
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == CourseLevel &&
                    x.IsActive);

            var vm = new AdditionalInformationInAcademicActivitiesVM
            {
                CollegeCode = CollegeCode,
                FacultyCode = facultyCode,
                CourseLevel = CourseLevel
            };

            if (entity != null)
            {
                vm.Id = entity.Id;
                vm.HasMedicalEducationUnit = entity.HasMedicalEducationUnit;
                vm.TOTProgrammesConducted = entity.TotprogrammesConducted;
                vm.TOTProgrammesAttended = entity.TotprogrammesAttended;
                vm.CMEProgrammePdfPath = entity.CmeprogrammePdfPath;
                vm.HasTOTProgrammesAttended = entity.HasTotprogrammesAttended;
                vm.HasTOTProgrammesConducted = entity.HasTotprogrammesConducted;
                vm.HasCMEProgrammeConducted = entity.HasCmeprogrammesConducted;
                vm.HasCMEProgrammeAttended = entity.HasCmeprogrammesAttended;
                vm.NoOfCMEProgrammesConducted = entity.NoOfCmeprogrammesConducted;
                vm.NoOfCMEProgrammesAttended = entity.NoOfCmeprogrammesAttended;

                vm.IsActive = entity.IsActive;
            }

            vm.ConferencesConducted = await _context
                .DentalConferencesConducteds
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel &&
                    x.TypeId.ToString() == typeId &&
                    x.IsActive)
                .OrderByDescending(x => x.ConferenceDate)
                .Select(x => new DentalConferencesConductedVM
                {
                    Id = x.Id,

                    CollegeCode = x.CollegeCode,

                    FacultyCode = x.FacultyCode,

                    CourseLevel = x.CourseLevel,

                    TypeId = x.TypeId,

                    ConferenceName = x.ConferenceName,

                    ConferencePlace = x.ConferencePlace,

                    ConferenceDate = x.ConferenceDate,

                    IsActive = x.IsActive
                })
                .ToListAsync();

            vm.ConferencesAttended = await _context
                .DentalConferencesAttendeds
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel &&
                    x.TypeId == int.Parse(typeId) &&
                    x.IsActive)
                .OrderBy(x => x.ConferenceDate)
                .Select(x => new DentalConferencesAttendedVM
                {
                    Id = x.Id,

                    CollegeCode = x.CollegeCode,

                    FacultyCode = x.FacultyCode,

                    CourseLevel = x.CourseLevel,

                    TypeId = x.TypeId,

                    ConferenceName = x.ConferenceName,

                    ConferencePlace = x.ConferencePlace,

                    ConferenceDate = x.ConferenceDate,

                    StudentParticipants = x.StudentParticipants,

                    TeacherParticipants = x.TeacherParticipants,

                    TotalParticipants = x.TotalParticipants,

                    IsActive = x.IsActive
                })
                .ToListAsync();


            return View(vm);
        }


        // ============================================================
        // POST: Save
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 3 * 1024 * 1024)]
        public async Task<IActionResult> Save(AdditionalInformationInAcademicActivitiesVM vm)
        {
            // ========================================================
            // Always use authenticated/session values
            // Do not trust hidden fields
            // ========================================================

            if (string.IsNullOrWhiteSpace(CollegeCode) || string.IsNullOrWhiteSpace(FacultyCode))
            {
                return RedirectToAction( "MultiLogin", "MainDashboard");
            }

            int facultyCode = Convert.ToInt32(FacultyCode);

            string collegeCode = CollegeCode;

            string courseLevel = CourseLevel;
            string affiliationTypeId = HttpContext.Session.GetString("TypeOfAffiliationId") ?? AffTypeId.ToString();

            // ========================================================
            // Remove CourseLevel from ModelState
            // ========================================================

            ModelState.Remove(nameof(vm.CourseLevel));

            if (vm.ConferencesAttended != null)
            {
                for (int i = 0; i < vm.ConferencesAttended.Count; i++)
                {
                    ModelState.Remove(
                        $"ConferencesAttended[{i}].CourseLevel"
                    );
                }
            }

            if (vm.ConferencesConducted != null)
            {
                for (int i = 0; i < vm.ConferencesConducted.Count; i++)
                {
                    ModelState.Remove(
                        $"ConferencesConducted[{i}].CourseLevel"
                    );
                }
            }

            // ========================================================
            // Validate CME PDF
            // ========================================================

            if (vm.CMEProgrammePdf != null && vm.CMEProgrammePdf.Length > 0)
            {
                // Maximum 2 MB
                if (vm.CMEProgrammePdf.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError( nameof(vm.CMEProgrammePdf),"CME programme PDF must not exceed 2 MB.");
                }

                // PDF only
                if (!vm.CMEProgrammePdf.FileName .EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError( nameof(vm.CMEProgrammePdf), "Only PDF files are allowed.");
                }
            }

            // ========================================================
            // Validate Model
            // ========================================================

            if (!ModelState.IsValid)
            {
                vm.CollegeCode = collegeCode;
                vm.FacultyCode = facultyCode;
                vm.CourseLevel = courseLevel;

                return View("Index", vm);
            }

            // ========================================================
            // Find existing record
            // ========================================================

            var entity = await _context
                .AdditionalInformationInAcademicActivities
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == courseLevel);

            bool isNew = entity == null;

            // ========================================================
            // INSERT
            // ========================================================

            if (isNew)
            {
                entity =
                    new AdditionalInformationInAcademicActivity
                    {
                        FacultyCode = facultyCode,
                        CollegeCode = collegeCode,
                        CourseLevel = courseLevel,
                        TypeId = int.Parse(affiliationTypeId),
                        HasMedicalEducationUnit = vm.HasMedicalEducationUnit,
                        HasTotprogrammesAttended = vm.HasTOTProgrammesAttended,
                        HasTotprogrammesConducted = vm.HasTOTProgrammesConducted,
                        TotprogrammesConducted = vm.TOTProgrammesConducted,
                        HasCmeprogrammesAttended = vm.HasCMEProgrammeAttended,
                        HasCmeprogrammesConducted = vm.HasCMEProgrammeConducted,
                        NoOfCmeprogrammesAttended = vm.NoOfCMEProgrammesAttended,
                        NoOfCmeprogrammesConducted = vm.NoOfCMEProgrammesConducted,
                        TotprogrammesAttended = vm.TOTProgrammesAttended,
                        IsActive = true,
                        CreatedBy = User.Identity?.Name,
                        CreatedDate = DateTime.Now
                    };

                // ====================================================
                // Save new CME PDF
                // ====================================================

                if (vm.CMEProgrammePdf != null && vm.CMEProgrammePdf.Length > 0)
                {
                    entity.CmeprogrammePdfPath = await SaveFileAndReturnPath( vm.CMEProgrammePdf, "AdditionalInformationInAcademicActivities", "CME");
                }

                _context.AdditionalInformationInAcademicActivities.Add(entity);
            }
            else
            {
                // ====================================================
                // UPDATE
                // ====================================================

                entity.HasMedicalEducationUnit = vm.HasMedicalEducationUnit;
                entity.HasTotprogrammesAttended = vm.HasTOTProgrammesAttended;
                entity.HasTotprogrammesConducted = vm.HasTOTProgrammesConducted;
                entity.HasCmeprogrammesConducted = vm.HasCMEProgrammeConducted;
                entity.HasCmeprogrammesAttended = vm.HasCMEProgrammeAttended;
                entity.NoOfCmeprogrammesConducted = vm.NoOfCMEProgrammesConducted;
                entity.NoOfCmeprogrammesAttended = vm.NoOfCMEProgrammesAttended;

                entity.TotprogrammesConducted = vm.TOTProgrammesConducted;

                entity.TotprogrammesAttended = vm.TOTProgrammesAttended;

                entity.ModifiedBy = User.Identity?.Name;

                entity.ModifiedDate = DateTime.Now;

                // ====================================================
                // Replace CME PDF
                // ====================================================

                if (vm.CMEProgrammePdf != null && vm.CMEProgrammePdf.Length > 0)
                {
                    string? oldFilePath = entity.CmeprogrammePdfPath;

                    // Save new PDF first
                    string? newFilePath =
                        await SaveFileAndReturnPath( vm.CMEProgrammePdf, "AdditionalInformationInAcademicActivities", "CME");

                    if (!string.IsNullOrWhiteSpace(newFilePath))
                    {
                        // Update database path
                        entity.CmeprogrammePdfPath = newFilePath;

                        // Delete old physical file
                        if (!string.IsNullOrWhiteSpace(oldFilePath) && System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Unable to delete old CME PDF: {ex.Message}");
                            }
                        }
                    }
                }
            }

            var existingConferencesConducted =
                await _context.DentalConferencesConducteds
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == courseLevel &&
                        x.TypeId == int.Parse(affiliationTypeId) &&
                        x.IsActive)
                    .ToListAsync();

            var submittedConferenceConductedIds =
                vm.ConferencesConducted?
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToHashSet()
                    ?? new HashSet<int>();


            // ========================================================
            // 10. SOFT DELETE REMOVED CONFERENCES
            // ========================================================

            foreach (var existingConferenceConducted in existingConferencesConducted)
            {
                if (!submittedConferenceConductedIds.Contains(
                        existingConferenceConducted.Id))
                {
                    existingConferenceConducted.IsActive = false;

                    existingConferenceConducted.ModifiedBy =
                        User.Identity?.Name;

                    existingConferenceConducted.ModifiedDate =
                        DateTime.Now;
                }
            }

            if (vm.ConferencesConducted != null)
            {
                foreach (var conferenceConductedVm in vm.ConferencesConducted)
                {
                    // -----------------------------------------------
                    // UPDATE EXISTING CONFERENCE
                    // -----------------------------------------------

                    if (conferenceConductedVm.Id > 0)
                    {
                        var conferenceConductedEntity = existingConferencesConducted.FirstOrDefault( x => x.Id == conferenceConductedVm.Id );

                        if (conferenceConductedEntity != null)
                        {
                            conferenceConductedEntity.ConferenceName = conferenceConductedVm.ConferenceName.Trim();
                            conferenceConductedEntity.ConferencePlace = conferenceConductedVm.ConferencePlace.Trim();
                            conferenceConductedEntity.ConferenceDate = conferenceConductedVm.ConferenceDate!.Value;

                            conferenceConductedEntity.TypeId = int.Parse(affiliationTypeId);
                            conferenceConductedEntity.ModifiedBy = User.Identity?.Name;

                            conferenceConductedEntity.ModifiedDate =  DateTime.Now;

                            conferenceConductedEntity.IsActive = true;
                        }
                    }

                    // -----------------------------------------------
                    // INSERT NEW CONFERENCE
                    // -----------------------------------------------

                    else
                    {
                        var conferenceEntity =
                            new DentalConferencesConducted
                            {
                                CollegeCode = collegeCode,

                                FacultyCode = facultyCode,

                                CourseLevel = courseLevel,

                                TypeId = int.Parse(affiliationTypeId),

                                ConferenceName = conferenceConductedVm.ConferenceName.Trim(),

                                ConferencePlace = conferenceConductedVm.ConferencePlace.Trim(),

                                ConferenceDate = conferenceConductedVm.ConferenceDate!.Value,

                                IsActive = true,

                                CreatedBy = User.Identity?.Name,

                                CreatedDate = DateTime.Now
                            };

                        _context.DentalConferencesConducteds.Add(conferenceEntity);
                    }
                }
            }


            // ========================================================
            // CONFERENCE ATTENDED
            // ========================================================

            var existingConferencesAttended =
                await _context.DentalConferencesAttendeds
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == courseLevel &&
                        x.TypeId == int.Parse(affiliationTypeId) &&
                        x.IsActive)
                    .ToListAsync();


            // ========================================================
            // GET SUBMITTED CONFERENCE IDS
            // ========================================================

            var submittedConferenceAttendedIds =
                vm.ConferencesAttended?
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToHashSet()
                    ?? new HashSet<int>();


            // ========================================================
            // SOFT DELETE REMOVED CONFERENCES
            // ========================================================

            foreach (var existingConferenceAttended in existingConferencesAttended)
            {
                if (!submittedConferenceAttendedIds.Contains(
                        existingConferenceAttended.Id))
                {
                    existingConferenceAttended.IsActive = false;

                    existingConferenceAttended.ModifiedBy =  User.Identity?.Name;

                    existingConferenceAttended.ModifiedDate = DateTime.Now;
                }
            }


            // ========================================================
            // UPDATE / INSERT CONFERENCES
            // ========================================================

            if (vm.ConferencesAttended != null)
            {
                foreach (var conferenceAttendedVm in vm.ConferencesAttended)
                {
                    // ====================================================
                    // UPDATE EXISTING CONFERENCE
                    // ====================================================

                    if (conferenceAttendedVm.Id > 0)
                    {
                        var conferenceAttendedEntity = existingConferencesAttended.FirstOrDefault( x => x.Id == conferenceAttendedVm.Id);

                        if (conferenceAttendedEntity != null)
                        {
                            conferenceAttendedEntity.ConferenceName = conferenceAttendedVm.ConferenceName.Trim();

                            conferenceAttendedEntity.ConferencePlace = conferenceAttendedVm.ConferencePlace.Trim();

                            conferenceAttendedEntity.ConferenceDate = conferenceAttendedVm.ConferenceDate!.Value;

                            conferenceAttendedEntity.StudentParticipants = conferenceAttendedVm.StudentParticipants;

                            conferenceAttendedEntity.TeacherParticipants = conferenceAttendedVm.TeacherParticipants;

                            conferenceAttendedEntity.TotalParticipants = conferenceAttendedVm.TotalParticipants;

                            conferenceAttendedEntity.TypeId = int.Parse(affiliationTypeId);

                            conferenceAttendedEntity.ModifiedBy = User.Identity?.Name;

                            conferenceAttendedEntity.ModifiedDate = DateTime.Now;

                            conferenceAttendedEntity.IsActive = true;
                        }
                    }

                    // ====================================================
                    // INSERT NEW CONFERENCE
                    // ====================================================

                    else
                    {
                        var conferenceEntity =
                            new DentalConferencesAttended
                            {
                                CollegeCode = collegeCode,

                                FacultyCode = facultyCode,

                                CourseLevel = courseLevel,

                                TypeId = int.Parse(affiliationTypeId),

                                ConferenceName = conferenceAttendedVm.ConferenceName.Trim(),

                                ConferencePlace = conferenceAttendedVm.ConferencePlace.Trim(),

                                ConferenceDate = conferenceAttendedVm.ConferenceDate!.Value,

                                StudentParticipants = conferenceAttendedVm.StudentParticipants,

                                TeacherParticipants = conferenceAttendedVm.TeacherParticipants,

                                TotalParticipants = conferenceAttendedVm.TotalParticipants,

                                IsActive = true,

                                CreatedBy = User.Identity?.Name,

                                CreatedDate = DateTime.Now
                            };

                        _context.DentalConferencesAttendeds.Add(conferenceEntity);
                    }
                }
            }


            // ========================================================
            // Save database changes
            // ========================================================

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                isNew
                    ? "Additional academic activity information saved successfully."
                    : "Additional academic activity information updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // View CME PDF
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ViewCMEProgrammePdf(int id)
        {
            if (string.IsNullOrWhiteSpace(CollegeCode) ||
                string.IsNullOrWhiteSpace(FacultyCode))
            {
                return Unauthorized();
            }

            int facultyCode = Convert.ToInt32(FacultyCode);

            var entity = await _context
                .AdditionalInformationInAcademicActivities
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.IsActive);

            if (entity == null ||
                string.IsNullOrWhiteSpace(entity.CmeprogrammePdfPath))
            {
                return NotFound("CME programme PDF not found.");
            }

            if (!System.IO.File.Exists(entity.CmeprogrammePdfPath))
            {
                return NotFound("CME programme PDF file not found.");
            }

            byte[] fileBytes =
                await System.IO.File.ReadAllBytesAsync(
                    entity.CmeprogrammePdfPath);

            return File( fileBytes, "application/pdf");
        }
    }
}