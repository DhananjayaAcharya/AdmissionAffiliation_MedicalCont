using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers;

public class DentalLibraryStaffController : BaseController
{
    public DentalLibraryStaffController(ApplicationDbContext context) : base(context)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? editId = null)
    {
        var collegeContext = GetCollegeContext();
        if (collegeContext == null)
        {
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        var page = await BuildPageModelAsync(collegeContext.Value);
        if (editId.HasValue)
        {
            var staff = page.Staff.FirstOrDefault(x => x.LibraryStaffId == editId.Value);
            if (staff != null)
            {
                page.Form = new DentalLibraryStaffForm
                {
                    LibraryStaffId = staff.LibraryStaffId,
                    Name = staff.Name,
                    Designation = staff.Designation,
                    Qualification = staff.Qualification,
                    ExperienceFrom = staff.ExperienceFrom,
                    ExperienceTo = staff.ExperienceTo,
                    CurrentlyWorking = staff.CurrentlyWorking,
                    PayScale = staff.PayScale,
                    Category = staff.Category
                };
            }
        }

        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(DentalLibraryStaffPageViewModel model)
    {
        var collegeContext = GetCollegeContext();
        if (collegeContext == null)
        {
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        if (model.Form.ExperienceFrom.HasValue &&
            model.Form.ExperienceTo.HasValue &&
            model.Form.ExperienceTo < model.Form.ExperienceFrom)
        {
            ModelState.AddModelError("Form.ExperienceTo", "Experience to date cannot be before the from date.");
        }

        if (!ModelState.IsValid)
        {
            var page = await BuildPageModelAsync(collegeContext.Value);
            page.Form = model.Form;
            return View(nameof(Index), page);
        }

        LibraryStaffDetail? entity = null;
        if (model.Form.LibraryStaffId > 0)
        {
            entity = await _context.LibraryStaffDetails.FirstOrDefaultAsync(x =>
                x.LibraryStaffId == model.Form.LibraryStaffId &&
                x.CollegeCode == collegeContext.Value.CollegeCode &&
                x.FacultyId == collegeContext.Value.FacultyId &&
                x.TypeId == collegeContext.Value.AffiliationTypeId &&
                x.CourseLevel == collegeContext.Value.CourseLevel);

            if (entity == null) return NotFound();
        }

        var experienceTo = model.Form.CurrentlyWorking ? null : model.Form.ExperienceTo;
        if (entity == null)
        {
            _context.LibraryStaffDetails.Add(new LibraryStaffDetail
            {
                CollegeCode = collegeContext.Value.CollegeCode,
                FacultyId = collegeContext.Value.FacultyId,
                TypeId = collegeContext.Value.AffiliationTypeId,
                Name = model.Form.Name.Trim(),
                Designation = model.Form.Designation.Trim(),
                Qualification = model.Form.Qualification?.Trim(),
                ExperienceFrom = model.Form.ExperienceFrom,
                ExperienceTo = experienceTo,
                CourseLevel = collegeContext.Value.CourseLevel,
                PayScale = model.Form.PayScale?.Trim(),
                Category = model.Form.Category?.Trim(),
                IsActive = true,
                CreatedBy = collegeContext.Value.CollegeCode,
                CreatedDate = DateTime.Now
            });
        }
        else
        {
            entity.Name = model.Form.Name.Trim();
            entity.Designation = model.Form.Designation.Trim();
            entity.Qualification = model.Form.Qualification?.Trim();
            entity.ExperienceFrom = model.Form.ExperienceFrom;
            entity.ExperienceTo = experienceTo;
            entity.CourseLevel = collegeContext.Value.CourseLevel;
            entity.PayScale = model.Form.PayScale?.Trim();
            entity.Category = model.Form.Category?.Trim();
            entity.IsActive = true;
            entity.ModifiedBy = collegeContext.Value.CollegeCode;
            entity.ModifiedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Library staff details saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var collegeContext = GetCollegeContext();
        if (collegeContext == null) return Unauthorized();

        var entity = await _context.LibraryStaffDetails.FirstOrDefaultAsync(x =>
            x.LibraryStaffId == id &&
            x.CollegeCode == collegeContext.Value.CollegeCode &&
            x.FacultyId == collegeContext.Value.FacultyId &&
            x.TypeId == collegeContext.Value.AffiliationTypeId &&
            x.CourseLevel == collegeContext.Value.CourseLevel);

        if (entity == null) return NotFound();

        entity.IsActive = false;
        entity.ModifiedBy = collegeContext.Value.CollegeCode;
        entity.ModifiedDate = DateTime.Now;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Library staff record removed.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<DentalLibraryStaffPageViewModel> BuildPageModelAsync(
        (string CollegeCode, int FacultyId, int AffiliationTypeId, string CourseLevel) context)
    {
        var staff = await _context.LibraryStaffDetails
            .AsNoTracking()
            .Where(x => x.CollegeCode == context.CollegeCode &&
                        x.FacultyId == context.FacultyId &&
                        x.TypeId == context.AffiliationTypeId &&
                        x.CourseLevel == context.CourseLevel &&
                        x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new DentalLibraryStaffRowViewModel
            {
                LibraryStaffId = x.LibraryStaffId,
                Name = x.Name,
                Designation = x.Designation,
                Qualification = x.Qualification,
                ExperienceFrom = x.ExperienceFrom,
                ExperienceTo = x.ExperienceTo,
                CurrentlyWorking = x.ExperienceTo == null,
                PayScale = x.PayScale,
                Category = x.Category
            })
            .ToListAsync();

        return new DentalLibraryStaffPageViewModel
        {
            CollegeCode = context.CollegeCode,
            FacultyId = context.FacultyId,
            AffiliationTypeId = context.AffiliationTypeId,
            Staff = staff
        };
    }

    private (string CollegeCode, int FacultyId, int AffiliationTypeId, string CourseLevel)? GetCollegeContext()
    {
        var collegeCode = HttpContext.Session.GetString("CollegeCode");
        var facultyValue = HttpContext.Session.GetString("FacultyCode");
        var courseLevel = HttpContext.Session.GetString("CourseLevel")?.Trim().ToUpperInvariant();
        var affiliationTypeId = HttpContext.Session.GetInt32("AffiliationType") ?? 0;

        if (affiliationTypeId <= 0 && int.TryParse(
            HttpContext.Session.GetString("AffiliationTypeId") ??
            HttpContext.Session.GetString("TypeOfAffiliationId"), out var parsedId))
        {
            affiliationTypeId = parsedId;
        }

        if (string.IsNullOrWhiteSpace(collegeCode) ||
            !int.TryParse(facultyValue, out var facultyId) ||
            facultyId <= 0 ||
            affiliationTypeId <= 0 ||
            string.IsNullOrWhiteSpace(courseLevel))
        {
            return null;
        }

        return (collegeCode, facultyId, affiliationTypeId, courseLevel);
    }
}
