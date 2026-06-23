namespace Medical_Affiliation.Models
{
    public class AcademicIntakeViewModel
    {
        public List<CourseIntakeViewModel> Courses { get; set; } = new();
    }

    public class CourseIntakeViewModel
    {
        public int CourseCode { get; set; }

        public string CourseName { get; set; }

        // Current approved intake shown in card header
        public int CurrentApprovedIntake { get; set; }

        // Previous approved details (from old tables)
        public int? PreviousExistingIntake { get; set; }

        public int? PreviousAdditionalIntake { get; set; }

        public int? PreviousTotalIntake { get; set; }

        public DateTime? PreviousLopDate { get; set; }

        // Future year-wise entries
        public List<AcademicYearIntakeVm> YearWiseIntakes { get; set; }
            = new();
    }

    public class AcademicYearIntakeVm
    {
        public int? Id { get; set; }

        public string AcademicYear { get; set; } // AY 2026-27

        public int ExistingIntake { get; set; }

        public int AdditionalIntake { get; set; }

        public int TotalIntake { get; set; }

        public string? ApprovalAuthority { get; set; }

        public DateTime? LopDate { get; set; }

        public IFormFile? Document { get; set; }

        // for displaying already uploaded file
        public string? DocumentPath { get; set; }
    }
}
