using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                vm.IsActive = entity.IsActive;
            }

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