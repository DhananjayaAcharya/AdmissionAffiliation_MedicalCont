using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_LibraryDetailsViewModel
    {
        public string? RegistrationNo { get; set; }
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }

        [Required(ErrorMessage = "Total No. of Nursing books is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Books count must be a positive number or zero.")]
        public int? TotalNursingBooks { get; set; }

        [Required(ErrorMessage = "No. of Nursing Journals subscribed is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Journals count must be a positive number or zero.")]
        public int? TotalNursingJournals { get; set; }

        [Required(ErrorMessage = "Please select if Internet Facility is available.")]
        public bool? InternetFacility { get; set; }

        [Required(ErrorMessage = "No. of Thesis/Research titles is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Thesis count must be a positive number or zero.")]
        public int? TotalThesis { get; set; }

        [Required(ErrorMessage = "No. of E-books available is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "E-books count must be a positive number or zero.")]
        public int? TotalEBooks { get; set; }

        [Required(ErrorMessage = "No. of Books purchased last year is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Books purchased must be a positive number or zero.")]
        public int? BooksPurchasedLastYear { get; set; }

        [Required(ErrorMessage = "Total Budget proposed is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than zero.")]
        public decimal? TotalBudget { get; set; }

        [Required(ErrorMessage = "Please select if Library is in independent building.")]
        public bool? IndependentBuilding { get; set; }

        [Required(ErrorMessage = "Total floor area in Sq. ft is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Floor area must be greater than zero.")]
        public int? TotalFloorAreaSqFt { get; set; }
    }
}
