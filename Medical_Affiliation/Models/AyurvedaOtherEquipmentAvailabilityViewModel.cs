using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class AyurvedaOtherEquipmentAvailabilityViewModel
    {
        public int SelectedEquipmentID { get; set; }

        public List<SelectListItem> Sections { get; set; } = new();

        public string? SelectedSectionName { get; set; }

        public int FacultyId { get; set; } = 4;

        public string? CollegeCode { get; set; }

        public string? AcademicYear { get; set; }

        public string CourseCode { get; set; } = "PG";

        // True only for Section ID 8 (OPD and ICU).
        public bool IsCapacitySpecificSection { get; set; }

        public List<AyurvedaOtherEquipmentItemViewModel> Equipments
        {
            get;
            set;
        } = new();
    }

    public class AyurvedaOtherEquipmentItemViewModel
    {
        public int EquipmentDetailID { get; set; }

        public string SubSectionName { get; set; } = string.Empty;

        public string EquipmentName { get; set; } = string.Empty;

        // Expected requirements from the master table.
        // MinimumNumberRequired represents the normal requirement
        // for OPD and the 60-intake requirement for OPD and ICU.
        public string MinimumNumberRequired { get; set; } = string.Empty;

        public string? MinimumNum100 { get; set; }

        public string? MinimumNum150 { get; set; }

        public string? MinimumNum200 { get; set; }

        // Actual availability entered by the college.
        // Normal section: one quantity.
        public int? AvailableQuantity { get; set; }

        // Capacity-specific section: four actual quantities.
        public int? AvailableQuantity60 { get; set; }

        public int? AvailableQuantity100 { get; set; }

        public int? AvailableQuantity150 { get; set; }

        public int? AvailableQuantity200 { get; set; }
    }
}