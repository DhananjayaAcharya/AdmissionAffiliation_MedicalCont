using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAEquipmentPreviewService : ICAEquipmentPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAEquipmentPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<EquipmentPreviewViewModel?> GetEquipmentPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            if (string.IsNullOrWhiteSpace(collegeCode))
                return null;

            // =========================================================
            // GET DEPARTMENTS
            // =========================================================

            var departments = await _context.MstEquipmentDepartments
                .AsNoTracking()
                .Where(d =>
                    d.FacultyCode == facultyCode &&
                    d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .Select(d => new EquipmentDepartmentPreviewVM
                {
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.DepartmentName
                })
                .ToListAsync();

            if (!departments.Any())
            {
                return new EquipmentPreviewViewModel();
            }

            // =========================================================
            // GET EQUIPMENT
            // =========================================================

            var departmentCodes = departments
                .Select(d => d.DepartmentCode)
                .ToList();

            var equipment = await _context.MstEquipmentDeptWises
                .AsNoTracking()
                .Where(e =>
                    e.FacultyCode == facultyCode &&
                    e.IsActive &&
                    departmentCodes.Contains(e.DepartmentCode))
                .Select(e => new
                {
                    e.DepartmentCode,

                    Row = new EquipmentRowVM
                    {
                        EquipmentId = e.Id,

                        EquipmentName = e.EquipmentName,

                        Specification = e.Specification,

                        OneUnitReq = e.OneUnitRequirement,

                        TwoUnitReq = e.TwoUnitRequirement,

                        // Existing quantities will come
                        // from availability/details table
                        OneUnitExisting = null,

                        TwoUnitExisting = null
                    }
                })
                .ToListAsync();

            // =========================================================
            // MAP EQUIPMENT TO DEPARTMENTS
            // =========================================================

            foreach (var department in departments)
            {
                department.Equipments = equipment
                    .Where(e =>
                        e.DepartmentCode == department.DepartmentCode)
                    .Select(e => e.Row)
                    .ToList();
            }

            // =========================================================
            // RETURN PREVIEW MODEL
            // =========================================================

            return new EquipmentPreviewViewModel
            {
                Departments = departments
            };
        }
    }
}