using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAEquipmentPreviewService : CAPreviewServiceBase, ICAEquipmentPreviewService
    {
        public CAEquipmentPreviewService(  ApplicationDbContext context, IUserContext userContext) : base(context, userContext)
        {
        }

        public async Task<EquipmentPreviewViewModel> GetEquipmentPreviewAsync()
        {
            var vm = new EquipmentPreviewViewModel();

            var departments = await _context.MstEquipmentDepartments
                .Where(x =>
                    x.FacultyCode == FacultyCode &&
                    x.IsActive)
                .OrderBy(x => x.DepartmentName)
                .ToListAsync();

            foreach (var department in departments)
            {
                var equipments = await (
                    from master in _context.MstEquipmentDeptWises

                    join saved in _context.DentalCollegeEquipmentDetails
                        .Where(x =>
                            x.CollegeCode == CollegeCode &&
                            x.DepartmentCode == department.DepartmentCode)
                    on master.Id equals saved.EquipmentId into grp

                    from saved in grp.DefaultIfEmpty()

                    where master.DepartmentCode == department.DepartmentCode
                          && master.FacultyCode == FacultyCode
                          && master.IsActive

                    orderby master.EquipmentName

                    select new EquipmentRowVM
                    {
                        EquipmentId = master.Id,
                        EquipmentName = master.EquipmentName,
                        Specification = master.Specification,
                        OneUnitReq = master.OneUnitRequirement,
                        TwoUnitReq = master.TwoUnitRequirement,
                        OneUnitExisting = saved != null
                            ? saved.OneUnitExisting
                            : null,
                        TwoUnitExisting = saved != null
                            ? saved.TwoUnitExisting
                            : null
                    })
                    .ToListAsync();

                vm.Departments.Add(new EquipmentDepartmentPreviewVM
                {
                    DepartmentCode = department.DepartmentCode,
                    DepartmentName = department.DepartmentName,
                    Equipments = equipments
                });
            }

            return vm;
        }
    }
}
