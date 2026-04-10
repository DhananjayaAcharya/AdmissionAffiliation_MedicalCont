using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class EquipmentAvailabilityViewModel
    {
        // Dropdown
        public string? SelectedDepartmentCode { get; set; }


        public List<SelectListItem> Courses { get; set; }

        // Equipment list
        public List<EquipmentItemViewModel> Equipments { get; set; }
    }

    public class EquipmentItemViewModel
    {
        public int EquipmentID { get; set; }
        public string EquipmentName { get; set; }
        // NEW
        public bool IsAvailable { get; set; }
        public int? AvailableQuantity { get; set; }


    }

}
