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
            // Session values
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
            // Validate college + faculty
            // --------------------------------------------------------

            var college =
                await _context.AffiliationCollegeMasters
                    .FirstOrDefaultAsync(c =>
                        c.CollegeCode == collegeCode);

            if (college == null)
            {
                TempData["ErrorMessage"] =
                    "College information not found.";

                return RedirectToAction("Index", "Home");
            }


            if (!string.Equals(
                    college.FacultyCode,
                    facultyCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "The selected faculty does not match the college.";

                return RedirectToAction("Index", "Home");
            }


            // --------------------------------------------------------
            // Get FacultyId
            // --------------------------------------------------------

            /*var facultyId =
                await _context.Faculties
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
            }*/


            // --------------------------------------------------------
            // Get Rural / Urban field types
            // --------------------------------------------------------

            var fieldTypes =
                await _context.MstFieldTypeChps
                    .Where(x =>
                        x.FacultyCode == int.Parse(facultyCode))
                    .OrderBy(x => x.Id)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.FieldType
                    })
                    .ToListAsync();


            // --------------------------------------------------------
            // Get existing records
            // --------------------------------------------------------

            var records =
                await _context.DentalFieldPracticeAreas
                    .Include(x => x.Faculty)
                    .Include(x => x.Type)
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive)
                    .OrderBy(x => x.FieldTypeId)
                    .ThenBy(x => x.DentalFieldPracticeAreaId)
                    .ToListAsync();


            // --------------------------------------------------------
            // Prepare model
            // --------------------------------------------------------

            var model = new DentalFieldPracticeAreaViewModel
            {
                FacultyId = int.Parse(facultyCode),

                CollegeCode = collegeCode,

                TypeId = typeId.Value,

                CourseLevel = courseLevel,

                FieldTypes = fieldTypes,

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
            int? fieldTypeId,
            string? location,
            string? address,
            string? managedBy,
            IFormFile? staffListFile,
            int? populationServed,
            string? activitiesAndServices,
            string? recordsMaintained,
            string? equipmentsAvailable,
            string? trainingActivities,
            string? supervisionMethod,
            string? traineeSupervisorAccommodation)
        {
            // --------------------------------------------------------
            // Session values
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
            // Validate Field Type
            // --------------------------------------------------------

            if (!fieldTypeId.HasValue || fieldTypeId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select Field Type.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Field Type belongs to faculty
            // --------------------------------------------------------

            var fieldTypeExists =
                await _context.MstFieldTypeChps
                    .AnyAsync(x =>
                        x.Id == fieldTypeId.Value &&
                        x.FacultyCode == int.Parse(facultyCode));


            if (!fieldTypeExists)
            {
                TempData["ErrorMessage"] =
                    "Invalid Field Type selected.";

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
            // Validate Population
            // --------------------------------------------------------

            if (!populationServed.HasValue ||
                populationServed.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid Population Served.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Activities
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(activitiesAndServices))
            {
                TempData["ErrorMessage"] =
                    "Please enter Activities and Services provided.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Records
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(recordsMaintained))
            {
                TempData["ErrorMessage"] =
                    "Please enter Records Maintained.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Equipment
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(equipmentsAvailable))
            {
                TempData["ErrorMessage"] =
                    "Please enter Equipments Available.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Training
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(trainingActivities))
            {
                TempData["ErrorMessage"] =
                    "Please enter Training Activities.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Supervision
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(supervisionMethod))
            {
                TempData["ErrorMessage"] =
                    "Please enter Supervision details.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Validate Accommodation
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(traineeSupervisorAccommodation))
            {
                TempData["ErrorMessage"] =
                    "Please enter Accommodation details.";

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
            // Validate Faculty
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

            //var facultyId =
            //    await _context.Faculties
            //        .Where(f =>
            //            f.Status == "Active" &&
            //            _context.AffiliationCollegeMasters.Any(c =>
            //                c.CollegeCode == collegeCode &&
            //                c.FacultyCode == facultyCode))
            //        .Select(f => (int?)f.FacultyId)
            //        .FirstOrDefaultAsync();


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
                    .AnyAsync(t =>
                        t.TypeId == typeId.Value);


            if (!affiliationTypeExists)
            {
                TempData["ErrorMessage"] =
                    "Invalid affiliation type.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Check duplicate
            //
            // One Rural + one Urban record is allowed.
            // --------------------------------------------------------

            var duplicateExists =
                await _context.DentalFieldPracticeAreas
                    .AnyAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.FieldTypeId == fieldTypeId.Value &&
                        x.IsActive);


            if (duplicateExists)
            {
                TempData["ErrorMessage"] =
                    "Details already exist for the selected Field Type.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Upload Staff List
            // --------------------------------------------------------

            string? staffListPath = null;

            if (staffListFile != null &&
                staffListFile.Length > 0)
            {
                const long maxFileSize =
                    2 * 1024 * 1024;


                var extension =
                    Path.GetExtension(
                        staffListFile.FileName)
                        .ToLowerInvariant();


                if (extension != ".pdf")
                {
                    TempData["ErrorMessage"] =
                        "Only PDF files are allowed for Staff List.";

                    return RedirectToAction(nameof(Index));
                }


                if (staffListFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] =
                        "Staff List PDF size must not exceed 2 MB.";

                    return RedirectToAction(nameof(Index));
                }


                // ----------------------------------------------------
                // Base path
                // ----------------------------------------------------

                string basePath =
                    facultyCode == "2"
                        ? (Directory.Exists(@"E:\")
                            ? @"E:\Affiliation_Dental"
                            : @"D:\Affiliation_Dental")
                        : (Directory.Exists(@"E:\")
                            ? @"E:\Affiliation_Medical"
                            : @"D:\Affiliation_Medical");


                string uploadFolder =
                    Path.Combine(
                        basePath,
                        "DentalFieldPracticeArea");


                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(
                        uploadFolder);
                }


                var fileName =
                    $"{Guid.NewGuid():N}.pdf";


                var filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);


                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
                {
                    await staffListFile.CopyToAsync(stream);
                }


                staffListPath =
                    filePath;
            }


            // --------------------------------------------------------
            // Create entity
            // --------------------------------------------------------

            var entity =
                new DentalFieldPracticeArea
                {
                    FacultyId =
                        int.Parse(facultyCode),

                    CollegeCode =
                        collegeCode,

                    TypeId =
                        typeId.Value,

                    CourseLevel =
                        courseLevel.ToUpperInvariant(),

                    FieldTypeId =
                        fieldTypeId.Value,

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

                    ActivitiesAndServices =
                        activitiesAndServices.Trim(),

                    RecordsMaintained =
                        recordsMaintained.Trim(),

                    EquipmentsAvailable =
                        equipmentsAvailable.Trim(),

                    TrainingActivities =
                        trainingActivities.Trim(),

                    SupervisionMethod =
                        supervisionMethod.Trim(),

                    TraineeSupervisorAccommodation =
                        traineeSupervisorAccommodation.Trim(),

                    IsActive =
                        true,

                    CreatedBy =
                        HttpContext.Session
                            .GetString("LoginID"),

                    CreatedDate =
                        DateTime.Now
                };


            _context.DentalFieldPracticeAreas
                .Add(entity);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Field Practice Area details saved successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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
                typeId <= 0 ||
                string.IsNullOrWhiteSpace(courseLevel))
            {
                TempData["ErrorMessage"] =
                    "Session information is missing.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Get FacultyId
            // --------------------------------------------------------

            //var facultyId =
            //    await _context.Faculties
            //        .Where(f =>
            //            f.Status == "Active" &&
            //            _context.AffiliationCollegeMasters.Any(c =>
            //                c.CollegeCode == collegeCode &&
            //                c.FacultyCode == facultyCode))
            //        .Select(f => (int?)f.FacultyId)
            //        .FirstOrDefaultAsync();


            //if (!facultyId.HasValue)
            //{
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Get record
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Field Practice Area record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Get all records
            // --------------------------------------------------------

            var records =
                await _context.DentalFieldPracticeAreas
                    .Include(x => x.Faculty)
                    .Include(x => x.Type)
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive)
                    .OrderBy(x => x.FieldTypeId)
                    .ThenBy(x => x.DentalFieldPracticeAreaId)
                    .ToListAsync();


            // --------------------------------------------------------
            // Field Types
            // --------------------------------------------------------

            var fieldTypes =
                await _context.MstFieldTypeChps
                    .Where(x =>
                        x.FacultyCode ==
                        int.Parse(facultyCode))
                    .OrderBy(x => x.Id)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.FieldType
                    })
                    .ToListAsync();


            // --------------------------------------------------------
            // Prepare model
            // --------------------------------------------------------

            var model =
                new DentalFieldPracticeAreaViewModel
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

                    FieldTypeId =
                        entity.FieldTypeId,

                    FieldTypes =
                        fieldTypes,

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

                    ActivitiesAndServices =
                        entity.ActivitiesAndServices,

                    RecordsMaintained =
                        entity.RecordsMaintained,

                    EquipmentsAvailable =
                        entity.EquipmentsAvailable,

                    TrainingActivities =
                        entity.TrainingActivities,

                    SupervisionMethod =
                        entity.SupervisionMethod,

                    TraineeSupervisorAccommodation =
                        entity.TraineeSupervisorAccommodation,

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
            int? fieldTypeId,
            string? location,
            string? address,
            string? managedBy,
            IFormFile? staffListFile,
            int? populationServed,
            string? activitiesAndServices,
            string? recordsMaintained,
            string? equipmentsAvailable,
            string? trainingActivities,
            string? supervisionMethod,
            string? traineeSupervisorAccommodation)
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
            // Session validation
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
            // Basic validation
            // --------------------------------------------------------

            if (!fieldTypeId.HasValue ||
                fieldTypeId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select Field Type.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(location))
            {
                TempData["ErrorMessage"] =
                    "Please enter Location.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["ErrorMessage"] =
                    "Please enter Address.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(managedBy))
            {
                TempData["ErrorMessage"] =
                    "Please enter Managed By.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (!populationServed.HasValue ||
                populationServed.Value <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid Population Served.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(activitiesAndServices))
            {
                TempData["ErrorMessage"] =
                    "Please enter Activities and Services provided.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(recordsMaintained))
            {
                TempData["ErrorMessage"] =
                    "Please enter Records Maintained.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(equipmentsAvailable))
            {
                TempData["ErrorMessage"] =
                    "Please enter Equipments Available.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(trainingActivities))
            {
                TempData["ErrorMessage"] =
                    "Please enter Training Activities.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(supervisionMethod))
            {
                TempData["ErrorMessage"] =
                    "Please enter Supervision details.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            if (string.IsNullOrWhiteSpace(
                    traineeSupervisorAccommodation))
            {
                TempData["ErrorMessage"] =
                    "Please enter Accommodation details.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            // --------------------------------------------------------
            // Validate field type
            // --------------------------------------------------------

            var fieldTypeExists =
                await _context.MstFieldTypeChps
                    .AnyAsync(x =>
                        x.Id == fieldTypeId.Value &&
                        x.FacultyCode ==
                            int.Parse(facultyCode));


            if (!fieldTypeExists)
            {
                TempData["ErrorMessage"] =
                    "Invalid Field Type selected.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            // --------------------------------------------------------
            // Get FacultyId
            // --------------------------------------------------------

            //var facultyId =
            //    await _context.Faculties
            //        .Where(f =>
            //            f.Status == "Active" &&
            //            _context.AffiliationCollegeMasters.Any(c =>
            //                c.CollegeCode == collegeCode &&
            //                c.FacultyCode == facultyCode))
            //        .Select(f => (int?)f.FacultyId)
            //        .FirstOrDefaultAsync();


            //if (!facultyId.HasValue)
            //{
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Get existing entity
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Field Practice Area record not found or does not belong to the current selection.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Check duplicate Rural / Urban
            //
            // Exclude current record.
            // --------------------------------------------------------

            var duplicateExists =
                await _context.DentalFieldPracticeAreas
                    .AnyAsync(x =>
                        x.DentalFieldPracticeAreaId != id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.FieldTypeId == fieldTypeId.Value &&
                        x.IsActive);


            if (duplicateExists)
            {
                TempData["ErrorMessage"] =
                    "Details already exist for the selected Field Type.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id });
            }


            // --------------------------------------------------------
            // Update fields
            // --------------------------------------------------------

            entity.FieldTypeId =
                fieldTypeId.Value;

            entity.Location =
                location.Trim();

            entity.Address =
                address.Trim();

            entity.ManagedBy =
                managedBy.Trim();

            entity.PopulationServed =
                populationServed.Value;

            entity.ActivitiesAndServices =
                activitiesAndServices.Trim();

            entity.RecordsMaintained =
                recordsMaintained.Trim();

            entity.EquipmentsAvailable =
                equipmentsAvailable.Trim();

            entity.TrainingActivities =
                trainingActivities.Trim();

            entity.SupervisionMethod =
                supervisionMethod.Trim();

            entity.TraineeSupervisorAccommodation =
                traineeSupervisorAccommodation.Trim();


            // --------------------------------------------------------
            // Replace Staff List
            // --------------------------------------------------------

            if (staffListFile != null &&
                staffListFile.Length > 0)
            {
                const long maxFileSize =
                    2 * 1024 * 1024;


                var extension =
                    Path.GetExtension(
                        staffListFile.FileName)
                        .ToLowerInvariant();


                if (extension != ".pdf")
                {
                    TempData["ErrorMessage"] =
                        "Only PDF files are allowed for Staff List.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id });
                }


                if (staffListFile.Length > maxFileSize)
                {
                    TempData["ErrorMessage"] =
                        "Staff List PDF size must not exceed 2 MB.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id });
                }


                string basePath =
                    facultyCode == "2"
                        ? (Directory.Exists(@"E:\")
                            ? @"E:\Affiliation_Dental"
                            : @"D:\Affiliation_Dental")
                        : (Directory.Exists(@"E:\")
                            ? @"E:\Affiliation_Medical"
                            : @"D:\Affiliation_Medical");


                string uploadFolder =
                    Path.Combine(
                        basePath,
                        "DentalFieldPracticeArea");


                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(
                        uploadFolder);
                }


                // ----------------------------------------------------
                // Delete old file
                // ----------------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                        entity.StaffList))
                {
                    if (System.IO.File.Exists(
                            entity.StaffList))
                    {
                        System.IO.File.Delete(
                            entity.StaffList);
                    }
                }


                // ----------------------------------------------------
                // Save new file
                // ----------------------------------------------------

                var fileName =
                    $"{Guid.NewGuid():N}.pdf";


                var filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);


                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
                {
                    await staffListFile.CopyToAsync(
                        stream);
                }


                entity.StaffList =
                    filePath;
            }


            // --------------------------------------------------------
            // Audit
            // --------------------------------------------------------

            entity.ModifiedBy =
                HttpContext.Session
                    .GetString("LoginID");

            entity.ModifiedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Field Practice Area details updated successfully.";


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
            // FacultyId
            // --------------------------------------------------------

            //var facultyId =
            //    await _context.Faculties
            //        .Where(f =>
            //            f.Status == "Active" &&
            //            _context.AffiliationCollegeMasters.Any(c =>
            //                c.CollegeCode == collegeCode &&
            //                c.FacultyCode == facultyCode))
            //        .Select(f => (int?)f.FacultyId)
            //        .FirstOrDefaultAsync();


            //if (!facultyId.HasValue)
            //{
            //    return NotFound();
            //}


            // --------------------------------------------------------
            // Get entity
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null ||
                string.IsNullOrWhiteSpace(
                    entity.StaffList))
            {
                return NotFound();
            }


            var filePath =
                entity.StaffList;


            // --------------------------------------------------------
            // Allowed folder
            // --------------------------------------------------------

            var dentalBasePath =
                Directory.Exists(@"E:\")
                    ? @"E:\Affiliation_Dental"
                    : @"D:\Affiliation_Dental";


            var allowedFolder =
                Path.GetFullPath(
                    Path.Combine(
                        dentalBasePath,
                        "DentalFieldPracticeArea"));


            var fullFilePath =
                Path.GetFullPath(filePath);


            // --------------------------------------------------------
            // Security check
            // --------------------------------------------------------

            if (!fullFilePath.StartsWith(
                    allowedFolder,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }


            // --------------------------------------------------------
            // File exists
            // --------------------------------------------------------

            if (!System.IO.File.Exists(
                    fullFilePath))
            {
                return NotFound();
            }


            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(
                    fullFilePath);


            return File(
                fileBytes,
                "application/pdf");
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
            // FacultyId
            // --------------------------------------------------------

            //var facultyId =
            //    await _context.Faculties
            //        .Where(f =>
            //            f.Status == "Active" &&
            //            _context.AffiliationCollegeMasters.Any(c =>
            //                c.CollegeCode == collegeCode &&
            //                c.FacultyCode == facultyCode))
            //        .Select(f => (int?)f.FacultyId)
            //        .FirstOrDefaultAsync();


            //if (!facultyId.HasValue)
            //{
            //    TempData["ErrorMessage"] =
            //        "Faculty information not found.";

            //    return RedirectToAction(nameof(Index));
            //}


            // --------------------------------------------------------
            // Get entity
            // --------------------------------------------------------

            var entity =
                await _context.DentalFieldPracticeAreas
                    .FirstOrDefaultAsync(x =>
                        x.DentalFieldPracticeAreaId == id &&
                        x.CollegeCode == collegeCode &&
                        x.FacultyId == int.Parse(facultyCode) &&
                        x.TypeId == typeId.Value &&
                        x.CourseLevel == courseLevel &&
                        x.IsActive);


            if (entity == null)
            {
                TempData["ErrorMessage"] =
                    "Field Practice Area record not found.";

                return RedirectToAction(nameof(Index));
            }


            // --------------------------------------------------------
            // Soft delete
            // --------------------------------------------------------

            entity.IsActive =
                false;

            entity.ModifiedBy =
                HttpContext.Session
                    .GetString("LoginID");

            entity.ModifiedDate =
                DateTime.Now;


            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Field Practice Area record removed successfully.";


            return RedirectToAction(nameof(Index));
        }
    }
}