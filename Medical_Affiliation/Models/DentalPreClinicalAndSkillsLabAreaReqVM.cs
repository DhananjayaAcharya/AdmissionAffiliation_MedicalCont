using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.ViewModels
{
    public class DentalPreClinicalAndSkillsLabAreaReqVM
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; } = null!;

        public int FacultyCode { get; set; }

        public int SeatIntake { get; set; }

        public int LabId { get; set; }

        public string LabName { get; set; } = null!;

        [Display(Name = "Required Area (Sq.ft)")]
        public decimal RequiredAreaSqFt { get; set; }

        [Display(Name = "Existing Area (Sq.ft)")]
        public decimal? ExistingAreaSqFt { get; set; }

        public string? SectionCode { get; set; }

        public string? LaboratorySection { get; set; }
    }
}