using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;

namespace Medical_Affiliation.Controllers
{
    public class AnimalHouseDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalHouseDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // --------------------------------------------------------
            // Get values from Session
            // --------------------------------------------------------

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");


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

                return RedirectToAction("Index", "Home");
            }


            // --------------------------------------------------------
            // Get FacultyId
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
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction("Index", "Home");
            //}


            // --------------------------------------------------------
            // Get existing Animal House records
            // --------------------------------------------------------

            var records = await _context.AnimalHouseDetails
                .Include(x => x.Faculty)
                .Include(x => x.Type)
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == int.Parse(facultyCode) &&
                    x.TypeId == typeId.Value &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive)
                .OrderBy(x => x.AnimalHouseDetailsId)
                .ToListAsync();


            // --------------------------------------------------------
            // Prepare ViewModel
            // --------------------------------------------------------

            var model = new AnimalHouseDetailsViewModel
            {
                FacultyId = int.Parse(facultyCode),

                CollegeCode = collegeCode,

                TypeId = typeId.Value,

                CourseLevel = courseLevel,

                AnimalHouseRecords = records
            };


            return View(model);
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            decimal? area,
            string? staff,
            string? typeOfAnimals)
        {
            // --------------------------------------------------------
            // Get values from Session
            // --------------------------------------------------------

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");


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
            // Validate Area
            // --------------------------------------------------------

            if (!area.HasValue || area.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid Area.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Staff
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(staff))
            {
                TempData["ErrorMessage"] =
                    "Please enter Staff details.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Type of Animals
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(typeOfAnimals))
            {
                TempData["ErrorMessage"] =
                    "Please enter Type of Animals.";

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
                TempData["ErrorMessage"] =
                    "College information not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Faculty against College
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
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Validate Affiliation Type
            // --------------------------------------------------------

            var affiliationTypeExists =
                await _context.TypeOfAffiliations
                    .AnyAsync(t => t.TypeId == typeId.Value);


            if (!affiliationTypeExists)
            {
                TempData["ErrorMessage"] =
                    "Invalid affiliation type.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Check Duplicate
            // --------------------------------------------------------

            var duplicateExists =
                await _context.AnimalHouseDetails
                    .AnyAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (duplicateExists)
            {
                TempData["ErrorMessage"] =
                    "Animal House details already exist for the current selection.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Create Entity
            // --------------------------------------------------------

            var entity = new AnimalHouseDetail
            {
                FacultyId = int.Parse(facultyCode),

                CollegeCode = collegeCode,

                TypeId = typeId.Value,

                CourseLevel = courseLevel.ToUpperInvariant(),

                Area = area.Value,

                Staff = staff.Trim(),

                TypeOfAnimals = typeOfAnimals.Trim(),

                IsActive = true,

                CreatedBy =
                    HttpContext.Session.GetString("LoginID"),

                CreatedDate = DateTime.Now
            };


            _context.AnimalHouseDetails.Add(entity);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Animal House details saved successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // --------------------------------------------------------
            // Get Session Values
            // --------------------------------------------------------

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");


            // --------------------------------------------------------
            // Validate Session
            // --------------------------------------------------------

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
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Get Record
            // --------------------------------------------------------

            var entity =
                await _context.AnimalHouseDetails
                    .FirstOrDefaultAsync(x =>
                        x.AnimalHouseDetailsId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Animal House record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Prepare ViewModel
            // --------------------------------------------------------

            var model = new AnimalHouseDetailsViewModel
            {
                AnimalHouseDetailsId =
                    entity.AnimalHouseDetailsId,

                FacultyId =
                    entity.FacultyId,

                CollegeCode =
                    entity.CollegeCode,

                TypeId =
                    entity.TypeId,

                CourseLevel =
                    entity.CourseLevel,

                Area =
                    entity.Area,

                Staff =
                    entity.Staff,

                TypeOfAnimals =
                    entity.TypeOfAnimals,

                AnimalHouseRecords =
                    await _context.AnimalHouseDetails
                        .Include(x => x.Faculty)
                        .Include(x => x.Type)
                        .Where(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyId == int.Parse(facultyCode) &&
                            x.TypeId == typeId.Value &&
                            x.CourseLevel == courseLevel &&
                            x.IsActive)
                        .OrderBy(x => x.AnimalHouseDetailsId)
                        .ToListAsync()
            };


            return View("Index", model);
        }


        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            decimal? area,
            string? staff,
            string? typeOfAnimals)
        {
            // --------------------------------------------------------
            // Get Session Values
            // --------------------------------------------------------

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");


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
                    "Session information is missing.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Input
            // --------------------------------------------------------

            if (!area.HasValue || area.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid Area.";

                return RedirectToAction(nameof(Edit), new { id });
            }


            if (string.IsNullOrWhiteSpace(staff))
            {
                TempData["ErrorMessage"] =
                    "Please enter Staff details.";

                return RedirectToAction(nameof(Edit), new { id });
            }


            if (string.IsNullOrWhiteSpace(typeOfAnimals))
            {
                TempData["ErrorMessage"] =
                    "Please enter Type of Animals.";

                return RedirectToAction(nameof(Edit), new { id });
            }


            // --------------------------------------------------------
            // Get FacultyId
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
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Get Record only for current session
            // --------------------------------------------------------

            var entity =
                await _context.AnimalHouseDetails
                    .FirstOrDefaultAsync(x =>
                        x.AnimalHouseDetailsId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Animal House record not found or does not belong to the current selection.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Update ONLY user-entered fields
            // --------------------------------------------------------

            entity.Area =
                area.Value;

            entity.Staff =
                staff.Trim();

            entity.TypeOfAnimals =
                typeOfAnimals.Trim();

            entity.ModifiedBy =
                HttpContext.Session.GetString("LoginID");

            entity.ModifiedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Animal House details updated successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // --------------------------------------------------------
            // Get Session Values
            // --------------------------------------------------------

            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");


            // --------------------------------------------------------
            // Validate Session
            // --------------------------------------------------------

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
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Find record within current session
            // --------------------------------------------------------

            var entity =
                await _context.AnimalHouseDetails
                    .FirstOrDefaultAsync(x =>
                        x.AnimalHouseDetailsId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Animal House record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Soft Delete
            // --------------------------------------------------------

            entity.IsActive = false;

            entity.ModifiedBy =
                HttpContext.Session.GetString("LoginID");

            entity.ModifiedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Animal House record removed successfully.";


            return RedirectToAction(nameof(Index));
        }
    }
}