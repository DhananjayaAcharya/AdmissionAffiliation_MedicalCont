using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;

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

            // General (a-d)
            var general = await _context.CaMedLibraryGenerals
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

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
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
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

            var savedTech = await _context.CaMedLibTechnicalProcesses
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
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

            var savedEquip = await _context.CaMedLibraryEquipments
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
                .ToListAsync();

            vm.Equipments = equipMaster.Select(m => new CA_Medi_LibraryEquipmentsVM
            {
                SlNo = m.SlNo,
                EquipmentName = m.EquipmentName,
                HasEquipment = savedEquip.FirstOrDefault(s => s.SlNo == m.SlNo)?.HasEquipment ?? string.Empty
            }).ToList();

            // Finance (h)
            var finance = await _context.CaMedLibraryFinances
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            vm.Finance = finance == null
                ? new CA_Medi_LibraryFinanceVM()
                : new CA_Medi_LibraryFinanceVM
                {
                    TotalBudgetLakhs = finance.TotalBudgetLakhs,
                    ExpenditureBooksLakhs = finance.ExpenditureBooksLakhs
                };

            return View(vm);
        }

        // POST 1: General + Items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGeneralAndItems(CA_Med_Lib_DetailsAndItemsVM model)
        {
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();
                fullVm.General = model.General;
                fullVm.Items = model.Items;
                return View("Aff_CA_Medical_LibraryDetails", fullVm);
            }
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Save General
            var general = await _context.CaMedLibraryGenerals
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            if (general == null)
            {
                general = new CaMedLibraryGeneral { CollegeCode = collegeCode, FacultyCode = facultyCode, CourseLevel = courseLevel };
                _context.CaMedLibraryGenerals.Add(general);
            }

            general.LibraryEmailId = model.General.LibraryEmailID;
            general.DigitalLibrary = model.General.DigitalLibrary;
            general.HelinetServices = model.General.HelinetServices;
            general.DepartmentWiseLibrary = model.General.DepartmentWiseLibrary;

            // Save Items
            foreach (var itemVm in model.Items)
            {
                var masterItem = await _context.CaMstMedLibraryItems
                    .FirstOrDefaultAsync(m => m.SlNo == itemVm.SlNo && m.FacultyCode == facultyCode);

                var entity = await _context.CaMedLibraryItems
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode
                    && x.SlNo == itemVm.SlNo
                    && x.CourseLevel == courseLevel);

                if (entity == null)
                {
                    entity = new CaMedLibraryItem
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel,
                        SlNo = itemVm.SlNo,
                        ItemName = masterItem?.ItemName ?? "Unknown Item"
                    };
                    _context.CaMedLibraryItems.Add(entity);
                }

                entity.CurrentForeign = itemVm.CurrentForeign;
                entity.CurrentIndian = itemVm.CurrentIndian;
                entity.PreviousForeign = itemVm.PreviousForeign;
                entity.PreviousIndian = itemVm.PreviousIndian;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Library details and Items are saved.";
            return RedirectToAction(nameof(Aff_CA_Medical_LibraryDetails));
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
            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            //
            // Custom validation for Area when IsIndependent = "Y"
            if (model.Building.IsIndependent == "Y")
            {
                if (!model.Building.AreaSqMtrs.HasValue || model.Building.AreaSqMtrs <= 0)
                {
                    ModelState.AddModelError("Building.AreaSqMtrs", "Total floor area is required and must be greater than zero when library is an independent building.");
                }
            }

            var building = await _context.CaMedLibraryBuildings
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            if (building == null)
            {
                building = new CaMedLibraryBuilding { CollegeCode = collegeCode, FacultyCode = facultyCode, CourseLevel = courseLevel };
                _context.CaMedLibraryBuildings.Add(building);
            }

            building.IsIndependent = model.Building.IsIndependent;
            building.AreaSqMtrs = model.Building.IsIndependent == "Y" ? model.Building.AreaSqMtrs : null;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Library Building details saved successfully!";
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

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            foreach (var item in model.TechnicalProcess)
            {
                // Fetch existing entity with composite key
                var entity = await _context.CaMedLibTechnicalProcesses
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode &&
                    x.SlNo == item.SlNo && x.CourseLevel == courseLevel);

                if (entity == null)
                {
                    // Pull ProcessName from master
                    var masterProcess = await _context.CaMstMedLibTechnicalProcesses
                        .FirstOrDefaultAsync(m => m.SlNo == item.SlNo && m.FacultyCode == facultyCode);

                    entity = new CaMedLibTechnicalProcess
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel,
                        SlNo = item.SlNo,
                        ProcessName = masterProcess?.ProcessName ?? "Unknown Process"
                    };
                    _context.CaMedLibTechnicalProcesses.Add(entity);
                }
                // Else: If exists, update only Value (don't duplicate ProcessName)

                entity.Value = item.Value;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Library Technical Process details have been saved successfully!";
            return RedirectToAction(nameof(Aff_CA_Medical_LibraryDetails));
        }

        // POST 4: Equipments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEquipments(CA_Med_Lib_EquipmentsVM model)
        {
            if (!ModelState.IsValid)
            {
                var fullVm = await LoadLibraryViewModel();
                fullVm.Equipments = model.Equipments;
                return View("Aff_CA_Medical_LibraryDetails", fullVm);
            }

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            foreach (var item in model.Equipments)
            {
                var entity = await _context.CaMedLibraryEquipments
                    .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.SlNo == item.SlNo
                    && x.CourseLevel == courseLevel);

                if (entity == null)
                {
                    entity = new CaMedLibraryEquipment
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        CourseLevel = courseLevel,
                        SlNo = item.SlNo,
                        EquipmentName = item.EquipmentName
                    };
                    _context.CaMedLibraryEquipments.Add(entity);
                }

                entity.HasEquipment = item.HasEquipment;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Library Equipments details have been saved successfully!";
            return RedirectToAction(nameof(Aff_CA_Medical_LibraryDetails));
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

            var courseLevel = HttpContext.Session.GetString("CourseLevel");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var finance = await _context.CaMedLibraryFinances
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            if (finance == null)
            {
                finance = new CaMedLibraryFinance { CollegeCode = collegeCode, FacultyCode = facultyCode, CourseLevel = courseLevel };
                _context.CaMedLibraryFinances.Add(finance);
            }

            finance.TotalBudgetLakhs = model.Finance.TotalBudgetLakhs;
            finance.ExpenditureBooksLakhs = model.Finance.ExpenditureBooksLakhs;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Library Finance details saved successfully!";
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
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

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
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
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

            var savedTech = await _context.CaMedLibTechnicalProcesses
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
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

            var savedEquip = await _context.CaMedLibraryEquipments
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel)
                .ToListAsync();

            vm.Equipments = equipMaster.Select(m => new CA_Medi_LibraryEquipmentsVM
            {
                SlNo = m.SlNo,
                EquipmentName = m.EquipmentName,
                HasEquipment = savedEquip.FirstOrDefault(s => s.SlNo == m.SlNo)?.HasEquipment ?? string.Empty
            }).ToList();

            // Finance (h)
            var finance = await _context.CaMedLibraryFinances
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.CourseLevel == courseLevel);

            vm.Finance = finance == null
                ? new CA_Medi_LibraryFinanceVM()
                : new CA_Medi_LibraryFinanceVM
                {
                    TotalBudgetLakhs = finance.TotalBudgetLakhs,
                    ExpenditureBooksLakhs = finance.ExpenditureBooksLakhs
                };

            return vm;
        }
        // POST actions for each save button (SaveGeneralAndItems, SaveBuilding, SaveTechnicalProcess, SaveEquipments, SaveFinance)
        // Implemented as in previous message — separate POST for each section to avoid validation bleed

        // ... (same POST methods as in previous clean version)


    }
}
