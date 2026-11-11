using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AyurvedaMinimumConstructedAreaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int AyurvedaFacultyId = 4;
        private const int InPatientZoneId = 6;
        private const string PgCourseLevel = "PG";
        private const string ConstructedAreaAcademicYear = "2027-28";

        public AyurvedaMinimumConstructedAreaController(ApplicationDbContext context) { _context = context; }

        private string? GetCollegeCode() => HttpContext.Session.GetString("CollegeCode");

        private bool IsAyurvedaFaculty()
        {
            string? facultyCode = HttpContext.Session.GetString("FacultyCode");
            return int.TryParse(facultyCode, out int facultyId) && facultyId == AyurvedaFacultyId;
        }

        private IActionResult RedirectToLogin() => RedirectToAction("Login", "Account");

        [HttpGet]
        public async Task<IActionResult> MinimumConstructedAreaAvailability(int? constructedAreaID)
        {
            string? collegeCode = GetCollegeCode();
            if (string.IsNullOrWhiteSpace(collegeCode) || !IsAyurvedaFaculty())
                return RedirectToLogin();

            var areas = await _context.MstAyurvedaConstructedAreas.AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId)
                .OrderBy(x => x.ConstructedAreaId).ToListAsync();

            if (!areas.Any())
                return View(new AyurvedaMinimumConstructedAreaViewModel
                {
                    FacultyId = AyurvedaFacultyId,
                    CollegeCode = collegeCode,
                    CourseLevel = PgCourseLevel,
                    AcademicYear = ConstructedAreaAcademicYear
                });

            int selectedAreaId = constructedAreaID ?? InPatientZoneId;
            var selectedArea = areas.FirstOrDefault(x => x.ConstructedAreaId == selectedAreaId);
            if (selectedArea == null)
            {
                selectedArea = areas.FirstOrDefault(x => x.ConstructedAreaId == InPatientZoneId) ?? areas.First();
                selectedAreaId = selectedArea.ConstructedAreaId;
            }

            var model = new AyurvedaMinimumConstructedAreaViewModel
            {
                FacultyId = AyurvedaFacultyId,
                CollegeCode = collegeCode,
                CourseLevel = PgCourseLevel,
                AcademicYear = ConstructedAreaAcademicYear,
                SelectedConstructedAreaID = selectedAreaId,
                SelectedConstructedAreaName = selectedArea.ConstructedAreaName,
                ConstructedAreas = areas.Select(x => new SelectListItem
                {
                    Value = x.ConstructedAreaId.ToString(),
                    Text = x.ConstructedAreaName,
                    Selected = x.ConstructedAreaId == selectedAreaId
                }).ToList()
            };

            var details = await _context.MstAyurvedaConstructedAreaDetails.AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId && x.ConstructedAreaId == selectedAreaId)
                .OrderBy(x => x.ConstructedAreaDetailId).ToListAsync();

            var detailIds = details.Select(x => x.ConstructedAreaDetailId).ToList();
            var savedRecords = await _context.TblAyurvedaConstructedAreaAvailabilities.AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId && x.CollegeCode == collegeCode
                    && x.CourseLevel == PgCourseLevel && x.AcademicYear == ConstructedAreaAcademicYear
                    && x.ConstructedAreaId == selectedAreaId
                    && detailIds.Contains(x.ConstructedAreaDetailId)).ToListAsync();

            var savedByDetailId = savedRecords.GroupBy(x => x.ConstructedAreaDetailId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var detail in details)
            {
                savedByDetailId.TryGetValue(detail.ConstructedAreaDetailId, out var saved);
                model.Items.Add(new AyurvedaMinimumConstructedAreaItemViewModel
                {
                    ConstructedAreaDetailID = detail.ConstructedAreaDetailId,
                    MegaSectionName = detail.MegaSectionName,
                    SectionName = detail.SectionName,
                    IntakeCapacity60Requirement = detail.IntakeCapacity60,
                    IntakeCapacity100Requirement = detail.IntakeCapacity100,
                    IntakeCapacity150Requirement = detail.IntakeCapacity150,
                    IntakeCapacity200Requirement = detail.IntakeCapacity200,
                    IntakeCapacity60 = saved?.IntakeCapacity60,
                    IntakeCapacity100 = saved?.IntakeCapacity100,
                    IntakeCapacity150 = saved?.IntakeCapacity150,
                    IntakeCapacity200 = saved?.IntakeCapacity200
                });
            }

            return View("MinimumConstructedAreaAvailability", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MinimumConstructedAreaAvailability(AyurvedaMinimumConstructedAreaViewModel model)
        {
            string? collegeCode = GetCollegeCode();
            if (string.IsNullOrWhiteSpace(collegeCode) || !IsAyurvedaFaculty())
                return RedirectToLogin();

            if (model.SelectedConstructedAreaID <= 0)
            {
                TempData["Error"] = "Please select a valid constructed area.";
                return RedirectToAction(nameof(MinimumConstructedAreaAvailability));
            }

            bool areaExists = await _context.MstAyurvedaConstructedAreas.AsNoTracking()
                .AnyAsync(x => x.ConstructedAreaId == model.SelectedConstructedAreaID && x.FacultyId == AyurvedaFacultyId);
            if (!areaExists)
            {
                TempData["Error"] = "The selected constructed area is invalid.";
                return RedirectToAction(nameof(MinimumConstructedAreaAvailability));
            }

            var validDetails = await _context.MstAyurvedaConstructedAreaDetails
                .Where(x => x.FacultyId == AyurvedaFacultyId && x.ConstructedAreaId == model.SelectedConstructedAreaID)
                .ToDictionaryAsync(x => x.ConstructedAreaDetailId);

            if (model.Items == null || model.Items.Any(x => !validDetails.ContainsKey(x.ConstructedAreaDetailID)))
            {
                TempData["Error"] = "Invalid constructed-area details were submitted. Please reload the page.";
                return RedirectToAction(nameof(MinimumConstructedAreaAvailability),
                    new { constructedAreaID = model.SelectedConstructedAreaID });
            }

            foreach (var item in model.Items)
            {
                if (item.IntakeCapacity60 < 0 || item.IntakeCapacity100 < 0
                    || item.IntakeCapacity150 < 0 || item.IntakeCapacity200 < 0)
                {
                    TempData["Error"] = "Area values cannot be negative.";
                    return RedirectToAction(nameof(MinimumConstructedAreaAvailability),
                        new { constructedAreaID = model.SelectedConstructedAreaID });
                }
            }

            var detailIds = validDetails.Keys.ToList();
            var existingRecords = await _context.TblAyurvedaConstructedAreaAvailabilities
                .Where(x => x.FacultyId == AyurvedaFacultyId && x.CollegeCode == collegeCode
                    && x.CourseLevel == PgCourseLevel && x.AcademicYear == ConstructedAreaAcademicYear
                    && x.ConstructedAreaId == model.SelectedConstructedAreaID
                    && detailIds.Contains(x.ConstructedAreaDetailId)).ToListAsync();

            var existingByDetailId = existingRecords.GroupBy(x => x.ConstructedAreaDetailId)
                .ToDictionary(g => g.Key, g => g.First());

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in model.Items)
                {
                    existingByDetailId.TryGetValue(item.ConstructedAreaDetailID, out var saved);
                    decimal? v60 = PositiveOrNull(item.IntakeCapacity60);
                    decimal? v100 = PositiveOrNull(item.IntakeCapacity100);
                    decimal? v150 = PositiveOrNull(item.IntakeCapacity150);
                    decimal? v200 = PositiveOrNull(item.IntakeCapacity200);

                    bool hasValues = v60.HasValue || v100.HasValue || v150.HasValue || v200.HasValue;
                    if (!hasValues)
                    {
                        if (saved != null) _context.TblAyurvedaConstructedAreaAvailabilities.Remove(saved);
                        continue;
                    }

                    if (saved == null)
                    {
                        saved = new TblAyurvedaConstructedAreaAvailability
                        {
                            ConstructedAreaId = model.SelectedConstructedAreaID,
                            ConstructedAreaDetailId = item.ConstructedAreaDetailID,
                            FacultyId = AyurvedaFacultyId,
                            CollegeCode = collegeCode,
                            CourseLevel = PgCourseLevel,
                            AcademicYear = ConstructedAreaAcademicYear,
                            CreatedOn = DateTime.Now
                        };
                        _context.TblAyurvedaConstructedAreaAvailabilities.Add(saved);
                        existingByDetailId[item.ConstructedAreaDetailID] = saved;
                    }

                    saved.ConstructedAreaId = model.SelectedConstructedAreaID;
                    saved.ConstructedAreaDetailId = item.ConstructedAreaDetailID;
                    saved.FacultyId = AyurvedaFacultyId;
                    saved.CollegeCode = collegeCode;
                    saved.CourseLevel = PgCourseLevel;
                    saved.AcademicYear = ConstructedAreaAcademicYear;
                    saved.IntakeCapacity60 = v60;
                    saved.IntakeCapacity100 = v100;
                    saved.IntakeCapacity150 = v150;
                    saved.IntakeCapacity200 = v200;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = "Minimum constructed area availability saved successfully.";
                return RedirectToAction(nameof(MinimumConstructedAreaAvailability),
                    new { constructedAreaID = model.SelectedConstructedAreaID });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while saving constructed area availability.";
                return RedirectToAction(nameof(MinimumConstructedAreaAvailability),
                    new { constructedAreaID = model.SelectedConstructedAreaID });
            }
        }

        private static decimal? PositiveOrNull(decimal? value) =>
            value.HasValue && value.Value > 0 ? value : null;
    }
}

