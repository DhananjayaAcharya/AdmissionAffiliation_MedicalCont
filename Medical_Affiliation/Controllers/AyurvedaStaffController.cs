using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AyurvedaStaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const int AyurvedaFacultyId = 4;

        // MST_AyurvedaStaff.StaffId values
        private const int TeachingStaffId = 1;
        private const int NonTeachingStaffId = 2;
        private const int TeachingDepartmentStaffId = 3;
        private const int HospitalStaffId = 4;

        // Categories that can be saved from this screen
        private static readonly int[] EditableStaffIds =
        {
            TeachingStaffId,
            NonTeachingStaffId,
            TeachingDepartmentStaffId,
            HospitalStaffId
        };

        private const string PgCourseLevel = "PG";
        private const string StaffAcademicYear = "2027-28";

        public AyurvedaStaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetCollegeCode() =>
            HttpContext.Session.GetString("CollegeCode");

        private bool IsAyurvedaFaculty()
        {
            string? facultyCode = HttpContext.Session.GetString("FacultyCode");
            return int.TryParse(facultyCode, out int facultyId)
                   && facultyId == AyurvedaFacultyId;
        }

        private IActionResult RedirectToLogin() =>
            RedirectToAction("Login", "Account");

        [HttpGet]
        public async Task<IActionResult> AyurvedaStaffAvailability(
            int? staffId, string? department)
        {
            string? collegeCode = GetCollegeCode();

            if (string.IsNullOrWhiteSpace(collegeCode) || !IsAyurvedaFaculty())
                return RedirectToLogin();

            var categories = await _context.MstAyurvedaStaffs
                .AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId)
                .OrderBy(x => x.StaffId)
                .ToListAsync();

            int selectedStaffId = staffId ?? TeachingDepartmentStaffId;

            var selectedCategory = categories.FirstOrDefault(x => x.StaffId == selectedStaffId);
            if (selectedCategory == null)
            {
                selectedCategory = categories.FirstOrDefault(x => x.StaffId == TeachingDepartmentStaffId);
                if (selectedCategory == null)
                    return View(new AyurvedaStaffAvailabilityViewModel
                    {
                        CollegeCode = collegeCode,
                        AcademicYear = StaffAcademicYear
                    });

                selectedStaffId = selectedCategory.StaffId;
            }

            var departments = await _context.MstAyurvedaStaffDetails
                .AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId && x.StaffId == selectedStaffId)
                .Select(x => x.StaffDepartments)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            // If no department is given, or the given one does not belong to the
            // selected category (e.g. user switched category), fall back to the first one.
            string? selectedDepartment = string.IsNullOrWhiteSpace(department)
                ? departments.FirstOrDefault()
                : departments.FirstOrDefault(x => x == department) ?? departments.FirstOrDefault();

            var model = new AyurvedaStaffAvailabilityViewModel
            {
                SelectedStaffId = selectedStaffId,
                SelectedStaffName = selectedCategory.StaffName,
                SelectedDepartment = selectedDepartment,
                FacultyId = AyurvedaFacultyId,
                CollegeCode = collegeCode,
                CourseLevel = PgCourseLevel,
                AcademicYear = StaffAcademicYear,
                StaffCategories = categories.Select(x => new SelectListItem
                {
                    Value = x.StaffId.ToString(),
                    Text = x.StaffName,
                    Selected = x.StaffId == selectedStaffId
                }).ToList(),
                Departments = departments.Select(x => new SelectListItem
                {
                    Value = x,
                    Text = x,
                    Selected = x == selectedDepartment
                }).ToList()
            };

            if (selectedDepartment == null)
                return View("AyurvedaStaffAvailability", model);

            var details = await _context.MstAyurvedaStaffDetails
                .AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId
                            && x.StaffId == selectedStaffId
                            && x.StaffDepartments == selectedDepartment)
                .OrderBy(x => x.StaffDetailId)
                .ToListAsync();

            var detailIds = details.Select(x => x.StaffDetailId).ToList();

            var savedRecords = await _context.TblAyurvedaStaffAvailabilities
                .AsNoTracking()
                .Where(x => x.FacultyId == AyurvedaFacultyId
                            && x.CollegeCode == collegeCode
                            && x.CourseLevel == PgCourseLevel
                            && x.AcademicYear == StaffAcademicYear
                            && x.StaffId == selectedStaffId
                            && detailIds.Contains(x.StaffDetailId))
                .ToListAsync();

            var savedByDetailId = savedRecords
                .GroupBy(x => x.StaffDetailId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var detail in details)
            {
                savedByDetailId.TryGetValue(detail.StaffDetailId, out var saved);

                model.Items.Add(new AyurvedaStaffAvailabilityItemViewModel
                {
                    StaffDetailID = detail.StaffDetailId,
                    StaffDepartments = detail.StaffDepartments,
                    StaffSubDepartments = detail.StaffSubDepartments,
                    StaffCadre = detail.StaffCadre,

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

            return View("AyurvedaStaffAvailability", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AyurvedaStaffAvailability(
            AyurvedaStaffAvailabilityViewModel model)
        {
            string? collegeCode = GetCollegeCode();

            if (string.IsNullOrWhiteSpace(collegeCode) || !IsAyurvedaFaculty())
                return RedirectToLogin();

            // Allow Teaching Staff (1), Non-Teaching Staff (2),
            // Teaching Department (3) and Hospital Staff (4)
            if (!EditableStaffIds.Contains(model.SelectedStaffId))
            {
                TempData["Error"] = "Invalid staff category selected.";
                return RedirectToAction(nameof(AyurvedaStaffAvailability));
            }

            if (string.IsNullOrWhiteSpace(model.SelectedDepartment))
            {
                TempData["Error"] = model.SelectedStaffId switch
                {
                    HospitalStaffId => "Please select a hospital department.",
                    NonTeachingStaffId => "Please select a section.",
                    _ => "Please select a teaching department."
                };

                return RedirectToAction(nameof(AyurvedaStaffAvailability),
                    new { staffId = model.SelectedStaffId });
            }

            // Retrieve valid master records for the selected category and department
            var validDetails = await _context.MstAyurvedaStaffDetails
                .Where(x => x.FacultyId == AyurvedaFacultyId
                            && x.StaffId == model.SelectedStaffId
                            && x.StaffDepartments == model.SelectedDepartment)
                .ToDictionaryAsync(x => x.StaffDetailId);

            // Validate submitted detail IDs
            if (model.Items == null
                || model.Items.Any(x => !validDetails.ContainsKey(x.StaffDetailID)))
            {
                TempData["Error"] = "Invalid staff detail records were submitted. Reload the page.";
                return RedirectToAction(nameof(AyurvedaStaffAvailability),
                    new { staffId = model.SelectedStaffId, department = model.SelectedDepartment });
            }

            // Prevent negative availability values
            if (model.Items.Any(x => x.IntakeCapacity60 < 0
                                     || x.IntakeCapacity100 < 0
                                     || x.IntakeCapacity150 < 0
                                     || x.IntakeCapacity200 < 0))
            {
                TempData["Error"] = "Availability values cannot be negative.";
                return RedirectToAction(nameof(AyurvedaStaffAvailability),
                    new { staffId = model.SelectedStaffId, department = model.SelectedDepartment });
            }

            var detailIds = validDetails.Keys.ToList();

            // Retrieve existing saved records
            var existingRecords = await _context.TblAyurvedaStaffAvailabilities
                .Where(x => x.FacultyId == AyurvedaFacultyId
                            && x.CollegeCode == collegeCode
                            && x.CourseLevel == PgCourseLevel
                            && x.AcademicYear == StaffAcademicYear
                            && x.StaffId == model.SelectedStaffId
                            && detailIds.Contains(x.StaffDetailId))
                .ToListAsync();

            var existingByDetailId = existingRecords
                .GroupBy(x => x.StaffDetailId)
                .ToDictionary(g => g.Key, g => g.First());

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in model.Items)
                {
                    // Get the trusted master record for this submitted detail ID
                    var detail = validDetails[item.StaffDetailID];

                    existingByDetailId.TryGetValue(item.StaffDetailID, out var saved);

                    int? q60 = PositiveOrNull(item.IntakeCapacity60);
                    int? q100 = PositiveOrNull(item.IntakeCapacity100);
                    int? q150 = PositiveOrNull(item.IntakeCapacity150);
                    int? q200 = PositiveOrNull(item.IntakeCapacity200);

                    bool hasValues = q60.HasValue || q100.HasValue
                                     || q150.HasValue || q200.HasValue;

                    // If all availability values are empty or zero,
                    // remove the existing saved record
                    if (!hasValues)
                    {
                        if (saved != null)
                            _context.TblAyurvedaStaffAvailabilities.Remove(saved);

                        continue;
                    }

                    // Insert a new record if one does not already exist
                    if (saved == null)
                    {
                        saved = new TblAyurvedaStaffAvailability
                        {
                            StaffId = model.SelectedStaffId,
                            StaffDetailId = item.StaffDetailID,
                            FacultyId = AyurvedaFacultyId,
                            CollegeCode = collegeCode,
                            CourseLevel = PgCourseLevel,
                            AcademicYear = StaffAcademicYear,
                            CreatedOn = DateTime.Now
                        };

                        _context.TblAyurvedaStaffAvailabilities.Add(saved);

                        existingByDetailId[item.StaffDetailID] = saved;
                    }

                    // Update common record details
                    saved.StaffId = model.SelectedStaffId;
                    saved.StaffDetailId = item.StaffDetailID;
                    saved.FacultyId = AyurvedaFacultyId;
                    saved.CollegeCode = collegeCode;
                    saved.CourseLevel = PgCourseLevel;
                    saved.AcademicYear = StaffAcademicYear;

                    // Copy department information from the master table
                    // (for Hospital Staff, StaffSubDepartments is NULL and StaffCadre holds the designation)
                    saved.StaffDepartments = detail.StaffDepartments;
                    saved.StaffSubDepartments = detail.StaffSubDepartments;
                    saved.StaffCadre = detail.StaffCadre;

                    // Update availability quantities
                    saved.IntakeCapacity60 = q60;
                    saved.IntakeCapacity100 = q100;
                    saved.IntakeCapacity150 = q150;
                    saved.IntakeCapacity200 = q200;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Staff availability saved successfully.";

                return RedirectToAction(nameof(AyurvedaStaffAvailability),
                    new
                    {
                        staffId = model.SelectedStaffId,
                        department = model.SelectedDepartment
                    });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] = "An error occurred while saving staff availability.";

                return RedirectToAction(nameof(AyurvedaStaffAvailability),
                    new
                    {
                        staffId = model.SelectedStaffId,
                        department = model.SelectedDepartment
                    });
            }
        }


        private static int? PositiveOrNull(int? value) =>
            value.HasValue && value.Value > 0 ? value : null;
    }
}