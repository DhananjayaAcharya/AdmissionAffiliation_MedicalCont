using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class AddDepartmentToDepartmentMaster
    {
        public int? SelectedFacultyCode { get; set; }
        public List<SelectListItem> Faculties { get; set; } = new List<SelectListItem>();
        public List<DepartmentMaster> DepartmentMaster { get; set; } = new List<DepartmentMaster>();

        public DepartmentMaster NewDepartment { get; set; } = new DepartmentMaster();
    }

}
