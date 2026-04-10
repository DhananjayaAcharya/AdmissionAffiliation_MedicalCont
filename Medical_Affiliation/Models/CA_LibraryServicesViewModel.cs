using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_LibraryServicesViewModel
    {
        public int Id { get; set; }

        public string? ServiceName { get; set; }

        // [Required(ErrorMessage = "Please select Yes or No")]
        public string? Specify { get; set; }   // Y / N

        // List
        public List<CA_LibraryServicesViewModel> ExistingList { get; set; }
            = new();
    }
}