using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace Medical_Affiliation.Controllers
{
    public class DentalController : BaseController
    {

        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public DentalController(ApplicationDbContext context, IUserContext userContext) : base(context)
        {
            this._context = context;
            this._userContext = userContext;
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Medical_EquimentDetails(string departmentCode)
        {
            // var facultyCodeStr = HttpContext.Session.GetString("FacultyCode") ?? "1";
            var collegeCode = CollegeCode;
            var facultyCodeStr = FacultyCode;
            int facultyCode = Convert.ToInt32(facultyCodeStr);

            var model = new EquipmentAvailabilityViewModel();

            // 1️⃣ Load Department dropdown (Faculty-wise)
            model.Courses = await _context.DepartmentMasters
                .Where(d => d.FacultyCode == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentCode,    // MD001
                    Text = d.DepartmentName      // Anatomy
                })
                .OrderBy(x => x.Text)
                .ToListAsync();


            // 2️⃣ Load equipment if department selected
            if (!string.IsNullOrEmpty(departmentCode))
            {

                model.SelectedDepartmentCode = departmentCode;

                // ✅ Load equipment list
                var equipments = await _context.MstLaboratoryEquipmentDetails
                    .Where(e =>
                        e.CourseCode == departmentCode &&
                        e.FacultyId == facultyCode)
                    .OrderBy(e => e.EquipmentId)
                    .ToListAsync();

                // ✅ Load availability ONCE (Fix N+1)
                var availabilityList = await _context.TblMedicalEquipmentAvailabilities
                    .Where(a =>
                        a.FacultyId == facultyCode &&
                        a.CourseCode == departmentCode &&
                        a.CollegeCode == collegeCode)
                    .ToListAsync();

                model.Equipments = equipments.Select(e =>
                {
                    var existing = availabilityList
                        .FirstOrDefault(a => a.EquipmentId == e.EquipmentId);

                    return new EquipmentItemViewModel
                    {
                        EquipmentID = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        IsAvailable = existing != null,
                        AvailableQuantity = existing?.AvailableQuantity
                    };
                }).ToList();
            }
            else
            {
                model.Equipments = new List<EquipmentItemViewModel>();
                TempData["Info"] = "Please select a department.";
            }

            return View(model);
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Medical_EquimentDetails(EquipmentAvailabilityViewModel model)
        {
            //var facultyCode = HttpContext.Session.GetString("FacultyCode") ?? "1";
            //int facultyId = Convert.ToInt32(facultyCode);



            if (string.IsNullOrWhiteSpace(FacultyCode) || string.IsNullOrWhiteSpace(CollegeCode))
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(FacultyCode, out int facultyId))
            {
                TempData["Error"] = "Invalid faculty code.";
                return RedirectToAction("Login", "Account");
            }

            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(model.SelectedDepartmentCode) || model.Equipments == null)
            {
                TempData["Error"] = "Invalid data submitted.";
                return RedirectToAction(nameof(Medical_EquimentDetails));
            }

            string departmentCode = model.SelectedDepartmentCode; // MD001

            var existingList = await _context.TblMedicalEquipmentAvailabilities
                .Where(x =>
                    x.FacultyId == facultyId &&
                    x.CourseCode == departmentCode &&
                    x.CollegeCode == collegeCode)
                .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                foreach (var item in model.Equipments)
                {
                    int quantity = item.AvailableQuantity ?? 0;

                    var existing = existingList.FirstOrDefault(x => x.EquipmentId == item.EquipmentID);

                    if (quantity > 0)
                    {
                        // AVAILABLE
                        if (existing == null)
                        {
                            _context.TblMedicalEquipmentAvailabilities.Add(
                                new TblMedicalEquipmentAvailability
                                {
                                    FacultyId = facultyId,
                                    CourseCode = departmentCode,
                                    EquipmentId = item.EquipmentID,
                                    IsAvailable = true,
                                    AvailableQuantity = quantity,
                                    CollegeCode = collegeCode,
                                });
                        }
                        else
                        {
                            existing.IsAvailable = true;
                            existing.AvailableQuantity = quantity;
                        }
                    }
                    else
                    {
                        // NOT AVAILABLE → Remove record
                        if (existing != null)
                        {
                            _context.TblMedicalEquipmentAvailabilities.Remove(existing);
                        }
                    }

                }
                // 🔥 ADD THIS
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Equipment availability saved successfully.";

                return RedirectToAction(nameof(Medical_EquimentDetails),
                    new { departmentCode = departmentCode });

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                TempData["Error"] = "Error while saving data.";
                return View(model);
            }

        }


        public async Task<IActionResult> EquipmentList()
        {
            string collegeCode = CollegeCode;
            int facultyCode = Convert.ToInt32(FacultyCode);

            var vm = new EquipmentPageVM
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                SelectedDepartmentCode = TempData["SelectedDept"]?.ToString()
            };

            vm.Departments = await _context.MstEquipmentDepartments
                .Where(d =>
                    d.FacultyCode == facultyCode &&
                    d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new DepartmentVM
                {
                    Code = d.DepartmentCode,
                    Name = d.DepartmentName
                })
                .ToListAsync();

            return View(vm);
        }

        // This is the method you requested: Fetches equipment list for the selected department
        [HttpGet]
        public async Task<IActionResult> GetDentalDepartmentEquipment(string deptCode)
        {
            string collegeCode = CollegeCode;
            int facultyCode = Convert.ToInt32(FacultyCode);
            // 1. Get intake to determine slab (One Unit vs Two Unit)
            //var intake = await _context.AcademicIntakes
            //    .FirstOrDefaultAsync(a => a.CollegeCode == collegeCode);

            //int totalIntake = intake?.Ay2025TotalIntake ?? 0;

            //// Logic: Dental (Faculty 2) has different slab limits than Medical (Faculty 1)
            //bool isTwoUnitSlab = (facultyCode == 2)
            //    ? totalIntake > 60  // Dental slab threshold
            //    : totalIntake > 150; // Medical slab threshold

            // 2. Fetch Master Equipment for this department and faculty
            var masterEquipment = await _context.MstEquipmentDeptWises
                .Where(e => e.DepartmentCode == deptCode && e.FacultyCode == facultyCode && e.IsActive)
                .ToListAsync();

            var result = new List<EquipmentRowVM>();
            var savedDataList = await _context.DentalCollegeEquipmentDetails
                                .Where(x =>
                                    x.CollegeCode == collegeCode &&
                                    x.DepartmentCode == deptCode)
                                .ToListAsync();

            foreach (var item in masterEquipment)
            {
                // 3. Fetch existing saved values from the DB-First save table
                var savedData = savedDataList.FirstOrDefault(x => x.EquipmentId == item.Id);

                result.Add(new EquipmentRowVM
                {
                    EquipmentId = item.Id,
                    EquipmentName = item.EquipmentName,
                    Specification = item.Specification ?? "N/A",
                    OneUnitReq = item.OneUnitRequirement,
                    TwoUnitReq = item.TwoUnitRequirement,
                    //IsTwoUnitSlab = isTwoUnitSlab,
                    OneUnitExisting = savedData?.OneUnitExisting,
                    TwoUnitExisting = savedData?.TwoUnitExisting
                });
            }

            return Json(result);
        }

        // Saves the input data to the DB-First table
        [HttpPost]
        public async Task<IActionResult> SaveEquipment(string deptCode, List<EquipmentRowVM> data)
        {
            try
            {
                string collegeCode = CollegeCode;
                int facultyCode = Convert.ToInt32(FacultyCode);

                if (string.IsNullOrWhiteSpace(deptCode))
                {
                    TempData["Error"] = "Department code is missing.";
                    return RedirectToAction("EquipmentList");
                }

                if (data == null || !data.Any())
                {
                    TempData["Error"] = "No equipment data received.";
                    return RedirectToAction("EquipmentList");
                }

                var existingRecords = await _context.DentalCollegeEquipmentDetails
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.DepartmentCode == deptCode)
                    .ToListAsync();

                var equipmentIds = data.Select(x => x.EquipmentId).ToList();

                var masterEquipment = await _context.MstEquipmentDeptWises
                    .Where(x => equipmentIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

                foreach (var item in data)
                {
                    var existingRecord = existingRecords
                        .FirstOrDefault(x => x.EquipmentId == item.EquipmentId);

                    if (existingRecord != null)
                    {
                        existingRecord.OneUnitExisting = item.OneUnitExisting;
                        existingRecord.TwoUnitExisting = item.TwoUnitExisting;
                        existingRecord.UpdatedDate = DateTime.Now;
                    }
                    else
                    {
                        masterEquipment.TryGetValue(item.EquipmentId, out var master);

                        var newEntry = new DentalCollegeEquipmentDetail
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            DepartmentCode = deptCode,

                            EquipmentId = item.EquipmentId,
                            EquipmentName = item.EquipmentName,

                            OneUnitRequirement = master?.OneUnitRequirement,
                            TwoUnitRequirement = master?.TwoUnitRequirement,

                            OneUnitExisting = item.OneUnitExisting,
                            TwoUnitExisting = item.TwoUnitExisting,

                            CreatedDate = DateTime.Now,
                            IsActive = true
                        };

                        _context.DentalCollegeEquipmentDetails.Add(newEntry);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SelectedDept"] = deptCode;
                TempData["Success"] = "Equipment data saved successfully.";

                return RedirectToAction("EquipmentList");
            }
            catch (DbUpdateException ex)
            {
                // Log ex
                TempData["Error"] = "Database error occurred while saving equipment data.";
                return RedirectToAction("EquipmentList");
            }
            catch (Exception ex)
            {
                // Log ex
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return RedirectToAction("EquipmentList");
            }
        }


        [HttpGet]
        public async Task<IActionResult> TeachingStaffDepartmentWise()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            int.TryParse(facultyCode, out int facultyCodeInt);

            // ── Designation headers ──────────────────────────────────────────────
            var designationMasters = await _context.DesignationMasters
                .Where(x =>
                    x.FacultyCode == facultyCodeInt &&
                    x.DesignationOrder != 0)
                .OrderBy(x => x.DesignationOrder)
                .ToListAsync();

            // ── Build base ViewModel ─────────────────────────────────────────────
            var vm = new DentalTeachingStaffVm
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,

                Colleges = await _context.AffiliationCollegeMasters
                    .Where(c => c.FacultyCode.ToString() == facultyCode)
                    .OrderBy(c => c.CollegeName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = c.CollegeName
                    })
                    .Distinct()
                    .ToListAsync(),

                Departments = await _context.MstCourses
                    .Where(x => x.FacultyCode == facultyCodeInt)
                    .OrderBy(x => x.CourseName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.CourseCode.ToString(),
                        Text = x.CourseName,
                        Group = new SelectListGroup { Name = x.CourseLevel }
                    })
                    .ToListAsync(),

                DesignationHeaders = designationMasters
                    .Select(x => new DentalDesignationTeachingVm
                    {
                        DesignationCode = x.DesignationCode,
                        DesignationName = x.DesignationName
                    })
                    .ToList()
            };

            // ── Faculty raw list (LEFT JOIN in memory) ───────────────────────────
            var facultyRaw = await _context.FacultyDetails
                .Where(f =>
                    f.CollegeCode == collegeCode &&
                    f.FacultyCode == facultyCode &&
                    f.IsRemoved != true)
                .ToListAsync();

            var mstCourses = await _context.MstCourses
                .Where(x => x.FacultyCode == facultyCodeInt)
                .ToListAsync();

            var designationLookup = await _context.DesignationMasters
                .Where(x =>
                    x.FacultyCode == facultyCodeInt &&
                    x.DesignationOrder != 0)
                .ToListAsync();

            var facultyList = facultyRaw
                .Where(f => !string.IsNullOrWhiteSpace(f.NameOfFaculty))
                .Select(f =>
                {
                    var course = mstCourses
                        .FirstOrDefault(c => c.CourseCode.ToString() == f.DepartmentDetails);

                    var designation = designationLookup
                        .FirstOrDefault(d => d.DesignationCode == f.Designation);

                    return new
                    {
                        f.NameOfFaculty,
                        f.DepartmentDetails,
                        DepartmentName = course?.CourseName ?? f.DepartmentDetails,
                        CourseLevel = course?.CourseLevel ?? string.Empty,
                        f.Designation,
                        DesignationName = designation?.DesignationName ?? f.Designation,
                        f.From,
                        f.To
                    };
                })
                .ToList();

            // ── FacultyList dropdown ─────────────────────────────────────────────
            vm.FacultyList = facultyList
                .Select(x => x.NameOfFaculty.Trim())
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem { Value = x, Text = x })
                .ToList();

            // ── Saved rows from DB ───────────────────────────────────────────────
            var saved = await _context.TeachingStaffDepartmentWiseDetails
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .ToListAsync();

            // ── Build FacultyRows ────────────────────────────────────────────────

            // Names already covered by saved rows
            var savedNames = saved
                .Select(x => x.NameOfFaculty?.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // ① Rows from saved data
            var groupedFaculty = saved
                .GroupBy(x => x.NameOfFaculty)
                .ToList();

            foreach (var facultyGroup in groupedFaculty)
            {
                var firstFaculty = facultyGroup.First();

                var facultyVm = new DentalFacultyTeachingVm
                {
                    NameOfFaculty = firstFaculty.NameOfFaculty,
                    Designations = new List<DentalDesignationTeachingVm>()
                };

                foreach (var header in designationMasters)
                {
                    var existing = facultyGroup
                        .FirstOrDefault(x => x.DesignationCode == header.DesignationCode);

                    if (existing == null)
                    {
                        facultyVm.Designations.Add(new DentalDesignationTeachingVm
                        {
                            DesignationCode = header.DesignationCode,
                            DesignationName = header.DesignationName
                        });
                        continue;
                    }

                    var vmDesignation = new DentalDesignationTeachingVm
                    {
                        Id = existing.Id,
                        DesignationCode = existing.DesignationCode,
                        DesignationName = existing.DesignationName,
                        DepartmentCode = existing.DepartmentCode,
                        CourseLevel = existing.CourseLevel,
                        TotalExperience = existing.TotalExperience
                    };

                    if (existing.CourseLevel == "UG")
                    {
                        vmDesignation.CollegeCode = existing.UgcollegeCode?.Trim();
                        vmDesignation.FromDate = existing.Ugfrom?.ToDateTime(TimeOnly.MinValue);
                        vmDesignation.ToDate = existing.Ugto?.ToDateTime(TimeOnly.MinValue);
                    }
                    else if (existing.CourseLevel == "PG")
                    {
                        vmDesignation.CollegeCode = existing.PgcollegeCode?.Trim();
                        vmDesignation.FromDate = existing.Pgfrom?.ToDateTime(TimeOnly.MinValue);
                        vmDesignation.ToDate = existing.Pgto?.ToDateTime(TimeOnly.MinValue);
                    }

                    facultyVm.Designations.Add(vmDesignation);
                }

                vm.FacultyRows.Add(facultyVm);
            }

            // ② Seed rows for faculty who have NO saved data yet
            var unseededNames = facultyList
                .Select(f => f.NameOfFaculty.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(name => !savedNames.Contains(name))
                .OrderBy(name => name);

            foreach (var name in unseededNames)
            {
                var facultyVm = new DentalFacultyTeachingVm
                {
                    NameOfFaculty = name,
                    Designations = designationMasters
                        .Select(d => new DentalDesignationTeachingVm
                        {
                            DesignationCode = d.DesignationCode,
                            DesignationName = d.DesignationName
                        })
                        .ToList()
                };

                vm.FacultyRows.Add(facultyVm);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 500000)]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> TeachingStaffDepartmentWise(DentalTeachingStaffVm vm)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            if (vm?.FacultyRows == null || !vm.FacultyRows.Any())
                return RedirectToAction("TeachingStaffDepartmentWise");

            // ── Collect all Ids that were posted (existing records) ─────────────
            var postedIds = vm.FacultyRows
                .SelectMany(x => x.Designations ?? new List<DentalDesignationTeachingVm>())
                .Where(x => x.Id > 0)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            // ── Delete rows that are no longer in the posted data ────────────────
            var recordsToDelete = await _context.TeachingStaffDepartmentWiseDetails
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    !postedIds.Contains(x.Id))
                .ToListAsync();

            if (recordsToDelete.Any())
                _context.TeachingStaffDepartmentWiseDetails.RemoveRange(recordsToDelete);

            // ── Upsert each designation slot ─────────────────────────────────────
            foreach (var faculty in vm.FacultyRows)
            {
                if (faculty.Designations == null) continue;

                foreach (var des in faculty.Designations)
                {
                    // Skip entirely empty slots
                    if (string.IsNullOrWhiteSpace(des.DesignationCode))
                        continue;

                    if (string.IsNullOrWhiteSpace(des.CourseLevel) &&
                        string.IsNullOrWhiteSpace(des.CollegeCode) &&
                        !des.FromDate.HasValue &&
                        !des.ToDate.HasValue)
                    {
                        continue;
                    }

                    // Try to find existing DB record
                    TeachingStaffDepartmentWiseDetail? existing = null;

                    if (des.Id > 0)
                    {
                        existing = await _context.TeachingStaffDepartmentWiseDetails
                            .FirstOrDefaultAsync(x => x.Id == des.Id);
                    }

                    // Insert if not found
                    if (existing == null)
                    {
                        existing = new TeachingStaffDepartmentWiseDetail
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            NameOfFaculty = faculty.NameOfFaculty,
                            DesignationCode = des.DesignationCode,
                            DesignationName = des.DesignationName
                        };
                        _context.TeachingStaffDepartmentWiseDetails.Add(existing);
                    }

                    // Common fields
                    existing.DepartmentCode = des.DepartmentCode;
                    existing.NameOfFaculty = faculty.NameOfFaculty;
                    existing.CourseLevel = des.CourseLevel;
                    existing.TotalExperience = des.TotalExperience;

                    // UG-specific fields
                    if (des.CourseLevel == "UG")
                    {
                        existing.UgcollegeCode = des.CollegeCode;
                        existing.Ugfrom = des.FromDate.HasValue
                            ? DateOnly.FromDateTime(des.FromDate.Value) : null;
                        existing.Ugto = des.ToDate.HasValue
                            ? DateOnly.FromDateTime(des.ToDate.Value) : null;
                    }

                    // PG-specific fields
                    else if (des.CourseLevel == "PG")
                    {
                        existing.PgcollegeCode = des.CollegeCode;
                        existing.Pgfrom = des.FromDate.HasValue
                            ? DateOnly.FromDateTime(des.FromDate.Value) : null;
                        existing.Pgto = des.ToDate.HasValue
                            ? DateOnly.FromDateTime(des.ToDate.Value) : null;
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Teaching staff details saved successfully.";
            return RedirectToAction("TeachingStaffDepartmentWise");
        }


    }
}
