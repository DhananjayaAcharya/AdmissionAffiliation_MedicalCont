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
            // GET DEPARTMENT CODES
            // =========================================================

            var departmentCodes = departments
                .Select(d => d.DepartmentCode)
                .ToList();

            // =========================================================
            // GET MASTER EQUIPMENT
            // =========================================================

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
                        OneUnitExisting = null,
                        TwoUnitExisting = null
                    }
                })
                .ToListAsync();

            // =========================================================
            // GET SAVED COLLEGE EQUIPMENT DETAILS
            // =========================================================

            var savedEquipment = await _context.DentalCollegeEquipmentDetails
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.IsActive &&
                    departmentCodes.Contains(x.DepartmentCode))
                .ToListAsync();

            // =========================================================
            // CREATE SAVED EQUIPMENT LOOKUP
            // =========================================================

            var savedEquipmentLookup = savedEquipment
                .GroupBy(x => new
                {
                    x.DepartmentCode,
                    x.EquipmentId
                })
                .ToDictionary(
                    g => (g.Key.DepartmentCode, g.Key.EquipmentId),
                    g => g.First()
                );

            // =========================================================
            // MAP EQUIPMENT AND SAVED DATA TO DEPARTMENTS
            // =========================================================

            foreach (var department in departments)
            {
                department.Equipments = equipment
                    .Where(e =>
                        e.DepartmentCode == department.DepartmentCode)
                    .Select(e =>
                    {
                        savedEquipmentLookup.TryGetValue(
                            (
                                e.DepartmentCode,
                                e.Row.EquipmentId
                            ),
                            out var saved);

                        return new EquipmentRowVM
                        {
                            EquipmentId = e.Row.EquipmentId,

                            EquipmentName = e.Row.EquipmentName,

                            Specification = e.Row.Specification,

                            // Use saved requirements when available;
                            // otherwise, use master requirements.
                            OneUnitReq = saved?.OneUnitRequirement
                                ?? e.Row.OneUnitReq,

                            TwoUnitReq = saved?.TwoUnitRequirement
                                ?? e.Row.TwoUnitReq,

                            // Display saved existing quantities.
                            OneUnitExisting = saved?.OneUnitExisting,

                            TwoUnitExisting = saved?.TwoUnitExisting
                        };
                    })
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