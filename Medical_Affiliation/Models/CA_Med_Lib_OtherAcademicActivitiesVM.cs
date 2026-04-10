using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace Medical_Affiliation.Models
{
    public class CA_Med_Lib_OtherAcademicActivitiesVM
    {
        public int Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        public string ActivityName { get; set; }

        [Required(ErrorMessage = "Department selection is required")]
        public string DepartmentCode { get; set; }

        [Required(ErrorMessage = "Document upload is required")]
        public IFormFile? ActivityPdf { get; set; }

        public string? ActivityPdfName { get; set; }

        public bool IsEdit { get; set; }

        public string? DepartmentWise { get; set; }
    }

}
