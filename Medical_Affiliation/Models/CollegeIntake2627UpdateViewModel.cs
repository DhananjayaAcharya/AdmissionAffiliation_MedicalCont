using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class CollegeIntake2627UpdateViewModel
    {
        public string? CollegeCode { get; set; }
        public List<SelectListItem>? CollegeList { get; set; } = new();
        public List<MstMedicalCollegeCourseIntake>? IntakeRows { get; set; } = new();
    }

    public class CollegeIntake2627RowUpdateViewModel
    {
        public int Slno { get; set; }
        public string? CollegeCode { get; set; }
        public int? Intake2627 { get; set; }
    }
}
