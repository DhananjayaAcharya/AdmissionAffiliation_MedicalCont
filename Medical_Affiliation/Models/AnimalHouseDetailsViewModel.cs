using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class AnimalHouseDetailsViewModel
    {
        public int AnimalHouseDetailsId { get; set; }

        // ============================================================
        // Session Controlled Fields
        // ============================================================

        public int FacultyId { get; set; }

        public string? CollegeCode { get; set; }

        public int TypeId { get; set; }

        public string? CourseLevel { get; set; }


        // ============================================================
        // Animal House Details
        // ============================================================

        [Required(ErrorMessage = "Please enter Area.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Area must be greater than 0.")]
        public decimal? Area { get; set; }


        [Required(ErrorMessage = "Please enter Staff details.")]
        public string? Staff { get; set; }


        [Required(ErrorMessage = "Please enter Type of Animals.")]
        public string? TypeOfAnimals { get; set; }


        // ============================================================
        // Existing Records
        // ============================================================

        public List<AnimalHouseDetail> AnimalHouseRecords { get; set; } = new();
    }
}