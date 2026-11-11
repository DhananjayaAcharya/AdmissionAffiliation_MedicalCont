using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AyurvedaOtherEquipmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const int AyurvedaFacultyId = 4;

        private const int NormalSectionId = 1;

        private const int CapacitySpecificSectionId = 8;

        private const string PgCourseCode = "PG";

        private const string EquipmentAcademicYear = "2027-28";

        public AyurvedaOtherEquipmentController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------
        // SESSION DETAILS
        // -------------------------------------------------------

        private string? GetCollegeCode()
        {
            return HttpContext.Session.GetString("CollegeCode");
        }

        private bool IsAyurvedaFaculty()
        {
            string? facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            return int.TryParse(facultyCode, out int facultyId)
                   && facultyId == AyurvedaFacultyId;
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        // -------------------------------------------------------
        // GET: AYURVEDA OTHER EQUIPMENT AVAILABILITY
        // -------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> AyurvedaOtherEquipmentAvailability(
            int? equipmentID)
        {
            string? collegeCode = GetCollegeCode();

            if (string.IsNullOrWhiteSpace(collegeCode)
                || !IsAyurvedaFaculty())
            {
                return RedirectToLogin();
            }

            // Load Ayurveda equipment sections.
            var sections = await _context
                .MstAyurvedaOtherEquipmentsSubjects
                .AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId)
                .OrderBy(x => x.EquipmentId)
                .ToListAsync();

            if (!sections.Any())
            {
                return View(
                    new AyurvedaOtherEquipmentAvailabilityViewModel
                    {
                        FacultyId = AyurvedaFacultyId,
                        CollegeCode = collegeCode,
                        AcademicYear = EquipmentAcademicYear,
                        CourseCode = PgCourseCode
                    });
            }

            // Default to the first section if no section is selected.
            int selectedEquipmentID = equipmentID
                ?? sections.First().EquipmentId;

            var selectedSection = sections.FirstOrDefault(
                x => x.EquipmentId == selectedEquipmentID);

            // Validate the selected section.
            if (selectedSection == null)
            {
                selectedEquipmentID = sections.First().EquipmentId;

                selectedSection = sections.First();
            }

            // IMPORTANT:
            // Section ID 8 uses four intake-specific quantities.
            // All other sections use one normal quantity.
            bool isCapacitySpecific =
                selectedEquipmentID == CapacitySpecificSectionId;

            var model = new AyurvedaOtherEquipmentAvailabilityViewModel
            {
                FacultyId = AyurvedaFacultyId,

                CollegeCode = collegeCode,

                AcademicYear = EquipmentAcademicYear,

                CourseCode = PgCourseCode,

                SelectedEquipmentID = selectedEquipmentID,

                SelectedSectionName = selectedSection.EquipmentName,

                IsCapacitySpecificSection = isCapacitySpecific,

                Sections = sections.Select(x => new SelectListItem
                {
                    Value = x.EquipmentId.ToString(),

                    Text = x.EquipmentName,

                    Selected = x.EquipmentId == selectedEquipmentID

                }).ToList()
            };

            // ---------------------------------------------------
            // FETCH MASTER EQUIPMENT DETAILS
            // ---------------------------------------------------

            var equipmentDetails = await _context
                .MstAyurvedaOtherEquipmentsDetails
                .AsNoTracking()
                .Where(x =>
                    x.FacultyId == AyurvedaFacultyId
                    && x.EquipmentId == selectedEquipmentID)
                .OrderBy(x => x.EquipmentDetailId)
                .ToListAsync();

            var detailIds = equipmentDetails
                .Select(x => x.EquipmentDetailId)
                .ToList();

            // ---------------------------------------------------
            // FETCH SAVED ACTUAL AVAILABILITY
            // ---------------------------------------------------

            var savedRecords = await _context
                        .TblAyurvedaOtherEquipmentAvailabilities
                        .AsNoTracking()
                        .Where(x =>
                            x.FacultyId == AyurvedaFacultyId
                            && x.CourseCode == PgCourseCode
                            && x.CollegeCode == collegeCode
                            && x.AcademicYear == EquipmentAcademicYear
                            && x.EquipmentSectionId == selectedEquipmentID
                            && detailIds.Contains(x.EquipmentId))
                        .ToListAsync();

            var savedByEquipmentId = savedRecords
                .GroupBy(x => x.EquipmentId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());

            // ---------------------------------------------------
            // MAP MASTER REQUIREMENTS + SAVED ACTUAL VALUES
            // ---------------------------------------------------

            foreach (var detail in equipmentDetails)
            {
                savedByEquipmentId.TryGetValue(
                    detail.EquipmentDetailId,
                    out var saved);

                var item = new AyurvedaOtherEquipmentItemViewModel
                {
                    EquipmentDetailID = detail.EquipmentDetailId,

                    SubSectionName = detail.SubSectionName,

                    EquipmentName = detail.EquipmentName,

                    // Expected requirement.
                    // For Section 8, this is the 60-intake value.
                    MinimumNumberRequired =
                        detail.MinimumNumberRequired,

                    // Expected requirements for other capacities.
                    MinimumNum100 = detail.MinimumNum100,

                    MinimumNum150 = detail.MinimumNum150,

                    MinimumNum200 = detail.MinimumNum200,

                    // Actual saved availability.
                    AvailableQuantity = saved?.AvailableQuantity,

                    AvailableQuantity60 = saved?.AvailableQuantity60,

                    AvailableQuantity100 = saved?.AvailableQuantity100,

                    AvailableQuantity150 = saved?.AvailableQuantity150,

                    AvailableQuantity200 = saved?.AvailableQuantity200
                };

                model.Equipments.Add(item);
            }

            return View(
                "AyurvedaOtherEquipmentAvailability",
                model);
        }

        // -------------------------------------------------------
        // POST: SAVE AYURVEDA OTHER EQUIPMENT AVAILABILITY
        // -------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AyurvedaOtherEquipmentAvailability(
            AyurvedaOtherEquipmentAvailabilityViewModel model)
        {
            string? collegeCode = GetCollegeCode();

            if (string.IsNullOrWhiteSpace(collegeCode)
                || !IsAyurvedaFaculty())
            {
                return RedirectToLogin();
            }

            if (model.SelectedEquipmentID <= 0)
            {
                TempData["Error"] =
                    "Please select a valid equipment section.";

                return RedirectToAction(
                    nameof(AyurvedaOtherEquipmentAvailability));
            }

            // ---------------------------------------------------
            // DETERMINE THE TYPE OF SECTION
            // ---------------------------------------------------

            // Section ID 8 = four capacity-specific actual values.
            // Section ID 1 = one normal actual value.
            bool isCapacitySpecific =
                model.SelectedEquipmentID == CapacitySpecificSectionId;

            // Confirm the section belongs to Ayurveda.
            bool sectionExists = await _context
                .MstAyurvedaOtherEquipmentsSubjects
                .AsNoTracking()
                .AnyAsync(x =>
                    x.EquipmentId == model.SelectedEquipmentID
                    && x.FacultyId == AyurvedaFacultyId);

            if (!sectionExists)
            {
                TempData["Error"] =
                    "The selected equipment section is invalid.";

                return RedirectToAction(
                    nameof(AyurvedaOtherEquipmentAvailability));
            }

            // ---------------------------------------------------
            // FETCH VALID EQUIPMENT DETAILS
            // ---------------------------------------------------

            var validDetails = await _context
                .MstAyurvedaOtherEquipmentsDetails
                .Where(x =>
                    x.FacultyId == AyurvedaFacultyId
                    && x.EquipmentId == model.SelectedEquipmentID)
                .ToDictionaryAsync(x => x.EquipmentDetailId);

            // Prevent invalid equipment IDs from being submitted.
            if (model.Equipments.Any(x =>
                !validDetails.ContainsKey(x.EquipmentDetailID)))
            {
                TempData["Error"] =
                    "Invalid equipment data was submitted. Please reload the page.";

                return RedirectToAction(
                    nameof(AyurvedaOtherEquipmentAvailability),
                    new { equipmentID = model.SelectedEquipmentID });
            }

            // ---------------------------------------------------
            // VALIDATE QUANTITIES
            // ---------------------------------------------------

            foreach (var item in model.Equipments)
            {
                if (item.AvailableQuantity < 0
                    || item.AvailableQuantity60 < 0
                    || item.AvailableQuantity100 < 0
                    || item.AvailableQuantity150 < 0
                    || item.AvailableQuantity200 < 0)
                {
                    TempData["Error"] =
                        "Available quantities cannot be negative.";

                    return RedirectToAction(
                        nameof(AyurvedaOtherEquipmentAvailability),
                        new { equipmentID = model.SelectedEquipmentID });
                }
            }

            // ---------------------------------------------------
            // FETCH EXISTING SAVED RECORDS
            // ---------------------------------------------------

            var detailIds = validDetails.Keys.ToList();

            var existingRecords = await _context
    .TblAyurvedaOtherEquipmentAvailabilities
    .Where(x =>
        x.FacultyId == AyurvedaFacultyId
        && x.CourseCode == PgCourseCode
        && x.CollegeCode == collegeCode
        && x.AcademicYear == EquipmentAcademicYear
        && x.EquipmentSectionId == model.SelectedEquipmentID
        && detailIds.Contains(x.EquipmentId))
    .ToListAsync();

            var existingByEquipmentId = existingRecords
     .GroupBy(x => x.EquipmentId)
     .ToDictionary(
         g => g.Key,
         g => g.First());

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in model.Equipments)
                {
                    // Get the trusted master record.
                    var detail =
                        validDetails[item.EquipmentDetailID];

                    int? normalQuantity = null;

                    int? quantity60 = null;

                    int? quantity100 = null;

                    int? quantity150 = null;

                    int? quantity200 = null;

                    // ------------------------------------------------
                    // SECTION ID 8: FOUR ACTUAL QUANTITIES
                    // ------------------------------------------------

                    if (isCapacitySpecific)
                    {
                        quantity60 = PositiveOrNull(
                            item.AvailableQuantity60);

                        quantity100 = PositiveOrNull(
                            item.AvailableQuantity100);

                        quantity150 = PositiveOrNull(
                            item.AvailableQuantity150);

                        quantity200 = PositiveOrNull(
                            item.AvailableQuantity200);
                    }
                    else
                    {
                        // ------------------------------------------------
                        // NORMAL SECTION: ONE ACTUAL QUANTITY
                        // ------------------------------------------------

                        normalQuantity = PositiveOrNull(
                            item.AvailableQuantity);
                    }

                    bool hasAnyQuantity =
                        normalQuantity.HasValue
                        || quantity60.HasValue
                        || quantity100.HasValue
                        || quantity150.HasValue
                        || quantity200.HasValue;

                    existingByEquipmentId.TryGetValue(
                        item.EquipmentDetailID,
                        out var saved);

                    // ------------------------------------------------
                    // REMOVE RECORD IF ALL QUANTITIES ARE EMPTY
                    // ------------------------------------------------

                    if (!hasAnyQuantity)
                    {
                        if (saved != null)
                        {
                            _context
                                .TblAyurvedaOtherEquipmentAvailabilities
                                .Remove(saved);
                        }

                        continue;
                    }

                    // ------------------------------------------------
                    // INSERT NEW RECORD IF NOT ALREADY SAVED
                    // ------------------------------------------------

                    if (saved == null)
                    {
                        saved =
     new TblAyurvedaOtherEquipmentAvailability
     {
         FacultyId = AyurvedaFacultyId,

         CourseCode = PgCourseCode,

         CollegeCode = collegeCode,

         AcademicYear = EquipmentAcademicYear,

         // Parent section ID: OPD = 1, OPD and ICU = 8, etc.
         EquipmentSectionId = model.SelectedEquipmentID,

         // Individual equipment detail ID.
         EquipmentId = item.EquipmentDetailID,

         CreatedOn = DateTime.Now
     };

                        _context
                            .TblAyurvedaOtherEquipmentAvailabilities
                            .Add(saved);

                        existingByEquipmentId[
                            item.EquipmentDetailID] = saved;
                    }

                    // ------------------------------------------------
                    // UPDATE COMMON DETAILS
                    // ------------------------------------------------

                    // ---------------------------------------------------
                    // UPDATE COMMON DETAILS
                    // ---------------------------------------------------

                    saved.FacultyId = AyurvedaFacultyId;

                    saved.CourseCode = PgCourseCode;

                    saved.CollegeCode = collegeCode;

                    saved.AcademicYear = EquipmentAcademicYear;

                    // Parent section ID.
                    saved.EquipmentSectionId = model.SelectedEquipmentID;

                    // Individual equipment detail ID.
                    saved.EquipmentId = item.EquipmentDetailID;

                    // ------------------------------------------------
                    // SAVE ACTUAL VALUES ONLY
                    // ------------------------------------------------

                    saved.AvailableQuantity = normalQuantity;

                    saved.AvailableQuantity60 = quantity60;

                    saved.AvailableQuantity100 = quantity100;

                    saved.AvailableQuantity150 = quantity150;

                    saved.AvailableQuantity200 = quantity200;

                    saved.IsAvailable = true;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Ayurveda PG equipment availability saved successfully.";

                return RedirectToAction(
                    nameof(AyurvedaOtherEquipmentAvailability),
                    new { equipmentID = model.SelectedEquipmentID });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "An error occurred while saving equipment availability.";

                return RedirectToAction(
                    nameof(AyurvedaOtherEquipmentAvailability),
                    new { equipmentID = model.SelectedEquipmentID });
            }
        }

        // -------------------------------------------------------
        // HELPER: CONVERT ZERO OR NULL TO NULL
        // -------------------------------------------------------

        private static int? PositiveOrNull(int? value)
        {
            return value.HasValue && value.Value > 0
                ? value
                : null;
        }
    }
}