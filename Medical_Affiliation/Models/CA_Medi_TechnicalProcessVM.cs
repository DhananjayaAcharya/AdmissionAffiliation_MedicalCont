using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Medi_TechnicalProcessVM
    {
        public int SlNo { get; set; }
        public string ProcessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Value is required for this technical process")]
        public string Value { get; set; } = string.Empty;
    }
}