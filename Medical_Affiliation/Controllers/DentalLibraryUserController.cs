using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers;

public class DentalLibraryUserController : BaseController
{
    public DentalLibraryUserController(ApplicationDbContext context) : base(context)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var collegeContext = GetCollegeContext();
        if (collegeContext == null)
        {
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        var existing = await _context.UserDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeContext.Value.CollegeCode &&
                x.FacultyId == collegeContext.Value.FacultyId &&
                x.TypeId == collegeContext.Value.AffiliationTypeId &&
                x.CourseLevel == collegeContext.Value.CourseLevel &&
                x.IsActive);

        return View(new DentalLibraryUserPageViewModel
        {
            CollegeCode = collegeContext.Value.CollegeCode,
            FacultyId = collegeContext.Value.FacultyId,
            AffiliationTypeId = collegeContext.Value.AffiliationTypeId,
            CourseLevel = collegeContext.Value.CourseLevel,
            Form = existing == null ? new DentalLibraryUserForm() : new DentalLibraryUserForm
            {
                NoOfTeachingStaff = existing.NoOfTeachingStaff ?? 0,
                NoOfResearchScholarsAssistants = existing.NoOfResearchScholarsAssistants ?? 0,
                NoOfPostGraduateStudents = existing.NoOfPostGraduateStudents ?? 0,
                NoOfUnderGraduateStudents = existing.NoOfUnderGraduateStudents ?? 0,
                NoOfAdministrativeStaff = existing.NoOfAdministrativeStaff ?? 0,
                NoOfParaMedicalStaff = existing.NoOfParaMedicalStaff ?? 0,
                NoOfOutsiders = existing.NoOfOutsiders ?? 0,
                ProvideUserEducationProgrammes = existing.ProvideUserEducationProgrammes ?? false
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(DentalLibraryUserPageViewModel model)
    {
        var collegeContext = GetCollegeContext();
        if (collegeContext == null)
        {
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        if (!ModelState.IsValid)
        {
            model.CollegeCode = collegeContext.Value.CollegeCode;
            model.FacultyId = collegeContext.Value.FacultyId;
            model.AffiliationTypeId = collegeContext.Value.AffiliationTypeId;
            model.CourseLevel = collegeContext.Value.CourseLevel;
            return View(nameof(Index), model);
        }

        var entity = await _context.UserDetails.FirstOrDefaultAsync(x =>
            x.CollegeCode == collegeContext.Value.CollegeCode &&
            x.FacultyId == collegeContext.Value.FacultyId &&
            x.TypeId == collegeContext.Value.AffiliationTypeId &&
            x.CourseLevel == collegeContext.Value.CourseLevel);

        if (entity == null)
        {
            _context.UserDetails.Add(new UserDetail
            {
                CollegeCode = collegeContext.Value.CollegeCode,
                FacultyId = collegeContext.Value.FacultyId,
                TypeId = collegeContext.Value.AffiliationTypeId,
                CourseLevel = collegeContext.Value.CourseLevel,
                IsActive = true,
                CreatedBy = collegeContext.Value.CollegeCode,
                CreatedDate = DateTime.Now,
                NoOfTeachingStaff = model.Form.NoOfTeachingStaff,
                NoOfResearchScholarsAssistants = model.Form.NoOfResearchScholarsAssistants,
                NoOfPostGraduateStudents = model.Form.NoOfPostGraduateStudents,
                NoOfUnderGraduateStudents = model.Form.NoOfUnderGraduateStudents,
                NoOfAdministrativeStaff = model.Form.NoOfAdministrativeStaff,
                NoOfParaMedicalStaff = model.Form.NoOfParaMedicalStaff,
                NoOfOutsiders = model.Form.NoOfOutsiders,
                ProvideUserEducationProgrammes = model.Form.ProvideUserEducationProgrammes
            });
        }
        else
        {
            entity.CourseLevel = collegeContext.Value.CourseLevel;
            entity.IsActive = true;
            entity.NoOfTeachingStaff = model.Form.NoOfTeachingStaff;
            entity.NoOfResearchScholarsAssistants = model.Form.NoOfResearchScholarsAssistants;
            entity.NoOfPostGraduateStudents = model.Form.NoOfPostGraduateStudents;
            entity.NoOfUnderGraduateStudents = model.Form.NoOfUnderGraduateStudents;
            entity.NoOfAdministrativeStaff = model.Form.NoOfAdministrativeStaff;
            entity.NoOfParaMedicalStaff = model.Form.NoOfParaMedicalStaff;
            entity.NoOfOutsiders = model.Form.NoOfOutsiders;
            entity.ProvideUserEducationProgrammes = model.Form.ProvideUserEducationProgrammes;
            entity.ModifiedBy = collegeContext.Value.CollegeCode;
            entity.ModifiedDate = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Library user details saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    private (string CollegeCode, int FacultyId, int AffiliationTypeId, string CourseLevel)? GetCollegeContext()
    {
        var collegeCode = HttpContext.Session.GetString("CollegeCode");
        var facultyValue = HttpContext.Session.GetString("FacultyCode");
        var affiliationTypeId = HttpContext.Session.GetInt32("AffiliationType") ?? 0;
        var courseLevel = HttpContext.Session.GetString("CourseLevel")?.Trim().ToUpperInvariant();

        if (affiliationTypeId <= 0 && int.TryParse(
            HttpContext.Session.GetString("AffiliationTypeId") ??
            HttpContext.Session.GetString("TypeOfAffiliationId"), out var parsedId))
        {
            affiliationTypeId = parsedId;
        }

        if (string.IsNullOrWhiteSpace(collegeCode) ||
            !int.TryParse(facultyValue, out var facultyId) ||
            facultyId <= 0 || affiliationTypeId <= 0 ||
            string.IsNullOrWhiteSpace(courseLevel))
        {
            return null;
        }

        return (collegeCode, facultyId, affiliationTypeId, courseLevel);
    }
}
