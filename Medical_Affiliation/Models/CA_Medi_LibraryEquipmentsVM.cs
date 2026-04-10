using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Medi_LibraryEquipmentsVM
    {
        public int SlNo { get; set; }
        public string EquipmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select availability for this equipment")]
        [RegularExpression("Y|N", ErrorMessage = "Please select Yes or No")]
        public string HasEquipment { get; set; } = string.Empty;
    }
}