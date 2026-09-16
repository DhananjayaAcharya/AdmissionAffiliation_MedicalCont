using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers;

public class DentalLibraryController : BaseController
{
    public DentalLibraryController(ApplicationDbContext context) : base(context)
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

        return View(await BuildPageModelAsync(
            collegeContext.Value.CollegeCode,
            collegeContext.Value.FacultyId,
            collegeContext.Value.courseLevel,
            collegeContext.Value.AffiliationTypeId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(DentalLibraryPageViewModel model)
    {
        var collegeContext = GetCollegeContext();
        if (collegeContext == null)
        {
            return RedirectToAction("MultiLogin", "MainDashboard");
        }

        var courseLevel = HttpContext.Session.GetString("CourseLevel");

        var validExpenditureIds = await _context.MstLibraryExpenditures
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.FacultyId == collegeContext.Value.FacultyId &&
                        x.TypeId == collegeContext.Value.AffiliationTypeId)
            .Select(x => x.LibraryExpenditureId)
            .ToListAsync();

        var submittedExpenditures = (model.Expenditures ?? new List<DentalLibraryExpenditureRowViewModel>())
            .Where(x => validExpenditureIds.Contains(x.ItemId))
            .GroupBy(x => x.ItemId)
            .Select(x => x.Last())
            .ToList();

        var savedExpenditures = await _context.LibraryExpenditures
            .Where(x => x.CollegeCode == collegeContext.Value.CollegeCode &&
                        x.CourseLevel.ToUpper() == courseLevel.ToUpper() &&
                        validExpenditureIds.Contains(x.ItemId))
            .ToListAsync();

        foreach (var submitted in submittedExpenditures)
        {
            var saved = savedExpenditures.FirstOrDefault(x => x.ItemId == submitted.ItemId);

            if (submitted.ExpenditureProposed <= 0)
            {
                if (saved != null)
                {
                    saved.IsActive = false;
                    saved.ModifiedBy = collegeContext.Value.CollegeCode;
                    saved.ModifiedDate = DateTime.Now;
                }

                continue;
            }

            if (saved == null)
            {
                _context.LibraryExpenditures.Add(new LibraryExpenditure
                {
                    CollegeCode = collegeContext.Value.CollegeCode,
                    CourseLevel = courseLevel.ToUpper(),
                    ItemId = submitted.ItemId,
                    ExpenditureProposed = submitted.ExpenditureProposed,
                    IsActive = true,
                    CreatedBy = collegeContext.Value.CollegeCode,
                    CreatedDate = DateTime.Now
                });
            }
            else
            {
                saved.ExpenditureProposed = submitted.ExpenditureProposed;
                saved.IsActive = true;
                saved.ModifiedBy = collegeContext.Value.CollegeCode;
                saved.ModifiedDate = DateTime.Now;
            }
        }

        var validServiceIds = await _context.MstDentalLibraryServices
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.FacultyId == collegeContext.Value.FacultyId &&
                        x.TypeId == collegeContext.Value.AffiliationTypeId)
            .Select(x => x.DentalLibraryServiceId)
            .ToListAsync();

        var submittedServices = (model.Services ?? new List<DentalLibraryServiceRowViewModel>())
            .Where(x => validServiceIds.Contains(x.ServiceId))
            .GroupBy(x => x.ServiceId)
            .Select(x => x.Last())
            .ToList();

        var savedServices = await _context.DentalLibraryServices
            .Where(x => x.CollegeCode == collegeContext.Value.CollegeCode &&
                        x.CourseLevel == courseLevel.ToUpper() &&
                        validServiceIds.Contains(x.ServiceId))
            .ToListAsync();

        foreach (var submitted in submittedServices)
        {
            var saved = savedServices.FirstOrDefault(x => x.ServiceId == submitted.ServiceId);

            if (saved == null)
            {
                _context.DentalLibraryServices.Add(new DentalLibraryService
                {
                    CollegeCode = collegeContext.Value.CollegeCode,
                    CourseLevel = courseLevel.ToUpper(),
                    ServiceId = submitted.ServiceId,
                    IsAvailable = submitted.IsAvailable,
                    IsActive = true,
                    CreatedBy = collegeContext.Value.CollegeCode,
                    CreatedDate = DateTime.Now
                });
            }
            else
            {
                saved.IsAvailable = submitted.IsAvailable;
                saved.IsActive = true;
                saved.ModifiedBy = collegeContext.Value.CollegeCode;
                saved.ModifiedDate = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Dental library details saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<DentalLibraryPageViewModel> BuildPageModelAsync(
        string collegeCode,
        int facultyId,
        string courseLevel,
        int affiliationTypeId)
    {
        var masterExpenditures = await _context.MstLibraryExpenditures
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.FacultyId == facultyId &&
                        x.TypeId == affiliationTypeId)
            .OrderBy(x => x.ItemName)
            .ToListAsync();

        var savedExpenditures = await _context.LibraryExpenditures
            .AsNoTracking()
            .Where(x => x.CollegeCode == collegeCode && x.CourseLevel == courseLevel  && x.IsActive)
            .ToDictionaryAsync(x => x.ItemId, x => x.ExpenditureProposed);

        var masterServices = await _context.MstDentalLibraryServices
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.FacultyId == facultyId &&
                        x.TypeId == affiliationTypeId)
            .OrderBy(x => x.ServiceName)
            .ToListAsync();

        var savedServices = await _context.DentalLibraryServices
            .AsNoTracking()
            .Where(x => x.CollegeCode == collegeCode && x.CourseLevel == courseLevel && x.IsActive)
            .ToDictionaryAsync(x => x.ServiceId, x => x.IsAvailable);

        return new DentalLibraryPageViewModel
        {
            CollegeCode = collegeCode,
            FacultyId = facultyId,
            AffiliationTypeId = affiliationTypeId,
            Expenditures = masterExpenditures.Select(item => new DentalLibraryExpenditureRowViewModel
            {
                ItemId = item.LibraryExpenditureId,
                ItemName = item.ItemName,
                ExpenditureProposed = savedExpenditures.GetValueOrDefault(item.LibraryExpenditureId)
            }).ToList(),
            Services = masterServices.Select(service => new DentalLibraryServiceRowViewModel
            {
                ServiceId = service.DentalLibraryServiceId,
                ServiceName = service.ServiceName,
                IsAvailable = savedServices.GetValueOrDefault(service.DentalLibraryServiceId)
            }).ToList()
        };
    }

    private (string CollegeCode, int FacultyId, int AffiliationTypeId, string courseLevel)? GetCollegeContext()
    {
        var collegeCode = HttpContext.Session.GetString("CollegeCode");
        var facultyValue = HttpContext.Session.GetString("FacultyCode");
        var courseLevel = HttpContext.Session.GetString("CourseLevel");
        var affiliationTypeId = HttpContext.Session.GetInt32("AffiliationType") ?? 0;

        if (affiliationTypeId <= 0 &&
            int.TryParse(
                HttpContext.Session.GetString("AffiliationTypeId") ??
                HttpContext.Session.GetString("TypeOfAffiliationId"),
                out var parsedAffiliationTypeId))
        {
            affiliationTypeId = parsedAffiliationTypeId;
        }

        if (string.IsNullOrWhiteSpace(collegeCode) ||
            !int.TryParse(facultyValue, out var facultyId) ||
            facultyId <= 0 ||
            affiliationTypeId <= 0)
        {
            return null;
        }

        return (collegeCode, facultyId, affiliationTypeId, courseLevel);
    }
}
