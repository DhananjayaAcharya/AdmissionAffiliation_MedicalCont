using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers;

[Authorize(AuthenticationSchemes = "AdminAuth")]
public class AdminDentalFeeStructureController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminDentalFeeStructureController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool includeInactive = false)
    {
        return View(await BuildPageModelAsync(new AdminDentalFeeStructureForm(), includeInactive));
    }

    [HttpGet]
    public async Task<IActionResult> GetAffiliationTypes(int facultyCode, string? courseLevel)
    {
        if (facultyCode <= 0 || string.IsNullOrWhiteSpace(courseLevel))
        {
            return Json(Array.Empty<object>());
        }

        var level = courseLevel.Trim();
        var types = await _context.MstDentalAffiliationTypes
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.FacultyCode == facultyCode &&
                        (x.CourseLevelGroup == null || x.CourseLevelGroup == level))
            .OrderBy(x => x.AffiliationCategory)
            .Select(x => new
            {
                id = x.DentalAffiliationTypeId,
                name = x.AffiliationCategory,
                academicYear = x.AcademicYear
            })
            .ToListAsync();

        return Json(types);
    }

    [HttpGet]
    public async Task<IActionResult> GetFeeTypes(int facultyCode, int affiliationTypeId, string? courseLevel)
    {
        if (facultyCode <= 0 || affiliationTypeId <= 0 || string.IsNullOrWhiteSpace(courseLevel))
        {
            return Json(Array.Empty<object>());
        }

        var level = courseLevel.Trim();
        var feeTypes = await _context.MstDentalFeeTypes
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.FacultyCode == facultyCode &&
                        x.AffiliationTypeId == affiliationTypeId &&
                        x.CourseLevel == level)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.FeeType)
            .Select(x => new { id = x.Id, feeType = x.FeeType })
            .ToListAsync();

        return Json(feeTypes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] AdminDentalFeeStructureForm form)
    {
        if (!await ValidateFormAsync(form))
        {
            return View(nameof(Index), await BuildPageModelAsync(form));
        }

        var entity = new MstDentalFeeStructure
        {
            FacultyCode = form.FacultyCode,
            FeeTypeId = form.FeeTypeId,
            CourseName = GetCourseName(),
            CourseCode = null,
            CourseLevel = form.CourseLevel.Trim(),
            AmountToBePaid = form.AmountToBePaid,
            CalculationType = form.CalculationType?.Trim(),
            AffiliationTypeId = form.AffiliationTypeId,
            IsActive = true,
            CreatedBy = User.Identity?.Name ?? "Admin",
            CreatedDate = DateTime.Now
        };

        _context.MstDentalFeeStructures.Add(entity);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Dental fee structure created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _context.MstDentalFeeStructures.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();

        var form = new AdminDentalFeeStructureForm
        {
            Id = entity.Id,
            FacultyCode = entity.FacultyCode,
            FeeTypeId = entity.FeeTypeId,
            CourseLevel = entity.CourseLevel ?? string.Empty,
            AmountToBePaid = entity.AmountToBePaid,
            CalculationType = entity.CalculationType,
            AffiliationTypeId = entity.AffiliationTypeId
        };

        return View(nameof(Index), await BuildPageModelAsync(form));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "Form")] AdminDentalFeeStructureForm form)
    {
        var entity = await _context.MstDentalFeeStructures.FirstOrDefaultAsync(x => x.Id == form.Id);
        if (entity == null) return NotFound();

        if (!await ValidateFormAsync(form, form.Id))
        {
            return View(nameof(Index), await BuildPageModelAsync(form));
        }

        entity.FacultyCode = form.FacultyCode;
        entity.FeeTypeId = form.FeeTypeId;
        entity.CourseName = GetCourseName();
        entity.CourseCode = null;
        entity.CourseLevel = form.CourseLevel.Trim();
        entity.AmountToBePaid = form.AmountToBePaid;
        entity.CalculationType = form.CalculationType?.Trim();
        entity.AffiliationTypeId = form.AffiliationTypeId;
        entity.ModifiedBy = User.Identity?.Name ?? "Admin";
        entity.ModifiedDate = DateTime.Now;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Dental fee structure updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.MstDentalFeeStructures.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();

        _context.MstDentalFeeStructures.Remove(entity);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Dental fee structure deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var entity = await _context.MstDentalFeeStructures.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();

        entity.IsActive = !entity.IsActive;
        entity.ModifiedBy = User.Identity?.Name ?? "Admin";
        entity.ModifiedDate = DateTime.Now;
        await _context.SaveChangesAsync();
        TempData["Success"] = entity.IsActive ? "Dental fee structure activated." : "Dental fee structure deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateFormAsync(AdminDentalFeeStructureForm form, int? excludedId = null)
    {
        if (!ModelState.IsValid) return false;

        if (!await _context.Faculties.AnyAsync(x => x.FacultyId == form.FacultyCode && x.Status == "Active"))
        {
            ModelState.AddModelError("Form.FacultyCode", "Select an active faculty.");
        }

        if (!await _context.MstDentalFeeTypes.AnyAsync(x =>
            x.Id == form.FeeTypeId &&
            x.IsActive &&
            x.FacultyCode == form.FacultyCode &&
            x.AffiliationTypeId == form.AffiliationTypeId &&
            x.CourseLevel == form.CourseLevel.Trim()))
        {
            ModelState.AddModelError("Form.FeeTypeId", "Select an active fee type for the selected faculty.");
        }

        if (!await _context.MstDentalAffiliationTypes.AnyAsync(x =>
            x.DentalAffiliationTypeId == form.AffiliationTypeId &&
            x.IsActive &&
            x.FacultyCode == form.FacultyCode &&
            (x.CourseLevelGroup == null || x.CourseLevelGroup == form.CourseLevel.Trim())))
        {
            ModelState.AddModelError("Form.AffiliationTypeId", "Select an active affiliation type for the selected faculty.");
        }

        var duplicateExists = await _context.MstDentalFeeStructures.AnyAsync(x =>
            x.Id != excludedId &&
            x.FacultyCode == form.FacultyCode &&
            x.FeeTypeId == form.FeeTypeId &&
            x.CourseName == GetCourseName() &&
            x.CourseLevel == form.CourseLevel.Trim() &&
            x.AffiliationTypeId == form.AffiliationTypeId);

        if (duplicateExists)
        {
            ModelState.AddModelError("Form.FeeTypeId", "This fee type already has a structure for the selected course level.");
        }

        return ModelState.IsValid;
    }

    private static string GetCourseName()
    {
        return "BDS";
    }

    private async Task<AdminDentalFeeStructurePageViewModel> BuildPageModelAsync(
        AdminDentalFeeStructureForm form,
        bool includeInactive = false)
    {
        var query = _context.MstDentalFeeStructures
            .AsNoTracking()
            .Include(x => x.FacultyCodeNavigation)
            .Include(x => x.FeeType)
            .AsQueryable();

        if (!includeInactive) query = query.Where(x => x.IsActive);

        var rows = await query
            .OrderBy(x => x.FacultyCodeNavigation.FacultyName)
            .ThenBy(x => x.CourseLevel)
            .ThenBy(x => x.CourseName)
            .Select(x => new AdminDentalFeeStructureRowViewModel
            {
                Id = x.Id,
                FacultyCode = x.FacultyCode,
                FeeTypeId = x.FeeTypeId,
                AffiliationTypeId = x.AffiliationTypeId,
                FacultyName = x.FacultyCodeNavigation.FacultyName,
                FeeType = x.FeeType.FeeType,
                CourseName = x.CourseName,
                CourseCode = x.CourseCode,
                CourseLevel = x.CourseLevel,
                AmountToBePaid = x.AmountToBePaid,
                CalculationType = x.CalculationType,
                AffiliationType = _context.MstDentalAffiliationTypes
                    .Where(a => a.DentalAffiliationTypeId == x.AffiliationTypeId)
                    .Select(a => a.AffiliationCategory)
                    .FirstOrDefault() ?? "-",
                IsActive = x.IsActive
            })
            .ToListAsync();

        var faculties = await _context.Faculties.AsNoTracking()
            .Where(x => x.Status == "Active")
            .OrderBy(x => x.FacultyName)
            .Select(x => new SelectListItem { Value = x.FacultyId.ToString(), Text = x.FacultyName })
            .ToListAsync();

        var affiliationQuery = _context.MstDentalAffiliationTypes.AsNoTracking()
            .Where(x => x.IsActive);

        var affiliationTypes = await affiliationQuery
            .OrderBy(x => x.AffiliationCategory)
            .Select(x => new SelectListItem { Value = x.DentalAffiliationTypeId.ToString(), Text = x.AffiliationCategory + " (Faculty " + x.FacultyCode + ")" })
            .ToListAsync();

        var feeTypeQuery = _context.MstDentalFeeTypes.AsNoTracking().Where(x => x.IsActive);

        var feeTypes = await feeTypeQuery
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.FeeType)
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.FeeType })
            .ToListAsync();

        return new AdminDentalFeeStructurePageViewModel
        {
            Form = form,
            FeeStructures = rows,
            Faculties = faculties,
            FeeTypes = feeTypes,
            AffiliationTypes = affiliationTypes,
            IncludeInactive = includeInactive
        };
    }
}
