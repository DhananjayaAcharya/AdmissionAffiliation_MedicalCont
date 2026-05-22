using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class EquipmentPageVM
    {
        public string CollegeCode { get; set; } = null!;
        public int FacultyCode { get; set; }
        public string? SelectedDepartmentCode { get; set; }
        public List<DepartmentVM> Departments { get; set; } = new();
        public List<EquipmentRowVM> Equipments { get; set; } = new();
    }

    public class DepartmentVM
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class EquipmentRowVM
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = null!;
        public string? Specification { get; set; }
        public int? OneUnitReq { get; set; }
        public int? TwoUnitReq { get; set; }
        public int? OneUnitExisting { get; set; }
        public int? TwoUnitExisting { get; set; }
    }
}