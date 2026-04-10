using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Medi_LibraryFinanceVM
    {
        [Required(ErrorMessage = "Total Budget proposed is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than zero")]
        public decimal? TotalBudgetLakhs { get; set; }

        [Required(ErrorMessage = "Expenditure proposed for books is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Expenditure must be zero or positive")]
        public decimal? ExpenditureBooksLakhs { get; set; }
    }
}
