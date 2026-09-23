namespace Medical_Affiliation.ViewModels
{
    public class CAProgressViewModel
    {
        public int Id { get; set; }

        public string? CollegeCode { get; set; }

        public string? CourseLevel { get; set; }

        public string? StepKey { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class ToggleCAProgressViewModel
    {
        public int Id { get; set; }

        public bool IsCompleted { get; set; }
    }
}