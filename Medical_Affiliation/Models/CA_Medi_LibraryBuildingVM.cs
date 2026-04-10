using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Medi_LibraryBuildingVM
    {
        //[Required(ErrorMessage = "Please select whether the library is in an independent building")]
        //[RegularExpression("Y|N", ErrorMessage = "Please select Yes or No")]
        //public string IsIndependent { get; set; } = string.Empty;

        //[Range(0.01, double.MaxValue, ErrorMessage = "Area must be greater than zero")]
        //public decimal? AreaSqMtrs { get; set; }


        [Required(ErrorMessage = "Please select whether the library is in an independent building")]
        [RegularExpression("Y|N", ErrorMessage = "Please select Yes or No")]
        public string IsIndependent { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Area must be greater than zero")]
        public decimal? AreaSqMtrs { get; set; }
    }
}