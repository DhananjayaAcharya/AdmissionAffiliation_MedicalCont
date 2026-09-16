using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.Models;
using Medical_Affiliation.DATA;

namespace Medical_Affiliation.Controllers
{
    public class DentalFieldPracticeAreaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DentalFieldPracticeAreaController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // --------------------------------------------------------
            // Get session values
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
            // Validate session
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
            // Get FacultyId using FacultyCode + College
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

                return RedirectToAction("Index", "Home");
            }


            // --------------------------------------------------------
            // Get existing records
            // --------------------------------------------------------

            var records =
                await _context.DentalFieldPracticeAreas
                    .Include(x => x.Faculty)
                    .Include(x => x.Type)
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive)
                    .OrderBy(x => x.DentalFieldPracticeAreaId)
                    .ToListAsync();


            // --------------------------------------------------------
            // Prepare ViewModel
            // --------------------------------------------------------

            var model = new DentalFieldPracticeAreaViewModel
            {
                FacultyId = facultyId.Value,

                CollegeCode = collegeCode,

                TypeId = typeId.Value,

                CourseLevel = courseLevel,

                DentalFieldPracticeRecords = records
            };


            return View(model);
        }


        // ============================================================
        // CREATE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string? location,
            string? address,
            string? managedBy,
            IFormFile? staffListFile,
            int? populationServed)
        {
            // --------------------------------------------------------
            // Get session values
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
            // Validate session
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
            // Validate Location
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(location))
            {
                TempData["ErrorMessage"] =
                    "Please enter Location.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Address
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["ErrorMessage"] =
                    "Please enter Address.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Managed By
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(managedBy))
            {
                TempData["ErrorMessage"] =
                    "Please enter Managed By.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Population Served
            // --------------------------------------------------------

            if (!populationServed.HasValue ||
                populationServed.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid Population Served.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate College
            // --------------------------------------------------------

            var college =
                await _context.AffiliationCollegeMasters
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
            // Check duplicate
            // --------------------------------------------------------

            var duplicateExists =
                await _context.DentalFieldPracticeAreas
                    .AnyAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (duplicateExists)
            {
                TempData["ErrorMessage"] =
                    "Dental Field Practice Area details already exist for the current selection.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Upload Staff List
            // --------------------------------------------------------

            string? staffListPath = null;

            if (staffListFile != null && staffListFile.Length > 0)
            {
                const long maxFileSize = 2 * 1024 * 1024; // 2 MB

                var extension = Path.GetExtension(staffListFile.FileName)
                    .ToLowerInvariant();

                // PDF only
                if (extension != ".pdf")
                {
                    TempData["ErrorMessage"] =
                        "Only PDF files are allowed for Staff List.";

                    return RedirectToAction(nameof(Index));
                }

                // Maximum 2 MB
                if (staffListFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] =
                        "Staff List PDF size must not exceed 2 MB.";

                    return RedirectToAction(nameof(Index));
                }

                // ----------------------------------------------------
                // Base path - same paths used by BaseController
                // ----------------------------------------------------

                string basePath = facultyCode == "2"
                    ? (Directory.Exists(@"E:\")
                        ? @"E:\Affiliation_Dental"
                        : @"D:\Affiliation_Dental")
                    : (Directory.Exists(@"E:\")
                        ? @"E:\Affiliation_Medical"
                        : @"D:\Affiliation_Medical");

                // DentalFieldPracticeArea folder
                string uploadFolder = Path.Combine(
                    basePath,
                    "DentalFieldPracticeArea"
                );

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Always save as PDF
                var fileName = $"{Guid.NewGuid():N}.pdf";

                var filePath = Path.Combine(
                    uploadFolder,
                    fileName
                );

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await staffListFile.CopyToAsync(stream);
                }

                // Store ONLY the physical file path
                staffListPath = filePath;
            }

            // --------------------------------------------------------
            // Create entity
            // --------------------------------------------------------

            var entity = new DentalFieldPracticeArea
            {
                FacultyId =
                    facultyId.Value,

                CollegeCode =
                    collegeCode,

                TypeId =
                    typeId.Value,

                CourseLevel =
                    courseLevel.ToUpperInvariant(),

                Location =
                    location.Trim(),

                Address =
                    address.Trim(),

                ManagedBy =
                    managedBy.Trim(),

                StaffList =
                    staffListPath,

                PopulationServed =
                    populationServed.Value,

                IsActive =
                    true,

                CreatedBy =
                    HttpContext.Session.GetString("LoginID"),

                CreatedDate =
                    DateTime.Now
            };


            _context.DentalFieldPracticeAreas.Add(entity);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Dental Field Practice Area details saved successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // --------------------------------------------------------
            // Get session values
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
            // Validate session
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
            // Get record
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Dental Field Practice Area record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Prepare ViewModel
            // --------------------------------------------------------

            var records =
                await _context.DentalFieldPracticeAreas
                    .Include(x => x.Faculty)
                    .Include(x => x.Type)
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive)
                    .OrderBy(x => x.DentalFieldPracticeAreaId)
                    .ToListAsync();


            var model = new DentalFieldPracticeAreaViewModel
            {
                DentalFieldPracticeAreaId =
                    entity.DentalFieldPracticeAreaId,

                FacultyId =
                    entity.FacultyId,

                CollegeCode =
                    entity.CollegeCode,

                TypeId =
                    entity.TypeId,

                CourseLevel =
                    entity.CourseLevel,

                Location =
                    entity.Location,

                Address =
                    entity.Address,

                ManagedBy =
                    entity.ManagedBy,

                StaffList =
                    entity.StaffList,

                PopulationServed =
                    entity.PopulationServed,

                DentalFieldPracticeRecords =
                    records
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
            string? location,
            string? address,
            string? managedBy,
            IFormFile? staffListFile,
            int? populationServed)
        {
            // --------------------------------------------------------
            // Get session values
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
            // Validate session
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
            // Validate input
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(location))
            {
                TempData["ErrorMessage"] =
                    "Please enter Location.";

                return RedirectToAction(nameof(Edit), new { id });
            }


            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["ErrorMessage"] =
                    "Please enter Address.";

                return RedirectToAction(nameof(Edit), new { id });
            }


            if (string.IsNullOrWhiteSpace(managedBy))
            {
                TempData["ErrorMessage"] =
                    "Please enter Managed By.";

                return RedirectToAction(nameof(Edit), new { id });
            }


            if (!populationServed.HasValue ||
                populationServed.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid Population Served.";

                return RedirectToAction(nameof(Edit), new { id });
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
            // Get existing record within current session
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Dental Field Practice Area record not found or does not belong to the current selection.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Update details
            // --------------------------------------------------------

            entity.Location =
                location.Trim();

            entity.Address =
                address.Trim();

            entity.ManagedBy =
                managedBy.Trim();

            entity.PopulationServed =
                populationServed.Value;


            // --------------------------------------------------------
            // Replace Staff List if new file uploaded
            // --------------------------------------------------------

            if (staffListFile != null && staffListFile.Length > 0)
            {
                const long maxFileSize = 2 * 1024 * 1024; // 2 MB

                var extension = Path.GetExtension(staffListFile.FileName)
                    .ToLowerInvariant();

                // PDF only
                if (extension != ".pdf")
                {
                    TempData["ErrorMessage"] =
                        "Only PDF files are allowed for Staff List.";

                    return RedirectToAction(nameof(Edit), new { id });
                }

                // Maximum 2 MB
                if (staffListFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] =
                        "Staff List PDF size must not exceed 2 MB.";

                    return RedirectToAction(nameof(Edit), new { id });
                }

                // ----------------------------------------------------
                // Base path - same paths used by BaseController
                // ----------------------------------------------------

                string basePath = facultyCode == "2"
                    ? (Directory.Exists(@"E:\")
                        ? @"E:\Affiliation_Dental"
                        : @"D:\Affiliation_Dental")
                    : (Directory.Exists(@"E:\")
                        ? @"E:\Affiliation_Medical"
                        : @"D:\Affiliation_Medical");

                string uploadFolder = Path.Combine(
                    basePath,
                    "DentalFieldPracticeArea"
                );

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // ----------------------------------------------------
                // Delete old physical file
                // ----------------------------------------------------

                if (!string.IsNullOrWhiteSpace(entity.StaffList))
                {
                    if (System.IO.File.Exists(entity.StaffList))
                    {
                        System.IO.File.Delete(entity.StaffList);
                    }
                }

                // ----------------------------------------------------
                // Save new file
                // ----------------------------------------------------

                var fileName = $"{Guid.NewGuid():N}.pdf";

                var filePath = Path.Combine(
                    uploadFolder,
                    fileName
                );

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await staffListFile.CopyToAsync(stream);
                }

                // Store ONLY physical file path
                entity.StaffList = filePath;
            }


            // --------------------------------------------------------
            // Audit
            // --------------------------------------------------------

            entity.ModifiedBy =
                HttpContext.Session.GetString("LoginID");

            entity.ModifiedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Dental Field Practice Area details updated successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // VIEW STAFF LIST PDF
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ViewStaffList(int id)
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString("FacultyCode");

            var typeId =
                HttpContext.Session.GetInt32("AffiliationType");

            var courseLevel =
                HttpContext.Session.GetString("CourseLevel");

            // --------------------------------------------------------
            // Validate session
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(collegeCode) ||
                string.IsNullOrWhiteSpace(facultyCode) ||
                !typeId.HasValue ||
                typeId <= 0 ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                return NotFound();
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
                return NotFound();
            }

            // --------------------------------------------------------
            // Get record
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);

            if (entity == null ||
                string.IsNullOrWhiteSpace(entity.StaffList))
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Physical file path stored in database
            // --------------------------------------------------------

            var filePath = entity.StaffList;

            // --------------------------------------------------------
            // Security check
            // Ensure file belongs to DentalFieldPracticeArea folder
            // --------------------------------------------------------

            var dentalBasePath = Directory.Exists(@"E:\")
                ? @"E:\Affiliation_Dental"
                : @"D:\Affiliation_Dental";

            var allowedFolder = Path.GetFullPath(
                Path.Combine(
                    dentalBasePath,
                    "DentalFieldPracticeArea"
                )
            );

            var fullFilePath = Path.GetFullPath(filePath);

            if (!fullFilePath.StartsWith(
                    allowedFolder,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // --------------------------------------------------------
            // Check file exists
            // --------------------------------------------------------

            if (!System.IO.File.Exists(fullFilePath))
            {
                return NotFound();
            }

            // --------------------------------------------------------
            // Return PDF inline
            // --------------------------------------------------------

            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(fullFilePath);

            return File(
                fileBytes,
                "application/pdf"
            );
        }


        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // --------------------------------------------------------
            // Get session values
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
            // Validate session
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
            // Find record
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == facultyId.Value &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Dental Field Practice Area record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Soft delete
            // --------------------------------------------------------

            entity.IsActive = false;

            entity.ModifiedBy =
                HttpContext.Session.GetString("LoginID");

            entity.ModifiedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Dental Field Practice Area record removed successfully.";


            return RedirectToAction(nameof(Index));
        }
    }
}