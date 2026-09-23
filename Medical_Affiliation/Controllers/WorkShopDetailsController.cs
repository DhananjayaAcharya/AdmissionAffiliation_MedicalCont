using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;

namespace Medical_Affiliation.Controllers
{
    public class WorkShopDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkShopDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var typeId = HttpContext.Session.GetInt32("AffiliationType");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["ErrorMessage"] =
                    "Session information is missing. Please select the required affiliation details.";

                return RedirectToAction("Index", "Home");
            }

            // --------------------------------------------------------
            // Get FacultyId using FacultyCode
            // FacultyCode is stored in AffiliationCollegeMaster
            // --------------------------------------------------------
            //var facultyId = await _context.Faculties
            //    .Where(f =>
            //        f.Status == "Active" &&
            //        _context.AffiliationCollegeMasters.Any(c =>
            //            c.CollegeCode == collegeCode &&
            //            c.FacultyCode == facultyCode))
            //    .Select(f => (int?)f.FacultyId)
            //    .FirstOrDefaultAsync();

            //if (!facultyId.HasValue)
            //{
            //    TempData["ErrorMessage"] = "Faculty information not found.";
            //    return RedirectToAction("Index", "Home");
            //}

            // --------------------------------------------------------
            // Get existing records for current session
            // --------------------------------------------------------
            var records = await _context.WorkShopDetails
                .Include(x => x.Faculty)
                .Include(x => x.Type)
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == int.Parse(facultyCode) &&
                    x.TypeId == typeId.Value &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive)
                .OrderBy(x => x.WorkShopDetailsId)
                .ToListAsync();

            var model = new WorkShopDetailsViewModel
            {
                FacultyId = int.Parse(facultyCode),
                CollegeCode = collegeCode,
                TypeId = typeId.Value,
                CourseLevel = courseLevel,
                WorkshopRecords = records
            };

            return View(model);
        }


        // ============================================================
        // CREATE
        // Only Staff, Equipment and ScopeOfWork come from the view.
        // Faculty / College / Type / CourseLevel come from Session.
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string staff,
            string equipment,
            string scopeOfWork)
        {
            // --------------------------------------------------------
            // Read required values from Session
            // --------------------------------------------------------
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var typeId = HttpContext.Session.GetInt32("AffiliationType");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            // --------------------------------------------------------
            // Validate Session
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                typeId <= 0 ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["ErrorMessage"] =
                    "Session information is missing. Please select the affiliation details again.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Validate entered data
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(staff))
            {
                TempData["ErrorMessage"] = "Please enter Staff details.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(equipment))
            {
                TempData["ErrorMessage"] = "Please enter Equipment details.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(scopeOfWork))
            {
                TempData["ErrorMessage"] = "Please enter Scope of Work.";
                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Validate College
            // --------------------------------------------------------
            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(c =>
                    c.CollegeCode == collegeCode);

            if (college == null)
            {
                TempData["ErrorMessage"] = "College information not found.";
                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Validate Faculty Code against College
            // --------------------------------------------------------
            if (!string.Equals(
                    college.FacultyCode,
                    facultyCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "The selected faculty does not match the college.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Get FacultyId
            // --------------------------------------------------------
            var facultyId = await _context.Faculties
                .Where(f =>
                    f.Status == "Active" &&
                    _context.AffiliationCollegeMasters.Any(c =>
                        c.CollegeCode == collegeCode &&
                        c.FacultyCode == facultyCode))
                .Select(f => (int?)f.FacultyId)
                .FirstOrDefaultAsync();

            if (!facultyId.HasValue)
            {
                TempData["ErrorMessage"] = "Faculty information not found.";
                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Validate Affiliation Type
            // --------------------------------------------------------
            var affiliationTypeExists = await _context.TypeOfAffiliations
                .AnyAsync(t => t.TypeId == typeId.Value);

            if (!affiliationTypeExists)
            {
                TempData["ErrorMessage"] = "Invalid affiliation type.";
                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Duplicate Check
            // --------------------------------------------------------
            var duplicateExists = await _context.WorkShopDetails
                .AnyAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId.Value &&
                    x.TypeId == typeId.Value &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive);

            if (duplicateExists)
            {
                TempData["ErrorMessage"] =
                    "Workshop details already exist for the selected session.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Create Entity
            // --------------------------------------------------------
            var entity = new WorkShopDetail
            {
                FacultyId = facultyId.Value,
                CollegeCode = collegeCode,
                TypeId = typeId.Value,
                CourseLevel = courseLevel.ToUpperInvariant(),

                Staff = staff.Trim(),
                Equipment = equipment.Trim(),
                ScopeOfWork = scopeOfWork.Trim(),

                IsActive = true,

                CreatedBy = HttpContext.Session.GetString("LoginID"),
                CreatedDate = DateTime.Now
            };

            _context.WorkShopDetails.Add(entity);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Workshop details saved successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT GET
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var typeId = HttpContext.Session.GetInt32("AffiliationType");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["ErrorMessage"] =
                    "Session information is missing.";

                return RedirectToAction(nameof(Index));
            }

            var entity = await _context.WorkShopDetails
                .FirstOrDefaultAsync(x =>
                    x.WorkShopDetailsId == id &&
                    x.CollegeCode == collegeCode &&
                    x.IsActive);

            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Workshop record not found.";

                return RedirectToAction(nameof(Index));
            }

            var facultyId = await _context.Faculties
                .Where(f =>
                    f.Status == "Active" &&
                    _context.AffiliationCollegeMasters.Any(c =>
                        c.CollegeCode == collegeCode &&
                        c.FacultyCode == facultyCode))
                .Select(f => (int?)f.FacultyId)
                .FirstOrDefaultAsync();

            if (!facultyId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Faculty information not found.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Security/session validation
            // --------------------------------------------------------
            if (entity.FacultyId != facultyId.Value ||
                entity.TypeId != typeId.Value ||
                !string.Equals(
                    entity.CourseLevel,
                    courseLevel,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "This workshop record does not belong to the current selection.";

                return RedirectToAction(nameof(Index));
            }

            var model = new WorkShopDetailsViewModel
            {
                WorkShopDetailsId = entity.WorkShopDetailsId,

                FacultyId = entity.FacultyId,
                CollegeCode = entity.CollegeCode,
                TypeId = entity.TypeId,
                CourseLevel = entity.CourseLevel,

                Staff = entity.Staff,
                Equipment = entity.Equipment,
                ScopeOfWork = entity.ScopeOfWork,

                WorkshopRecords = await _context.WorkShopDetails
                    .Include(x => x.Faculty)
                    .Include(x => x.Type)
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive)
                    .OrderBy(x => x.WorkShopDetailsId)
                    .ToListAsync()
            };

            return View("Index", model);
        }


        // ============================================================
        // EDIT POST
        // Only editable fields come from View.
        // Session controls Faculty / College / Type / Course Level.
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string staff,
            string equipment,
            string scopeOfWork)
        {
            // --------------------------------------------------------
            // Read session values
            // --------------------------------------------------------
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var typeId = HttpContext.Session.GetInt32("AffiliationType");
            var courseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                typeId <= 0 ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["ErrorMessage"] =
                    "Session information is missing.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Validate fields
            // --------------------------------------------------------
            if (string.IsNullOrWhiteSpace(staff) ||
                string.IsNullOrWhiteSpace(equipment) ||
                string.IsNullOrWhiteSpace(scopeOfWork))
            {
                TempData["ErrorMessage"] =
                    "Staff, Equipment and Scope of Work are required.";

                return RedirectToAction(nameof(Edit), new { id });
            }

            // --------------------------------------------------------
            // Get FacultyId from session FacultyCode
            // --------------------------------------------------------
            var facultyId = await _context.Faculties
                .Where(f =>
                    f.Status == "Active" &&
                    _context.AffiliationCollegeMasters.Any(c =>
                        c.CollegeCode == collegeCode &&
                        c.FacultyCode == facultyCode))
                .Select(f => (int?)f.FacultyId)
                .FirstOrDefaultAsync();

            if (!facultyId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Faculty information not found.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Find record only within current session context
            // --------------------------------------------------------
            var entity = await _context.WorkShopDetails
                .FirstOrDefaultAsync(x =>
                    x.WorkShopDetailsId == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId.Value &&
                    x.TypeId == typeId.Value &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive);

            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Workshop record not found or does not belong to the current selection.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Update ONLY editable fields
            // --------------------------------------------------------
            entity.Staff = staff.Trim();
            entity.Equipment = equipment.Trim();
            entity.ScopeOfWork = scopeOfWork.Trim();

            entity.ModifiedBy =
                HttpContext.Session.GetString("LoginID");

            entity.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Workshop details updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["ErrorMessage"] =
                    "Session information is missing.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Get FacultyId
            // --------------------------------------------------------
            var facultyId = await _context.Faculties
                .Where(f =>
                    f.Status == "Active" &&
                    _context.AffiliationCollegeMasters.Any(c =>
                        c.CollegeCode == collegeCode &&
                        c.FacultyCode == facultyCode))
                .Select(f => (int?)f.FacultyId)
                .FirstOrDefaultAsync();

            if (!facultyId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Faculty information not found.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Find record only within current session context
            // --------------------------------------------------------
            var entity = await _context.WorkShopDetails
                .FirstOrDefaultAsync(x =>
                    x.WorkShopDetailsId == id &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId.Value &&
                    x.TypeId == typeId.Value &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive);

            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Workshop record not found.";

                return RedirectToAction(nameof(Index));
            }

            // --------------------------------------------------------
            // Soft Delete
            // --------------------------------------------------------
            entity.IsActive = false;

            entity.ModifiedBy =
                HttpContext.Session.GetString("LoginID");

            entity.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Workshop record removed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}