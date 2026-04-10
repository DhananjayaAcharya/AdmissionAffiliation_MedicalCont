using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Medi_LibraryGeneralVM
    {
        [Required(ErrorMessage = "Library email ID is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string LibraryEmailID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digital Library selection is required")]
        [RegularExpression("Y|N", ErrorMessage = "Please select Yes or No")]
        public string DigitalLibrary { get; set; } = string.Empty;

        [Required(ErrorMessage = "HELINET services selection is required")]
        [RegularExpression("Y|N", ErrorMessage = "Please select Yes or No")]
        public string HelinetServices { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department wise Library selection is required")]
        [RegularExpression("Y|N", ErrorMessage = "Please select Yes or No")]
        public string DepartmentWiseLibrary { get; set; } = string.Empty;
    }
}
