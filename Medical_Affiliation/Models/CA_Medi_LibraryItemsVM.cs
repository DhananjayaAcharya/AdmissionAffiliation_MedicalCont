using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Medi_LibraryItemsVM
    {
        public int SlNo { get; set; }
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current year Foreign count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be zero or positive")]
        public int CurrentForeign { get; set; }

        [Required(ErrorMessage = "Current year Indian count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be zero or positive")]
        public int CurrentIndian { get; set; }

        [Required(ErrorMessage = "Previous year Foreign count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be zero or positive")]
        public int PreviousForeign { get; set; }

        [Required(ErrorMessage = "Previous year Indian count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be zero or positive")]
        public int PreviousIndian { get; set; }
    }
}