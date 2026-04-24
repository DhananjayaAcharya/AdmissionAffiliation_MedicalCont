using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    public class Aff_CA_MedicalLibraryController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public Aff_CA_MedicalLibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Aff_CA_Medical_LibraryDetails()
        {
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Login");

            var vm = new CA_Med_LibraryMainVM();

            // Changes by Ram on 23/04/2026
            var raw =
            HttpContext.Session.GetString(
            "ExistingCourseLevels");

            List<string> levels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                levels = new List<string> { "UG" };
            }
            else
            {
                try
                {
                    levels =
                    System.Text.Json.JsonSerializer
                    .Deserialize<List<string>>(raw)
                    ?? new List<string> { "UG" };
                }
                catch
                {
                    // Handles old session format:
                    // UG,PG,SS

                    levels =
                        raw.Split(',')
                        .Select(x => x.Trim().ToUpper())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                }
            }

            vm.ExistingCourseLevels = levels;

            // General (a-d)
            var general = await _context.CaMedLibraryGenerals
             .Where(x =>
                 x.CollegeCode == collegeCode &&
                 x.FacultyCode == facultyCode)
             .OrderBy(x => x.CourseLevel)
             .FirstOrDefaultAsync();

            vm.General = general == null
                ? new CA_Medi_LibraryGeneralVM()
                : new CA_Medi_LibraryGeneralVM
                {
                    LibraryEmailID = general.LibraryEmailId ?? string.Empty,
                    DigitalLibrary = general.DigitalLibrary ?? string.Empty,
                    HelinetServices = general.HelinetServices ?? string.Empty,
                    DepartmentWiseLibrary = general.DepartmentWiseLibrary ?? string.Empty
                };

            // Items Table
            var itemsMaster = await _context.CaMstMedLibraryItems
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedItems = await _context.CaMedLibraryItems
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .ToListAsync();

            savedItems = savedItems
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToList();

            vm.Items = itemsMaster.Select(m => new CA_Medi_LibraryItemsVM
            {
                SlNo = m.SlNo,
                ItemName = m.ItemName,
                CurrentForeign = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.CurrentForeign ?? 0,
                CurrentIndian = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.CurrentIndian ?? 0,
                PreviousForeign = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.PreviousForeign ?? 0,
                PreviousIndian = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.PreviousIndian ?? 0
            }).ToList();

            // Building (e)
            var building = await _context.CaMedLibraryBuildings
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            vm.Building = building == null
                ? new CA_Medi_LibraryBuildingVM()
                : new CA_Medi_LibraryBuildingVM
                {
                    IsIndependent = building.IsIndependent ?? string.Empty,
                    AreaSqMtrs = building.AreaSqMtrs
                };

            // Technical Process (f)
            var techMaster = await _context.CaMstMedLibTechnicalProcesses
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            // Changes by Ram on 23/04/2026
            // Common section now saved for UG/PG/SS.
            // Load from any one saved row (same values in all three).

            var savedTech = await _context.CaMedLibTechnicalProcesses
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            vm.TechnicalProcess = techMaster.Select(m => new CA_Medi_TechnicalProcessVM
            {
                SlNo = m.SlNo,
                ProcessName = m.ProcessName,
                Value = savedTech.FirstOrDefault(s => s.SlNo == m.SlNo)?.Value ?? string.Empty
            }).ToList();

            // Equipments (g)
            var equipMaster = await _context.CaMstMedLibraryEquipments
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            // Changes by Ram on 23/04/2026
            // Load equipments independent of course level

            var savedEquip = await _context.CaMedLibraryEquipments
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            var bindery =
            savedEquip.FirstOrDefault(
            x => x.EquipmentName == "Bindery");

            vm.BinderyValue =
            bindery?.HasEquipment;

            vm.Equipments = equipMaster.Select(m =>
            new CA_Medi_LibraryEquipmentsVM
            {
                SlNo = m.SlNo,
                EquipmentName = m.EquipmentName,
                HasEquipment =
                  savedEquip
                  .FirstOrDefault(
                     s => s.SlNo == m.SlNo
                  )?.HasEquipment
                  ?? ""
            }).ToList();

            // Changes by Ram on 23/04/2026
            // Finance saved for UG/PG/SS as common.
            // Load any one common row.

            var finance = await _context.CaMedLibraryFinances
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .OrderBy(x => x.CourseLevel)   // deterministic
                .FirstOrDefaultAsync();

            vm.Finance = finance == null
                ? new CA_Medi_LibraryFinanceVM()
                : new CA_Medi_LibraryFinanceVM
                {
                    TotalBudgetLakhs =
                        finance.TotalBudgetLakhs,

                    ExpenditureBooksLakhs =
                        finance.ExpenditureBooksLakhs
                };

            return View(vm);
        }

        // POST 1: General + Items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGeneralAndItems(CA_Med_Lib_DetailsAndItemsVM model)
        {
            // -------------------------------
            // Validation return
            // -------------------------------
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();

                fullVm.General = model.General;

                // Preserve master item rows
                if (model.Items != null && model.Items.Any())
                {
                    foreach (var row in fullVm.Items)
                    {
                        var posted =
                            model.Items.FirstOrDefault(
                                x => x.SlNo == row.SlNo);

                        if (posted != null)
                        {
                            row.CurrentForeign = posted.CurrentForeign;
                            row.CurrentIndian = posted.CurrentIndian;
                            row.PreviousForeign = posted.PreviousForeign;
                            row.PreviousIndian = posted.PreviousIndian;
                        }
                    }
                }

                return View(
                    "Aff_CA_Medical_LibraryDetails",
                    fullVm);
            }


            var collegeCode =
                HttpContext.Session.GetString(
                    "CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString(
                    "FacultyCode");


            // ==========================================
            // Changes by Ram on 23/04/2026
            // Safe ExistingCourseLevels handling
            // ==========================================

            // FIXED course levels parsing
            var raw = HttpContext.Session.GetString("ExistingCourseLevels");

            string[] levels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                levels = new[] { "UG" };
            }
            else
            {
                try
                {
                    var parsed =
                        JsonSerializer.Deserialize<List<string>>(raw);

                    levels = parsed?
                        .Select(x => x?.Trim())
                        .Where(x =>
                            x == "UG" ||
                            x == "PG" ||
                            x == "SS")
                        .Distinct()
                        .ToArray()

                        ?? new[] { "UG" };
                }
                catch
                {
                    levels =
                        raw.Split(',')
                           .Select(x => x.Trim())
                           .Select(x =>
                                x.Replace("[", "")
                                 .Replace("]", "")
                                 .Replace("\"", ""))
                           .Where(x =>
                                x == "UG" ||
                                x == "PG" ||
                                x == "SS")
                           .Distinct()
                           .ToArray();
                }
            }



            // ==========================================
            // Save General section
            // ==========================================

            foreach (var level in levels)
            {
                var general =
                await _context.CaMedLibraryGenerals
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.CourseLevel == level);

                if (general == null)
                {
                    general =
                    new CaMedLibraryGeneral
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = level
                    };

                    _context.CaMedLibraryGenerals
                    .Add(general);
                }

                general.LibraryEmailId =
                    model.General.LibraryEmailID;

                general.DigitalLibrary =
                    model.General.DigitalLibrary;

                general.HelinetServices =
                    model.General.HelinetServices;

                general.DepartmentWiseLibrary =
                    model.General.DepartmentWiseLibrary;
            }



            // ==========================================
            // Save Items section
            // ==========================================

            foreach (var itemVm in model.Items)
            {
                var masterItem =
                    await _context.CaMstMedLibraryItems
                    .FirstOrDefaultAsync(m =>
                        m.SlNo == itemVm.SlNo &&
                        m.FacultyCode == facultyCode);

                foreach (var level in levels)
                {
                    var entity =
                    await _context.CaMedLibraryItems
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.SlNo == itemVm.SlNo &&
                        x.CourseLevel == level);

                    if (entity == null)
                    {
                        entity =
                        new CaMedLibraryItem
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = level,
                            SlNo = itemVm.SlNo,
                            ItemName =
                              masterItem?.ItemName
                              ?? "Unknown Item"
                        };

                        _context.CaMedLibraryItems
                        .Add(entity);
                    }

                    entity.CurrentForeign =
                        itemVm.CurrentForeign;

                    entity.CurrentIndian =
                        itemVm.CurrentIndian;

                    entity.PreviousForeign =
                        itemVm.PreviousForeign;

                    entity.PreviousIndian =
                        itemVm.PreviousIndian;
                }
            }


            await _context.SaveChangesAsync();

            TempData["Success"] =
              "Library details and items saved successfully.";

            return RedirectToAction(
              nameof(
               Aff_CA_Medical_LibraryDetails));
        }

        // POST 2: Building
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBuilding(CA_Med_Lib_BuildingDetailsVM model)
        {
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();
                fullVm.Building = model.Building;
                return View("Aff_CA_Medical_LibraryDetails", fullVm);
            }

            if (model.Building.IsIndependent == "Y")
            {
                if (!model.Building.AreaSqMtrs.HasValue || model.Building.AreaSqMtrs <= 0)
                {
                    ModelState.AddModelError(
                        "Building.AreaSqMtrs",
                        "Area required when independent."
                    );

                    var fullVm = await LoadLibraryViewModel();
                    fullVm.Building = model.Building;
                    return View("Aff_CA_Medical_LibraryDetails", fullVm);
                }
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Changes by Ram on 23/04/2026
            var raw =
            HttpContext.Session.GetString(
            "ExistingCourseLevels");

            string[] levels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                levels = new[] { "UG" };
            }
            else
            {
                try
                {
                    // Handles JSON session:
                    // ["UG","PG","SS"]

                    levels =
                     System.Text.Json.JsonSerializer
                     .Deserialize<List<string>>(raw)
                     ?.Select(x => x.Trim().ToUpper())
                     .Distinct()
                     .ToArray()

                     ?? new[] { "UG" };
                }
                catch
                {
                    // Handles old comma session:
                    // UG,PG,SS

                    levels =
                     raw.Split(',')
                     .Select(x => x.Trim()
                                 .Replace("[", "")
                                 .Replace("]", "")
                                 .Replace("\"", "")
                                 .ToUpper())
                     .Where(x => !string.IsNullOrEmpty(x))
                     .Distinct()
                     .ToArray();
                }
            }

            foreach (var level in levels)
            {
                var building = await _context.CaMedLibraryBuildings
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == level);

                if (building == null)
                {
                    building = new CaMedLibraryBuilding
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = level
                    };

                    _context.Add(building);
                }

                building.IsIndependent = model.Building.IsIndependent;
                building.AreaSqMtrs =
                    model.Building.IsIndependent == "Y"
                    ? model.Building.AreaSqMtrs
                    : null;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Building saved.";
            return RedirectToAction(nameof(Aff_CA_Medical_LibraryDetails));
        }
        // POST: Technical Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTechnicalProcess(CA_Med_Lib_TechnicalProcessVM model)
        {
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();
                fullVm.TechnicalProcess = model.TechnicalProcess;
                return View("Aff_CA_Medical_LibraryDetails", fullVm);
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Changes by Ram on 23/04/2026
            var raw =
            HttpContext.Session.GetString(
            "ExistingCourseLevels");

            string[] levels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                levels = new[] { "UG" };
            }
            else
            {
                try
                {
                    // Handles JSON session:
                    // ["UG","PG","SS"]

                    levels =
                     System.Text.Json.JsonSerializer
                     .Deserialize<List<string>>(raw)
                     ?.Select(x => x.Trim().ToUpper())
                     .Distinct()
                     .ToArray()

                     ?? new[] { "UG" };
                }
                catch
                {
                    // Handles old comma session:
                    // UG,PG,SS

                    levels =
                     raw.Split(',')
                     .Select(x => x.Trim()
                                 .Replace("[", "")
                                 .Replace("]", "")
                                 .Replace("\"", "")
                                 .ToUpper())
                     .Where(x => !string.IsNullOrEmpty(x))
                     .Distinct()
                     .ToArray();
                }
            }

            foreach (var item in model.TechnicalProcess)
            {
                var masterProcess =
                    await _context.CaMstMedLibTechnicalProcesses
                    .FirstOrDefaultAsync(m =>
                        m.SlNo == item.SlNo &&
                        m.FacultyCode == facultyCode);

                foreach (var level in levels)
                {
                    var entity =
                        await _context.CaMedLibTechnicalProcesses
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.SlNo == item.SlNo &&
                            x.CourseLevel == level);

                    if (entity == null)
                    {
                        entity =
                            new CaMedLibTechnicalProcess
                            {
                                CollegeCode = collegeCode,
                                FacultyCode = facultyCode,
                                CourseLevel = level,
                                SlNo = item.SlNo,
                                ProcessName =
                                    masterProcess?.ProcessName
                                    ?? "Unknown Process"
                            };

                        _context.Add(entity);
                    }

                    entity.Value = item.Value;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Technical Process saved.";
            return RedirectToAction(nameof(Aff_CA_Medical_LibraryDetails));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEquipments(CA_Med_Lib_EquipmentsVM model)
        {
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();
                fullVm.Equipments = model.Equipments;

                return View(
                    "Aff_CA_Medical_LibraryDetails",
                    fullVm);
            }

            var collegeCode =
                HttpContext.Session.GetString(
                    "CollegeCode");

            var facultyCode =
                HttpContext.Session.GetString(
                    "FacultyCode");

            // Changes by Ram on 23/04/2026
            // IMPORTANT:
            // Equipment table key does NOT support
            // saving UG/PG/SS separately.
            // Save only once using current course level.

            var courseLevel =
                HttpContext.Session.GetString(
                    "CourseLevel");

            foreach (var item in model.Equipments)
            {
                var entity =
                    await _context.CaMedLibraryEquipments
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.SlNo == item.SlNo);

                if (entity == null)
                {
                    entity =
                        new CaMedLibraryEquipment
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = courseLevel, // save once
                            SlNo = item.SlNo,
                            EquipmentName = item.EquipmentName
                        };

                    _context.Add(entity);
                }

                entity.HasEquipment =
                    item.HasEquipment;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Equipments saved successfully.";

            return RedirectToAction(
               nameof(
               Aff_CA_Medical_LibraryDetails));
        }

        // POST 5: Finance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFinance(CA_Medi_Lib_FinanceVM model)
        {
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();
                fullVm.Finance = model.Finance;
                return View("Aff_CA_Medical_LibraryDetails", fullVm);
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Changes by Ram on 23/04/2026
            var raw =
            HttpContext.Session.GetString(
            "ExistingCourseLevels");

            string[] levels;

            if (string.IsNullOrWhiteSpace(raw))
            {
                levels = new[] { "UG" };
            }
            else
            {
                try
                {
                    // Handles JSON session:
                    // ["UG","PG","SS"]

                    levels =
                     System.Text.Json.JsonSerializer
                     .Deserialize<List<string>>(raw)
                     ?.Select(x => x.Trim().ToUpper())
                     .Distinct()
                     .ToArray()

                     ?? new[] { "UG" };
                }
                catch
                {
                    // Handles old comma session:
                    // UG,PG,SS

                    levels =
                     raw.Split(',')
                     .Select(x => x.Trim()
                                 .Replace("[", "")
                                 .Replace("]", "")
                                 .Replace("\"", "")
                                 .ToUpper())
                     .Where(x => !string.IsNullOrEmpty(x))
                     .Distinct()
                     .ToArray();
                }
            }

            foreach (var level in levels)
            {
                var finance =
                    await _context.CaMedLibraryFinances
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.CourseLevel == level);

                if (finance == null)
                {
                    finance =
                        new CaMedLibraryFinance
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            CourseLevel = level
                        };

                    _context.Add(finance);
                }

                finance.TotalBudgetLakhs =
                    model.Finance.TotalBudgetLakhs;

                finance.ExpenditureBooksLakhs =
                    model.Finance.ExpenditureBooksLakhs;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Finance saved.";
            return RedirectToAction(nameof(Aff_CA_Medical_LibraryDetails));
        }
        // Helper method to reload full view model (used on validation errors)
        private async Task<CA_Med_LibraryMainVM> LoadLibraryViewModel()
        {

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                return new CA_Med_LibraryMainVM();
            }

            var vm = new CA_Med_LibraryMainVM();

            // General (a-d)
            var general = await _context.CaMedLibraryGenerals
                  .Where(x =>
                      x.CollegeCode == collegeCode &&
                      x.FacultyCode == facultyCode)
                  .OrderBy(x => x.CourseLevel)
                  .FirstOrDefaultAsync();

            vm.General = general == null
                ? new CA_Medi_LibraryGeneralVM()
                : new CA_Medi_LibraryGeneralVM
                {
                    LibraryEmailID = general.LibraryEmailId ?? string.Empty,
                    DigitalLibrary = general.DigitalLibrary ?? string.Empty,
                    HelinetServices = general.HelinetServices ?? string.Empty,
                    DepartmentWiseLibrary = general.DepartmentWiseLibrary ?? string.Empty
                };

            // Items Table
            var itemsMaster = await _context.CaMstMedLibraryItems
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            var savedItems = await _context.CaMedLibraryItems
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            vm.Items = itemsMaster.Select(m => new CA_Medi_LibraryItemsVM
            {
                SlNo = m.SlNo,
                ItemName = m.ItemName,
                CurrentForeign = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.CurrentForeign ?? 0,
                CurrentIndian = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.CurrentIndian ?? 0,
                PreviousForeign = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.PreviousForeign ?? 0,
                PreviousIndian = savedItems.FirstOrDefault(s => s.SlNo == m.SlNo)?.PreviousIndian ?? 0
            }).ToList();

            // Building (e)
            var building = await _context.CaMedLibraryBuildings
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            vm.Building = building == null
                ? new CA_Medi_LibraryBuildingVM()
                : new CA_Medi_LibraryBuildingVM
                {
                    IsIndependent = building.IsIndependent ?? string.Empty,
                    AreaSqMtrs = building.AreaSqMtrs
                };

            // Technical Process (f)
            var techMaster = await _context.CaMstMedLibTechnicalProcesses
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();
            // Changes by Ram on 23/04/2026
            // Common section now saved for UG/PG/SS.
            // Load from any one saved row (same values in all three).

            var savedTech = await _context.CaMedLibTechnicalProcesses
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            vm.TechnicalProcess = techMaster.Select(m => new CA_Medi_TechnicalProcessVM
            {
                SlNo = m.SlNo,
                ProcessName = m.ProcessName,
                Value = savedTech.FirstOrDefault(s => s.SlNo == m.SlNo)?.Value ?? string.Empty
            }).ToList();

            // Equipments (g)
            var equipMaster = await _context.CaMstMedLibraryEquipments
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            // Changes by Ram on 23/04/2026
            // Equipment now loads independent of CourseLevel

            var savedEquip = await _context.CaMedLibraryEquipments
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .GroupBy(x => x.SlNo)
                .Select(g => g.First())
                .ToListAsync();

            vm.Equipments = equipMaster.Select(m => new CA_Medi_LibraryEquipmentsVM
            {
                SlNo = m.SlNo,
                EquipmentName = m.EquipmentName,
                HasEquipment = savedEquip.FirstOrDefault(s => s.SlNo == m.SlNo)?.HasEquipment ?? string.Empty
            }).ToList();

            // Finance (h)
            // Changes by Ram on 23/04/2026
            // Finance common data saved for UG/PG/SS
            // Load any one row

            // Changes by Ram on 23/04/2026
            // Finance common data saved for UG/PG/SS
            // Load any one row

            var finance = await _context.CaMedLibraryFinances
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode)
                .FirstOrDefaultAsync();

            vm.Finance = finance == null
                ? new CA_Medi_LibraryFinanceVM()
                : new CA_Medi_LibraryFinanceVM
                {
                    TotalBudgetLakhs =
                        finance.TotalBudgetLakhs,

                    ExpenditureBooksLakhs =
                        finance.ExpenditureBooksLakhs
                };

            return vm;
        }
        // POST actions for each save button (SaveGeneralAndItems, SaveBuilding, SaveTechnicalProcess, SaveEquipments, SaveFinance)
        // Implemented as in previous message — separate POST for each section to avoid validation bleed

        // ... (same POST methods as in previous clean version)

        //code added by ram on 23/04/2026

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
        SaveBinderyOnly(string BinderyValue)
        {
            var collegeCode =
              HttpContext.Session.GetString(
                 "CollegeCode");

            var facultyCode =
              HttpContext.Session.GetString(
                 "FacultyCode");

            var courseLevel =
              HttpContext.Session.GetString(
                 "CourseLevel");


            // Changes by Ram on 23/04/2026
            // Equipment key does not allow
            // separate PG/SS rows.
            // Save ONE shared bindery row.

            var entity =
            await _context.CaMedLibraryEquipments
            .FirstOrDefaultAsync(x =>
                 x.CollegeCode == collegeCode &&
                 x.FacultyCode == facultyCode &&
                 x.SlNo == 3); // bindery slno


            if (entity == null)
            {
                entity =
                new CaMedLibraryEquipment
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,

                    // stored once
                    //CourseLevel = courseLevel,
                    // Changes by Ram on 23/04/2026
                    // Shared bindery row marker
                    CourseLevel = "PG",

                    SlNo = 3,
                    EquipmentName = "Bindery"
                };

                _context.Add(entity);
            }

            entity.HasEquipment =
                BinderyValue;


            await _context.SaveChangesAsync();

            TempData["Success"] =
              "Bindery saved successfully";

            return RedirectToAction(
              nameof(
               Aff_CA_Medical_LibraryDetails));
        }


    }
}
