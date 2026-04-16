using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class AffiliationDeclarationController : BaseController
    {
        public readonly ApplicationDbContext _context;
        public readonly IUserContext _userContext;

        public AffiliationDeclarationController(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> Declaration()
        {
            var collegeCode = _userContext.CollegeCode;
            int facultyCode = _userContext.FacultyId;
            int affiliationTypeId = _userContext.TypeOfAffiliation;

            var data = await _context.AffiliationFinalDeclarations
                .Where(x => x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.AffiliationTypeId == affiliationTypeId)
                .Select(x => new AffiliationFinalDeclarationViewModel
                {
                    Id = x.Id,
                    PrincipalName = x.PrincipalName,
                    IsSubmitted = x.IsSubmitted
                })
                .FirstOrDefaultAsync();

            // 👉 If no record, return empty model
            if (data == null)
            {
                data = new AffiliationFinalDeclarationViewModel();
            }

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDeclaration(AffiliationFinalDeclarationViewModel model)
        {
            var collegeCode = _userContext.CollegeCode;
            int facultyCode = _userContext.FacultyId;
            int affiliationTypeId = _userContext.TypeOfAffiliation;

            // 🔍 Check existing record
            var entity = await _context.AffiliationFinalDeclarations
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.AffiliationTypeId == affiliationTypeId);

            if (entity != null && entity.IsSubmitted)
            {
                TempData["Error"] = "Already submitted. No further changes allowed.";
                return RedirectToAction("Declaration");
            }

            if (entity == null)
            {
                // ➕ INSERT
                entity = new AffiliationFinalDeclaration
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    AffiliationTypeId = affiliationTypeId,
                    CreatedDate = DateTime.Now
                };

                _context.AffiliationFinalDeclarations.Add(entity);
            }

            // 🔄 COMMON UPDATE
            entity.PrincipalName = model.PrincipalName;
            entity.IsSubmitted = true;
            entity.SubmittedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            //TempData["Success"] = "Declaration submitted successfully";

            // 🚀 REDIRECT TO PREVIEW PAGE
            return RedirectToAction("Preview", "CAPreview");
        }
    }
}
